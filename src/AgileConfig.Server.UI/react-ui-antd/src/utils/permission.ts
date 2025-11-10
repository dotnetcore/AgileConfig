import React, { ReactNode, ReactElement } from 'react';
import { hasFunction } from './authority';

// Hook: check a single function permission key.
export function useFunction(fnKey: string): boolean {
  return hasFunction(fnKey);
}

// Props for RequireFunction component
export interface RequireFunctionProps {
  fn: string;
  fallback?: ReactNode;
  children?: ReactNode;
}

// Component without JSX fragments (compatible with .ts)
export const RequireFunction: React.FC<RequireFunctionProps> = ({ fn, fallback = null, children }): ReactElement | null => {
  const allowed = useFunction(fn);
  const safeChildren: ReactNode = children === undefined ? null : children;
  const safeFallback: ReactNode = fallback === undefined ? null : fallback;
  return allowed
    ? React.createElement(React.Fragment, null, safeChildren)
    : React.createElement(React.Fragment, null, safeFallback);
};

// ANY logic
export function hasAnyFunction(...fnKeys: string[]): boolean {
  return fnKeys.some(k => hasFunction(k));
}

// ALL logic
export function hasAllFunctions(...fnKeys: string[]): boolean {
  return fnKeys.every(k => hasFunction(k));
}
