import React from 'react';
import CheckPermissions from './CheckPermissions';

/** Default behavior denies access to every page; default authority is "NULL". */
const Exception403 = () => 403;

export const isComponentClass = (component: React.ComponentClass | React.ReactNode): boolean => {
  if (!component) return false;
  const proto = Object.getPrototypeOf(component);
  if (proto === React.Component || proto === Function.prototype) return true;
  return isComponentClass(proto);
};

// Determine whether the incoming component has been instantiated
// AuthorizedRoute is already instantiated
// Authorized  render is already instantiated, children is no instantiated
// Secured is not instantiated
const checkIsInstantiation = (target: React.ComponentClass | React.ReactNode) => {
  if (isComponentClass(target)) {
    const Target = target as React.ComponentClass;
    return (props: any) => <Target {...props} />;
  }
  if (React.isValidElement(target)) {
    return (props: any) => React.cloneElement(target, props);
  }
  return () => target;
};

/**
 * Determine whether the current user has permission to access the view.
 * The authority parameter supports string, () => boolean, or Promise values.
 * For example, 'user' means only user role can access; 'user,admin' allows both.
 * When a function is provided, returning true grants access; false denies it.
 * When a promise is provided, resolve grants access and reject denies access.
 *
 * @param {string | function | Promise} authority
 * @param {ReactNode} error Optional component to render when access is denied.
 */
const authorize = (authority: string, error?: React.ReactNode) => {
  /**
   * Convert to a class component to avoid missing staticContext errors when passing strings.
   */
  let classError: boolean | React.FunctionComponent = false;
  if (error) {
    classError = (() => error) as React.FunctionComponent;
  }
  if (!authority) {
    throw new Error('authority is required');
  }
  return function decideAuthority(target: React.ComponentClass | React.ReactNode) {
    const component = CheckPermissions(authority, target, classError || Exception403);
    return checkIsInstantiation(component);
  };
};

export default authorize;
