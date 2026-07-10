'use client';

import type { UserLoggedDto } from '@/services';
import { logout as apiLogout } from '@/services';
import { registerAxiosAuthHandlers } from '@/services/axios/axios-auth';
import React, { createContext, useCallback, useContext, useEffect, useMemo, useSyncExternalStore } from 'react';
import {
    clearSessionMetadata,
    getSessionMetadataSnapshot,
    saveSessionMetadata,
    subscribeSessionMetadata
} from 'src/services/users/session';

type AuthState = {
  isAuthenticated: boolean;
  roles: string[];
  username: string | null;
  email: string | null;
  hydrated: boolean;
};

type AuthContextValue = AuthState & {
  setLoggedUser: (logged: UserLoggedDto) => void;
  logout: () => Promise<void>;
};

const AuthContext = createContext<AuthContextValue | null>(null);

const EMPTY_ROLES: string[] = [];

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const meta = useSyncExternalStore(subscribeSessionMetadata, getSessionMetadataSnapshot, () => null);

  // OBS: No SSR/primeira pintura o snapshot do servidor é null; após a hidratação o
  // snapshot real do cliente é aplicado.
  const hydrated = useSyncExternalStore(
    () => () => {},
    () => true,
    () => false
  );

  const setLoggedUser = useCallback((logged: UserLoggedDto) => {
    saveSessionMetadata({
      roles: logged.userToken.roles ?? [],
      username: logged.userToken.username ?? null,
      email: logged.userToken.email ?? null
    });
  }, []);

  /** Limpa apenas o estado local da sessão (sem chamar o servidor). */
  const clearLocalSession = useCallback(() => {
    clearSessionMetadata();
  }, []);

  /** Logout iniciado pelo usuário: revoga o refresh token e limpa os cookies no servidor + estado local. */
  const logout = useCallback(async () => {
    try {
      await apiLogout();
    } catch (err) {
      // Ignora falhas de rede/servidor: a sessão local é sempre limpa a seguir.
      console.error('Falha ao chamar /logout', err);
    }
    clearLocalSession();
  }, [clearLocalSession]);

  useEffect(() => {
    registerAxiosAuthHandlers({
      onSessionRefreshed: setLoggedUser,
      // Sessão já inválida (refresh falhou): basta limpar o estado local, sem novo request.
      onLogout: clearLocalSession
    });
  }, [setLoggedUser, clearLocalSession]);

  const value = useMemo<AuthContextValue>(
    () => ({
      isAuthenticated: meta !== null,
      roles: meta?.roles ?? EMPTY_ROLES,
      username: meta?.username ?? null,
      email: meta?.email ?? null,
      hydrated,
      setLoggedUser,
      logout
    }),
    [meta, hydrated, setLoggedUser, logout]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth deve ser usado dentro de AuthProvider');
  return ctx;
}
