'use client';

import type { UserLoggedDto } from '@/services';
import { refreshToken } from '@/services';
import { registerAxiosAuthHandlers } from '@/services/axios/axios-auth';
import React, { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import {
    clearSessionClient,
    decodeRolesFromJwt,
    normalizeBearer,
    readRefreshTokenFromBrowser,
    readSessionFromBrowser,
    saveSessionClient
} from 'src/services/users/session';

type AuthState = {
  token: string | null;
  roles: string[];
  username: string | null;
  email: string | null;
  hydrated: boolean;
};

type AuthContextValue = AuthState & {
  setLoggedUser: (logged: UserLoggedDto) => void;
  logout: () => void;
};

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [token, setToken] = useState<string | null>(null);
  const [roles, setRoles] = useState<string[]>([]);
  const [username, setUsername] = useState<string | null>(null);
  const [email, setEmail] = useState<string | null>(null);
  const [hydrated, setHydrated] = useState(false);

  const applyLoggedUser = useCallback((logged: UserLoggedDto) => {
    const normalized = normalizeBearer(logged.accessToken);
    saveSessionClient({ token: normalized, refreshToken: logged.refreshToken });
    setToken(normalized);
    setRoles(logged.userToken.roles?.length ? logged.userToken.roles : decodeRolesFromJwt(normalized));
    setUsername(logged.userToken.username ?? null);
    setEmail(logged.userToken.email ?? null);
  }, []);

  const logout = useCallback(() => {
    clearSessionClient();
    setToken(null);
    setRoles([]);
    setUsername(null);
    setEmail(null);
  }, []);

  useEffect(() => {
    registerAxiosAuthHandlers({
      onSessionRefreshed: applyLoggedUser,
      onLogout: logout
    });
  }, [applyLoggedUser, logout]);

  useEffect(() => {
    let cancelled = false;

    const hydrate = async () => {
      const session = readSessionFromBrowser();
      if (session?.token) {
        if (!cancelled) {
          setToken(session.token);
          setRoles(session.roles);
          setUsername(session.username);
          setEmail(session.email);
        }
        setHydrated(true);
        return;
      }

      const refresh = readRefreshTokenFromBrowser();
      if (refresh) {
        try {
          const logged = await refreshToken({ refreshToken: refresh });
          if (!cancelled) applyLoggedUser(logged);
        } catch {
          if (!cancelled) logout();
        }
      }

      if (!cancelled) setHydrated(true);
    };

    void hydrate();
    return () => {
      cancelled = true;
    };
  }, [applyLoggedUser, logout]);

  const setLoggedUser = useCallback(
    (logged: UserLoggedDto) => {
      applyLoggedUser(logged);
    },
    [applyLoggedUser]
  );

  const value = useMemo<AuthContextValue>(
    () => ({
      token,
      roles,
      username,
      email,
      hydrated,
      setLoggedUser,
      logout
    }),
    [token, roles, username, email, hydrated, setLoggedUser, logout]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth deve ser usado dentro de AuthProvider');
  return ctx;
}
