import { expect, test, type APIRequestContext } from '@playwright/test';
import WebSocket from 'ws';

const baseUrl = 'http://127.0.0.1:5187';

async function login(request: APIRequestContext) {
  const response = await request.post(`${baseUrl}/admin/jwt/login`, {
    data: { userName: 'admin', password: 'e2e-admin-password' },
  });
  await expect(response).toBeOK();
  return (await response.json()).token as string;
}

test('publishing a configuration notifies its connected WebSocket client', async ({ request }) => {
  const suffix = `${Date.now()}-${test.info().parallelIndex}`;
  const appId = `e2e-ws-${suffix}`;
  const secret = 'e2e-ws-secret';
  const token = await login(request);
  const authorization = { Authorization: `Bearer ${token}` };

  const addApp = await request.post(`${baseUrl}/app/add`, {
    headers: authorization,
    data: { id: appId, name: appId, secret, enabled: true, inheritanced: true },
  });
  await expect(addApp).toBeOK();

  // Publish events are forwarded through registered nodes, including this local test node.
  const addLocalNode = await request.post(`${baseUrl}/ServerNode/Add`, {
    headers: authorization,
    data: { address: baseUrl, remark: 'E2E local node' },
  });
  await expect(addLocalNode).toBeOK();

  const socket = new WebSocket(baseUrl.replace('http', 'ws') + '/ws?client_name=e2e', {
    headers: {
      Authorization: `Basic ${Buffer.from(`${appId}:${secret}`).toString('base64')}`,
      appid: appId,
      env: 'DEV',
      'client-v': '1.8.0',
    },
  });

  await new Promise<void>((resolve, reject) => {
    socket.once('open', resolve);
    socket.once('error', reject);
  });

  try {
    const addConfig = await request.post(`${baseUrl}/config/add?env=DEV`, {
      headers: authorization,
      data: { appId, key: 'reload-key', value: 'reload-value', group: '', description: 'E2E' },
    });
    await expect(addConfig).toBeOK();

    const reload = new Promise<string>((resolve, reject) => {
      const timeout = setTimeout(() => reject(new Error('Timed out waiting for reload notification.')), 15_000);
      socket.once('message', (data) => {
        clearTimeout(timeout);
        resolve(data.toString());
      });
    });

    const publish = await request.post(`${baseUrl}/config/publish?env=DEV`, {
      headers: authorization,
      data: { appId, ids: [], log: 'E2E publish' },
    });
    await expect(publish).toBeOK();
    expect(JSON.parse(await reload)).toEqual(expect.objectContaining({ Module: 'c', Action: 'reload' }));
  } finally {
    socket.close();
  }
});
