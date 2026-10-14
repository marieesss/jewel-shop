import { describe, expect, it } from 'vitest';
import {
  PASSWORD_MAX_LENGTH,
  PASSWORD_MIN_LENGTH,
  passwordRules,
  validatePassword,
} from './passwordPolicy';

const LENGTH_MESSAGE = 'Le mot de passe doit contenir au moins 8 caractères.';
const UPPERCASE_MESSAGE = 'Le mot de passe doit contenir au moins une majuscule.';
const DIGIT_MESSAGE = 'Le mot de passe doit contenir au moins un chiffre.';
const SPECIAL_MESSAGE = 'Le mot de passe doit contenir au moins un caractère spécial.';
const SPACE_MESSAGE = "Le mot de passe ne doit pas contenir d'espace.";

function passedRuleIds(password: string): string[] {
  return passwordRules.filter((rule) => rule.test(password)).map((rule) => rule.id);
}

function validPasswordOfLength(length: number): string {
  return 'Ab1!'.padEnd(length, 'a');
}

describe('passwordRules', () => {
  it('ne respecte que la règle « Aucun espace » pour abc', () => {
    expect(passedRuleIds('abc')).toEqual(['noSpace']);
  });

  it.each(['Abcdef1!', 'Écrin12!'])('respecte toutes les règles pour %s', (password) => {
    expect(passedRuleIds(password)).toEqual(passwordRules.map((rule) => rule.id));
  });

  it('ne compte pas une lettre accentuée minuscule comme majuscule', () => {
    expect(passedRuleIds('éabcde1!')).not.toContain('uppercase');
  });
});

describe('validatePassword', () => {
  it('renvoie seulement le message requis pour un mot de passe vide', () => {
    expect(validatePassword('')).toEqual(['Le mot de passe est requis.']);
  });

  it.each(['Abcdef1!', 'Écrin12!'])('accepte %s', (password) => {
    expect(validatePassword(password)).toEqual([]);
  });

  it.each([
    ['abcdef1!', UPPERCASE_MESSAGE],
    ['éabcde1!', UPPERCASE_MESSAGE],
    ['Abcdefg!', DIGIT_MESSAGE],
    ['Abcdefg1', SPECIAL_MESSAGE],
    ['Abcd ef1!', SPACE_MESSAGE],
  ])('refuse %s avec le message de la règle non respectée', (password, message) => {
    expect(validatePassword(password)).toEqual([message]);
  });

  it("renvoie les messages de longueur, de caractère spécial et d'espace dans l'ordre du back", () => {
    expect(validatePassword('Ab1 ')).toEqual([LENGTH_MESSAGE, SPECIAL_MESSAGE, SPACE_MESSAGE]);
  });

  it.each([PASSWORD_MIN_LENGTH, PASSWORD_MAX_LENGTH])(
    'accepte un mot de passe de %i caractères',
    (length) => {
      expect(validatePassword(validPasswordOfLength(length))).toEqual([]);
    }
  );

  it('refuse un mot de passe de 7 caractères', () => {
    expect(validatePassword(validPasswordOfLength(PASSWORD_MIN_LENGTH - 1))).toEqual([
      LENGTH_MESSAGE,
    ]);
  });

  it('refuse un mot de passe de 101 caractères', () => {
    expect(validatePassword(validPasswordOfLength(PASSWORD_MAX_LENGTH + 1))).toEqual([
      '100 caractères maximum.',
    ]);
  });
});
