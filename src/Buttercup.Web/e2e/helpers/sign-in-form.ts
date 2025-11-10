import type { Page } from '@playwright/test';

export class SignInForm {
  public constructor(public readonly page: Page) {}

  public readonly emailInput = this.page.getByLabel('Email');
  public readonly passwordInput = this.page.getByLabel('Password');
  public readonly signInButton = this.page.getByRole('button', {
    name: 'Sign in',
  });
}
