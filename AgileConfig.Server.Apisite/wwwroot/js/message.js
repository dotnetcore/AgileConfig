app.factory('msg', function () {
    var o = {};

    o.success = function (message) {
        return layer.alert(message, { icon: 1 })
    }

    o.fail = function (message) {
        return layer.alert(message, { icon: 2 })
    }

    o.alert = function (message) {
        return layer.alert(message)
    }

    o.confirm = function (message, buttons, okCallback, noCallback) {
        return layer.confirm(message, {
            btn: buttons//按钮
        }, okCallback, noCallback);
    }

    o.msg = function (message) {
        return layer.msg(message);
    }

    o.clear = function (index) {
        layer.close(index);
    }

    return o;

});