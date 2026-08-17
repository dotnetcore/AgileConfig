import {
  MenuDataItem,
  ProLayout,
  ProLayoutProps,
} from '@ant-design/pro-components';
import {
  AppstoreOutlined,
  BarsOutlined,
  CloudOutlined,
  DashboardOutlined,
  DatabaseOutlined,
  SafetyCertificateOutlined,
  ShrinkOutlined,
  TableOutlined,
  UserOutlined,
} from '@ant-design/icons';
import React, { useEffect, useMemo, useRef, useCallback } from 'react';
import { Dispatch, getIntl, getLocale } from '@umijs/max';
import { Link, Outlet, useIntl, useLocation, connect, history } from '@umijs/max';
import { Result, Button } from 'antd';
import { getCategories } from '@/utils/authority';
import Authorized from '@/utils/Authorized';
import RightContent from '@/components/GlobalHeader/RightContent';
import type { ConnectState } from '@/models/connect';
import { getMatchMenu } from '@umijs/route-utils';
import logo from '../assets/logo.svg';
import LayoutFooter from './compos/LayoutFooter';
import type { DefaultSettings } from '../../config/defaultSettings';
import routes from '../../config/routes';

const securityRoute = routes[0].routes?.find(
  (route) => route.component === '../layouts/SecurityLayout',
);
const basicRoute = securityRoute && 'routes' in securityRoute
  ? securityRoute.routes.find((route) => route.component === '../layouts/BasicLayout')
  : undefined;
const menuIcons: Record<string, React.ReactNode> = {
  Appstore: <AppstoreOutlined />,
  Bars: <BarsOutlined />,
  Cloud: <CloudOutlined />,
  Dashboard: <DashboardOutlined />,
  Database: <DatabaseOutlined />,
  SafetyCertificate: <SafetyCertificateOutlined />,
  Shrink: <ShrinkOutlined />,
  Table: <TableOutlined />,
  User: <UserOutlined />,
};
const layoutRoute = {
  routes: basicRoute && 'routes' in basicRoute
    ? (basicRoute.routes || []).map((route) => ({
        ...route,
        icon: route.icon ? menuIcons[route.icon] : undefined,
      }))
    : [],
} as ProLayoutProps['route'];

const noMatch = (
  <Result
    status={403}
    title="403"
    subTitle="Sorry, you are not authorized to access this page."
    extra={
      <Button type="primary">
        <Link to="/user/login">Go Login</Link>
      </Button>
    }
  />
);
export type BasicLayoutProps = {
  breadcrumbNameMap: Record<string, MenuDataItem>;
  route?: ProLayoutProps['route'] & {
    authority: string[];
  };
  categories?: string[];
  settings: DefaultSettings;
  dispatch: Dispatch;
} & ProLayoutProps;
export type BasicLayoutContext = { [K in 'location']: BasicLayoutProps[K] } & {
  breadcrumbNameMap: Record<string, MenuDataItem>;
};
/** Use Authorized check all menu item */

const BasicLayout: React.FC<BasicLayoutProps> = (props) => {
  const currentLocation = useLocation();
  const {
    dispatch,
    children,
    settings,
  } = props;
  const location = props.location || currentLocation;

  // keep categories in props to force re-render when user changes; fall back to persisted storage to avoid empty menu during bootstrap
  const categories = useMemo<string[]>(() => {
    if (props.categories && props.categories.length) {
      return props.categories;
    }
    return getCategories();
  }, [props.categories]);

  // Filter menu by categories stored from login (e.g., Application, Configuration, Node, Client, User, Role, Service, System)
  const menuDataRender = useCallback(
    (menuList: MenuDataItem[]): MenuDataItem[] => {
      const cats = categories || [];
      console.log('menuDataRender categories=', cats);
      return menuList
        .filter((m) => {
          // category filter
          const category = (m as any).category;
          if (category && !cats.includes(category)) return false;

          return true;
        })
        .map((item) => {
          const localItem = {
            ...item,
            children: item.children ? menuDataRender(item.children) : undefined,
          };
          return Authorized.check(item.authority, localItem, null) as MenuDataItem;
        });
    },
    [categories],
  );

  const menuDataRef = useRef<MenuDataItem[]>([]);

  useEffect(() => {
    if (dispatch) {
      dispatch({
        type: 'user/fetchCurrent',
      });
    }
  }, []);

  /** Init variables */

  const handleMenuCollapse = (payload: boolean): void => {
    if (dispatch) {
      dispatch({
        type: 'global/changeLayoutCollapsed',
        payload,
      });
    }
  };
  // get children authority
  const authorized = useMemo(
    () =>
      getMatchMenu(location.pathname || '/', menuDataRef.current).pop() || {
        authority: undefined,
      },
    [location.pathname],
  );

  const { formatMessage } = useIntl();
  const { darkMode } = settings;

  return (
    <ProLayout
      key={`pro-layout-`}
      logo={logo}
      formatMessage={formatMessage}
      {...props}
      {...settings}
      route={props.route || layoutRoute}
      location={location}
      navTheme={darkMode ? 'realDark' : settings.navTheme}
      onCollapse={handleMenuCollapse}
      onMenuHeaderClick={() => history.push('/')}
      menuItemRender={(menuItemProps, defaultDom) => {
        if (
          menuItemProps.isUrl ||
          !menuItemProps.path ||
          location.pathname === menuItemProps.path
        ) {
          return defaultDom;
        }
        return <Link to={menuItemProps.path}>{defaultDom}</Link>;
      }}
      breadcrumbRender={(routers = []) => {
        const configRouter = routers.find((item) => item.path?.includes('/app/config'));
        if (configRouter) {
          const intl = getIntl(getLocale());
          const breadcrumbName = intl.formatMessage({
            id: 'pages.configs.breadcrumbName'
          });
          configRouter.breadcrumbName = breadcrumbName ;
          return [
            ...routers,
          ]
        }
        else {
          return [];
        }
      }}
      itemRender={(route, params, routes, paths) => {
        return  (
          <span>{route.breadcrumbName}</span>
        );
      }}
      footerRender={() => {
        return (
           <LayoutFooter></LayoutFooter>
        )
      }}
      menuDataRender={menuDataRender}
      actionsRender={() => <RightContent />}
      postMenuData={(menuData) => {
        menuDataRef.current = menuData || [];
        return menuData || [];
      }}
    >
      <Authorized authority={authorized!.authority} noMatch={noMatch}>
        {children ?? <Outlet />}
      </Authorized>
    </ProLayout>
  );
};

export default connect(({ global, settings, user }: ConnectState) => ({
  collapsed: global.collapsed,
  settings,
  categories: user.currentUser?.currentCategories,
}))(BasicLayout);
