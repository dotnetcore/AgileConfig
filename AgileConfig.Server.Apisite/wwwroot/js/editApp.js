app.controller('editAppCtrl', function ($scope, $http, $state, $stateParams, $window, msg) {
    let id = $stateParams.app_id;
    $scope.app = {
    };
    var getAllInheritancedApps = function () {
        $http.get('/app/InheritancedApps?currentAppId=' + $scope.app.id + '&_=' + new Date().getTime())
            .then(r => {
                if (r.data.success) {
                    $scope.inheritancedApps = r.data.data;
                }
            }, err => {
                console.log(err);
                alert(err.statusText);
            });
    }
    $http.get('/app/get?id=' + id + '&_=' + (new Date).getTime())
        .then(
            r => {
                if (r.data.success) {
                    $scope.app = r.data.data;
                    getAllInheritancedApps();
                } else {
                    alert(r.data.message);
                }
            }, err => {
                console.log(err);
                alert(err.statusText);
            });
    $scope.save = function () {
        $http.post('/app/edit', $scope.app)
            .then(r => {
                if (r.data.success) {
                    msg.success('修改应用成功。');
                    $state.go('apps.list');
                } else {
                    $scope.error_message = r.data.message;
                }
            }, err => {
                console.log(err);
                alert(err.statusText);
            });
    };

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