import React, { ReactNode, ReactElement } from 'react';
import { checkUserPermission } from '@/components/Authorized/AuthorizedElement';
import { getFunctions } from './authority';

// Hook: check a single function permission key.
export function useFunction(fnKey: string, appId?: string): boolean {
  return checkUserPermission(getFunctions(), fnKey, appId);
}

// Props for RequireFunction component
export interface RequireFunctionProps {
  fn: string;
  fallback?: ReactNode;
  children?: ReactNode;
  appId?: string;
  extraCheck?: () => boolean;
}

// Component without JSX fragments (compatible with .ts)
export const RequireFunction: React.FC<RequireFunctionProps> = ({ fn, fallback = null, children, appId, extraCheck }): ReactElement | null => {
  const allowed = useFunction(fn, appId) && (!extraCheck || extraCheck());
  const safeChildren: ReactNode = children === undefined ? null : children;
  const safeFallback: ReactNode = fallback === undefined ? null : fallback;
  return allowed
    ? React.createElement(React.Fragment, null, safeChildren)
    : React.createElement(React.Fragment, null, safeFallback);
};

// ANY logic
export function hasAnyFunction(...fnKeys: string[]): boolean {
  return fnKeys.some(k => useFunction(k));
}

// ALL logic
export function hasAllFunctions(...fnKeys: string[]): boolean {
  return fnKeys.every(k => useFunction(k));
}
