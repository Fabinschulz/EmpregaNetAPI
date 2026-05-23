'use client';

import { Button, FormSubmitButton, InputField, TextareaField } from '@/components';
import { useFormContext } from '@/context';
import type { CSSProperties } from 'react';

const JOB_FORM_GRID_STYLE: CSSProperties = {
  display: 'grid',
  gap: 12,
  maxWidth: 640,
  marginTop: 12
};

type JobFormFieldsProps = {
  submitLabel: string;
  onClose?: () => void;
  closeDisabled?: boolean;
};

export function JobFormFields({ submitLabel, onClose, closeDisabled }: JobFormFieldsProps) {
  const { submitting } = useFormContext();

  return (
    <div style={JOB_FORM_GRID_STYLE}>
      <InputField name="title" label="Título" required />
      <InputField name="location" label="Localização" />
      <TextareaField name="description" label="Descrição" rows={5} />
      <div style={{ display: 'flex', gap: 10, flexWrap: 'wrap' }}>
        <FormSubmitButton variant="primary">{submitting ? 'Salvando...' : submitLabel}</FormSubmitButton>
        {onClose ? (
          <Button type="button" onClick={onClose} disabled={closeDisabled}>
            Encerrar vaga
          </Button>
        ) : null}
      </div>
    </div>
  );
}
