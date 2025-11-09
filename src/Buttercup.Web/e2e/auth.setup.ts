import { Page } from '@playwright/test';
import { authStatePath } from './auth-state';
import { SignInForm } from './fixtures/sign-in-form';
import { test as setup } from './test';

const storeAuthenticatedState = async (
  page: Page,
  signInForm: SignInForm,
  username: string,
) => {
  await page.goto('/sign-in');

  await signInForm.emailInput.fill(`${username}@example.com`);
  await signInForm.passwordInput.fill(`${username}-pass`);
  await signInForm.signInButton.click();

  await page.waitForURL('/');
  await page.context().storageState({ path: authStatePath(username) });
};

setup('store authenticated admin state', ({ page, signInForm }) =>
  storeAuthenticatedState(page, signInForm, 'e2e-admin'),
);

setup('store authenticated user state', ({ page, signInForm }) =>
  storeAuthenticatedState(page, signInForm, 'e2e-user'),
);
