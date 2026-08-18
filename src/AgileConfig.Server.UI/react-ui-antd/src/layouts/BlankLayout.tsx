import React, { PropsWithChildren } from 'react';
import { Outlet } from '@umijs/max';

const Layout: React.FC<PropsWithChildren> = ({ children }) => {
  return <>{children ?? <Outlet />}</>;
};

export default Layout;
