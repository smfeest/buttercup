import { expect } from '@playwright/test';
import { authStatePath } from './auth-state';
import { test } from './test';

test.use({ storageState: authStatePath('e2e-user') });

test('can comment on a recipe', async ({ page, api }) => {
  const { createRecipe, hardDeleteRecipe } = api('e2e-admin');

  const recipe = await createRecipe();

  try {
    await page.goto(`recipes/${recipe.id}`);

    await page
      .getByPlaceholder('Write a comment…')
      .fill('You can also use precooked beans');
    await page.getByRole('button', { name: 'Add' }).click();

    await expect(
      page.getByText('You can also use precooked beans'),
    ).toBeInViewport();
  } finally {
    await hardDeleteRecipe(recipe.id);
  }
});

test('can delete a comment', async ({ page, api }) => {
  const { createRecipe, hardDeleteRecipe } = api('e2e-admin');
  const { createComment } = api('e2e-user');

  const recipe = await createRecipe({ title: 'Chocolate fudge cake' });

  try {
    await createComment(recipe.id, { body: 'Delicious with cream' });

    await page.goto(`recipes/${recipe.id}`);

    await page
      .getByRole('article')
      .filter({ hasText: 'Delicious with cream' })
      .getByRole('link', { name: 'Delete' })
      .click();
    await page.getByRole('button', { name: 'Delete' }).click();

    await expect(page.locator('h1')).toHaveText('Chocolate fudge cake');
    await expect(page.getByText('Delicious with cream')).toHaveCount(0);
  } finally {
    await hardDeleteRecipe(recipe.id);
  }
});
