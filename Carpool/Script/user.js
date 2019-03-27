$(function () {
    if (!islogin()) {
        location.href = 'index.html';
        return;
    }
    //用户
    //基本信息
    var menu_user_info = new Vue({
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
    })
    //修改密码
    var menu_user_updatepassword = new Vue({
        el: '#menu-user-updatepassword',
        data: {
            tel: '',
            telcode: '',
            password: ''
        },
        methods: {
            init: function () {
                this.tel = '';
                this.telcode = '';
                this.password = '';
            },
            //发送短信
            telcode_send: function (e) {
                $(".alert.tip").alert('close');
                var _this = $(e.target);

                _this.prop("disabled", true);


                if (!this.verify_tel(this.tel)) {
                    alert_error_tip($(this.$el), '手机号码格式错误');
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
                    data: { 'tel': this.tel, 'CarpoolSSID': $.cookie('CarpoolSSID'), 'option': 1 },
                    call: (in_data) => {
                        if (in_data['code'] == 10001) {
                            clearInterval(send_telcode_interval);
                            _this.prop("disabled", false);
                            _this.text("发送短信验证码");
                            alert_error_tip($(this.$el), in_data['msg']);
                        } else if (in_data['code'] == 10002) {
                            clearInterval(send_telcode_interval);
                            _this.prop("disabled", false);
                            _this.text("发送短信验证码");
                            alert_error_tip($(this.$el), in_data['msg']);
                        } else if (in_data['code'] != 200) {
                            alert_error_tip($(this.$el), in_data['msg']);
                            return;
                        } else {
                            alert_success_tip($(this.$el), '短信发送成功,请注意查收.');
                        }
                    }
                })
            },
            verify_tel: function (val) {
                var tel_reg = /1((3\d)|(4[5-9])|(5[0-35-9])|(66)|(7[0-8])|(8\d)|(9[8-9]))\d{8}/;
                return tel_reg.test(val);
            },
            verify_password: function (val) {
                var password_reg = [/[a-zA-Z]+/, /\d+/, /^[^\u4e00-\u9fa5]+$/];
                return (val.length >= 6 && val.length <= 18) && password_reg[0].test(val) && password_reg[1].test(val);
            },
            updatepassword: function (e) {
                var _this = $(e.target);
                _this.prop("disabled", true);

                if (this.tel == "" ||
                    this.telcode == "" ||
                    this.password == "") {
                    alert_error_tip($(this.$el), '请将信息填写完整');
                    _this.prop("disabled", false);

                    return;
                }

                //验证手机号码
                if (!this.verify_tel(this.tel)) {
                    alert_error_tip($(this.$el), '手机号码格式错误');
                    _this.prop("disabled", false);
                    return;
                }
                //验证密码
                if (!this.verify_password(this.password)) {
                    alert_error_tip($(this.$el), '密码格式错误');
                    _this.prop("disabled", false);
                    return;
                }
                senddata({
                    url: api_url + 'users/updatepassword',
                    data: { 'tel': this.tel, 'telcode': this.telcode, 'password': this.password, 'CarpoolSSID': $.cookie('CarpoolSSID') },
                    call: (in_data) => {
                        _this.prop("disabled", false);
                        if (in_data['code'] == 15001) {
                            alert_error_tip($(this.$el), in_data['msg']);
                            Login_Show(logined);
                            return;
                        } else if (in_data['code'] != 200) {
                            alert_error_tip($(this.$el), in_data['msg']);
                            return;
                        } else {
                            Login_Logout(loginout);
                            alert_success_tip($(this.$el), in_data['msg'], 4500);
                            this.tel = "";
                            this.telcode = "";
                            this.password = "";
                        }
                    }
                })
            }
        }
    })
    //实名认证
    var menu_user_verified = new Vue({
        el: "#menu-user-verified",
        data: {
            name: '',
            idcard: '',
            userinfo: ''
        },
        methods: {
            init: function () {
                this.name = '';
                this.idcard = '';
                this.userinfo = '';

                senddata({
                    url: api_url + 'users/getinfo',
                    data: { 'CarpoolSSID': $.cookie('CarpoolSSID') },
                    call: (in_data) => {
                        if (in_data['code'] == 200) {
                            this.userinfo = in_data['data'];
                        } else alert_error_tip($(this.$el), "加载失败，请刷新");
                    }
                })
            },
            verified: function (e) {
                var _this = $(e.target);
                _this.prop('disabled', true);
                if (this.name == '' || this.idcard.length != 18) {
                    alert_error_tip($(this.$el), "信息填写错误");
                    _this.prop('disabled', false);
                    return;
                }

                senddata({
                    url: api_url + 'users/verified',
                    data: { 'CarpoolSSID': $.cookie('CarpoolSSID'), 'name': this.name, 'idcard': this.idcard },
                    call: (in_data) => {
                        if (in_data['code'] == 200) {
                            alert_success_tip($(this.$el), '实名成功');
                            this.init();
                        } else {
                            alert_error_tip($(this.$el), in_data['msg']);
                            _this.prop('disabled', false);
                        }
                    }
                })
            }
        }
    });
    //成为司机
    var menu_user_car = new Vue({
        el: '#menu-user-car',
        data: {
            carname: '',
            caridcard: '',
            cartype: -1,
            capacity: '',
            carinfo: ''
        },
        methods: {
            init: function () {
                this.carname = '';
                this.caridcard = '';
                this.cartype = -1;
                this.capacity = '';

                senddata({
                    url: api_url + 'users/getinfo',
                    data: { 'CarpoolSSID': $.cookie('CarpoolSSID') },
                    call: (in_data) => {
                        if (in_data['code'] == 200) {
                            console.log(in_data);
                            if (in_data['data']['name'] == null || in_data['data']['name'] == '')
                                dialog_ok({
                                    title: '提示',
                                    content: '请先实名认证后才能注册成为司机',
                                    ok_call: () => {
                                        location.reload();
                                    }
                                })
                            else
                                senddata({
                                    url: api_url + 'users/menu_user_info',
                                    data: { 'CarpoolSSID': $.cookie('CarpoolSSID') },
                                    call: (in_data) => {
                                        if (in_data['code'] == 200) {
                                            this.carinfo = in_data['data']['car'];
                                        }
                                    }
                                })
                        } else alert_error_tip($(this.$el), "加载失败，请刷新");
                    }
                })
            },
            applycar: function (e) {
                $(e.target).prop('disabled', true);
                if (this.carname == '' ||
                    this.caridcard == '' ||
                    this.cartype == -1 ||
                    this.capacity == '') {
                    alert_error_tip($(this.$el), "请将信息填写完整");
                    $(e.target).prop('disabled', false);
                    return;
                }
                senddata({
                    url: api_url + 'cars/apply',
                    data: {
                        'carname': this.carname,
                        'caridcard': this.caridcard,
                        'cartype': this.cartype,
                        'capacity': this.capacity,
                        'CarpoolSSID': $.cookie('CarpoolSSID')
                    },
                    call: (in_data) => {
                        if (in_data['code'] == 200) {
                            alert_success_tip($(this.$el), '成功申请为司机')
                            setTimeout(() => {
                                location.reload();
                            }, 2000);
                        } else {
                            $(e.target).prop('disabled', false);
                            alert_error_tip($(this.$el), in_data['msg']);
                        }

                    }
                })
            }
        }
    })
    //全部订单
    var menu_order_all = new Vue({
        el: "#menu-order-all",
        data: {
            orderlist: "",
            searchcondition: {
                curpage: 1,
                pagecount: 10,
                pages: 1,
                startplace: "",
                endplace: "",
                identity: 0
            },
            us_id: $.cookie("CarpoolSSID")
        },
        methods: {
            init: function () {
                //this.curpage = 1;
                //this.pages = 1;
                //this.pagecount = 5;
                this.orderlist = "";
                this.getdata();
            },
            getdata: function () {
                senddata({
                    url: api_url + 'carpool/getuserorderall',
                    data: { 'curpage': this.searchcondition['curpage'], 'pagecount': this.searchcondition['pagecount'], 'CarpoolSSID': $.cookie('CarpoolSSID') },
                    call: (in_data) => {
                        if (in_data['code'] == 200) {
                            var data = in_data['data'];
                            this.orderlist = data['data'];
                            this.$set(this.searchcondition, 'pages', data['pages']);
                        } else if (in_data['code'] == 15001) {
                            Login_Show(logined);
                        }
                    }
                })
            },
            person_list_toggle: function (msg, e) {
                $("#person-list-" + msg).collapse('toggle');
            },
            dataformat: function (t, fmt) {
                return new Date(t).format(fmt);
            },
            ordercomplete: function (or_id) {
                senddata({
                    url: api_url + 'carpool/orderoperation',
                    data: { 'or_id': or_id, 'option': 0, 'CarpoolSSID': $.cookie('CarpoolSSID') },
                    call: (in_data) => {
                        if (in_data['code'] == 200)
                            this.getdata();
                    }
                })
            },
            agree: function (or_id, ods_id) {
                senddata({
                    url: api_url + 'carpool/orderoperation',
                    data: { 'or_id': or_id, 'ods_id': ods_id, 'option': 2, 'CarpoolSSID': $.cookie('CarpoolSSID') },
                    call: (in_data) => {
                        if (in_data['code'] == 200)
                            this.getdata();
                        else alert_error_tip($("#menu-order-all"), in_data['status']);
                    }
                })
            },
            disagree: function (or_id, ods_id) {
                senddata({
                    url: api_url + 'carpool/orderoperation',
                    data: { 'or_id': or_id, 'ods_id': ods_id, 'option': 3, 'CarpoolSSID': $.cookie('CarpoolSSID') },
                    call: (in_data) => {
                        if (in_data['code'] == 200)
                            this.getdata();
                        else alert_error_tip($("#menu-order-all"), in_data['status']);
                    }
                })
            }, closeOrder: function (or_id) {
                senddata({
                    url: api_url + 'carpool/orderoperation',
                    data: { 'or_id': or_id, 'option': 1, 'CarpoolSSID': $.cookie('CarpoolSSID') },
                    call: (in_data) => {
                        if (in_data['code'] = 200)
                            this.getdata();
                        else alert_error_tip($("#menu-order-processing"), in_data['status']);
                    }
                })
            },
            completeOrder: function (or_id) {
                senddata({
                    url: api_url + 'carpool/orderoperation',
                    data: { 'or_id': or_id, 'option': 0, 'CarpoolSSID': $.cookie('CarpoolSSID') },
                    call: (in_data) => {
                        if (in_data['code'] == 200)
                            this.getdata();
                        else alert_error_tip($("#menu-order-all"), in_data['status']);
                    }
                })
            },
            setsearchcondition: function (condition) {

                for (var item in condition)
                    this.searchcondition[item] = condition[item];

            },
            page_prev: function () {
                this.setsearchcondition({
                    'curpage': this.searchcondition['curpage'] - 1
                });
                this.getdata();
            },
            page_next: function () {
                this.setsearchcondition({
                    'curpage': this.searchcondition['curpage'] + 1
                });
                this.getdata();
            },
            page_number_search: function (page_id) {
                this.setsearchcondition({
                    'curpage': page_id
                });
                this.getdata();
            },
            acceptApply: function (or_id, ods_id) {
                senddata({
                    url: api_url + 'carpool/orderoperation',
                    data: { 'CarpoolSSID': $.cookie('CarpoolSSID'), 'option': 4, 'or_id': or_id, 'ods_id': ods_id },
                    call: (in_data) => {
                        if (in_data['code'] == 200) {
                            this.init();
                        } else {
                            alert_error_tip($(this.$el), in_data['msg']);
                        }
                    }
                })
            },
            disacceptApply: function (or_id, ods_id) {
                senddata({
                    url: api_url + 'carpool/orderoperation',
                    data: { 'CarpoolSSID': $.cookie('CarpoolSSID'), 'option': 5, 'or_id': or_id, 'ods_id': ods_id },
                    call: (in_data) => {
                        if (in_data['code'] == 200) {
                            this.init();
                        } else {
                            alert_error_tip($(this.$el), in_data['msg']);
                        }
                    }
                })
            },
            payfor: function (ods_id) {
                senddata({
                    url: api_url + 'carpool/payfor',
                    data: { 'CarpoolSSID': $.cookie('CarpoolSSID'), 'ods_id': ods_id },
                    call: (in_data) => {
                        if (in_data['code'] == 200) {
                            alert_success_tip($(this.$el), "支付成功");
                            this.init();
                        }
                        else alert_error_tip($(this.$el), in_data['msg']);
                    }
                })
            }
        }
    })
    //进行中订单
    var menu_order_processing = new Vue({
        el: "#menu-order-processing",
        data: {
            searchcondition: {
                curpage: 1,
                pagecount: 10,
                pages: 1,
                startplace: "",
                endplace: "",
                identity: 0
            },
            orderlist: "",
            us_id: $.cookie("CarpoolSSID")
        },
        methods: {
            init: function () {
                this.orderlist = "";

                this.getdata();
            },
            getdata: function () {
                senddata({
                    url: api_url + 'carpool/getuserorderprocessing',
                    data: { 'curpage': this.searchcondition['curpage'], 'pagecount': this.searchcondition['pagecount'], 'CarpoolSSID': $.cookie('CarpoolSSID') },
                    call: (in_data) => {
                        if (in_data['code'] == 200) {
                            var data = in_data['data'];
                            this.orderlist = data['data'];
                            this.$set(this.searchcondition, 'pages', data['pages']);
                        }
                    }
                })
            },
            person_list_toggle: function (msg, e) {
                $("#person-list-processing-" + msg).collapse('toggle');
            },
            dataformat: function (t, fmt) {
                return new Date(t).format(fmt);
            },
            ordercomplete: function (or_id) {
                senddata({
                    url: api_url + 'carpool/orderoperation',
                    data: { 'or_id': or_id, 'option': 0, 'CarpoolSSID': $.cookie('CarpoolSSID') },
                    call: (in_data) => {
                        if (in_data['code'] == 200) {
                            this.getdata();
                        }
                    }
                })
            },
            agree: function (or_id, ods_id) {
                senddata({
                    url: api_url + 'carpool/orderoperation',
                    data: { 'or_id': or_id, 'ods_id': ods_id, 'option': 2, 'CarpoolSSID': $.cookie('CarpoolSSID') },
                    call: (in_data) => {
                        if (in_data['code'] == 200)
                            this.getdata();
                        else alert_error_tip($("#menu-order-processing"), in_data['status']);
                    }
                })
            },
            disagree: function (or_id, ods_id) {
                senddata({
                    url: api_url + 'carpool/orderoperation',
                    data: { 'or_id': or_id, 'ods_id': ods_id, 'option': 3, 'CarpoolSSID': $.cookie('CarpoolSSID') },
                    call: (in_data) => {
                        if (in_data['code'] == 200)
                            this.getdata();
                        else alert_error_tip($("#menu-order-processing"), in_data['status']);
                    }
                })
            },
            closeOrder: function (or_id) {
                senddata({
                    url: api_url + 'carpool/orderoperation',
                    data: { 'or_id': or_id, 'option': 1, 'CarpoolSSID': $.cookie('CarpoolSSID') },
                    call: (in_data) => {
                        if (in_data['code'] = 200)
                            this.getdata();
                        else alert_error_tip($("#menu-order-processing"), in_data['status']);
                    }
                })
            },
            completeOrder: function (or_id) {
                senddata({
                    url: api_url + 'carpool/orderoperation',
                    data: { 'or_id': or_id, 'option': 0, 'CarpoolSSID': $.cookie('CarpoolSSID') },
                    call: (in_data) => {
                        if (in_data['code'] == 200)
                            this.getdata();
                        else alert_error_tip($("#menu-order-processing"), in_data['status']);
                    }
                })
            },
            setsearchcondition: function (condition) {

                for (var item in condition)
                    this.searchcondition[item] = condition[item];

            },
            page_prev: function () {
                this.setsearchcondition({
                    'curpage': this.searchcondition['curpage'] - 1
                });
                this.getdata();
            },
            page_next: function () {
                this.setsearchcondition({
                    'curpage': this.searchcondition['curpage'] + 1
                });
                this.getdata();
            },
            page_number_search: function (page_id) {
                this.setsearchcondition({
                    'curpage': page_id
                });
                this.getdata();
            },
            acceptApply: function (or_id, ods_id) {
                senddata({
                    url: api_url + 'carpool/orderoperation',
                    data: { 'CarpoolSSID': $.cookie('CarpoolSSID'), 'option': 4, 'or_id': or_id, 'ods_id': ods_id },
                    call: (in_data) => {
                        if (in_data['code'] == 200) {
                            this.init();
                        } else {
                            alert_error_tip($(this.$el), in_data['msg']);
                        }
                    }
                })
            },
            disacceptApply: function (or_id, ods_id) {
                senddata({
                    url: api_url + 'carpool/orderoperation',
                    data: { 'CarpoolSSID': $.cookie('CarpoolSSID'), 'option': 5, 'or_id': or_id, 'ods_id': ods_id },
                    call: (in_data) => {
                        if (in_data['code'] == 200) {
                            this.init();
                        } else {
                            alert_error_tip($(this.$el), in_data['msg']);
                        }
                    }
                })
            },
            payfor: function (ods_id) {
                senddata({
                    url: api_url + 'carpool/payfor',
                    data: { 'CarpoolSSID': $.cookie('CarpoolSSID'), 'ods_id': ods_id },
                    call: (in_data) => {
                        if (in_data['code'] == 200) {
                            alert_success_tip($(this.$el), "支付成功");
                            this.init();
                        }
                        else alert_error_tip($(this.$el), in_data['msg']);
                    }
                })
            }
        }
    })
    //完成订单
    var menu_order_completed = new Vue({
        el: "#menu-order-completed",
        data: {
            searchcondition: {
                curpage: 1,
                pagecount: 10,
                pages: 1,
                startplace: "",
                endplace: "",
                identity: 0
            },
            orderlist: "",
            us_id: $.cookie("CarpoolSSID")
        },
        methods: {
            init: function () {
                this.orderlist = "";

                this.getdata();
            },
            getdata: function () {
                senddata({
                    url: api_url + 'carpool/getuserordercompleted',
                    data: { 'curpage': this.searchcondition['curpage'], 'pagecount': this.searchcondition['pagecount'], 'CarpoolSSID': $.cookie('CarpoolSSID') },
                    call: (in_data) => {
                        if (in_data['code'] == 200) {
                            var data = in_data['data'];
                            this.orderlist = data['data'];
                            this.$set(this.searchcondition, 'pages', data['pages']);
                        }
                    }
                })
            },
            person_list_toggle: function (msg, e) {
                $("#person-list-completed-" + msg).collapse('toggle');
            },
            dataformat: function (t, fmt) {
                return new Date(t).format(fmt);
            },
            ordercomplete: function (or_id) {
                senddata({
                    url: api_url + 'carpool/orderoperation',
                    data: { 'or_id': or_id, 'option': 0, 'CarpoolSSID': $.cookie('CarpoolSSID') },
                    call: (in_data) => {

                    }
                })
            },
            agree: function (or_id, ods_id) {
                senddata({
                    url: api_url + 'carpool/orderoperation',
                    data: { 'or_id': or_id, 'ods_id': ods_id, 'option': 2, 'CarpoolSSID': $.cookie('CarpoolSSID') },
                    call: (in_data) => {
                        if (in_data['code'] = 200)
                            this.getdata();
                        else alert_error_tip($("#menu-order-processing"), in_data['status']);
                    }
                })
            },
            disagree: function (or_id, ods_id) {
                senddata({
                    url: api_url + 'carpool/orderoperation',
                    data: { 'or_id': or_id, 'ods_id': ods_id, 'option': 3, 'CarpoolSSID': $.cookie('CarpoolSSID') },
                    call: (in_data) => {
                        if (in_data['code'] = 200)
                            this.getdata();
                        else alert_error_tip($("#menu-order-processing"), in_data['status']);
                    }
                })
            },
            closeOrder: function (or_id) {
                senddata({
                    url: api_url + 'carpool/orderoperation',
                    data: { 'or_id': or_id, 'option': 1, 'CarpoolSSID': $.cookie('CarpoolSSID') },
                    call: (in_data) => {
                        if (in_data['code'] = 200)
                            this.getdata();
                        else alert_error_tip($("#menu-order-processing"), in_data['status']);
                    }
                })
            },
            completeOrder: function (or_id) {
                senddata({
                    url: api_url + 'carpool/orderoperation',
                    data: { 'or_id': or_id, 'option': 0, 'CarpoolSSID': $.cookie('CarpoolSSID') },
                    call: (in_data) => {
                        if (in_data['code'] = 200)
                            this.getdata();
                        else alert_error_tip($("#menu-order-processing"), in_data['status']);
                    }
                })
            },
            setsearchcondition: function (condition) {

                for (var item in condition)
                    this.searchcondition[item] = condition[item];

            },
            page_prev: function () {
                this.setsearchcondition({
                    'curpage': this.searchcondition['curpage'] - 1
                });
                this.getdata();
            },
            page_next: function () {
                this.setsearchcondition({
                    'curpage': this.searchcondition['curpage'] + 1
                });
                this.getdata();
            },
            page_number_search: function (page_id) {
                this.setsearchcondition({
                    'curpage': page_id
                });
                this.getdata();
            },
            acceptApply: function (or_id, ods_id) {
                senddata({
                    url: api_url + 'carpool/orderoperation',
                    data: { 'CarpoolSSID': $.cookie('CarpoolSSID'), 'option': 4, 'or_id': or_id, 'ods_id': ods_id },
                    call: (in_data) => {
                        if (in_data['code'] == 200) {
                            this.init();
                        } else {
                            alert_error_tip($(this.$el), in_data['msg']);
                        }
                    }
                })
            },
            disacceptApply: function (or_id, ods_id) {
                senddata({
                    url: api_url + 'carpool/orderoperation',
                    data: { 'CarpoolSSID': $.cookie('CarpoolSSID'), 'option': 5, 'or_id': or_id, 'ods_id': ods_id },
                    call: (in_data) => {
                        if (in_data['code'] == 200) {
                            this.init();
                        } else {
                            alert_error_tip($(this.$el), in_data['msg']);
                        }
                    }
                })
            },
            payfor: function (ods_id) {
                senddata({
                    url: api_url + 'carpool/payfor',
                    data: { 'CarpoolSSID': $.cookie('CarpoolSSID'), 'ods_id': ods_id },
                    call: (in_data) => {
                        if (in_data['code'] == 200) {
                            alert_success_tip($(this.$el), "支付成功");
                            this.init();
                        }
                        else alert_error_tip($(this.$el), in_data['msg']);
                    }
                })
            }
        }
    })
    //全部通知
    var menu_notice_all = new Vue({
        el: "#menu-notice-all",
        data: {
            instance: $(this.$el),
            datalist: '',
            searchcondition: {
                curpage: 1,
                pagecount: 10,
                pages: 1,
                startplace: "",
                endplace: "",
                identity: 0
            },
            us_id: $.cookie("CarpoolSSID")
        },
        methods: {
            refresh: function () {
                this.searchcondition = {
                    curpage: 1,
                    pagecount: 10,
                    pages: 1,
                    startplace: "",
                    endplace: "",
                    identity: 0
                };

                this.getdata();
            },
            getdata: function () {
                senddata({
                    url: api_url + 'notification/getnoticeall',
                    data: { 'pagecount': this.searchcondition['pagecount'], 'curpage': this.searchcondition['curpage'], 'CarpoolSSID': $.cookie("CarpoolSSID") },
                    call: (in_data) => {
                        if (in_data['code'] == 200) {
                            var data = in_data['data'];
                            this.datalist = data['data'];

                            this.$set(this.searchcondition, 'pages', data['pages']);
                        } else {
                            alert_error_tip($(document.body), in_data['msg']);
                        }
                    }
                })
            },
            setsearchcondition: function (condition) {

                for (var item in condition)
                    this.searchcondition[item] = condition[item];

            },
            page_prev: function () {
                this.setsearchcondition({
                    'curpage': this.searchcondition['curpage'] - 1
                });
                this.getdata();
            },
            page_next: function () {
                this.setsearchcondition({
                    'curpage': this.searchcondition['curpage'] + 1
                });
                this.getdata();
            },
            page_number_search: function (page_id) {
                this.setsearchcondition({
                    'curpage': page_id
                });
                this.getdata();
            },
            dataformat: function (t, fmt) {
                return new Date(t).format(fmt);
            },
            notice_list_toggle: function (nt_id, nt_data, e) {
                $("#notice-list-all-" + nt_id).collapse('toggle');

                this.notice_deal(nt_id, nt_data);
            },
            notice_deal: function (nt_id, nt_data) {
                if (nt_data['is_deal'] == 1)
                    return;

                senddata({
                    url: api_url + 'notification/noticedeal',
                    data: { 'CarpoolSSID': $.cookie('CarpoolSSID'), 'nt_id': nt_id },
                    call: (in_data) => {

                    }
                })
            }
        }
    })

    var menu_manager = new Vue({
        el: "#menu-manager",
        methods: {
            menu_user_info_refresh: function () {
                menu_user_info.init();
            },
            menu_user_updatepassword_refresh: function () {
                menu_user_updatepassword.init();
            },
            menu_user_verified_refresh: function () {
                menu_user_verified.init();
            },
            menu_user_car_refresh: function () {
                menu_user_car.init();
            },
            menu_order_all_refresh: function () {
                menu_order_all.init();
            },
            menu_order_processing_refresh: function () {
                menu_order_processing.init();
            },
            menu_order_completed_refresh: function () {
                menu_order_completed.init();
            },
            menu_notice_all_refresh: function () {
                menu_notice_all.refresh();
            }
        }
    })

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

                    menu_user_info.init();
                    menu_user_updatepassword.init();
                    menu_order_all.getdata();
                    menu_order_processing.getdata();
                    menu_order_completed.getdata();
                }
            }
        })
    };
    function loginout() {
        $.cookie('CarpoolSSID', '');
        $(".nav-login").removeClass('d-none');
        $(".user-menu").addClass('d-none');
        Login_Show(logined);
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


    var tab = getQuery("menu");
    if (tab == null)
        return;
    switch (tab) {
        case "notice-all":

            break;
    }
})