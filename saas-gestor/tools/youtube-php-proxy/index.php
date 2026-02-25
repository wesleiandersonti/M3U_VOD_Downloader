<?php
declare(strict_types=1);

/**
 * YouTube Live Proxy (uso interno/estudo)
 * - Valida input
 * - Rate limit simples por IP
 * - Cache local por videoId
 * - Retorna JSON de erro (nada de echo solto)
 */

const CACHE_DIR = __DIR__ . '/cache';
const CACHE_TTL = 120; // segundos
const RATE_LIMIT_WINDOW = 60; // segundos
const RATE_LIMIT_MAX = 30; // req por janela/IP
const USER_AGENT = 'MGV-YT-Proxy/1.0';

header('Access-Control-Allow-Origin: *');

function jsonError(string $message, int $status = 400): never {
    http_response_code($status);
    header('Content-Type: application/json; charset=utf-8');
    echo json_encode(['ok' => false, 'error' => $message], JSON_UNESCAPED_UNICODE);
    exit;
}

function ensureDirs(): void {
    if (!is_dir(CACHE_DIR) && !mkdir(CACHE_DIR, 0775, true) && !is_dir(CACHE_DIR)) {
        jsonError('Falha ao criar diretório de cache.', 500);
    }
}

function clientIp(): string {
    return $_SERVER['REMOTE_ADDR'] ?? 'unknown';
}

function enforceRateLimit(): void {
    $ip = preg_replace('/[^a-zA-Z0-9:._-]/', '_', clientIp());
    $file = CACHE_DIR . '/rl_' . $ip . '.json';
    $now = time();

    $data = ['start' => $now, 'count' => 0];
    if (is_file($file)) {
        $raw = file_get_contents($file);
        $tmp = json_decode((string)$raw, true);
        if (is_array($tmp) && isset($tmp['start'], $tmp['count'])) {
            $data = ['start' => (int)$tmp['start'], 'count' => (int)$tmp['count']];
        }
    }

    if (($now - $data['start']) > RATE_LIMIT_WINDOW) {
        $data = ['start' => $now, 'count' => 0];
    }

    $data['count']++;
    file_put_contents($file, json_encode($data));

    if ($data['count'] > RATE_LIMIT_MAX) {
        jsonError('Rate limit excedido. Tente novamente em instantes.', 429);
    }
}

function getVideoId(): string {
    $id = trim((string)($_GET['id'] ?? ''));
    if ($id === '') {
        jsonError('Parâmetro id é obrigatório. Ex.: ?id=WkhCfPPgqWc');
    }
    if (!preg_match('/^[a-zA-Z0-9_-]{11}$/', $id)) {
        jsonError('ID de vídeo inválido.');
    }
    return $id;
}

function cachePath(string $videoId): string {
    return CACHE_DIR . '/yt_' . $videoId . '.json';
}

function fromCache(string $videoId): ?array {
    $path = cachePath($videoId);
    if (!is_file($path)) return null;

    $raw = file_get_contents($path);
    $data = json_decode((string)$raw, true);
    if (!is_array($data) || !isset($data['ts'], $data['hls'])) return null;

    if ((time() - (int)$data['ts']) > CACHE_TTL) return null;

    return $data;
}

function toCache(string $videoId, string $hls): void {
    $path = cachePath($videoId);
    file_put_contents($path, json_encode(['ts' => time(), 'hls' => $hls], JSON_UNESCAPED_SLASHES));
}

function fetchYouTubePage(string $videoId): string {
    $url = 'https://www.youtube.com/watch?v=' . urlencode($videoId);

    $ctx = stream_context_create([
        'http' => [
            'method' => 'GET',
            'timeout' => 12,
            'header' => "User-Agent: " . USER_AGENT . "\r\nAccept-Language: pt-BR,pt;q=0.9,en;q=0.8\r\n"
        ]
    ]);

    $html = @file_get_contents($url, false, $ctx);
    if ($html === false || $html === '') {
        jsonError('Falha ao consultar YouTube.', 502);
    }

    return $html;
}

function extractHlsUrl(string $html): string {
    if (preg_match('/"hlsManifestUrl":"([^"]+)"/', $html, $m)) {
        $decoded = json_decode('"' . $m[1] . '"', true);
        if (is_string($decoded) && str_starts_with($decoded, 'http')) {
            return $decoded;
        }
    }

    jsonError('hlsManifestUrl não encontrado para este vídeo (talvez não seja live).', 404);
}

ensureDirs();
enforceRateLimit();
$videoId = getVideoId();
$format = strtolower(trim((string)($_GET['format'] ?? 'redirect'))); // redirect|json

$cached = fromCache($videoId);
$hls = $cached['hls'] ?? null;
$cacheHit = $hls !== null;

if ($hls === null) {
    $html = fetchYouTubePage($videoId);
    $hls = extractHlsUrl($html);
    toCache($videoId, $hls);
}

if ($format === 'json') {
    header('Content-Type: application/json; charset=utf-8');
    echo json_encode([
        'ok' => true,
        'videoId' => $videoId,
        'hls' => $hls,
        'cacheHit' => $cacheHit,
        'ttlSeconds' => CACHE_TTL
    ], JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);
    exit;
}

header('Location: ' . $hls, true, 302);
exit;
