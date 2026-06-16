import { makeAutoObservable, runInAction } from 'mobx';
import {
  createCharm,
  createChain,
  updateCharm,
  updateChain,
  uploadCharmImage,
  uploadChainImage,
  type CreateCharmPayload,
  type CreateChainPayload,
} from '../api/products';
import { extractApiError } from '../api/errors';

export type ProductKind = 'charm' | 'chain';
export type ProductAction = 'create' | 'update';

export interface ProductResult {
  kind: ProductKind;
  name: string;
  action: ProductAction;
}

/** Gère la création et la modification de produits (breloques / chaînes) côté admin. */
export class ProductAdminStore {
  loading = false;
  error: string | null = null;
  lastResult: ProductResult | null = null;

  constructor() {
    makeAutoObservable(this);
  }

  reset(): void {
    this.error = null;
    this.lastResult = null;
  }

  async createCharm(payload: CreateCharmPayload, image?: File | null): Promise<boolean> {
    return this.run('charm', 'create', payload.name, async () => {
      const charm = await createCharm(payload);
      if (image) await uploadCharmImage(charm.id, image);
    });
  }

  async createChain(payload: CreateChainPayload, image?: File | null): Promise<boolean> {
    return this.run('chain', 'create', payload.name, async () => {
      const chain = await createChain(payload);
      if (image) await uploadChainImage(chain.id, image);
    });
  }

  async updateCharm(id: number, payload: CreateCharmPayload, image?: File | null): Promise<boolean> {
    return this.run('charm', 'update', payload.name, async () => {
      await updateCharm(id, payload);
      if (image) await uploadCharmImage(id, image);
    });
  }

  async updateChain(id: number, payload: CreateChainPayload, image?: File | null): Promise<boolean> {
    return this.run('chain', 'update', payload.name, async () => {
      await updateChain(id, payload);
      if (image) await uploadChainImage(id, image);
    });
  }

  private async run(
    kind: ProductKind,
    action: ProductAction,
    name: string,
    request: () => Promise<unknown>
  ): Promise<boolean> {
    this.loading = true;
    this.error = null;
    try {
      await request();
      runInAction(() => {
        this.lastResult = { kind, name, action };
      });
      return true;
    } catch (err) {
      runInAction(() => {
        this.error = extractApiError(err);
      });
      return false;
    } finally {
      runInAction(() => {
        this.loading = false;
      });
    }
  }
}
