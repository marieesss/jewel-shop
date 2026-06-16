const eur = new Intl.NumberFormat('fr-FR', {
  style: 'currency',
  currency: 'EUR',
});

/** Formate un montant en euros (ex. 12.5 → « 12,50 € »). */
export function formatPrice(value: number): string {
  return eur.format(value);
}
