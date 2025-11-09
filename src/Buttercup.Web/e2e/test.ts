import { test as base } from '@playwright/test';
import { api, ApiFixture } from './fixtures/api';
import { navigation, NavigationFixture } from './fixtures/navigation';
import { recipeForm, RecipeFormFixture } from './fixtures/recipe-form';
import { signInForm, SignInFormFixture } from './fixtures/sign-in-form';

export const test = base.extend<{
  api: ApiFixture;
  navigation: NavigationFixture;
  recipeForm: RecipeFormFixture;
  signInForm: SignInFormFixture;
}>({
  api: ({ baseURL }, use) => use(api(baseURL)),
  navigation: ({ page }, use) => use(navigation(page)),
  recipeForm: ({ page }, use) => use(recipeForm(page)),
  signInForm: ({ page }, use) => use(signInForm(page)),
});
