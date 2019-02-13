app.controller('configCtrl', function ($scope, $state) {
    $state.go('config.list');
});

app.controller('listConfigCtrl', function ($scope, $http, $state) {
    $scope.toAdd = function () {
        $state.go('config.add');
    };
});