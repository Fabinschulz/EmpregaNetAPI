'use client';

import { Button, FormSubmitButton, InputField, SelectField } from '@/components';
import { useFormContext } from '@/context';
import { defaultJobsFilter, type JobsFilterFormValues } from '@/services';
import { Search, X } from 'lucide-react';

const STATUS_OPTIONS = [
  { label: 'Todas', value: 'all' },
  { label: 'Ativas', value: 'active' },
  { label: 'Encerradas', value: 'closed' }
];

type JobsFilterFieldsProps = {
  /** Chamado ao limpar os filtros (além do reset do formulário). */
  onClear: () => void;
};

export function JobsFilterFields({ onClear }: JobsFilterFieldsProps) {
  const { reset, submitting } = useFormContext<JobsFilterFormValues>();

  const handleClear = () => {
    reset(defaultJobsFilter);
    onClear();
  };

  return (
    <>
      <InputField name="search" label="Buscar" placeholder="Título ou descrição da vaga" />
      <SelectField name="status" label="Situação" options={STATUS_OPTIONS} />
      <div style={{ display: 'flex', gap: 8, flex: '0 0 auto' }}>
        <FormSubmitButton variant="primary">
          <Search aria-hidden />
          {submitting ? 'Buscando...' : 'Buscar'}
        </FormSubmitButton>
        <Button type="button" variant="outline" onClick={handleClear}>
          <X aria-hidden />
          Limpar
        </Button>
      </div>
    </>
  );
}
