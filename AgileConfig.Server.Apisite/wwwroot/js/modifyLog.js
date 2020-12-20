app.controller('modifyLogsCtrl', function ($scope, $http, $state, $stateParams, $filter, msg) {
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
    $scope.rollback = async function (log) {
        let dateFilter = $filter('date');
        let modifyTime = dateFilter(log.modifyTime, 'yyyy-MM-dd HH:mm:ss');
        let layerindex = msg.confirm(`是否确定回滚至此版本【${modifyTime}】？`,['确定','取消'],async function () {
            let configResult = await $http.get('/config/get?id=' + log.configId + '&_=' + new Date().getTime());
            $http.post('/config/rollback?configId=' + log.configId + '&logId=' + log.id)
                .then(r => {
                    msg.clear(layerindex);
                    if (r.data.success) {
                        msg.success('回滚成功。');
                        $state.go('config.list', {
                            app_id: configResult.data.data.appId
                        });
                    } else {
                        msg.fail(r.data.message);
                    }
                }, err => {
                    msg.clear(layerindex);
                    console.log(err);
                    msg.fail(err.statusText);
                });
        });
    };

    $scope.goback = async function () {
        let configResult = await $http.get('/config/get?id=' + id + '&_=' + new Date().getTime());
        $state.go('config.list', {
            app_id: configResult.data.data.appId
        });
    };
});