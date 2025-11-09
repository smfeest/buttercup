import { test as base } from '@playwright/test';
import { api, Api } from './fixtures/api';
import { navigation, Navigation } from './fixtures/navigation';
import { recipeForm, RecipeForm } from './fixtures/recipe-form';
import { signInForm, SignInForm } from './fixtures/sign-in-form';

export const test = base.extend<{
  api: Api;
  navigation: Navigation;
  recipeForm: RecipeForm;
  signInForm: SignInForm;
}>({
  api,
  navigation,
  recipeForm,
  signInForm,
});
