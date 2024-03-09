import { test as base } from '@playwright/test';
import { api } from './fixtures/api';
export { expect } from '@playwright/test';

export const test = base.extend<{ api: ReturnType<typeof api> }>({
  async api({ baseURL }, use) {
    await use(api(baseURL!));
  },
});
