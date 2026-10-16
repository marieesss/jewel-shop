// Reproduit ForgotPasswordCommandValidator côté API : mêmes règles, mêmes messages.

const REQUIRED_MESSAGE = "L'email est requis.";
const INVALID_MESSAGE = "L'email n'est pas valide.";

// Même contrôle que EmailAddress() de FluentValidation 11 : un seul @, ni en premier ni en dernier.
function hasValidEmailShape(email: string): boolean {
  const at = email.indexOf('@');
  return at > 0 && at !== email.length - 1 && at === email.lastIndexOf('@');
}

/** Messages des règles non respectées, dans l'ordre du back. Tableau vide si l'email est valide. */
export function validateEmail(email: string): string[] {
  const isBlank = email.trim() === '';
  return [
    ...(isBlank ? [REQUIRED_MESSAGE] : []),
    ...(hasValidEmailShape(email) ? [] : [INVALID_MESSAGE]),
  ];
}
