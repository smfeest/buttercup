import { expect } from '@playwright/test';
import { authStatePath } from '../auth-state';
import { randomString } from '../helpers/random';
import { test } from '../test';
import { gql } from '@urql/core';

test.describe('when signed in as an admin user', () => {
  test.use({ storageState: authStatePath('e2e-admin') });

  test('can view all users', async ({ page, navigation }) => {
    await page.goto('/');

    await navigation.menuButton.click();
    await navigation.usersLink.click();

    await expect(page.locator('tr')).toContainText(['E2E Admin', 'E2E User']);
  });

  test('can add a new user', async ({ api, page }) => {
    const { client, hardDeleteTestUser } = api('e2e-admin');

    await page.goto('/admin/users');
    await page.getByRole('link', { name: 'Add user' }).click();

    const email = `joe.bloggs.${randomString()}@example.com`;

    await page.getByLabel('Name').fill('Joe Bloggs');
    await page.getByLabel('Email').fill(email);
    await page.getByLabel('Time zone').selectOption('GMT+09:00 - Tokyo');
    await page.getByRole('button', { name: 'Add' }).click();

    try {
      await expect(page.getByRole('heading', { name: 'Users' })).toBeVisible();
      await expect(page.getByRole('cell', { name: email })).toBeVisible();
    } finally {
      const QUERY_USER_QUERY = gql`
        query ($email: String!) {
          users(where: { email: { eq: $email } }) {
            id
          }
        }
      `;

      const result = await client.query<{ users: { id: number }[] }>(
        QUERY_USER_QUERY,
        { email },
      );

      const userId = result.data?.users[0]?.id;

      if (userId) {
        await hardDeleteTestUser(userId);
      }
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
