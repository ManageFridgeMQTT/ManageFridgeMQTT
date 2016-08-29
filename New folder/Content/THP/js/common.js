var IC_ERROR_MESSAGE = 1, IC_INFO_MESSAGE = 0;
//Document Onload
$(document).ready(function () {
    $('.button').button();
    $('input[readonly]').css('backgroundColor', '#f6f6f6');
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
    $('.left h3 .arrow').click(function () {
        if ($(this).parents('.nav-left li').hasClass('active'))
            $(this).parents('.nav-left li').removeClass('active');
        else
            $(this).parents('.nav-left li').addClass('active');

        $('.nav-left li:not(:first)').find('>.description').slideUp(function () {
            $(this).parents(".nav-left li").removeClass("active");
        });
        $(this).parents('.nav-left li').find('>.description').slideDown(function () {
            $(this).parents('.nav-left li').addClass('active');
            $('.box-content').css("overflow", "auto"); //.jScrollPane();
        });
    });

    $(".right h3 .arrow").click(function () {
        $(".list-search .result").fadeToggle();
        $('.right h3').toggleClass("active");
    });

    $("div.colorbox[list=dsm]").each(function () {
        $(this).css("background-color", colors[$("div.colorbox[list=dsm]").index($(this))]);
    })
    $("div.colorbox[list=dsr]").each(function () {
        $(this).css("background-color", colors[$("div.colorbox[list=dsr]").index($(this))]);
    })
    $("div.colorbox[list=segment]").each(function () {
        $(this).css("background-color", colors[$("div.colorbox[list=segment]").index($(this))]);
    })
    $("div.colorbox[list=classid]").each(function () {
        $(this).css("background-color", colors[$("div.colorbox[list=classid]").index($(this))]);
    })
    $("div.colorbox[list=route]").each(function () {
        $(this).css("background-color", colors[$("div.colorbox[list=route]").index($(this))]);
    })
    //Init Events
    if ($.isFunction($.jScrollPane))
        $('.box-content').jScrollPane();
    $('.nav-left li:not(:first)').find('>.description').hide();
});

function requestAjax(o) {
    $.ajax({
        url: o.url, data: o.data || null,
        type: o.type || 'GET', dataType: o.dataType || 'json',
        error: function (r) {
            if (typeof o.error == 'function') {
                o.error(r);
            } else {
                showMessage(r.responseText);
            }
        },
        success: function (r) {
            if (r.status == true) {
                if (typeof o.success == 'function') {
                    o.success(r.data);
                } else {
                    showMessage(r.data, IC_INFO_MESSAGE);
                }
            }
            else if (r.status == false) {
                showMessage(r.data, IC_ERROR_MESSAGE);
            } else if (r.redirect) {
                showMessage(r.data, IC_ERROR_MESSAGE);
                setTimeout(function () {
                    document.location.href = r.redirect
                }, 5000);
            }
        }
    });
}

function showMessage(m, t) {
    var ms = $('#messages');
    if (ms.length == 0) {
        alert(m);
    }
    else {
        var mt = (new Date()).getTime();
        ms.append("<div id='m" + mt + "' class='message ui-widget " + (t == IC_INFO_MESSAGE ? "ui-state-highlight" : "ui-state-error") + "'>" +
                "<div class='content'><span class='ui-icon ui-icon-close' onclick=\"$(this).parents('.message').fadeOut();\"></span>" +
                "<span class='ui-icon " + (t == IC_INFO_MESSAGE ? "ui-icon-info" : "ui-icon-alert") + "'></span>" +
                "<p>" + m + "</p><div class='clear'></div></div></div>");
        setTimeout(function () {
            $('#m' + mt).fadeOut(1000, function () {
                $(this).remove();
            });
        }, t == IC_INFO_MESSAGE ? 3000 : 5000);
    }
}

$.fn.clearForm = function () {
    return this.each(function () {
        var type = this.type, tag = this.tagName.toLowerCase();
        if (tag == 'form')
            return $(':input', this).clearForm();
        if (type == 'text' || type == 'password' || tag == 'textarea')
            this.value = '';
        else if (type == 'checkbox' || type == 'radio')
            this.checked = false;
        else if (tag == 'select')
            this.selectedIndex = -1;
    });
};

function filterSelection() {
    //View Salesman
    if (pgmap.selection == 0) {
        filterRefresh();
    }
    else {
        var d = new Date();
        var h = d.getHours();
        var m = d.getMinutes();
        $("strong[id=hour]").html(h + "h:" + m);
        filterApply();
    }
}

function filterTest(salesman) {
    document.getElementById("txt_salesman").value = salesman;
    pgmap.selection = 1;
    filterApply();
}

function filterRefresh() {

    $('#divTest').html('');
    LogTime('filterRefresh');

    CheckSession();
    pgmap.selection = 0;
    $("div.loading").show();
    $("div.loading div.progressbar").each(function () {
        $(this).progressbar({ value: 0 })
    });
    $("div.loading div.status").text("Loading...");
    LogTime('BeginAJAX');
    $.post("/Home/Refresh", $("#mapfrm").serialize(), function (data) {
        pgmap.data = data.html;
        LogTime('BeginPaint');
        setTimeout("displaySalesman()", 0);
        //displaySalesman();
        if (data == "") {
            alert("Salesman này không có lịch viếng thăm khách hàng vào ngày đã chọn");
        }
    }, 'json');
}

function filterApply() {
    //CheckSession();
    $("div.loading").show();
    $("div.loading div.progressbar").each(function () {
        $(this).progressbar({ value: 0 })
    });
    $("div.loading div.status").text("Loading...");
    $.post("/Home/Filter", $("#mapfrm").serialize(), function (data) {
        $("#resulttable tr").remove();
        pgmap.data = data.html;
        //vietmap.data = data;
        setTimeout("displayCustomer()", 1);
        setTimeout("displayDatatable()", 1);
    }, 'json');
}

function filterApplyInquiry() {
    //CheckSession();
    //alert("Inquiry");
    $("div.loading").show();
    $("div.loading div.progressbar").each(function () {
        $(this).progressbar({ value: 0 })
    });
    $("div.loading div.status").text("Loading...");
    $.post(base_url + "ajax/MapData/filterInquiry", $("#mapfrm").serialize(), function (data) {
        $("#resulttable tr").remove();
        pgmap.data = data;
        //vietmap.data = data;
        setTimeout("displayCustomerInquiry()", 1);
        setTimeout("displayDatatable()", 1);
    }, 'json');
}


function filterMerApply() {
    //CheckSession();
    $("div.loading").show();
    $("div.loading div.progressbar").each(function () {
        $(this).progressbar({ value: 0 })
    });
    $("div.loading div.status").text("Loading...");
    $.post(base_url + "ajax/MapData/filtermer", $("#mapfrm").serialize(), function (data) {
        pgmap.data = data;
        setTimeout("displayMerchandiser()", 1);
    }, 'json');
}


function displaySalesman() {
    //CheckSession();
    LogTime('displaySalesman');
    pgmap.showSalesmenMarkers();
}

function displayCustomer() {
    //CheckSession();
    pgmap.showMarkers();
}

function displayCustomerInquiry() {
    //CheckSession();
    pgmap.showMarkersInquiry();
}

function displayMerchandiser() {
    //CheckSession();
    if (!$("#gmap_canvas").is(":hidden"))
        pgmap.showMerMarkers();
}

function createCellVolume(value) {
    var bgColor = "#6BCC4D";

    if (value <= 0)
        bgColor = "#f00";

    return $("<td width=80>").text(value)
            .css("background-color", bgColor)
            .css("text-align", "center");
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
                    .append($('<td width=80>').append($('<span>').text(pgmap.data[i].dsr)).css("background-color", $("div[code=" + pgmap.data[i].dsr + "]").css("background-color")))
                    .append($('<td width=80>').append($('<span>').text(pgmap.data[i].code)))
                    .append($('<td width=200>').append($('<span>').text(pgmap.data[i].site)))
                    .append($('<td width=80>').append($('<span>').text(pgmap.data[i].segment)).css("background-color", $("div[code=" + pgmap.data[i].segment + "]").css("background-color")))
                    .append($('<td width=80>').append($('<span>').text(pgmap.data[i].classid)).css("background-color", $("div[code=" + pgmap.data[i].classid + "]").css("background-color")))
                    .append($('<td width=80>').append($('<span>').text(pgmap.data[i].route)).css("background-color", $("div[code=" + pgmap.data[i].route + "]").css("background-color")))
                    .append(createCellVolume(pgmap.data[i].mtd))
                    .append(createCellVolume(pgmap.data[i].lm))
                    .append(createCellVolume(pgmap.data[i].p3m))
                    .append(createCellVolume(pgmap.data[i].p6m))
                    .append(createCellVolume(pgmap.data[i].p12m))
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

function displayDatatable() {
    $("#resulttable tr").remove();
    $("div.loading[obj=table]").show();
    setTimeout("renderData(0)", 0);
}

function updateMapByPeriod() {
    $("#chkLabels").attr("checked", "checked");
    pgmap.periodmarker = $("select[name=period] option:selected").val();
    pgmap.showMarkers();

    vietmap.periodmarker = $("select[name=period] option:selected").val();
    vietmap.showMarkers();

}

function exportData() {
    $("form[name=mapfrm]").attr("action", base_url + "export/data").submit();
    $("form[name=mapfrm]").removeAttr("action");
}

function labelToggle() {
    pgmap.showLabels = $("#chkLabels").is(":checked");
    pgmap.showMarkers();
}

function CircleToggle() {
    pgmap.showCircle = !pgmap.showCircle;
    pgmap.circleToggle();
}

function ClearMap() {
    $('input[type=checkbox]:checked').removeAttr('checked');
    $("#resulttable tr").remove();
    pgmap.map.clear();
}

function ChangeColor(option) {
    var indexColor = 0;
    switch (option) {
        case "DRS":
            pgmap.colorBy = "DRS";
            indexColor = 2;
            break;
        case "Segment":
            pgmap.colorBy = "Segment";
            indexColor = 5;
            break;
        case "Route":
            pgmap.colorBy = "Route";
            indexColor = 6;
            break;
        default:


    }
    if (indexColor != 0) {
        $("div.labels").each(function () {
            var color = $("table#resulttable tr:nth-child(" + $(this).text() + ") td:nth-child(" + indexColor + ")").css("background-color");
            $(this).css("background-color", color);
        })
    }
    else
        $(".labels").css("background-color", "#5874F5");
}

function ChangeMap() {
    if ($("#vietmap_canvas").is(":hidden")) {
        $("#vietmap_canvas").show();
        $("#gmap_canvas").hide();
    }
    else {
        $("#vietmap_canvas").hide();
        $("#gmap_canvas").show();
    }
}

function PrintMap() {
    $("select#pagesize > option").hide();
    if (!$("#vietmap_canvas").is(":hidden"))
        $("select#pagesize > option.VietMap").show();
    else
        $("select#pagesize > option.GMap").show();

    $("#pagesetup").dialog({
        height: 160, modal: true, resizable: false,
        buttons: [
            {
                text: 'OK',
                click: function () {
                    $("#pagesetup").dialog('close');
                    var width = 0, height = 0;
                    var zoom = pgmap.map.getZoom();
                    switch ($('#pagesize').val()) {
                        case '4A0':
                            height = 8987;
                            width = 6357;
                            break;
                        case '2A0':
                            height = 6357;
                            width = 4493;
                            break;
                        case 'A0':
                            //zoom = 17;
                            height = 4493;
                            width = 3178;
                            break;
                        case 'A1':
                            //zoom = 16;
                            height = 3178;
                            width = 2245;
                            break;
                        case 'A2':
                            //zoom = 15;
                            height = 2245;
                            width = 1587;
                            break;
                        case 'A3':
                            //zoom = 14;
                            height = 1587;
                            width = 0;
                            break;
                        default:
                            height = 1122;
                            width = 0;                                //width=793;                                
                            break;
                    }

                    var center = pgmap.map.getCenter();
                    var params = '?ms=' + (new Date()).getTime() + '&clat=' + center.lat() + '&clng=' + center.lng() +
                            '&zoom=' + zoom + '&width=' + width + '&height=' + height;
                    window.open($("#printpreview").attr('map') + params, 'printview');
                }
            },
            {
                text: 'Cancel',
                click: function () {
                    $("#pagesetup").dialog('close');
                }
            }
        ]
    });
}

function loadjscssfile(filename, filetype) {
    var fileref = "";
    if (filetype == "js") { //if filename is a external JavaScript file
        fileref = document.createElement('script')
        fileref.setAttribute("type", "text/javascript");
        fileref.setAttribute("src", filename);
    }
    else if (filetype == "css") { //if filename is an external CSS file
        fileref = document.createElement("link")
        fileref.setAttribute("rel", "stylesheet");
        fileref.setAttribute("type", "text/css");
        fileref.setAttribute("href", filename);
    }
    if (typeof fileref != "undefined")
        document.getElementsByTagName("head")[0].appendChild(fileref)
}

var filesadded = "" //list of files already added

function checkloadjscssfile(filename, filetype) {
    if (filesadded.indexOf("[" + filename + "]") == -1) {
        loadjscssfile(filename, filetype)
        filesadded += "[" + filename + "]" //add to list of files already added, in the form of "[filename1],[filename2],etc"
    }
    else {
        //alert("file already added!")
    }
}

function removejscssfile(filename, filetype) {
    var removedelements = 0
    var targetelement = (filetype == "js") ? "script" : (filetype == "css") ? "link" : "none" //determine element type to create nodelist using
    var targetattr = (filetype == "js") ? "src" : (filetype == "css") ? "href" : "none" //determine corresponding attribute to test for
    var allsuspects = document.getElementsByTagName(targetelement)
    for (var i = allsuspects.length; i >= 0; i--) { //search backwards within nodelist for matching elements to remove
        if (allsuspects[i] && allsuspects[i].getAttribute(targetattr) != null && allsuspects[i].getAttribute(targetattr).indexOf(filename) != -1) {
            allsuspects[i].parentNode.removeChild(allsuspects[i]) //remove element by calling parentNode.removeChild()
            removedelements += 1
        }
    }
    if (removedelements > 0) {
        //alert("Removed "+removedelements+" instances of "+filename)
    }
}

function createjscssfile(filename, filetype) {
    if (filetype == "js") { //if filename is a external JavaScript file
        var fileref = document.createElement('script')
        fileref.setAttribute("type", "text/javascript")
        fileref.setAttribute("src", filename)
    }
    else if (filetype == "css") { //if filename is an external CSS file
        fileref = document.createElement("link")
        fileref.setAttribute("rel", "stylesheet")
        fileref.setAttribute("type", "text/css")
        fileref.setAttribute("href", filename)
    }
    return fileref
}

function replacejscssfile(oldfilename, newfilename, filetype) {
    var replacedelements = 0
    var targetelement = (filetype == "js") ? "script" : (filetype == "css") ? "link" : "none" //determine element type to create nodelist using
    var targetattr = (filetype == "js") ? "src" : (filetype == "css") ? "href" : "none" //determine corresponding attribute to test for
    var allsuspects = document.getElementsByTagName(targetelement)
    for (var i = allsuspects.length; i >= 0; i--) { //search backwards within nodelist for matching elements to remove
        if (allsuspects[i] && allsuspects[i].getAttribute(targetattr) != null && allsuspects[i].getAttribute(targetattr).indexOf(oldfilename) != -1) {
            var newelement = createjscssfile(newfilename, filetype)
            allsuspects[i].parentNode.replaceChild(newelement, allsuspects[i])
            replacedelements += 1
        }
    }
    if (replacedelements > 0) {
        //alert("Replaced "+replacedelements+" instances of "+oldfilename+" with "+newfilename)
    }
}

//post to report-sales
function post_to_url(path, params, method) {
    method = method || "post"; // Set method to post by default, if not specified.

    // The rest of this code assumes you are not using a library.
    // It can be made less wordy if you use one.
    var form = document.createElement("form");
    form.setAttribute("method", method);
    form.setAttribute("action", path);

    for (var key in params) {
        if (params.hasOwnProperty(key)) {
            var hiddenField = document.createElement("input");
            hiddenField.setAttribute("type", "hidden");
            hiddenField.setAttribute("name", params[key].name);
            hiddenField.setAttribute("value", params[key].value);

            form.appendChild(hiddenField);
        }
    }
    var hiddenField = document.createElement("input");
    hiddenField.setAttribute("type", "hidden");
    hiddenField.setAttribute("name", "saleReport");
    hiddenField.setAttribute("value", "empty");
    form.appendChild(hiddenField);
    document.body.appendChild(form);
    form.submit();
}
//post to synchronization-report
function post_to_synReport(path, params, method) {
    method = method || "post"; // Set method to post by default, if not specified.

    // The rest of this code assumes you are not using a library.
    // It can be made less wordy if you use one.
    var form = document.createElement("form");
    form.setAttribute("method", method);
    form.setAttribute("action", path);
    for (var key in params) {
        if (params.hasOwnProperty(key)) {
            var hiddenField = document.createElement("input");
            hiddenField.setAttribute("type", "hidden");
            hiddenField.setAttribute("name", params[key].name);
            hiddenField.setAttribute("value", params[key].value);

            form.appendChild(hiddenField);
        }
    }
    var hiddenField = document.createElement("input");
    hiddenField.setAttribute("type", "hidden");
    hiddenField.setAttribute("name", "saleReport");
    hiddenField.setAttribute("value", "empty");
    form.appendChild(hiddenField);
    document.body.appendChild(form);
    form.submit();
}
//pagination for sales-report
function pagination_sale(page) {
    var hiddenField = document.createElement("input");
    hiddenField.setAttribute("type", "hidden");
    hiddenField.setAttribute("name", "pageSales");
    hiddenField.setAttribute("value", page);
    document.getElementById('search_form').appendChild(hiddenField);
    document.getElementById('search_form').submit();
}
//focus to salesman
function focusSalesman() {
    if (pgmap.salesmanLocation !== pgmap.center)
    pgmap.map.setCenter(pgmap.salesmanLocation);
}
function focusLastOutlet() {
    if (pgmap.lastOutlet !== pgmap.center)
        pgmap.map.setCenter(pgmap.lastOutlet);
}