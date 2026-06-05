import type { UserLoggedDto } from '@/services';
import { clearSessionClient, readRefreshTokenFromBrowser, refreshToken, saveSessionClient } from '@/services';
import type { AxiosError, AxiosInstance, InternalAxiosRequestConfig } from 'axios';

type AuthHandlers = {
  onSessionRefreshed: (logged: UserLoggedDto) => void;
  onLogout: () => void;
};

let handlers: AuthHandlers | null = null;
let refreshPromise: Promise<UserLoggedDto | null> | null = null;

export const registerAxiosAuthHandlers = (next: AuthHandlers) => (handlers = next);

/** Tenta atualizar a sessão usando o token de refresh. */
async function tryRefreshSession(): Promise<UserLoggedDto | null> {
  const refresh = readRefreshTokenFromBrowser();
  if (!refresh) return null;

  if (!refreshPromise) {
    refreshPromise = refreshToken({ refreshToken: refresh })
      .then((logged) => {
        saveSessionClient({ token: logged.accessToken, refreshToken: logged.refreshToken });
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
      if (!config || error.response?.status !== 401 || config._retry) {
        return Promise.reject(error);
      }

      config._retry = true;
      const refreshed = await tryRefreshSession();
      if (!refreshed) {
        clearSessionClient();
        handlers?.onLogout();
        return Promise.reject(error);
      }

      config.headers.Authorization = refreshed.accessToken.startsWith('Bearer ')
        ? refreshed.accessToken
        : `Bearer ${refreshed.accessToken}`;

      return instance.request(config);
    }
  );
}
