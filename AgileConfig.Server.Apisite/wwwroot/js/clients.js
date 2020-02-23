app.controller('clientsCtrl', function ($scope, $http, nodeStatusReflushService) {
    $scope.getClients = function (address) {
        $http.get('/report/ServerNodeClients?address=' + address + '&_' + new Date().getTime())
            .then(resp => {
                if (resp.data.data) {
                    $scope.clients = resp.data.data.clientsCopy;
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
        $http.post('/home/Client_Offline?id=' + client.id)
            .then(r => {
                if (r.data.success) {
                    alert("客户端已断开。");
                    $scope.clients.splice(index, 1);
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
