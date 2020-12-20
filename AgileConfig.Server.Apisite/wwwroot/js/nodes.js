app.factory('nodeStatusReflushService', function ($http, $interval,msg) {
    var service = {
        started: false,
        serverNodes: [],
        uiNodes: []
    };

    var reflushNodeStatus = function () {
        if (service.started) {
            $http.get('/report/RemoteNodesStatus?_=' + (new Date).getTime())
                .then(r => {
                    for (var i = 0; i < r.data.length; i++) {
                        service.serverNodes = r.data;
                        if (service.callback) {
                            service.callback(service.serverNodes);
                        }
                    }
                }, err => {
                    console.log(err);
                });
        }
    };

    service.start = function (callback) {
        service.callback = callback;
        if (!service.started) {
            service.started = true;
            reflushNodeStatus();
            $interval(reflushNodeStatus, 5 * 1000);
        }
    };

    return service;
});

app.controller('nodesCtrl', function ($state) {
    $state.go('nodes.list');
});

app.controller('ListnodeCtrl', function ($scope, $http, $state, nodeStatusReflushService, msg) {
    $scope.toAdd = function () {
        $state.go('nodes.add');
    };
    $scope.deleteNode = function (node, index) {
        let layerIndex = msg.confirm(`是否确定删除节点【${node.address}】? <br>删除节点并不会让其真正的下线，只是脱离控制台的管理。所有连接至此节点的客户端都会继续正常工作。`,
            ['确定', '取消'],
            function () {
                $http.post('/servernode/delete', node)
                    .then(r => {
                        msg.clear(layerIndex);
                        if (r.data.success) {
                            $scope.nodes.splice(index, 1);
                        } else {
                            msg.fail(r.data.message);
                        }
                    }, err => {
                        msg.clear(layerIndex);
                        console.log(err);
                        msg.fail(err.statusText);
                    });
            }
        );
    };

    $scope.nodeClientsReflushConfigItems = function (address) {
        $http.post('/RemoteServerProxy/AllClients_Reload?address=' + address)
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

    var load = function () {
        $http.get('/servernode/all?_=' + (new Date).getTime())
            .then(r => {
                if (r.data.success) {
                    $scope.nodes = r.data.data;
                    nodeStatusReflushService.start((serverNodes) => {
                        for (var i = 0; i < serverNodes.length; i++) {
                            var node = serverNodes[i].n;
                            if ($scope.nodes) {
                                var uiNode = $scope.nodes.find(n => n.address === node.address);
                                if (uiNode) {
                                    uiNode.lastEchoTime = node.lastEchoTime;
                                    uiNode.status = node.status;
                                }
                            }
                        }
                    });
                }
            }, err => {
                console.log(err);
                msg.fail(err.statusText);
            });
    };
    load();
});