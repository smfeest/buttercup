import type { Page } from '@playwright/test';

export type NavigationFixture = ReturnType<typeof navigation>;

export const navigation = (page: Page) => ({
  allRecipesButton: page.getByRole('link', { name: 'All recipes' }),
  newRecipeButton: page.getByRole('link', { name: 'New recipe' }),
  menuButton: page.getByRole('button', { name: 'Toggle menu' }),
  yourAccountLink: page.getByRole('link', { name: 'Your account' }),
  usersLink: page.getByRole('link', { name: 'Users' }),
  signOutLink: page.getByRole('link', { name: 'Sign out' }),
});
