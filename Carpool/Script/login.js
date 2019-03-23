$(function () {
    //$("[data-target='#Login_dialog']").click(function () {
    //    Login_Show();
    //})
    //$("[data-target='Login_logout']").click(function () {
    //    Login_Logout();
    //})
})
function Login_Show(login_call) {
    var login_model = '<div class="modal fade" id="Login_dialog">' +
        '<div class="modal-dialog modal-dialog-centered ">' +
        '<div class="modal-content">' +
        '<div class="modal-body p-0" style="background:#556080">' +
        '<div class="container p-3">' +
        '<small class="text-white">用户登录</small>' +
        '<button type="button" class="close text-white" data-dismiss="modal">&times;</button>' +
        '<div class="row">' +
        '<div class="col text-center">' +
        '<img class="img-fluid" style="width:45%;" src="images/user-headprotrait/def_headportrait.png" />' +
        '</div>' +
        '</div>' +
        '</div>' +
        '<div style="clear:both">' + '</div>' +
        '<div class="container-fluid p-4" style="background:#fff">' +
        '<div class="row mt-3">' +
        '<div class="col">' +
        '<input type="tel" class="form-control tel" v-model="tel" placeholder="手机号码" maxlength="11" />' +
        '</div>' +
        '</div>' +
        '<div class="row mt-3">' +
        '<div class="col">' +
        '<input type="password" class="form-control password" v-model="password" placeholder="密码" maxlength="18" />' +
        '</div>' +
        '</div>' +
        '<div class="row mt-3">' +
        '<div class="col">' +
        '<button type="button" v-on:click="login" class="btn btn-primary btn-block btn-lg btn-login">登录</button>' +
        '</div>' +
        '</div>' +
        '<div class="row mt-3">' +
        '<div class="col-md-6">' +
        '<button type="button" class="btn btn-block btn-lg btn-register" v-on:click="target_register">' + '<span>注册</span></button>' +
        '</div>' +
        '<div class="col-md-6">' +
        '<button type="button" class="btn btn-block btn-lg btn-findpassword"><span>找回密码</span></button>' +
        '</div>' +
        '</div>' +
        '</div>' +
        '</div>' +
        '</div>' +
        '</div>' +
        '</div>';

    $(document.body).append($(login_model));
    new Vue({
        el: "#Login_dialog",
        data: {
            tel: "",
            password: ""
        },
        methods: {
            init: function () {
                var _this = $(this.$el);
                _this.modal({
                    backdrop: 'static',
                    keyboard: true
                })

                _this.on('hidden.bs.modal', (e) => {
                    $(this.$el).remove();
                })
            },
            show: function () {
                var _this = $(this.$el);
                this.init();
                _this.modal('show');
            },
            login: function (e) {
                var btn_login = $(e.target);
                btn_login.prop('disabled', true);

                if (this.tel == '' || this.password == '') {
                    alert_error_tip($("#Login_dialog"), "请将信息填写完整");
                    btn_login.prop('disabled', false);
                    return;
                }
                btn_login.text("登录中...");
                senddata({
                    url: api_url + 'users/login',
                    data: { 'tel': this.tel, 'password': this.password },
                    call: (in_data) => {
                        if (in_data['code'] == 200) {
                            var data = in_data['data'];
                            $.cookie('CarpoolSSID', data['CarpoolSSID'])
                            alert_success_tip($("#Login_dialog"), '登录成功');
                            btn_login.text("登录成功");
                            setTimeout(() => {
                                $(this.$el).modal('hide');
                            }, 1000);
                        } else {
                            btn_login.prop('disabled', false);
                            alert_error_tip($("#Login_dialog"), in_data['msg']);
                            btn_login.text("登录");
                        }
                        if (login_call)
                            login_call(in_data);
                    }
                })
            },
            target_register: function () {
                location.href = 'register.html';
            }
        }
    }).show();
}

function Login_Logout(logout_call) {
    $.cookie('CarpoolSSID', '');
    if (logout_call)
        logout_call();
}