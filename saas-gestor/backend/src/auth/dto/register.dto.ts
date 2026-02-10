import { ApiProperty } from '@nestjs/swagger';
import { IsEmail, IsString, Matches, MinLength } from 'class-validator';

export class RegisterDto {
  @ApiProperty({ example: 'Minha Empresa' })
  @IsString()
  @MinLength(2)
  tenantName: string;

  @ApiProperty({ example: 'minha-empresa' })
  @IsString()
  @Matches(/^[a-z0-9-]+$/)
  tenantSlug: string;

  @ApiProperty({ example: 'Admin' })
  @IsString()
  @MinLength(2)
  name: string;

  @ApiProperty({ example: 'admin@empresa.com' })
  @IsEmail()
  email: string;

  @ApiProperty({ example: 'StrongPassword123!' })
  @IsString()
  @MinLength(6)
  password: string;
}
