app.controller('editConfigCtrl', function ($scope, $http, $state, $stateParams, msg) {
    let id = $stateParams.config_id;
    $scope.config = {
    };
    $scope.apps = [];

    $http.get('/app/all?_=' + (new Date).getTime())
        .then(r => {
            if (r.data.success) {
                $scope.apps = r.data.data;
            } else {
                msg.fail(r.data.message);
            }
        }, err => {
            console.log(err)
            msg.fail(err.statusText)
        });

    $http.get('/config/get?id=' + id + '&_=' + (new Date).getTime())
        .then(
            r => {
                if (r.data.success) {
                    $scope.config = r.data.data;
                } else {
                    msg.fail(r.data.message);
                }
            }, err => {
                console.log(err);
                msg.fail(err.statusText);
            });

    $scope.save = function () {
        $http.post('/config/edit', $scope.config)
            .then(r => {
                if (r.data.success) {
                    msg.success('修改配置成功。');
                    $state.go('config.list', {
                        app_id: $scope.config.appId
                    });
                } else {
                    $scope.error_message = r.data.message;
                }
            }, err => {
                console.log(err);
                msg.fail(err.statusText);
            });
    };

    $scope.formatJson = function (config) {
        if (config.value) {
            var obj = JSON.parse(config.value);
            var json = JSON.stringify(obj, null, 4);

            config.value = json;
        }
    }

    $scope.cancel = function () {
        $state.go('config.list', {
            app_id: $scope.config.appId
        });
    };
});