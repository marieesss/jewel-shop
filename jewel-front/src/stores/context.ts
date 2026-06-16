import { createContext, useContext } from 'react';
import { AuthStore } from './AuthStore';
import { ProductAdminStore } from './ProductAdminStore';

export class RootStore {
  auth = new AuthStore();
  productAdmin = new ProductAdminStore();
}

export const rootStore = new RootStore();

const StoreContext = createContext<RootStore>(rootStore);

export const StoreProvider = StoreContext.Provider;

export function useStores(): RootStore {
  return useContext(StoreContext);
}

export function useAuthStore(): AuthStore {
  return useStores().auth;
}

export function useProductAdminStore(): ProductAdminStore {
  return useStores().productAdmin;
}
