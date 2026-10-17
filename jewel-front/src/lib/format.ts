const eur = new Intl.NumberFormat('fr-FR', {
  style: 'currency',
  currency: 'EUR',
});

/** Formate un montant en euros (ex. 12.5 → « 12,50 € »). */
export function formatPrice(value: number): string {
  return eur.format(value);
}

export const MISSING_DATE = '—';

// L'API renvoie les dates UTC sans fuseau (colonne TIMESTAMP) : sans le `Z`, le
// navigateur les lirait comme des heures locales.
const HAS_TIME_ZONE = /(Z|[+-]\d{2}:?\d{2})$/;

/**
 * Formate une date UTC en `dd/MM/yyyy` (`null` → « — »).
 * Le fuseau est celui du navigateur, sauf si `timeZone` est fourni.
 */
export function formatDate(value: string | null, timeZone?: string): string {
  if (!value) return MISSING_DATE;
  const utcValue = HAS_TIME_ZONE.test(value) ? value : `${value}Z`;
  return new Intl.DateTimeFormat('fr-FR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    timeZone,
  }).format(new Date(utcValue));
}

/**
 * Formate une date seule `yyyy-MM-dd` en `dd/MM/yyyy` (`null` → « — »).
 * Découpée comme une chaîne : passer par `Date` la décalerait d'un jour à l'ouest d'UTC.
 */
export function formatDateOnly(value: string | null): string {
  if (!value) return MISSING_DATE;
  const [year, month, day] = value.split('-');
  return `${day}/${month}/${year}`;
}
