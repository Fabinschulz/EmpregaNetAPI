# syntax=docker/dockerfile:1

# Imagem do frontend para ECS Fargate. Não substitui o deploy atual (Vercel): é só o artefato que a Fase de
# infraestrutura vai publicar no ECR.
#
# Variáveis de BUILD (embutidas no bundle do navegador, públicas por definição):
#   NEXT_PUBLIC_API_BASE_URL         vazio = o navegador chama /api no mesmo domínio (o ALB roteia /api/* para a API)
#   NEXT_PUBLIC_GOOGLE_CLIENT_ID     opcional
#   API_INTERNAL_BASE_URL            opcional. O Next grava os rewrites de /api no build, então só vale se a imagem
#                                    for construída com ele; com o ALB roteando /api/*, deixe vazio
# Variáveis de EXECUÇÃO (lidas a cada requisição no servidor, nunca vão para o navegador):
#   API_INTERNAL_BASE_URL            URL interna da API usada pelo SSR (ex.: http://api.interno:8080)
#   INTERNAL_API_SECRET              segredo que identifica o SSR para o rate limit da API (vem do Secrets Manager)

ARG NODE_VERSION=22

FROM node:${NODE_VERSION}-alpine AS base
RUN corepack enable
WORKDIR /app

# Dependências: camada separada para o cache do Docker só invalidar quando o lockfile mudar.
FROM base AS deps
COPY package.json pnpm-lock.yaml .npmrc ./
RUN --mount=type=cache,id=pnpm,target=/root/.local/share/pnpm/store \
    pnpm install --frozen-lockfile

FROM base AS build
ARG NEXT_PUBLIC_API_BASE_URL=""
ARG NEXT_PUBLIC_GOOGLE_CLIENT_ID=""
ARG API_INTERNAL_BASE_URL=""
ENV NEXT_PUBLIC_API_BASE_URL=${NEXT_PUBLIC_API_BASE_URL} \
    NEXT_PUBLIC_GOOGLE_CLIENT_ID=${NEXT_PUBLIC_GOOGLE_CLIENT_ID} \
    API_INTERNAL_BASE_URL=${API_INTERNAL_BASE_URL} \
    NEXT_TELEMETRY_DISABLED=1
COPY --from=deps /app/node_modules ./node_modules
COPY . .
# O standalone copia public/ se existir; garante a pasta para o COPY da imagem final não falhar.
RUN mkdir -p public && pnpm build

FROM node:${NODE_VERSION}-alpine AS runtime
ENV NODE_ENV=production \
    NEXT_TELEMETRY_DISABLED=1 \
    HOSTNAME=0.0.0.0 \
    PORT=3000 \
    TZ=America/Sao_Paulo
RUN apk add --no-cache tzdata
WORKDIR /app

COPY --from=build --chown=node:node /app/.next/standalone ./
COPY --from=build --chown=node:node /app/.next/static ./.next/static
COPY --from=build --chown=node:node /app/public ./public

USER node
EXPOSE 3000
HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
    CMD wget -q --spider http://127.0.0.1:3000/healthz || exit 1

CMD ["node", "server.js"]
