app.controller('clientsCtrl', function ($scope, $http) {
    $http.get('/home/report')
        .then(r => {
            if (r.data.success) {
                $scope.clients = r.data.data.websocketCollectionReport.clientsCopy;
            }
        }, err => {
            console.log(err);
            alert(err.statusText);
        });

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
});
