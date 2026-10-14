// Reproduit PasswordPolicyRules côté API : mêmes règles, mêmes messages, même ordre.

export const PASSWORD_MIN_LENGTH = 8;
export const PASSWORD_MAX_LENGTH = 100;

const REQUIRED_MESSAGE = 'Le mot de passe est requis.';
const MAX_LENGTH_MESSAGE = `${PASSWORD_MAX_LENGTH} caractères maximum.`;

export interface PasswordRule {
  id: string;
  label: string;
  message: string;
  test: (pwd: string) => boolean;
}

const minLengthRule: PasswordRule = {
  id: 'minLength',
  label: `${PASSWORD_MIN_LENGTH} caractères minimum`,
  message: `Le mot de passe doit contenir au moins ${PASSWORD_MIN_LENGTH} caractères.`,
  test: (pwd) => pwd.length >= PASSWORD_MIN_LENGTH,
};

const characterRules: PasswordRule[] = [
  {
    id: 'uppercase',
    label: 'Une majuscule',
    message: 'Le mot de passe doit contenir au moins une majuscule.',
    test: (pwd) => /\p{Lu}/u.test(pwd),
  },
  {
    id: 'digit',
    label: 'Un chiffre',
    message: 'Le mot de passe doit contenir au moins un chiffre.',
    test: (pwd) => /[0-9]/.test(pwd),
  },
  {
    id: 'special',
    label: 'Un caractère spécial',
    message: 'Le mot de passe doit contenir au moins un caractère spécial.',
    test: (pwd) => /[^\p{L}0-9\s]/u.test(pwd),
  },
  {
    id: 'noSpace',
    label: 'Aucun espace',
    message: "Le mot de passe ne doit pas contenir d'espace.",
    test: (pwd) => !/\s/.test(pwd),
  },
];

export const passwordRules: PasswordRule[] = [minLengthRule, ...characterRules];

function failedMessages(rules: PasswordRule[], password: string): string[] {
  return rules.filter((rule) => !rule.test(password)).map((rule) => rule.message);
}

/** Messages des règles non respectées, dans l'ordre du back. Tableau vide si le mot de passe est valide. */
export function validatePassword(password: string): string[] {
  if (password === '') return [REQUIRED_MESSAGE];

  const isTooLong = password.length > PASSWORD_MAX_LENGTH;
  return [
    ...failedMessages([minLengthRule], password),
    ...(isTooLong ? [MAX_LENGTH_MESSAGE] : []),
    ...failedMessages(characterRules, password),
  ];
}
