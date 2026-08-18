'use client';

import { useAuth } from '@/shared/context';
import { can, type Permission } from '@/shared/utils';
import { useCallback } from 'react';

export function usePermissions() {
  const { roles } = useAuth();

  const check = useCallback((permission: Permission) => can(roles, permission), [roles]);

  return { can: check };
}
