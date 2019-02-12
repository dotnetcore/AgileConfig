app.controller('appsCtrl', function ($scope, $state) {
    $state.go('apps.list');
});

app.controller('listAppCtrl', function ($scope, $http, $state) {
    $scope.toAdd = function () {
        $state.go('apps.add');
    };
});