import type { Page } from '@playwright/test';

export type RecipeFormFixture = ReturnType<typeof recipeForm>;

export const recipeForm = (page: Page) => ({
  titleInput: page.getByLabel('Title'),
  ingredientsInput: page.getByLabel('Ingredients'),
  methodInput: page.getByLabel('Method'),
  saveButton: page.getByRole('button', { name: 'Save' }),
});
