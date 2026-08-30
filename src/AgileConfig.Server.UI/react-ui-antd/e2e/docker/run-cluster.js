const { execFileSync } = require('child_process');
const { setTimeout: delay } = require('timers/promises');
const path = require('path');
const WebSocket = require('ws');

const composeFile = path.join(__dirname, 'docker-compose.yml');
const compose = (args) => execFileSync('docker', ['compose', '-f', composeFile, ...args], { stdio: 'inherit' });
const adminUrl = 'http://127.0.0.1:15100';
const nodeUrl = 'http://127.0.0.1:15102';

async function waitFor(url, timeoutMs = 120_000) {
  const deadline = Date.now() + timeoutMs;
  while (Date.now() < deadline) {
    try {
      const response = await fetch(url);
      if (response.ok) return;
    } catch {
      // The container has not completed startup yet.
    }
    await delay(1_000);
  }
  throw new Error(`Timed out waiting for ${url}.`);
}

async function json(url, options) {
  const response = await fetch(url, options);
  if (!response.ok) throw new Error(`${options?.method ?? 'GET'} ${url} failed with ${response.status}.`);
  return response.json();
}

async function main() {
  compose(['up', '--build', '--detach']);
  try {
    await waitFor(`${adminUrl}/home/echo`);
    await waitFor(`${nodeUrl}/home/echo`);

    const login = await json(`${adminUrl}/admin/jwt/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ userName: 'admin', password: 'e2e-admin-password' }),
    });
    if (login.status !== 'ok') throw new Error('Admin login failed.');
    const adminHeaders = { Authorization: `Bearer ${login.token}`, 'Content-Type': 'application/json' };
    const suffix = Date.now().toString();
    const appId = `e2e-cluster-${suffix}`;
    const secret = 'e2e-cluster-secret';

    const app = await json(`${adminUrl}/app/add`, {
      method: 'POST', headers: adminHeaders,
      body: JSON.stringify({ id: appId, name: appId, secret, enabled: true, inheritanced: true }),
    });
    if (!app.success) throw new Error(`Application creation failed: ${app.message ?? 'unknown error'}.`);

    const socket = new WebSocket(`${nodeUrl.replace('http', 'ws')}/ws?client_name=e2e-cluster`, {
      headers: {
        Authorization: `Basic ${Buffer.from(`${appId}:${secret}`).toString('base64')}`,
        appid: appId,
        env: 'DEV',
        'client-v': '1.8.0',
      },
    });
    await new Promise((resolve, reject) => { socket.once('open', resolve); socket.once('error', reject); });

    try {
      const config = await json(`${adminUrl}/config/add?env=DEV`, {
        method: 'POST', headers: adminHeaders,
        body: JSON.stringify({ appId, key: 'cluster.reload', value: 'ok', group: '', description: 'E2E cluster' }),
      });
      if (!config.success) throw new Error(`Configuration creation failed: ${config.message ?? 'unknown error'}.`);

      const reload = new Promise((resolve, reject) => {
        const timeout = setTimeout(() => reject(new Error('Timed out waiting for cross-node reload.')), 30_000);
        socket.once('message', (data) => { clearTimeout(timeout); resolve(JSON.parse(data.toString())); });
      });
      const publish = await json(`${adminUrl}/config/publish?env=DEV`, {
        method: 'POST', headers: adminHeaders,
        body: JSON.stringify({ appId, ids: [], log: 'E2E cluster publish' }),
      });
      if (!publish.success) throw new Error(`Configuration publish failed: ${publish.message ?? 'unknown error'}.`);
      const notification = await reload;
      if (notification.Module !== 'c' || notification.Action !== 'reload') {
        throw new Error(`Unexpected WebSocket notification: ${JSON.stringify(notification)}.`);
      }

      const published = await json(`${nodeUrl}/api/v2/applications/${appId}/environments/DEV/published-configurations`, {
        headers: { Authorization: `Basic ${Buffer.from(`${appId}:${secret}`).toString('base64')}` },
      });
      if (!published.some((item) => item.key === 'cluster.reload' && item.value === 'ok')) {
        throw new Error('Published configuration was not available from node2.');
      }
    } finally {
      socket.close();
    }
  } finally {
    compose(['down', '--volumes', '--remove-orphans']);
  }
}

main().catch((error) => { console.error(error); process.exitCode = 1; });
