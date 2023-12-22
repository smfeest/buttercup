import type { Page, Locator } from '@playwright/test';

export class HomePage {
  public readonly menuButton: Locator;

  constructor(public readonly page: Page) {
    this.menuButton = page.getByRole('button', { name: 'Toggle menu' });
  }

  public async toggleMenu() {
    await this.menuButton.click();
  }
}
