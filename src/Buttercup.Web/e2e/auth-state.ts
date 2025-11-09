import { join } from 'path';

export const authStatePath = (username: string) =>
  join(__dirname, `../.playwright/auth/${username}.json`);
