import { expect, test } from '@playwright/test';
import { authStatePath } from './auth-state';
import { Navigation } from './helpers/navigation';

test.use({ storageState: authStatePath('e2e-user') });

test('can view own profile and preferences', async ({ page }) => {
  await page.goto('/');

  const navigation = new Navigation(page);
  await navigation.menuButton.click();
  await navigation.yourAccountLink.click();

  await expect(page.getByText('e2e-user@example.com')).toBeVisible();
});
