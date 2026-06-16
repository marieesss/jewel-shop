import { useState } from 'react';
import { useProductAdminStore } from '../../stores/context';
import { PageHeader } from '../../components/ui/PageHeader';
import { Tabs, type TabItem } from '../../components/ui/Tabs';
import { GemIcon, ChainIcon } from '../../components/icons-admin';
import { CharmForm } from '../../features/products/CharmForm';
import { ChainForm } from '../../features/products/ChainForm';
import type { ProductKind } from '../../stores/ProductAdminStore';

const TABS: TabItem<ProductKind>[] = [
  { value: 'charm', label: 'Breloque', icon: <GemIcon width={16} height={16} /> },
  { value: 'chain', label: 'Chaîne', icon: <ChainIcon width={16} height={16} /> },
];

/** Page admin de création de produit, avec un onglet par type (breloque / chaîne). */
export function ProductCreatePage() {
  const store = useProductAdminStore();
  const [tab, setTab] = useState<ProductKind>('charm');

  const changeTab = (next: ProductKind) => {
    setTab(next);
    store.reset(); // évite de traîner un message d'un onglet à l'autre
  };

  return (
    <div className="mx-auto w-full max-w-[560px]">
      <PageHeader
        title="Créer un produit"
        subtitle="Ajoutez une nouvelle breloque ou une nouvelle chaîne au catalogue"
      />

      <Tabs items={TABS} value={tab} onChange={changeTab} className="mb-6" />

      {tab === 'charm' ? <CharmForm /> : <ChainForm />}
    </div>
  );
}
