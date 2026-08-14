import { getPublicEnv } from '@/utils';
import axios, { type AxiosInstance, type AxiosRequestConfig } from 'axios';
import * as Qs from 'qs';

export const buildAxiosParams = (): AxiosRequestConfig => {
  const { NEXT_PUBLIC_API_BASE_URL } = getPublicEnv();

  return {
    baseURL: NEXT_PUBLIC_API_BASE_URL,
    responseType: 'json' as const,
    withCredentials: true,
    paramsSerializer: (params: unknown) => Qs.stringify(params as Record<string, unknown>, { arrayFormat: 'repeat' })
  };
};

let bareInstance: AxiosInstance | null = null;

const getBareInstance = () => {
  if (!bareInstance) {
    bareInstance = axios.create(buildAxiosParams());
  }

  return bareInstance;
};

/**
 * Cliente sem o interceptor de auth. Destinado aos endpoints do próprio ciclo de sessão
 * (`/auth/refresh-token`, `/auth/logout`), que nunca podem disparar o refresh automático
 * OBS: ver `isAuthEndpoint` em `axios-auth.ts` para mais detalhes e use o `axiosAuth` para chamadas de API normais, que precisam do interceptor de auth.
 */
export const axiosBase = {
  get: <T>(url: string, config?: AxiosRequestConfig) => getBareInstance().get<T>(url, config),
  post: <T>(url: string, body: unknown, config?: AxiosRequestConfig) => getBareInstance().post<T>(url, body, config)
};
