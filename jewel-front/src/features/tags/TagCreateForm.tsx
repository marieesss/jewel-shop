import { useState, type FormEvent } from 'react';
import { createTag, type TagCategory } from '../../api/tags';
import { Alert } from '../../components/ui/Alert';
import { Button } from '../../components/ui/Button';
import { Card } from '../../components/ui/Card';
import { Select } from '../../components/ui/Select';
import { TextField } from '../../components/ui/TextField';
import {
  CREATE_SUCCESS_MESSAGE,
  DEFAULT_TAG_CATEGORY,
  TAG_CATEGORY_OPTIONS,
  TAG_NAME_MAX_LENGTH,
  noTagFormErrors,
  tagFormErrorsFromApi,
  validateTagName,
  type TagFormErrors,
} from '../../lib/tagAdmin';

interface TagCreateFormProps {
  /** Appelé au début de chaque envoi, pour effacer les messages de l'action précédente. */
  onSubmitStart?: () => void;
  onCreated: (successMessage: string) => void;
}

/** Formulaire de création d'un tag : nom et catégorie (admin). */
export function TagCreateForm({ onSubmitStart, onCreated }: TagCreateFormProps) {
  const [name, setName] = useState('');
  const [category, setCategory] = useState<TagCategory>(DEFAULT_TAG_CATEGORY);
  const [errors, setErrors] = useState<TagFormErrors>(noTagFormErrors);
  const [saving, setSaving] = useState(false);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    onSubmitStart?.();

    const nameError = validateTagName(name);
    setErrors({ fields: nameError ? { name: nameError } : {}, form: null });
    if (nameError) return;

    setSaving(true);
    try {
      await createTag({ name: name.trim(), category });
      setName('');
      onCreated(CREATE_SUCCESS_MESSAGE);
    } catch (err) {
      setErrors(tagFormErrorsFromApi(err));
    } finally {
      setSaving(false);
    }
  };

  return (
    <Card className="mb-8">
      <form onSubmit={handleSubmit} noValidate>
        <h2 className="mb-4 font-comfortaa text-xl font-light text-storm">Ajouter un tag</h2>

        {errors.form && <Alert className="mb-4">{errors.form}</Alert>}

        <div className="grid gap-4 sm:grid-cols-2">
          <TextField
            label="Nom"
            maxLength={TAG_NAME_MAX_LENGTH}
            value={name}
            onChange={setName}
            error={errors.fields.name}
            placeholder="Mer"
            required
          />
          <Select
            label="Catégorie"
            value={category}
            onChange={(value) => setCategory(value as TagCategory)}
            options={TAG_CATEGORY_OPTIONS}
            error={errors.fields.category}
            required
          />
        </div>

        <div className="mt-5">
          <Button type="submit" loading={saving}>
            Ajouter
          </Button>
        </div>
      </form>
    </Card>
  );
}
