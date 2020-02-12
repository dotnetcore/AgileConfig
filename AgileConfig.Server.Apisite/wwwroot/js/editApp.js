app.controller('editAppCtrl', function ($scope, $http, $state, $stateParams) {
    let id = $stateParams.app_id;
    $scope.app = {
    };

    $http.get('/app/get?id=' + id + '&_=' + (new Date).getTime())
        .then(
            r => {
                if (r.data.success) {
                    $scope.app = r.data.data;
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
                    alert('修改应用成功。');
                    $state.go('apps.list');
                } else {
                    $scope.error_message = r.data.message;
                }
            }, err => {
                console.log(err);
                alert(err.statusText);
            });
    };

    $scope.cancel = function () {
        $state.go('apps.list');
    };
});