import React, { ReactNode } from 'react';
import { checkUserPermission } from '@/components/Authorized/AuthorizedElement';
import { getFunctions } from './authority';

/** Hook: check single function permission key */
export function useFunction(fnKey: string, appId?: string): boolean {
  return checkUserPermission(getFunctions(), fnKey, appId);
}

interface RequireFunctionProps {
  fn: string;
  fallback?: ReactNode;
  children?: ReactNode;
  /** optional appId for app-scoped permission logic */
  appId?: string;
  /** optional predicate for additional custom check */
  extraCheck?: () => boolean;
}

/** Component: conditionally render children if user has function permission */
export const RequireFunction: React.FC<RequireFunctionProps> = ({ fn, fallback = null, children, appId, extraCheck }) => {
  const allowed = useFunction(fn, appId) && (!extraCheck || extraCheck());
  if (!allowed) return <>{fallback}</>;
  return <>{children}</>;
};

/** ANY logic */
export function hasAnyFunction(...fnKeys: string[]): boolean {
  return fnKeys.some(k => useFunction(k));
}

/** ALL logic */
export function hasAllFunctions(...fnKeys: string[]): boolean {
  return fnKeys.every(k => useFunction(k));
}
