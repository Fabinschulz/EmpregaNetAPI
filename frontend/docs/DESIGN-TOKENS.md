# EmpregaUAI — Design tokens

Definição principal: `src/app/globals.scss`.

## Shell (dark e light)

O menu lateral usa o **mesmo layout** nos dois temas: painel flutuante com inset, borda, `--sidebar-radius` e `--shadow-card`.

| Token                 | Uso                                               |
| --------------------- | ------------------------------------------------- |
| `--shell-panel-inset` | Margem do painel e alinhamento do topbar (`12px`) |
| `--sidebar-radius`    | Raio do painel (`20px`)                           |
| `--sidebar-bg`        | Fundo do painel                                   |
| `--content-bg`        | Fundo da área principal                           |
| `--topbar-bg`         | Fundo do header (transparente em desktop)         |

## Tema dark — modern panel

| Token        | Hex       | Uso                      |
| ------------ | --------- | ------------------------ |
| `--gray-950` | `#000000` | Fundo da aplicação       |
| `--gray-850` | `#121212` | Painéis (sidebar, cards) |
| `--gray-700` | `#2A2A2A` | Item de menu **ativo**   |
| `--text`     | `#E0E0E0` | Texto primário           |
| `--muted`    | `#A0A0A0` | Texto secundário         |

### Navegação (dark)

| Token                                      | Uso                                            |
| ------------------------------------------ | ---------------------------------------------- |
| `--nav-bg-active`                          | `#2A2A2A` — fundo do item selecionado (neutro) |
| `--nav-fg-active` / `--nav-fg-active-icon` | Texto e ícone brancos                          |
| Sem borda colorida nem glow no item ativo  |

## Tema light

| Token                                    | Uso                                                         |
| ---------------------------------------- | ----------------------------------------------------------- |
| `--nav-bg-active`                        | `--surface-active` (`#E2E2E6`) — equivalente neutro ao dark |
| Mesma semântica de tipografia que o dark |

## Acento do sistema (neutro)

Sem rosa/vermelho. Ações primárias e foco usam escala **preto/branco/cinza**:

| Tema  | `--brand` (primário) | `--brand-on` |
| ----- | -------------------- | ------------ |
| Dark  | `#FFFFFF`            | `#000000`    |
| Light | `#1C1C1E`            | `#FFFFFF`    |

## Controles

| Token                      | Dark               | Light            | Uso                                |
| -------------------------- | ------------------ | ---------------- | ---------------------------------- |
| `--control-bg`             | `#1A1A1A`          | `#FFFFFF`        | Fundo de inputs, selects, triggers |
| `--control-min-height`     | `44px`             | `44px`           | Altura mínima de todos os controls |
| `--control-padding-inline` | `14px`             | `14px`           | Padding horizontal                 |
| `--control-padding-block`  | `11px`             | `11px`           | Padding vertical                   |
| `--control-font-size`      | `0.9375rem` (15px) | idem             | Tipografia dos controls            |
| `--control-radius`         | `14px`             | `14px`           | Raio dos controls                  |
| `--control-invalid-border` | vermelho suave     | vermelho suave   | Borda em `aria-invalid`            |
| `--control-focus-shadow`   | anel branco suave  | anel cinza suave | Foco                               |

## Campos de formulário

| Token                                         | Uso                                        |
| --------------------------------------------- | ------------------------------------------ |
| `--field-gap`                                 | Espaço label → control → hint/erro (`8px`) |
| `--field-label-size` / `--field-label-weight` | Tipografia do label                        |
| `--field-hint-size`                           | Hint e mensagem de erro                    |

**Implementação:** mixins em `components/form-fields/_field-base.scss`; consumidos por `InputField`, `SelectField`, `TextareaField`, `AutocompleteField`, `MultiSelectField` e primitivos `Input`/`Select`/`Command`.

## Auth (login, registo, etc.)

| Token                   | Dark                     | Light              |
| ----------------------- | ------------------------ | ------------------ |
| `--auth-shell-bg`       | `#121212` (`--gray-850`) | `#F5F5F7` (`--bg`) |
| `--auth-outline-btn-bg` | transparente             | `--control-bg`     |

Usar estes tokens em `features/auth/` — **nunca** `--gray-850` direto no SCSS de auth (primitivo dark-only). Campos de formulário nas páginas auth usam os mesmos tokens `--control-*` / `--field-*` do resto do sistema.

## Estados de erro (`ErrorFallback`)

Tokens em `globals.scss`; componente em `components/common/error/error-fallback/`.

| Token                                                              | Uso                                                            |
| ------------------------------------------------------------------ | -------------------------------------------------------------- |
| `--error-page-bg`                                                  | Fundo das rotas `.error-page` e `global-error-body`            |
| `--error-card-bg` / `--error-card-shadow` / `--error-card-padding` | Card central                                                   |
| `--error-danger-*`                                                 | Variantes `error` e `service` (borda, badge e ícone vermelhos) |
| `--error-neutral-*`                                                | Variante `not-found` (borda e ícone neutros)                   |
| `--error-title-size` / `--error-description-size`                  | Tipografia do card                                             |

| Variante    | Visual                                 | Páginas                            |
| ----------- | -------------------------------------- | ---------------------------------- |
| `error`     | Alerta vermelho + detalhes recolhíveis | `error.tsx`, `global-error.tsx`    |
| `service`   | Banner do serviço + alerta vermelho    | `GracefullyDegradingErrorBoundary` |
| `not-found` | Neutro, sem painel de detalhes         | `not-found.tsx`                    |

Layout full-page: classe global `.error-page` (não no componente).

## Notas

- Menu: `features/shell/sidebar/sidebar.module.scss` — item ativo só com fundo neutro arredondado.
- `--danger` mantém-se para erros (semântico); estados de UI usam também `--error-danger-*` / `--error-neutral-*`.
