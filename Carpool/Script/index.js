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
    /*     乘客     */
    //打车
    var perList = new Vue({
        el: "#customer-list",
        data: {
            customer_list: '',
            startplace: "",
            endplace: "",
            searchcondition: {
                curpage: 1,
                pagecount: 10,
                pages: 1,
                startplace: "",
                endplace: "",
                identity: 1
            }
        },
        methods: {
            refresh: function () {
                this.resetsearch_data();
                senddata({
                    url: api_url + 'carpool/searchlist',
                    data: this.searchcondition,
                    call: (in_data) => {
                        if (in_data['code'] == 200) {
                            data = in_data['data'];
                            this.customer_list = data['data'];
                            this.searchcondition.pages = data['pages'];
                        }
                    }
                })
            },
            resetsearch_data: function () {
                this.searchcondition.curpage = 1;
                this.searchcondition.pagecount = 5;
                this.searchcondition.pages = 1;
                this.searchcondition.startplace = '';
                this.searchcondition.endplace = '';
                this.searchcondition.identity = 1;
            },
            setsearchcondition: function (condition) {

                for (var item in condition)
                    this.searchcondition[item] = condition[item];

                //this.setsearchcondition.startplace = $(this.$el).find('.startplace').val();
                //this.setsearchcondition.endplace = $(this.$el).find('.endplace').val();
            },
            btn_search: function () {
                this.resetsearch_data();
                this.setsearchcondition({
                    'startplace': $(this.$el).find('.startplace').val(),
                    'endplace': $(this.$el).find('.endplace').val()
                });
                this.search();
            },
            search: function () {
                senddata({
                    url: api_url + 'carpool/searchlist',
                    data: this.searchcondition,
                    call: (in_data) => {
                        if (in_data['code'] == 200) {
                            data = in_data['data'];
                            this.customer_list = data['data'];
                            this.searchcondition.pages = data['pages'];
                        }
                    }
                })
            },
            page_prev: function () {
                this.setsearchcondition({
                    'curpage': this.searchcondition['curpage'] - 1
                });
                this.search();
            },
            page_next: function () {
                this.setsearchcondition({
                    'curpage': this.searchcondition['curpage'] + 1
                });
                this.search();
            },
            page_number_search: function (page_id) {
                this.setsearchcondition({
                    'curpage': page_id
                });
                this.search();
            },
            format_date: function (time, fmt) {
                return new Date(time).format(fmt);
            }
        }
    }).refresh();
    //上下班
    new Vue({
        el: "#customer-work",
        data: {
            startplace: "",
            endplace: "",
            paytype: -1,
            price: '',
            way: "",
            remarks: ""
        },
        methods: {
            init: function () {
                $(this.$el).find('.starttime').fdatepicker({
                    format: 'yyyy-mm-dd hh:ii',
                    pickTime: true
                })
            },
            clear_data: function () {
                this.startplace = "";
                this.endplace = "";
                this.paytype = -1;
                this.price = "";
                this.way = "";
                this.remarks = "";

                $(this.$el).find('input.way:not(:first)').parent().parent().parent().remove();
                $(this.$el).find('input.way').val('');
            },
            addway: function (e) {
                if ($(this.$el).find(".row.way").length >= 5)
                    return;

                var elem = $('<div class="row mt-4 way"><div class="col-12"><div class="input-group mb-3"><div class="input-group-prepend"><span class="input-group-text">经</span></div><input type="text" class="form-control way" placeholder="途径(可选,最多5个)"><div class="input-group-append"><button class="btn btn-primary" type="submit">移除</button></div></div></div></div>');
                $(this.$el).find(".row.way:last").after(elem);
                //elem.on('click', elem.find('button[type=submit]'), this.removeway);
                elem.find('button[type=submit]').bind('click', this.removeway);
            },
            removeway: function (e) {
                $(e.target).parent().parent().parent().parent().remove();
            },
            publish: function () {
                if (this.startplace == "" ||
                    this.endplace == "" ||
                    this.starttime == "" ||
                    this.paytype == -1) {

                    alert_error_tip($(".container.page-2"), '请将发布信息填写完整');
                    return;
                }

                if (this.paytype == 2 && isNaN(parseFloat(this.price))) {
                    alert_error_tip($(".container.page-2"), '请填写好价格');
                    return;
                }

                if (Date.parse($(this.$el).find('.starttime').val()) <= Date.now()) {
                    alert_error_tip($(".container.page-2"), '出发时间不能比现在更早');
                    return;
                }
                this.way = "";
                var el_way = $(this.$el).find('input.way');

                for (var i = 0; i < el_way.length; i++) {
                    if (el_way.eq(i).val() != "") {
                        this.way += el_way.eq(i).val();
                        if (i + 1 < el_way.length)
                            this.way += ',';
                    }
                }

                senddata({
                    url: api_url + 'carpool/publishcustomer',
                    data: {
                        'startplace': this.startplace,
                        'endplace': this.endplace,
                        'way': this.way,
                        'starttime': $(this.$el).find('.starttime').val(),
                        'remarks': this.remarks,
                        'or_type': 0,
                        'paytype': this.paytype,
                        'price': this.price,
                        'CarpoolSSID': $.cookie('CarpoolSSID')
                    },
                    call: (in_data) => {
                        if (in_data['code'] == 15001) {
                            alert_error_tip($(".container.page-2"), in_data['msg'])
                            Login_Show(logined);
                        } else if (in_data['code'] == 200) {
                            
                            this.clear_data();
                            alert_success_tip($(".container.page-2"), in_data['msg']);
                        } else alert_error_tip($(".container.page-2"), in_data['msg'])
                    }
                })
            }
        }
    }).init();

    //长途
    new Vue({
        el: "#customer-long",
        data: {
            startplace: "",
            endplace: "",
            paytype: -1,
            price: '',
            remarks: ""
        },
        methods: {
            init: function () {
                $(this.$el).find('.starttime').fdatepicker({
                    format: 'yyyy-mm-dd hh:ii',
                    pickTime: true
                })
            },
            clear_data: function () {
                this.startplace = "";
                this.endplace = "";
                this.paytype = -1;
                this.price = "";
                this.remarks = "";

                $(this.$el).find('input.way:not(:first)').parent().parent().parent().remove();
                $(this.$el).find('input.way').val('');
            },
            addway: function (e) {
                if ($(this.$el).find(".row.way").length >= 5)
                    return;

                var elem = $('<div class="row mt-4 way"><div class="col-12"><div class="input-group mb-3"><div class="input-group-prepend"><span class="input-group-text">经</span></div><input type="text" class="form-control way" placeholder="途径(可选,最多5个)"><div class="input-group-append"><button class="btn btn-primary" type="submit">移除</button></div></div></div></div>');
                $(this.$el).find(".row.way:last").after(elem);
                //elem.on('click', elem.find('button[type=submit]'), this.removeway);
                elem.find('button[type=submit]').bind('click', this.removeway);
            },
            removeway: function (e) {
                $(e.target).parent().parent().parent().parent().remove();
            },
            publish: function () {
                if (this.startplace == "" ||
                    this.endplace == "" ||
                    this.starttime == "" ||
                    this.paytype == -1) {
                    console.log()
                    alert_error_tip($(".container.page-2"), '请将发布信息填写完整');
                    return;
                }

                if (this.paytype == 2 && isNaN(parseFloat(this.price))) {
                    alert_error_tip($(".container.page-2"), '请填写好价格');
                    return;
                }

                if (Date.parse($(this.$el).find('.starttime').val()) <= Date.now()) {
                    alert_error_tip($(".container.page-2"), '出发时间不能比现在更早');
                    return;
                }

                //var el_way = $(this.$el).find('input.way');

                //for (var i = 0; i < el_way.length; i++) {
                //    if (el_way.eq(i).val() != "")
                //        this.way += el_way.eq(i).val() + ',';
                //}

                senddata({
                    url: api_url + 'carpool/publishcustomer',
                    data: {
                        'startplace': this.startplace,
                        'endplace': this.endplace,
                        'way': this.way,
                        'starttime': $(this.$el).find('.starttime').val(),
                        'remarks': this.remarks,
                        'or_type': 1,//长途
                        'paytype': this.paytype,
                        'price': this.price,
                        'CarpoolSSID': $.cookie('CarpoolSSID')
                    },
                    call: (in_data) => {
                        if (in_data['code'] == 15001) {
                            alert_error_tip($(".container.page-2"), in_data['msg'])
                            Login_Show(logined);
                        } else if (in_data['code'] == 200) {
                            
                            this.clear_data();
                            alert_success_tip($(".container.page-2"), in_data['msg']);
                        } else alert_error_tip($(".container.page-2"), in_data['msg'])
                    }
                })
            }
        }
    }).init();



    /*     司机     */

    //拉客
    var carLisst = new Vue({
        el: "#driver-list",
        data: {
            driver_list: '',
            startplace: "",
            endplace: "",
            searchcondition: {
                curpage: 1,
                pagecount: 10,
                pages: 1,
                startplace: "",
                endplace: "",
                identity: 0
            }
        },
        methods: {
            refresh: function () {
                this.resetsearch_data();
                this.search();
                //senddata({
                //    url: api_url + 'carpool/searchlist',
                //    data: this.searchcondition,
                //    call: (in_data) => {
                //        if (in_data['code'] == 200) {
                //            data = in_data['data'];
                //            this.driver_list = data['data'];
                //            this.searchcondition.pages = data['pages'];
                //        }
                //    }
                //})
            },
            resetsearch_data: function () {
                this.searchcondition.curpage = 1;
                this.searchcondition.pagecount = 5;
                this.searchcondition.pages = 1;
                this.searchcondition.startplace = '';
                this.searchcondition.endplace = '';
                this.searchcondition.identity = 0;
            },
            setsearchcondition: function (condition) {

                for (var item in condition)
                    this.searchcondition[item] = condition[item];

                //this.setsearchcondition.startplace = $(this.$el).find('.startplace').val();
                //this.setsearchcondition.endplace = $(this.$el).find('.endplace').val();
            },
            btn_search: function () {
                this.resetsearch_data();
                this.setsearchcondition({
                    'startplace': $(this.$el).find('.startplace').val(),
                    'endplace': $(this.$el).find('.endplace').val()
                });
                this.search();
            },
            search: function () {
                senddata({
                    url: api_url + 'carpool/searchlist',
                    data: this.searchcondition,
                    call: (in_data) => {
                        if (in_data['code'] == 200) {
                            data = in_data['data'];
                            this.driver_list = data['data'];
                            this.searchcondition.pages = data['pages'];
                        }
                    }
                })
            },
            page_prev: function () {
                this.setsearchcondition({
                    'curpage': this.searchcondition['curpage'] - 1
                });
                this.search();
            },
            page_next: function () {
                this.setsearchcondition({
                    'curpage': this.searchcondition['curpage'] + 1
                });
                this.search();
            },
            page_number_search: function (page_id) {
                this.setsearchcondition({
                    'curpage': page_id
                });
                this.search();
            },
            format_date: function (time, fmt) {
                return new Date(time).format(fmt);
            }
        }
    }).refresh();
    //上下班
    new Vue({
        el: "#driver-work",
        data: {
            startplace: "",
            endplace: "",
            paytype: -1,
            price: '',
            way: "",
            remarks: ""
        },
        methods: {
            init: function () {
                $(this.$el).find('.starttime').fdatepicker({
                    format: 'yyyy-mm-dd hh:ii',
                    pickTime: true
                })
            },
            clear_data: function () {
                this.startplace = "";
                this.endplace = "";
                this.paytype = -1;
                this.price = "";
                this.way = "";
                this.remarks = "";

                $(this.$el).find('input.way:not(:first)').parent().parent().parent().remove();
                $(this.$el).find('input.way').val('');
            },
            addway: function (e) {
                if ($(this.$el).find(".row.way").length >= 5)
                    return;

                var elem = $('<div class="row mt-4 way"><div class="col-12"><div class="input-group mb-3"><div class="input-group-prepend"><span class="input-group-text">经</span></div><input type="text" class="form-control way" placeholder="途径(可选,最多5个)"><div class="input-group-append"><button class="btn btn-primary" type="submit">移除</button></div></div></div></div>');
                $(this.$el).find(".row.way:last").after(elem);
                //elem.on('click', elem.find('button[type=submit]'), this.removeway);
                elem.find('button[type=submit]').bind('click', this.removeway);
            },
            removeway: function (e) {
                $(e.target).parent().parent().parent().parent().remove();
            },
            publish: function () {
                if (this.startplace == "" ||
                    this.endplace == "" ||
                    this.starttime == "" ||
                    this.paytype == -1) {

                    alert_error_tip($(".container.page-3"), '请将发布信息填写完整');
                    return;
                }

                if (this.paytype == 2 && isNaN(parseFloat(this.price))) {
                    alert_error_tip($(".container.page-3"), '请填写好价格');
                    return;
                }

                if (Date.parse($(this.$el).find('.starttime').val()) <= Date.now()) {
                    alert_error_tip($(".container.page-3"), '出发时间不能比现在更早');
                    return;
                }
                this.way = "";
                var el_way = $(this.$el).find('input.way');
                for (var i = 0; i < el_way.length; i++) {
                    if (el_way.eq(i).val() != "") {
                        this.way += el_way.eq(i).val();
                        if (i + 1 < el_way.length)
                            this.way += ',';
                    }
                }

                senddata({
                    url: api_url + 'carpool/publishdriver',
                    data: {
                        'startplace': this.startplace,
                        'endplace': this.endplace,
                        'way': this.way,
                        'starttime': $(this.$el).find('.starttime').val(),
                        'remarks': this.remarks,
                        'or_type': 0,
                        'paytype': this.paytype,
                        'price': this.price,
                        'CarpoolSSID': $.cookie('CarpoolSSID')
                    },
                    call: (in_data) => {
                        if (in_data['code'] == 15001) {
                            alert_error_tip($(".container.page-3"), in_data['msg'])
                            Login_Show(logined);
                        } else if (in_data['code'] == 15003) {
                            alert_error_tip($(".container.page-3"), '您不是司机，<a class="text-primary">申请成为司机</a>', 10000);
                            return;
                        } else if (in_data['code'] == 200) {
                            
                            this.clear_data();
                            alert_success_tip($(".container.page-3"), in_data['msg']);
                        } else alert_error_tip($(".container.page-3"), in_data['msg'])
                    }
                })
            }
        }
    }).init();

    //长途
    new Vue({
        el: "#driver-long",
        data: {
            startplace: "",
            endplace: "",
            paytype: -1,
            price: '',
            remarks: ""
        },
        methods: {
            init: function () {
                $(this.$el).find('.starttime').fdatepicker({
                    format: 'yyyy-mm-dd hh:ii',
                    pickTime: true
                })
            },
            clear_data: function () {
                this.startplace = "";
                this.endplace = "";
                this.paytype = -1;
                this.price = "";
                this.remarks = "";
            },
            addway: function (e) {
                if ($(this.$el).find(".row.way").length >= 5)
                    return;

                var elem = $('<div class="row mt-4 way"><div class="col-12"><div class="input-group mb-3"><div class="input-group-prepend"><span class="input-group-text">经</span></div><input type="text" class="form-control way" placeholder="途径(可选,最多5个)"><div class="input-group-append"><button class="btn btn-primary" type="submit">移除</button></div></div></div></div>');
                $(this.$el).find(".row.way:last").after(elem);
                //elem.on('click', elem.find('button[type=submit]'), this.removeway);
                elem.find('button[type=submit]').bind('click', this.removeway);
            },
            removeway: function (e) {
                $(e.target).parent().parent().parent().parent().remove();
            },
            publish: function () {
                if (this.startplace == "" ||
                    this.endplace == "" ||
                    this.starttime == "" ||
                    this.paytype == -1) {
                    console.log()
                    alert_error_tip($(".container.page-3"), '请将发布信息填写完整');
                    return;
                }

                if (this.paytype == 2 && isNaN(parseFloat(this.price))) {
                    alert_error_tip($(".container.page-3"), '请填写好价格');
                    return;
                }

                if (Date.parse($(this.$el).find('.starttime').val()) <= Date.now()) {
                    alert_error_tip($(".container.page-3"), '出发时间不能比现在更早');
                    return;
                }


                senddata({
                    url: api_url + 'carpool/publishdriver',
                    data: {
                        'startplace': this.startplace,
                        'endplace': this.endplace,
                        'way': this.way,
                        'starttime': $(this.$el).find('.starttime').val(),
                        'remarks': this.remarks,
                        'or_type': 1,//长途
                        'paytype': this.paytype,
                        'price': this.price,
                        'CarpoolSSID': $.cookie('CarpoolSSID')
                    },
                    call: (in_data) => {
                        if (in_data['code'] == 15001) {
                            alert_error_tip($(".container.page-3"), in_data['msg'])
                            Login_Show(logined);
                        } else if (in_data['code'] == 200) {
                            
                            this.clear_data();
                            alert_success_tip($(".container.page-3"), in_data['msg']);
                        } else alert_error_tip($(".container.page-3"), in_data['msg'])
                    }
                })
            }
        }
    }).init();

})