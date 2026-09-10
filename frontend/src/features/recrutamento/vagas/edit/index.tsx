'use client';

import {
    actionIcons,
    Alert,
    ApiQueryBoundary,
    Button,
    ConfirmDialog,
    entityIcons,
    FormFieldsSkeleton,
    FormHeader,
    FormNotice,
    PageHeader,
    StatusBadge
} from '@/shared/components';
import { FormProvider } from '@/shared/context';
import Link from 'next/link';
import { useParams } from 'next/navigation';
import { useMemo, useState } from 'react';
import {
    closeJobDialogCopy,
    describeCloseJobConfirmation,
    jobStatusLabel,
    type OpenApplicationsCount
} from '../close-job-copy';
import { defaultFormJob, JobFormFields, jobFormSchema, jobFormValuesFromResponse, type JobFormValues } from '../form';
import { jobsRoutes } from '../jobs-routes';
import { useCloseJobMutation, useJobQuery, useOpenApplicationsCountQuery, useUpdateJobMutation } from '../service';

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
  const [isConfirmingClose, setIsConfirmingClose] = useState(false);
  const {
    data: openApplicationsCount,
    isPending: isCountingOpenApplications,
    isError: isCountUnavailable
  } = useOpenApplicationsCountQuery(jobId, { enabled: isConfirmingClose });

  const initial = useMemo<JobFormValues>(() => {
    if (!job) return defaultFormJob;
    return jobFormValuesFromResponse(job);
  }, [job]);

  const handleSubmit = (formValue: JobFormValues) => update(formValue);

  const openApplications: OpenApplicationsCount = isCountUnavailable
    ? { status: 'unavailable' }
    : openApplicationsCount === undefined
      ? { status: 'counting' }
      : { status: 'ready', count: openApplicationsCount };

  const isJobActive = job?.isActive ?? false;

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
          <FormHeader
            title={HEADING.title}
            description={
              <>
                {HEADING.description}{' '}
                <StatusBadge label={jobStatusLabel(isJobActive)} tone={isJobActive ? 'positive' : 'negative'} />
              </>
            }
            backHref={jobsRoutes.list}
            submitLabel="Salvar"
          >
            <Button variant="outline" asChild>
              <Link href={jobsRoutes.candidates(jobId)}>
                <entityIcons.candidates aria-hidden />
                Ver candidatos
              </Link>
            </Button>
            {isJobActive ? (
              <Button
                type="button"
                startIcon={actionIcons.archive}
                onClick={() => setIsConfirmingClose(true)}
                disabled={isUpdating || isClosing}
              >
                Encerrar vaga
              </Button>
            ) : null}
          </FormHeader>
          {apiError ? (
            <FormNotice>
              <Alert variant="destructive" title="Erro">
                {apiError}
              </Alert>
            </FormNotice>
          ) : null}
          {!isJobActive ? (
            <FormNotice>
              <Alert title="Vaga encerrada">
                Esta vaga saiu do feed público e não recebe candidaturas. O encerramento não tem retorno: a vaga não
                pode ser reativada.
              </Alert>
            </FormNotice>
          ) : null}
          <JobFormFields />

          <ConfirmDialog
            open={isConfirmingClose}
            onOpenChange={setIsConfirmingClose}
            title={closeJobDialogCopy.title}
            description={describeCloseJobConfirmation(openApplications)}
            confirmLabel={closeJobDialogCopy.confirmLabel}
            cancelLabel={closeJobDialogCopy.cancelLabel}
            confirmIcon={actionIcons.archive}
            tone="destructive"
            loading={isClosing || (isConfirmingClose && isCountingOpenApplications)}
            onConfirm={() => close(undefined, { onSettled: () => setIsConfirmingClose(false) })}
          />
        </FormProvider>
      )}
    </ApiQueryBoundary>
  );
}
