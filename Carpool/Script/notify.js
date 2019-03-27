$(function () {
    function check() {
        if (!islogin())
            return;
        senddata({
            url: api_url + 'notification/checknotify',
            data: { 'CarpoolSSID': $.cookie('CarpoolSSID') },
            call: function (in_data) {
                if (in_data['code'] == 200) {
                    var data = in_data['data'];
                    if (data.length <= 0) {
                        return;
                    }

                    alert_info_tip("<a class='info'>" + data[0]['title'] + "</a>", 30000, () => {
                        location.href = 'user.html?menu=notice-all';
                    });
                }
            }
        })
    }
    function CheckNotify() {
        check();
        setInterval(() => {
            check();
        }, 5000);
    }

    CheckNotify();
})