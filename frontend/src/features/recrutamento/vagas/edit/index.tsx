'use client';

import {
  actionIcons,
  Alert,
  ApiQueryBoundary,
  Button,
  entityIcons,
  FormFieldsSkeleton,
  FormHeader,
  FormNotice,
  PageHeader
} from '@/shared/components';
import { FormProvider } from '@/shared/context';
import Link from 'next/link';
import { useParams } from 'next/navigation';
import { useMemo } from 'react';
import { JobFormFields, defaultFormJob, jobFormSchema, jobFormValuesFromResponse, type JobFormValues } from '../form';
import { jobsRoutes } from '../jobs-routes';
import { useCloseJobMutation, useJobQuery, useUpdateJobMutation } from '../service';

const HEADING = {
  title: 'Editar vaga',
  description: 'Atualize os dados ou encerre a vaga.'
} as const;

export function RecruitmentEditJobPage() {
  const params = useParams<{ id: string }>();
  const jobId = useMemo(() => Number(params.id), [params.id]);
  const { data: job, isPending, isError, error, refetch } = useJobQuery(jobId);
  const { apiError: updateApiError, mutate: update, isPending: isUpdating } = useUpdateJobMutation(jobId);
  const { apiError: closeApiError, mutate: close, isPending: isClosing } = useCloseJobMutation(jobId);
  const apiError = updateApiError ?? closeApiError;

  const initial = useMemo<JobFormValues>(() => {
    if (!job) return defaultFormJob;
    return jobFormValuesFromResponse(job);
  }, [job]);

  const handleSubmit = (formValue: JobFormValues) => update(formValue);

  return (
    <ApiQueryBoundary
      fallback="vaga"
      isPending={isPending}
      isError={isError}
      error={error}
      resource="vaga"
      onRetry={() => void refetch()}
    >
      {isPending ? (
        <section>
          <PageHeader {...HEADING} />
          <FormFieldsSkeleton fields={8} />
        </section>
      ) : (
        <FormProvider
          key={`job-${jobId}`}
          validationSchema={jobFormSchema}
          defaultValues={initial}
          onSubmit={handleSubmit}
        >
          <FormHeader {...HEADING} backHref={jobsRoutes.list} submitLabel="Salvar">
            <Button variant="outline" asChild>
              <Link href={jobsRoutes.candidates(jobId)}>
                <entityIcons.candidates aria-hidden />
                Ver candidatos
              </Link>
            </Button>
            <Button
              type="button"
              startIcon={actionIcons.archive}
              onClick={() => close()}
              disabled={isUpdating || isClosing}
            >
              Encerrar vaga
            </Button>
          </FormHeader>
          {apiError ? (
            <FormNotice>
              <Alert variant="destructive" title="Erro">
                {apiError}
              </Alert>
            </FormNotice>
          ) : null}
          <JobFormFields />
        </FormProvider>
      )}
    </ApiQueryBoundary>
  );
}
