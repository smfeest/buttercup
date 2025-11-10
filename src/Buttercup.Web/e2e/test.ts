import { test as base } from '@playwright/test';
import { api, ApiFixture } from './fixtures/api';

export const test = base.extend<{
  api: ApiFixture;
}>({
  api: ({ baseURL }, use) => use(api(baseURL)),
});
