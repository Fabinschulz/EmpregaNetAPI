'use client';

import { useAuth } from '@/context';
import { useMutation, useQuery } from '@tanstack/react-query';
import { queryKeys } from '../query-keys';
import { requireAuthToken } from '../shared';
import { login, me, register } from './users-public-api';
import type { LoginDto, RegisterDto } from './users-schema';

export function useMeQuery() {
  const { token } = useAuth();

  return useQuery({
    queryKey: queryKeys.users.me(),
    queryFn: () => me(requireAuthToken(token)),
    enabled: !!token
  });
}

export function useLoginMutation() {
  return useMutation({
    mutationFn: (dto: LoginDto) => login(dto)
  });
}

export function useRegisterMutation() {
  return useMutation({
    mutationFn: (dto: RegisterDto) => register(dto)
  });
}
