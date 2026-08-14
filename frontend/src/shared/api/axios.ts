import axios, { type AxiosInstance, type AxiosRequestConfig } from 'axios';
import { attachAxiosAuthInterceptor } from './axios-auth';
import { buildAxiosParams } from './axios-base';

let instance: AxiosInstance | null = null;

const createAxiosInstance = async () => {
  const created = axios.create(buildAxiosParams());
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
 * obs: o token JWT é armazenado no cookie httpOnly, que não é acessível via JS, mas é enviado automaticamente pelo navegador em requisições para o mesmo domínio.
 */
export function createAxiosConfig<T>(params?: T) {
  return {
    params,
    withCredentials: true
  } satisfies AxiosRequestConfig;
}
