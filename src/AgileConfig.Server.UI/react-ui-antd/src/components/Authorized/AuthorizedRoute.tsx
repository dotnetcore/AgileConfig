import { Navigate } from '@umijs/max';

import React from 'react';
import Authorized from './Authorized';
import type { IAuthorityType } from './CheckPermissions';

type AuthorizedRouteProps = {
  currentAuthority?: string;
  component?: React.ComponentType<any>;
  render?: (props: any) => React.ReactNode;
  redirectPath: string;
  authority: IAuthorityType;
  [key: string]: unknown;
};

const AuthorizedRoute: React.FC<AuthorizedRouteProps> = ({
  component: Component,
  render,
  authority,
  redirectPath,
  ...rest
}) => (
  <Authorized
    authority={authority}
    noMatch={<Navigate to={redirectPath} replace />}
  >
    {Component ? <Component {...rest} /> : render ? render(rest) : null}
  </Authorized>
);

export default AuthorizedRoute;
