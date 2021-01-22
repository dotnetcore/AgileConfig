app.controller('addConfigCtrl', function ($scope, $http, $state, $window, msg) {
    $scope.config = {
        group: ''
    };
    $scope.apps = [];

    $http.get('/app/all?_=' + (new Date).getTime())
        .then(r => {
            if (r.data.success) {
                $scope.apps = r.data.data;
            } else {
                msg.success(r.data.message);
            }
        }, err => {
            console.log(err);
            msg.fail(err.statusText);
        });

    $scope.save = function () {
        $http.post('/config/add', $scope.config)
            .then(r => {
                if (r.data.success) {
                    let index = msg.confirm('新建配置成功, 是否继续新建配置？', ['继续新建', '完成'], () => {
                        msg.clear(index);
                        $scope.$apply(() => {
                            let lastAppId = $scope.config.appId;
                            $scope.config = {
                                group: '',
                                appId: lastAppId
                            };
                        });
                    }, () => {
                        msg.clear(index);
                        $scope.$apply(() => {
                            $state.go('config.list', {
                                app_id: $scope.config.appId
                            });
                        });
                    })
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
        $window.history.back();
    };
});