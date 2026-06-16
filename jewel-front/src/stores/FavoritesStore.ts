import { makeAutoObservable, runInAction } from 'mobx';
import {
  getMyFavorites,
  addFavorite,
  removeFavorite,
  type FavoriteDto,
} from '../api/favorites';
import { extractApiError } from '../api/errors';
import type { ProductKind } from './ProductAdminStore';

const favKey = (kind: ProductKind, productId: number) => `${kind}:${productId}`;

/** Favoris de l'utilisateur courant + index pour un toggle rapide depuis listes/détails. */
export class FavoritesStore {
  favorites: FavoriteDto[] = [];
  loaded = false;
  loading = false;
  error: string | null = null;

  /** Clés (`kind:id`) en cours de bascule, pour désactiver les boutons concernés. */
  pending = new Set<string>();

  constructor() {
    makeAutoObservable(this);
  }

  /** Index produit → id du favori (pour suppression). */
  private get index(): Map<string, number> {
    const map = new Map<string, number>();
    for (const f of this.favorites) {
      if (f.charmId != null) map.set(favKey('charm', f.charmId), f.id);
      if (f.chainId != null) map.set(favKey('chain', f.chainId), f.id);
    }
    return map;
  }

  isFavorite(kind: ProductKind, productId: number): boolean {
    return this.index.has(favKey(kind, productId));
  }

  isPending(kind: ProductKind, productId: number): boolean {
    return this.pending.has(favKey(kind, productId));
  }

  /** Charge la liste des favoris (une seule fois sauf `force`). */
  async load(force = false): Promise<void> {
    if (this.loaded && !force) return;
    this.loading = true;
    this.error = null;
    try {
      const result = await getMyFavorites({ page: 1, pageSize: 100 });
      runInAction(() => {
        this.favorites = result.items;
        this.loaded = true;
      });
    } catch (err) {
      runInAction(() => {
        this.error = extractApiError(err);
      });
    } finally {
      runInAction(() => {
        this.loading = false;
      });
    }
  }

  /** Ajoute ou retire le produit des favoris selon son état courant. */
  async toggle(kind: ProductKind, productId: number): Promise<void> {
    const key = favKey(kind, productId);
    if (this.pending.has(key)) return;

    this.pending.add(key);
    this.error = null;
    try {
      const existingId = this.index.get(key);
      if (existingId != null) {
        await removeFavorite(existingId);
        runInAction(() => {
          this.favorites = this.favorites.filter((f) => f.id !== existingId);
        });
      } else {
        const created = await addFavorite(kind, productId);
        runInAction(() => {
          this.favorites = [created, ...this.favorites];
        });
      }
    } catch (err) {
      runInAction(() => {
        this.error = extractApiError(err);
      });
    } finally {
      runInAction(() => {
        this.pending.delete(key);
      });
    }
  }

  reset(): void {
    this.favorites = [];
    this.loaded = false;
    this.error = null;
    this.pending.clear();
  }
}
