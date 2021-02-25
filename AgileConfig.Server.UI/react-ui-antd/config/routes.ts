export default [
  {
    path: '/',
    component: '../layouts/BlankLayout',
    routes: [
      {
        path: '/user',
        component: '../layouts/UserLayout',
        routes: [
          {
            name: 'login',
            path: '/user/login',
            component: './User/login',
          },
        ],
      },
      {
        path: '/',
        component: '../layouts/SecurityLayout',
        routes: [
          {
            path: '/',
            component: '../layouts/BasicLayout',
            authority: ['admin', 'user'],
            routes: [
              {
                path: '/',
                redirect: '/welcome',
              },
              {
                path: '/welcome',
                name: 'welcome',
                icon: 'DashboardOutlined',
                component: './Welcome',
              },
              {
                path: '/admin',
                name: 'admin',
                icon: 'crown',
                component: './Admin',
                authority: ['admin'],
                routes: [
                  {
                    path: '/admin/sub-page',
                    name: 'sub-page',
                    icon: 'smile',
                    component: './Welcome',
                    authority: ['admin'],
                  },
                ],
              },
              {
                name: 'list.table-list',
                icon: 'AppstoreOutlined',
                path: '/list',
                component: './TableList',
              },
              {
                name: 'list.node-list',
                icon: 'Database',
                path: '/node',
                component: './Nodes',
              },
              {
                name: 'list.app-list',
                icon: 'Appstore',
                path: '/app',
                component: './Apps',
              },
              {
                name: 'list.config-list',
                icon: 'Table',
                path: '/app/config/:app_id/:app_name',
                component: './Configs',
                hideInMenu: true,
              },
              {
                name: 'list.client-list',
                icon: 'Shrink',
                path: '/client',
                component: './Clients',
              },
              {
                name: 'list.logs-list',
                icon: 'Bars',
                path: '/logs',
                component: './Logs',
              },
              {
                component: './404',
              },
            ],
          },
          {
            component: './404',
          },
        ],
      },
    ],
  },
  {
    component: './404',
  },
];
