import { expect } from '@playwright/test';
import { authStatePath } from './auth-state';
import { test } from './test';

test.use({ storageState: authStatePath('e2e-user') });

test('can sign out', async ({ page, navigation }) => {
  await page.goto('/');

  await navigation.menuButton.click();
  await navigation.signOutLink.click();

  await expect(page).toHaveTitle(/Sign in/);
});

test('user is signed out on deactivation', async ({
  api,
  page,
  navigation,
  session: { signIn },
}) => {
  const { createTestUser, deactivateUser, hardDeleteTestUser } =
    api('e2e-admin');
  const user = await createTestUser();

  try {
    await signIn(user.email, user.password);
    await navigation.menuButton.click();
    await expect(page.getByText(`Signed in as ${user.name}`)).toBeVisible();

    await deactivateUser(user.id);

    await navigation.yourAccountLink.click();
    await expect(page).toHaveTitle(/Sign in/);
  } finally {
    await hardDeleteTestUser(user.id);
  }
});
