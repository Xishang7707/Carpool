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
    logined();

    //登录
    $('[data-target="#Login_dialog"]').click(() => {
        Login_Show(() => {
            logined();

            $(".nav-login").addClass('d-none');
            $(".user-menu").removeClass('d-none');
        })
    });

    $('[data-target="Login_logout"]').click(() => {
        Login_Logout(() => {
            $.cookie('CarpoolSSID', '');
            $(".nav-login").removeClass('d-none');
            $(".user-menu").addClass('d-none');
        })
    });

    new Vue({
        el: "#form-register",
        data: {
            tel: "",
            telcode: "",
            password: ""
        },
        methods: {
            /**
             * 手机号码焦点
             * */
            tel_blur: function () {
                this.tel_exist((in_data) => {
                    if (in_data['code'] != 0)
                        alert_error_tip($("#form-register"), in_data['msg']);
                })
            },
            //发送短信
            telcode_send: function (e) {
                $(".alert.tip").alert('close');
                var _this = $(e.target);

                _this.prop("disabled", true);


                if (!this.verify_tel(this.tel)) {
                    alert_error_tip($("#form-register"), '手机号码格式错误');
                    _this.prop("disabled", false);
                    return;
                }

                var reserve_telcode = 60
                var send_telcode_interval = setInterval(() => {
                    _this.text('重新发送' + --reserve_telcode + '秒');
                    if (reserve_telcode <= 0) {
                        _this.prop("disabled", false);
                        _this.text("发送短信验证码");
                        clearInterval(send_telcode_interval);
                    }
                }, 1000);

                senddata({
                    url: api_url + 'communication/sendtelcode',
                    data: { 'tel': this.tel, 'option': 0 },
                    call: (in_data) => {
                        if (in_data['code'] == 10001) {
                            clearInterval(send_telcode_interval);
                            _this.prop("disabled", false);
                            _this.text("发送短信验证码");
                            alert_error_tip($("#form-register"), in_data['msg']);
                        } else if (in_data['code'] != 200) {
                            alert_error_tip($("#form-register"), in_data['msg']);
                            return;
                        } else {
                            alert_success_tip($("#form-register"), '短信发送成功,请注意查收.');
                        }
                    }
                })
            },
            /**
             * 注册
             * */
            register: function (e) {
                var _this = $(e.target);
                _this.prop("disabled", true);

                if (this.tel == "" ||
                    this.telcode == "" ||
                    this.password == "") {
                    alert_error_tip($("#form-register"), '请将信息填写完整');
                    _this.prop("disabled", false);

                    return;
                }

                //验证手机号码
                if (!this.verify_tel(this.tel)) {
                    alert_error_tip($("#form-register"), '手机号码格式错误');
                    _this.prop("disabled", false);
                    return;
                }
                //验证密码
                if (!this.verify_password(this.password)) {
                    alert_error_tip($("#form-register"), '密码格式错误');
                    _this.prop("disabled", false);
                    return;
                }
                senddata({
                    url: api_url + 'users/register',
                    data: { 'tel': this.tel, 'telcode': this.telcode, 'password': this.password },
                    call: (in_data) => {
                        _this.prop("disabled", false);
                        if (in_data['code'] != 200) {
                            alert_error_tip($("#form-register"), in_data['msg']);
                        } else {
                            alert_success_tip($("#form-register"), in_data['msg']);
                            this.tel = "";
                            this.telcode = "";
                            this.password = "";
                        }
                    }
                })
            },
            /**
             * 验证手机号码
             * @param {any} val
             */
            verify_tel: function (val) {
                var tel_reg = /1((3\d)|(4[5-9])|(5[0-35-9])|(66)|(7[0-8])|(8\d)|(9[8-9]))\d{8}/;
                return tel_reg.test(val);
            },
            verify_password: function (val) {
                var password_reg = [/[a-zA-Z]+/, /\d+/, /^[^\u4e00-\u9fa5]+$/];
                return (val.length >= 6 && val.length <= 18) && password_reg[0].test(val) && password_reg[1].test(val);
            }
            ,
            /**
             * 手机号码是否存在
             * @param {any} call
             */
            tel_exist: function (call) {
                if (!this.verify_tel(this.tel))
                    return;
                senddata({
                    url: api_url + 'users/exist',
                    data: { 'tel': this.tel },
                    call: (in_data) => {
                        if (call)
                            call(in_data);
                    }
                })
            }
        },
        watch: {
            tel: function () {
                this.tel_exist((in_data) => {
                    if (in_data['code'] != 0)
                        alert_error_tip($("#form-register"), in_data['msg']);
                    else if (in_data['code'] == 0) {
                        alert_success_tip($("#form-register"), in_data['msg']);
                    }
                })
            }
        }
    })
})