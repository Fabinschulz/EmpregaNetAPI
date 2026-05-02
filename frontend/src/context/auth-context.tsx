"use client";

import React, { createContext, useCallback, useContext, useMemo, useState } from "react";
import type { UserLoggedDto } from "@/services";
import { clearSessionClient, decodeRolesFromJwt, normalizeBearer, saveSessionClient } from "@/features/auth/session";

type AuthState = {
  token: string | null;
  roles: string[];
};

type AuthContextValue = AuthState & {
  setLoggedUser: (logged: UserLoggedDto) => void;
  logout: () => void;
};

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [token, setToken] = useState<string | null>(null);
  const [roles, setRoles] = useState<string[]>([]);

  const setLoggedUser = useCallback((logged: UserLoggedDto) => {
    const normalized = normalizeBearer(logged.accessToken);
    saveSessionClient({ token: normalized, refreshToken: logged.refreshToken });
    setToken(normalized);
    setRoles(logged.userToken.roles?.length ? logged.userToken.roles : decodeRolesFromJwt(normalized));
  }, []);

  const logout = useCallback(() => {
    clearSessionClient();
    setToken(null);
    setRoles([]);
  }, []);

  const value = useMemo<AuthContextValue>(
    () => ({
      token,
      roles,
      setLoggedUser,
      logout,
    }),
    [token, roles, setLoggedUser, logout]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth deve ser usado dentro de AuthProvider");
  return ctx;
}

