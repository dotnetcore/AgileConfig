app.controller('modifyLogsCtrl', function ($scope, $http, $state, $stateParams, $filter) {
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
        let cr = confirm(`是否确定回滚至此版本【${modifyTime}】？`);
        if (!cr) {
            return;
        }
        let configResult = await $http.get('/config/get?id=' + log.configId + '&_=' + new Date().getTime());
        if (configResult.data.success) {
            let config = configResult.data.data;
            config.key = log.key;
            config.group = log.group;
            config.value = log.value;
            $http.post('/config/edit', config)
                .then(r => {
                    if (r.data.success) {
                        alert('回滚成功。');
                        $state.go('config.list');
                    } else {
                        alert(r.data.message);
                    }
                }, err => {
                    console.log(err);
                    alert(err.statusText);
                });
        } else {
            alert(configResult.data.message);
        }
    };
});