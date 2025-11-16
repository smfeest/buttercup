import { expect } from '@playwright/test';
import { authStatePath } from './auth-state';
import { test } from './test';

test.use({ storageState: authStatePath('e2e-user') });

test('can view own profile and preferences', async ({ page, navigation }) => {
  await page.goto('/');

  await navigation.menuButton.click();
  await navigation.yourAccountLink.click();

  await expect(page.getByText('e2e-user@example.com')).toBeVisible();
});
