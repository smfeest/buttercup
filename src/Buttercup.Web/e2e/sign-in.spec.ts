import { expect, test } from '@playwright/test';
import { HomePage } from './page-models/home-page';
import { SignInPage } from './page-models/sign-in-page';

const userEmail = 'e2e-user@example.com';

test('signing in with valid credentials', async ({ page }) => {
  const signInPage = new SignInPage(page);
  await signInPage.goto();
  await signInPage.signIn(userEmail, 'e2e-user-pass');

  const homePage = new HomePage(page);
  await homePage.toggleMenu();
  await expect(page.getByText('Signed in as E2E User')).toBeVisible();
});

test('signing in with an incorrect password', async ({ page }) => {
  const signInPage = new SignInPage(page);
  await signInPage.goto();
  await signInPage.signIn(userEmail, 'incorrect-password');

  await expect(page.getByText('Wrong email address or password')).toBeVisible();
  await expect(signInPage.emailInput).toHaveValue(userEmail);
});
