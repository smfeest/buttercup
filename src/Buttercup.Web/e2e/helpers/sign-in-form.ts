import type { Page } from '@playwright/test';

export class SignInForm {
  public constructor(public readonly page: Page) {}

  public readonly emailInput = this.page.getByLabel('Email');
  public readonly passwordInput = this.page.getByLabel('Password');
  public readonly signInButton = this.page.getByRole('button', {
    name: 'Sign in',
  });

  public async fill(email: string, password: string) {
    await this.emailInput.fill(email);
    await this.passwordInput.fill(password);
  }

  public async submit() {
    await this.signInButton.click();
  }
}
