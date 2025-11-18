import { BrowserContext } from '@playwright/test';
import { authStatePath } from './auth-state';
import { Session } from './fixtures/session';
import { test as setup } from './test';

const storeAuthenticatedState = async (
  context: BrowserContext,
  { signIn }: Session,
  username: string,
) => {
  await signIn(`${username}@example.com`, `${username}-pass`);
  await context.storageState({ path: authStatePath(username) });
};

setup('store authenticated admin state', ({ context, session }) =>
  storeAuthenticatedState(context, session, 'e2e-admin'),
);

setup('store authenticated user state', ({ context, session }) =>
  storeAuthenticatedState(context, session, 'e2e-user'),
);
