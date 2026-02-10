# SaaS Gestor - One Command Deploy

Deploy completo com apenas **UM COMANDO**!

## 🚀 Como Usar

### Método 1: Download e Execução (Recomendado)

```bash
# Faça login como root na sua VM
ssh root@192.168.170.124

# Execute o deploy com um comando
curl -sSL https://raw.githubusercontent.com/seu-usuario/saas-gestor/main/one-command-deploy.sh | sudo bash
```

### Método 2: Copiar e Executar

```bash
# 1. Copie o arquivo para a VM
scp one-command-deploy.sh root@192.168.170.124:/root/

# 2. Conecte na VM
ssh root@192.168.170.124

# 3. Execute
chmod +x one-command-deploy.sh
./one-command-deploy.sh
```

### Método 3: Instalação Local (VM já conectada)

```bash
# Dentro da VM, execute:
wget https://raw.githubusercontent.com/seu-usuario/saas-gestor/main/one-command-deploy.sh
chmod +x one-command-deploy.sh
./one-command-deploy.sh
```

## 📋 O que o Script Faz

O script executa automaticamente:

1. ✅ **Atualiza o sistema** (apt update/upgrade)
2. ✅ **Instala Docker** (se não tiver)
3. ✅ **Instala Docker Compose** (se não tiver)
4. ✅ **Cria diretórios** (/opt/saas-gestor)
5. ✅ **Gera senhas seguras** automaticamente
6. ✅ **Cria arquivo .env** configurado
7. ✅ **Cria docker-compose.yml** completo
8. ✅ **Configura firewall** (UFW)
9. ✅ **Cria scripts de gerenciamento**
10. ✅ **Cria serviço systemd**
11. ✅ **Inicia infraestrutura** (MariaDB + Redis)
12. ✅ **Mostra informações finais**

⏱️ **Tempo total:** ~5-10 minutos

## 🎯 Após o Deploy

### URLs de Acesso
- **Frontend:** http://192.168.170.124
- **API:** http://192.168.170.124:3000
- **Documentação:** http://192.168.170.124:3000/api/docs

### Comandos de Gerenciamento

```bash
# Iniciar sistema
/opt/saas-gestor/start.sh

# Parar sistema
/opt/saas-gestor/stop.sh

# Ver logs
/opt/saas-gestor/logs.sh
/opt/saas-gestor/logs.sh backend
/opt/saas-gestor/logs.sh frontend

# Status do sistema
/opt/saas-gestor/status.sh

# Backup
/opt/saas-gestor/backup.sh

# Docker Compose
cd /opt/saas-gestor
docker-compose ps
docker-compose logs -f
docker-compose restart
```

### Credenciais

As senhas são geradas automaticamente e salvas em:
```
/opt/saas-gestor/.env
```

**⚠️ IMPORTANTE:** Salve essas senhas em um local seguro!

## 📁 Estrutura Criada

```
/opt/saas-gestor/
├── .env                      # Variáveis de ambiente
├── docker-compose.yml        # Config Docker
├── backend/                  # Código backend (copiar)
├── frontend/                 # Código frontend (copiar)
├── data/
│   ├── mysql-master/        # Dados MariaDB Master
│   ├── mysql-slave/         # Dados MariaDB Slave
│   ├── redis/               # Dados Redis
│   └── builds/              # Build artifacts
├── backups/                 # Backups automáticos
├── init-scripts/            # Scripts SQL
├── start.sh                 # Iniciar
├── stop.sh                  # Parar
├── logs.sh                  # Logs
├── status.sh                # Status
└── backup.sh                # Backup
```

## 🔧 Configuração Avançada

### Variáveis de Ambiente

Edite `/opt/saas-gestor/.env`:

```bash
# Banco de dados
DB_ROOT_PASSWORD=sua_senha_root
DB_APP_PASSWORD=sua_senha_app

# Redis
REDIS_PASSWORD=sua_senha_redis

# JWT
JWT_SECRET=sua_chave_jwt_32caracteres
JWT_REFRESH_SECRET=sua_chave_refresh_32caracteres

# Encriptação (exatamente 32 caracteres)
DB_ENCRYPTION_KEY=chave_32_caracteres_exata
ENV_ENCRYPTION_KEY=outra_chave_32_caracteres
```

### Clonar do Git

Para clonar automaticamente do Git, defina a variável:

```bash
export REPO_URL="https://github.com/seu-usuario/saas-gestor.git"
./one-command-deploy.sh
```

## 🛠️ Solução de Problemas

### Verificar status
```bash
/opt/saas-gestor/status.sh
```

### Ver logs
```bash
# Todos os serviços
/opt/saas-gestor/logs.sh

# Serviço específico
/opt/saas-gestor/logs.sh backend
/opt/saas-gestor/logs.sh mariadb-master
```

### Reiniciar tudo
```bash
cd /opt/saas-gestor
docker-compose restart
```

### Reset completo
```bash
cd /opt/saas-gestor
docker-compose down -v  # Remove volumes também
docker-compose up -d
```

## 🌐 Configurar Domínio

### Com Certbot (SSL)

```bash
# Instalar certbot
apt-get install -y certbot python3-certbot-nginx

# Gerar certificado
certbot --nginx -d seu-dominio.com -d www.seu-dominio.com

# Renovação automática (já configurada)
systemctl status certbot.timer
```

## 📊 Recursos do Sistema

**Requisitos Mínimos:**
- Ubuntu 20.04+ (recomendado 24.04 LTS)
- 4GB RAM
- 2 CPUs
- 50GB disco

**Recomendado:**
- 8GB RAM
- 4 CPUs
- 100GB SSD

## 🔒 Segurança

O script já configura:
- ✅ Firewall UFW (portas 22, 80, 443, 3000, 3306, 3307, 6379)
- ✅ Senhas aleatórias seguras
- ✅ Permissões restritas em arquivos
- ✅ Docker isolado
- ✅ MariaDB apenas localhost (com port forwarding)

## 🆘 Suporte

Problemas comuns:

### "Permission denied"
```bash
# Execute como root
sudo ./one-command-deploy.sh
```

### "Port already in use"
```bash
# Verifique o que está usando a porta
netstat -tlnp | grep -E ':(80|3000|3306)'

# Pare o serviço ou mude a porta no docker-compose.yml
```

### "Docker not found"
```bash
# Instale manualmente
curl -fsSL https://get.docker.com | sh
```

## 📝 Notas

- O script detecta automaticamente se componentes já estão instalados
- Todas as senhas são geradas aleatoriamente (32-48 caracteres)
- O firewall é configurado automaticamente
- O serviço inicia automaticamente no boot
- Backups podem ser feitos com `./backup.sh`

## ✅ Checklist Pós-Deploy

- [ ] Anotar senhas do arquivo `.env`
- [ ] Copiar código backend/frontend
- [ ] Testar acesso: http://192.168.170.124
- [ ] Verificar logs: `./logs.sh`
- [ ] Configurar domínio (opcional)
- [ ] Configurar SSL (opcional)
- [ ] Agendar backups automáticos

---

**Deploy rápido, seguro e automatizado!** 🚀
