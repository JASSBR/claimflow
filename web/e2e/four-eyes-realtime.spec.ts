import { expect, test } from '@playwright/test';
import { decide, declareClaim, loginAs } from './support';

test('two managers: a decision appears live, and the approver cannot release the payment', async ({ browser }) => {
  const lea = await loginAs(browser, 'Léa Martin');
  const number = await declareClaim(lea, 3_200);
  await decide(lea, 'Prendre en charge');
  const claimUrl = lea.url();

  const nadia = await loginAs(browser, 'Nadia Haddad');
  await expect(nadia.getByText('Temps réel', { exact: true })).toBeVisible();

  const karim = await loginAs(browser, 'Karim Benali');
  await karim.goto(claimUrl);
  await decide(karim, 'Accepter');
  await karim.getByRole('button', { name: 'Accepter', exact: true }).click();
  await expect(karim.locator('h1')).toContainText('Accepté');

  // Nadia never reloaded: the outbox delivered the event and SignalR pushed it to her dashboard.
  await expect(nadia.locator('.feed')).toContainText(`Karim Benali a accepté ${number}`, { timeout: 15_000 });

  // Four-eyes: Karim approved, so he is told why he cannot pay…
  await expect(karim.getByText('Principe des quatre yeux.')).toBeVisible();
  await expect(karim.getByRole('button', { name: "Verser l'indemnité" })).toHaveCount(0);

  // …and Nadia, the second signature, can.
  await nadia.goto(claimUrl);
  await decide(nadia, "Verser l'indemnité");
  await expect(nadia.locator('h1')).toContainText('Indemnisé');
  await expect(karim.locator('h1')).toContainText('Indemnisé', { timeout: 15_000 });
});
