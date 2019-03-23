var api_url = "http://localhost:61640/api/"

function alert_success(el, info, cls = '') {
    var elem = $('<div class="alert ' + cls + ' alert-success alert-dismissible fade show"><button type="button" class="close" data-dismiss="alert">&times;</button><strong>成功!</strong> ' + info + '</div>');
    el.prepend(elem);
}

function alert_error(el, info, cls = '') {
    var elem = $('<div class="fixed-top alert ' + cls + ' alert-danger alert-dismissible fade show"><button type="button" class="close" data-dismiss="alert">&times;</button><strong>错误!</strong> ' + info + '</div>');
    el.prepend(elem);
}

function alert_success_tip(el, info, sec = 3000) {
    var elem = $('<div class="container-fluid fixed-top"><div class="row p-0 d-flex flex-wrap justify-content-center"><div class="col-md-6 p-2"><div class="alert tip alert-success alert-dismissible fade show"><button type="button" class="close" data-dismiss="alert">&times;</button><strong>成功!</strong>' + info + '</div></div></div></div>');
    el.prepend(elem);


    setTimeout(() => {
        $(".alert.tip").alert('close');
        elem.remove();
    }, sec)
}

function alert_info_tip(el, info, sec = 3000) {
    var elem = $('<div class="container-fluid fixed-top"><div class="row p-0 d-flex flex-wrap justify-content-center"><div class="col-md-6 p-2"><div class="alert tip alert-info alert-dismissible fade show"><button type="button" class="close" data-dismiss="alert">&times;</button><strong>提示!</strong>' + info + '</div></div></div></div>');
    el.prepend(elem);

    setTimeout(() => {
        $(".alert.tip").alert('close');
        elem.remove();
    }, sec)
}

function alert_error_tip(el, info, sec = 3000) {
    var elem = $('<div class="container-fluid fixed-top"><div class="row p-0 d-flex flex-wrap justify-content-center"><div class="col-md-6 p-2"><div class="alert tip alert-danger alert-dismissible fade show"><button type="button" class="close" data-dismiss="alert">&times;</button><strong>错误!</strong>' + info + '</div></div></div></div>');
    el.prepend(elem);

    setTimeout(() => {
        $(".alert.tip").alert('close');
        elem.remove();
    }, sec)
}

function loading(el, text, color) {
    var elem = $('<span class="loading spinner-border spinner-border-sm ' + color + '" role="status" aria-hidden="true"></span> <span class="sr-only ' + color + '">' + text + '</span>');
    el.prepend(elem);
}

/**
 * 发送数据
 * @param {any} param0
 */
function senddata({ url, data, call }) {
    $.ajax({
        url: url,
        method: "POST",
        data: JSON.stringify(data),
        "headers": {
            "content-type": "application/json"
        },
        async: true,
        xhrFields: {
            withCredentials: true
        },
        success: function (in_data) {
            if (call)
                call(in_data);
        }
    })
}

function islogin() {
    var ssid = $.cookie("CarpoolSSID");
    if (!ssid || ssid == '')
        return false;
    return true;
}

Date.prototype.format = function (fmt) {
    var o = {
        "M+": this.getMonth() + 1, //月份 
        "d+": this.getDate(), //日 
        "h+": this.getHours(), //小时 
        "m+": this.getMinutes(), //分 
        "s+": this.getSeconds(), //秒 
        "q+": Math.floor((this.getMonth() + 3) / 3), //季度 
        "S": this.getMilliseconds() //毫秒 
    };
    if (/(y+)/.test(fmt)) fmt = fmt.replace(RegExp.$1, (this.getFullYear() + "").substr(4 - RegExp.$1.length));
    for (var k in o)
        if (new RegExp("(" + k + ")").test(fmt)) fmt = fmt.replace(RegExp.$1, (RegExp.$1.length == 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)));
    return fmt;
}