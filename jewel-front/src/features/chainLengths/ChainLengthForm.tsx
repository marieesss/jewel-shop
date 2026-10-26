import { useState, type FormEvent } from 'react';
import { createChainLength, updateChainLength, type ChainLength } from '../../api/chainLengths';
import { Alert } from '../../components/ui/Alert';
import { Button } from '../../components/ui/Button';
import { Card } from '../../components/ui/Card';
import { TextField } from '../../components/ui/TextField';
import {
  CREATE_SUCCESS_MESSAGE,
  LABEL_MAX_LENGTH,
  MAX_LENGTH_CM,
  MIN_LENGTH_CM,
  UPDATE_SUCCESS_MESSAGE,
  chainLengthFormErrorsFromApi,
  chainLengthFormFrom,
  emptyChainLengthForm,
  noChainLengthFormErrors,
  toChainLengthInput,
  validateChainLengthForm,
  type ChainLengthField,
  type ChainLengthFormErrors,
  type ChainLengthFormValues,
} from '../../lib/chainLengthForm';

interface ChainLengthFormProps {
  /** Longueur à modifier ; absente ⇒ création. */
  length?: ChainLength;
  /** Appelé au début de chaque envoi, pour effacer les messages de l'action précédente. */
  onSubmitStart?: () => void;
  onSaved: (successMessage: string) => void;
  onCancel?: () => void;
}

/** Formulaire de création ou de modification d'une longueur de chaîne (admin). */
export function ChainLengthForm({
  length,
  onSubmitStart,
  onSaved,
  onCancel,
}: ChainLengthFormProps) {
  const isEdit = Boolean(length);
  const [values, setValues] = useState<ChainLengthFormValues>(
    length ? chainLengthFormFrom(length) : emptyChainLengthForm
  );
  const [errors, setErrors] = useState<ChainLengthFormErrors>(noChainLengthFormErrors);
  const [saving, setSaving] = useState(false);

  const setField = (field: ChainLengthField) => (value: string) =>
    setValues((current) => ({ ...current, [field]: value }));

  const save = (input: ReturnType<typeof toChainLengthInput>) =>
    length ? updateChainLength(length.id, input) : createChainLength(input);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    onSubmitStart?.();

    const fieldErrors = validateChainLengthForm(values);
    setErrors({ fields: fieldErrors, form: null });
    if (Object.keys(fieldErrors).length > 0) return;

    setSaving(true);
    try {
      await save(toChainLengthInput(values));
      if (!isEdit) setValues(emptyChainLengthForm);
      onSaved(isEdit ? UPDATE_SUCCESS_MESSAGE : CREATE_SUCCESS_MESSAGE);
    } catch (err) {
      setErrors(chainLengthFormErrorsFromApi(err));
    } finally {
      setSaving(false);
    }
  };

  return (
    <Card className="mb-8">
      <form onSubmit={handleSubmit} noValidate>
        <h2 className="mb-4 font-comfortaa text-xl font-light text-storm">
          {isEdit ? 'Modifier la longueur' : 'Ajouter une longueur'}
        </h2>

        {errors.form && <Alert className="mb-4">{errors.form}</Alert>}

        <div className="grid gap-4 sm:grid-cols-2">
          <TextField
            label="Longueur (cm)"
            type="number"
            inputMode="numeric"
            min={MIN_LENGTH_CM}
            max={MAX_LENGTH_CM}
            step={1}
            value={values.lengthCm}
            onChange={setField('lengthCm')}
            error={errors.fields.lengthCm}
            required
          />
          <TextField
            label="Libellé"
            maxLength={LABEL_MAX_LENGTH}
            value={values.label}
            onChange={setField('label')}
            error={errors.fields.label}
            placeholder="Princesse"
            required
          />
        </div>

        <div className="mt-5 flex gap-3">
          <Button type="submit" loading={saving}>
            {isEdit ? 'Enregistrer' : 'Ajouter'}
          </Button>
          {isEdit && (
            <Button type="button" variant="ghost" disabled={saving} onClick={onCancel}>
              Annuler
            </Button>
          )}
        </div>
      </form>
    </Card>
  );
}
