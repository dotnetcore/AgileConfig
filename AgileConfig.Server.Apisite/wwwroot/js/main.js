const app = angular.module('app',
    ['ui.router',
        'angular-loading-bar',
        'agile.bootstrap-pagebar',
        'ngCookies', 'ngAnimate']);
app.config(function ($stateProvider, $urlRouterProvider) {
    let homeState = {
        url: '/dash',
        templateUrl: '/home/getview?viewName=dashbroad',
        controller: 'dashCtrl'
    };
    
    let appsState = {
        url: '/apps',
        templateUrl: '/home/getview?viewName=apps',
        controller: 'appsCtrl'
    };
    let listAppState = {
        url: '/list',
        templateUrl: '/home/getview?viewName=app_list',
        controller: 'listAppCtrl'
    };
    let addAppState = {
        url: '/add',
        templateUrl: '/home/getview?viewName=app_add',
        controller: 'addAppCtrl'
    };
    let editAppState = {
        url: '/edit/:app_id',
        templateUrl: '/home/getview?viewName=app_edit',
        controller: 'editAppCtrl'
    };

    let configState = {
        url: '/config',
        templateUrl: '/home/getview?viewName=config',
        controller: 'configCtrl'
    };
    let listConfigState = {
        url: '/list/:app_id',
        templateUrl: '/home/getview?viewName=config_list',
        controller: 'listConfigCtrl'
    };
    let addConfigState = {
        url: '/add',
        templateUrl: '/home/getview?viewName=config_add',
        controller: 'addConfigCtrl'
    };
    let jsonConfigState = {
        url: '/json/:app_id',
        templateUrl: '/home/getview?viewName=config_json',
        controller: 'jsonConfigCtrl'
    };
    let editConfigState = {
        url: '/edit/:config_id',
        templateUrl: '/home/getview?viewName=config_edit',
        controller: 'editConfigCtrl'
    };
    let modifyLogsState = {
        url: '/logs/:config_id',
        templateUrl: '/home/getview?viewName=config_logs',
        controller: 'modifyLogsCtrl'
    };
    let clientsState = {
        url: '/clients',
        templateUrl: '/home/getview?viewName=clients',
        controller: 'clientsCtrl'
    };
    let nodesState = {
        url: '/nodes',
        templateUrl: '/home/getview?viewName=node',
        controller: 'nodesCtrl'
    };
    let listNodesState = {
        url: '/list',
        templateUrl: '/home/getview?viewName=node_list',
        controller: 'ListnodeCtrl'
    };
    let addNodesState = {
        url: '/add',
        templateUrl: '/home/getview?viewName=node_add',
        controller: 'addNodeCtrl'
    };
    let syslogsState = {
        url: '/syslogs',
        templateUrl: '/home/getview?viewName=syslogs',
        controller: 'syslogsCtrl'
    };

    $stateProvider.state('dash', homeState);

    $stateProvider.state('apps', appsState);
    $stateProvider.state('apps.list', listAppState);
    $stateProvider.state('apps.add', addAppState);
    $stateProvider.state('apps.edit', editAppState);

    $stateProvider.state('config',configState);
    $stateProvider.state('config.list', listConfigState);
    $stateProvider.state('config.add', addConfigState);
    $stateProvider.state('config.edit', editConfigState);
    $stateProvider.state('config.logs', modifyLogsState);
    $stateProvider.state('config.json', jsonConfigState);

    $stateProvider.state('clients', clientsState);

    $stateProvider.state('nodes', nodesState);
    $stateProvider.state('nodes.list', listNodesState);
    $stateProvider.state('nodes.add', addNodesState);

    $stateProvider.state('syslogs', syslogsState);

    $urlRouterProvider.otherwise("/dash");
    }
);
