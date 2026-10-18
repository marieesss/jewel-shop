// Reproduit ImageFileRules côté API (upload d'image des chaînes et des breloques) :
// mêmes règles, mêmes messages.

export const IMAGE_MAX_SIZE_MB = 5;
const IMAGE_MAX_SIZE_BYTES = IMAGE_MAX_SIZE_MB * 1024 * 1024;

const EXTENSIONS_BY_CONTENT_TYPE: Record<string, string[]> = {
  'image/jpeg': ['.jpg', '.jpeg'],
  'image/png': ['.png'],
  'image/webp': ['.webp'],
};

/** Valeur de l'attribut `accept` du sélecteur de fichier. */
export const IMAGE_ACCEPT = Object.keys(EXTENSIONS_BY_CONTENT_TYPE).join(',');

export const IMAGE_CONTENT_TYPE_MESSAGE = 'Le fichier doit être une image JPEG, PNG ou WEBP.';
export const IMAGE_EXTENSION_MESSAGE = "L'extension du fichier ne correspond pas à son format.";
export const IMAGE_MAX_SIZE_MESSAGE = `L'image ne doit pas dépasser ${IMAGE_MAX_SIZE_MB} Mo.`;

function extensionOf(fileName: string): string {
  const dot = fileName.lastIndexOf('.');
  return dot === -1 ? '' : fileName.slice(dot).toLowerCase();
}

/** Messages des règles non respectées, dans l'ordre du back. Tableau vide si l'image est valide. */
export function validateImageFile(file: Pick<File, 'name' | 'type' | 'size'>): string[] {
  const extensions = EXTENSIONS_BY_CONTENT_TYPE[file.type.toLowerCase()];
  // Comme au back, l'extension n'est contrôlée que pour un type autorisé.
  const extensionMatches = !extensions || extensions.includes(extensionOf(file.name));
  return [
    ...(extensions ? [] : [IMAGE_CONTENT_TYPE_MESSAGE]),
    ...(extensionMatches ? [] : [IMAGE_EXTENSION_MESSAGE]),
    ...(file.size > IMAGE_MAX_SIZE_BYTES ? [IMAGE_MAX_SIZE_MESSAGE] : []),
  ];
}
