import type { MenuDataItem, ProLayoutProps } from '@ant-design/pro-components';
import { getMenuData, getPageTitle } from '@ant-design/pro-components';
import { Helmet, HelmetProvider } from 'react-helmet-async';
import type { ConnectProps } from '@umijs/max';
import { Link, Outlet, SelectLang, useIntl, connect, FormattedMessage } from '@umijs/max';
import React from 'react';
import type { ConnectState } from '@/models/connect';
import styles from './UserLayout.less';
import LayoutFooter from './compos/LayoutFooter';

type UserLayoutRoute = Parameters<typeof getMenuData>[0][number];
type UserLayoutLocation = NonNullable<ProLayoutProps['location']>;

export type UserLayoutProps = {
  breadcrumbNameMap: Record<string, MenuDataItem>;
  children?: React.ReactNode;
  route?: UserLayoutRoute;
  location?: UserLayoutLocation;
} & Partial<ConnectProps>;

const UserLayout: React.FC<UserLayoutProps> = (props) => {
  const {
    route = {
      routes: [],
    },
  } = props;
  const { routes = [] } = route;
  const {
    children,
    location = {
      pathname: '',
    },
  } = props;
  const { formatMessage } = useIntl();
  const { breadcrumb } = getMenuData(routes);
  const title = getPageTitle({
    pathname: location.pathname,
    formatMessage,
    breadcrumb,
    ...props,
  });
  return (
    <HelmetProvider>
      <Helmet>
        <title>{title}</title>
      </Helmet>

      <div className={styles.container}>
        <div className={styles.lang}>
          <SelectLang  />
        </div>
        <div className={styles.content}>
          <div className={styles.top}>
            <div className={styles.header}>
              <Link to="/">
                <span className={styles.title}>AgileConfig</span>
              </Link>
            </div>
            <div className={styles.desc}>
              <FormattedMessage
                id="pages.layouts.userLayout.title"
                defaultMessage="AgileConfig"
              />
            </div>
          </div>
          {children ?? <Outlet />}
        </div>
        <LayoutFooter />
      </div>
    </HelmetProvider>
  );
};

export default connect(({ settings }: ConnectState) => ({ ...settings }))(UserLayout);
