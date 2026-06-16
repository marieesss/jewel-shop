/** Concatène des classes conditionnelles (ignore les valeurs falsy). */
export function cn(...classes: Array<string | false | null | undefined>): string {
  return classes.filter(Boolean).join(' ');
}
