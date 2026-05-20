'use client';

import { Alert, ApiQueryBoundary, Button, FormFieldsSkeleton, FormSubmitButton, InputField, TextareaField } from '@/components';
import { FormProvider, useFormContext } from '@/context';
import {
    jobFormSchema,
    useCloseJobMutation,
    useJobQuery,
    useUpdateJobMutation,
    type JobFormValues
} from '@/services';
import { getApiErrorMessage, startRouterTransition } from '@/utils';
import { useParams, useRouter } from 'next/navigation';
import { useMemo, useState } from 'react';

function SaveJobButton() {
  const { submitting } = useFormContext();
  return <FormSubmitButton variant="primary">{submitting ? 'Salvando...' : 'Salvar'}</FormSubmitButton>;
}

export function RecruitmentEditJobPage() {
  const router = useRouter();
  const params = useParams<{ id: string }>();
  const jobId = useMemo(() => Number(params.id), [params.id]);
  const { data: job, isPending, isError, error, refetch } = useJobQuery(jobId);
  const updateMutation = useUpdateJobMutation();
  const closeMutation = useCloseJobMutation();
  const [apiError, setApiError] = useState<string | null>(null);

  const initial = useMemo<JobFormValues | null>(() => {
    if (!job) return null;
    return {
      title: job.title,
      description: job.description ?? '',
      location: job.location ?? ''
    };
  }, [job]);

  async function handleSubmit(data: JobFormValues) {
    setApiError(null);
    updateMutation.mutate(
      {
        id: jobId,
        dto: {
          title: data.title,
          description: data.description.trim() || null,
          location: data.location.trim() || null
        }
      },
      {
        onSuccess: () => startRouterTransition(() => router.push('/recrutamento/vagas')),
        onError: (err) => setApiError(getApiErrorMessage(err, 'vaga'))
      }
    );
  }

  function onClose() {
    setApiError(null);
    closeMutation.mutate(
      { id: jobId },
      {
        onSuccess: () => startRouterTransition(() => router.push('/recrutamento/vagas')),
        onError: (err) => setApiError(getApiErrorMessage(err, 'vaga'))
      }
    );
  }

  const isMutating = updateMutation.isPending || closeMutation.isPending;

  return (
    <ApiQueryBoundary
      fallback="vaga"
      isPending={isPending}
      isError={isError}
      error={error}
      resource="vaga"
      onRetry={() => void refetch()}
    >
      <section>
        <h1>Editar vaga</h1>
        {apiError ? (
          <Alert variant="destructive" title="Erro">
            {apiError}
          </Alert>
        ) : null}
        {isPending ? (
          <FormFieldsSkeleton fields={8} />
        ) : initial ? (
          <FormProvider
            key={`job-${jobId}`}
            validationSchema={jobFormSchema}
            defaultValues={initial}
            onSubmit={handleSubmit}
          >
            <div style={{ display: 'grid', gap: 12, maxWidth: 640, marginTop: 12 }}>
              <InputField name="title" label="Título" required />
              <InputField name="location" label="Localização" />
              <TextareaField name="description" label="Descrição" rows={5} />
              <div style={{ display: 'flex', gap: 10, flexWrap: 'wrap' }}>
                <SaveJobButton />
                <Button type="button" onClick={onClose} disabled={isMutating}>
                  Encerrar vaga
                </Button>
              </div>
            </div>
          </FormProvider>
        ) : null}
      </section>
    </ApiQueryBoundary>
  );
}
