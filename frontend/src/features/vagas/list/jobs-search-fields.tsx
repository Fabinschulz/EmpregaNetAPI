'use client';

import { Button, FormSubmitButton, InputField } from '@/components';
import { useFormContext } from '@/context';
import { defaultJobsSearch, type JobsSearchFormValues } from '@/services';
import styles from './jobs-search-fields.module.scss';

type JobsSearchFieldsProps = {
  /** Chamado ao limpar a busca (além do reset do formulário). */
  onClear: () => void;
};

export function JobsSearchFields({ onClear }: JobsSearchFieldsProps) {
  const { reset } = useFormContext<JobsSearchFormValues>();

  const handleClear = () => {
    reset(defaultJobsSearch);
    onClear();
  };

  return (
    <div className={styles.fields}>
      <InputField name="search" label="Buscar" placeholder="Cargo, palavra-chave..." className={styles.search} />
      <div className={styles.actions}>
        <FormSubmitButton variant="primary">Buscar</FormSubmitButton>
        <Button type="button" variant="outline" onClick={handleClear}>
          Limpar
        </Button>
      </div>
    </div>
  );
}
