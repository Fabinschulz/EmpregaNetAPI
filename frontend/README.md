# Frontend

Pasta reservada para a aplicação cliente (por exemplo React, Angular ou Blazor WebAssembly), mantida na mesma raiz do repositório que o `backend` e o `bff`, em modelo **monorepo**.

Quando o projeto UI for criado, recomenda-se manter aqui o `package.json` ou o ficheiro de solução do frontend, e documentar no README da raiz como instalar dependências e executar em desenvolvimento.

## Variáveis de ambiente

Crie `frontend/.env` nesta pasta com as variáveis exigidas pela toolchain (por exemplo `VITE_*`, `NEXT_PUBLIC_*`, etc.) e as URLs da API/BFF conforme a stack local. O ficheiro não deve ser versionado.
