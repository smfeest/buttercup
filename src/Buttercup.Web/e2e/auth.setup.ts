import { Page, test as setup } from '@playwright/test';
import { authStatePath } from './auth-state';
import { SignInForm } from './helpers/sign-in-form';

const storeAuthenticatedState = async (page: Page, username: string) => {
  await page.goto('/sign-in');

  const signInForm = new SignInForm(page);
  await signInForm.emailInput.fill(`${username}@example.com`);
  await signInForm.passwordInput.fill(`${username}-pass`);
  await signInForm.signInButton.click();

  await page.waitForURL('/');
  await page.context().storageState({ path: authStatePath(username) });
};

setup('store authenticated admin state', ({ page }) =>
  storeAuthenticatedState(page, 'e2e-admin'),
);

setup('store authenticated user state', ({ page }) =>
  storeAuthenticatedState(page, 'e2e-user'),
);
