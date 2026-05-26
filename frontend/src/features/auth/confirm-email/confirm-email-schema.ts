import { confirmEmailSchema, type ConfirmEmailDto } from '@/services';

export { confirmEmailSchema, type ConfirmEmailDto };

export function confirmEmailDefaultValues(userId: number, token: string): ConfirmEmailDto {
  return { userId, token };
}
