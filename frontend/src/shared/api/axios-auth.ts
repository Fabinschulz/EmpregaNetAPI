import { refreshToken, type UserLoggedDto } from '@/shared';
import type { AxiosError, AxiosInstance, InternalAxiosRequestConfig } from 'axios';

type AuthHandlers = {
  onSessionRefreshed: (logged: UserLoggedDto) => void;
  onLogout: () => void;
};

let handlers: AuthHandlers | null = null;
let refreshPromise: Promise<UserLoggedDto | null> | null = null;

export const registerAxiosAuthHandlers = (next: AuthHandlers) => (handlers = next);

/** Tenta atualizar a sessão usando o refresh token do cookie httpOnly (enviado por `withCredentials`). */
async function tryRefreshSession(): Promise<UserLoggedDto | null> {
  if (!refreshPromise) {
    refreshPromise = refreshToken()
      .then((logged) => {
        handlers?.onSessionRefreshed(logged);
        return logged;
      })
      .catch(() => null)
      .finally(() => {
        refreshPromise = null;
      });
  }

  return refreshPromise;
}

/** Endpoints de auth não devem disparar o fluxo de refresh (evita recursão em 401). */
function isAuthEndpoint(url: string | undefined): boolean {
  if (!url) return false;
  return url.includes('/users/refresh-token') || url.includes('/users/logout');
}

/** Configura o interceptor de autenticação do Axios,
 * que lida com erros 401 e tenta atualizar a sessão automaticamente.
 * Se a atualização falhar, limpa a sessão e chama o handler de logout.
 */
export function attachAxiosAuthInterceptor(instance: AxiosInstance) {
  instance.defaults.withCredentials = true;

  instance.interceptors.response.use(
    (response) => response,
    async (error: AxiosError) => {
      const config = error.config as InternalAxiosRequestConfig & { _retry?: boolean };
      if (!config || error.response?.status !== 401 || config._retry || isAuthEndpoint(config.url)) {
        return Promise.reject(error);
      }

      config._retry = true;
      const refreshed = await tryRefreshSession();
      if (!refreshed) {
        handlers?.onLogout();
        return Promise.reject(error);
      }

      // A requisição repetida reautentica pelo cookie httpOnly já renovado (withCredentials).
      return instance.request(config);
    }
  );
}
