# EmpregaUAI — Design tokens

Definição principal: `src/app/globals.scss`.

## Tema dark —  HIG

O player em [music.apple.com](https://music.apple.com/br/new?l=en) segue a **mesma família de cores** que o [Human Interface Guidelines](https://developer.apple.com/design/human-interface-guidelines/color) (system colors em dark mode). Não há paleta CSS pública na página; aplicamos a **matriz de camadas** oficial.

| Token projeto | Hex / valor | Equivalente HIG / uso no Music |
| ------------- | ----------- | ------------------------------ |
| `--gray-950` | `#000000` | `systemBackground` — fundo da app |
| `--gray-900` | `#121212` | Chrome da barra lateral (web) |
| `--gray-850` | `#1C1C1E` | `secondarySystemBackground` — inputs, blocos |
| `--gray-800` | `#252525` | Cards / filas “elevadas” |
| `--gray-750` | `#2C2C2E` | `tertiarySystemBackground` — hover |
| `--gray-700` | `#3A3A3C` | Quaternary fill — pressed |
| `--text` | `#F5F5F7` | Label primário |
| `--muted` | `#98989D` | Secondary label (aprox.) |
| `--border` | `rgba(84,84,88,0.48)` | Separator (HIG) |
| `--border-strong` | `rgba(84,84,88,0.65)` | Separator em destaque |
| `--separator-opaque` | `#38383A` | Separador opaco (opcional) |

### Acento da marca (EmpregaUAI)

O acento secundário/terciário foi atualizado para **vermelho** com base visual no Apple Music.

## Tema light

Mantém o off-white `#F5F5F7`, texto `#1C1C1E` e aplica acento vermelho com contraste AA em branco — ver `globals.scss`.

### Navegação lateral (light)

| Token | Uso |
| ----- | --- |
| `--nav-fg` | Itens inativos (`#48484A`, ~7:1 em branco) |
| `--nav-fg-active` | Item ativo (vermelho marca) |
| `--nav-bg-active` | Fundo do item ativo (`rgba` vermelho suave) |
| `--nav-bg-hover` | Hover dos itens |
| `--nav-border-ring` | Contorno do item ativo (visível no light) |
| `--nav-group-title` | Título de secção do menu |

## Nota

Referência visual de acento rosado aplicada no projeto: família centrada em **`#FA586A`** com variações para hover/focus e contraste.
