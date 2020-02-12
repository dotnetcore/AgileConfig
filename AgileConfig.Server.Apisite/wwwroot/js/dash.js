app.controller('dashCtrl', function ($scope, $http, $state) {
    $http.get('/home/report')
        .then(r => {
            if (r.data.success) {
                $scope.report = r.data.data;
            }
        }, err => {
            console.log(err);
            alert(err.statusText);
        });

    $scope.goto = function (path) {
        $state.go(path);
    };
});