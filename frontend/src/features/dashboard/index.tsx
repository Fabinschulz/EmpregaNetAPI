'use client';

import { useAuth } from '@/shared/context';
import { isAdmin, isRecruitmentStaff } from '@/shared/utils';
import { AnalyticsDashboard } from './analytics';
import { CandidateDashboard } from './candidate';
export { AnalyticsDashboard } from './analytics';
export { CandidateDashboard } from './candidate';

export function DashboardPage() {
  const { roles } = useAuth();

  if (!isRecruitmentStaff(roles)) {
    return <CandidateDashboard />;
  }

  return <AnalyticsDashboard canSelectCompany={isAdmin(roles)} />;
}
