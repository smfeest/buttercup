import { expect } from '@playwright/test';
import { test } from './test';

const userEmail = 'e2e-user@example.com';

test('can sign in with valid credentials', async ({
  page,
  navigation,
  signInForm,
}) => {
  await page.goto('/sign-in');

  await signInForm.emailInput.fill(userEmail);
  await signInForm.passwordInput.fill('e2e-user-pass');
  await signInForm.signInButton.click();

  await navigation.menuButton.click();

  await expect(page.getByText('Signed in as E2E User')).toBeVisible();
});

test('cannot sign in with incorrect password', async ({ page, signInForm }) => {
  await page.goto('/sign-in');

  await signInForm.emailInput.fill(userEmail);
  await signInForm.passwordInput.fill('incorrect-password');
  await signInForm.signInButton.click();

  await expect(page.getByText('Wrong email address or password')).toBeVisible();
  await expect(signInForm.emailInput).toHaveValue(userEmail);
});

test('cannot sign in as deactivated user', async ({
  api,
  page,
  signInForm,
}) => {
  const { createTestUser, deactivateUser, hardDeleteTestUser } =
    api('e2e-admin');
  const user = await createTestUser();
  await deactivateUser(user.id);

  try {
    await page.goto('/sign-in');

    await signInForm.emailInput.fill(user.email);
    await signInForm.passwordInput.fill(user.password);
    await signInForm.signInButton.click();

    await expect(
      page.getByText('Wrong email address or password'),
    ).toBeVisible();
    await expect(signInForm.emailInput).toHaveValue(user.email);
  } finally {
    await hardDeleteTestUser(user.id);
  }
});

test.describe('when redirected to sign in from protected page', () => {
  test('is returned to requested page after signing in', async ({
    page,
    signInForm,
  }) => {
    await page.goto('/account');

    await signInForm.emailInput.fill(userEmail);
    await signInForm.passwordInput.fill('e2e-user-pass');
    await signInForm.signInButton.click();

    await expect(page).toHaveTitle(/Your account/);
  });
});
