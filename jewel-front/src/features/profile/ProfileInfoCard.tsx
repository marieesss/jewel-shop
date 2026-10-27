import type { ProfileDto } from '../../api/users';
import { Card } from '../../components/ui/Card';
import { DetailRow } from '../../components/ui/DetailRow';
import { formatDateOnly } from '../../lib/format';

interface ProfileInfoCardProps {
  profile: ProfileDto;
}

/** Bloc « Mes informations » du profil, en lecture seule. */
export function ProfileInfoCard({ profile }: ProfileInfoCardProps) {
  return (
    <Card className="mb-8">
      <h2 className="mb-4 font-comfortaa text-xl font-light text-storm">Mes informations</h2>
      <DetailRow label="Nom">{profile.name}</DetailRow>
      <DetailRow label="Prénom">{profile.surname}</DetailRow>
      <DetailRow label="Email">{profile.email}</DetailRow>
      <DetailRow label="Date de naissance">{formatDateOnly(profile.birthday)}</DetailRow>
    </Card>
  );
}
