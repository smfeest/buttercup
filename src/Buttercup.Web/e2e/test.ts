import { test as base } from '@playwright/test';
import { api, Api } from './fixtures/api';

export const test = base.extend<{ api: Api }>({
  api,
});
