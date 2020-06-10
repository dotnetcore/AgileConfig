app.controller('addConfigCtrl', function ($scope, $http, $state, $window) {
    $scope.config = {
        group: ''
    };
    $scope.apps = [];

    $http.get('/app/all?_=' + (new Date).getTime())
        .then(r => {
            if (r.data.success) {
                $scope.apps = r.data.data;
            } else {
                alert(r.data.message);
            }
        }, err => {
            console.log(err);
            alert(err.statusText);
        });

    $scope.save = function () {
        $http.post('/config/add', $scope.config)
            .then(r => {
                if (r.data.success) {
                    alert('新建配置成功。');
                    let cr = confirm('是否继续新建配置？');
                    if (!cr) {
                        $state.go('config.list', {
                            app_id: $scope.config.appId
                        });
                    } else {
                        let lastAppId = $scope.config.appId;
                        $scope.config = {
                            group: '',
                            appId: lastAppId
                        };
                    }
                    //$window.history.back();
                } else {
                    $scope.error_message = r.data.message;
                }
            }, err => {
                console.log(err);
                alert(err.statusText);
            });
    };

    $scope.cancel = function () {
        $window.history.back();
    };
});