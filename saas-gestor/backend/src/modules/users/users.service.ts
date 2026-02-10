import { Injectable, NotFoundException } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import { User, UserStatus } from './entities/user.entity';

@Injectable()
export class UsersService {
  constructor(
    @InjectRepository(User)
    private readonly userRepository: Repository<User>,
  ) {}

  async findAll(tenantId?: number): Promise<User[]> {
    const where = tenantId ? { tenantId } : undefined;
    return this.userRepository.find({ where, relations: ['tenant'], order: { id: 'DESC' } });
  }

  async findOne(id: number): Promise<User> {
    const user = await this.userRepository.findOne({ where: { id }, relations: ['tenant'] });
    if (!user) {
      throw new NotFoundException('User not found');
    }
    return user;
  }

  async activate(id: number): Promise<User> {
    const user = await this.findOne(id);
    user.status = UserStatus.ACTIVE;
    return this.userRepository.save(user);
  }
}
