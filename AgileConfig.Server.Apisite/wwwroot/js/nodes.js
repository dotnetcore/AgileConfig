app.factory('nodeStatusReflushService', function ($interval) {
    var service = {
        started: false,
        uiNodes: []
    };

    var reflushNodeStatus = function () {
        if (service.started) {
            $http.get('/servernode/all?_=' + (new Date).getTime())
                .then(r => {
                    if (r.data.success) {
                        for (var i = 0; i < r.data.data.length; i++) {
                            var node = r.data.data[i];
                            var uiNode = service.uiNodes.find(n => n.address === node.address);
                            if (uiNode) {
                                uiNode.lastEchoTime = node.lastEchoTime;
                                uiNode.status = node.status;
                            }
                        }
                    }
                }, err => {
                    console.log(err);
                });
        }
    };

 

    service.start = function (nodes) {
        service.uiNodes = nodes;
        service.started = true;
        if (service.started) {
            $interval(reflushNodeStatus, 5000);
        }
    };

    return service;
});

app.controller('nodesCtrl', function ($state) {
    $state.go('nodes.list');
});

app.controller('ListnodeCtrl', function ($scope, $http, $state, nodeStatusReflushService ) {
    $scope.toAdd = function () {
        $state.go('nodes.add');
    };
    $scope.deleteNode = function (node, index) {
        let cr = confirm(`是否确定删除节点【${node.address}】?`);
        if (!cr) {
            return;
        }

        $http.post('/servernode/delete', node)
            .then(r => {
                if (r.data.success) {
                    $scope.nodes.splice(index, 1);
                } else {
                    alert(r.data.message);
                }
            }, err => {
                console.log(err);
                alert(err.statusText);
            });
    };

    var load = function () {
        $http.get('/servernode/all?_=' + (new Date).getTime())
            .then(r => {
                if (r.data.success) {
                    $scope.nodes = r.data.data;
                }
            }, err => {
                console.log(err);
                alert(err.statusText);
            });
    };

    nodeStatusReflushService.start($scope.nodes);
    load();
});