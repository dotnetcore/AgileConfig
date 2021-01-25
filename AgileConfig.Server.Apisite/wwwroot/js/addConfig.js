app.controller('addConfigCtrl', function ($scope, $http, $state, $window, msg) {
    $scope.config = {
        group: ''
    };
    $scope.apps = [];

    $http.get('/app/all?_=' + (new Date).getTime())
        .then(r => {
            if (r.data.success) {
                $scope.apps = r.data.data;
            } else {
                msg.success(r.data.message);
            }
        }, err => {
            console.log(err);
            msg.fail(err.statusText);
        });

    $scope.save = function () {
        $http.post('/config/add', $scope.config)
            .then(r => {
                if (r.data.success) {
                    let index = msg.confirm('新建配置成功, 是否继续新建配置？', ['继续新建', '完成'], () => {
                        msg.clear(index);
                        $scope.$apply(() => {
                            let lastAppId = $scope.config.appId;
                            $scope.config = {
                                group: '',
                                appId: lastAppId
                            };
                        });
                    }, () => {
                        msg.clear(index);
                        $scope.$apply(() => {
                            $state.go('config.list', {
                                app_id: $scope.config.appId
                            });
                        });
                    })
                } else {
                    $scope.error_message = r.data.message;
                }
            }, err => {
                console.log(err);
                msg.fail(err.statusText);
            });
    };

    $scope.formatMode = 'text';
    $scope.cm_el = null;
    $scope.cm = null;
    $scope.formatJson = function (config) {
        if (config.value) {
            $scope.formatMode = 'rich';
            var jsonStr = '';
            try {
                var obj = JSON.parse(config.value);
                jsonStr = JSON.stringify(obj, null, 4);
            } catch (e) {
                console.log(e);
            }
            if (jsonStr) {
                config.value = jsonStr;
            }
            var cmArea = document.getElementById('cm_area');
            $scope.cm = CodeMirror(function (elt) {
                if ($scope.cm_el) {
                    cmArea.removeChild($scope.cm_el);
                }
                $scope.cm_el = elt;
                cmArea.appendChild(elt);
            }, {
                value: config.value,
                mode: {
                    name: 'javascript',
                    json: true
                }
            });
            $scope.cm.on('change', function () {
                console.log('cm content change');
                var content = $scope.cm.getDoc().getValue();
                $scope.$apply(function () {
                    config.value = content;
                });
            });
        }
    }
    $scope.formatYml = function (config) {
        if (config.value) {
            $scope.formatMode = 'rich';
            var cmArea = document.getElementById('cm_area');
            $scope.cm = CodeMirror(function (elt) {
                if ($scope.cm_el) {
                    cmArea.removeChild($scope.cm_el);
                }
                $scope.cm_el = elt;
                cmArea.appendChild(elt);
            }, {
                value: config.value,
                mode: 'yaml',
                tabSize: 2,
            });
            $scope.cm.on('change', function () {
                console.log('cm content change');
                var content = $scope.cm.getDoc().getValue();
                $scope.$apply(function () {
                    config.value = content;
                });
            });
        }
    }
    $scope.formatText = function (config) {
        $scope.formatMode = 'text';
        var cmArea = document.getElementById('cm_area');
        if ($scope.cm_el) {
            cmArea.removeChild($scope.cm_el);
            $scope.cm_el = null;
        }
    }
    $scope.cancel = function () {
        $window.history.back();
    };
});