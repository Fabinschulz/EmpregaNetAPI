import type { JobStatus } from './domain';

/** "3 candidaturas em aberto" / "1 candidatura em aberto" / "Nenhuma candidatura em aberto". */
function openApplicationsSubject(count: number): string {
  if (count === 0) return 'Nenhuma candidatura em aberto';
  if (count === 1) return '1 candidatura em aberto';
  return `${count} candidaturas em aberto`;
}

export function describeOpenApplicationsEffect(count: number): string {
  return `${openApplicationsSubject(count)} ${count > 1 ? 'serão canceladas' : 'será cancelada'}.`;
}

export function describeClosedApplicationsEffect(count: number): string {
  return `${openApplicationsSubject(count)} ${count > 1 ? 'foram canceladas' : 'foi cancelada'}.`;
}

export const closeJobDialogCopy = {
  title: 'Encerrar vaga?',
  confirmLabel: 'Encerrar vaga',
  cancelLabel: 'Manter vaga ativa',
  warning:
    'A vaga sai do feed público e deixa de receber candidaturas. ' +
    'Esta ação não tem retorno: a vaga não pode ser reativada.',

  describe: (openApplicationsCount: number): string =>
    `${closeJobDialogCopy.warning} ${describeOpenApplicationsEffect(openApplicationsCount)}`,
  describeWhileCounting: (): string =>
    `${closeJobDialogCopy.warning} Verificando quantas candidaturas em aberto serão canceladas...`,
  describeWithoutCount: (): string =>
    `${closeJobDialogCopy.warning} Não foi possível verificar quantas candidaturas em aberto serão ` +
    'canceladas. Se houver candidaturas em aberto, elas serão canceladas mesmo assim.'
} as const;

export const closeJobSuccessCopy = {
  title: 'Vaga encerrada',
  describe: (affectedApplications: number): string =>
    `A vaga saiu do feed público e deixou de receber candidaturas. ${describeClosedApplicationsEffect(
      affectedApplications
    )}`
} as const;

export const closedJobNoticeCopy: Record<Exclude<JobStatus, 'Active'>, { title: string; body: string }> = {
  ClosedManually: {
    title: 'Vaga encerrada',
    body:
      'Esta vaga saiu do feed público e não recebe candidaturas. O encerramento não tem retorno: a vaga não ' +
      'pode ser reativada.'
  },
  ClosedByFulfillment: {
    title: 'Vagas preenchidas',
    body:
      'Todas as vagas foram preenchidas, e por isso a vaga saiu do feed público e deixou de receber ' +
      'candidaturas. O encerramento não tem retorno: para contratar mais pessoas, publique uma nova vaga.'
  }
};

export type OpenApplicationsCount =
  | { status: 'counting' }
  | { status: 'unavailable' }
  | { status: 'ready'; count: number };

export function describeCloseJobConfirmation(count: OpenApplicationsCount): string {
  if (count.status === 'unavailable') return closeJobDialogCopy.describeWithoutCount();
  if (count.status === 'counting') return closeJobDialogCopy.describeWhileCounting();
  return closeJobDialogCopy.describe(count.count);
}

/**
 * Diálogo de confirmação ao aprovar a **última** posição em aberto de uma vaga.
 */
export const approveLastPositionDialogCopy = {
  title: 'Aprovar a última vaga?',
  confirmLabel: 'Aprovar e encerrar vaga',
  cancelLabel: 'Voltar',
  warning:
    'Esta é a última posição em aberto: aprovar encerra a vaga automaticamente. ' +
    'A vaga sai do feed público e o encerramento não tem retorno.',

  describe: (openApplicationsCount: number): string =>
    `${approveLastPositionDialogCopy.warning} ${describeOpenApplicationsEffect(openApplicationsCount)}`,
  describeWhileCounting: (): string =>
    `${approveLastPositionDialogCopy.warning} Verificando quantas outras candidaturas em aberto serão ` +
    'canceladas...',
  describeWithoutCount: (): string =>
    `${approveLastPositionDialogCopy.warning} Não foi possível verificar quantas outras candidaturas em ` +
    'aberto serão canceladas. Se houver, elas serão canceladas mesmo assim.'
} as const;

export function describeApproveLastPositionConfirmation(count: OpenApplicationsCount): string {
  if (count.status === 'unavailable') return approveLastPositionDialogCopy.describeWithoutCount();
  if (count.status === 'counting') return approveLastPositionDialogCopy.describeWhileCounting();
  return approveLastPositionDialogCopy.describe(count.count);
}
