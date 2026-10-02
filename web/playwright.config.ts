import { defineConfig, devices } from '@playwright/test';

/**
 * End-to-end tests against a running stack (API + PostgreSQL + Azurite + Angular).
 * Locally: start the AppHost, then E2E_BASE_URL=<web endpoint> npm run e2e. In CI: see the e2e job of ci.yml.
 */
export default defineConfig({
  testDir: './e2e',
  fullyParallel: false,
  forbidOnly: !!process.env['CI'],
  retries: process.env['CI'] ? 1 : 0,
  reporter: process.env['CI'] ? [['github'], ['html', { open: 'never' }]] : 'list',
  timeout: 60_000,
  // A freshly started API compiles its EF queries on first use: allow cold-start latency.
  expect: { timeout: 10_000 },
  use: {
    baseURL: process.env['E2E_BASE_URL'] ?? 'http://localhost:4200',
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
    locale: 'fr-FR',
  },
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }],
});
