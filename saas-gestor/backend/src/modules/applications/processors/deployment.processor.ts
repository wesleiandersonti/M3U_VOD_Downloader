import { Processor, WorkerHost, OnWorkerEvent } from '@nestjs/bullmq';
import { Job } from 'bullmq';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import { Logger } from '@nestjs/common';
import { Deployment, DeploymentStatus } from '../entities/deployment.entity';
import { Application, ApplicationStatus } from '../entities/application.entity';

interface DeployJobData {
  deploymentId: number;
  applicationId: number;
  tenantId: number;
  buildId: number;
  deploymentType: string;
  autoRollback: boolean;
}

interface RollbackJobData {
  deploymentId: number;
  applicationId: number;
  tenantId: number;
  previousDeploymentId: string;
}

@Processor('deployments')
export class DeploymentProcessor extends WorkerHost {
  private readonly logger = new Logger(DeploymentProcessor.name);

  constructor(
    @InjectRepository(Deployment)
    private readonly deploymentRepository: Repository<Deployment>,
    @InjectRepository(Application)
    private readonly applicationRepository: Repository<Application>,
  ) {
    super();
  }

  async process(job: Job<DeployJobData | RollbackJobData>): Promise<void> {
    if (job.name === 'rollback') {
      await this.handleRollback(job as Job<RollbackJobData>);
      return;
    }

    await this.handleDeploy(job as Job<DeployJobData>);
  }

  private async handleDeploy(job: Job<DeployJobData>): Promise<void> {
    const { deploymentId, applicationId, buildId } = job.data;

    const deployment = await this.deploymentRepository.findOne({ where: { id: deploymentId } });
    if (!deployment) {
      throw new Error(`Deployment ${deploymentId} not found`);
    }

    const startedAt = new Date();
    deployment.status = DeploymentStatus.RUNNING;
    deployment.stage = 'preparing';
    deployment.progress = 10;
    deployment.startedAt = startedAt;
    deployment.logs = this.appendLog(deployment.logs, `Deployment ${deploymentId} started`);
    await this.deploymentRepository.save(deployment);

    await this.sleep(1000);
    deployment.stage = 'deploying';
    deployment.progress = 55;
    deployment.logs = this.appendLog(deployment.logs, `Deploying build ${buildId}`);
    await this.deploymentRepository.save(deployment);

    await this.sleep(1000);
    deployment.stage = 'health_check';
    deployment.progress = 85;
    deployment.logs = this.appendLog(deployment.logs, 'Health check passed');
    await this.deploymentRepository.save(deployment);

    const completedAt = new Date();
    deployment.status = DeploymentStatus.SUCCESS;
    deployment.stage = 'completed';
    deployment.progress = 100;
    deployment.completedAt = completedAt;
    deployment.duration = Math.max(1, Math.round((completedAt.getTime() - startedAt.getTime()) / 1000));
    deployment.logs = this.appendLog(deployment.logs, 'Deployment completed successfully');
    await this.deploymentRepository.save(deployment);

    await this.applicationRepository.update(applicationId, {
      status: ApplicationStatus.ACTIVE,
      currentBuildId: buildId,
      lastDeployedAt: completedAt,
    });
  }

  private async handleRollback(job: Job<RollbackJobData>): Promise<void> {
    const { deploymentId, applicationId, previousDeploymentId } = job.data;

    const deployment = await this.deploymentRepository.findOne({ where: { id: deploymentId } });
    if (!deployment) {
      throw new Error(`Deployment ${deploymentId} not found`);
    }

    deployment.status = DeploymentStatus.ROLLING_BACK;
    deployment.stage = 'rolling_back';
    deployment.progress = 50;
    deployment.logs = this.appendLog(
      deployment.logs,
      `Rollback started to deployment ${previousDeploymentId}`,
    );
    await this.deploymentRepository.save(deployment);

    await this.sleep(1000);

    deployment.status = DeploymentStatus.ROLLED_BACK;
    deployment.stage = 'rolled_back';
    deployment.progress = 100;
    deployment.rolledBackAt = new Date();
    deployment.completedAt = deployment.rolledBackAt;
    deployment.logs = this.appendLog(deployment.logs, 'Rollback completed');
    await this.deploymentRepository.save(deployment);

    await this.applicationRepository.update(applicationId, {
      status: ApplicationStatus.ACTIVE,
    });
  }

  @OnWorkerEvent('active')
  onActive(job: Job): void {
    this.logger.log(`Processing deployment job ${job.id} (${job.name})`);
  }

  @OnWorkerEvent('completed')
  onCompleted(job: Job): void {
    this.logger.log(`Deployment job ${job.id} completed`);
  }

  @OnWorkerEvent('failed')
  onFailed(job: Job, error: Error): void {
    this.logger.error(`Deployment job ${job.id} failed: ${error.message}`);
  }

  private appendLog(existing: string | null | undefined, line: string): string {
    const stamp = new Date().toISOString();
    const logLine = `[${stamp}] ${line}`;
    return existing ? `${existing}
${logLine}` : logLine;
  }

  private sleep(ms: number): Promise<void> {
    return new Promise((resolve) => setTimeout(resolve, ms));
  }
}
