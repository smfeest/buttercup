import { test as base } from '@playwright/test';
import { api } from './fixtures/api';

export const test = base.extend<{
  api: (username: string) => ReturnType<typeof api>;
}>({
  async api({ baseURL }, use) {
    await use((username) => api(baseURL!, username));
  },
});
