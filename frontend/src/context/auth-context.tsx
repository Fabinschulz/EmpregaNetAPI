'use client';

import React, { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import type { UserLoggedDto } from '@/services';
import {
  clearSessionClient,
  decodeRolesFromJwt,
  normalizeBearer,
  readSessionFromBrowser,
  saveSessionClient
} from '@/features/auth/session';

type AuthState = {
  token: string | null;
  roles: string[];
  username: string | null;
  email: string | null;
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

  useEffect(() => {
    const session = readSessionFromBrowser();
    if (!session) return;
    const normalized = normalizeBearer(session.token);
    setToken(normalized);
    setRoles(session.roles.length ? session.roles : decodeRolesFromJwt(normalized));
    setUsername(session.username);
    setEmail(session.email);
  }, []);

  const setLoggedUser = useCallback((logged: UserLoggedDto) => {
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

  const value = useMemo<AuthContextValue>(
    () => ({
      token,
      roles,
      username,
      email,
      setLoggedUser,
      logout
    }),
    [token, roles, username, email, setLoggedUser, logout]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth deve ser usado dentro de AuthProvider');
  return ctx;
}
