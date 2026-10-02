import { expect, test } from '@playwright/test';
import { decide, declareClaim, loginAs } from './support';

test('a handler declares, reviews and approves a claim within her delegation', async ({ browser }) => {
  const lea = await loginAs(browser, 'Léa Martin');
  await expect(lea.getByRole('heading', { name: /(Bonjour|Bonsoir), Léa/ })).toBeVisible();

  const number = await declareClaim(lea, 1_800);
  await expect(lea.locator('h1')).toContainText('Déclaré');

  await decide(lea, 'Prendre en charge');
  await expect(lea.locator('h1')).toContainText('En instruction');

  await decide(lea, 'Accepter');
  await expect(lea.getByText('Votre délégation : 10 000 €')).toBeVisible();
  await lea.getByRole('button', { name: 'Accepter', exact: true }).click();
  await expect(lea.locator('h1')).toContainText('Accepté');

  // The audit trail names who did what.
  await expect(lea.locator('app-claim-timeline')).toContainText('Léa Martin · Accepter');
  // Releasing the payment is a manager's job: the button is not offered.
  await expect(lea.getByText('Le versement est réservé aux responsables indemnisation.')).toBeVisible();

  await lea.goto('/claims');
  await lea.getByPlaceholder('N° de sinistre ou de contrat…').fill(number);
  await expect(lea.locator('tbody tr')).toHaveCount(1);
});

test('the server blocks an approval above the delegated limit', async ({ browser }) => {
  const lea = await loginAs(browser, 'Léa Martin');
  await declareClaim(lea, 25_000);
  await decide(lea, 'Prendre en charge');

  await decide(lea, 'Accepter');
  await expect(lea.getByText('Au-delà, un responsable doit accepter.')).toBeVisible();
  await lea.getByRole('button', { name: 'Accepter', exact: true }).click();

  await expect(lea.getByRole('alert')).toContainText('approval authority');
  await expect(lea.locator('h1')).toContainText('En instruction');
});
