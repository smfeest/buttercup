import type {
  Locator,
  PlaywrightTestArgs,
  TestFixture,
} from '@playwright/test';

export type SignInForm = {
  emailInput: Locator;
  passwordInput: Locator;
  signInButton: Locator;
};

export const signInForm: TestFixture<SignInForm, PlaywrightTestArgs> = async (
  { page },
  use,
) =>
  use({
    emailInput: page.getByLabel('Email'),
    passwordInput: page.getByLabel('Password'),
    signInButton: page.getByRole('button', {
      name: 'Sign in',
    }),
  });
