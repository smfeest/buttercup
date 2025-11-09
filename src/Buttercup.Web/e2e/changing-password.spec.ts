import { expect } from '@playwright/test';
import { randomString } from './helpers/random';
import { test } from './test';

test('can change own password', async ({
  page,
  api,
  changePasswordForm,
  signInForm,
}) => {
  const { createTestUser } = api('e2e-admin');
  const { name, email, password } = await createTestUser();

  await page.goto('/sign-in');

  await signInForm.emailInput.fill(email);
  await signInForm.passwordInput.fill(password);
  await signInForm.signInButton.click();

  await page.goto('/account');

  await page.getByRole('link', { name: 'Password' }).click();

  const newPassword = `new-password-${randomString()}`;

  await changePasswordForm.currentPasswordInput.fill(password);
  await changePasswordForm.newPasswordInput.fill(newPassword);
  await changePasswordForm.confirmNewPasswordInput.fill(newPassword);
  await changePasswordForm.changePasswordButton.click();

  await expect(
    page.getByRole('heading', { name: 'Your account' }),
  ).toBeVisible();

  await page.goto('/sign-in');
  await signInForm.emailInput.fill(email);
  await signInForm.passwordInput.fill(newPassword);
  await signInForm.signInButton.click();

  await expect(page.getByText(`Signed in as ${name}`)).toBeAttached();

  // TODO: Delete old user
});
