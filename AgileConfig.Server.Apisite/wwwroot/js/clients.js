app.controller('clientsCtrl', function ($scope, $http, nodeStatusReflushService) {
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
                alert(err.statusText);
            });
    };
    $scope.offline = function (client, index) {
        let cr = confirm(`是否确定断开客户端【${client.id}】?`);
        if (!cr) {
            return;
        }
        $http.post('/RemoteServerProxy/Client_Offline?clientId=' + client.id + '&address=' + $scope.selectedNodeAddress)
            .then(r => {
                if (r.data.success) {
                    alert("客户端已断开。");
                    $scope.clients.splice(index, 1);
                } else {
                    alert(r.data.message);
                }
            }, err => {
                console.log(err);
                alert(err.statusText);
            });
    };
    $scope.clientsReflushConfigItems = function (client) {
        $http.post('/RemoteServerProxy/Client_Reload?address=' + $scope.selectedNodeAddress + '&clientId=' + client.id)
            .then(r => {
                if (r.data.success) {
                    alert("刷新成功。");
                } else {
                    alert(r.data.message);
                }
            }, err => {
                console.log(err);
                alert(err.statusText);
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
            alert(err.statusText);
        });

});
