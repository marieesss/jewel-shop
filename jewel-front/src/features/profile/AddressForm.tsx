import { useState, type FormEvent } from 'react';
import { updateMyAddress, type ProfileDto } from '../../api/users';
import { Alert } from '../../components/ui/Alert';
import { Button } from '../../components/ui/Button';
import { Card } from '../../components/ui/Card';
import { TextField } from '../../components/ui/TextField';
import {
  ADDRESS_LINE_MAX_LENGTH,
  ADDRESS_SAVED_MESSAGE,
  CITY_MAX_LENGTH,
  DELIVERY_COUNTRY,
  addressFormErrorsFromApi,
  addressFormFrom,
  noAddressFormErrors,
  toAddressInput,
  validateAddressForm,
  type AddressField,
  type AddressFormErrors,
  type AddressFormValues,
} from '../../lib/addressForm';

interface AddressFormProps {
  /** Profil chargé, qui pré-remplit l'adresse et le téléphone déjà enregistrés. */
  profile: ProfileDto;
}

/** Bloc « Adresse de livraison » du profil : adresse en France et téléphone. */
export function AddressForm({ profile }: AddressFormProps) {
  const [values, setValues] = useState<AddressFormValues>(() => addressFormFrom(profile));
  const [errors, setErrors] = useState<AddressFormErrors>(noAddressFormErrors);
  const [saved, setSaved] = useState(false);
  const [saving, setSaving] = useState(false);

  const setField = (field: AddressField) => (value: string) =>
    setValues((current) => ({ ...current, [field]: value }));

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setSaved(false);

    const fieldErrors = validateAddressForm(values);
    setErrors({ fields: fieldErrors, form: null });
    if (Object.keys(fieldErrors).length > 0) return;

    setSaving(true);
    try {
      const updated = await updateMyAddress(toAddressInput(values));
      setValues(addressFormFrom(updated));
      setSaved(true);
    } catch (err) {
      setErrors(addressFormErrorsFromApi(err));
    } finally {
      setSaving(false);
    }
  };

  return (
    <Card>
      <form onSubmit={handleSubmit} noValidate>
        <h2 className="mb-4 font-comfortaa text-xl font-light text-storm">Adresse de livraison</h2>

        {errors.form && <Alert className="mb-4">{errors.form}</Alert>}
        {saved && (
          <Alert variant="success" className="mb-4">
            {ADDRESS_SAVED_MESSAGE}
          </Alert>
        )}

        <div className="grid gap-4 sm:grid-cols-2">
          <TextField
            label="Adresse"
            className="sm:col-span-2"
            autoComplete="address-line1"
            maxLength={ADDRESS_LINE_MAX_LENGTH}
            value={values.line1}
            onChange={setField('line1')}
            error={errors.fields.line1}
            required
          />
          <TextField
            label="Complément d'adresse"
            autoComplete="address-line2"
            maxLength={ADDRESS_LINE_MAX_LENGTH}
            value={values.line2}
            onChange={setField('line2')}
            error={errors.fields.line2}
          />
          <TextField
            label="Code postal"
            inputMode="numeric"
            autoComplete="postal-code"
            maxLength={5}
            value={values.postalCode}
            onChange={setField('postalCode')}
            error={errors.fields.postalCode}
            required
          />
          <TextField
            label="Ville"
            autoComplete="address-level2"
            maxLength={CITY_MAX_LENGTH}
            value={values.city}
            onChange={setField('city')}
            error={errors.fields.city}
            required
          />
          <TextField label="Pays" value={DELIVERY_COUNTRY} onChange={() => {}} readOnly />
          <TextField
            label="Téléphone"
            type="tel"
            autoComplete="tel"
            value={values.phone}
            onChange={setField('phone')}
            error={errors.fields.phone}
            placeholder="0612345678"
            required
          />
        </div>

        <div className="mt-5">
          <Button type="submit" loading={saving}>
            Enregistrer
          </Button>
        </div>
      </form>
    </Card>
  );
}
