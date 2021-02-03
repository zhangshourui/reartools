app.config(['$httpProvider', function ($httpProvider) {
    $httpProvider.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $httpProvider.defaults.headers.post['Content-Type'] = 'application/json;charset=utf-8';
}]);

app.factory('ngAjax', function ($http) {
    return {
        post: function (url, postData, callback, p1, p2) {
            var option = {};
            var errCall = null;
            if (arguments.length == 4) {
                option = p1;
            }
            else if (arguments.length == 5) {
                errCall = p1;
                option = p2;
            }


            if (typeof option.beforeSend == 'function') {
                option.beforeSend.apply(this, arguments);
            }
            $http.post(url, postData).then(function (resp) {

                var data = resp.data;
                if (data) {
                    if (typeof data.Success === 'boolean' && typeof data.Message !== 'undefined') {
                        if (!data.Success) {
                            console.log(resp);
                            if (errCall) {
                                errCall(resp);
                            }
                            else {
                                alert(data.Message, -1);
                            }
                            return;
                        }
                    }
                }
                callback(data);
                if (typeof option.complete == 'function') {
                    option.complete.apply(this, arguments);
                }
            }, function (resp) {
                console.log(resp);

                if (resp) {
                    if (resp.status <= 0) {
                        alert("网络连接失败", -1);
                    }
                    else
                        alert(resp.statusText, -1);
                }
                if (typeof option.complete == 'function') {
                    option.complete.apply(this, arguments);
                }
            });
        },
        get: function (url, callback, p1, p2) {
            var option = {};
            var errCall = null;
            if (arguments.length == 3) {
                option = p1;
            }
            else if (arguments.length == 4) {
                errCall = p1;
                option = p2;
            }

            if (typeof option.beforeSend == 'function') {
                option.beforeSend.apply(this, arguments);
            }
            $http.get(url).then(function (resp) {


                var data = resp.data;
                if (data) {
                    if (typeof data.Success === 'boolean' && typeof data.Message !== 'undefined') {
                        if (!data.Success) {
                            console.log(resp);
                            if (errCall) {
                                errCall(resp);
                            }
                            else {
                                alert(data.Message, -1);
                            }
                            return;
                        }
                    }
                }
                callback(data);
                if (typeof option.complete == 'function') {
                    option.complete.apply(this, arguments);
                }

            }, function (resp) {
                console.log(resp);

                if (resp) {
                    if (resp.status <= 0) {
                        alert("网络连接失败", -1);
                    }
                    else
                        alert(resp.statusText, -1);
                }

                if (typeof option.complete == 'function') {
                    option.complete.apply(this, arguments);
                }

            });
        }


        , _urlEncode: function (param, key, encode) {
            if (param === null) return '';
            var paramStr = '';
            var t = typeof param;
            if (t === 'string' || t === 'number' || t === 'boolean') {
                paramStr += '&' + key + '=' + (encode ? encodeURIComponent(param) : param);
            } else {
                for (var i in param) {
                    var k = !key ? i : key + (param instanceof Array ? '[' + i + ']' : '.' + i);
                    paramStr += this._urlEncode(param[i], k, encode);
                }
            }
            return paramStr;
        }
        , objToParam: function (param, key, encode) {
            var paramStr = this._urlEncode(param, key, encode);
            if (paramStr) {
                if (paramStr.startsWith('&'))
                    paramStr = paramStr.substr(1);

                return paramStr;
            }
            else
                return null;
        }
    };
});