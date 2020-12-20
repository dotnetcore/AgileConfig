app.controller('syslogsCtrl', function ($scope, $http, msg) {
    $scope.selectedAppId = '';
    $scope.startTime = moment().add(-1, 'months').format('YYYY-MM-DD')
    $scope.endTime = moment().format('YYYY-MM-DD')
    $scope.logs = [];
    $scope.pageInfo = {
        pageIndex: 1,
        showPages: 5,
        totalPages: 0
    };
    $scope.typeName = function (type) {
        if (type === 0) {
            return '普通';
        }
        if (type === 1) {
            return '警告';
        }

        return ''
    }
    $scope.findApp = function (appId) {
        return $scope.apps.find(a => a.id === appId);
    }
    $scope.changePage = function (index) {
        $scope.pageInfo.pageIndex = index;
        $scope.search();
    }

    let searchUrl = function () {
        if ($scope.pageInfo.pageIndex === 0) {
            $scope.pageInfo.pageIndex += 1;
        }

        let url = '/SysLog/search?';
        url += 'startTime=' + $scope.startTime;
        url += '&endTime=' + $scope.endTime;
        url += '&appId=' + $scope.selectedAppId;
        url += '&pageIndex=' + $scope.pageInfo.pageIndex;
        url += '&pageSize=20';
        url += ('&_=' + new Date().getTime());
        return url;
    }

    $scope.search = function () {
        $http.get(searchUrl())
            .then(
                r => {
                    if (r.data.success) {
                        $scope.logs = r.data.data;
                        $scope.pageInfo.totalPages = r.data.totalPages;
                    } else {
                        $scope.logs = [];
                        msg.fail(r.data.message);
                    }
                },
                err => {
                    console.log(err);
                    msg.fail(err.statusText);
                });
    }

    $http.get('/app/all?_=' + (new Date).getTime())
        .then(
            r => {
                if (r.data.success) {
                    $scope.apps = r.data.data;
                    $scope.apps.unshift({
                        id: '',
                        name: ''
                    });
                    
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

    $scope.search();
});
