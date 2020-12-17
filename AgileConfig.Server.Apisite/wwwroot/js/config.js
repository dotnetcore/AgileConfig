app.controller('configCtrl', function ($scope, $state) {
    $state.go('config.list', {
        app_id: ''
    });
});

app.controller('listConfigCtrl', function ($scope, $http, $state, $stateParams, msg) {

    let _appId = $stateParams.app_id;

    $scope.configs = [];
    $scope.apps = [];
    $scope.selectedAppId = '';
    $scope.filter_group = '';
    $scope.filter_key = '';
    $scope.pageInfo = {
        pageIndex: 1,
        showPages: 5,
        totalPages: 0
    };

    $scope.onlineStatusDesc = {
        '0': "待上线",
        '1': "已上线"
    };

    $scope.toAdd = function () {
        $state.go('config.add');
    };

    $scope.toEdit = function (config) {
        $state.go('config.edit', {
            config_id: config.id
        });
    };
    $scope.toJson = function () {
        $state.go('config.json',
            {
                app_id: $scope.selectedAppId
            }
        );
    };
    $scope.deleteConfig = function (config) {
        let message = '是否确定删除' + `配置【${config.key}】?`;
        let index = msg.confirm(message, ['确定', '取消'], () => {
            $http.post('/config/delete?id=' + config.id)
                .then(r => {
                    msg.clear(index);
                    if (r.data.success) {
                        config.status = 0;
                    } else {
                        msg.fail(r.data.message);
                    }
                }, err => {
                    msg.clear(index);
                    console.log(err);
                    msg.fail(err.statusText);
                });
        });
    };

    $scope.changePage = function (index) {
        $scope.pageInfo.pageIndex = index;
        $scope.search();
    }

    $scope.showWholeValue = function (item) {
        console.log(item.value);
        $scope.selectedValueDetail = item.value;
        $('#modal_show_value_detail').modal('show');
    }

    let newSearchUrl = function () {
        if ($scope.pageInfo.pageIndex === 0) {
            $scope.pageInfo.pageIndex += 1;
        }

        let url = `/config/search?appId=${$scope.selectedAppId}&group=${$scope.filter_group}&key=${$scope.filter_key}&pageSize=20&pageIndex=${$scope.pageInfo.pageIndex}`;
        url = url + '&_=' + (new Date).getTime();

        return url;
    };
    $scope.search = function () {
        $http.get(newSearchUrl())
            .then(
                r => {
                    if (r.data.success) {
                        $scope.configs = r.data.data;
                        $scope.pageInfo.totalPages = r.data.totalPages;
                    } else {
                        $scope.configs = [];
                        alert(r.data.message);
                    }
                },
                err => {
                    console.log(err);
                    alert(err.statusText);
                }
            );
    };

    $scope.toModifylogs = function (config) {
        $state.go('config.logs', {
            config_id: config.id
        });
    };

    $scope.publish = function (config) {
        let message = '是否确定上线' + `配置【${config.key}】?`;
        let index = msg.confirm(message, ['确定', '取消'], () => {
            if (config.onlineStatus === 0) {
                $http.post('/config/publish?configId=' + config.id)
                    .then(
                        r => {
                            msg.clear(index);
                            if (r.data.success) {
                                config.onlineStatus = 1;
                                msg.success('上线成功。');
                            } else {
                                msg.fail(r.data.message);
                            }
                        },
                        err => {
                            msg.clear(index);
                            console.log(err);
                            msg.fail(err.statusText);
                        }
                    );
            }
        });
    };
    $scope.publishSelected = function () {
        var selectedConfgs = $scope.configs.filter(c => c.selected && c.onlineStatus === 0);
        if (!selectedConfgs.length) {
            msg.alert('请至少选择一行待上线的配置。');
            return;
        }
        var message = '是否确定上线选中的配置?';
        let index = msg.confirm(message, ['确定', '取消'], () => {
            $http.post('/config/PublishSome', selectedConfgs.map(c => c.id))
                .then(
                    r => {
                        msg.clear(index);
                        if (r.data.success) {
                            selectedConfgs.forEach(item => {
                                item.onlineStatus = 1;
                            })
                            msg.success('上线成功。');
                        } else {
                            msg.fail(r.data.message);
                        }
                    },
                    err => {
                        msg.clear(index);
                        console.log(err);
                        msg.fail(err.statusText);
                    }
                );
        });
    }
    $scope.offline = function (config) {
        var message = '是否确定下线' + `配置【${config.key}】?`;
        let index = msg.confirm(message, ['确定', '取消'], () => {
            if (config.onlineStatus === 1) {
                $http.post('/config/offline?configId=' + config.id)
                    .then(
                        r => {
                            msg.clear(index);
                            if (r.data.success) {
                                config.onlineStatus = 0;
                                msg.success('下线成功。');
                            } else {
                                msg.fail(r.data.message);
                            }
                        },
                        err => {
                            msg.clear(index);
                            console.log(err);
                            msg.fail(err.statusText);
                        }
                    );
            }
        });
    };

    $scope.$watch('selectedAppId', function (newval, oldval) {
        if (newval != oldval) {
            $scope.search();
        }
    });

    $http.get('/app/all?_=' + (new Date).getTime())
        .then(
            r => {
                if (r.data.success) {
                    $scope.apps = r.data.data;
                    if ($scope.apps.length > 0) {
                        if (_appId) {
                            $scope.selectedAppId = _appId;
                        } else {
                            $scope.selectedAppId = $scope.apps[0].id;
                        }
                    }
                } else {
                    $scope.apps = [];
                    msg.fail(r.data.message);
                }
            },
            err => {
                console.log(err);
                msg.fail(err.statusText);
            }
        );

});