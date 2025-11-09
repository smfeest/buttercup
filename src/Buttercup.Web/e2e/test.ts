import { test as base } from '@playwright/test';
import { api, Api } from './fixtures/api';
import { navigation, Navigation } from './fixtures/navigation';
import { recipeForm, RecipeForm } from './fixtures/recipe-form';

export const test = base.extend<{
  api: Api;
  navigation: Navigation;
  recipeForm: RecipeForm;
}>({
  api,
  navigation,
  recipeForm,
});
