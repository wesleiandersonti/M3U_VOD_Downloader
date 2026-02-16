# SaaS Gestor – Troubleshooting no Ubuntu (VM)

Guia para quando o **saas-gestor não sobe** ou falha na VM Ubuntu.

---

## 1. Usar o docker-compose do repositório (recomendado)

Na VM, use o **docker-compose.yml da raiz do projeto** (não o gerado pelo `deploy.sh` ou `install-ubuntu.sh`, que usam nomes/env diferentes).

### 1.1 Pré-requisitos na VM

```bash
# Docker
sudo apt-get update && sudo apt-get install -y docker.io docker-compose-plugin
sudo systemctl enable docker && sudo systemctl start docker
sudo usermod -aG docker $USER   # opcional: rodar sem sudo
# Faça logout/login após usermod
```

### 1.2 Arquivo .env obrigatório

Na pasta do projeto (onde está o `docker-compose.yml`):

```bash
cd /caminho/para/saas-gestor
cp .env.example .env
nano .env
```

Preencha **todas** as variáveis; não deixe vazias:

| Variável | Exemplo | Observação |
|----------|---------|------------|
| `DB_ROOT_PASSWORD` | senha forte | MariaDB root |
| `DB_APP_PASSWORD` | senha forte | Usuário `saas_app` |
| `DB_USER` | `saas_app` | Deve ser igual ao master |
| `REDIS_PASSWORD` | senha forte | Não deixe vazio (Redis usa `--requirepass`) |
| `JWT_SECRET` | string ≥ 32 caracteres | Obrigatório |
| `JWT_REFRESH_SECRET` | string ≥ 32 caracteres | Pode ser igual ao JWT_SECRET |

O backend lê **`DB_PASSWORD`** (e não só `DB_APP_PASSWORD`). O `docker-compose` do repositório já repassa `DB_PASSWORD: ${DB_APP_PASSWORD}`; se você usar outro compose, garanta que `DB_PASSWORD` esteja definido.

### 1.3 Pasta init-scripts

O compose monta `./init-scripts` no MariaDB. A pasta existe no repositório (pode estar vazia). Se você copiou só alguns arquivos para a VM, crie se faltar:

```bash
mkdir -p init-scripts
```

### 1.4 Subir os serviços

```bash
cd /caminho/para/saas-gestor
docker compose up -d
# ou, se tiver apenas docker-compose (v1):
docker-compose up -d
```

---

## 2. Verificar o que está falhando

### 2.1 Status dos containers

```bash
docker compose ps
# ou
docker-compose ps
```

Se algum container estiver **Exit** ou **Restarting**, veja os logs (próximo passo).

### 2.2 Logs por serviço

```bash
# Todos
docker compose logs -f

# Só backend (NestJS)
docker compose logs -f backend

# Só MariaDB master
docker compose logs -f mariadb-master

# Só Redis
docker compose logs -f redis
```

Erros comuns nos logs:

- **Backend**: `ER_ACCESS_DENIED_ERROR` ou “connect ECONNREFUSED”  
  → Senha errada ou variável `DB_PASSWORD`/`DB_APP_PASSWORD` vazia/errada no `.env`; ou backend subiu antes do banco (agora há healthcheck no master/slave).

- **MariaDB**: “Access denied for user 'saas_app'”  
  → `DB_APP_PASSWORD` e `DB_USER` (e, no backend, `DB_PASSWORD`) devem bater com o que está no `.env`.

- **Redis**: “NOAUTH Authentication required” ou falha ao iniciar  
  → `REDIS_PASSWORD` não pode estar vazio; defina no `.env`.

### 2.3 Testar conectividade (dentro da rede Docker)

```bash
# Backend consegue ver o master?
docker compose exec backend sh -c 'nc -zv mariadb-master 3306'

# Redis
docker compose exec backend sh -c 'nc -zv redis 6379'
```

---

## 3. Problemas comuns e correções

### 3.1 “Cannot connect to MySQL/MariaDB” no backend

- Confirme que no `.env` você tem:
  - `DB_APP_PASSWORD=...` (e o compose repassa como `DB_PASSWORD` para o backend).
  - `DB_USER=saas_app`.
- Reinicie o backend depois do banco estar saudável:
  - `docker compose up -d mariadb-master mariadb-slave`
  - Aguardar ~30 s e depois `docker compose up -d backend`.

### 3.2 Redis não inicia ou backend falha com Redis

- Defina `REDIS_PASSWORD` no `.env` (não vazio).
- Reinicie: `docker compose restart redis backend`.

### 3.3 Porta 80 ou 3000 já em uso

```bash
sudo ss -tlnp | grep -E ':80|:3000'
# ou
sudo netstat -tlnp | grep -E ':80|:3000'
```

Altere no `docker-compose.yml` se necessário (ex.: `"8080:80"` para o frontend).

### 3.4 Frontend abre mas a API não é chamada / 404 na API

- O frontend é buildado com `VITE_API_URL` em tempo de build. No compose está algo como `VITE_API_URL: http://localhost:3000/api/v1`.
- Se você acessa a VM por IP (ex.: `http://192.168.x.x`), o browser continua chamando `http://localhost:3000`, que é a máquina do usuário.
- Soluções:
  - Acessar a API pelo mesmo host que o frontend (ex.: colocar Nginx na frente e fazer proxy de `/api` para o backend) e usar no build algo como `VITE_API_URL=/api/v1`.
  - Ou rebuildar o frontend com `VITE_API_URL=http://IP_DA_VM:3000/api/v1` e subir de novo.

### 3.5 Scripts com erro “Permission denied” ou “not found”

- Se os scripts (`deploy.sh`, `start.sh`, etc.) foram criados/editados no Windows, podem ter fim de linha CRLF:
  ```bash
  sudo apt-get install -y dos2unix
  dos2unix deploy.sh start.sh logs.sh status.sh
  chmod +x deploy.sh start.sh logs.sh status.sh
  ```

### 3.6 Recomeçar do zero (volumes e containers)

```bash
cd /caminho/para/saas-gestor
docker compose down -v
# Confira o .env e init-scripts
docker compose up -d
docker compose logs -f backend
```

---

## 4. Checklist rápido (Ubuntu VM)

- [ ] Docker e Docker Compose instalados e em execução.
- [ ] Arquivo `.env` criado a partir de `.env.example`, com **todas** as variáveis preenchidas.
- [ ] `DB_USER=saas_app` e `DB_APP_PASSWORD` definidos (e backend recebendo como `DB_PASSWORD`).
- [ ] `REDIS_PASSWORD` não vazio.
- [ ] `JWT_SECRET` e `JWT_REFRESH_SECRET` com pelo menos 32 caracteres.
- [ ] Pasta `init-scripts` existente (pode estar vazia).
- [ ] `docker compose up -d` executado na pasta do `docker-compose.yml`.
- [ ] Após erros: `docker compose logs -f backend` (e mariadb-master/redis se precisar).

---

## 5. URLs após subir

- **Frontend**: `http://IP_DA_VM` (porta 80).
- **Backend API**: `http://IP_DA_VM:3000/api/v1`.
- **Swagger**: `http://IP_DA_VM:3000/api/docs`.

Se mesmo assim o saas-gestor não rodar na VM Ubuntu, envie a saída de:

- `docker compose ps`
- `docker compose logs --tail=100 backend`
- `docker compose logs --tail=50 mariadb-master`
- (opcional) trecho do `.env` **sem as senhas** (só nomes das variáveis).
