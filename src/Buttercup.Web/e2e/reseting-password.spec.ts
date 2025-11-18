import { expect } from '@playwright/test';
import { latestEmailText } from './helpers/email';
import { randomString } from './helpers/random';
import { test } from './test';

test('can reset password', async ({ api, baseURL, page }) => {
  const { createTestUser, hardDeleteTestUser } = api('e2e-admin');
  const user = await createTestUser();

  try {
    await page.goto('/sign-in');
    await page.getByRole('link', { name: 'Forgot your password?' }).click();

    await page.getByLabel('Email').fill(user.email);
    await page.getByRole('button', { name: 'Send reset link' }).click();

    await expect(
      page.getByText(
        `If we found an account with the email address ${user.email}, we’ll have sent you a link you can use to set your new password.`,
      ),
    ).toBeVisible();

    const resetPasswordEmailText = await latestEmailText(user.email);
    const resetLinkMatch = resetPasswordEmailText.match(
      new RegExp(`${baseURL}\\/reset-password\\/[^\\s]+`),
    );
    expect(resetLinkMatch).not.toBeNull();

    await page.goto(resetLinkMatch![0]);
    const newPassword = `new-password-${randomString()}`;
    await page
      .getByLabel('New password (6 characters or more)')
      .fill(newPassword);
    await page.getByLabel('Confirm new password').fill(newPassword);
    await page.getByRole('button', { name: 'Set password' }).click();

    expect(await latestEmailText(user.email)).toContain(
      'Your Buttercup password has been changed',
    );

    await expect(page).toHaveTitle('Home - Buttercup');
    await expect(page.getByText(`Signed in as ${user.name}`)).toBeAttached();
  } finally {
    await hardDeleteTestUser(user.id);
  }
});
