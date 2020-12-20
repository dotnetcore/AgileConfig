app.controller('clientsCtrl', function ($scope, $http, msg) {
    $scope.getClients = function (address) {
        $http.get('/report/ServerNodeClients?address=' + address + '&_' + new Date().getTime())
            .then(resp => {
                if (resp.data) {
                    $scope.clients = resp.data.infos;
                } else {
                    $scope.clients = [];
                }
            }, err => {
                console.log(err);
                msg.fail(err.statusText);
            });
    };
    $scope.offline = function (client, index) {
        let layerIndex = msg.confirm(`是否确定断开客户端【${client.id}】?`, ['确定', '取消'], function () {
            $http.post('/RemoteServerProxy/Client_Offline?clientId=' + client.id + '&address=' + $scope.selectedNodeAddress)
                .then(r => {
                    msg.clear(layerIndex);
                    if (r.data.success) {
                        msg.success("客户端已断开。");
                        $scope.clients.splice(index, 1);
                    } else {
                        msg.fail(r.data.message);
                    }
                }, err => {
                    msg.clear(layerIndex);
                    console.log(err);
                    msg.fail(err.statusText);
                });
        });
    };
    $scope.clientsReflushConfigItems = function (client) {
        $http.post('/RemoteServerProxy/Client_Reload?address=' + $scope.selectedNodeAddress + '&clientId=' + client.id)
            .then(r => {
                if (r.data.success) {
                    msg.success("刷新成功。");
                } else {
                    msg.fail(r.data.message);
                }
            }, err => {
                console.log(err);
                msg.fail(err.statusText);
            });
    };
    $http.get('/servernode/all?_' + new Date().getTime())
        .then(resp => {
            $scope.nodes = resp.data.data;
            if (resp.data.data.length) {
                $scope.selectedNodeAddress = resp.data.data[0].address;
                $scope.getClients($scope.selectedNodeAddress);
            }
        }, err => {
            console.log(err);
            msg.fail(err.statusText);
        });

});
