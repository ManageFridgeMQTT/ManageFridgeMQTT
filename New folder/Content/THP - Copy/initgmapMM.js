var pgmap = {};
pgmap.system = "Google Map";
pgmap.useMarkerCluster = true;
pgmap.iterationRender = 9;
pgmap.center = new google.maps.LatLng(16, 106);
pgmap.zoom = 6;
pgmap.disableUIControl = false;

pgmap.periodmarker = 'mtd';
pgmap.map = null;
pgmap.markerClusterer = null;

//Saleman render, Outlet render data
pgmap.data = null;

//Saleman Location render data
pgmap.salemanData = null;

//SaleSup Location render data
pgmap.SSData = null;

//ASM Location render data
pgmap.asmData = null;

//Marker and Polyline
pgmap.markers = [];
pgmap.polylines = [];

pgmap.salemanMarkers = [];
pgmap.salemanPolylines = [];

pgmap.saleSupMarkers = [];
pgmap.saleSupPolylines = [];

pgmap.asmMarkers = [];
pgmap.asmPolylines = [];

pgmap.infoWindow = null;
pgmap.bounds = null;
pgmap.circle = null;
pgmap.statisticMarker = null;
pgmap.colorBy = "Normal"; //Normal, DRS, Segment, Route
pgmap.showCircle = false;
pgmap.showLabels = false;
pgmap.blueMarkers = 0;
pgmap.selection = 0;
pgmap.showSelectedOnly = 0;

//Last Location
pgmap.salesmanLocation = pgmap.center; //luuDN
pgmap.salesupLocation = pgmap.center; //SyVN
pgmap.asmLocation = pgmap.center; //SyVN
pgmap.lastOutlet = pgmap.center; // luuDN: last Outlet location
pgmap.renderLine = 0;

pgmap.ddlLabelInfo = 0;

pgmap.SalesmenStepRendered = 0;
pgmap.SalesmenStepSecond = 0;
pgmap.SalesmenStepStop = 0;

var imagesCustomerUrl = '';
var ODLatitude = "0";
var ODLongtitude = "0";
// sua ngay 10/04
var source = null;
var destination = null;

var customer_id = '';
var distributor_code = '';
var customer_name = '';
var saleRun = 0;

//INIT GMAP
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
////INIT GMAP

//ClearMap
pgmap.clear = function () {
    pgmap.blueMarkers = 0;
    pgmap.bounds = new google.maps.LatLngBounds();
    pgmap.markers = [];
    pgmap.salemanMarkers = [];
    pgmap.saleSupMarkers = [];
    pgmap.asmMarkers = [];

    if (pgmap.useMarkerCluster && pgmap.markerClusterer != null)
        pgmap.markerClusterer.clearMarkers();
    for (var i = 0, marker; i < pgmap.markers.length; i++) {
        marker = pgmap.markers[i];
        marker.setMap(null);
    }
    for (var i = 0, marker; i < pgmap.salemanMarkers.length; i++) {
        marker = pgmap.salemanMarkers[i];
        marker.setMap(null);
    }
    for (var i = 0, marker; i < pgmap.saleSupMarkers.length; i++) {
        marker = pgmap.saleSupMarkers[i];
        marker.setMap(null);
    }
    for (var i = 0, marker; i < pgmap.asmMarkers.length; i++) {
        marker = pgmap.asmMarkers[i];
        marker.setMap(null);
    }
    //clear polyline
    for (var i = 0, polyline; i < pgmap.polylines.length; i++) {
        polyline = pgmap.polylines[i];
        polyline.setMap(null);
    }
};
////ClearMap

//markerClusterer
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
        for (var i = 0, marker; marker = pgmap.salemanMarkers[i]; i++) {
            marker.setMap(pgmap.map);
        }
        for (var i = 0, marker; marker = pgmap.saleSupMarkers
[i]; i++) {
            marker.setMap(pgmap.map);
        }
        for (var i = 0, marker; marker = pgmap.asmMarkers[i]; i++) {
            marker.setMap(pgmap.map);
        }
    }
    //var end = new Date();
    //$('timetaken').innerHTML = end - start;
};
////markerClusterer

//RENDER SALEMAN
pgmap.showSalesmenMarkers = function () {
    //CheckSession();
    pgmap.clear();
    pgmap.init();
    pgmap.infoWindow.close();

    $("div.loading[obj=map]").show();
    window.setTimeout("pgmap.renderSalesmenMarkers(0)", 0);
};

pgmap.renderSaleman = function () {
    //RenderSaleman
    var latLng = new google.maps.LatLng(pgmap.salemanData.Latitude, pgmap.salemanData.Longtitude);

    var marker = new MarkerWithLabel({
        position: latLng,
        draggable: false,
        raiseOnDrag: false,
        labelContent: pgmap.salemanData.SalesmanName,
        labelAnchor: new google.maps.Point(pgmap.salemanData.SalesmanName.length * 4, 46),
        labelClass: "labels"// the CSS class for the label

    });

    var fn1 = pgmap.markerSalesmenMouseOverFunction(pgmap.salemanData, latLng);
    var fn2 = pgmap.markerSalesmenDblClickFunction(pgmap.salemanData, latLng);

    google.maps.event.addListener(marker, 'mouseover', fn1);
    google.maps.event.addListener(marker, 'mouseout', function () {
        pgmap.infoWindow.close();
    });
    google.maps.event.addListener(marker, 'click', fn2);
    pgmap.markers.push(marker);
    currlatLng = latLng;
    pgmap.salesmanLocation = latLng;
};

function MapPanToOutletSMSSASM(lat, long, SMlat, SMlong, SSlat, SSlong, ASMlat, ASMlong) {

    if ($('#itemSelectedOnly').is(':checked')) {
        pgmap.showSelectedOnly = 1;
    }
    else {
        pgmap.showSelectedOnly = 0;
    }

    if (pgmap.showSelectedOnly == 1) {
        for (var i = 0, polyline; i < pgmap.polylines.length; i++) {
            polyline = pgmap.polylines[i];
            polyline.setMap(null);
        }

        for (var i = 0, marker; marker = pgmap.markers[i]; i++) {
            marker.setMap(null);
        }
        for (var i = 0, marker; marker = pgmap.salemanMarkers[i]; i++) {
            marker.setMap(null);
        }
        for (var i = 0, marker; marker = pgmap.saleSupMarkers
[i]; i++) {
            marker.setMap(null);
        }
        for (var i = 0, marker; marker = pgmap.asmMarkers[i]; i++) {
            marker.setMap(null);
        }
    }

    if (lat != 0 && long != 0) {
        var latLng = new google.maps.LatLng(lat, long);
        pgmap.map.panTo(latLng);
        toggleBounceOutlet(latLng);

        var SMlatLng = new google.maps.LatLng(SMlat, SMlong);
        toggleBounceSM(SMlatLng);

        var SSlatLng = new google.maps.LatLng(SSlat, SSlong);
        toggleBounceSS(SSlatLng);

        var ASMlatLng = new google.maps.LatLng(ASMlat, ASMlong);
        toggleBounceASM(ASMlatLng);
    }
};

function MapPanToOutlet(lat, long) {
    if (lat != 0 && long != 0) {
        var latLng = new google.maps.LatLng(lat, long);
        pgmap.map.panTo(latLng);
        toggleBounceOutlet(latLng);
    }
};

function toggleBounceOutlet(latLng) {
    //alert('Animation');
    var index = pgmap.markers.length;
    for (var i = 0; i < index; i++) {
        //alert(pgmap.markers.length + ' - ' + pgmap.markers[i].getPosition().lat() + ' - ' + latLng.lat());
        //pgmap.markers[i].setAnimation(null);
        if (pgmap.markers[i].getAnimation() != null) {
            pgmap.markers[i].setAnimation(null);
        }
        //pgmap.markers[i].setMap(null);
        if (pgmap.markers[i].getPosition().lat() == latLng.lat()) {
            pgmap.markers[i].setMap(pgmap.map);
            pgmap.markers[i].setAnimation(google.maps.Animation.BOUNCE);
        }
    }
}

function MapPanToSM(lat, long) {
    if (lat != 0 && long != 0) {
        var latLng = new google.maps.LatLng(lat, long);
        pgmap.map.panTo(latLng);
        toggleBounceSM(latLng);
    }
};

function toggleBounceSM(latLng) {
    //alert('Animation');
    var index = pgmap.salemanMarkers.length;
    for (var i = 0; i < index; i++) {
        //alert(pgmap.markers.length + ' - ' + pgmap.markers[i].getPosition().lat() + ' - ' + latLng.lat());
        //pgmap.markers[i].setAnimation(null);
        if (pgmap.salemanMarkers[i].getAnimation() != null) {
            pgmap.salemanMarkers[i].setAnimation(null);
        }
        //pgmap.salemanMarkers[i].setMap(null);
        if (pgmap.salemanMarkers[i].getPosition().lat() == latLng.lat()) {
            pgmap.salemanMarkers[i].setMap(pgmap.map);
            pgmap.salemanMarkers[i].setAnimation(google.maps.Animation.BOUNCE);
        }
    }
}

function MapPanToSS(lat, long) {
    if (lat != 0 && long != 0) {
        var latLng = new google.maps.LatLng(lat, long);
        pgmap.map.panTo(latLng);
        toggleBounceSS(latLng);
    }
};

function toggleBounceSS(latLng) {
    //alert('Animation');
    var index = pgmap.saleSupMarkers.length;
    for (var i = 0; i < index; i++) {
        //alert(pgmap.markers.length + ' - ' + pgmap.markers[i].getPosition().lat() + ' - ' + latLng.lat());
        //pgmap.markers[i].setAnimation(null);
        if (pgmap.saleSupMarkers[i].getAnimation() != null) {
            pgmap.saleSupMarkers[i].setAnimation(null);
        }
        //pgmap.saleSupMarkers[i].setMap(null);
        if (pgmap.saleSupMarkers[i].getPosition().lat() == latLng.lat()) {
            pgmap.saleSupMarkers[i].setMap(pgmap.map);
            pgmap.saleSupMarkers[i].setAnimation(google.maps.Animation.BOUNCE);
        }
    }
}

function MapPanToASM(lat, long) {
    if (lat != 0 && long != 0) {
        var latLng = new google.maps.LatLng(lat, long);
        pgmap.map.panTo(latLng);
        toggleBounceASM(latLng);
    }
};

function toggleBounceASM(latLng) {
    //alert('Animation');
    var index = pgmap.asmMarkers.length;
    for (var i = 0; i < index; i++) {
        //alert(pgmap.markers.length + ' - ' + pgmap.markers[i].getPosition().lat() + ' - ' + latLng.lat());
        //pgmap.markers[i].setAnimation(null);
        if (pgmap.asmMarkers[i].getAnimation() != null) {
            pgmap.asmMarkers[i].setAnimation(null);
        }
        //pgmap.asmMarkers[i].setMap(null);
        if (pgmap.asmMarkers[i].getPosition().lat() == latLng.lat()) {
            pgmap.asmMarkers[i].setMap(pgmap.map);
            pgmap.asmMarkers[i].setAnimation(google.maps.Animation.BOUNCE);
        }
    }
}

//function MapShowLine(lat, long) {
//    var latLng = new google.maps.LatLng(lat, long);
//    pgmap.map.panTo(latLng);
//};

pgmap.renderSalesmenMarkers = function (fromIndex) {
    LogTime(fromIndex);
    var toIndex = fromIndex + pgmap.iterationRender;
    if (toIndex > pgmap.data.length)
        toIndex = pgmap.data.length;
    //alert(pgmap.data.length);

    for (var i = fromIndex; i < toIndex; i++) {
        var latLng = new google.maps.LatLng(pgmap.data[i].Latitude, pgmap.data[i].Longtitude);

        var marker = new MarkerWithLabel({
            position: latLng,
            draggable: false,
            raiseOnDrag: false,
            labelContent: pgmap.data[i].SalesmanName,
            labelAnchor: new google.maps.Point(pgmap.data[i].SalesmanName.length * 4, 46),
            labelClass: "labels"// the CSS class for the label

        });

        var fn1 = pgmap.markerSalesmenMouseOverFunction(pgmap.data[i], latLng);
        var fn2 = pgmap.markerSalesmenDblClickFunction(pgmap.data[i], latLng);

        google.maps.event.addListener(marker, 'mouseover', fn1);
        google.maps.event.addListener(marker, 'mouseout', function () {
            pgmap.infoWindow.close();
        });
        google.maps.event.addListener(marker, 'click', fn2);
        pgmap.salemanMarkers.push(marker);
        pgmap.bounds.extend(latLng);

        currlatLng = latLng;
    }
    pgmap.map.panTo(currlatLng);

    if (toIndex >= pgmap.data.length) {
        $("div.loading[obj=map]").hide();
        window.setTimeout(pgmap.time, 0);
    }
    else {
        var value = (toIndex * 100) / pgmap.data.length;
        $("div.loading[obj=map] div.progressbar").progressbar({ value: value });
        $("div.loading[obj=map] div.status").text(toIndex + " / " + pgmap.data.length);
        window.setTimeout("pgmap.renderSalesmenMarkers(" + toIndex + ")", 0);
    }

    LogTime('End');
}

pgmap.markerSalesmenMouseOverFunction = function (salesman, latlng) {
    return function (e) {
        var infoHtml = '<div class="info">' +
                '<div class="info-body">' +
                '<table width="350px" border="0" cellpadding="0" cellspacing="0">' +
                '<tr>' +
                '<td>' +
                '<h3>(' + salesman.SalesmanID + ') ' + salesman.SalesmanName + ' - (' + salesman.Phone + ')</h3>' +
                '<br/><strong>Thời gian</strong>: ' + salesman.VisitTime +
                '<br/><strong>Latitude</strong>: ' + salesman.Latitude +
                '<br/><strong>Longitude</strong>: ' + salesman.Longtitude +
                '<h4>SalesSup: (' + salesman.SaleSupID + ') - ' + salesman.SaleSupName + ' - (' + salesman.SaleSupIDPhone + ')</h4>' +
                '<h4>ASM: (' + salesman.ASMID + ') - ' + salesman.ASMName + ' - (' + salesman.ASMPhone + ')</h4>' +
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

//luudn - show tooltip when click on marker when show all outlets marker of salesman and salesman
pgmap.markerSalesmenClickFunction = function (salesman, latlng) {
    //CheckSession();
    return function (e) {
        var infoHtml = '<div class="info">' +
                '<div class="info-body">' +
                '<table width="350px" border="0" cellpadding="0" cellspacing="0">' +
                '<tr>' +
                '<td>' +
                '<h3>(' + salesman.SalesmanID + ') ' + salesman.SalesmanName + ' - (' + salesman.Phone + ')</h3>' +
                '<br/><strong>Thời gian</strong>: ' + salesman.VisitTime +
                '<br/><strong>Latitude</strong>: ' + salesman.Latitude +
                '<br/><strong>Longitude</strong>: ' + salesman.Longtitude +
                '<h4>SalesSup: (' + salesman.SaleSupID + ') - ' + salesman.SaleSupName + ' - (' + salesman.SaleSupIDPhone + ')</h4>' +
                '<h4>ASM: (' + salesman.ASMID + ') - ' + salesman.ASMName + ' - (' + salesman.ASMPhone + ')</h4>' +
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
        filterSaleman(salesman.code);
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
////RENDER SALEMAN

//RENDER SALEMAN Movement
pgmap.showSalesmenMovement = function () {
    //CheckSession();
    //pgmap.clear();
    //pgmap.init();
    //pgmap.infoWindow.close();

    $("div.loading[obj=map]").show();
    window.setTimeout("pgmap.renderSalesmenMovement(0)", 0);
};

pgmap.renderSalesmenMovement = function (fromIndex) {
    var toIndex = pgmap.salemanData.length;
    var total = pgmap.salemanData.length;
    if (total > 0) {
        for (var i = fromIndex; i < toIndex; i++) {
            var latLng = new google.maps.LatLng(pgmap.salemanData[i].Latitude, pgmap.salemanData[i].Longtitude);

            var labelContent = '';
            if (pgmap.ddlLabelInfo == 1) {
                labelContent = pgmap.salemanData[i].strVisitTime;
            }
            else {
                labelContent = i + 1;
            }

            var marker = new MarkerWithLabel({
                position: latLng,
                draggable: false,
                map: pgmap.map,
                icon: base_url + 'Content/THP/images/route.png',
                //labelContent: '<p class="mcp-order" style="color:' + mcpcolor + ';font-size: 13px;" >' + ((pgmap.data[i].SalesmanID).toString()).substring(0, 2) + '</p>' + '<br>' + '<p class="real-order" style="display:inline;color:blue;background-color:#7cc1f0;" >' + AddFirstZero(pgmap.data[i].ODVisitOrder) + '</p>',
                labelContent: labelContent,//pgmap.salemanData[i].strVisitTime,
                labelAnchor: new google.maps.Point(8, 27),
                labelClass: "labelmaps", // the CSS class for the label
                labelStyle: { opacity: 0.75 }
            });

            if (i == 0) {
                //if (source == null) {
                source = latLng;
                destination = latLng;
            }
            else {
                source = destination;
                destination = latLng;
                var route = [source, destination];
                var polyline = new google.maps.Polyline({
                    path: route,
                    strokeColor: '#64FE2E',
                    strokeOpacity: 1.0,
                    strokeWeight: 2
                });
                pgmap.polylines.push(polyline);
                polyline.setMap(pgmap.map);
            }

            //Add new marker
            pgmap.salemanMarkers.push(marker);

            pgmap.bounds.extend(latLng);
        }

        $("div.loading[obj=map]").hide();
        window.setTimeout(pgmap.time, 0);

        pgmap.map.setZoom(16);
        pgmap.map.panTo(destination);
    }
}
////RENDER SALEMAN Movement

//RENDER SS Movement
pgmap.showSSMovement = function () {
    $("div.loading[obj=map]").show();
    window.setTimeout("pgmap.renderSSMovement(0)", 0);
};

pgmap.renderSSMovement = function (fromIndex) {
    var toIndex = pgmap.SSData.length;
    var total = pgmap.SSData.length;
    if (total > 0) {
        for (var i = fromIndex; i < toIndex; i++) {
            var latLng = new google.maps.LatLng(pgmap.SSData[i].Latitude, pgmap.SSData[i].Longtitude);

            var labelContent = '';
            if (pgmap.ddlLabelInfo == 1) {
                labelContent = pgmap.SSData[i].strVisitTime;
            }
            else {
                labelContent = i + 1;
            }

            var marker = new MarkerWithLabel({
                position: latLng,
                draggable: false,
                map: pgmap.map,
                icon: base_url + 'Content/THP/images/SalesmanID.png',
                //labelContent: '<p class="mcp-order" style="color:' + mcpcolor + ';font-size: 13px;" >' + ((pgmap.data[i].SalesmanID).toString()).substring(0, 2) + '</p>' + '<br>' + '<p class="real-order" style="display:inline;color:blue;background-color:#7cc1f0;" >' + AddFirstZero(pgmap.data[i].ODVisitOrder) + '</p>',
                labelContent: labelContent,//pgmap.SSData[i].strVisitTime,
                labelAnchor: new google.maps.Point(8, 27),
                labelClass: "labelmaps", // the CSS class for the label
                labelStyle: { opacity: 0.75 }
            });

            if (i == 0) {
                //if (source == null) {
                source = latLng;
                destination = latLng;
            }
            else {
                source = destination;
                destination = latLng;
                var route = [source, destination];
                var polyline = new google.maps.Polyline({
                    path: route,
                    strokeColor: '#0000FF',
                    strokeOpacity: 1.0,
                    strokeWeight: 2
                });
                pgmap.polylines.push(polyline);
                polyline.setMap(pgmap.map);
            }

            //Add new marker
            pgmap.saleSupMarkers.push(marker);

            pgmap.bounds.extend(latLng);
        }

        $("div.loading[obj=map]").hide();
        window.setTimeout(pgmap.time, 0);

        pgmap.map.setZoom(16);
        pgmap.map.panTo(destination);
        pgmap.salesupLocation = destination;
    }
}
////Render SSMovement

//RENDER ASM Movement
pgmap.showASMMovement = function () {
    $("div.loading[obj=map]").show();
    window.setTimeout("pgmap.renderASMMovement(0)", 0);
};

pgmap.renderASMMovement = function (fromIndex) {
    var toIndex = pgmap.ASMData.length;
    var total = pgmap.ASMData.length;
    if (total > 0) {
        for (var i = fromIndex; i < toIndex; i++) {
            var latLng = new google.maps.LatLng(pgmap.ASMData[i].Latitude, pgmap.ASMData[i].Longtitude);

            var labelContent = '';
            if (pgmap.ddlLabelInfo == 1) {
                labelContent = pgmap.ASMData[i].strVisitTime;
            }
            else {
                labelContent = i + 1;
            }

            var marker = new MarkerWithLabel({
                position: latLng,
                draggable: false,
                map: pgmap.map,
                icon: base_url + 'Content/THP/images/segment.png',
                //labelContent: '<p claASM="mcp-order" style="color:' + mcpcolor + ';font-size: 13px;" >' + ((pgmap.data[i].SalesmanID).toString()).substring(0, 2) + '</p>' + '<br>' + '<p claASM="real-order" style="display:inline;color:blue;background-color:#7cc1f0;" >' + AddFirstZero(pgmap.data[i].ODVisitOrder) + '</p>',
                labelContent: labelContent,//pgmap.ASMData[i].strVisitTime,
                labelAnchor: new google.maps.Point(8, 27),
                labelClaASM: "labelmaps", // the CASM claASM for the label
                labelStyle: { opacity: 0.75 }
            });

            if (i == 0) {
                //if (source == null) {
                source = latLng;
                destination = latLng;
            }
            else {
                source = destination;
                destination = latLng;
                var route = [source, destination];
                var polyline = new google.maps.Polyline({
                    path: route,
                    strokeColor: '#FF9900',
                    strokeOpacity: 1.0,
                    strokeWeight: 2
                });
                pgmap.polylines.push(polyline);
                polyline.setMap(pgmap.map);
            }

            //Add new marker
            pgmap.asmMarkers.push(marker);

            pgmap.bounds.extend(latLng);
        }

        $("div.loading[obj=map]").hide();
        window.setTimeout(pgmap.time, 0);

        pgmap.map.setZoom(16);
        pgmap.map.panTo(destination);
        pgmap.asmLocation = destination;
    }
}
////Render ASMMovement

// renderMerMarker
//RENDER Outlet MARKER
pgmap.showMarkers = function () {
    //CheckSession();
    //pgmap.clear();
    pgmap.infoWindow.close();

    $("div.loading[obj=map]").show();
    window.setTimeout("pgmap.renderMarkers(0)", 0);
    //window.setTimeout("pgmap.renderSalesmenVisit(0)", 0);
    source = null;
    destination = null;
    saleRun = 0;
};

var outletPainted = new Array();
pgmap.renderMarkers = function (fromIndex) {
    var toIndex = pgmap.data.length;
    //alert(pgmap.data.length);
    var totalCustomer = pgmap.data.length;
    if (totalCustomer > 0) {
        for (var i = fromIndex; i < toIndex; i++) {
            var latLng = new google.maps.LatLng(pgmap.data[i].Latitude, pgmap.data[i].Longtitude);
            var outletcolor = "00FF00"; //default là màu xanh
            var mcpcolor = "#000000";

            //render status
            switch (pgmap.data[i].Status) {
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
            //alert(base_url + 'Content/THP/images/marker' + outletcolor + '.png');
            //Vi tri cuoi cung cua salesman trung voi vi tri outlet
            //Dau *, vi tri hien tai

            var labelContent = '';
            if (pgmap.ddlLabelInfo == 1) {
                labelContent = pgmap.data[i].StartTime;
            }
            else {
                labelContent = pgmap.data[i].ODVisitOrder;
            }

            if (pgmap.data[i].Status == '1' && pgmap.data[i].ODOutletID == pgmap.data[i].OutletID) {
                // gán vị trí OutLet cuối cùng
                pgmap.lastOutlet = latLng;
                var marker = new MarkerWithLabel({
                    position: latLng,
                    draggable: false,
                    map: pgmap.map,
                    icon: base_url + 'Content/THP/images/markerFFFF00.png', //ODVisitOrder, StartTime
                    labelContent: '<p class="mcp-order" style="color:' + mcpcolor + ';font-size: 13px;" >' + ((pgmap.data[i].SalesmanID).toString()).substring(0, 2) + '</p>' + '<br>' + '<p class="real-order" style="display:inline;color:yellow;background-color:#7cc1f0;" >' + labelContent + '</p>',
                    labelAnchor: new google.maps.Point(8, 32),
                    labelClass: "labelmaps", // the CSS class for the label
                    labelStyle: { opacity: 0.75 }
                });
            }
            else {
                //IF begin: ve them co mau xanh
                //+ AddFirstZero(pgmap.data[i].ODVisitOrder) + 
                if (pgmap.data[i].ODVisitOrder === 1) {
                    var marker = new MarkerWithLabel({
                        position: latLng,
                        draggable: false,
                        map: pgmap.map,
                        icon: base_url + 'Content/THP/images/marker' + outletcolor + '.png',
                        labelContent: '<p class="mcp-order" style="color:' + mcpcolor + ';font-size: 13px;" >' + ((pgmap.data[i].SalesmanID).toString()).substring(0, 2) + '</p>' + '<br>' + '<p class="real-order" style="display:inline;color:red;background-color:yellow;" >' + labelContent + '</p>',
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
                        icon: base_url + 'Content/THP/images/marker' + outletcolor + '.png',
                        labelContent: '<p class="mcp-order" style="color:' + mcpcolor + ';font-size: 13px;" >' + ((pgmap.data[i].SalesmanID).toString()).substring(0, 2) + '</p>' + '<br>' + '<p class="real-order" style="display:inline;color:yellow;background-color:red;" >' + labelContent + '</p>',
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
                        icon: base_url + 'Content/THP/images/marker' + outletcolor + '.png',
                        labelContent: '<p class="mcp-order" style="color:' + mcpcolor + ';font-size: 13px;" >' + ((pgmap.data[i].SalesmanID).toString()).substring(0, 2) + '</p>' + '<br>' + '<p class="real-order" style="display:inline;color:blue;background-color:#7cc1f0;" >' + labelContent + '</p>',
                        labelAnchor: new google.maps.Point(8, 32),
                        labelClass: "labelmaps", // the CSS class for the label
                        labelStyle: { opacity: 0.75 }
                    });
                }
            }


            ODLatitude = pgmap.data[i].Latitude;
            ODLongtitude = pgmap.data[i].Longtitude;

            if (pgmap.renderLine != 0) {
                if (pgmap.data[i].ODVisitOrder != 0) {
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
            }

            var fn = pgmap.markerClickFunction(pgmap.data[i].SalesmanID, pgmap.data[i], latLng);
            google.maps.event.addListener(marker, 'click', fn);

            //Add new marker
            pgmap.markers.push(marker);

            pgmap.bounds.extend(latLng);
        }

        $("div.loading[obj=map]").hide();
        window.setTimeout(pgmap.time, 0);

        var currlatLng = new google.maps.LatLng(ODLatitude, ODLongtitude);
        pgmap.map.setZoom(16);
        pgmap.map.panTo(currlatLng);
    }
}

//SyVN: nhiều lần ghé thăm 1 cửa hàng
pgmap.markerClickFunction = function (order, customer, latlng) {
    //alert(customer.ImageFile);
    return function (e) {
        e.cancelBubble = true;
        e.returnValue = false;
        if (e.stopPropagation) {
            e.stopPropagation();
            e.preventDefault();
        }

        imagesCustomerUrl = '<td><div class="info-image"><img src="' + customer.ImageFile + '" width="100px" /></div></td>';
        customer_id = customer.OutletID;
        customer_name = customer.OutletName;
        distributor_code = customer.OutletID;
        var infoHtml = '';

        //HEADER
        infoHtml = '<div class="info">' +
                '<div class="info-body">' +
                '<table width="350px" border="0" cellpadding="0" cellspacing="0">' +
                '<tr>' +
                imagesCustomerUrl +
                '<td>' +
                '<h3>(' + order + ') ' + customer.OutletName + '</h3>' +
                '<br/><strong>Mã CH</strong>: ' + customer.OutletID +
                '<br/><strong>Tên CH</strong>: ' + customer.OutletName +
                '<br/><strong>Người liên hệ</strong>: ' + customer.OutletName +
                '<br/><strong>Địa chỉ</strong>: ' + customer.Address +
                '<br/><strong>Mã NVBH</strong>: ' + customer.SalesmanID +
                '<br/><strong>Tên nhân viên BH</strong>: ' + customer.SalesmanName +
                '<br/><strong>Latitude</strong>: ' + customer.Latitude +
                '<br/><strong>Longitude</strong>: ' + customer.Longtitude +
                '<br/>' +
                '</td>' +
                '</tr></table>' +
                '<br/><h3 style="color:red;font-weight:bold;">Thông tin đơn hàng</h3>' +
                '<br/>&nbsp;&nbsp;<strong>Ngày</strong>: ' + customer.VisitDate;

        // Nếu khách hàng có đơn hàng
        if (customer.status == 2) {
            //Process List Order of this OutLet
            var length = customer.listOD.length;
            element = null;
            for (var i = 0; i < length; i++) {
                element = customer.listOD[i];
                // Do something with element i.
                if (element.status == 2) {
                    infoHtml += '<br/>&nbsp;&nbsp;<strong>Đơn hàng số</strong>: ' + element.Code +
                                '<br/>&nbsp;&nbsp;<strong>Sản lượng (thùng/két)</strong>: ' + element.DropSize +
                                '<br/>&nbsp;&nbsp;<strong>Giá trị đơn hàng (VND)</strong>: ' + element.TotalAmt +
                                '<br/>&nbsp;&nbsp;<strong>SKU</strong>: ' + element.TotalSKU +
                                '<br/>&nbsp;&nbsp;<strong>Giờ vào</strong>: ' + element.StartTime +
                                '<br/>&nbsp;&nbsp;<strong>Giờ ra</strong>: ' + element.EndTime +
                                '<br/>&nbsp;&nbsp;<strong>Khoảng cách đến cửa hàng(m)</strong>: ' + element.Distance + '<br/>';
                }
                else if (element.status == 3) {
                    infoHtml += '<br/>&nbsp;&nbsp;<strong>Đơn hàng số</strong>: không có' +
                                '<br/>&nbsp;&nbsp;<strong>Lý do</strong>: ' + element.Reason +
                                '<br/>&nbsp;&nbsp;<strong>Giờ vào</strong>: ' + element.StartTime +
                                '<br/>&nbsp;&nbsp;<strong>Giờ ra</strong>: ' + element.EndTime +
                                '<br/>&nbsp;&nbsp;<strong>Khoảng cách đến cửa hàng(m)</strong>: ' + element.Distance + '<br/>';
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
                    infoHtml += '<br/>&nbsp;&nbsp;<strong>Đơn hàng số</strong>: ' + element.Code +
                                '<br/>&nbsp;&nbsp;<strong>Sản lượng (thùng/két)</strong>: ' + element.DropSize +
                                '<br/>&nbsp;&nbsp;<strong>Giá trị đơn hàng (VND)</strong>: ' + element.TotalAmt +
                                '<br/>&nbsp;&nbsp;<strong>SKU</strong>: ' + element.TotalSKU +
                                '<br/>&nbsp;&nbsp;<strong>Giờ vào</strong>: ' + element.StartTime +
                                '<br/>&nbsp;&nbsp;<strong>Giờ ra</strong>: ' + element.EndTime +
                                '<br/>&nbsp;&nbsp;<strong>Khoảng cách đến cửa hàng(m)</strong>: ' + element.Distance + '<br/>';
                }
                else if (element.status == 3) {
                    infoHtml += '<br/>&nbsp;&nbsp;<strong>Đơn hàng số</strong>: không có' +
                                '<br/>&nbsp;&nbsp;<strong>Lý do</strong>: ' + element.Reason +
                                '<br/>&nbsp;&nbsp;<strong>Giờ vào</strong>: ' + element.StartTime +
                                '<br/>&nbsp;&nbsp;<strong>Giờ ra</strong>: ' + element.EndTime +
                                '<br/>&nbsp;&nbsp;<strong>Khoảng cách đến cửa hàng(m)</strong>: ' + element.Distance + '<br/>';
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
                '<h3>(' + order + ') ' + customer.OutletName + '</h3>' +
                '<br/><strong>Mã CH</strong>: ' + customer.OutletID +
                '<br/><strong>Tên CH</strong>: ' + customer.OutletName +
                '<br/><strong>Người liên hệ</strong>: ' + customer.OutletName +
                '<br/><strong>Địa chỉ</strong>: ' + customer.Address +
                '<br/><strong>Mã NVBH</strong>: ' + customer.SalesmanID +
                '<br/><strong>Tên nhân viên BH</strong>: ' + customer.SalesmanName +
                '<br/><strong>Latitude</strong>: ' + customer.Latitude +
                '<br/><strong>Longitude</strong>: ' + customer.Longtitude +
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
                    infoHtml += '<br/>&nbsp;&nbsp;<strong>Đơn hàng số</strong>: ' + element.Code +
                                '<br/>&nbsp;&nbsp;<strong>Sản lượng (thùng/két)</strong>: ' + element.DropSize +
                                '<br/>&nbsp;&nbsp;<strong>Giá trị đơn hàng (VND)</strong>: ' + element.TotalAmt +
                                '<br/>&nbsp;&nbsp;<strong>SKU</strong>: ' + element.TotalSKU +
                                '<br/>&nbsp;&nbsp;<strong>Giờ vào</strong>: ' + element.StartTime +
                                '<br/>&nbsp;&nbsp;<strong>Giờ ra</strong>: ' + element.EndTime +
                                '<br/>&nbsp;&nbsp;<strong>Khoảng cách đến cửa hàng(m)</strong>: ' + element.Distance + '<br/>';
                }
                else if (element.status == 3) {
                    infoHtml += '<br/>&nbsp;&nbsp;<strong>Đơn hàng số</strong>: không có' +
                                '<br/>&nbsp;&nbsp;<strong>Lý do</strong>: ' + element.Reason +
                                '<br/>&nbsp;&nbsp;<strong>Giờ vào</strong>: ' + element.StartTime +
                                '<br/>&nbsp;&nbsp;<strong>Giờ ra</strong>: ' + element.EndTime +
                                '<br/>&nbsp;&nbsp;<strong>Khoảng cách đến cửa hàng(m)</strong>: ' + element.Distance + '<br/>';
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
////RENDER Outlet MARKER






pgmap.showSalesmenStepAuto = function () {
    window.setTimeout("pgmap.renderSalesmenStepAuto()", 0);
};
pgmap.showSalesmenStep = function () {
    $("div.loading[obj=map]").show();
    window.setTimeout("pgmap.renderSalesmenStep()", 0);
};
pgmap.renderSalesmenStep = function () {
    var toIndex = pgmap.salemanData.length;
    var total = pgmap.salemanData.length;
    if (total > 0 && toIndex > pgmap.SalesmenStepRendered) {
        renderSalesmenStep();
    }
    $("div.loading[obj=map]").hide();
}
pgmap.renderSalesmenStepAuto = function () {
    var toIndex = pgmap.salemanData.length;
    var total = pgmap.salemanData.length;
    if (total > 0 && toIndex > pgmap.SalesmenStepRendered) {
        renderSalesmenStep();
        if (pgmap.SalesmenStepStop == 0) {
            window.setTimeout("pgmap.renderSalesmenStepAuto()", pgmap.SalesmenStepSecond);
        }
    }
    else {
        $('#btnSalemanStepAuto').show();
        $('#btnSalemanStop').hide();
    }
    $("div.loading[obj=map]").hide();
}
function renderSalesmenStep() {
    var latLng = new google.maps.LatLng(pgmap.salemanData[pgmap.SalesmenStepRendered].Latitude, pgmap.salemanData[pgmap.SalesmenStepRendered].Longtitude);

    var labelContent = '';
    if (pgmap.ddlLabelInfo == 1) {
        labelContent = pgmap.salemanData[pgmap.SalesmenStepRendered].strVisitTime;
    }
    else {
        labelContent = pgmap.SalesmenStepRendered + 1;
    }

    var marker = new MarkerWithLabel({
        position: latLng,
        draggable: false,
        map: pgmap.map,
        icon: base_url + 'Content/THP/images/route.png',
        labelContent: labelContent,//pgmap.SalesmenStepRendered + 1, //pgmap.salemanData[pgmap.SalesmenStepRendered].strVisitTime, //ODVisitOrder
        labelAnchor: new google.maps.Point(8, 27),
        labelClass: "labelmaps", // the CSS class for the label
        labelStyle: { opacity: 0.75 }
    });

    if (pgmap.SalesmenStepRendered == 0) {
        //if (source == null) {
        source = latLng;
        destination = latLng;
    }
    else {
        source = destination;
        destination = latLng;
        var route = [source, destination];
        var polyline = new google.maps.Polyline({
            path: route,
            strokeColor: '#64FE2E',
            strokeOpacity: 1.0,
            strokeWeight: 2
        });
        pgmap.polylines.push(polyline);
        polyline.setMap(pgmap.map);
    }

    //Add new marker
    pgmap.salemanMarkers.push(marker);

    pgmap.bounds.extend(latLng);

    $("div.loading[obj=map]").hide();
    window.setTimeout(pgmap.time, 0);

    //pgmap.map.setZoom(16);
    pgmap.map.panTo(destination);

    pgmap.SalesmenStepRendered = pgmap.SalesmenStepRendered + 1;
}