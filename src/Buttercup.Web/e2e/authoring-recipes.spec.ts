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

test('can edit a recipe', async ({
  page,
  api: { createRecipe, deleteRecipe },
}) => {
  const { id } = await createRecipe({ title: 'Glorious cheese sandwich' });

  try {
    await page.goto(`recipes/${id}`);
    await page.getByRole('link', { name: 'Edit' }).click();

    const recipeForm = new RecipeForm(page);

    await expect(recipeForm.titleInput).toHaveValue('Glorious cheese sandwich');

    await recipeForm.titleInput.fill('Spectacular cheese sandwich');
    await recipeForm.saveButton.click();

    await expect(page.locator('h1')).toHaveText('Spectacular cheese sandwich');
  } finally {
    await deleteRecipe(id);
  }
});

test('can delete a recipe', async ({
  page,
  api: { createRecipe, deleteRecipe },
}) => {
  const { id } = await createRecipe();

  try {
    await page.goto(`recipes/${id}`);
    await page.getByRole('link', { name: 'Delete' }).click();
    await page.getByRole('button', { name: 'Delete' }).click();

    await expect(page).toHaveTitle(/All recipes/);
  } finally {
    await deleteRecipe(id);
  }
});
