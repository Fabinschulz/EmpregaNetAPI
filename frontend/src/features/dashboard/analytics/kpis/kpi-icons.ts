import {
  Briefcase,
  BriefcaseBusiness,
  Building2,
  ChartNoAxesColumn,
  FileText,
  Target,
  UserPlus,
  type LucideIcon
} from 'lucide-react';

export const KPI_ICONS: Record<string, LucideIcon> = {
  activeJobs: Briefcase,
  newJobs: BriefcaseBusiness,
  newApplications: FileText,
  newCandidates: UserPlus,
  conversionRate: Target,
  activeCompanies: Building2
};

export const KPI_ICON_FALLBACK: LucideIcon = ChartNoAxesColumn;
