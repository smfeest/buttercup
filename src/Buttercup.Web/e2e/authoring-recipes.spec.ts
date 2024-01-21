import { authStatePath } from './auth-state';
import { Navigation } from './helpers/navigation';
import { RecipeForm } from './helpers/recipe-form';
import { expect, test } from './test';

test.use({ storageState: authStatePath('e2e-user') });

test('can add a recipe', async ({ page, api: { deleteRecipe } }) => {
  await page.goto('/');

  const navigation = new Navigation(page);
  await navigation.newRecipeButton.click();

  const ingredients = ['2 eggs', '100g plain flour', '300ml milk'];
  const steps = [
    'Mix ingredients and whisk to a smooth batter',
    'Cook for one minute on each side until golden',
  ];

  const recipeForm = new RecipeForm(page);
  await recipeForm.titleInput.fill('Pancakes');
  await recipeForm.ingredientsInput.fill(ingredients.join('\n'));
  await recipeForm.methodInput.fill(steps.join('\n'));
  await recipeForm.saveButton.click();

  await expect(page).toHaveTitle(/Pancakes/);

  const recipeId = parseInt(page.url().split('/').pop()!, 10);

  try {
    await expect(page.locator('ul > li')).toContainText(ingredients);
    await expect(page.locator('ol > li')).toContainText(steps);
  } finally {
    await deleteRecipe(recipeId);
  }
});
