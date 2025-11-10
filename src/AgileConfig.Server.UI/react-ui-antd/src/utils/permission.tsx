import React, { ReactNode } from 'react';
import { hasFunction } from './authority';

/** Hook: check single function permission key */
export function useFunction(fnKey: string): boolean {
  return hasFunction(fnKey);
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
  const allowed = useFunction(fn) && (!extraCheck || extraCheck());
  if (!allowed) return <>{fallback}</>;
  return <>{children}</>;
};

/** ANY logic */
export function hasAnyFunction(...fnKeys: string[]): boolean {
  return fnKeys.some(k => hasFunction(k));
}

/** ALL logic */
export function hasAllFunctions(...fnKeys: string[]): boolean {
  return fnKeys.every(k => hasFunction(k));
}