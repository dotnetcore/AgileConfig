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
                    $state.go('config.list', {
                        app_id: $scope.config.appId
                    });
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