import type { PlaywrightTestArgs, TestFixture } from '@playwright/test';
import { SignInForm } from './sign-in-form';

export type Session = {
  signIn(email: string, password: string): Promise<void>;
};

export const session: TestFixture<
  Session,
  PlaywrightTestArgs & { signInForm: SignInForm }
> = async ({ page, signInForm }, use) =>
  use({
    async signIn(email, password) {
      await page.goto('/sign-in');

      await signInForm.emailInput.fill(email);
      await signInForm.passwordInput.fill(password);
      await signInForm.signInButton.click();

      await page.waitForURL('/');
    },
  });
