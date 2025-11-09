import { test as base } from '@playwright/test';
import { api, ApiFixture } from './fixtures/api';
import { navigation, NavigationFixture } from './fixtures/navigation';

export const test = base.extend<{
  api: ApiFixture;
  navigation: NavigationFixture;
}>({
  api: ({ baseURL }, use) => use(api(baseURL)),
  navigation: ({ page }, use) => use(navigation(page)),
});
