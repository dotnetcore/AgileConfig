app.controller('appsCtrl', function ($scope, $state) {
	$state.go('apps.list');
});

app.controller('listAppCtrl', function ($scope, $http, $state, msg) {
	$scope.toAdd = function () {
		$state.go('apps.add');
	};

	$scope.toEdit = function (app) {
		$state.go('apps.edit', {
			app_id: app.id
		});
	};

	$scope.disableOrEnable = function (app) {
		let message = '是否确定' + (app.enabled ? '停用' : '启用') + `应用【${app.id}】?`;
		var index = msg.confirm(message, ['确定', '取消'], () => {
			$http.post('/app/disableOrEanble?id=' + app.id)
				.then(r => {
					msg.clear(index);
					if (r.data.success) {
						app.enabled = !app.enabled;
					} else {
						msg.fail(r.data.message);
					}
				}, err => {
					msg.clear(index);
					console.log(err);
					msg.fail(err.statusText);
				});
		});
	  
	};
	$scope.delete = function (app) {
		let message = '是否确定删除' + `应用【${app.id}】?`;
		var index = msg.confirm(message, ['确定', '取消'], () => {
			$http.post('/app/delete?id=' + app.id)
				.then(r => {
					msg.clear(index);
					if (r.data.success) {
						$scope.getAll();
					} else {
						msg.fail(r.data.message);
					}
				}, err => {
					msg.clear(index);
					console.log(err);
					msg.fail(err.statusText);
				});
		});
    }

	$scope.apps = [];

	$scope.getAll = function () {
		$http.get('/app/all?_=' + (new Date).getTime())
			.then(
				r => {
					if (r.data.success) {
						$scope.apps = r.data.data;
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
	}

	$scope.getAll();
});