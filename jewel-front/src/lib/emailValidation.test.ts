import { describe, expect, it } from 'vitest';
import { validateEmail } from './emailValidation';

const REQUIRED_MESSAGE = "L'email est requis.";
const INVALID_MESSAGE = "L'email n'est pas valide.";

describe('validateEmail', () => {
  it.each(['marie@exemple.com', 'a@b'])('accepte %s', (email) => {
    expect(validateEmail(email)).toEqual([]);
  });

  it('exige un email', () => {
    expect(validateEmail('')).toEqual([REQUIRED_MESSAGE, INVALID_MESSAGE]);
  });

  it('considère un email fait d’espaces comme absent', () => {
    expect(validateEmail('   ')[0]).toBe(REQUIRED_MESSAGE);
  });

  it.each([
    ['sans @', 'marie.exemple.com'],
    ['avec un @ en premier', '@exemple.com'],
    ['avec un @ en dernier', 'marie@'],
    ['avec deux @', 'marie@@exemple.com'],
  ])('refuse un email %s', (_label, email) => {
    expect(validateEmail(email)).toEqual([INVALID_MESSAGE]);
  });
});
