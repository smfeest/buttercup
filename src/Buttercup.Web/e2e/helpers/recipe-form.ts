import type { Page } from '@playwright/test';

export class RecipeForm {
  public constructor(public readonly page: Page) {}

  public readonly titleInput = this.page.getByLabel('Title');
  public readonly ingredientsInput = this.page.getByLabel('Ingredients');
  public readonly methodInput = this.page.getByLabel('Method');
  public readonly saveButton = this.page.getByRole('button', { name: 'Save' });
}
