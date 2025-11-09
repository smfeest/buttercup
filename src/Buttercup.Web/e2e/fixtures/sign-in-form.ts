import type { Page } from '@playwright/test';

export type SignInFormFixture = ReturnType<typeof signInForm>;

export const signInForm = (page: Page) => ({
  emailInput: page.getByLabel('Email'),
  passwordInput: page.getByLabel('Password'),
  signInButton: page.getByRole('button', {
    name: 'Sign in',
  }),
});
