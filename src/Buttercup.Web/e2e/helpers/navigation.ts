import type { Page } from '@playwright/test';

export class Navigation {
  public constructor(public readonly page: Page) {}

  public allRecipesButton = this.page.getByRole('link', {
    name: 'All recipes',
  });
  public newRecipeButton = this.page.getByRole('link', { name: 'New recipe' });
  public menuButton = this.page.getByRole('button', { name: 'Toggle menu' });
  public yourAccountLink = this.page.getByRole('link', {
    name: 'Your account',
  });
  public signOutLink = this.page.getByRole('link', { name: 'Sign out' });
}
