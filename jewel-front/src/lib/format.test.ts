import { describe, expect, it } from 'vitest';
import { formatDate, formatDateOnly } from './format';

const PARIS = 'Europe/Paris';

describe('formatDate', () => {
  it('affiche « — » pour une date absente', () => {
    expect(formatDate(null)).toBe('—');
  });

  it('formate une date en jj/mm/aaaa', () => {
    expect(formatDate('2026-03-05T10:00:00Z', PARIS)).toBe('05/03/2026');
  });

  it('lit une date sans fuseau comme une date UTC', () => {
    expect(formatDate('2026-10-17T23:30:00', PARIS)).toBe('18/10/2026');
  });

  it('respecte un décalage explicite', () => {
    expect(formatDate('2026-10-17T23:30:00+02:00', PARIS)).toBe('17/10/2026');
  });
});

describe('formatDateOnly', () => {
  it('affiche « — » pour une date absente', () => {
    expect(formatDateOnly(null)).toBe('—');
  });

  it('formate une date seule sans décalage de jour', () => {
    expect(formatDateOnly('1990-01-01')).toBe('01/01/1990');
  });
});
