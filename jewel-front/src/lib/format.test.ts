import { describe, expect, it } from 'vitest';
import {
  formatDate,
  formatDateOnly,
  formatLengthCm,
  formatOptionalPrice,
  formatPrice,
} from './format';

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

describe('formatOptionalPrice', () => {
  it('affiche « — » pour un prix absent', () => {
    expect(formatOptionalPrice(null)).toBe('—');
    expect(formatOptionalPrice(undefined)).toBe('—');
  });

  it('formate un prix renseigné comme formatPrice', () => {
    expect(formatOptionalPrice(12.5)).toBe(formatPrice(12.5));
  });

  it('formate un prix à zéro', () => {
    expect(formatOptionalPrice(0)).toBe(formatPrice(0));
  });
});

describe('formatLengthCm', () => {
  it('affiche « — » pour une longueur absente', () => {
    expect(formatLengthCm(null)).toBe('—');
  });

  it('affiche la longueur en cm', () => {
    expect(formatLengthCm(45)).toBe('45 cm');
  });
});
