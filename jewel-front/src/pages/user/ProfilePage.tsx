import { getMyProfile } from '../../api/users';
import { Alert } from '../../components/ui/Alert';
import { PageHeader } from '../../components/ui/PageHeader';
import { Spinner } from '../../components/ui/Spinner';
import { AddressForm } from '../../features/profile/AddressForm';
import { ProfileInfoCard } from '../../features/profile/ProfileInfoCard';
import { useAsync } from '../../hooks/useAsync';

/** Profil de l'utilisateur connecté : informations du compte et adresse de livraison. */
export function ProfilePage() {
  const { data: profile, loading, error } = useAsync(getMyProfile, []);

  return (
    <div className="mx-auto w-full max-w-[720px]">
      <PageHeader title="Mon profil" subtitle="Vos informations et votre adresse de livraison" />

      {loading && <Spinner label="Chargement…" />}
      {!loading && error && <Alert>{error}</Alert>}

      {!loading && !error && profile && (
        <>
          <ProfileInfoCard profile={profile} />
          <AddressForm profile={profile} />
        </>
      )}
    </div>
  );
}
