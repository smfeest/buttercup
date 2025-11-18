import { expect } from '@playwright/test';
import { authStatePath } from './auth-state';
import { latestEmailText } from './helpers/email';
import { randomString } from './helpers/random';
import { test } from './test';

test.use({ storageState: authStatePath('e2e-user') });

test('can view own profile and preferences', async ({ page, navigation }) => {
  await page.goto('/');

  await navigation.menuButton.click();
  await navigation.yourAccountLink.click();

  await expect(page.getByText('e2e-user@example.com')).toBeVisible();
});

test('can change own password', async ({ page, api, session: { signIn } }) => {
  const { createTestUser, hardDeleteTestUser } = api('e2e-admin');
  const user = await createTestUser();

  try {
    await signIn(user.email, user.password);

    await page.goto('/account');
    await page.getByRole('link', { name: 'Password' }).click();

    const newPassword = `new-password-${randomString()}`;

    await page.getByLabel('Current password').fill(user.password);
    await page
      .getByLabel('New password (6 characters or more)')
      .fill(newPassword);
    await page.getByLabel('Confirm new password').fill(newPassword);
    await page.getByRole('button', { name: 'Change password' }).click();

    const emailText = await latestEmailText(user.email);
    expect(emailText).toContain('Your Buttercup password has been changed');

    await expect(
      page.getByRole('heading', { name: 'Your account' }),
    ).toBeVisible();

    await signIn(user.email, newPassword);

    await expect(page.getByText(`Signed in as ${user.name}`)).toBeAttached();
  } finally {
    await hardDeleteTestUser(user.id);
  }
});
