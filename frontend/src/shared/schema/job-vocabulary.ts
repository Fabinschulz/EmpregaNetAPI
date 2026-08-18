export type VocabularyOption<T extends string = string> = {
  readonly value: T;
  readonly label: string;
};

/** Cria o par nome→rótulo e o normalizador de um enum do backend, a partir da ordem dele. */
function createVocabulary<const T extends readonly VocabularyOption[]>(
  options: T,
  /** Ordem completa do enum no backend, índice 0 = `NaoSelecionado`. Buracos entram como `null`. */
  order: readonly (string | null)[]
) {
  const valueSet = new Set<string>(options.map((o) => o.value));
  const labels = new Map(options.map((o) => [o.value, o.label]));
  const byLowercase = new Map(options.map((o) => [o.value.toLowerCase(), o.value]));

  const fromIndex = (index: number): T[number]['value'] | '' => {
    const name = order[index];
    return name && valueSet.has(name) ? (name as T[number]['value']) : '';
  };

  return {
    options,
    values: options.map((o) => o.value) as ReadonlyArray<T[number]['value']>,
    has: (value: string): value is T[number]['value'] => valueSet.has(value),
    label: (value: string | null | undefined): string => (value ? (labels.get(value) ?? '') : ''),
    /** Aceita nome (em qualquer caixa) ou índice do enum; devolve `''` quando não reconhecido. */
    normalize: (input: string | number | null | undefined): T[number]['value'] | '' => {
      if (input == null) return '';
      if (typeof input === 'number') return fromIndex(input);

      const trimmed = input.trim();
      if (trimmed === '') return '';

      const byName = byLowercase.get(trimmed.toLowerCase());
      if (byName) return byName as T[number]['value'];

      const asIndex = Number(trimmed);
      if (Number.isInteger(asIndex) && asIndex > 0) return fromIndex(asIndex);

      return '';
    }
  };
}

const JOB_TYPE_ORDER = [
  'NaoSelecionado',
  'FullTime',
  'PartTime',
  'Internship',
  'Freelancer',
  'Temporary',
  'Trainee',
  'Volunteer',
  null,
  'Clt',
  'Pj'
] as const;

export const jobTypeVocabulary = createVocabulary(
  [
    { value: 'Clt', label: 'CLT' },
    { value: 'Pj', label: 'PJ' },
    { value: 'Temporary', label: 'Temporário' },
    { value: 'Internship', label: 'Estágio' },
    { value: 'Trainee', label: 'Trainee' },
    { value: 'FullTime', label: 'Tempo Integral' },
    { value: 'PartTime', label: 'Meio Período' },
    { value: 'Freelancer', label: 'Freelancer' },
    { value: 'Volunteer', label: 'Voluntário' }
  ] as const,
  JOB_TYPE_ORDER
);

export type JobTypeValue = (typeof jobTypeVocabulary.options)[number]['value'];

export const workShiftVocabulary = createVocabulary(
  [
    { value: 'Administrativo', label: 'Administrativo (comercial)' },
    { value: 'PrimeiroTurno', label: '1º turno' },
    { value: 'SegundoTurno', label: '2º turno' },
    { value: 'TerceiroTurno', label: '3º turno (noturno)' },
    { value: 'Revezamento', label: 'Turno de revezamento' },
    { value: 'Escala12x36', label: 'Escala 12x36' },
    { value: 'Escala6x1', label: 'Escala 6x1' }
  ] as const,
  [
    'NaoSelecionado',
    'Administrativo',
    'PrimeiroTurno',
    'SegundoTurno',
    'TerceiroTurno',
    'Revezamento',
    'Escala12x36',
    'Escala6x1'
  ]
);

export type WorkShiftValue = (typeof workShiftVocabulary.options)[number]['value'];

export const experienceLevelVocabulary = createVocabulary(
  [
    { value: 'SemExperiencia', label: 'Sem experiência' },
    { value: 'AteUmAno', label: 'Até 1 ano' },
    { value: 'DeUmATresAnos', label: 'De 1 a 3 anos' },
    { value: 'DeTresACincoAnos', label: 'De 3 a 5 anos' },
    { value: 'MaisDeCincoAnos', label: 'Mais de 5 anos' }
  ] as const,
  ['NaoSelecionado', 'SemExperiencia', 'AteUmAno', 'DeUmATresAnos', 'DeTresACincoAnos', 'MaisDeCincoAnos']
);

export type ExperienceLevelValue = (typeof experienceLevelVocabulary.options)[number]['value'];

export const NO_EXPERIENCE_REQUIRED: ExperienceLevelValue = 'SemExperiencia';
export const workModelVocabulary = createVocabulary(
  [
    { value: 'OnSite', label: 'Presencial' },
    { value: 'Hybrid', label: 'Híbrido' },
    { value: 'Remote', label: 'Remoto' }
  ] as const,
  ['NaoSelecionado', 'OnSite', 'Hybrid', 'Remote']
);

export type WorkModelValue = (typeof workModelVocabulary.options)[number]['value'];

export const jobAreaVocabulary = createVocabulary(
  [
    { value: 'Producao', label: 'Produção' },
    { value: 'Logistica', label: 'Logística' },
    { value: 'Almoxarifado', label: 'Almoxarifado' },
    { value: 'Transporte', label: 'Transporte' },
    { value: 'Manutencao', label: 'Manutenção' },
    { value: 'Qualidade', label: 'Qualidade' },
    { value: 'SegurancaTrabalho', label: 'Segurança do Trabalho' },
    { value: 'ServicosGerais', label: 'Serviços Gerais' },
    { value: 'Administrativo', label: 'Administrativo' },
    { value: 'Comercial', label: 'Comercial' },
    { value: 'RecursosHumanos', label: 'Recursos Humanos' },
    { value: 'Financeiro', label: 'Financeiro' },
    { value: 'Ti', label: 'Tecnologia da Informação' },
    { value: 'Saude', label: 'Saúde' },
    { value: 'Educacao', label: 'Educação' },
    { value: 'Outras', label: 'Outras' }
  ] as const,
  [
    'NaoSelecionado',
    'Producao',
    'Logistica',
    'Almoxarifado',
    'Transporte',
    'Manutencao',
    'Qualidade',
    'SegurancaTrabalho',
    'ServicosGerais',
    'Administrativo',
    'Comercial',
    'RecursosHumanos',
    'Financeiro',
    'Ti',
    'Saude',
    'Educacao',
    'Outras'
  ]
);

export type JobAreaValue = (typeof jobAreaVocabulary.options)[number]['value'];

export type SalaryRange = {
  readonly value: string;
  readonly label: string;
  readonly min?: number;
  readonly max?: number;
};

/**
 * Faixas do filtro, calibradas para o mercado atendido.
 *
 * A escala anterior (até 2k / 2–4k / 4–6k / 6–10k / +10k) achatava justamente a faixa onde está
 * a maior parte das vagas do polo: quase tudo caía nos dois primeiros degraus, o que torna o
 * filtro inútil. Aqui os degraus são estreitos na base e largos no topo.
 *
 * O `value` vai para a URL e precisa ser estável: mudá-lo invalida links já compartilhados.
 */
export const SALARY_RANGE_OPTIONS: readonly SalaryRange[] = [
  { value: 'ate-1800', label: 'Até R$ 1.800', max: 1800 },
  { value: '1800-2300', label: 'R$ 1.800 a R$ 2.300', min: 1800, max: 2300 },
  { value: '2300-3000', label: 'R$ 2.300 a R$ 3.000', min: 2300, max: 3000 },
  { value: '3000-4500', label: 'R$ 3.000 a R$ 4.500', min: 3000, max: 4500 },
  { value: '4500-7000', label: 'R$ 4.500 a R$ 7.000', min: 4500, max: 7000 },
  { value: 'acima-7000', label: 'Acima de R$ 7.000', min: 7000 }
] as const;

export const findSalaryRange = (value: string | null | undefined): SalaryRange | undefined =>
  value ? SALARY_RANGE_OPTIONS.find((range) => range.value === value) : undefined;

/** Espelha `JobPublishedWindowEnum`. `Today` e `Last24Hours` são janelas diferentes. */
export const PUBLISHED_WITHIN_OPTIONS = [
  { value: 'Today', label: 'Hoje' },
  { value: 'Last24Hours', label: 'Últimas 24 horas' },
  { value: 'Last3Days', label: 'Últimos 3 dias' },
  { value: 'Last7Days', label: 'Últimos 7 dias' },
  { value: 'Last15Days', label: 'Últimos 15 dias' },
  { value: 'Last30Days', label: 'Últimos 30 dias' }
] as const;

export type PublishedWithinValue = (typeof PUBLISHED_WITHIN_OPTIONS)[number]['value'];

const PUBLISHED_WITHIN_SET = new Set<string>(PUBLISHED_WITHIN_OPTIONS.map((o) => o.value));

export const isPublishedWithinValue = (value: string): value is PublishedWithinValue => PUBLISHED_WITHIN_SET.has(value);

export const publishedWithinLabel = (value: string | null | undefined): string =>
  PUBLISHED_WITHIN_OPTIONS.find((o) => o.value === value)?.label ?? '';

export const JOB_SORT_OPTIONS = [
  { value: 'Recent', label: 'Mais recentes' },
  { value: 'Relevance', label: 'Relevância' },
  { value: 'Salary', label: 'Maior salário' },
  { value: 'Company', label: 'Empresa (A-Z)' },
  { value: 'Location', label: 'Localização (A-Z)' }
] as const;

export type JobSortValue = (typeof JOB_SORT_OPTIONS)[number]['value'];

export const DEFAULT_JOB_SORT: JobSortValue = 'Recent';
const JOB_SORT_SET = new Set<string>(JOB_SORT_OPTIONS.map((o) => o.value));

export const isJobSortValue = (value: string): value is JobSortValue => JOB_SORT_SET.has(value);

export const MAX_VOCABULARY_ITEMS_PER_JOB = 20;
