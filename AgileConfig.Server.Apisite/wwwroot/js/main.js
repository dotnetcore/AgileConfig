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
        url: '/list',
        templateUrl: '/home/getview?viewName=config_list',
        controller: 'listConfigCtrl'
    };
    let addConfigState = {
        url: '/add',
        templateUrl: '/home/getview?viewName=config_add',
        controller: 'addConfigCtrl'
    };
    let editConfigState = {
        url: '/edit/:config_id',
        templateUrl: '/home/getview?viewName=config_edit',
        controller: 'editConfigCtrl'
    };

    let clientsState = {
        url: '/clients',
        templateUrl: '/home/getview?viewName=clients',
        controller: 'clientsCtrl'
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

    $stateProvider.state('clients', clientsState);

    $urlRouterProvider.otherwise("/dash");
    }
);
