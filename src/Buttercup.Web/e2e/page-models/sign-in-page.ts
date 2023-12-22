import type { Page, Locator } from '@playwright/test';

export class SignInPage {
  public readonly emailInput: Locator;
  public readonly passwordInput: Locator;
  public readonly signInButton: Locator;

  constructor(public readonly page: Page) {
    this.emailInput = page.getByLabel('Email');
    this.passwordInput = page.getByLabel('Password');
    this.signInButton = page.getByRole('button', { name: 'Sign in' });
  }

  async goto() {
    await this.page.goto('/sign-in');
  }

  async signIn(email: string, password: string) {
    await this.emailInput.fill(email);
    await this.passwordInput.fill(password);
    await this.signInButton.click();
  }
}
