import { test as base } from '@playwright/test';
import { api, ApiFixture } from './fixtures/api';
import { navigation, NavigationFixture } from './fixtures/navigation';
import { recipeForm, RecipeFormFixture } from './fixtures/recipe-form';

export const test = base.extend<{
  api: ApiFixture;
  navigation: NavigationFixture;
  recipeForm: RecipeFormFixture;
}>({
  api: ({ baseURL }, use) => use(api(baseURL)),
  navigation: ({ page }, use) => use(navigation(page)),
  recipeForm: ({ page }, use) => use(recipeForm(page)),
});
