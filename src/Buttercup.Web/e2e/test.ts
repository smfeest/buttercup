import { test as base } from '@playwright/test';
import { api, Api } from './fixtures/api';
import { navigation, Navigation } from './fixtures/navigation';

export const test = base.extend<{ api: Api; navigation: Navigation }>({
  api,
  navigation,
});
