app.controller('configCtrl', function ($scope, $state) {
    $state.go('config.list');
});

app.controller('listConfigCtrl', function ($scope, $http, $state) {
    $scope.configs = [];
    $scope.apps = [];
    $scope.selectedAppId = '';

    $scope.toAdd = function () {
        $state.go('config.add');
    };

    $scope.toEdit = function (config) {
        $state.go('config.edit', {
            config_id: config.id
        });
    };

    $scope.deleteConfig = function (config) {
        let cr = confirm('是否确定删除' + `配置【${config.key}】?`);
        if (!cr) {
            return;
        }

        $http.post('/config/delete?id=' + config.id)
            .then(r => {
                if (r.data.success) {
                    config.status = 0;
                } else {
                    alert(r.data.message);
                }
            }, err => {
                console.log(err);
                alert(err.statusText);
            });
    };
    $http.get('/app/all?_=' + (new Date).getTime())
        .then(
            r => {
                if (r.data.success) {
                    $scope.apps = r.data.data;
                } else {
                    $scope.apps = [];
                    alert(r.data.message);
                }
            },
            err => {
                console.log(err);
                alert(err.statusText);
            }
    );

    $http.get('/config/all?_=' + (new Date).getTime())
        .then(
            r => {
                if (r.data.success) {
                    $scope.configs = r.data.data;
                } else {
                    $scope.configs = [];
                    alert(r.data.message);
                }
            },
            err => {
                console.log(err);
                alert(err.statusText);
            }
        );
});