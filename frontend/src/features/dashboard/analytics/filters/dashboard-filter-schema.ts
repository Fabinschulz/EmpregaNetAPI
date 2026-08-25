import { z } from 'zod';
import { DASHBOARD_PERIODS, type DashboardFilters } from '../../service';

export const dashboardFilterFormSchema = z
  .object({
    period: z.enum(DASHBOARD_PERIODS),
    from: z.string(),
    to: z.string(),
    companyId: z.string(),
    states: z.array(z.string()),
    areas: z.array(z.string()),
    applicationStatus: z.string()
  })
  .refine((values) => values.period !== 'Custom' || Boolean(values.from), {
    message: 'Informe a data inicial.',
    path: ['from']
  })
  .refine((values) => values.period !== 'Custom' || Boolean(values.to), {
    message: 'Informe a data final.',
    path: ['to']
  })
  .refine((values) => values.period !== 'Custom' || !values.from || !values.to || values.from <= values.to, {
    message: 'A data final não pode ser anterior à inicial.',
    path: ['to']
  });

export type DashboardFilterFormValues = z.infer<typeof dashboardFilterFormSchema>;

export const ALL_COMPANIES_VALUE = 'all';
export const ALL_STATUSES_VALUE = 'all';

export const defaultDashboardFilterForm: DashboardFilterFormValues = {
  period: 'Last30Days',
  from: '',
  to: '',
  companyId: ALL_COMPANIES_VALUE,
  states: [],
  areas: [],
  applicationStatus: ALL_STATUSES_VALUE
};

export function dashboardFilterFormToFilters(values: DashboardFilterFormValues): DashboardFilters | null {
  if (values.period === 'Custom' && (!values.from || !values.to || values.from > values.to)) {
    return null;
  }

  const companyId = values.companyId && values.companyId !== ALL_COMPANIES_VALUE ? Number(values.companyId) : undefined;

  return {
    period: values.period,
    from: values.period === 'Custom' ? values.from : undefined,
    to: values.period === 'Custom' ? values.to : undefined,
    companyId: Number.isFinite(companyId) ? companyId : undefined,
    states: values.states.length ? values.states : undefined,
    areas: values.areas.length ? values.areas : undefined,
    applicationStatus:
      values.applicationStatus && values.applicationStatus !== ALL_STATUSES_VALUE ? values.applicationStatus : undefined
  };
}
