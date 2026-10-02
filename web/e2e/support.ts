import { Browser, Page, expect } from '@playwright/test';

export type PersonaName = 'Léa Martin' | 'Karim Benali' | 'Nadia Haddad' | 'Sophie Laurent';

/** Each persona gets its own browser context, like two colleagues on two machines. */
export async function loginAs(browser: Browser, persona: PersonaName): Promise<Page> {
  const context = await browser.newContext();
  const page = await context.newPage();
  await page.goto('/login');
  await page.getByRole('button', { name: new RegExp(persona) }).click();
  await expect(page).toHaveURL(/\/dashboard$/);
  return page;
}

/** Declares a fresh claim through the UI and returns its number, so tests never depend on each other's data. */
export async function declareClaim(page: Page, amount = 1_800): Promise<string> {
  await page.goto('/claims/new');
  await page.getByLabel('N° de contrat').fill(`POL-${Math.floor(100_000 + Math.random() * 899_999)}`);
  await page.getByLabel('Type de sinistre').selectOption('Home');
  await page.getByLabel('Montant estimé (€)').fill(String(amount));
  await page.getByLabel('Circonstances').fill('Dégât des eaux dans la salle de bain, plafond du dessous taché.');
  await page.getByRole('button', { name: 'Déclarer le sinistre' }).click();
  await expect(page).toHaveURL(/\/claims\/[0-9a-f-]{36}$/);
  return (await page.locator('h1').innerText()).split(/\s/)[0]!;
}

export async function decide(page: Page, action: string, confirm?: string): Promise<void> {
  await page.getByRole('button', { name: action, exact: true }).click();
  if (confirm) {
    await page.getByRole('button', { name: confirm, exact: true }).click();
  }
}
