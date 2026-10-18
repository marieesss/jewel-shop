import { useEffect, useRef, useState } from 'react';
import { IMAGE_ACCEPT, IMAGE_MAX_SIZE_MB, validateImageFile } from '../../lib/imageFile';
import { FieldShell } from './FieldShell';

interface ImageUploadProps {
  label: string;
  value: File | null;
  onChange: (file: File | null) => void;
  error?: string;
  required?: boolean;
  /** Remonte une erreur de validation locale (type, extension, taille). */
  onValidationError?: (message: string | null) => void;
}

/** Sélecteur de fichier image réutilisable, avec aperçu et retrait. */
export function ImageUpload({
  label,
  value,
  onChange,
  error,
  required,
  onValidationError,
}: ImageUploadProps) {
  const inputRef = useRef<HTMLInputElement>(null);
  const [preview, setPreview] = useState<string | null>(null);

  // Génère/révoque l'URL d'aperçu en suivant le fichier courant.
  useEffect(() => {
    if (!value) {
      setPreview(null);
      return;
    }
    const url = URL.createObjectURL(value);
    setPreview(url);
    return () => URL.revokeObjectURL(url);
  }, [value]);

  const handleSelect = (file: File | null) => {
    onValidationError?.(null);
    if (!file) {
      onChange(null);
      return;
    }
    const [firstError] = validateImageFile(file);
    if (firstError) {
      onValidationError?.(firstError);
      return;
    }
    onChange(file);
  };

  const clear = () => {
    onChange(null);
    onValidationError?.(null);
    if (inputRef.current) inputRef.current.value = '';
  };

  return (
    <FieldShell label={label} error={error} required={required}>
      <input
        ref={inputRef}
        type="file"
        accept={IMAGE_ACCEPT}
        className="hidden"
        onChange={(e) => handleSelect(e.target.files?.[0] ?? null)}
      />

      {value && preview ? (
        <div className="flex items-center gap-3 rounded-xl border border-lin bg-white p-3">
          <img src={preview} alt="Aperçu" className="h-16 w-16 shrink-0 rounded-lg object-cover" />
          <div className="min-w-0 flex-1">
            <p className="truncate font-body text-sm font-medium text-encre">{value.name}</p>
            <p className="font-body text-[11px] text-slate">{(value.size / 1024).toFixed(0)} Ko</p>
          </div>
          <button
            type="button"
            onClick={clear}
            className="shrink-0 font-body text-[12px] font-medium text-fuchsia transition-opacity hover:opacity-70"
          >
            Retirer
          </button>
        </div>
      ) : (
        <button
          type="button"
          onClick={() => inputRef.current?.click()}
          className="flex w-full flex-col items-center justify-center gap-1 rounded-xl border-[1.5px] border-dashed border-slate/40 bg-lin py-6 font-body text-sm text-slate transition-colors hover:border-fuchsia hover:text-fuchsia"
        >
          <span className="text-base">＋</span>
          Choisir une image
          <span className="text-[11px] text-slate/70">
            PNG, JPG, WEBP — max {IMAGE_MAX_SIZE_MB} Mo
          </span>
        </button>
      )}
    </FieldShell>
  );
}
