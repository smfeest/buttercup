import type {
  Locator,
  PlaywrightTestArgs,
  TestFixture,
} from '@playwright/test';

export type RecipeForm = {
  titleInput: Locator;
  ingredientsInput: Locator;
  methodInput: Locator;
  saveButton: Locator;
};

export const recipeForm: TestFixture<RecipeForm, PlaywrightTestArgs> = async (
  { page },
  use,
) =>
  use({
    titleInput: page.getByLabel('Title'),
    ingredientsInput: page.getByLabel('Ingredients'),
    methodInput: page.getByLabel('Method'),
    saveButton: page.getByRole('button', { name: 'Save' }),
  });
