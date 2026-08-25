import {
    Archive,
    ArrowLeft,
    ArrowRight,
    Briefcase,
    Building2,
    Check,
    ChevronDown,
    ChevronLeft,
    ChevronRight,
    ChevronUp,
    ChevronsLeft,
    ChevronsRight,
    Download,
    Eye,
    FileText,
    FilterX,
    Info,
    KeyRound,
    LayoutDashboard,
    Link2,
    LogIn,
    LogOut,
    Mail,
    Menu,
    Minus,
    Moon,
    MoreVertical,
    PanelLeft,
    PanelLeftClose,
    Pencil,
    Phone,
    Plus,
    RefreshCw,
    Save,
    Search,
    Send,
    Settings,
    ShieldCheck,
    SlidersHorizontal,
    Sun,
    Trash2,
    Upload,
    UserCircle,
    UserPlus,
    UserRound,
    Users,
    X,
    type LucideIcon
} from 'lucide-react';

/**
 * Vocabulário canónico de ícones de **ação**.
 * @example
 * <Button startIcon={actionIcons.create}>Nova vaga</Button>
 * <Button asChild><Link href={href}><actionIcons.back aria-hidden />Voltar</Link></Button>
 */
export const actionIcons = {
  /** Gravar o formulário. */
  save: Save,
  /** Voltar à origem da navegação (lista, tela anterior). */
  back: ArrowLeft,
  /** Criar um registro novo. */
  create: Plus,
  /** Acrescentar um item a um conjunto já em edição. */
  add: Plus,
  /** Editar um registro existente. */
  edit: Pencil,
  /** Excluir em definitivo. */
  delete: Trash2,
  /** Retirar um item do conjunto, sem excluir o registro. */
  remove: Minus,
  /** Abandonar a operação em curso. */
  cancel: X,
  /** Fechar um overlay (dialog, drawer, painel). */
  close: X,
  /** Confirmar / aceitar. */
  confirm: Check,
  /** Buscar. */
  search: Search,
  /** Abrir os controles de filtro. */
  filter: SlidersHorizontal,
  /** Devolver os filtros ao padrão - **não** é "fechar". */
  clearFilters: FilterX,
  /** Esvaziar um campo de texto. */
  clearInput: X,
  /** Abrir o registro para leitura. */
  view: Eye,
  /** Abrir a ficha completa do registro. */
  details: Eye,
  /** Refazer a busca / recarregar. */
  refresh: RefreshCw,
  /** Repetir a operação que falhou. */
  retry: RefreshCw,
  /** Baixar dados. */
  export: Download,
  /** Enviar dados de um arquivo. */
  import: Upload,
  /** Preferências e configuração. */
  settings: Settings,
  /** Ações excedentes num menu (⋮). */
  more: MoreVertical,
  /** Abrir a navegação no telefone. */
  menu: Menu,
  /** Avançar num fluxo. */
  next: ArrowRight,
  /** Recuar num fluxo. */
  previous: ArrowLeft,
  /** Primeira página de uma listagem. */
  firstPage: ChevronsLeft,
  /** Página anterior. */
  previousPage: ChevronLeft,
  /** Próxima página. */
  nextPage: ChevronRight,
  /** Última página. */
  lastPage: ChevronsRight,
  /** Revelar o conteúdo de uma seção recolhida. */
  expand: ChevronDown,
  /** Recolher uma seção aberta. */
  collapse: ChevronUp,
  /** Expandir o menu lateral compactado. */
  expandSidebar: PanelLeft,
  /** Compactar o menu lateral. */
  collapseSidebar: PanelLeftClose,
  /** Iniciar sessão. */
  signIn: LogIn,
  /** Encerrar sessão. */
  signOut: LogOut,
  /** Criar conta. */
  signUp: UserPlus,
  /** Operações de senha (alterar, redefinir). */
  password: KeyRound,
  /** Enviar por e-mail. */
  email: Mail,
  /** Enviar um formulário/pedido ao servidor (link de recuperação, confirmação). */
  send: Send,
  /** Candidatar-se a uma vaga. */
  apply: Send,
  /** Ligar para o contato. */
  phone: Phone,
  /** Copiar o link do registro. */
  copyLink: Link2,
  /** Encerrar uma vaga sem excluí-la. */
  archive: Archive,
  /** Explicação de apoio sobre o que está na tela. */
  info: Info,
  /** Alternar para o tema claro. */
  themeLight: Sun,
  /** Alternar para o tema escuro. */
  themeDark: Moon
} as const satisfies Record<string, LucideIcon>;

export type ActionIconName = keyof typeof actionIcons;

export const entityIcons = {
  dashboard: LayoutDashboard,
  job: Briefcase,
  application: FileText,
  candidate: UserRound,
  candidates: Users,
  user: UserRound,
  users: Users,
  company: Building2,
  profile: UserCircle,
  permissions: ShieldCheck
} as const satisfies Record<string, LucideIcon>;

export type EntityIconName = keyof typeof entityIcons;
