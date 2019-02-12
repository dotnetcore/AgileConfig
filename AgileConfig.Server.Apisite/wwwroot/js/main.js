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
        templateUrl: '/home/getview?viewName=Apps',
        controller: 'appsCtrl'
    };
    let listAppState = {
        url: '/list',
        templateUrl: '/home/getview?viewName=App_List',
        controller: 'listAppCtrl'
    };
    let addAppState = {
        url: '/add',
        templateUrl: '/home/getview?viewName=App_Add',
        controller: 'addAppCtrl'
    };

    let configState = {
        url: '/config',
        templateUrl: '/home/getview?viewName=config',
        controller: 'configCtrl'
    };

    $stateProvider.state('dash', homeState);

    $stateProvider.state('apps', appsState);
    $stateProvider.state('apps.list', listAppState);
    $stateProvider.state('apps.add', addAppState);

    $stateProvider.state('config',configState);

    $urlRouterProvider.otherwise("/dash");
    }
);
