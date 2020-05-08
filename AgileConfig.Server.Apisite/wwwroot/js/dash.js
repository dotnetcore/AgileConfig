app.controller('dashCtrl', function ($scope, $http, $state, nodeStatusReflushService) {
    $scope.nodeCount = 0;
    $scope.appCount = 0;
    $scope.configCount = 0;
    $scope.clientCount = 0;
    $scope.serverNodes = [];

    nodeStatusReflushService.start(serverNodes => {
        $scope.serverNodes = serverNodes;
        var clientCount = 0;
        for (var i = 0; i < serverNodes.length; i++) {
            if (serverNodes[i].server_status) {
                clientCount += serverNodes[i].server_status.clientCount;
            }
        }
        $scope.clientCount = clientCount;
    });
    $http.get('/report/appcount')
        .then(r => {
            $scope.appCount = r.data;
        }, err => {
            console.log(err);
            alert(err.statusText);
        });
    $http.get('/report/nodecount')
        .then(r => {
            $scope.nodeCount = r.data;
        }, err => {
            console.log(err);
            alert(err.statusText);
        });
    $http.get('/report/configcount')
        .then(r => {
            $scope.configCount = r.data;
        }, err => {
            console.log(err);
            alert(err.statusText);
        });

    $scope.goto = function (path) {
        $state.go(path);
    };
});