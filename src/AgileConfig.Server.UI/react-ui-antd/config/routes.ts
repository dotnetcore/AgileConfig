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
          }
        ],
      },
      {
        path: '/oidc',
        component: '../layouts/BlankLayout',
        routes: [
          {
            name: 'login',
            path: '/oidc/login',
            component: './OIDC',
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
            routes: [
              { path: '/', redirect: '/home' },
              { path: '/index.html', redirect: '/home' },
              {
                name: 'home', icon: 'Dashboard', path: '/home', component: './Home',
                category: 'Application', functionKey: 'APP_READ'
              },
              {
                name: 'list.node-list', icon: 'Database', path: '/node', component: './Nodes',
                category: 'Node', functionKey: 'NODE_READ'
              },
              {
                name: 'list.app-list', icon: 'Appstore', path: '/app', component: './Apps',
                category: 'Application', functionKey: 'APP_READ'
              },
              {
                name: 'list.config-list', icon: 'Table', path: '/app/config/:app_id/:app_name', component: './Configs',
                hideInMenu: true, category: 'Configuration', functionKey: 'CONFIG_READ'
              },
              {
                name: 'list.client-list', icon: 'Shrink', path: '/client', component: './Clients',
                category: 'Client', functionKey: 'CLIENT_REFRESH'
              },
              {
                name: 'list.service-list', icon: 'Cloud', path: '/service', component: './Services',
                category: 'Service', functionKey: 'SERVICE_READ'
              },
              {
                name: 'list.user-list', icon: 'User', path: '/users', component: './User',
                category: 'User', functionKey: 'USER_READ'
              },
              {
                name: 'list.role-list', icon: 'SafetyCertificate', path: '/roles', component: './Role',
                category: 'Role', functionKey: 'ROLE_READ'
              },
              {
                name: 'list.logs-list', icon: 'Bars', path: '/logs', component: './Logs',
                category: 'Log', functionKey: 'LOG_READ'
              },
              { component: './404' },
            ],
          },
          { component: './404' },
        ],
      },
    ],
  },
  { component: './404' },
];
