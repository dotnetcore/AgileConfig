app.controller('addNodeCtrl', function ($scope, $http, $state, $window, msg) {
    $scope.node = {
        address: '',
        remark: ''
    };
    $scope.save = function () {
        $http.post('/servernode/add', $scope.node)
            .then(r => {
                if (r.data.success) {
                    msg.success('新增节点成功。');
                    $state.go('nodes.list');
                } else {
                    $scope.error_message = r.data.message;
                }
            }, err => {
                console.log(err);
                msg.fail(err.statusText);
            });
    };

    $scope.cancel = function () {
        $window.history.back();
    };
});