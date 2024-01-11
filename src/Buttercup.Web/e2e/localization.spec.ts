import { expect, test } from '@playwright/test';

test.describe('when browser locale is set to fr-FR', () => {
  test.use({ locale: 'fr-FR' });

  test('page is displayed in French by default', async ({ page }) => {
    await page.goto('/sign-in');
    await expect(page.locator('h1')).toHaveText('Connexion');
  });
});

test.describe('when browser locale is set to en-GB', () => {
  test.use({ locale: 'en-GB' });

  test('page is displayed in English by default', async ({ page }) => {
    await page.goto('/sign-in');
    await expect(page.locator('h1')).toHaveText('Sign in');
  });

  test('can request page in French through query param', async ({ page }) => {
    await page.goto('/sign-in?culture=fr-FR');
    await expect(page.locator('h1')).toHaveText('Connexion');
  });
});
