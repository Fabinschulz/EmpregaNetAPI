import { applicationStatusLabel, type ApplicationStatus } from './domain';

const transitionTitleByStatus: Record<ApplicationStatus, (candidateName: string) => string> = {
  Pending: (name) => `Reabrir a candidatura de ${name}?`,
  Processing: (name) => `Iniciar a análise da candidatura de ${name}?`,
  Approved: (name) => `Aprovar a candidatura de ${name}?`,
  Rejected: (name) => `Reprovar a candidatura de ${name}?`,
  Timeout: (name) => `Marcar como expirada a candidatura de ${name}?`,
  Canceled: (name) => `Cancelar a candidatura de ${name}?`,
  Error: (name) => `Marcar erro na candidatura de ${name}?`,
  Finished: (name) => `Concluir a candidatura de ${name}?`,
  CanceledByCandidate: (name) => `Cancelar a candidatura de ${name} pelo candidato?`
};

export function applicationTransitionDialogTitle(targetStatus: ApplicationStatus, candidateName: string): string {
  return transitionTitleByStatus[targetStatus](candidateName);
}

export function describeApplicationTransitionConfirmation(
  currentStatus: ApplicationStatus | null,
  targetStatus: ApplicationStatus
): string {
  const currentLabel = currentStatus ? applicationStatusLabel(currentStatus, 'recruiter') : 'atual';
  const targetLabel = applicationStatusLabel(targetStatus, 'recruiter');
  return `Isso moverá o status de '${currentLabel}' para '${targetLabel}'.`;
}
