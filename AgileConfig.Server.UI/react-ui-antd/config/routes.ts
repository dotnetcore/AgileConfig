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
          {
            name: 'initPassword',
            path: '/user/initPassword',
            component: './User/initPassword',
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
                redirect: '/home'
              },
              {
                path: '/index.html',
                redirect: '/home'
              },
              {
                name: 'home',
                icon: 'Dashboard',
                path: '/home',
                component: './Home',
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
