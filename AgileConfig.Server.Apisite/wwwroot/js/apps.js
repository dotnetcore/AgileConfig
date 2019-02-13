app.controller('appsCtrl', function ($scope, $state) {
    $state.go('apps.list');
});

app.controller('listAppCtrl', function ($scope, $http, $state) {
    $scope.toAdd = function () {
        $state.go('apps.add');
    };

    $scope.toEdit = function (app) {
        $state.go('apps.edit', {
            app_id: app.id
        });
    };

    $scope.disableOrEnable = function (app) {
        let cr = confirm('是否确定' + (app.enabled ? '停用' : '启用') + `应用【${app.id}】?`);
        if (!cr) {
            return;
        }

        $http.post('/app/disableOrEanble?id=' + app.id)
            .then(r => {
                if (r.data.success) {
                    app.enabled = !app.enabled;
                } else {
                    alert(r.data.message);
                }
            }, err => {
                console.log(err);
                alert(err.statusText);
            });
    };

    $scope.apps = [];

    $http.get('/app/all?_=' + (new Date).getTime())
        .then(
            r => {
                if (r.data.success) {
                    $scope.apps = r.data.data;
                } else {
                    $scope.apps = [];
                    alert(r.data.message);
                }
            },
            err => {
                console.log(err);
                alert(err.statusText);
            }
        );
});