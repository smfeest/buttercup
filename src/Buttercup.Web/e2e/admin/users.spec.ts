import { expect } from '@playwright/test';
import { authStatePath } from '../auth-state';
import { randomString } from '../helpers/random';
import { test } from '../test';

test.describe('when signed in as an admin user', () => {
  test.use({ storageState: authStatePath('e2e-admin') });

  test('can view user details', async ({ page, navigation }) => {
    await page.goto('/');

    await navigation.menuButton.click();
    await navigation.usersLink.click();

    await expect(page.locator('tr')).toContainText(['E2E Admin', 'E2E User']);

    await page.getByRole('link', { name: 'E2E User' }).click();
    await expect(page.locator('h1')).toHaveText('E2E User');
  });

  test('can add a new user', async ({ api, page }) => {
    const { hardDeleteTestUser } = api('e2e-admin');

    await page.goto('/admin/users');
    await page.getByRole('link', { name: 'Add user' }).click();

    const name = `Joe Bloggs ${randomString()}`;

    await page.getByLabel('Name').fill(name);
    await page
      .getByLabel('Email')
      .fill(`joe.bloggs.${randomString()}@example.com`);
    await page.getByLabel('Time zone').selectOption('GMT+09:00 - Tokyo');
    await page.getByRole('button', { name: 'Add' }).click();

    await expect(page.getByRole('heading', { name: 'Users' })).toBeVisible();
    await expect(page.getByRole('link', { name })).toBeVisible();

    const href = await page.getByRole('link', { name }).getAttribute('href');
    expect(href).not.toBeNull();
    const userId = href!.split('/').pop();
    expect(userId).toBeDefined();
    await hardDeleteTestUser(parseInt(userId!, 10));
  });

  test('can deactivate and reactivate a user', async ({ api, page }) => {
    const { createTestUser, hardDeleteTestUser } = api('e2e-admin');

    const testUser = await createTestUser();

    try {
      await page.goto('/admin/users');
      await page.getByRole('link', { name: testUser.name }).click();

      await page.getByRole('button', { name: 'Deactivate' }).click();

      await expect(page.getByText('Deactivated')).toBeVisible();

      await page.getByRole('button', { name: 'Reactivate' }).click();

      await expect(
        page.getByRole('button', { name: 'Deactivate' }),
      ).toBeVisible();
      await expect(page.getByText('Deactivated')).not.toBeVisible();
    } finally {
      await hardDeleteTestUser(testUser.id);
    }
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
