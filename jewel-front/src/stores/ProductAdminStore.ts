import { makeAutoObservable, runInAction } from 'mobx';
import {
  createCharm,
  createChain,
  type CreateCharmPayload,
  type CreateChainPayload,
} from '../api/products';
import { extractApiError } from '../api/errors';

export type ProductKind = 'charm' | 'chain';

export interface LastCreated {
  kind: ProductKind;
  name: string;
}

/** Gère la création de produits (breloques / chaînes) côté admin. */
export class ProductAdminStore {
  loading = false;
  error: string | null = null;
  lastCreated: LastCreated | null = null;

  constructor() {
    makeAutoObservable(this);
  }

  reset(): void {
    this.error = null;
    this.lastCreated = null;
  }

  async createCharm(payload: CreateCharmPayload): Promise<boolean> {
    return this.run('charm', payload.name, () => createCharm(payload));
  }

  async createChain(payload: CreateChainPayload): Promise<boolean> {
    return this.run('chain', payload.name, () => createChain(payload));
  }

  private async run(
    kind: ProductKind,
    name: string,
    request: () => Promise<unknown>
  ): Promise<boolean> {
    this.loading = true;
    this.error = null;
    try {
      await request();
      runInAction(() => {
        this.lastCreated = { kind, name };
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
