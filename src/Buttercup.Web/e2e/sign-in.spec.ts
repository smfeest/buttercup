import { expect } from '@playwright/test';
import { SignInForm } from './helpers/sign-in-form';
import { test } from './test';

const userEmail = 'e2e-user@example.com';

test('can sign in with valid credentials', async ({ page, navigation }) => {
  await page.goto('/sign-in');

  const signInForm = new SignInForm(page);
  await signInForm.fill(userEmail, 'e2e-user-pass');
  await signInForm.submit();

  await navigation.menuButton.click();

  await expect(page.getByText('Signed in as E2E User')).toBeVisible();
});

test('cannot sign in with incorrect password', async ({ page }) => {
  await page.goto('/sign-in');

  const signInForm = new SignInForm(page);
  await signInForm.fill(userEmail, 'incorrect-password');
  await signInForm.submit();

  await expect(page.getByText('Wrong email address or password')).toBeVisible();
  await expect(signInForm.emailInput).toHaveValue(userEmail);
});

test.describe('when redirected to sign in from protected page', () => {
  test('is returned to requested page after signing in', async ({ page }) => {
    await page.goto('/account');

    const signInForm = new SignInForm(page);
    await signInForm.fill(userEmail, 'e2e-user-pass');
    await signInForm.submit();

    await expect(page).toHaveTitle(/Your account/);
  });
});
