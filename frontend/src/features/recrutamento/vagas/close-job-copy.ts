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

export function jobStatusLabel(isActive: boolean): string {
  return isActive ? 'Ativa' : 'Encerrada';
}

export type OpenApplicationsCount =
  | { status: 'counting' }
  | { status: 'unavailable' }
  | { status: 'ready'; count: number };

export function describeCloseJobConfirmation(count: OpenApplicationsCount): string {
  if (count.status === 'unavailable') return closeJobDialogCopy.describeWithoutCount();
  if (count.status === 'counting') return closeJobDialogCopy.describeWhileCounting();
  return closeJobDialogCopy.describe(count.count);
}
