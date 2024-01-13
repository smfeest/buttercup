import type { Page } from '@playwright/test';

export class Navigation {
  public constructor(public readonly page: Page) {}

  public menuButton = this.page.getByRole('button', { name: 'Toggle menu' });
  public signOutLink = this.page.getByRole('link', { name: 'Sign out' });
}
