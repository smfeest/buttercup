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
    await page.getByRole('button', { name: 'Continue' }).click();

    await expect(
      page.getByText(
        `If you have an account, we’ll have sent a link to ${user.email} for you to reset your password.`,
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

test('can return to sign in when resetting password', async ({ page }) => {
  await page.goto('/reset-password');

  await expect(
    page.getByRole('heading', { name: 'Forgot your password?' }),
  ).toBeVisible();
  await page.getByRole('link', { name: 'Cancel' }).click();

  await expect(page.getByRole('heading', { name: 'Sign in' })).toBeVisible();
});

test('cannot reset password as deactivated user', async ({
  api,
  baseURL,
  page,
}) => {
  const { createTestUser, deactivateUser, hardDeleteTestUser } =
    api('e2e-admin');
  const user = await createTestUser();
  await deactivateUser(user.id);

  try {
    await page.goto('/reset-password');
    await page.getByLabel('Email').fill(user.email);
    await page.getByRole('button', { name: 'Continue' }).click();

    await expect(page.getByText('sent a link')).toBeVisible();

    const resetPasswordEmailText = await latestEmailText(user.email);
    const resetLinkMatch = resetPasswordEmailText.match(
      new RegExp(`${baseURL}\\/reset-password\\/[^\\s]+`),
    );
    expect(resetLinkMatch).not.toBeNull();

    await page.goto(resetLinkMatch![0]);
    await expect(
      page.getByText(
        'You cannot reset your password because your user account has been deactivated.',
      ),
    ).toBeVisible();
  } finally {
    await hardDeleteTestUser(user.id);
  }
});
