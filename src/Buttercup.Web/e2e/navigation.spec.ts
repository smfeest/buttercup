import { expect, test } from '@playwright/test';
import { authStatePath } from './auth-state';
import { Navigation } from './helpers/navigation';

test.describe('when signed in as an admin user', () => {
  test.use({ storageState: authStatePath('e2e-admin') });

  test('can see admin-only navigation links', async ({ page }) => {
    await page.goto('/');

    const navigation = new Navigation(page);
    await navigation.menuButton.click();
    await expect(navigation.usersLink).toBeVisible();
  });
});

test.describe('when signed in as a non-admin user', () => {
  test.use({ storageState: authStatePath('e2e-user') });

  test('cannot see admin-only navigation links', async ({ page }) => {
    await page.goto('/');

    const navigation = new Navigation(page);
    await navigation.menuButton.click();
    await expect(navigation.usersLink).not.toBeVisible();
  });

  test.describe('when JavaScript is disabled', () => {
    test.use({ javaScriptEnabled: false });

    test('can see fallback navigation links and no menu button', async ({
      page,
    }) => {
      await page.goto('/');

      const navigation = new Navigation(page);
      await expect(navigation.menuButton).not.toBeVisible();
      await expect(navigation.yourAccountLink).toBeVisible();
    });
  });
});
