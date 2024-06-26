import { authStatePath } from './auth-state';
import { expect, test } from './test';

test.use({ storageState: authStatePath('e2e-user') });

test('can comment on a recipe', async ({ page, api }) => {
  const { createRecipe, hardDeleteRecipe } = api('e2e-admin');

  const { id } = await createRecipe();

  try {
    await page.goto(`recipes/${id}`);

    const commentBody = 'You can also use precooked beans to save time';

    await page.getByPlaceholder('Write a comment…').fill(commentBody);
    await page.getByRole('button', { name: 'Add' }).click();

    await expect(page.getByText(commentBody)).toBeInViewport();
  } finally {
    await hardDeleteRecipe(id);
  }
});

test('can delete a comment', async ({ page, api }) => {
  const { createRecipe, hardDeleteRecipe } = api('e2e-admin');
  const { createComment } = api('e2e-user');

  const { id, title } = await createRecipe();

  try {
    const comment = await createComment(id);

    await page.goto(`recipes/${id}`);

    await page
      .locator(`#comment${comment.id}`)
      .getByRole('link', { name: 'Delete' })
      .click();
    await page.getByRole('button', { name: 'Delete' }).click();

    await expect(page.locator('h1')).toHaveText(title);
    await expect(page.getByText(comment.body)).toHaveCount(0);
  } finally {
    await hardDeleteRecipe(id);
  }
});
