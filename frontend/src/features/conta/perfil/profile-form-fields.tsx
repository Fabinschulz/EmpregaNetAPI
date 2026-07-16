'use client';

import { FormSubmitButton, InputField } from '@/components';
import { Save } from 'lucide-react';
import styles from '../conta.module.scss';

export function ProfileFormFields() {
  return (
    <>
      <div className={styles.formGrid}>
        <InputField name="username" label="Nome de usuário" required />
        <InputField name="email" label="E-mail" type="email" required />
        <InputField name="phoneNumber" label="Telefone" placeholder="(00) 90000-0000" className={styles.fieldFull} />
      </div>
      <div className={styles.formFooter}>
        <FormSubmitButton variant="primary">
          <Save aria-hidden />
          Salvar alterações
        </FormSubmitButton>
      </div>
    </>
  );
}
