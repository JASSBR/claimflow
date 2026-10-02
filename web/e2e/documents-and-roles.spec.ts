import path from 'node:path';
import { expect, test } from '@playwright/test';
import { declareClaim, loginAs } from './support';

test('a handler attaches evidence to a claim', async ({ browser }) => {
  const lea = await loginAs(browser, 'Léa Martin');
  await declareClaim(lea);

  await lea.locator('app-documents-panel input[type=file]').setInputFiles(path.join(__dirname, 'fixtures', 'facture.pdf'));

  await expect(lea.locator('app-documents-panel .document')).toContainText('facture.pdf');
  await expect(lea.getByText('facture.pdf ajouté au dossier')).toBeVisible();
});

test('the seeded collision claim comes with its three documents', async ({ browser }) => {
  const sophie = await loginAs(browser, 'Sophie Laurent');
  await sophie.goto('/claims');
  await sophie.getByPlaceholder('N° de sinistre ou de contrat…').fill('POL-104233');
  await sophie.getByRole('link', { name: /SIN-\d{4}-\d{6}/ }).first().click();
  await expect(sophie.locator('app-documents-panel .document')).toHaveCount(3);
});

test('an auditor can read everything but change nothing', async ({ browser }) => {
  const sophie = await loginAs(browser, 'Sophie Laurent');
  await expect(sophie.getByText('Profil en lecture seule')).toBeVisible();
  await expect(sophie.getByRole('link', { name: 'Déclarer' })).toHaveCount(0);

  await sophie.goto('/claims');
  await sophie.locator('tbody tr a').first().click();
  await expect(sophie.getByText('Votre profil est en lecture seule.')).toBeVisible();
  await expect(sophie.locator('.dropzone')).toHaveCount(0);
});
