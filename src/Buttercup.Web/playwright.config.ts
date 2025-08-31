import { defineConfig, devices } from '@playwright/test';

const baseURL = 'http://localhost:5000';

export default defineConfig({
  outputDir: './.playwright/results',
  testDir: './e2e',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  maxFailures: process.env.CI ? 10 : undefined,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  expect: {
    timeout: 1000,
  },
  use: {
    baseURL,
    locale: 'en-GB',
    timezoneId: 'Europe/London',
    trace: 'on-first-retry',
  },
  projects: [
    {
      name: 'setup',
      testMatch: /.*\.setup\.ts/,
    },
    {
      name: 'chromium',
      dependencies: ['setup'],
      use: { ...devices['Desktop Chrome'] },
    },
    {
      name: 'firefox',
      dependencies: ['setup'],
      use: { ...devices['Desktop Firefox'] },
    },
    {
      name: 'webkit',
      dependencies: ['setup'],
      use: { ...devices['Desktop Safari'] },
    },
  ],
  webServer: {
    command: `dotnet run --urls ${baseURL} ${
      process.env.CI ? '--configuration Release --no-build' : ''
    }`,
    url: baseURL,
    reuseExistingServer: !process.env.CI,
  },
});
