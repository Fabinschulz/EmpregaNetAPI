'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { Alert, FormSubmitButton, InputField, TextareaField } from '@/components';
import { FormProvider, useFormContext } from '@/context';
import { jobFormSchema, useCreateJobMutation, type JobFormValues } from '@/services';
import { getApiErrorMessage } from '@/utils/helpers';
import { startRouterTransition } from '@/utils/lib';

const jobEmpty: JobFormValues = {
  title: '',
  description: '',
  location: ''
};

function SaveJobButton({ label }: { label: string }) {
  const { submitting } = useFormContext();
  return <FormSubmitButton variant="primary">{submitting ? 'Salvando...' : label}</FormSubmitButton>;
}

export function RecruitmentNewJobPage() {
  const router = useRouter();
  const createMutation = useCreateJobMutation();
  const [apiError, setApiError] = useState<string | null>(null);

  async function handleSubmit(data: JobFormValues) {
    setApiError(null);
    createMutation.mutate(
      {
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

  return (
    <div>
      <h1>Nova vaga</h1>
      {apiError ? (
        <Alert variant="destructive" title="Erro">
          {apiError}
        </Alert>
      ) : null}

      <FormProvider validationSchema={jobFormSchema} defaultValues={jobEmpty} onSubmit={handleSubmit}>
        <div style={{ display: 'grid', gap: 12, maxWidth: 640, marginTop: 12 }}>
          <InputField name="title" label="Título" required />
          <InputField name="location" label="Localização" />
          <TextareaField name="description" label="Descrição" rows={5} />
          <SaveJobButton label="Criar vaga" />
        </div>
      </FormProvider>
    </div>
  );
}
