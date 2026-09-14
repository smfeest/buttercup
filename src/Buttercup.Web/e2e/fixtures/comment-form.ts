import type {
  Locator,
  PlaywrightTestArgs,
  TestFixture,
} from '@playwright/test';

export type CommentForm = {
  input: Locator;
  saveButton: Locator;
};

export const commentForm: TestFixture<CommentForm, PlaywrightTestArgs> = async (
  { page },
  use,
) =>
  use({
    input: page.getByLabel('Comment'),
    saveButton: page.getByRole('button', { name: 'Save' }),
  });
