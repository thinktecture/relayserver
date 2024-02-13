import { effect } from '@angular/core';
import {
  patchState,
  signalStore,
  withHooks,
  withMethods,
  withState,
} from '@ngrx/signals';

const STORAGE_KEY = 'api-auth';

interface ApiAuthState {
  headerName: string | undefined;
  key: string | undefined;
}

export const ApiAuthStore = signalStore(
  { providedIn: 'root' },
  withState({
    headerName: undefined,
    key: '',
  } as ApiAuthState),
  withMethods((state) => ({
    update(headerName: string, key: string): void {
      patchState(state, { headerName, key });
    },
  })),
  withHooks({
    onInit(state) {
      const persisted = localStorage.getItem(STORAGE_KEY) ?? '{}';
      patchState(state, JSON.parse(persisted));

      effect(() => {
        const persisted = JSON.stringify({
          headerName: state.headerName(),
          key: state.key(),
        });
        localStorage.setItem(STORAGE_KEY, persisted);
      });
    },
  }),
);
