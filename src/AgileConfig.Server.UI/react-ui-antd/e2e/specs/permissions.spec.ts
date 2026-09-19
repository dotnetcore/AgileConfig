import { expect, test, type APIRequestContext } from '@playwright/test';

const baseUrl = 'http://127.0.0.1:5187';
const adminCredentials = { userName: 'admin', password: 'e2e-admin-password' };

async function login(request: APIRequestContext, credentials: { userName: string; password: string }) {
  const response = await request.post(`${baseUrl}/admin/jwt/login`, { data: credentials });
  await expect(response).toBeOK();
  const body = await response.json();
  expect(body.status).toBe('ok');
  return body.token as string;
}

test('a read-only user cannot create applications through the UI or API', async ({ page, request }) => {
  const suffix = `${Date.now()}-${test.info().parallelIndex}`;
  const roleId = `e2e-read-role-${suffix}`;
  const userName = `e2e-read-user-${suffix}`;
  const password = 'e2e-read-password';
  const adminToken = await login(request, adminCredentials);
  const adminAuthorization = { Authorization: `Bearer ${adminToken}` };

  const createRole = await request.post(`${baseUrl}/role/add`, {
    headers: adminAuthorization,
    data: { id: roleId, name: `E2E Read Role ${suffix}`, description: 'Read-only E2E role', functions: ['APP_READ'] },
  });
  await expect(createRole).toBeOK();
  expect((await createRole.json()).success).toBe(true);

  const createUser = await request.post(`${baseUrl}/user/add`, {
    headers: adminAuthorization,
    data: { userName, password, team: 'E2E', userRoleIds: [roleId] },
  });
  await expect(createUser).toBeOK();
  expect((await createUser.json()).success).toBe(true);

  await page.goto('/ui#/user/login');
  await page.getByRole('textbox', { name: 'Username', exact: true }).fill(userName);
  await page.getByRole('textbox', { name: 'Password', exact: true }).fill(password);
  await page.getByRole('button', { name: 'Login', exact: true }).click();
  await expect(page).toHaveURL(/#\/home$/);

  await page.goto('/ui#/app');
  await expect(page.getByRole('button', { name: /Create$/ })).toHaveCount(0);

  const userToken = await page.evaluate(() => localStorage.getItem('token'));
  const rejectedCreate = await request.post(`${baseUrl}/app/add`, {
    headers: { Authorization: `Bearer ${userToken}` },
    data: { id: `forbidden-${suffix}`, name: 'Forbidden', secret: 'secret', enabled: true },
  });
  expect(rejectedCreate.status()).toBe(403);
});
