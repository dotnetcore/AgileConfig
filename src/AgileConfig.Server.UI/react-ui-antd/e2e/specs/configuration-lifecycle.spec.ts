import { expect, test } from '@playwright/test';

test('an administrator can publish a configuration from the hosted management UI', async ({ page, request }) => {
  const suffix = `${Date.now()}-${test.info().parallelIndex}`;
  const appId = `e2e-ui-${suffix}`;
  const appName = `E2E UI ${suffix}`;
  const key = `e2e.key.${suffix}`;
  const value = `value-${suffix}`;

  const login = await request.post('/admin/jwt/login', {
    data: { userName: 'admin', password: 'e2e-admin-password' },
  });
  await expect(login).toBeOK();
  const { token } = await login.json();
  const addNode = await request.post('/ServerNode/Add', {
    headers: { Authorization: `Bearer ${token}` },
    data: { address: 'http://127.0.0.1:5187', remark: 'E2E local node' },
  });
  await expect(addNode).toBeOK();

  await page.goto('/ui#/user/login');
  await page.getByRole('textbox', { name: 'Username', exact: true }).fill('admin');
  await page.getByRole('textbox', { name: 'Password', exact: true }).fill('e2e-admin-password');
  await page.getByRole('button', { name: 'Login', exact: true }).click();
  await expect(page).toHaveURL(/#\/home$/);

  await page.goto('/ui#/app');
  await page.getByTestId('app-create').click();
  const applicationDialog = page.getByRole('dialog');
  await applicationDialog.locator('#app-create-name').fill(appName);
  await applicationDialog.locator('#app-create-id').fill(appId);
  await applicationDialog.locator('#app-create-secret').fill('e2e-app-secret');
  await applicationDialog.getByTestId('app-create-submit').click();
  await expect(page.getByText(appId, { exact: true })).toBeVisible();

  await page.goto(`/ui#/app/config/${appId}/${encodeURIComponent(appName)}`);
  await page.getByTestId('config-create').click();
  const configurationDialog = page.getByRole('dialog');
  await configurationDialog.locator('#config-create-key').fill(key);
  await configurationDialog.locator('#config-create-value').fill(value);
  await configurationDialog.getByTestId('config-create-submit').click();
  await expect(page.getByText(key, { exact: true })).toBeVisible();

  await page.getByTestId('config-publish-all').click();
  const publishDialog = page.getByRole('dialog');
  await publishDialog.getByRole('button', { name: 'OK', exact: true }).click();
  await expect(page.getByText('Publish Success!', { exact: true })).toBeVisible();

  const published = await request.get(
    `/api/v2/applications/${appId}/environments/DEV/published-configurations`,
    {
      headers: {
        Authorization: `Basic ${Buffer.from(`${appId}:e2e-app-secret`).toString('base64')}`,
      },
    },
  );
  await expect(published).toBeOK();
  await expect(published.json()).resolves.toEqual(
    expect.arrayContaining([expect.objectContaining({ key, value })]),
  );
});
