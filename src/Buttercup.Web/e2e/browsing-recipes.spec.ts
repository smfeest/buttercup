import { authStatePath } from './auth-state';
import { Navigation } from './helpers/navigation';
import { randomString } from './helpers/random';
import { expect, test } from './test';

test.use({ storageState: authStatePath('e2e-user') });

test('can view a recipe', async ({
  page,
  api: { createRecipe, deleteRecipe },
}) => {
  const title = `${randomString()} Cheese on toast`;
  const ingredients = ['1 slice of bread', '2 slices of cheese'];
  const steps = [
    'Place bread under grill until lightly toasted',
    'Flip toast over and add cheese',
    'Place bread back under grill until cheese starts to bubble',
  ];

  const { id } = await createRecipe({
    title,
    ingredients: ingredients.join('\n'),
    method: steps.join('\n'),
  });

  try {
    await page.goto('/');

    const navigation = new Navigation(page);
    await navigation.allRecipesButton.click();

    await page.getByRole('link', { name: title }).click();

    await expect(page.locator('h1')).toHaveText(title);
    await expect(page.locator('ul > li')).toContainText(ingredients);
    await expect(page.locator('ol > li')).toContainText(steps);
  } finally {
    await deleteRecipe(id);
  }
});

test('can find recipes by title', async ({
  page,
  api: { createRecipe, deleteRecipe },
}) => {
  const prefix = randomString();
  const prefixedTitle = (title: string) => `${prefix} ${title}`;

  const recipes = await Promise.all(
    ['Chocolate cookie', 'Chocolate cake', 'Triple chocolate cookie'].map(
      (title) => createRecipe({ title: prefixedTitle(title) }),
    ),
  );

  try {
    await page.goto('/');

    const navigation = new Navigation(page);
    await navigation.allRecipesButton.click();

    await page.getByLabel('Find a recipe').fill(`${prefix} kie choc`);

    await expect(
      page.getByText(prefixedTitle('Chocolate cookie')),
    ).toBeVisible();
    await expect(
      page.getByText(prefixedTitle('Chocolate cake')),
    ).not.toBeVisible();
    await expect(
      page.getByText(prefixedTitle('Triple chocolate cookie')),
    ).toBeVisible();
  } finally {
    for (const { id } of recipes) {
      await deleteRecipe(id);
    }
  }
});
