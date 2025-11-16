import type {
  Locator,
  PlaywrightTestArgs,
  TestFixture,
} from '@playwright/test';

export type Navigation = {
  allRecipesButton: Locator;
  newRecipeButton: Locator;
  menuButton: Locator;
  yourAccountLink: Locator;
  usersLink: Locator;
  signOutLink: Locator;
};

export const navigation: TestFixture<Navigation, PlaywrightTestArgs> = async (
  { page },
  use,
) =>
  use({
    allRecipesButton: page.getByRole('link', { name: 'All recipes' }),
    newRecipeButton: page.getByRole('link', { name: 'New recipe' }),
    menuButton: page.getByRole('button', { name: 'Toggle menu' }),
    yourAccountLink: page.getByRole('link', { name: 'Your account' }),
    usersLink: page.getByRole('link', { name: 'Users' }),
    signOutLink: page.getByRole('link', { name: 'Sign out' }),
  });
