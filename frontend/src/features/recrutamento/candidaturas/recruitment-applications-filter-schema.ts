import { LIST_ORDER_BY_VALUES } from '@/shared/schema';
import { z } from 'zod';

export const recruitmentApplicationsFilterFormSchema = z.object({
  orderBy: z.enum(LIST_ORDER_BY_VALUES)
});

export type RecruitmentApplicationsFilterFormValues = z.infer<typeof recruitmentApplicationsFilterFormSchema>;

export const defaultRecruitmentApplicationsFilter: RecruitmentApplicationsFilterFormValues = {
  orderBy: 'createdAt_DESC'
};
