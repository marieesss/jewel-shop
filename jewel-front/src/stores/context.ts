import { createContext, useContext } from 'react';
import { AuthStore } from './AuthStore';

export class RootStore {
  auth = new AuthStore();
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
