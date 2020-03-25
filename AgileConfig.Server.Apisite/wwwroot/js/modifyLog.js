app.controller('modifyLogsCtrl', function ($scope, $http, $state, $stateParams) {
    let id = $stateParams.config_id;
    $scope.logs = [];

    $http.get('/config/modifylogs?configId=' + id + '&_=' + new Date().getTime())
        .then(r => {
            if (r.data.success) {
                $scope.logs = r.data.data;
            } else {
                $scope.logs = [];
                alert(r.data.message);
            }
        },
            err => {
                console.log(err);
                alert(err.statusText);
            });
});