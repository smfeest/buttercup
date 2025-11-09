import type { Page } from '@playwright/test';

export type ChangePasswordFormFixture = ReturnType<typeof changePasswordForm>;

export const changePasswordForm = (page: Page) => ({
  currentPasswordInput: page.getByLabel('Current password'),
  newPasswordInput: page.getByLabel('New password (6 characters or more)'),
  confirmNewPasswordInput: page.getByLabel('Confirm new password'),
  changePasswordButton: page.getByRole('button', { name: 'Change password' }),
});
