interface JobAvailabilityFlags {
  isJobAvailable?: boolean;
  isJobViewable?: boolean;
}

export function isJobAvailable({ isJobAvailable }: JobAvailabilityFlags): boolean {
  return isJobAvailable !== false;
}

// Vaga encerrada abre o aviso de processo encerrado; só a excluída dá 404. Sem isJobViewable (API antiga), vale o comportamento anterior.
export function isJobViewable(application: JobAvailabilityFlags): boolean {
  return application.isJobViewable ?? isJobAvailable(application);
}
