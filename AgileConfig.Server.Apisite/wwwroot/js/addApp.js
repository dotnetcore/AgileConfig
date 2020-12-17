app.controller('addAppCtrl', function ($scope, $http, $state, $window, msg) {
    $scope.app = {
        id: '',
        name: '',
        secret: '',
        enabled: true,
        inheritancedApps: []
    };

    $scope.inheritancedApps = [];

    $scope.save = function () {
        $http.post('/app/add', $scope.app)
            .then(r => {
                if (r.data.success) {
                    msg.success('新建应用成功。');
                    $state.go('apps.list');
                } else {
                    $scope.error_message = r.data.message;
                }
            }, err => {
                console.log(err);
                msg.fail(err.statusText);
            });
    };

    var getAllInheritancedApps = function () {
        $http.get('/app/InheritancedApps?_=' + new Date().getTime())
            .then(r => {
                if (r.data.success) {
                    $scope.inheritancedApps = r.data.data;
                }
            }, err => {
                console.log(err);
                msg.fail(err.statusText);
            });
    }
    getAllInheritancedApps();

    $scope.chooseRefApp = function (app) {
        app.selected = !app.selected;
    }
    $scope.showChooseRefAppsModal = function () {
        $scope.inheritancedApps.forEach(a => {
            a.selected = false;
            var app = $scope.app.inheritancedApps.find(x => x.id === a.id);
            if (app) {
                //排除已添加的应用
                a.refed = 1;
            } else {
                a.refed = 0;
            }
        });
        $('#modal_add_refapp').modal('show');
    }
    $scope.AddSelectedRefApps = function () {
        $scope.inheritancedApps.forEach(a => {
            if (a.selected) {
                $scope.app.inheritancedApps.push(a);
            }
        });
        $('#modal_add_refapp').modal('hide');
    }
    $scope.removeSelectedRefApp = function (index) {
        $scope.app.inheritancedApps.splice(index, 1);
    }

    $scope.cancel = function () {
        $window.history.back();
    };
});