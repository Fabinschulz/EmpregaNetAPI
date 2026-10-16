interface JobAvailabilityFlags {
  isJobAvailable?: boolean;
  isJobViewable?: boolean;
}

export function isJobAvailable({ isJobAvailable }: JobAvailabilityFlags): boolean {
  return isJobAvailable !== false;
}

export function isJobViewable(application: JobAvailabilityFlags): boolean {
  return application.isJobViewable ?? isJobAvailable(application);
}
