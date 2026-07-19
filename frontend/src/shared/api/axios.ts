import { getPublicEnv } from '@/utils';
import axios, { type AxiosInstance, type AxiosRequestConfig } from 'axios';
import * as Qs from 'qs';
import { attachAxiosAuthInterceptor } from './axios-auth';

let instance: AxiosInstance | null = null;

const createAxiosInstance = async () => {
  const { NEXT_PUBLIC_API_BASE_URL } = getPublicEnv();

  const axiosParams: AxiosRequestConfig = {
    baseURL: NEXT_PUBLIC_API_BASE_URL,
    responseType: 'json' as const,
    withCredentials: true,
    paramsSerializer: (params: unknown) => Qs.stringify(params as Record<string, unknown>, { arrayFormat: 'repeat' })
  };

  const created = axios.create(axiosParams);
  attachAxiosAuthInterceptor(created);
  return created;
};

const getAxiosInstance = async () => {
  if (!instance) {
    instance = await createAxiosInstance();
  }

  return instance;
};

export const axiosApi = {
  get: <T>(url: string, config?: AxiosRequestConfig) => {
    return getAxiosInstance().then((inst) => inst.get<T>(url, config));
  },
  post: <T>(url: string, body: unknown, config?: AxiosRequestConfig) => {
    return getAxiosInstance().then((inst) => inst.post<T>(url, body, config));
  },
  patch: <T>(url: string, body: unknown, config?: AxiosRequestConfig) => {
    return getAxiosInstance().then((inst) => inst.patch<T>(url, body, config));
  },
  put: <T>(url: string, body: unknown, config?: AxiosRequestConfig) => {
    return getAxiosInstance().then((inst) => inst.put<T>(url, body, config));
  },
  delete: <T>(url: string, config?: AxiosRequestConfig) => {
    return getAxiosInstance().then((inst) => inst.delete<T>(url, config));
  }
};

/**
 * Config para chamadas autenticadas. A autenticação é feita pelo cookie httpOnly
 * (enviado automaticamente por `withCredentials`) -> não há header Bearer nem token em JS.
 */
export function createAxiosConfig<T>(params?: T) {
  return {
    params,
    withCredentials: true
  } satisfies AxiosRequestConfig;
}
