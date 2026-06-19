const { env } = require('process');

const target = env.services__api__https__0 || env.services__api__http__0 || 'https://localhost:7222';

console.log(`[Proxy] Target API URL: ${target}`);

const PROXY_CONFIG = {
  "/api": {
    "target": target,
    "secure": false,
    "changeOrigin": true,
    "logLevel": "debug"
  },
  "/groupHub": {
    "target": target,
    "secure": false,
    "ws": true,
    "changeOrigin": true,
    "logLevel": "debug"
  }
};

module.exports = PROXY_CONFIG;
