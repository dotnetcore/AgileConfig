app.controller('addConfigCtrl', function ($scope, $http, $state) {
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
                    $state.go('config.list');
                } else {
                    $scope.error_message = r.data.message;
                }
            }, err => {
                console.log(err);
                alert(err.statusText);
            });
    };

    $scope.cancel = function () {
        $state.go('config.list');
    };
});