import { expect, test } from '@playwright/test';

test('an administrator can publish a configuration from the hosted management UI', async ({ page, request }) => {
  const suffix = `${Date.now()}-${test.info().parallelIndex}`;
  const appId = `e2e-ui-${suffix}`;
  const appName = `E2E UI ${suffix}`;
  const key = `e2e.key.${suffix}`;
  const value = `value-${suffix}`;

  await page.goto('/ui#/user/login');
  await page.getByRole('textbox', { name: 'Username', exact: true }).fill('admin');
  await page.getByRole('textbox', { name: 'Password', exact: true }).fill('e2e-admin-password');
  await page.getByRole('button', { name: 'Login', exact: true }).click();
  await expect(page).toHaveURL(/#\/home$/);
  await page.getByRole('button', { name: 'Add Now', exact: true }).click();

  await page.goto('/ui#/app');
  await page.getByRole('button', { name: /Create$/ }).click();
  const applicationDialog = page.getByRole('dialog');
  await applicationDialog.getByLabel('Name').fill(appName);
  await applicationDialog.getByLabel('AppID').fill(appId);
  await applicationDialog.getByLabel('Secret').fill('e2e-app-secret');
  await applicationDialog.getByRole('button', { name: 'Submit', exact: true }).click();
  await expect(page.getByText(appId, { exact: true })).toBeVisible();

  await page.goto(`/ui#/app/config/${appId}/${encodeURIComponent(appName)}`);
  await page.getByRole('button', { name: /Add$/ }).click();
  const configurationDialog = page.getByRole('dialog');
  await configurationDialog.getByLabel('Key').fill(key);
  await configurationDialog.getByLabel('Value').fill(value);
  await configurationDialog.getByRole('button', { name: 'Submit', exact: true }).click();
  await expect(page.getByText(key, { exact: true })).toBeVisible();

  await page.getByRole('button', { name: /Publish All$/ }).click();
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
