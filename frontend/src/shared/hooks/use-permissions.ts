'use client';

import { useAuth } from '@/context';
import { can, type Permission } from '@/utils/lib';
import { useCallback } from 'react';

export function usePermissions() {
  const { roles } = useAuth();

  const check = useCallback((permission: Permission) => can(roles, permission), [roles]);

  return { can: check };
}
