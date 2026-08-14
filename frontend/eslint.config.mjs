import nextVitals from 'eslint-config-next/core-web-vitals';
import nextTs from 'eslint-config-next/typescript';
import prettier from 'eslint-config-prettier/flat';
import { defineConfig, globalIgnores } from 'eslint/config';

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
          patterns: [
            {
              group: ['@/features', '@/features/*', '../features/*', '../../features/*'],
              message:
                'shared/ não pode importar de features/. Um componente compartilhado que precisa conhecer uma entidade do domínio pertence a features/.'
            },
            {
              group: ['@/app', '@/app/*'],
              message: 'shared/ não pode importar de app/. A dependência aponta para dentro: app -> features -> shared.'
            }
          ]
        }
      ]
    }
  },
  {
    files: ['src/app/**/*.tsx'],
    rules: {
      'no-restricted-imports': [
        'error',
        {
          paths: [
            {
              name: 'axios',
              message: 'Página não fala HTTP. Use o hook da feature (features/<x>/service/).'
            }
          ]
        }
      ]
    }
  },
  {
    files: ['src/features/**/*.tsx', 'src/shared/components/**/*.tsx', 'src/shared/shell/**/*.tsx'],
    rules: {
      'no-restricted-imports': [
        'error',
        {
          paths: [
            {
              name: 'axios',
              message:
                'Componente não fala HTTP. A chamada pertence a features/<x>/service/<x>-api.ts (infra transversal em shared/api).'
            }
          ]
        }
      ]
    }
  }
]);

export default eslintConfig;
