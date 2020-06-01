app.controller('addNodeCtrl', function ($scope, $http, $state, $window) {
    $scope.node = {
        address: '',
        remark: ''
    };
    $scope.save = function () {
        $http.post('/servernode/add', $scope.node)
            .then(r => {
                if (r.data.success) {
                    alert('新增节点成功。');
                    $state.go('nodes.list');
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