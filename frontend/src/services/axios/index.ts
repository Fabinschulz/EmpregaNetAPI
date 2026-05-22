import { getPublicEnv } from '@/utils';
import axios, { type AxiosInstance, type AxiosRequestConfig } from 'axios';
import * as Qs from 'qs';

let instance: AxiosInstance | null = null;

const createAxiosInstance = async () => {
  const { NEXT_PUBLIC_API_BASE_URL } = getPublicEnv();

  const axiosParams: AxiosRequestConfig = {
    baseURL: NEXT_PUBLIC_API_BASE_URL,
    responseType: 'json' as const,
    paramsSerializer: (params: unknown) => Qs.stringify(params as Record<string, unknown>, { arrayFormat: 'repeat' })
  };

  return axios.create(axiosParams);
};

const getAxiosInstance = async () => {
  if (!instance) {
    instance = await createAxiosInstance();
  }

  return instance;
};

export const axiosApi = {
  get: <T>(url: string, config?: AxiosRequestConfig) => {
    return getAxiosInstance().then((instance) => instance.get<T>(url, config));
  },
  post: <T>(url: string, body: unknown, config?: AxiosRequestConfig) => {
    return getAxiosInstance().then((instance) => instance.post<T>(url, body, config));
  },
  patch: <T>(url: string, body: unknown, config?: AxiosRequestConfig) => {
    return getAxiosInstance().then((instance) => instance.patch<T>(url, body, config));
  },
  put: <T>(url: string, body: unknown, config?: AxiosRequestConfig) => {
    return getAxiosInstance().then((instance) => instance.put<T>(url, body, config));
  },
  delete: <T>(url: string, config?: AxiosRequestConfig) => {
    return getAxiosInstance().then((instance) => instance.delete<T>(url, config));
  }
};

function normalizeAuthorizationHeader(token: string) {
  return token.startsWith('Bearer ') ? token : `Bearer ${token}`;
}

export function createAxiosConfig<T>(token: string, params?: T) {
  return {
    headers: {
      Authorization: normalizeAuthorizationHeader(token)
    },
    params
  } satisfies AxiosRequestConfig;
}
