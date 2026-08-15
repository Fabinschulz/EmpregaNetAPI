/**
 * Atributos que o dono de um campo liga ao elemento focável de um controle composto
 * (select, combobox), onde não há um `<input>` nativo para receber `id`/`aria-*`.
 */
export type ControlTriggerProps = {
  id?: string;
  name?: string;
  'aria-label'?: string;
  'aria-labelledby'?: string;
  'aria-describedby'?: string;
  'aria-invalid'?: boolean;
  'data-testid'?: string;
};
