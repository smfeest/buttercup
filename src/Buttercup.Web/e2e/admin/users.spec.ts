import { expect } from '@playwright/test';
import { authStatePath } from '../auth-state';
import { test } from '../test';

test.describe('when signed in as an admin user', () => {
  test.use({ storageState: authStatePath('e2e-admin') });

  test('can view all users', async ({ page, navigation }) => {
    await page.goto('/');

    await navigation.menuButton.click();
    await navigation.usersLink.click();

    await expect(page.locator('tr')).toContainText(['E2E Admin', 'E2E User']);
  });
});

test.describe('when signed in as a non-admin user', () => {
  test.use({ storageState: authStatePath('e2e-user') });

  test('is denied access', async ({ page }) => {
    await page.goto('/admin/users');
    await expect(page.getByText('Access denied')).toBeVisible();
  });
});

test.describe('when not signed in', () => {
  test('is redirected to sign in', async ({ page }) => {
    await page.goto('/admin/users');
    await expect(page).toHaveTitle(/Sign in/);
  });
});
