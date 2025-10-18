import React from 'react';
import { CURRENT } from './renderAuthorize';
// eslint-disable-next-line import/no-cycle
import PromiseRender from './PromiseRender';

export type IAuthorityType =
  | undefined
  | string
  | string[]
  | Promise<boolean>
  | ((currentAuthority: string | string[]) => IAuthorityType);

/**
 * Generic permission check helper.
 *
 * @param {IAuthorityType} authority Permission definition.
 * @param {string | string[]} currentAuthority Current user permissions.
 * @param {T} target Component to render when authorized.
 * @param {K} Exception Component to render when authorization fails.
 */
const checkPermissions = <T, K>(
  authority: IAuthorityType,
  currentAuthority: string | string[],
  target: T,
  Exception: K,
): T | K | React.ReactNode => {
  // Default to allowing access when no authority is defined.
  // Retirement authority, return target;
  if (!authority) {
    return target;
  }
  // Handle array-based authority definitions.
  if (Array.isArray(authority)) {
    if (Array.isArray(currentAuthority)) {
      if (currentAuthority.some((item) => authority.includes(item))) {
        return target;
      }
    } else if (authority.includes(currentAuthority)) {
      return target;
    }
    return Exception;
  }
  // Handle string-based authority definitions.
  if (typeof authority === 'string') {
    if (Array.isArray(currentAuthority)) {
      if (currentAuthority.some((item) => authority === item)) {
        return target;
      }
    } else if (authority === currentAuthority) {
      return target;
    }
    return Exception;
  }
  // Handle promise-based authority definitions.
  if (authority instanceof Promise) {
    return <PromiseRender<T, K> ok={target} error={Exception} promise={authority} />;
  }
  // Handle function-based authority definitions.
  if (typeof authority === 'function') {
    const bool = authority(currentAuthority);
    // Handle functions that return a promise.
    if (bool instanceof Promise) {
      return <PromiseRender<T, K> ok={target} error={Exception} promise={bool} />;
    }
    if (bool) {
      return target;
    }
    return Exception;
  }
  throw new Error('unsupported parameters');
};

export { checkPermissions };

function check<T, K>(authority: IAuthorityType, target: T, Exception: K): T | K | React.ReactNode {
  return checkPermissions<T, K>(authority, CURRENT, target, Exception);
}

export default check;
