#!/usr/bin/env node
// Eval estático do Harness (sem LLM, sem dependências). Só cobra regras que já estão escritas
// noutro lado — cada violação cita a fonte da regra. Regra nova nasce no documento, não aqui.
// Uso: node .claude/evals/check-harness.mjs   (exit 1 se houver violação)

import { existsSync, readdirSync, readFileSync } from "node:fs";
import { basename, dirname, join, relative, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const ROOT = resolve(dirname(fileURLToPath(import.meta.url)), "../..");
const AGENTS_DIR = join(ROOT, ".claude/agents");
const SKILLS_DIR = join(ROOT, ".claude/skills");
const AGENTS_README = join(ROOT, "docs/agents/README.md");
const SKILLS_README = join(ROOT, "docs/skills/README.md");
const CLAUDE_MD = join(ROOT, ".claude/CLAUDE.md");
const META_AGENT = join(SKILLS_DIR, "meta-agent/SKILL.md");

const SRC_AGENT_PATTERN = 'docs/agents/README.md "Padrão obrigatório de um agent"';
const SRC_ADD_AGENT = 'docs/agents/README.md "Convenções" (ao adicionar um agent)';
const AGENT_KEYS = new Set(["name", "description", "tools", "model"]);
const AGENT_SECTIONS = [
  "Papel", "Use quando", "Não use quando", "Contexto obrigatório", "Entradas necessárias",
  "Processo", "Regras invioláveis", "Validação", "Falhas e escalonamento", "Formato de saída",
];
const WRITE_TOOLS = ["Edit", "Write", "NotebookEdit"];

const violations = [];
const report = (file, line, rule, msg, source) =>
  violations.push({ at: `${relative(ROOT, file).replaceAll("\\", "/")}${line ? `:${line}` : ""}`, rule, msg, source });

const read = (file) => readFileSync(file, "utf8").replace(/\r\n/g, "\n");
const lineOf = (text, index) => text.slice(0, index).split("\n").length;

function frontmatter(text) {
  const m = text.match(/^---\n([\s\S]*?)\n---/);
  if (!m) return null;
  return Object.fromEntries(
    m[1].split("\n").filter((l) => /^\w[\w-]*:/.test(l)).map((l) => {
      const i = l.indexOf(":");
      return [l.slice(0, i).trim(), l.slice(i + 1).trim()];
    }),
  );
}

function section(text, title) {
  const start = text.search(new RegExp(`^## ${title}`, "m"));
  if (start < 0) return "";
  const rest = text.slice(start + 3);
  const end = rest.search(/^## /m);
  return end < 0 ? rest : rest.slice(0, end);
}

// --- Agentes -------------------------------------------------------------------------------------

const agentsReadme = read(AGENTS_README);
const claudeMd = read(CLAUDE_MD);
const metaAgent = read(META_AGENT);
const agentFiles = readdirSync(AGENTS_DIR).filter((f) => f.endsWith(".md"));

for (const f of agentFiles) {
  const file = join(AGENTS_DIR, f);
  const text = read(file);
  const name = basename(f, ".md");
  const fm = frontmatter(text);

  if (!fm) {
    report(file, 1, "agent-frontmatter", "sem frontmatter", SRC_AGENT_PATTERN);
    continue;
  }
  for (const key of Object.keys(fm)) {
    if (!AGENT_KEYS.has(key)) report(file, null, "agent-frontmatter", `campo "${key}" não é consumido pelo Claude Code`, SRC_AGENT_PATTERN);
  }
  if (fm.name !== name) report(file, null, "agent-frontmatter", `name "${fm.name}" difere do ficheiro "${name}"`, SRC_AGENT_PATTERN);
  if (!fm.description) report(file, null, "agent-frontmatter", "description vazia", SRC_AGENT_PATTERN);
  if (!fm.tools) report(file, null, "agent-frontmatter", "tools ausente — a allowlist tem de ser explícita", SRC_AGENT_PATTERN);

  // Secções obrigatórias, na ordem. Secções extra são permitidas entre elas.
  const headings = [...text.matchAll(/^## (.+)$/gm)].map((m) => ({ title: m[1], line: lineOf(text, m.index) }));
  let cursor = 0;
  for (const required of AGENT_SECTIONS) {
    const idx = headings.findIndex((h, i) => i >= cursor && h.title.startsWith(required));
    if (idx < 0) {
      const elsewhere = headings.some((h) => h.title.startsWith(required));
      report(file, null, "agent-sections", elsewhere ? `"## ${required}" fora de ordem` : `falta "## ${required}"`, SRC_AGENT_PATTERN);
    } else cursor = idx + 1;
  }

  if (!/harness-contract/.test(section(text, "Contexto obrigatório")))
    report(file, null, "agent-contract", '"## Contexto obrigatório" não manda ler a harness-contract', SRC_AGENT_PATTERN);
  if (!/bloco de contrato|confidence/i.test(section(text, "Formato de saída")))
    report(file, null, "agent-contract", '"## Formato de saída" não abre com o bloco de contrato', SRC_AGENT_PATTERN);

  // Agente que o índice declara sem escrita não pode ter ferramenta de escrita.
  const row = agentsReadme.split("\n").find((l) => l.startsWith("|") && l.includes(`\`${name}\``));
  const declaresNoWrite = row && /\|\s*Não[^|]*\|\s*$/.test(row);
  const tools = (fm.tools ?? "").split(",").map((t) => t.trim());
  const writeTools = tools.filter((t) => WRITE_TOOLS.includes(t));
  if (declaresNoWrite && writeTools.length)
    report(file, null, "read-only", `declarado sem escrita no índice, mas tem ${writeTools.join(", ")}`, 'docs/agents/README.md "Anti-padrões" (Edit/Write a quem julga)');

  // Registo nos quatro sítios.
  if (!row) report(AGENTS_README, null, "registry", `agente "${name}" ausente da tabela de agentes`, SRC_ADD_AGENT);
  if (!claudeMd.includes(`\`${name}\``)) report(CLAUDE_MD, null, "registry", `agente "${name}" ausente da tabela de agentes`, SRC_ADD_AGENT);
  if (!metaAgent.includes(`${name}`)) report(META_AGENT, null, "registry", `agente "${name}" ausente da tabela de roteamento`, SRC_ADD_AGENT);
}

// --- Skills --------------------------------------------------------------------------------------

const skillsReadme = read(SKILLS_README);
const skillDirs = readdirSync(SKILLS_DIR, { withFileTypes: true }).filter((d) => d.isDirectory()).map((d) => d.name);

for (const dir of skillDirs) {
  const file = join(SKILLS_DIR, dir, "SKILL.md");
  if (!existsSync(file)) {
    report(join(SKILLS_DIR, dir), null, "skill-frontmatter", "pasta de skill sem SKILL.md", "docs/skills/README.md");
    continue;
  }
  const fm = frontmatter(read(file));
  if (!fm) report(file, 1, "skill-frontmatter", "sem frontmatter", "docs/skills/README.md");
  else {
    if (fm.name !== dir) report(file, null, "skill-frontmatter", `name "${fm.name}" difere da pasta "${dir}"`, "docs/skills/README.md");
    if (!fm.description) report(file, null, "skill-frontmatter", "description vazia — a skill não carrega sozinha sem ela", "docs/skills/README.md");
  }
  if (!skillsReadme.includes(`\`${dir}\``)) report(SKILLS_README, null, "registry", `skill "${dir}" ausente do índice`, "docs/skills/README.md");
  // Skills invocáveis aparecem como comando (`/nome`); as de conhecimento, pelo nome.
  if (!claudeMd.includes(`\`${dir}\``) && !claudeMd.includes(`\`/${dir}\``)) report(CLAUDE_MD, null, "registry", `skill "${dir}" ausente da tabela de skills`, ".claude/CLAUDE.md");
}

// --- Ligações e referências ----------------------------------------------------------------------

const harnessDocs = [
  CLAUDE_MD, AGENTS_README, SKILLS_README,
  ...agentFiles.map((f) => join(AGENTS_DIR, f)),
  ...skillDirs.flatMap((d) => readdirSync(join(SKILLS_DIR, d)).filter((f) => f.endsWith(".md")).map((f) => join(SKILLS_DIR, d, f))),
];

for (const file of harnessDocs) {
  const text = read(file);
  const prose = text.replace(/```[\s\S]*?```/g, (m) => m.replace(/[^\n]/g, " "));

  for (const m of prose.matchAll(/\]\(([^)\s]+)\)/g)) {
    const target = m[1].split("#")[0];
    if (!target || /^[a-z]+:/i.test(target)) continue;
    // Ligação deliberada para um documento futuro, marcada pelo autor com "(quando existir)".
    if (/\(quando existir\)/.test(text.split("\n")[lineOf(text, m.index) - 1])) continue;
    if (!existsSync(resolve(dirname(file), decodeURIComponent(target))))
      report(file, lineOf(text, m.index), "broken-link", `ligação para "${m[1]}" não resolve`, "—");
  }

  // Referência entre ficheiros por número de secção: quebra em silêncio quando a outra skill reordena.
  const crossRef = /\]\([^)]+\.md[^)]*\)\)?\s*§\s?\d+|§\s?\d+(\.\d+)?\s+d[oa]\s+(\[)?`[\w-]+`/g;
  for (const m of prose.matchAll(crossRef))
    report(file, lineOf(text, m.index), "cross-file-section-ref", `"${m[0].trim()}" — usar o título da secção entre aspas`, 'docs/agents/README.md "Anti-padrões ao escrever um agent"');
}

// --- Resultado -----------------------------------------------------------------------------------

if (!violations.length) {
  console.log(`harness ok — ${agentFiles.length} agentes, ${skillDirs.length} skills, ${harnessDocs.length} documentos verificados`);
  process.exit(0);
}
for (const v of violations) console.log(`${v.at}  [${v.rule}]  ${v.msg}\n    regra: ${v.source}`);
console.log(`\n${violations.length} violação(ões).`);
process.exit(1);
