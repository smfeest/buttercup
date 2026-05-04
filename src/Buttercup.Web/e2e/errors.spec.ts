import { expect } from '@playwright/test';
import { authStatePath } from './auth-state';
import { test } from './test';

test.use({ storageState: authStatePath('e2e-user') });

test('404 error is displayed when navigating to a non-existent page', async ({
  page,
}) => {
  const response = await page.goto('/non-existent-page');

  expect(response?.status()).toBe(404);
  await expect(page.locator('h1')).toHaveText('Page not found');
});
