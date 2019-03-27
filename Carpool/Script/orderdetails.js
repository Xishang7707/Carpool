var det;
$(function () {
    //$('[data-toggle="popover"]').popover();
    $('[data-toggle="tooltip"]').tooltip();

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
        locattion.reload();
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
    //详情
    det = new Vue({
        el: "#details-content",
        data: {
            map: '',
            order: '',
            orderdetails: '',
            userinfo: '',
            applycount: '',
            car: '',
            us_id: '',
            distance: '',
            paytype: ['免费', '面议', '一口价'],
            cartype: ['轿车', 'MPV', 'SUV', '跑车', '客车', '其他'],
            isapplyorinvate: -1
        },
        methods: {
            init: function () {
                this.us_id = $.cookie('CarpoolSSID');

                this.map = new BMap.Map("map-content");
                this.map.centerAndZoom(new BMap.Point(116.404, 39.915), 11);
                this.map.enableScrollWheelZoom(true);

                //var addr = ["长沙", '湘潭', '株洲', '衡阳', '永州'];
                //this.searchLine(addr);

                this.getorderinfo();
            },
            searchLine: function (addr) {
                var tmpMap = new BMap.Map();
                var driver = new BMap.DrivingRoute(tmpMap);
                //var addr = ["长沙", '湘潭', '株洲', '衡阳', '永州'];
                var points = new Array();

                var complete_count = 0;
                var tmpDistance = 0;
                (function sh(i, list) {
                    if (i >= list.length - 1)
                        return;

                    driver.search(list[i], list[i + 1]);
                    driver.setSearchCompleteCallback((e) => {
                        tmpDistance += parseFloat(e.getPlan(0).getDistance());
                        //console.log(driver.getResults().taxiFare.distance)
                        complete_count++;
                        points.push(driver.getResults());
                        sh(i + 1, list);
                    })
                })(0, addr);
                var intval = setInterval(() => {
                    if (complete_count < addr.length - 1) return;
                    clearInterval(intval);
                    this.distance = tmpDistance;
                    //driver.getResults().getPlan(0).getRoute(0).getPath();
                    var polyline = [];
                    var marker;
                    var start_icon = new BMap.Icon("images/marker-start.png", new BMap.Size(23, 30), {
                        anchor: new BMap.Size(12, 30)
                    });
                    var way_icon = new BMap.Icon("images/marker-way.png", new BMap.Size(23, 30), {
                        anchor: new BMap.Size(12, 30)
                    });
                    var end_icon = new BMap.Icon("images/marker-end.png", new BMap.Size(23, 30), {
                        anchor: new BMap.Size(13, 30)
                    });
                    var markers = [];
                    for (var i in points) {
                        polyline = polyline.concat(points[i].getPlan(0).getRoute(0).getPath());
                        if (i == 0) {
                            marker = new BMap.Marker(points[i].Rv.point, { icon: start_icon });
                        } else
                            marker = new BMap.Marker(points[i].Rv.point, { icon: way_icon });
                        markers.push(marker);
                    }
                    marker = new BMap.Marker(points[points.length - 1].cv.point, { icon: end_icon });
                    markers.push(marker);
                    //this.map.addOverlay(marker);
                    new BMapLib.MarkerClusterer(this.map, { markers: markers });
                    var lines = new BMap.Polyline(polyline)
                    this.map.addOverlay(lines);

                    var viewPort = this.map.getViewport(polyline)
                    this.map.centerAndZoom(viewPort.center, viewPort.zoom);

                }, 100);
            },
            getorderinfo: function () {
                senddata({
                    url: api_url + 'carpool/getorderdetails',
                    data: { 'or_id': getQuery('id') },
                    call: (in_data) => {
                        if (in_data['code'] == 200) {
                            var data = in_data['data'];
                            this.order = data['order'];
                            this.orderdetails = data['orderdetails'];
                            this.applycount = data['applycount'];
                            this.car = data['car'];
                            this.userinfo = data['userinfo'];

                            if (islogin) {
                                var option = -1;
                                if (this.order['identity'] == 0) {

                                    option = 1;
                                } else if (this.order['identity'] == 1) {
                                    option = 0;
                                }
                                senddata({
                                    url: api_url + 'carpool/isapplyorinvate',
                                    data: { 'option': option, 'CarpoolSSID': $.cookie("CarpoolSSID"), 'or_id': this.order['or_id'] },
                                    call: (in_data) => {
                                        if (in_data['code'] == 200)
                                            this.isapplyorinvate = in_data['data'];
                                    }
                                })
                            }
                        }
                    }
                })
            },
            format_date: function (time, fmt) {
                return new Date(time).format(fmt);
            },
            getchatowner: function () {
                if (islogin())
                    dialog_ok({ title: '联系方式', content: this.userinfo['name'] + ' ' + this.userinfo['tel'] });
                else Login_Show(logined);
            },
            applyorder: function (or_id) {
                senddata({
                    url: api_url + 'carpool/applyorder',
                    data: { 'or_id': or_id, 'CarpoolSSID': $.cookie('CarpoolSSID') },
                    call: (in_data) => {
                        if (in_data['code'] == 200) {
                            alert_success_tip($(this.$el), "成功申请加入拼车,请耐心等待对方的回复");
                            setTimeout(() => {
                                location.reload();
                            }, 3000);
                        } else if (in_data['code'] == 15001) {
                            Login_Show();
                        } else {
                            console.log(in_data);
                            alert_error_tip($(this.$el), in_data['msg']);
                        }
                    }
                })
            }, invateorder: function (or_id) {
                senddata({
                    url: api_url + 'carpool/invateorder',
                    data: { 'or_id': or_id, 'CarpoolSSID': $.cookie('CarpoolSSID') },
                    call: (in_data) => {
                        if (in_data['code'] == 200) {
                            alert_success_tip($(this.$el), "成功邀请对方,请耐心等待对方的回复");
                            setTimeout(() => {
                                location.reload();
                            }, 3000);
                        } else if (in_data['code'] == 15001) {
                            Login_Show();
                        }
                        else {
                            alert_error_tip($(this.$el), in_data['msg']);
                        }
                    }
                })
            }
        },
        watch: {
            order: function (n_v) {
                var line = [];
                line.push(n_v['startplace']);
                var ways = n_v['way'].split(',');
                for (var i = 0; i < ways.length; i++) {
                    if (ways[i] !== null && ways[i] != "")
                        line.push(ways[i]);
                }
                //line = line.concat(ways);
                line.push(n_v['endplace']);
                this.searchLine(line);
            }
        }
    })
    det.init();
})