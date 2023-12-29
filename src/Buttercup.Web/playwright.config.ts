import { defineConfig, devices } from '@playwright/test';

const baseURL = 'http://localhost:5005';

export default defineConfig({
  testDir: './e2e',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  workers: process.env.CI ? 1 : undefined,
  use: {
    baseURL,
    trace: 'on-first-retry',
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
    {
      name: 'firefox',
      use: { ...devices['Desktop Firefox'] },
    },
    {
      name: 'webkit',
      use: { ...devices['Desktop Safari'] },
    },
  ],
  webServer: {
    command: `dotnet run --environment E2E --urls ${baseURL} ${
      process.env.CI ? '--configuration Release --no-build' : ''
    }`,
    url: baseURL,
    reuseExistingServer: !process.env.CI,
  },
});
