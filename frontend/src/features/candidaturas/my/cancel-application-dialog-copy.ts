/**
 * Texto da confirmação de cancelamento na tela do candidato.
 *
 * Fica fora do componente porque é regra de produto verificável: a confirmação tem de dizer,
 * antes de executar, que o ato **não tem retorno** e o botão de recusa não pode chamar-se
 * "Cancelar", que no contexto significaria justamente cancelar a candidatura.
 */
export const cancelApplicationDialogCopy = {
  title: 'Cancelar candidatura?',
  confirmLabel: 'Cancelar candidatura',
  cancelLabel: 'Manter candidatura',
  describe: (applicationId: number): string =>
    `A candidatura #${applicationId} será cancelada e o recrutador verá que você desistiu. ` +
    'Esta ação não tem retorno: a candidatura não volta ao processo seletivo. ' +
    'Se mudar de ideia, você pode se candidatar de novo enquanto a vaga estiver ativa.'
} as const;
