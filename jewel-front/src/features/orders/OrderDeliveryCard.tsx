import type { OrderDetail } from '../../api/orders';
import { Card } from '../../components/ui/Card';
import { DetailRow } from '../../components/ui/DetailRow';

interface OrderDeliveryCardProps {
  order: OrderDetail;
}

/** Destinataire et adresse de livraison copiés à la commande. */
export function OrderDeliveryCard({ order }: OrderDeliveryCardProps) {
  const { recipient, address } = order;

  return (
    <Card className="mb-6">
      <h2 className="mb-4 font-comfortaa text-xl font-light text-storm">Livraison</h2>
      <DetailRow label="Destinataire">
        {recipient.surname} {recipient.name}
      </DetailRow>
      <DetailRow label="Téléphone">{recipient.phone}</DetailRow>
      <DetailRow label="Adresse">
        {address.line1}
        {address.line2 && (
          <>
            <br />
            {address.line2}
          </>
        )}
        <br />
        {address.postalCode} {address.city}
      </DetailRow>
    </Card>
  );
}
