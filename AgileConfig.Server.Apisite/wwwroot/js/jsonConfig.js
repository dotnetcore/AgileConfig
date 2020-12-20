app.controller('jsonConfigCtrl', function ($scope, $http, $state, $stateParams,msg) {
    let _appId = $stateParams.app_id;

    $scope.apps = [];
    $scope.selectedAppId = '';
    $scope.configs = [];

    $http.get('/app/all?_=' + (new Date).getTime())
        .then(r => {
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
                msg.fail(r.data.message);
            }
        }, err => {
            console.log(err)
             msg.fail(err.statusText)
        });

    $scope.save = function () {
        $http.post('/config/edit', $scope.config)
            .then(r => {
                if (r.data.success) {
                    msg.success('修改配置成功。');
                    $state.go('config.list', {
                        app_id: $scope.config.appId
                    });
                } else {
                    $scope.error_message = r.data.message;
                }
            }, err => {
                console.log(err);
                msg.fail(err.statusText);
            });
    };

    $scope.selectFile = function () {
        fileSelect.click();
    }
    let fileSelect = document.getElementById("file");
    fileSelect.onchange = function () {
        let file = fileSelect.files[0];
        if (!file) {
            return;
        }
        let fd = new FormData();
        fd.append('files', file);
        $http.post("/config/PreViewJsonFile",
            fd,
            {
                headers: { 'Content-Type': undefined }
            }
        ).then(function (rep) {
            if (rep.data.success) {
                $scope.configs = rep.data.data;
                $scope.configs.forEach(item => {
                    item.selected = true; 
                });
            } else {
                msg.fail(rep.data.message);
            }
        }, err => {
                msg.fail('解析json文件失败');
        });
    }

    $scope.selectedRows = function () {
        return $scope.configs.filter(c => c.selected).length;
    }

    $scope.selectRow = function (row) {
        row.selected = !row.selected;
    }

    $scope.import = function () {
        var rows = $scope.selectedRows();
        if (rows === 0) {
            $scope.error_message = '请选择至少一行配置';
            return;
        }
        if (!$scope.selectedAppId) {
            $scope.error_message = '请选择所属应用';
            return;
        }

        let configs = $scope.configs.filter(item => item.selected);
        configs.forEach(item => {
            item.appId = $scope.selectedAppId
        })

        $scope.adding = true;
        $http.post('/config/addRange', configs)
            .then(
                r => {
                    if (r.data.success) {
                        msg.success('导入成功。');
                        $state.go('config.list', {
                            app_id: $scope.selectedAppId
                        });
                    } else {
                        msg.fail(r.data.message);
                    }
                    $scope.adding = false;
                },
                err => {
                    console.log(err);
                    msg.fail(err.statusText);
                    $scope.adding = false;
                }
            );
    }

    $scope.cancel = function () {
        $state.go('config.list', {
            app_id: $scope.selectedAppId
        });
    };
});