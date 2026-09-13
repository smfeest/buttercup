import { test as base } from '@playwright/test';
import { api, Api } from './fixtures/api';
import { navigation, Navigation } from './fixtures/navigation';
import { recipeForm, RecipeForm } from './fixtures/recipe-form';
import { session, Session } from './fixtures/session';
import { signInForm, SignInForm } from './fixtures/sign-in-form';
import { commentForm, CommentForm } from './fixtures/comment-form';

export const test = base.extend<{
  api: Api;
  commentForm: CommentForm;
  navigation: Navigation;
  recipeForm: RecipeForm;
  session: Session;
  signInForm: SignInForm;
}>({
  api,
  commentForm,
  navigation,
  recipeForm,
  session,
  signInForm,
});
