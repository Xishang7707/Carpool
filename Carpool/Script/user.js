$(function () {
    function logined() {
        if (!islogin())
            return;
        senddata({
            url: api_url + 'users/getinfo',
            data: { 'CarpoolSSID': $.cookie('CarpoolSSID') },
            call: (in_data) => {
                if (in_data['code'] == 200) {

                    var data = in_data['data'];
                    var us_info_name = $('.user-info-name');
                    us_info_name.text(data['name'] == null ? data['tel'] : data['name']);
                    if (!us_info_name.hasClass('pl-5'))
                        us_info_name.addClass('pl-5');
                    us_info_name.css({
                        'background': 'url(images/user-headprotrait/' + data['headportrait'] + ') no-repeat',
                        'background-size': '40px'
                    });
                    $(".nav-login").addClass('d-none');
                    $(".user-menu").removeClass('d-none');
                }
            }
        })
    };
    function loginout() {
        $.cookie('CarpoolSSID', '');
        $(".nav-login").removeClass('d-none');
        $(".user-menu").addClass('d-none');
    }
    logined();

    //登录
    $('[data-target="#Login_dialog"]').click(() => {
        Login_Show(() => {
            logined();
        })
    });

    $('[data-target="Login_logout"]').click(() => {
        Login_Logout(() => {
            loginout();
        })
    });

    //用户
    //基本信息
    new Vue({
        el: "#menu-user-info",
        data: {
            car: "",
            user: "",
            notice: "",
            order: ""
        },
        methods: {
            init: function () {
                senddata({
                    url: api_url + 'users/menu_user_info',
                    data: { 'CarpoolSSID': $.cookie('CarpoolSSID') },
                    call: (in_data) => {
                        if (in_data['code'] == 200) {
                            var data = in_data['data'];
                            this.car = data['car'];
                            this.user = data['user'];
                            this.notice = data['notice'];
                            this.order = data['order'];
                        }
                    }
                })
            }
        }
    }).init();
})