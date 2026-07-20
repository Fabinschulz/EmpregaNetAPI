/**
 * Configuração do Cucumber.js (BDD). Executado via `tsx` (ver package.json:scripts),
 * que registra o loader TS/ESM antes do Cucumber importar specs e steps —
 * por isso nenhuma etapa de build/transpile é necessária aqui.
 *
 * Perfis do Cucumber = exports nomeados deste módulo (não chaves aninhadas
 * dentro do objeto). `--profile unit` seleciona o export `unit` abaixo.
 */
const importPaths = ['tests/support/**/*.ts', 'tests/steps/**/*.ts'];
const format = ['@cucumber/pretty-formatter'];

export const unit = {
  paths: ['tests/specs/unit/**/*.feature'],
  import: importPaths,
  format,
  publishQuiet: true
};

export const integration = {
  paths: ['tests/specs/integration/**/*.feature'],
  import: importPaths,
  format,
  publishQuiet: true
};

const defaultProfile = {
  paths: ['tests/specs/**/*.feature'],
  import: importPaths,
  format,
  publishQuiet: true
};

export default defaultProfile;
