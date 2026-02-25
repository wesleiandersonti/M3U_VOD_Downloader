# YouTube PHP Proxy (interno/estudo)

Proxy simples para extrair `hlsManifestUrl` de vídeos do YouTube Live, com proteção básica.

## Recursos
- validação de `id`
- rate limit por IP
- cache local (TTL)
- retorno JSON (`?format=json`) ou redirect padrão

## Uso

### Redirect (padrão)
`/index.php?id=WkhCfPPgqWc`

### JSON
`/index.php?id=WkhCfPPgqWc&format=json`

## Observações
- Este método depende de parsing de HTML (pode quebrar com mudanças no YouTube).
- Use para laboratório interno; para produção 24/7 prefira seu `youtube-live-manager`.
