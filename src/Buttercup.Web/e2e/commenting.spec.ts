import { authStatePath } from './auth-state';
import { expect, test } from './test';

test.use({ storageState: authStatePath('e2e-user') });

test('can comment on a recipe', async ({
  page,
  api: { createRecipe, deleteRecipe },
}) => {
  const { id } = await createRecipe();

  try {
    await page.goto(`recipes/${id}`);

    const commentBody = 'You can also use precooked beans to save time';

    await page.getByPlaceholder('Write a comment…').fill(commentBody);
    await page.getByRole('button', { name: 'Add' }).click();

    await expect(page.getByText(commentBody)).toBeInViewport();
  } finally {
    await deleteRecipe(id);
  }
});
