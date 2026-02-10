import { Injectable } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import { Tenant } from '../tenants/entities/tenant.entity';
import { User } from '../users/entities/user.entity';
import { Application } from '../applications/entities/application.entity';
import { Build, BuildStatus } from '../applications/entities/build.entity';
import { Deployment, DeploymentStatus } from '../applications/entities/deployment.entity';

@Injectable()
export class DashboardService {
  constructor(
    @InjectRepository(Tenant) private readonly tenantRepository: Repository<Tenant>,
    @InjectRepository(User) private readonly userRepository: Repository<User>,
    @InjectRepository(Application) private readonly applicationRepository: Repository<Application>,
    @InjectRepository(Build) private readonly buildRepository: Repository<Build>,
    @InjectRepository(Deployment) private readonly deploymentRepository: Repository<Deployment>,
  ) {}

  async getOverview() {
    const [
      tenants,
      users,
      applications,
      builds,
      deployments,
      successfulBuilds,
      successfulDeployments,
    ] = await Promise.all([
      this.tenantRepository.count(),
      this.userRepository.count(),
      this.applicationRepository.count(),
      this.buildRepository.count(),
      this.deploymentRepository.count(),
      this.buildRepository.count({ where: { status: BuildStatus.SUCCESS } }),
      this.deploymentRepository.count({ where: { status: DeploymentStatus.SUCCESS } }),
    ]);

    return {
      tenants,
      users,
      applications,
      builds,
      deployments,
      buildSuccessRate: builds > 0 ? Math.round((successfulBuilds / builds) * 100) : 0,
      deploymentSuccessRate: deployments > 0 ? Math.round((successfulDeployments / deployments) * 100) : 0,
      timestamp: new Date().toISOString(),
    };
  }
}
