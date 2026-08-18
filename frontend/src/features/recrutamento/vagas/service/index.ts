export * from './jobs-actions';
export * from './jobs-api';
export * from './jobs-keys';
export * from './jobs-queries';
export * from './jobs-request-schema';
export * from './jobs-response-schema';

// OBS: NÃO reexportar `./jobs-server` aqui: este barrel é consumido por Client Components
// (ex.: dashboard, listas), e `jobs-server` usa `'use cache'` + `next/cache`, que não podem
// entrar no bundle do cliente. Server Components importam por path direto:
// `@/features/recrutamento/vagas/service/jobs-server`.
