import { expect, test } from '@playwright/test';
import { authStatePath } from './auth-state';
import { Navigation } from './helpers/navigation';

test.use({ storageState: authStatePath('e2e-user') });

test('can sign out', async ({ page }) => {
  await page.goto('/');

  const navigation = new Navigation(page);
  await navigation.menuButton.click();
  await navigation.signOutLink.click();

  await expect(page).toHaveTitle(/Sign in/);
});
