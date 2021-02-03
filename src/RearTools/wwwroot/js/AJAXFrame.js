AJAXFrame = {};
var AppPath = window.location.protocol + "//" + window.location.host;

if (typeof window.loading ==='undefined') {
    window.loading = function () { };
}

AJAXFrame.Ajax = {
    MAX_REREQUEST_COUNT: 2,
    _requestKeys: [],
    _requestErrorCounts: [],
    PARAM_CALLBACKTYPE: "_ajaxcalltype",
    PARAM_CALLBACKTAG: "_ajaxcall",
    PARAM_CALLBACKID: "_ajaxcallid",
    PARAM_CALLBACKMETHOD: "_ajaxcallm",
    PARAM_CALLBACKARG: "_ajaxcallarg",

    /*Ajax请求
    *@serverType:服务器端的类型
    *@serverMethod:服务器端的方法
    *@args:Json形式的参数
    *@clientCallBack:客户端回调的方法
    *@url:请求的URL
    *@httpMethod:请求的方法：get/post
    */
    callBack: function (serverType, serverMethod, args, clientCallBack, url, httpMethod, timeout, async) {
        if (typeof async === "undefined")
            async = true;

        var parameter = {};
        parameter[AJAXFrame.Ajax.PARAM_CALLBACKTYPE] = serverType;
        parameter[AJAXFrame.Ajax.PARAM_CALLBACKMETHOD] = serverMethod;
        if (args) {
            for (var i in args) {
                var name = args[i].name;
                var paramVal = args[i].value;

                //if ((paramVal instanceof Object) && !(paramVal instanceof Array))
                //	paramVal = JSON.stringify(paramVal);
                //else if (paramVal instanceof Array) {
                //	for (var k in paramVal) {
                //		var v = paramVal[k];
                //		if (v instanceof Object)
                //			paramVal[k] = JSON.stringify(v);

                //	}
                //}
                parameter[name] = paramVal;
            }
        }
        httpMethod = httpMethod || "post";
        if (url.indexOf("http://") < 0) {
            url = AppPath + url;
        }
        //var ajaxRequest = null;
        AjaxCommon(url, clientCallBack, httpMethod, parameter, async);
    }
};


function AjaxCommon(url, clientCallBack, httpMethod, parameter, async, option) {
    if (typeof async === "undefined")
        async = true;
    option = option || {};
    var hideLoading = option && typeof option.showLoading === 'boolean' && !option.showLoading;
    if (!hideLoading) {
        window.loading('show');
    }


    $.ajax({
        type: httpMethod,
        url: url,
        data: JSON.stringify(parameter),
        // contentType: "application/x-www-form-urlencoded,charset=UTF-8",
        contentType: "application/json;charset=UTF-8",
        async: async,
        timeout: 50000,
        dataType: "json",
        disableLoadingMask: hideLoading,
        headers: {
            "X-Requested-With": "XMLHttpRequest"
        },
        beforeSend: function (xhr, settings) {
            if (typeof option.beforeSend == 'function') {
                option.beforeSend.apply(this, arguments);
            }
        },
        success: function (data, result, xhr) {
            if (data) {
                if (typeof data.Success === 'boolean' && typeof data.Message !== 'undefined') {
                    if (!data.Success) {
                        alert(data.Message, -1);
                        return;
                    }
                }
            }
            clientCallBack.apply(this, arguments);
        },
        error: function (e) {

            console.debug("%o", e);
            if (e.status === 0) {
                alert("网络连接错误");
            }
            else {
                //alert(e.responseText || e.statusText);
            }
        },
        complete: function () {
            if (!hideLoading) {
                window.loading("hide");
            }
            if (typeof option.complete == 'function') {
                option.complete.apply(this, arguments);
        }
        }

    });

}


function lcsPost(url, parameter, clientCallBack, option) {

    AjaxCommon(url, clientCallBack, "POST", parameter, true, option);

}
function lcsGet(url, clientCallBack, option) {

    AjaxCommon(url, clientCallBack, "GET", null, true, option);

}