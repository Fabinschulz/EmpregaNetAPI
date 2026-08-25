import nextVitals from 'eslint-config-next/core-web-vitals';
import nextTs from 'eslint-config-next/typescript';
import prettier from 'eslint-config-prettier/flat';
import { defineConfig, globalIgnores } from 'eslint/config';

const NO_FEATURES_FROM_SHARED = {
  group: ['@/features', '@/features/*', '../features/*', '../../features/*'],
  message:
    'shared/ não pode importar de features/. Um componente compartilhado que precisa conhecer uma entidade do domínio pertence a features/.'
};

const NO_APP_FROM_SHARED = {
  group: ['@/app', '@/app/*'],
  message: 'shared/ não pode importar de app/. A dependência aponta para dentro: app -> features -> shared.'
};

const NO_DEEP_CROSS_SLICE = {
  group: [
    '@/features/*/service/*',
    '@/features/*/*/service/*',
    '@/features/*/form/*',
    '@/features/*/*/form/*',
    '@/features/*/domain/*',
    '@/features/*/*/domain/*'
  ],
  message:
    'Import profundo num módulo de feature. Use a fronteira pública: `@/features/<slice>/service` (ou `/form`, `/domain`). Dentro do próprio slice, use caminho relativo (`../service`).'
};

const NO_SHARED_MEGA_BARREL = {
  name: '@/shared',
  message:
    'Import o módulo concreto (ex: `@/shared/components`, `@/shared/shell`, `@/shared/status`) em vez do barrel genérico. Isso evita dependência de barrel intermediário e permite tree-shaking.'
};

const NO_AXIOS_IN_PAGE = {
  name: 'axios',
  message: 'Página não fala HTTP. Use o hook da feature (features/<x>/service/).'
};

const NO_AXIOS_IN_COMPONENT = {
  name: 'axios',
  message:
    'Componente não fala HTTP. A chamada pertence a features/<x>/service/<x>-api.ts (infra transversal em shared/api).'
};

const NO_EXPORT_STAR = {
  selector: 'ExportAllDeclaration',
  message:
    '`export *` proibido na fronteira pública do módulo de feature: use exports nomeados, para que a superfície pública do módulo seja explícita e não dependa de barrel intermediário.'
};

/**
 * Atomic design em `shared/components/ui`: a dependência aponta sempre para baixo na
 * escada `atoms <- molecules <- organisms <- templates` (as "pages" são as rotas em
 * `src/app`).
 */
const UI_TIER_ORDER = ['atoms', 'molecules', 'organisms', 'templates'];

const uiTierSpecifiers = (tier) => [
  `@/shared/components/ui/${tier}`,
  `@/shared/components/ui/${tier}/*`,
  `@/shared/components/ui/${tier}/**`,
  `../${tier}`,
  `../${tier}/*`,
  `../${tier}/**`,
  `../../${tier}`,
  `../../${tier}/*`,
  `../../${tier}/**`
];

const noTiersAbove = (tier) => {
  const above = UI_TIER_ORDER.slice(UI_TIER_ORDER.indexOf(tier) + 1);
  if (above.length === 0) return null;
  return {
    group: above.flatMap(uiTierSpecifiers),
    message: `Import ascendente no atomic design: \`${tier}/\` não pode depender de ${above
      .map((t) => `\`${t}/\``)
      .join(' nem ')}. Inverta a dependência - o nivel de cima compõe o de baixo, passando o conteúdo por prop/slot.`
  };
};


const NO_UI_BARRELS_INSIDE_UI = [
  {
    name: '@/shared/components',
    message:
      'Dentro de shared/components/ui, importe o nível concreto por caminho relativo (ex: `../../atoms/button`). Passar pelo barrel raiz cria um ciclo entre o barrel e os seus próprios membros.'
  },
  {
    name: '@/shared/components/ui',
    message:
      'Dentro de shared/components/ui, importe o nível concreto por caminho relativo (ex: `../../molecules/popover`) em vez do barrel do próprio módulo.'
  }
];

const uiTierRule = (tier) => ({
  files: [`src/shared/components/ui/${tier}/**/*.{ts,tsx}`],
  rules: {
    'no-restricted-imports': [
      'error',
      {
        patterns: [NO_FEATURES_FROM_SHARED, NO_APP_FROM_SHARED, noTiersAbove(tier)].filter(Boolean),
        paths: [NO_AXIOS_IN_COMPONENT, NO_SHARED_MEGA_BARREL, ...NO_UI_BARRELS_INSIDE_UI]
      }
    ]
  }
});

const eslintConfig = defineConfig([
  ...nextVitals,
  ...nextTs,
  {
    settings: {
      react: {
        version: '19'
      }
    }
  },
  prettier,

  globalIgnores(['.next/**', 'out/**', 'build/**', 'next-env.d.ts']),

  {
    files: ['**/*.{ts,tsx,mts,cts}'],
    rules: {
      '@typescript-eslint/no-explicit-any': 'error',
      '@typescript-eslint/no-non-null-assertion': 'error',
      '@typescript-eslint/ban-ts-comment': 'error',
      '@typescript-eslint/no-unused-vars': [
        'error',
        { argsIgnorePattern: '^_', varsIgnorePattern: '^_', caughtErrorsIgnorePattern: '^_' }
      ]
    }
  },
  {
    files: ['**/*.{js,jsx,mjs,ts,tsx,mts,cts}'],
    rules: {
      'react-hooks/exhaustive-deps': 'error',
      'no-console': ['error', { allow: ['error', 'warn'] }]
    }
  },
  {
    files: ['tests/**/*.{ts,tsx}'],
    rules: {
      '@typescript-eslint/no-non-null-assertion': 'off'
    }
  },
  {
    files: ['src/shared/**/*.{ts,tsx}'],
    rules: {
      'no-restricted-imports': [
        'error',
        {
          patterns: [NO_FEATURES_FROM_SHARED, NO_APP_FROM_SHARED],
          paths: [NO_SHARED_MEGA_BARREL]
        }
      ]
    }
  },
  {
    files: ['src/shared/components/**/*.tsx', 'src/shared/shell/**/*.tsx', 'src/shared/status/**/*.tsx'],
    rules: {
      'no-restricted-imports': [
        'error',
        {
          patterns: [NO_FEATURES_FROM_SHARED, NO_APP_FROM_SHARED],
          paths: [NO_AXIOS_IN_COMPONENT, NO_SHARED_MEGA_BARREL]
        }
      ]
    }
  },

  {
    files: ['src/app/**/*.{ts,tsx}'],
    rules: {
      'no-restricted-imports': ['error', { patterns: [NO_DEEP_CROSS_SLICE], paths: [NO_SHARED_MEGA_BARREL] }]
    }
  },
  {
    files: ['src/app/**/*.tsx'],
    rules: {
      'no-restricted-imports': [
        'error',
        { patterns: [NO_DEEP_CROSS_SLICE], paths: [NO_AXIOS_IN_PAGE, NO_SHARED_MEGA_BARREL] }
      ]
    }
  },
  {
    files: ['src/features/**/*.{ts,tsx}'],
    rules: {
      'no-restricted-imports': ['error', { patterns: [NO_DEEP_CROSS_SLICE], paths: [NO_SHARED_MEGA_BARREL] }]
    }
  },
  {
    files: ['src/features/**/*.tsx'],
    rules: {
      'no-restricted-imports': [
        'error',
        { patterns: [NO_DEEP_CROSS_SLICE], paths: [NO_AXIOS_IN_COMPONENT, NO_SHARED_MEGA_BARREL] }
      ]
    }
  },
  {
    files: ['src/features/**/index.ts'],
    rules: {
      'no-restricted-syntax': ['error', NO_EXPORT_STAR]
    }
  },

  ...UI_TIER_ORDER.map(uiTierRule)
]);

export default eslintConfig;
