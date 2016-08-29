var pgmap = {};
pgmap.system = "Google Map";
pgmap.useMarkerCluster = true;
pgmap.iterationRender = 9;
pgmap.center = new google.maps.LatLng(16, 106);
pgmap.zoom = 6;
pgmap.disableUIControl = false;
pgmap.data = null;
pgmap.periodmarker = 'mtd';
pgmap.map = null;
pgmap.markerClusterer = null;
pgmap.markers = [];
pgmap.polylines = [];
pgmap.infoWindow = null;
pgmap.bounds = null;
pgmap.circle = null;
pgmap.statisticMarker = null;
pgmap.colorBy = "Normal"; //Normal, DRS, Segment, Route
pgmap.showCircle = false;
pgmap.showLabels = false;
pgmap.blueMarkers = 0;
pgmap.selection = 0;
pgmap.salesmanLocation = pgmap.center; //luuDN
pgmap.lastOutlet = pgmap.center; // luuDN: last Outlet location
var imagesCustomerUrl = '';
var order_latitude = "0";
var order_longitude = "0";
// sua ngay 10/04
var source = null;
var destination = null;

var customer_id = '';
var distributor_code = '';
var customer_name = '';
var saleRun = 0;

pgmap.init = function (options) {
    if (options == undefined)
        options = {};
    $.extend(pgmap, options);

    var mapOptions = {
        center: pgmap.center,
        mapTypeControl: false,
        scrollwheel: false,
        zoom: pgmap.zoom,
        disableDefaultUI: pgmap.disableUIControl,
        mapTypeId: google.maps.MapTypeId.ROADMAP,
        backgroundColor: '#FFFFFF'
    };

    var zoomOptions = {
        key: "shift",
        boxStyle: {
            border: "1px dashed #00f",
            backgroundColor: "transparent",
            opacity: 1.0
        },
        veilStyle: {
            backgroundColor: "#ccc",
            opacity: 0.5,
            cursor: "crosshair"
        },
        visualEnabled: true
    };

    pgmap.map = new google.maps.Map(document.getElementById('gmap_canvas'), mapOptions);
    pgmap.map.enableKeyDragZoom(zoomOptions);
    pgmap.infoWindow = new google.maps.InfoWindow();
    google.maps.event.addListener(pgmap.map, 'click', function (event) {
        pgmap.infoWindow.close();
    });


    //INSERT VIETNAMESE territorial sovereignty
    var bienDong = new MarkerWithLabel({
        position: new google.maps.LatLng(15.794318, 113.834839),
        draggable: false,
        raiseOnDrag: false,
        map: pgmap.map,
        labelContent: "Biển Đông - Việt Nam",
        labelAnchor: new google.maps.Point(22, 0),
        labelClass: "labelmaps", // the CSS class for the label
        icon: {}
    });

    var HoangSa = new MarkerWithLabel({
        position: new google.maps.LatLng(17.232054, 112.026215),
        draggable: false,
        raiseOnDrag: false,
        map: pgmap.map,
        labelContent: "Quần Đảo Hoàng Sa " + "<br />" + " (Đà Nẵng - Việt Nam)",
        labelAnchor: new google.maps.Point(22, 0),
        labelClass: "labelmaps", // the CSS class for the label
        icon: {}
    });

    var TruongSa = new MarkerWithLabel({
        position: new google.maps.LatLng(11.195326, 115.611877),
        draggable: false,
        raiseOnDrag: false,
        map: pgmap.map,
        labelContent: "Quần Đảo Trường Sa " + "<br />" + " (Khánh Hòa - Việt Nam)",
        labelAnchor: new google.maps.Point(22, 0),
        labelClass: "labelmaps", // the CSS class for the label
        icon: {}
    });
};

pgmap.renderSalesmenMarkers = function (fromIndex) {
    LogTime(fromIndex);
    var toIndex = fromIndex + pgmap.iterationRender;
    if (toIndex > pgmap.data.length)
        toIndex = pgmap.data.length;
    //alert(pgmap.data.length);

    for (var i = fromIndex; i < toIndex; i++) {
        var latLng = new google.maps.LatLng(pgmap.data[i].latitude, pgmap.data[i].longitude);
        var infoLatLng = new google.maps.LatLng(pgmap.data[i].latitude, pgmap.data[i].longitude);

        var labelClass = "labels";
//        if (pgmap.data[i].SyncLate == 1) {
//            labelClass = "labelSMSyncLate";
//        }

        var marker = new MarkerWithLabel({
            position: latLng,
            draggable: false,
            raiseOnDrag: false,
            labelContent: pgmap.data[i].name,
            labelAnchor: new google.maps.Point(pgmap.data[i].name.length * 4, -1),
            //labelAnchor: new google.maps.Point(pgmap.data[i].name.length * 4, 46),
            labelClass: labelClass// the CSS class for the label
        });


        var fn1 = pgmap.markerSalesmenMouseOverFunction(pgmap.data[i], infoLatLng);
        var fn2 = pgmap.markerSalesmenDblClickFunction(pgmap.data[i], infoLatLng);

        google.maps.event.addListener(marker, 'mouseover', fn1);
        google.maps.event.addListener(marker, 'mouseout', function () {
            pgmap.infoWindow.close();
        });
        google.maps.event.addListener(marker, 'click', fn2);
        pgmap.markers.push(marker);
        pgmap.bounds.extend(latLng);

        currlatLng = latLng;
    }
    pgmap.map.panTo(currlatLng);

    if (toIndex >= pgmap.data.length) {
        $("div.loading[obj=map]").hide();
        pgmap.createCircle();
        pgmap.circleToggle();
        //        if (pgmap.showLabels)
        //            $("div.labels").show();
        //        else
        //            $("div.labels").hide();
        window.setTimeout(pgmap.time, 0);
        //        window.setTimeout("ChangeColor('" + pgmap.colorBy + "')", 0);
    }
    else {
        var value = (toIndex * 100) / pgmap.data.length;
        $("div.loading[obj=map] div.progressbar").progressbar({ value: value });
        $("div.loading[obj=map] div.status").text(toIndex + " / " + pgmap.data.length);
        window.setTimeout("pgmap.renderSalesmenMarkers(" + toIndex + ")", 0);
    }

    LogTime('End');
}

// renderMerMarker


pgmap.renderMerMarkers = function (fromIndex) {

    var toIndex = fromIndex + pgmap.iterationRender;
    if (toIndex > pgmap.data.length)
        toIndex = pgmap.data.length;

    for (var i = fromIndex; i < toIndex; i++) {
        var latLng = new google.maps.LatLng(pgmap.data[i].latitude, pgmap.data[i].longitude);


        var contentString = '<div id ="div-slide" class="container">' +
                '<div id="infor-outler">' +
                '<p id="p-outlet"> Outlet:(' + pgmap.data[i].code + ') ' + '<strong>' + pgmap.data[i].name + '</strong>' + '</p>' +
                '</div>' +
                '<div id="slides"> ' +
                pgmap.data[i].file_name +
                '<a href="#" class="slidesjs-previous slidesjs-navigation"><i class="icon-chevron-left icon-large"></i></a>' +
                '<a href="#" class="slidesjs-next slidesjs-navigation"><i class="icon-chevron-right icon-large"></i></a>' +
                '</div>' +
                '</div>';


        var marker = new google.maps.Marker({
            position: latLng,
            map: pgmap.map,
            title: pgmap.data[i].code
        });

        infoWindow = new google.maps.InfoWindow({
            content: ""
        });

        marker.notes = contentString;
        google.maps.event.addListener(marker, 'click', function () {
            infoWindow.width = 1000;
            infoWindow.content = this.notes;
            infoWindow.open(pgmap.map, this);
        });
        google.maps.event.addListener(infoWindow, 'domready', function () {
            // initiate slider here      
            $('#slides').slidesjs({
                width: 960,
                height: 640,
                navigation: false
            });
        });
        pgmap.markers.push(marker);
        pgmap.bounds.extend(latLng);
    }

    if (toIndex >= pgmap.data.length) {
        $("div.loading[obj=map]").hide();
        pgmap.createCircle();
        pgmap.circleToggle();
        window.setTimeout(pgmap.time, 0);
        window.setTimeout("ChangeColor('" + pgmap.colorBy + "')", 0);
    }
    else {
        var value = (toIndex * 100) / pgmap.data.length;
        $("div.loading[obj=map] div.progressbar").progressbar({ value: value });
        $("div.loading[obj=map] div.status").text(toIndex + " / " + pgmap.data.length);
        window.setTimeout("pgmap.renderMerMarkers(" + toIndex + ")", 0);
    }
}

function contains(a, obj) {
    var i = a.length;
    while (i--) {
        if (a[i] === obj) {
            return true;
        }
    }
    return false;
}

var outletPainted = new Array();
pgmap.renderMarkers = function (fromIndex) {
    var toIndex = fromIndex + pgmap.iterationRender;
    if (toIndex > pgmap.data.length)
        toIndex = pgmap.data.length;
    //alert(pgmap.data.length);
    var totalCustomer = pgmap.data.length;
    if (totalCustomer > 0) {
        for (var i = fromIndex; i < toIndex; i++) {
            var latLng = new google.maps.LatLng(pgmap.data[i].latitude, pgmap.data[i].longitude);
            var outletcolor = "00FF00"; //default là màu xanh
            var mcpcolor = "#000000";

            //render status
            switch (pgmap.data[i].status) {
                //1-Dang visit          
                case '1':
                    outletcolor = "FF0000"; // màu đỏ
                    mcpcolor = "#000000"; // màu đen
                    break;
                //Có đơn hàng        
                case '2':
                    outletcolor = "0000FF"; // màu xanh da trời đậm
                    mcpcolor = "#FFFFFF"; // màu trắng
                    break;
                //Không có đơn hàng         
                case '3':
                    outletcolor = "FF0000"; // màu đỏ
                    mcpcolor = "#FFFFFF"; // màu trắng
                    break;
                default:
                    outletcolor = "00FF00"; //default là màu xanh
                    mcpcolor = "#000000"; // màu đen
                    break;
            }

            if (pgmap.data[i].customer_aso > 0) {
                outletcolor = "#FF00FF"; // Màu hồng
            }
            //alert(base_url + 'Content/img/marker' + outletcolor + '.png');
            //Vi tri cuoi cung cua salesman trung voi vi tri outlet
            //Dau *, vi tri hien tai
            if (pgmap.data[i].status == '1' && pgmap.data[i].order_customercode == pgmap.data[i].code) {
                // gán vị trí OutLet cuối cùng
                pgmap.lastOutlet = latLng;
                var marker = new MarkerWithLabel({
                    position: latLng,
                    draggable: false,
                    map: pgmap.map,
                    icon: base_url + 'Content/img/markerFFFF00.png',
                    labelContent: '<p class="mcp-order" style="color:' + mcpcolor + ';font-size: 13px;" >' + ((pgmap.data[i].geo2).toString()).substring(0, 2) + '</p>' + '<br>' + '<p class="real-order" style="display:inline;color:yellow;background-color:#7cc1f0;" >' + AddFirstZero(pgmap.data[i].visitODReal) + '</p>',
                    labelAnchor: new google.maps.Point(8, 32),
                    labelClass: "labelmaps", // the CSS class for the label
                    labelStyle: { opacity: 0.75 }
                });
            }
            else {
                //IF begin: ve them co mau xanh
                if (pgmap.data[i].visitODReal === 1) {
                    var marker = new MarkerWithLabel({
                        position: latLng,
                        draggable: false,
                        map: pgmap.map,
                        icon: base_url + 'Content/img/marker' + outletcolor + '.png',
                        labelContent: '<p class="mcp-order" style="color:' + mcpcolor + ';font-size: 13px;" >' + ((pgmap.data[i].geo2).toString()).substring(0, 2) + '</p>' + '<br>' + '<p class="real-order" style="display:inline;color:red;background-color:yellow;" >' + AddFirstZero(pgmap.data[i].visitODReal) + '</p>',
                        labelAnchor: new google.maps.Point(8, 32),
                        labelClass: "labelmaps", // the CSS class for the label
                        labelStyle: { opacity: 0.75 }
                    });
                }
                //IF end: ve them co mau do
                else if (i === (totalCustomer - 1)) {
                    // gán vị trí OutLet cuối cùng
                    pgmap.lastOutlet = latLng;
                    var marker = new MarkerWithLabel({
                        position: latLng,
                        draggable: false,
                        map: pgmap.map,
                        icon: base_url + 'Content/img/marker' + outletcolor + '.png',
                        labelContent: '<p class="mcp-order" style="color:' + mcpcolor + ';font-size: 13px;" >' + ((pgmap.data[i].geo2).toString()).substring(0, 2) + '</p>' + '<br>' + '<p class="real-order" style="display:inline;color:yellow;background-color:red;" >' + AddFirstZero(pgmap.data[i].visitODReal) + '</p>',
                        labelAnchor: new google.maps.Point(8, 32),
                        labelClass: "labelmaps", // the CSS class for the label
                        labelStyle: { opacity: 0.75 }
                    });
                }
                else {
                    var marker = new MarkerWithLabel({
                        position: latLng,
                        draggable: false,
                        map: pgmap.map,
                        icon: base_url + 'Content/img/marker' + outletcolor + '.png',
                        labelContent: '<p class="mcp-order" style="color:' + mcpcolor + ';font-size: 13px;" >' + ((pgmap.data[i].geo2).toString()).substring(0, 2) + '</p>' + '<br>' + '<p class="real-order" style="display:inline;color:blue;background-color:#7cc1f0;" >' + AddFirstZero(pgmap.data[i].visitODReal) + '</p>',
                        labelAnchor: new google.maps.Point(8, 32),
                        labelClass: "labelmaps", // the CSS class for the label
                        labelStyle: { opacity: 0.75 }
                    });
                }
            }


            order_latitude = pgmap.data[i].latitude;
            order_longitude = pgmap.data[i].longitude;

            if (pgmap.data[i].visitODReal != 0) {
                if (source == null) {
                    source = latLng;
                    destination = latLng;
                }
                else {
                    source = destination;
                    destination = latLng;
                    var route = [source, destination];
                    var polyline = new google.maps.Polyline({
                        path: route,
                        strokeColor: '#FF0000',
                        strokeOpacity: 1.0,
                        strokeWeight: 2
                    });
                    pgmap.polylines.push(polyline);
                    polyline.setMap(pgmap.map);
                }
            }
            var fn = pgmap.markerClickFunction(pgmap.data[i].geo2, pgmap.data[i], latLng);
            google.maps.event.addListener(marker, 'click', fn);

            //Remove Marker If Dupplicate
            //            var intIndex = pgmap.markers.indexOf(marker);
            //            if (intIndex > -1) {
            //                alert(intIndex);
            //                pgmap.markers.splice(marker, intIndex);
            //            }

            //Add new marker
            pgmap.markers.push(marker);

            pgmap.bounds.extend(latLng);
        }

        if (toIndex >= pgmap.data.length) {
            $("div.loading[obj=map]").hide();
            pgmap.createCircle();
            pgmap.circleToggle();
            window.setTimeout(pgmap.time, 0);
            window.setTimeout("ChangeColor('" + pgmap.colorBy + "')", 0);
        }
        else {
            var value = (toIndex * 100) / pgmap.data.length;
            $("div.loading[obj=map] div.progressbar").progressbar({ value: value });
            $("div.loading[obj=map] div.status").text(toIndex + " / " + pgmap.data.length);
            window.setTimeout("pgmap.renderMarkers(" + toIndex + ")", 0);
        }

        var currlatLng = new google.maps.LatLng(order_latitude, order_longitude);
        pgmap.map.setZoom(16);
        pgmap.map.panTo(currlatLng);
    }
}

//function render if check box rendermarker_second checked
pgmap.renderMarkers_second = function (fromIndex) {
    var toIndex = fromIndex + pgmap.iterationRender;
    if (toIndex > pgmap.data.length)
        toIndex = pgmap.data.length;
    for (var i = fromIndex; i < toIndex; i++) {
        var latLng = new google.maps.LatLng(pgmap.data[i].latitude, pgmap.data[i].longitude);

        var outletcolor = "#00FF00";

        switch (pgmap.data[i].status) {
            case '1':
                outletcolor = "#FFFF00";
                break;
            case '2':
                outletcolor = "#0000CC";
                break;
            case '3':
                outletcolor = "#FF0000";
                break;
            default:
                outletcolor = "#00FF00";
                break;
        }

        if (pgmap.data[i].customer_aso > 0) {
            outletcolor = "#FF00FF";
        }

        if ((pgmap.data[i].order_customercode.replace(/^\s+|\s+$/g, '') == pgmap.data[i].code.replace(/^\s+|\s+$/g, ''))) {
            var marker = new MarkerWithLabel({ position: latLng,
                draggable: false,
                raiseOnDrag: false,
                labelContent: pgmap.data[i].name + "\n" + pgmap.data[i].code + "\n" + pgmap.data[i].address,
                labelAnchor: new google.maps.Point(40, 50),
                labelClass: "labels_second"
            });

        }
        else {
            var marker = new MarkerWithLabel({
                position: latLng,
                draggable: false,
                raiseOnDrag: false,
                labelContent: "<p>" + pgmap.data[i].name + "</p>" +
                        "<br/>" +
                        "<p>DT: " + pgmap.data[i].phone + "</p>" +
                        "<br/>" +
                        "<p>N.Lienhe: " + pgmap.data[i].attn + "</p>",
                labelAnchor: new google.maps.Point(40, 50),
                labelClass: "labels_second"
            });
        }

        order_latitude = pgmap.data[i].latitude;
        order_longitude = pgmap.data[i].longitude;

        pgmap.markers.push(marker);
        pgmap.bounds.extend(latLng);
    }

    if (toIndex >= pgmap.data.length) {
        $("div.loading[obj=map]").hide();
        pgmap.createCircle();
        pgmap.circleToggle();
        window.setTimeout(pgmap.time, 0);
        window.setTimeout("ChangeColor('" + pgmap.colorBy + "')", 0);
    }
    else {
        var value = (toIndex * 100) / pgmap.data.length;
        $("div.loading[obj=map] div.progressbar").progressbar({ value: value });
        $("div.loading[obj=map] div.status").text(toIndex + " / " + pgmap.data.length);
        window.setTimeout("pgmap.renderMarkers_second(" + toIndex + ")", 0);
    }


    var currlatLng = new google.maps.LatLng(order_latitude, order_longitude);
    pgmap.map.setZoom(16);
    pgmap.map.panTo(currlatLng);

}



pgmap.showMarkers = function () {
    //CheckSession();
    pgmap.clear();
    pgmap.infoWindow.close();

    $("div.loading[obj=map]").show();
    window.setTimeout("pgmap.renderMarkers(0)", 0);
    source = null;
    destination = null;
    saleRun = 0;
};

// function showMarker when check box showmarker_second is checked
pgmap.showMarkers_second = function () {
    //CheckSession();
    pgmap.clear();
    pgmap.infoWindow.close();

    $("div.loading[obj=map]").show();
    window.setTimeout("pgmap.renderMarkers_second(0)", 0);
    source = null;
    destination = null;
};

pgmap.showMerMarkers = function () {
    //CheckSession();
    pgmap.clear();
    pgmap.infoWindow.close();

    $("div.loading[obj=map]").show();
    window.setTimeout("pgmap.renderMerMarkers(0)", 0);
};

pgmap.showSalesmenMarkers = function () {
    //CheckSession();
    pgmap.clear();
    pgmap.init();
    pgmap.infoWindow.close();

    $("div.loading[obj=map]").show();
    window.setTimeout("pgmap.renderSalesmenMarkers(0)", 0);
};

pgmap.markerSalesmenMouseOverFunction = function (salesman, latlng) {
    return function (e) {
        //        if (salesman.Avatar != '' && salesman.Avatar != null) {
        //            imagesSalesmanUrl = '<td><div class="info-image"><img src="' + SalesmanAvatarFolder + salesman.Avatar + '" height="100px" /></div></td>';
        //        }
        //        else {
        //            imagesSalesmanUrl = '<td><div class="info-image"><img src="' + SalesmanAvatarFolder + 'no_image.jpg" height="100px" /></div></td>';
        //        }

        var infoHtml = '<div class="info">' +
                '<div class="info-body">' +
                '<table width="350px" border="0" cellpadding="0" cellspacing="0">' +
                '<tr>' +
        //imagesSalesmanUrl +
                '<td>' +
                '<h3>(' + salesman.code + ') ' + salesman.name + '</h3>' +
                '<br/><strong>Mã NVBH</strong>: ' + salesman.code +
                '<br/><strong>Tên NVBH</strong>: ' + salesman.name +
                '<br/><strong>Thời gian</strong>: ' + salesman.visittime +
                '<br/><strong>Latitude</strong>: ' + salesman.latitude +
                '<br/><strong>Longitude</strong>: ' + salesman.longitude +
                '<br/>' +
                '</td>' +
                '</tr></table>' +
                '<br/>' +
                '<br/>' +
                '</div>';

        pgmap.infoWindow.setContent(infoHtml);
        pgmap.infoWindow.setPosition(latlng);
        pgmap.infoWindow.open(pgmap.map);
    }
};

pgmap.markerSalesmenDblClickFunction = function (salesman, latlng) {
    return function (e) {
        filterTest(salesman.code);
        LoadSalesmanInfo();
        pgmap.salesmanLocation = latlng;
        //ST : Add Salesman marker
        var marker = new google.maps.Marker({
            position: latlng,
            map: pgmap.map
            //icon: image
        });
        var fnSalesman1 = pgmap.markerSalesmenClickFunction(salesman, latlng);
        google.maps.event.addListener(marker, 'click', fnSalesman1);
    }
};

//luudn - show tooltip when click on marker when show all outlets marker of salesman and salesman
pgmap.markerSalesmenClickFunction = function (salesman, latlng) {
    //CheckSession();
    return function (e) {
        if (salesman.Avatar != '' && salesman.Avatar != null) {
            imagesSalesmanUrl = '<td><div class="info-image"><img src="' + SalesmanAvatarFolder + salesman.Avatar + '" height="100px" /></div></td>';
        }
        else {
            imagesSalesmanUrl = '<td><div class="info-image"><img src="' + SalesmanAvatarFolder + 'no_image.jpg" height="100px" /></div></td>';
        }

        var infoHtml = '<div class="info">' +
                '<div class="info-body">' +
                '<table width="350px" border="0" cellpadding="0" cellspacing="0">' +
                '<tr>' +
        //imagesSalesmanUrl +
                '<td>' +
                '<h3>(' + salesman.code + ') ' + salesman.name + '</h3>' +
                '<br/><strong>Mã NVBH</strong>: ' + salesman.code +
                '<br/><strong>Tên NVBH</strong>: ' + salesman.name +
                '<br/><strong>Thời gian</strong>: ' + salesman.visittime +
                '<br/><strong>Latitude</strong>: ' + salesman.latitude +
                '<br/><strong>Longitude</strong>: ' + salesman.longitude +
                '<br/>' +
                '</td>' +
                '</tr></table>' +
                '<br/>' +
                '<br/>' +
                '</div>';

        pgmap.infoWindow.setContent(infoHtml);
        pgmap.infoWindow.setPosition(latlng);
        pgmap.infoWindow.open(pgmap.map);
    }
};

//pgmap.markerClickFunction = function (order, customer, latlng) {
//    //alert(customer.file_name);
//    return function (e) {
//        e.cancelBubble = true;
//        e.returnValue = false;
//        if (e.stopPropagation) {
//            e.stopPropagation();
//            e.preventDefault();
//        }

//        imagesCustomerUrl = '<td><div class="info-image"><img src="' + customer.file_name + '" width="100px" /></div></td>';
//        customer_id = customer.code;
//        customer_name = customer.name;
//        distributor_code = customer.branchcode;
//        var infoHtml = '';

//        // Nếu khách hàng có đơn hàng
//        if (customer.status == 2) {
//            infoHtml = '<div class="info">' +
//                '<div class="info-body">' +
//                '<table width="350px" border="0" cellpadding="0" cellspacing="0">' +
//                '<tr>' +
//                imagesCustomerUrl +
//                '<td>' +
//                '<h3>(' + order + ') ' + customer.name + '</h3>' +
//                '<br/><strong>Mã CH</strong>: ' + customer.code +
//                '<br/><strong>Tên CH</strong>: ' + customer.name +
//                '<br/><strong>Người liên hệ</strong>: ' + customer.name +
//                '<br/><strong>Địa chỉ</strong>: ' + customer.address +
//                '<br/><strong>Mã NVBH</strong>: ' + customer.dsr +
//                '<br/><strong>Tên nhân viên BH</strong>: ' + customer.dsr_name +
//                '<br/>' +
//                '</td>' +
//                '</tr></table>' +
//                '<br/><h3 style="color:red;font-weight:bold;">Thông tin đơn hàng</h3>' +
//                '<br/>&nbsp;&nbsp;<strong>Đơn hàng số</strong>: ' + customer.order_code +
//                '<br/>&nbsp;&nbsp;<strong>Ngày</strong>: ' + customer.created_at +
//                '<br/>&nbsp;&nbsp;<strong>Sản lượng (thùng)</strong>: ' + customer.drop_size +
//                '<br/>&nbsp;&nbsp;<strong>Giá trị đơn hàng (VND)</strong>: ' + customer.total_amt +
//                '<br/>&nbsp;&nbsp;<strong>SKU</strong>: ' + customer.total_sku +
//                '<br/>&nbsp;&nbsp;<strong>Giờ vào</strong>: ' + customer.start_time +
//                '<br/>&nbsp;&nbsp;<strong>Giờ ra</strong>: ' + customer.end_time +
//                '<br/>&nbsp;&nbsp;<strong>Khoảng cách đến cửa hàng(m)</strong>: ' + customer.distance +
//                '<br/>&nbsp;&nbsp;<p style="color:red;cursor:pointer;text-align:center;font-weight:bold;" ><a href="' + base_url + "Home/Slide?outletID=" + customer.code + '&txt_date=' + $('#txt_date').val() + '" target="_blank">Xem hình ảnh hiện diện tại cửa hàng</a></p>' +
//                '<br/>' +
//                '<br/>' +
//                '</div>';
//        }
//        // Nếu khách hàng có không đơn hàng
//        else if (customer.status == 3) {
//            infoHtml = '<div class="info">' +
//                '<div class="info-body">' +
//                '<table width="350px" border="0" cellpadding="0" cellspacing="0">' +
//                '<tr>' +
//                imagesCustomerUrl +
//                '<td>' +
//                '<h3>(' + order + ') ' + customer.name + '</h3>' +
//                '<br/><strong>Mã CH</strong>: ' + customer.code +
//                '<br/><strong>Tên CH</strong>: ' + customer.name +
//                '<br/><strong>Người liên hệ</strong>: ' + customer.name +
//                '<br/><strong>Địa chỉ</strong>: ' + customer.address +
//                '<br/><strong>Mã NVBH</strong>: ' + customer.dsr +
//                '<br/><strong>Tên nhân viên BH</strong>: ' + customer.dsr_name +
//                '<br/>' +
//                '</td>' +
//                '</tr></table>' +
//                '<br/><h3 style="color:red;font-weight:bold;">Thông tin đơn hàng - Không có</h3>' +
//                '<br/>&nbsp;&nbsp;<strong>Lý do</strong>: ' + customer.reason +
//                '<br/>&nbsp;&nbsp;<strong>Giờ vào</strong>: ' + customer.start_time +
//                '<br/>&nbsp;&nbsp;<strong>Giờ ra</strong>: ' + customer.end_time +
//                '<br/>&nbsp;&nbsp;<strong>Khoảng cách đến cửa hàng(m)</strong>: ' + customer.distance +
//                '<br/>&nbsp;&nbsp;<p style="color:red;cursor:pointer;text-align:center;font-weight:bold;" ><a href="' + base_url + "Home/Slide?outletID=" + customer.code + '&txt_date=' + $('#txt_date').val() + '" target="_blank">Xem hình ảnh trưng bày</a></p>' +
//                '<br/>' +
//                '<br/>' +
//                '</div>';
//        }
//        // Nếu khách hàng chưa được viếng thăm
//        else if (customer.status == '') {
//            infoHtml = '<div class="info">' +
//                '<div class="info-body">' +
//                '<table width="350px" border="0" cellpadding="0" cellspacing="0">' +
//                '<tr>' +
//                imagesCustomerUrl +
//                '<td>' +
//                '<h3>(' + order + ') ' + customer.name + '</h3>' +
//                '<br/><strong>Mã CH</strong>: ' + customer.code +
//                '<br/><strong>Tên CH</strong>: ' + customer.name +
//                '<br/><strong>Người liên hệ</strong>: ' + customer.name +
//                '<br/><strong>Địa chỉ</strong>: ' + customer.address +
//                '<br/><strong>Mã NVBH</strong>: ' + customer.dsr +
//                '<br/><strong>Tên nhân viên BH</strong>: ' + customer.dsr_name +
//                '<br/>' +
//                '</td>' +
//                '</tr></table>' +
//                '<br/>' +
//                '<br/>' +
//                '</div>';
//        }
//        // Nếu khách hàng chưa được viếng thăm
//        else {
//            infoHtml = '<div class="info">' +
//                '<div class="info-body">' +
//                '<table width="350px" border="0" cellpadding="0" cellspacing="0">' +
//                '<tr>' +
//                imagesCustomerUrl +
//                '<td>' +
//                '<h3>(' + order + ') ' + customer.name + '</h3>' +
//                '<br/><strong>Mã CH</strong>: ' + customer.code +
//                '<br/><strong>Tên CH</strong>: ' + customer.name +
//                '<br/><strong>Người liên hệ</strong>: ' + customer.name +
//                '<br/><strong>Địa chỉ</strong>: ' + customer.address +
//                '<br/><strong>Mã NVBH</strong>: ' + customer.dsr +
//                '<br/><strong>Tên nhân viên BH</strong>: ' + customer.dsr_name +
//                '<br/>' +
//                '</td>' +
//                '</tr></table>' +
//                '<br/><h3 style="color:red;font-weight:bold;">Thông tin đơn hàng</h3>' +
//                '<br/>&nbsp;&nbsp;<strong>Đơn hàng số</strong>: ' + customer.order_code +
//                '<br/>&nbsp;&nbsp;<strong>Ngày</strong>: ' + customer.created_at +
//                '<br/>&nbsp;&nbsp;<strong>Độ lớn đơn hàng (thùng)</strong>: ' + customer.drop_size +
//                '<br/>&nbsp;&nbsp;<strong>Giá trị đơn hàng (VND)</strong>: ' + customer.total_amt +
//                '<br/>&nbsp;&nbsp;<strong>SKU</strong>: ' + customer.total_sku +
//                '<br/>&nbsp;&nbsp;<strong>Lý do</strong>: ' + customer.reason +
//                '<br/>&nbsp;&nbsp;<strong>Giờ vào</strong>: ' + customer.start_time +
//                '<br/>&nbsp;&nbsp;<strong>Giờ ra</strong>: ' + customer.end_time +
//                '<br/>&nbsp;&nbsp;<strong>Khoảng cách đến cửa hàng(m)</strong>: ' + customer.distance +
//                '<br/>&nbsp;&nbsp;<p style="color:red;cursor:pointer;text-align:center;font-weight:bold;" ><a href="' + base_url + "Home/Slide?outletID=" + customer.code + '&txt_date=' + $('#txt_date').val() + '" target="_blank">Xem hình ảnh trưng bày</a></p>' +
//                '<br/>' +
//                '<br/>' +
//                '</div>';
//        }
//        pgmap.infoWindow.setContent(infoHtml);
//        pgmap.infoWindow.setPosition(latlng);
//        pgmap.infoWindow.open(pgmap.map);
//    };
//};

//SyVN: nhiều lần ghé thăm 1 cửa hàng
pgmap.markerClickFunction = function (order, customer, latlng) {
    //alert(customer.file_name);
    return function (e) {
        e.cancelBubble = true;
        e.returnValue = false;
        if (e.stopPropagation) {
            e.stopPropagation();
            e.preventDefault();
        }

        imagesCustomerUrl = '<td><div class="info-image"><img src="' + customer.file_name + '" width="100px" /></div></td>';
        customer_id = customer.code;
        customer_name = customer.name;
        distributor_code = customer.branchcode;
        var infoHtml = '';

        //HEADER
        infoHtml = '<div class="info">' +
                '<div class="info-body">' +
                '<table width="350px" border="0" cellpadding="0" cellspacing="0">' +
                '<tr>' +
                imagesCustomerUrl +
                '<td>' +
                '<h3>(' + order + ') ' + customer.name + '</h3>' +
                '<br/><strong>Mã CH</strong>: ' + customer.code +
                '<br/><strong>Tên CH</strong>: ' + customer.name +
                '<br/><strong>Người liên hệ</strong>: ' + customer.name +
                '<br/><strong>Địa chỉ</strong>: ' + customer.address +
                '<br/><strong>Mã NVBH</strong>: ' + customer.dsr +
                '<br/><strong>Tên nhân viên BH</strong>: ' + customer.dsr_name +
                '<br/>' +
                '</td>' +
                '</tr></table>' +
                '<br/><h3 style="color:red;font-weight:bold;">Thông tin đơn hàng</h3>' +
                '<br/>&nbsp;&nbsp;<strong>Ngày</strong>: ' + customer.created_at;

        // Nếu khách hàng có đơn hàng
        if (customer.status == 2) {
            //Process List Order of this OutLet
            var length = customer.listOD.length;
            element = null;
            for (var i = 0; i < length; i++) {
                element = customer.listOD[i];
                // Do something with element i.
                if (element.status == 2) {
                    infoHtml += '<br/>&nbsp;&nbsp;<strong>Đơn hàng số</strong>: ' + element.order_code +
                                '<br/>&nbsp;&nbsp;<strong>Sản lượng (thùng)</strong>: ' + element.drop_size +
                                '<br/>&nbsp;&nbsp;<strong>Giá trị đơn hàng (VND)</strong>: ' + element.total_amt +
                                '<br/>&nbsp;&nbsp;<strong>SKU</strong>: ' + element.total_sku +
                                '<br/>&nbsp;&nbsp;<strong>Giờ vào</strong>: ' + element.start_time +
                                '<br/>&nbsp;&nbsp;<strong>Giờ ra</strong>: ' + element.end_time +
                                '<br/>&nbsp;&nbsp;<strong>Khoảng cách đến cửa hàng(m)</strong>: ' + element.distance + '<br/>';
                }
                else if (element.status == 3) {
                    infoHtml += '<br/>&nbsp;&nbsp;<strong>Đơn hàng số</strong>: không có' +
                                '<br/>&nbsp;&nbsp;<strong>Lý do</strong>: ' + element.reason +
                                '<br/>&nbsp;&nbsp;<strong>Giờ vào</strong>: ' + element.start_time +
                                '<br/>&nbsp;&nbsp;<strong>Giờ ra</strong>: ' + element.end_time +
                                '<br/>&nbsp;&nbsp;<strong>Khoảng cách đến cửa hàng(m)</strong>: ' + element.distance + '<br/>';
                }
            }

            infoHtml += '<br/>&nbsp;&nbsp;<p style="color:red;cursor:pointer;text-align:center;font-weight:bold;" ><a href="' + base_url + "Home/Slide?outletID=" + customer.code + '&txt_date=' + $('#txt_date').val() + '" target="_blank">Xem hình ảnh hiện diện tại cửa hàng</a></p>' +
                '<br/>' +
                '<br/>' +
                '</div>';
        }
        // Nếu khách hàng có không đơn hàng
        else if (customer.status == 3) {
            //Process List Order of this OutLet
            var length = customer.listOD.length;
            element = null;
            for (var i = 0; i < length; i++) {
                element = customer.listOD[i];
                // Do something with element i.
                if (element.status == 2) {
                    infoHtml += '<br/>&nbsp;&nbsp;<strong>Đơn hàng số</strong>: ' + element.order_code +
                                '<br/>&nbsp;&nbsp;<strong>Sản lượng (thùng)</strong>: ' + element.drop_size +
                                '<br/>&nbsp;&nbsp;<strong>Giá trị đơn hàng (VND)</strong>: ' + element.total_amt +
                                '<br/>&nbsp;&nbsp;<strong>SKU</strong>: ' + element.total_sku +
                                '<br/>&nbsp;&nbsp;<strong>Giờ vào</strong>: ' + element.start_time +
                                '<br/>&nbsp;&nbsp;<strong>Giờ ra</strong>: ' + element.end_time +
                                '<br/>&nbsp;&nbsp;<strong>Khoảng cách đến cửa hàng(m)</strong>: ' + element.distance + '<br/>';
                }
                else if (element.status == 3) {
                    infoHtml += '<br/>&nbsp;&nbsp;<strong>Đơn hàng số</strong>: không có' +
                                '<br/>&nbsp;&nbsp;<strong>Lý do</strong>: ' + element.reason +
                                '<br/>&nbsp;&nbsp;<strong>Giờ vào</strong>: ' + element.start_time +
                                '<br/>&nbsp;&nbsp;<strong>Giờ ra</strong>: ' + element.end_time +
                                '<br/>&nbsp;&nbsp;<strong>Khoảng cách đến cửa hàng(m)</strong>: ' + element.distance + '<br/>';
                }
            }

            infoHtml += '<br/>&nbsp;&nbsp;<p style="color:red;cursor:pointer;text-align:center;font-weight:bold;" ><a href="' + base_url + "Home/Slide?outletID=" + customer.code + '&txt_date=' + $('#txt_date').val() + '" target="_blank">Xem hình ảnh hiện diện tại cửa hàng</a></p>' +
                '<br/>' +
                '<br/>' +
                '</div>';
        }
        // Nếu khách hàng chưa được viếng thăm
        else if (customer.status == '') {
            infoHtml = '<div class="info">' +
                '<div class="info-body">' +
                '<table width="350px" border="0" cellpadding="0" cellspacing="0">' +
                '<tr>' +
                imagesCustomerUrl +
                '<td>' +
                '<h3>(' + order + ') ' + customer.name + '</h3>' +
                '<br/><strong>Mã CH</strong>: ' + customer.code +
                '<br/><strong>Tên CH</strong>: ' + customer.name +
                '<br/><strong>Người liên hệ</strong>: ' + customer.name +
                '<br/><strong>Địa chỉ</strong>: ' + customer.address +
                '<br/><strong>Mã NVBH</strong>: ' + customer.dsr +
                '<br/><strong>Tên nhân viên BH</strong>: ' + customer.dsr_name +
                '<br/>' +
                '</td>' +
                '</tr></table>' +
                '<br/>' +
                '<br/>' +
                '</div>';
        }
        // Nếu khách hàng chưa được viếng thăm
        else {
            //Process List Order of this OutLet
            var length = customer.listOD.length;
            element = null;
            for (var i = 0; i < length; i++) {
                element = customer.listOD[i];
                // Do something with element i.
                if (element.status == 2) {
                    infoHtml += '<br/>&nbsp;&nbsp;<strong>Đơn hàng số</strong>: ' + element.order_code +
                                '<br/>&nbsp;&nbsp;<strong>Sản lượng (thùng)</strong>: ' + element.drop_size +
                                '<br/>&nbsp;&nbsp;<strong>Giá trị đơn hàng (VND)</strong>: ' + element.total_amt +
                                '<br/>&nbsp;&nbsp;<strong>SKU</strong>: ' + element.total_sku +
                                '<br/>&nbsp;&nbsp;<strong>Giờ vào</strong>: ' + element.start_time +
                                '<br/>&nbsp;&nbsp;<strong>Giờ ra</strong>: ' + element.end_time +
                                '<br/>&nbsp;&nbsp;<strong>Khoảng cách đến cửa hàng(m)</strong>: ' + element.distance + '<br/>';
                }
                else if (element.status == 3) {
                    infoHtml += '<br/>&nbsp;&nbsp;<strong>Đơn hàng số</strong>: không có' +
                                '<br/>&nbsp;&nbsp;<strong>Lý do</strong>: ' + element.reason +
                                '<br/>&nbsp;&nbsp;<strong>Giờ vào</strong>: ' + element.start_time +
                                '<br/>&nbsp;&nbsp;<strong>Giờ ra</strong>: ' + element.end_time +
                                '<br/>&nbsp;&nbsp;<strong>Khoảng cách đến cửa hàng(m)</strong>: ' + element.distance + '<br/>';
                }
            }

            infoHtml += '<br/>&nbsp;&nbsp;<p style="color:red;cursor:pointer;text-align:center;font-weight:bold;" ><a href="' + base_url + "Home/Slide?outletID=" + customer.code + '&txt_date=' + $('#txt_date').val() + '" target="_blank">Xem hình ảnh hiện diện tại cửa hàng</a></p>' +
                '<br/>' +
                '<br/>' +
                '</div>';
        }
        pgmap.infoWindow.setContent(infoHtml);
        pgmap.infoWindow.setPosition(latlng);
        pgmap.infoWindow.open(pgmap.map);
    };
};

pgmap.clear = function () {
    // $('timetaken').innerHTML = 'cleaning...';
    var showHide = pgmap.showCircle;
    pgmap.showCircle = false;
    pgmap.circleToggle();
    pgmap.showCircle = showHide

    pgmap.blueMarkers = 0;
    pgmap.bounds = new google.maps.LatLngBounds();
    pgmap.markers = [];


    if (pgmap.useMarkerCluster && pgmap.markerClusterer != null)
        pgmap.markerClusterer.clearMarkers();
    else {
        for (var i = 0, marker; marker = pgmap.markers[i]; i++) {
            marker.setMap(null);
        }
    }
    //clear polyline
    for (var i = 0, polyline; i < pgmap.polylines.length; i++) {
        polyline = pgmap.polylines[i];
        polyline.setMap(null);
    }
};

pgmap.time = function () {
    //$('timetaken').innerHTML = 'timing...';
    //var start = new Date();
    if (pgmap.useMarkerCluster) {
        var clusterOptions = { maxZoom: 14, gridSize: 40 };
        pgmap.markerClusterer = new MarkerClusterer(pgmap.map, pgmap.markers, clusterOptions);
    } else {
        for (var i = 0, marker; marker = pgmap.markers[i]; i++) {
            marker.setMap(pgmap.map);
        }
    }
    //var end = new Date();
    //$('timetaken').innerHTML = end - start;
};

pgmap.createCircle = function () {
    var center = pgmap.bounds.getCenter();
    var ne = pgmap.bounds.getNorthEast();
    // r = radius of the earth in statute miles
    var r = 3963.0;

    // Convert lat or lng from decimal degrees into radians (divide by 57.2958)
    var lat1 = center.lat() / 57.2958;
    var lon1 = center.lng() / 57.2958;
    var lat2 = ne.lat() / 57.2958;
    var lon2 = ne.lng() / 57.2958;

    // distance = circle radius from center to Northeast corner of bounds
    var dist = r * Math.acos(Math.sin(lat1) * Math.sin(lat2) + Math.cos(lat1) * Math.cos(lat2) * Math.cos(lon2 - lon1)) * 1800;

    if (pgmap.circle != null && pgmap.circle.getMap() != null)
        pgmap.circle.setMap(null);
    if (pgmap.markers.length > 0) {
        pgmap.circle = new google.maps.Circle({
            center: center,
            radius: dist,
            strokeColor: "#0000FF",
            strokeOpacity: 0.8,
            strokeWeight: 1,
            fillColor: "#00FFFF",
            fillOpacity: 0.1
        });

        pgmap.circle.setMap(pgmap.map);
        //pgmap.map.fitBounds(pgmap.bounds);
        var imageUrl = base_url + 'assets/img/statistic.png';
        var markerImage = new google.maps.MarkerImage(imageUrl, new google.maps.Size(32, 37));

        pgmap.statisticMarker = new MarkerWithLabel({
            //map: pgmap.map,
            position: center,
            draggable: true,
            //raiseOnDrag: true,
            icon: markerImage,
            labelContent: "<span class='bluemarker'>" + pgmap.blueMarkers + "</span> / <span class='redmarker'>" + (pgmap.data.length - pgmap.blueMarkers) + "</span><br/>(<span class='bluemarker'>" + (pgmap.blueMarkers * 100 / pgmap.markers.length).toPrecision(3) + "%</span> / <span class='redmarker'>" + ((pgmap.data.length - pgmap.blueMarkers) * 100 / pgmap.data.length).toPrecision(3) + "%</span>)",
            labelAnchor: new google.maps.Point(70, 0),
            labelClass: "statistic", // the CSS class for the label
            labelInBackground: true
        });
    }
}

pgmap.circleToggle = function () {
    if (pgmap.showCircle) {
        pgmap.circle.setMap(pgmap.map);
        pgmap.statisticMarker.setMap(pgmap.map);
    }
    else
        if (pgmap.circle != null && pgmap.circle.getMap() != null) {
            pgmap.circle.setMap(null);
            pgmap.statisticMarker.setMap(null);
        }
}

//LuuDN
function numberWithCommas(x) {
    if ((typeof x) != 'undefined')
        return x.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
    else
        return '';
}
function ymdToDmy(string) {
    if ((typeof string) != 'undefined')
        return (string.substring(8) + '-' + string.substring(5, 7) + '-' + string.substring(0, 4));
    else
        return '';
}
function hourFullToSort(hour) {
    if ((typeof hour) != 'undefined')
        return (hour.substring(0, 5));
    else
        return '';
}
function myFormatNumber(x) {
    if ((typeof x) != 'undefined') {
        return Number(x).toFixed(2);
    }
    else
        return '';
}
function AddFirstZero(x) {
    if (Number(x) < 10 && Number(x) > 0) {
        return ('0' + x.toString());
    }
    else return x;
}