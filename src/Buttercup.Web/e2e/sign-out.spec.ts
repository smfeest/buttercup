import { expect } from '@playwright/test';
import { authStatePath } from './auth-state';
import { test } from './test';

test.use({ storageState: authStatePath('e2e-user') });

test('can sign out', async ({ page, navigation }) => {
  await page.goto('/');

  await navigation.menuButton.click();
  await navigation.signOutLink.click();

  await expect(page).toHaveTitle(/Sign in/);
});
