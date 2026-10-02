// Same-origin in dev: no CORS to configure. Under Aspire the API URL is injected via service discovery env vars.
const target = process.env['services__api__http__0'] ?? 'http://localhost:5180';

export default {
  '/api': { target, secure: false, changeOrigin: true },
  '/hubs': { target, secure: false, changeOrigin: true, ws: true },
};
