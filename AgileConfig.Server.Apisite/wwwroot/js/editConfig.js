app.controller('editConfigCtrl', function ($scope, $http, $state, $stateParams) {
    let id = $stateParams.config_id;
    $scope.config = {
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
            console.log(err)
            alert(err.statusText)
        });

    $http.get('/config/get?id=' + id + '&_=' + (new Date).getTime())
        .then(
            r => {
                if (r.data.success) {
                    $scope.config = r.data.data;
                } else {
                    alert(r.data.message);
                }
            }, err => {
                console.log(err);
                alert(err.statusText);
        });

    $scope.save = function () {
        $http.post('/config/edit', $scope.config)
            .then(r => {
                if (r.data.success) {
                    alert('修改配置成功。');
                    $state.go('config.list');
                } else {
                    $scope.error_message = r.data.message;
                }
            }, err => {
                console.log(err);
                alert(err.statusText);
            });
    };
});