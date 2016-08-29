var logedin = 0;
var base_url = '/';
//Document Onload
$(document).ready(function () {
    $('.button').button();
    $('body').css('overflow', 'hidden');
    $("#txt_date").datepicker({
        maxDate: 0,
        dateFormat: 'dd-mm-yy'
    }).change(function () {
        LoadSalesmanByDistributor();
    });

    LoadSalesmanByDistributor();

    //Hide button when begin processing
    $('input[type="button"]').click(function () {
        hideButton();
    });

    pgmap.init();
    $('input[type="checkbox"][name="SalesmanID"]:checked').removeAttr("checked");

    //Render Left MENU
    $('.arrowleft').live('click', function () {
        $(".left").toggle("fast", function () {
            google.maps.event.trigger(pgmap.map, 'resize');
            $(".map .showfilter").toggle();
            if ($(this).is(":visible"))
                $("div.list-search h3, div.list-search div.result, div.loading").css("left", "338px");
            else
                $("div.list-search h3, div.list-search div.result, div.loading").css("left", "0px");
        });
        $(this).parent('.content').toggleClass("active");
        return false;
    });
});

function PAINT() {
    BeginPaint();
    if ($('#renderMCP').is(":checked")) {
        filterSaleman($('#txt_salesman').val());
        LoadSalesmanInfo();
        LoadVisitInfo();
    }

    if ($('#renderSM').is(":checked")) {
        renderSalemanMovement();
    }

    if ($('#renderSS').is(":checked")) {
        renderSSMovement();
    }

    if ($('#renderASM').is(":checked")) {
        renderASMMovement();
    }
}

function PAINTSTEP() {
    BeginPaint();
    if ($('#renderMCP').is(":checked")) {
        filterSaleman($('#txt_salesman').val());
        LoadSalesmanInfo();
        LoadVisitInfo();
    }

    if ($('#renderSM').is(":checked")) {
        renderSalemanStep();
    }

    if ($('#renderSS').is(":checked")) {
        renderSSMovement();
    }

    if ($('#renderASM').is(":checked")) {
        renderASMMovement();
    }
}

function BeginPaint() {
    var totalCheckedItems = $("input[type=checkbox][group=" + "SalesmanID" + "]:checked").length;
    $('#txt_salesman').val($("input[type=checkbox][group=" + "SalesmanID" + "]:checked").first().val());

    pgmap.clear();
    pgmap.init();

    pgmap.SalesmenStepRendered = 0;
    pgmap.SalesmenStepStop = 0;

    if ($('#renderMCPLine').is(":checked")) {
        pgmap.renderLine = 1;
    }
    else {
        pgmap.renderLine = 0;
    }

    if ($('#ddlLabelInfo').val() == 'VisitOrder') {
        pgmap.ddlLabelInfo = 0;
    }
    else {
        pgmap.ddlLabelInfo = 1;
    }
}

//Render List Outlet
function filterSelection() {
    //View Salesman
    if (pgmap.selection == 0) {
        renderSalemanLocation();
    }
    else {
        var d = new Date();
        var h = d.getHours();
        var m = d.getMinutes();
        $("strong[id=hour]").html(h + "h:" + m);
        filterApply();
    }
}

//LoadVisitInfo
function LoadVisitInfo() {
    $('#divVisitInfo').html('');
    $.post("/Tracking/TableVisitInfo", $("#mapfrm").serialize(), function (data) {
        if (data != null) {
            if (data.list.length > 0) {
                var index = data.list.length;
                var strTB = '';
                strTB += '<table class="visitInfoTable" cellpadding="0" cellspacing="0" >';
                strTB += '<thead>';
                strTB += '<tr>';
                strTB += '<th>VisitOrder</th>';
                strTB += '<th>OutletID</th>';
                strTB += '<th>OutletName</th>';
                strTB += '<th>Address</th>';
                strTB += '<th nowrap >SM: ' + data.list[0].item.SalesmanID + '<br />' + data.list[0].item.SalesmanName + '</th>';
                strTB += '<th nowrap >Khoảng cách SM</th>';
                strTB += '<th nowrap >SUP: ' + data.list[0].item.SaleSupID + '<br />' + data.list[0].item.SaleSupName + '</th>';
                strTB += '<th nowrap >Khoảng cách SUP</th>';
                strTB += '<th nowrap >ASM: ' + data.list[0].item.ASMID + '<br />' + data.list[0].item.ASMName + '</th>';
                strTB += '<th nowrap >Khoảng cách ASM</th>';
                strTB += '</tr>';
                strTB += '</thead>';
                strTB += '<tbody>';

                for (var i = 0; i < index; i++) {
                    strTB += '<tr>';
                    strTB += '<td nowrap onclick="MapPanToOutletSMSSASM(' + data.list[i].item.Latitude + ',' + data.list[i].item.Longtitude + ',' + data.list[i].item.SMLatitude + ',' + data.list[i].item.SMLongitude + ',' + data.list[i].item.SUPLatitudeStart + ',' + data.list[i].item.SUPLongtitudeStart + ',' + data.list[i].item.ASMLatitudeStart + ',' + data.list[i].item.ASMLongtitudeStart + ');" >' + data.list[i].item.VisitOrder + '</td>';
                    strTB += '<td nowrap onclick="MapPanToOutletSMSSASM(' + data.list[i].item.Latitude + ',' + data.list[i].item.Longtitude + ',' + data.list[i].item.SMLatitude + ',' + data.list[i].item.SMLongitude + ',' + data.list[i].item.SUPLatitudeStart + ',' + data.list[i].item.SUPLongtitudeStart + ',' + data.list[i].item.ASMLatitudeStart + ',' + data.list[i].item.ASMLongtitudeStart + ');" >' + data.list[i].item.OutletID + '</td>';
                    strTB += '<td nowrap onclick="MapPanToOutletSMSSASM(' + data.list[i].item.Latitude + ',' + data.list[i].item.Longtitude + ',' + data.list[i].item.SMLatitude + ',' + data.list[i].item.SMLongitude + ',' + data.list[i].item.SUPLatitudeStart + ',' + data.list[i].item.SUPLongtitudeStart + ',' + data.list[i].item.ASMLatitudeStart + ',' + data.list[i].item.ASMLongtitudeStart + ');" >' + data.list[i].item.OutletName + '</td>';
                    strTB += '<td nowrap onclick="MapPanToOutletSMSSASM(' + data.list[i].item.Latitude + ',' + data.list[i].item.Longtitude + ',' + data.list[i].item.SMLatitude + ',' + data.list[i].item.SMLongitude + ',' + data.list[i].item.SUPLatitudeStart + ',' + data.list[i].item.SUPLongtitudeStart + ',' + data.list[i].item.ASMLatitudeStart + ',' + data.list[i].item.ASMLongtitudeStart + ');" >' + data.list[i].item.Address + '</td>';
                    strTB += '<td nowrap onclick="MapPanToSM(' + data.list[i].item.SMLatitude + ',' + data.list[i].item.SMLongitude + ');" >' + data.list[i].SMTime + '</td>';
                    strTB += '<td nowrap onclick="MapPanToSM(' + data.list[i].item.SMLatitude + ',' + data.list[i].item.SMLongitude + ');" >' + data.list[i].item.SMDistance + '</td>';
                    strTB += '<td nowrap onclick="MapPanToSS(' + data.list[i].item.SUPLatitudeStart + ',' + data.list[i].item.SUPLongtitudeStart + ');" >' + data.list[i].SUPTime + '</td>';
                    strTB += '<td nowrap onclick="MapPanToSS(' + data.list[i].item.SUPLatitudeStart + ',' + data.list[i].item.SUPLongtitudeStart + ');" >' + data.list[i].item.SUPDistance + '</td>';
                    strTB += '<td nowrap onclick="MapPanToASM(' + data.list[i].item.ASMLatitudeStart + ',' + data.list[i].item.ASMLongtitudeStart + ');" >' + data.list[i].ASMTime + '</td>';
                    strTB += '<td nowrap onclick="MapPanToASM(' + data.list[i].item.ASMLatitudeStart + ',' + data.list[i].item.ASMLongtitudeStart + ');" >' + data.list[i].item.ASMDistance + '</td>';
                    strTB += '</tr>';
                }

                strTB += '</tbody>';
                strTB += '</table>';
                $('#divVisitInfo').html(strTB);
            }
        }
    });
//    $.ajax({
//        url: '@Url.Action("TableVisitInfo", "Home")',
//        type: "post",
//        data: $('#mapfrm').serialize(),
//        success: function (data) {
//            if (data != null) {
//                var strTB = '';
//                strTB = data.list;

//                $('#divVisitInfo').html(strTB);
//            }
//            else {
//                alert('Không hợp lệ');
//            }
//        },
//        error: function () {
//        }
//    });
}


//Render List Outlet - 1
function filterApply() {
    //CheckSession();
    $("div.loading").show();
    $("div.loading div.progressbar").each(function () {
        $(this).progressbar({ value: 0 })
    });
    $("div.loading div.status").text("Loading...");
    $.post("/Tracking/Filter", $("#mapfrm").serialize(), function (data) {
        $("#resulttable tr").remove();
        pgmap.data = data.html;
        pgmap.salemanData = data.smLocation;
        pgmap.renderSaleman();
        setTimeout("displayCustomer()", 1);
        //setTimeout("displayDatatable()", 1);
    }, 'json');
}

function filterSaleman(salesman) {
    document.getElementById("txt_salesman").value = salesman;
    pgmap.selection = 1;
    filterApply();
}

//Render List Outlet - 2
function displayCustomer() {
    pgmap.showMarkers();
}

function displayDatatable() {
    $("#resulttable tr").remove();
    $("div.loading[obj=table]").show();
    setTimeout("renderData(0)", 0);
}

function renderData(fromIndex) {
    if (!$("#gmap_canvas").is(":hidden")) {
        var toIndex = fromIndex + pgmap.iterationRender;
        if (toIndex > pgmap.data.length)
            toIndex = pgmap.data.length;

        for (var i = fromIndex; i < toIndex; i++) {
            var latLng = new google.maps.LatLng(pgmap.data[i].latitude, pgmap.data[i].longitude);
            var fn = pgmap.markerClickFunction(i + 1, pgmap.data[i], latLng);
            $("#resulttable").find('tbody')
                    .append($('<tr>')
                    .append($('<td width=40>').append($('<span>').text(i + 1)).css("text-align", "center"))
                    .append($('<td width=80>').append($('<span>').text(pgmap.data[i].SalesmanID)).css("background-color", $("div[code=" + pgmap.data[i].SalesmanID + "]").css("background-color")))
                    .append($('<td width=80>').append($('<span>').text(pgmap.data[i].code)))
                    .append($('<td width=200>').append($('<span>').text(pgmap.data[i].site)))
                    .append($('<td width=80>').append($('<span>').text(pgmap.data[i].segment)).css("background-color", $("div[code=" + pgmap.data[i].segment + "]").css("background-color")))
                    .append($('<td width=80>').append($('<span>').text(pgmap.data[i].classid)).css("background-color", $("div[code=" + pgmap.data[i].classid + "]").css("background-color")))
                    .append($('<td width=80>').append($('<span>').text(pgmap.data[i].route)).css("background-color", $("div[code=" + pgmap.data[i].route + "]").css("background-color")))
                    .append($('<td>'))
                    .attr('id', "r" + i)
                    );
            google.maps.event.addDomListener(document.getElementById("r" + i), 'click', fn);
        }

        if (toIndex >= pgmap.data.length)
            $("div.loading[obj=table]").hide();
        else {
            var value = (toIndex * 100) / pgmap.data.length;
            $("div.loading[obj=table] div.progressbar").progressbar({ value: value });
            $("div.loading[obj=table] div.status").text(toIndex + " / " + pgmap.data.length);
            setTimeout("renderData(" + toIndex + ")", 0);
        }
    }
}
////Render List Outlet



//Render Saleman Location
function renderSalemanLocation() {

    $('#divTest').html('');
    LogTime('renderSalemanLocation');

    CheckSession();
    pgmap.selection = 0;
    $("div.loading").show();
    $("div.loading div.progressbar").each(function () {
        $(this).progressbar({ value: 0 })
    });
    $("div.loading div.status").text("Loading...");
    LogTime('BeginAJAX');
    $.post("/Tracking/Refresh", $("#mapfrm").serialize(), function (data) {
        //displaySalesman();
        if (data == "") {
            alert("Salesman này không có lịch viếng thăm khách hàng vào ngày đã chọn");
        }
        pgmap.data = data.html;
        LogTime('BeginPaint');
        setTimeout("displaySalesman()", 0);
    }, 'json');
}

function displaySalesman() {
    LogTime('displaySalesman');
    pgmap.showSalesmenMarkers();
}
////Render Saleman Location

//renderSalemanMovement
function renderSalemanMovement() {

    $('#divTest').html('');
    LogTime('renderSalemanLocation');

    CheckSession();
    pgmap.selection = 0;
    $("div.loading").show();
    $("div.loading div.progressbar").each(function () {
        $(this).progressbar({ value: 0 })
    });
    $("div.loading div.status").text("Loading...");
    LogTime('BeginAJAX');
    $.post("/Tracking/SalemanMovement", $("#mapfrm").serialize(), function (data) {
        if (data.html.length == 0) {
            alert("Salesman này không có lộ trình vào ngày đã chọn");
        }
        pgmap.salemanData = data.html;
        LogTime('BeginPaint');
        setTimeout("mapSalemanMovement()", 0);
    }, 'json');
}

function mapSalemanMovement() {
    LogTime('mapSalemanMovement');
    pgmap.showSalesmenMovement();
}
//renderSalemanMovement


//renderSSMovement
function renderSSMovement() {

    $('#divTest').html('');
    LogTime('renderSSLocation');

    CheckSession();
    pgmap.selection = 0;
    $("div.loading").show();
    $("div.loading div.progressbar").each(function () {
        $(this).progressbar({ value: 0 })
    });
    $("div.loading div.status").text("Loading...");
    LogTime('BeginAJAX');
    $.post("/Tracking/SSMovement", $("#mapfrm").serialize(), function (data) {
        if (data.html.length == 0) {
            alert("Salessup này không có lộ trình vào ngày đã chọn");
        }

        pgmap.SSData = data.html;
        LogTime('BeginPaint');
        setTimeout("mapSSMovement()", 0);
    }, 'json');
}

function mapSSMovement() {
    LogTime('mapSSMovement');
    pgmap.showSSMovement();
}
//renderSSMovement

//renderASMMovement
function renderASMMovement() {

    $('#divTest').html('');
    LogTime('renderASMLocation');

    CheckSession();
    pgmap.selection = 0;
    $("div.loading").show();
    $("div.loading div.progreASMbar").each(function () {
        $(this).progreASMbar({ value: 0 })
    });
    $("div.loading div.status").text("Loading...");
    LogTime('BeginAJAX');
    $.post("/Tracking/ASMMovement", $("#mapfrm").serialize(), function (data) {
        if (data.html.length == 0) {
            alert("ASM này không có lộ trình vào ngày đã chọn");
        }
        pgmap.ASMData = data.html;
        LogTime('BeginPaint');
        setTimeout("mapASMMovement()", 0);
    }, 'json');
}

function mapASMMovement() {
    LogTime('mapASMMovement');
    pgmap.showASMMovement();
}
//renderASMMovement

//Focus to salesman
function focusSalesman() {
    if (pgmap.salesmanLocation !== pgmap.center)
        pgmap.map.setCenter(pgmap.salesmanLocation);
}

//Focus to last visit
function focusLastOutlet() {
    if (pgmap.lastOutlet !== pgmap.center)
        pgmap.map.setCenter(pgmap.lastOutlet);
}

function focusSaleSup() {
    if (pgmap.salesupLocation !== pgmap.center)
        pgmap.map.setCenter(pgmap.salesupLocation);
}

function focusASM() {
    if (pgmap.asmLocation !== pgmap.center)
        pgmap.map.setCenter(pgmap.asmLocation);
}

function getMap() {
    return pgmap;
}

//renderSalemanMovement
function renderSalemanStep() {
    $('#divTest').html('');
    LogTime('renderSalemanLocation');

    CheckSession();
    pgmap.selection = 0;
    $("div.loading").show();
    $("div.loading div.progressbar").each(function () {
        $(this).progressbar({ value: 0 })
    });
    $("div.loading div.status").text("Loading...");
    LogTime('BeginAJAX');
    $.post("/Tracking/SalemanMovement", $("#mapfrm").serialize(), function (data) {
        if (data.html.length == 0) {
            alert("Salesman này không có lộ trình vào ngày đã chọn");
        }
        pgmap.salemanData = data.html;
        pgmap.SalesmenStepRendered = 0;
        pgmap.SalesmenStepSecond = $('#txt_second_to_play').val() * 1000;
        pgmap.SalesmenStepStop = 0;
        LogTime('BeginPaint');
        setTimeout("mapSalemanStep()", 0);
    }, 'json');
}

function mapSalemanStep() {
    LogTime('mapSalemanStep');
    pgmap.showSalesmenStep();
}

function mapSalemanStepAuto() {
    pgmap.SalesmenStepStop = 0;
    $('#btnSalemanStepAuto').hide();
    $('#btnSalemanStop').show();
    LogTime('mapSalemanStep');
    pgmap.showSalesmenStepAuto();
}
function mapSalemanStop() {
    $('#btnSalemanStepAuto').show();
    $('#btnSalemanStop').hide();
    LogTime('mapSalemanStep');
    pgmap.SalesmenStepStop = 1;
}

//renderSalemanMovement



















function showInfo(divID) {
    $("#" + divID).css('display', 'block');
}
function CloseInfo(divID) {
    $("#" + divID).css('display', 'none');
}

function showButton() {
    //$('input[type="button"].process').removeAttr('disabled');
}

function hideButton() {
    //$('input[type="button"].process').attr('disabled', 'disabled');
}

function LogTime(str) {
    //        var d1 = new Date();
    //        var newDate = d1.toString('h:m:i');
    //        $('#divTest').html($('#divTest').html() + '<br />' + str + ' : ' + newDate);
}

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
function contains(a, obj) {
    var i = a.length;
    while (i--) {
        if (a[i] === obj) {
            return true;
        }
    }
    return false;
}