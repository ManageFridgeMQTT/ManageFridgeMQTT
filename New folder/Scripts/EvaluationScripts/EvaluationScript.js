// Những function được xài sau khi turning 
var ListGeo = [];
var ListImageSelect = [];
var ListUserLeader = [];
var currentURL = '../';

$(document).ajaxStart(function () {
    $(".loading").show();
});
$(document).ajaxStop(function () {
    $(".loading").hide();
});

function ShowMessageNoData(mess) {
    $('#mSNoData').text(mess);
    $('#shCenter').show();
    $('#shCenter').delay(2500).fadeOut();
}

function EvalProcessing() {
    $('#RunProcessing').prop('disabled', true);
    $.ajax({
        type: 'POST',
        url: currentURL + 'Evaluation/RunAutoMark',
        dataType: "json",
        data: { 'DisplayID': "sds" },
        success: function (result) {
            ShowMessageNoData(result);
        }
    }).done(function () {
    });

    $('.slides_container').addClass('scanning');

    var myInterval = setInterval(function () {
        $.ajax({
            type: 'GET',
            url: currentURL + 'Evaluation/GetDataAutoInProcessing',
            dataType: "json",
            success: function (result) {
                var arr = result;
                if (result.TotalImages != 0) {
                    var val = (result.ImagesProgress * 100) / result.TotalImages;
                    var timeMarking = 0;
                    if (result.TimeMarking > 0) {
                        timeMarking = result.TimeMarking / 60;
                    }
                    progressbar.progressbar("value", val);
                    $('#OutletMarking').html(result.OutletMarking + ' / ' + result.TotalOutlet);
                    $('#TimeMarking').html(timeMarking.toFixed(1) + ' / ' + result.TimePlanMarking + 'Phút');
                    $('#TimeAverage').html(result.TimeAverage.toFixed(2) + '/ ảnh');
                    $('#ImgPass').html(result.ImgPass + ' / ' + result.ImagesProgress);
                    $('#ImgThat').html(result.ImgThat + ' / ' + result.ImagesProgress);
                    $('#ImgChuan').html(result.ImgChuan + ' / ' + result.ImagesProgress);
                    $('#ImgNumberic').html(result.ImgNumberic + ' / ' + result.ImagesProgress);
                    $('#ImgNotExist').html(result.ImgNotExist + ' / ' + result.ImagesProgress);
                    $('#ImgErrorMarking').html(result.ImgErrorMarking + ' / ' + result.ImagesProgress);
                    if (result.ImagesProgress == result.TotalImages) {
                        ShowMessageNoData('Đã xử lý xong');
                        clearTimeout(myInterval);
                        $('.slides_container').removeClass('scanning');
                    }
                }
            }
        }).done(function () {
        });

    }, 500);

}

function SearchDetailByInput() {

    var FilterData = {
        inputEvaluationID: '', inputReferenceCD: '',
        inputDisplay: '', inputDescription: '', inputContent: '',
        inputDisplayDateFrom: '', inputDisplayDateTo: '',
        inputEvalDateFrom: '', inputEvalDateTo: '',
        inputScreenID: '', inputUserID: '', inputStatus: '',
        inputNumPage: '', inputNumPaging: ''
    }

    FilterData.inputEvaluationID = $('#EvaluationCode').val();
    FilterData.inputReferenceCD = $('#refnumber').val();
    FilterData.inputDisplay = $('#selectedDisplayID').val();
    FilterData.inputDescription = $('#EvalDesc').val();
    FilterData.inputContent = $('#EvalCont').val();
    FilterData.inputDisplayDateFrom = $('#timeDisfrom').val();
    FilterData.inputDisplayDateTo = $('#timeDisto').val();
    FilterData.inputEvalDateFrom = $('#timeEvalfrom').val();
    FilterData.inputEvalDateTo = $('#timeEvalto').val();
    FilterData.inputScreenID = $('#ScreenID').val();
    FilterData.inputUserID = $('#UserID').val();
    FilterData.inputStatus = $('#EvalState').val();
    FilterData.inputNumPage = $('#NumPageId').val();
    FilterData.inputNumPaging = $('#NumPagingId').val();

    $.ajax({
        type: 'GET',
        url: 'SearchDetailByInput',
        data: FilterData,
        success: function (result) {
            window.location = window.location.pathname;
        }
    }).done(function () {
    });
}

function stringToDate(_date, _format, _delimiter) {
    var formatLowerCase = _format.toLowerCase();
    var formatItems = formatLowerCase.split(_delimiter);
    var dateItems = _date.split(_delimiter);
    var monthIndex = formatItems.indexOf("mm");
    var dayIndex = formatItems.indexOf("dd");
    var yearIndex = formatItems.indexOf("yyyy");
    var month = parseInt(dateItems[monthIndex]);
    month -= 1;
    var formatedDate = new Date(dateItems[yearIndex], month, dateItems[dayIndex]);
    return formatedDate;
}
var ListResult = [];
onload = function () {
    $("table").on('click', '#my_table_EvaluationID tr', function () {
        $(this).addClass('selected').siblings().removeClass('selected');
        var keyVal = $(this).find('td:nth-child(2)').html();
        $('#EvaluationCode').val(keyVal);
    });

    var rows = $('#my_table_Outlets').children();
    for (i = 0; i < rows.length; i++) {
        rows[i].onclick = function () {
            var nextOutletID = this.children[1].innerHTML;//Ma cua hang duoc chon 
            var nextOutletName = this.children[2].innerHTML;//Ten cua hang duoc chon 

            $('#nextOutletID').val(nextOutletID);
            $('#nextOutletName').val(nextOutletName);

            OutletBrowser('BySelection');
        }
    }

    $('input:radio[name="ViewType"]').change(function () {
        var sViewType = $(this).val();

        $.ajax({
            type: 'GET',
            url: currentURL + 'Evaluation/ChangeViewTypeInDetailOutletImageEvaluationOrReview',
            data: { 'sViewType': sViewType },
            success: function (result) {
                window.location = window.location.pathname;
            }
        }).done(function () {
        });
    });

    if ($('#ScreenID').val() == "VSC002") {
        //if ($('#PageID').val() == "Detail Evaluation View" || $('#PageID').val() == "Xem Chi Tiết Kỳ Đánh Giá") {

        $('#EvaluationCode').prop('readonly', true);
        $('#EvaluationCode').prop('disabled', true);
        $('#btncode').prop('readonly', true);
        $('#btncode').prop('disabled', true);

        $('#DisplayCode').prop('readonly', true);
        $('#DisplayCode').prop('disabled', true);
        $('#btndisplay').prop('readonly', true);
        $('#btndisplay').prop('disabled', true);

        $('#refnumber').prop('readonly', true);
        $('#refnumber').prop('disabled', true);
        $('#timeDisfrom').prop('readonly', true);
        $('#timeDisfrom').prop('disabled', true);
        $('#timeDisto').prop('readonly', true);
        $('#timeDisto').prop('disabled', true);
        $('#timeEvalfrom').prop('readonly', true);
        $('#timeEvalfrom').prop('disabled', true);
        $('#timeEvalto').prop('readonly', true);
        $('#timeEvalto').prop('disabled', true);
        $('#EvalDesc').prop('readonly', true);
        $('#EvalDesc').prop('disabled', true);
        $('#EvalCont').prop('readonly', true);
        $('#EvalCont').prop('disabled', true);

        $('#timeDisfrom').hide();
        $('#timeDisto').hide();
        $('#timeEvalfrom').hide();
        $('#timeEvalto').hide();
        $('#lbtimeDisplay').hide();
        $('#lbtimeEvaluation').hide();

        $('#EvalState').prop('readonly', true);
        $('#EvalState').prop('disabled', true);
        $('#btnsearch').hide();

    }
    if ($('#ScreenID').val() == "VSC003") {
        //if ($('#PageID').val() == "Detail Evaluation Definition" || $('#PageID').val() == "Chi Tiết Kỳ Đánh Giá") {
        $('#EvaluationCode').prop('readonly', true);
        $('#EvaluationCode').prop('disabled', true);
        $('#btncode').prop('readonly', true);
        $('#btncode').prop('disabled', true);

        $('#EvalDesc').prop('readonly', true);
        $('#EvalDesc').prop('disabled', true);

        //$('#EvalState').prop('readonly', true);
        //$('#EvalState').prop('disabled', true);
        $('#EvalState').hide();
        $('#labelEvalState').hide();

        $('#timeDisfrom').hide();
        $('#timeDisto').hide();
        $('#timeEvalfrom').hide();
        $('#timeEvalto').hide();
        $('#lbtimeDisplay').hide();
        $('#lbtimeEvaluation').hide();
        $('#btnsearch').hide();



        var RefCD = sessionStorage.getItem('RefCD');
        if (RefCD !== null) $('#refnumber').val(RefCD);
    }

    if ($('#ScreenID').val() == "VSC005") {
        $('#box-filter-Status').hide();
        $('#addFilter').hide();
        $('#box-filter-Type').hide();
        $('#box-filter-TimeEvaluation').hide();
        $('#box-filter-Content').hide();


    }

    if ($('#ScreenID').val() == "VSC006") {
        var checkFinished = $('#ChkFinished').val();
        $('#image-' + $('#ImageIndex').val()).addClass('active');
        var ListLyDoChamDiem = $('#DSLyDoChamDiem').val();
        var ArrayLyDoChamDiem = ListLyDoChamDiem.split(';');
    }
    if ($('#ScreenID').val() == "VSC007") {
        $('#box-filter-Status').hide();
        $('#addFilter').hide();
        $('#box-filter-Type').hide();
        $('#box-filter-TimeEvaluation').hide();
        $('#box-filter-Content').hide();
    }
    if ($('#ScreenID').val() == "VSC008") {
        $('#image-' + $('#ImageIndex').val()).addClass('active');

    }
    $('input:radio[name="okay"]').change(function () {
        if ($(this).val() == '1') {
            $('#ReviewReason').prop('disabled', true);
            $("#ReviewReason").val("");
        } else {
            $('#ReviewReason').prop('disabled', false);
        }
    });
}

function CancelListEmployee() {
    $('#participant-list').modal('hide');
}
function ChooseListEmployee() {
    //Chọn Danh Sách Nhân Viên Tham Gia
    //$('#participant-list').on('hidden.bs.modal', function (e) {
    var DSNhanVienDuocChon = "";
    var DSNhanVienChonTruoc = "";
    var rows = $('#my_table_Participant').children();
    var rowEs = $('#my_table_Member').children();
    for (i = 0; i < rows.length; i++) {
        if ($('#participant-' + rows[i].children[0].innerHTML).is(':checked'))
            DSNhanVienDuocChon = DSNhanVienDuocChon + $('#participant-' + rows[i].children[0].innerHTML).val() + "-" + rows[i].children[1].innerHTML + ";";
    }
    if (rowEs != null && rowEs.length > 0) {
        for (i = 0; i < rowEs.length; i++) {
            DSNhanVienChonTruoc = DSNhanVienChonTruoc + $('#participantL-' + i).val() + "-" + rowEs[i].children[2].innerHTML + ";";
        }
    }
    DSNhanVienDuocChon = DSNhanVienDuocChon + DSNhanVienChonTruoc;
    for (i = 0; i < rows.length; i++) {
        $('#participant-' + rows[i].children[0].innerHTML).prop('checked', false)
    }
    $.ajax({
        type: 'GET',
        url: currentURL + 'Evaluation/LoadParticipantListForEvaluation',
        data: { 'DSNhanVienDuocChon': DSNhanVienDuocChon },
        success: function (result) {
            $('#participant-list').modal('hide');
            $('#userlistbyselection').load( currentURL + 'Evaluation/UserListBySelection');
            //window.location = window.location.pathname;
        }
    }).done(function () {
    });
    //})
}

function SelectGeo()
{
    $('#DSKhuVucDiaLy').val("");
    $('#DSKhuVucDiaLy').val(ListGeo.join(';'));
    $('#geo-area-list').modal('hide');
}
function CancelSelectGeo() {
    var keyVal = $('#DisplayID').val();
    var dateFrom = $('#DisplayDateFrom').val();
    var dateTo = $('#DisplayDateTo').val();
    $.ajax({
        type: 'POST',
        url: currentURL + 'Evaluation/ReloadGeoologyData',
        dataType: "json",
        data: { 'DisplayID': keyVal, 'DispFromDate': dateFrom, 'DispToDate': dateTo },
        success: function (result) {
            ListGeo = [];
            ListResult = result;
            var selectedGeo = $('#DSKhuVucDiaLy').val().split(';');
            ListGeo = selectedGeo;
            $.each(ListResult, function (i, item) {
                var strItem = "+" + item.MaKhuVuc;
                if ($.inArray(strItem, ListGeo) > -1) {
                    item.state.checked = true;
                    item.state.selected = true;
                }
                else {
                    item.state.checked = false;
                    item.state.selected = false;
                }
            });
            geoData = ListResult.reduce(function (r, a) {
                function getParent(s, b) {
                    return b.id === a.parentId ? b : (b.nodes && b.nodes.reduce(getParent, s));
                }
                var index = 0, node;
                if ('parentId' in a) {
                    node = r.reduce(getParent, {});
                }
                if (node && Object.keys(node).length) {
                    node.nodes = node.nodes || [];
                    node.nodes.push(a);
                } else {
                    while (index < r.length) {
                        if (r[index].parentId === a.id) {
                            a.nodes = (a.nodes || []).concat(r.splice(index, 1));
                        } else {
                            index++;
                        }
                    }
                    r.push(a);
                }
                return r;
            }, []);

            var $checkableTree = $('#geo-area-treeview').treeview({
                data: geoData,
                showIcon: false,
                collapsed: true,
                unique:true,
                showCheckbox: true,
                selectable: false,
                multiSelect: true,
                highlightSelected: false,
                onNodeChecked: function (event, node) {
                    ListGeo.push("+" + node.MaKhuVuc);
                },
                onNodeUnchecked: function (event, node) {
                    ListGeo = jQuery.grep(ListGeo, function (a) {
                        return a !== "+" + node.MaKhuVuc;
                    });
                }
            });
        }
    })

    //var listGeo = ListResult;
    //var dataGeo = listGeo.reduce(function (r, a) {
    //    function getParent(s, b) {
    //        return b.id === a.parentId ? b : (b.nodes && b.nodes.reduce(getParent, s));
    //    }
    //    var index = 0, node;
    //    if ('parentId' in a) {
    //        node = r.reduce(getParent, {});
    //    }
    //    if (node && Object.keys(node).length) {
    //        node.nodes = node.nodes || [];
    //        node.nodes.push(a);
    //    } else {
    //        while (index < r.length) {
    //            if (r[index].parentId === a.id) {
    //                a.nodes = (a.nodes || []).concat(r.splice(index, 1));
    //            } else {
    //                index++;
    //            }
    //        }
    //        r.push(a);
    //    }
    //    return r;
    //}, []);
    //$('#geo-area-treeview').html('');

    //var $RefrehTree = $('#geo-area-treeview').treeview({
    //    data: ListResult,
    //    showIcon: false,
    //    showCheckbox: true,
    //    selectable: false,
    //    multiSelect: true,
    //    highlightSelected: false,
    //    onNodeChecked: function (event, node) {
    //        ListGeo.push("+" + node.MaKhuVuc);
    //    },
    //    onNodeUnchecked: function (event, node) {
    //        ListGeo = jQuery.grep(ListGeo, function (a) {
    //            return a !== "+" + node.MaKhuVuc;
    //        });
    //    }
    //});
}
function GetAllOutletDisplay() {
    $('#DSKhuVucDiaLy').val('');
    for (var item in ListResult) {
        if (ListResult[item].parentId == "")
            $('#DSKhuVucDiaLy').val($('#DSKhuVucDiaLy').val() + "+" + ListResult[item].MaKhuVuc + ";");
    }
}
function ApplyReviewEmployee() {
    if ($('#isApplyReview').is(':checked')) {
        $('#ReviewRate').show();
    } else {
        $('#ReviewRate').hide();
        //$('#ReviewRate').val('');
    }
}
function VisibleSelectEmployee() {
    if ($('#assumeManu').is(':checked')) {
        $('#btnSelectEmployee').show();
    } else {
        $('#btnSelectEmployee').hide();
    }
}
window.onbeforeunload = function () {
    sessionStorage.setItem("RefCD", $('#refnumber').val());
}

function LoadItemListByRefNumber() {

    if ($('#PageID').val() == "Detail Evaluation Definition" || $('#PageID').val() == "Chi Tiết Kỳ Đánh Giá") {
        var sinputRef = $('#refnumber').val();
        $.ajax({
            type: 'GET',
            url: currentURL + 'Evaluation/LoadItemListByRefNumber',
            data: { 'sinputRef': sinputRef },
            success: function (result) {
                $('#itemlistbyrefnbr').load(currentURL + 'Evaluation/ItemListByReferenceNbr');
            }
        }).done(function () {
        });
    }
}

//Định nghĩa - thêm mới lưu validate
var Evaluation_ConfirmYesNo = "";
var Evaluation_ValidateDisPlayNull = "";
var Evaluation_ValidateListRegionNull = "";
var Evaluation_ValidateTimeInformationEmpty = "";
var Evaluation_ValidateTypeEvaluationNull = "";
var Evaluation_SaveOki = "";
var Evaluation_ValidateSumOutletCurentMustSumOutletFill = '';
var Evaluation_ValidateCheckKPI = "";
var Evaluation_ValiCheck_KPI_Radio4Image = "";
var Evaluation_ValiCheck_KPI_FakeImage = "";
var Evaluation_ValiCheck_KPI_StandardImage = "";
var Evaluation_ValiCheck_KPI_PassImage = "";
var Evaluation_ValiCheck_KPI_NumbericImage = "";
var Evaluation_ValiCheck_Reason_PassImage = "";
var Evaluation_ValiCheck_Reason_Standard = "";
var Evaluation_ValiCheck_Reason_Fake = "";


function RedirectURLEvaluation(url) {
    if (url) {
        var EvaluationID = $('#EvaluationCode').val();
        if (EvaluationID == '' || EvaluationID == undefined) {
            $('#ErMesgBox span[class^="error"]').html('Please save the data before go to next step.');
        }
        else {
            if ($('input[id=assumeAuto]:checked').length > 0) {
                url = url.replace("DistributeOutletToAuditor", "EvalAutoMark");
                url = url + '?sEvalID=' + EvaluationID;
            } else {
                url = url + '?EvaluationCode=' + EvaluationID;
            }
            window.location = url;
        }
    }
}
function remove_duplicates() {
    var objectsArray = [];
    var rows = $('#my_table_Member').children();
    for (i = 0; i < rows.length; i++) {
        var item = { key: rows[i].children[2].innerHTML, Role: $("#participantL-" + i + " option:selected").val() };
        objectsArray.push(item);
    }
    var usedObjects = {};
    for (var i = objectsArray.length - 1; i >= 0; i--) {
        var so = JSON.stringify(objectsArray[i]);

        if (usedObjects[so]) {
            objectsArray.splice(i, 1);
        } else {
            usedObjects[so] = true;
        }
    }
    return objectsArray;
}
function CancelEvaluationAction() {
    var sEvalID = $('#EvaluationCode').val();
    var answer = confirm("Bạn có muốn hủy mã đánh giá " + sEvalID + " hay không ?")
    if (answer == false) {
        return;
    }

    $.ajax({
        url: 'CancelEvaluationAction',
        method: 'GET',
        data: { 'sEvalID': sEvalID },
        success: function (result) {
            alert(result);
        }
    }).done(function () {
    });
}

function ModifyParticipants() {
    //var answer = confirm("Bạn có muốn thay đổi danh sách nhân viên không ?")
    //if (answer == false) {
    //    return;
    //}

    DanhSachUserID = "";
    DanhSachUserRole = "";
    var rows = $('#my_table_Member').children();
    for (i = 0; i < rows.length; i++) {
        var roles = rows[i].children[6];
        if (roles.innerHTML > 0) {
            DanhSachUserID = DanhSachUserID + ";" + rows[i].children[2].innerHTML;
            DanhSachUserRole = DanhSachUserRole + ";" + roles.innerHTML.toString();
        }
    }
    //alert(DanhSachUserRole);
    $.ajax({
        url: currentURL + 'Evaluation/ModifyParticipants',
        method: 'GET',
        data: {
            'DanhSachUserID': DanhSachUserID, 'DanhSachUserRole': DanhSachUserRole
        },
        success: function (result) {
            //confirm(result);
            $('#OkiMesgBox span[class^="error"]').html(Evaluation_SaveOki);
        }
    }).done(function () {
    });
    //$('#OkiMesgBox span[class^="error"]').html(Evaluation_SaveOki);
}
function CalculateSum() {
    var arr = document.getElementsByClassName('outlet-distribute');
    var tot = 0;
    for (var i = 0; i < arr.length; i++) {
        if (parseInt(arr[i].value))
            tot += parseInt(arr[i].value);
    }

    $('#outlet-sum').val(tot.toString());
}
function AutoDistributeOutletToAuditor() {
    var arr = document.getElementsByClassName('outlet-distribute');
    for (var i = 0; i < arr.length; i++) {
        if (i < arr.length - 1)
            arr[i].value = $('#IDOfSoPhanBo').val();
        else
            arr[i].value = $('#IDOfSoOutlet').val() - ((arr.length - 1) * $('#IDOfSoPhanBo').val());
    }

    $('#outlet-sum').val($('#IDOfSoOutlet').val());
}
function ConfirmationOfDistribution() {

    if ($('#outlet-sum').val() != $('#IDOfSoOutlet').val()) {
        //alert("Tổng số outlet chưa bằng tổng số phân bổ. Xin kiểm tra lại");
        $('#ErMesgBox span[class^="error"]').html("");
        $('#ErMesgBox span[class^="error"]').html(Evaluation_ValidateSumOutletCurentMustSumOutletFill);
    } else {
        $('#ErMesgBox span[class^="error"]').html("");


        var DistributionDataFrom = "";
        var DistributionDataTo = "";

        var arr = document.getElementsByClassName('outlet-distribute');
        var tot = 0;
        for (var i = 0; i < arr.length; i++) {
            DistributionDataFrom = DistributionDataFrom + (tot + 1).toString() + ";";
            tot += parseInt(arr[i].value);
            DistributionDataTo = DistributionDataTo + tot.toString() + ";";
        }
        var sEvalID = $('#EvaluationCode').val();

        $.ajax({
            type: 'GET',
            url: currentURL + 'Evaluation/ProcessDistributionOutletToAuditor',
            data: { 'sEvalID': sEvalID, 'DistributionDataFrom': DistributionDataFrom, 'DistributionDataTo': DistributionDataTo },
            success: function (result) {
                //confirm(result);
                if (result == 1) {
                    $('#OkiMesgBox span[class^="error"]').html("");
                    $('#OkiMesgBox span[class^="error"]').html(Evaluation_SaveOki);
                } else {
                    $('#ErMesgBox span[class^="error"]').html("");
                    $('#ErMesgBox span[class^="error"]').html(Evaluation_Validate_EvalID_Evaluting);
                }

            }
        }).done(function () {
        });
    }
}
// Called from SCREEN VSC005: "Đánh Giá Hình Ảnh"
function FilterOutletImageEvalByStatus() {
    var AllStatus = "";
    var EachStatus = "";

    EachStatus = "0";
    if ($('input[id=EvalFinished]:checked').length > 0) EachStatus = "1";
    AllStatus += EachStatus + ";";

    EachStatus = "0";
    if ($('input[id=EvalGoing]:checked').length > 0) EachStatus = "1";
    AllStatus += EachStatus + ";";

    EachStatus = "0";
    if ($('input[id=RevFinished]:checked').length > 0) EachStatus = "1";
    AllStatus += EachStatus + ";";

    EachStatus = "0";
    if ($('input[id=RevRejected]:checked').length > 0) EachStatus = "1";
    AllStatus += EachStatus + ";";

    if (AllStatus == "0;0;0;0;") AllStatus = "1;1;1;1;";

    $.ajax({
        type: 'GET',
        url: 'FilterOutletImageEvalByStatus',
        data: { 'AllStatus': AllStatus },
        success: function (result) { }
    }).done(function () {
    });
}

function GoToDetailOutletImageEvaluation(AllOrCurrent) {

    var rows = $('#my_table_DetailEvaluation').children();
    var selectedAuditorID = "";
    var selectedOutletName = "";
    var selectedEvaluationID = "";
    AllOrCurrent = "All";
    if (AllOrCurrent == "All") {
        selectedAuditorID = rows[0].children[2].innerHTML;
        selectedOutletID = rows[0].children[3].innerHTML;
        selectedOutletName = rows[0].children[4].innerHTML;
        selectedEvaluationID = rows[0].children[11].innerHTML;

        $.ajax({
            type: 'GET',
            url: currentURL + 'Evaluation/GoToDetailOutletImageEvaluation',
            data: { 'selectedEvaluationID': selectedEvaluationID, 'selectedAuditorID': selectedAuditorID, 'selectedOutletID': selectedOutletID, 'selectedOutletName': selectedOutletName },
            success: function (result) { }
        }).done(function () {
        });

    }

    for (i = 0; i < rows.length; i++) {
        rows[i].onclick = function () {
            selectedAuditorID = this.children[2].innerHTML; // Cột 0 là STT, cột 1 là MaAuditor
            selectedOutletID = this.children[3].innerHTML;
            selectedOutletName = this.children[4].innerHTML;
            selectedEvaluationID = rows[0].children[11].innerHTML;

            $.ajax({
                type: 'GET',
                url: currentURL + 'Evaluation/GoToDetailOutletImageEvaluation',
                data: { 'selectedEvaluationID': selectedEvaluationID, 'selectedAuditorID': selectedAuditorID, 'selectedOutletID': selectedOutletID, 'selectedOutletName': selectedOutletName },
                success: function (result) { }
            }).done(function () {
            });
        }
    }
}
// Called from SCREEN VSC006: "Chi Tiết Đánh Giá Hình Ảnh"
function OutletBrowser(buttontype) {
    //BySelection
    var nextOutletName = "";
    var nextOutletID = "";
    var nextImageIndex = 0;

    if (buttontype == 'BySelection') {
        // Khi chon nut search
        //alert($('#sl_outlet').val());
        nextOutletID = $('#sl_outlet').val();
        //nextOutletName = $('#nextOutletName').val();
    }
    else {
        nextOutletID = $('#nextOutletID').val();
        nextOutletName = $('#nextOutletName').val();
        //var rows = $('#my_table_Outlets').children();
        //for (i = 0; i < rows.length; i++) {
        //    if (rows[i].children[1].innerHTML == $('#selectedOutletID').val()) {
        //        switch (buttontype) {
        //            case 'first':
        //                j = 0;
        //                break;
        //            case 'back':
        //                j = i - 1;
        //                break;
        //            case 'next':
        //                j = i + 1;
        //                break;
        //            case 'last':
        //                j = rows.length - 1;
        //                break;
        //        }

        //        if (j < 0 || j > rows.length - 1) j = i;

        //        nextOutletID = rows[j].children[1].innerHTML;
        //        nextOutletName = rows[j].children[2].innerHTML;
        //        break;
        //    }
        //}
    }

    //Luu Thong Tin Truoc Khi Thay Doi Qua Image Khac
    var Reason1 = $('#Reason_1').find(":selected").index().toString();
    var Reason2 = $('#Reason_2').find(":selected").index().toString();
    var Reason3 = $('#Reason_3').find(":selected").index().toString();
    var ReviewReason = $('#ReviewReason').find(":selected").index().toString();

    var isMatchedWithBefore = ($('input[id=isMatchedWithBefore]:checked').length > 0 ? "1" : "0");
    var isCaptured = ($('input[id=isCaptured]:checked').length > 0 ? "1" : "0");
    var isAccepted = ($('input[id=isAccepted]:checked').length > 0 ? "1" : "0");
    var isPassed = ($('input[id=isPassed]:checked').length > 0 ? "1" : "0");

    var sOutletID = $('#selectedOutletID').val();
    var sImageIndex = $('#ImageIndex').val();
    var ListNumericChamDiem = $('#DSNumericChamDiem').val();

    var ImageReviewResult = "";
    ImageReviewResult = ($('input[id=ReviewAccept]:checked').length > 0 ? "1" : "0");
    ImageReviewReason = $('#ReviewReason').find(":selected").index().toString();
    $('#ImageReviewResult').val(ImageReviewResult);
    $('#ImageReviewReason').val(ImageReviewReason);
    //VSC006

    //if ($('#ScreenID').val() == "VSC005") {
    //if ($('#PageID').val() == "Detail Outlet Image Evaluation" || $('#PageID').val() == "Chi Tiết Đánh Giá Hình Ảnh") {        
    $.ajax({
        type: 'GET',
        url: currentURL + 'Evaluation/SaveEvaluationResultIntoCurrentImageID',
        data: {
            'sOutletID': sOutletID, 'sImageIndex': sImageIndex, 'ListNumericChamDiem': ListNumericChamDiem,
            'Reason1': Reason1, 'Reason2': Reason2, 'Reason3': Reason3, 'ReviewReason': ReviewReason,
            'isMatchedWithBefore': isMatchedWithBefore, 'isCaptured': isCaptured, 'isAccepted': isAccepted, 'isPassed': isPassed
        },
        success: function (result) {

        }
    }).done(function () {
    });

    //$('#selectedOutletID').val(nextOutletID);
    //$('#selectedOutletName').val(nextOutletName);
    //alert(nextOutletID);

    $.ajax({
        url: currentURL + 'Evaluation/LoadNextDataToDetailOutletImageEvaluationScreen',
        data: { 'nextOutletID': nextOutletID, 'nextOutletName': nextOutletName, 'nextImageIndex': nextImageIndex, 'funtion': buttontype },
        type: 'POST',
        success: function (result) {
            window.location = window.location.pathname + "?selectedEvaluationID=" + result.selectedEvaluationID + "&selectedAuditorID=" + result.selectedAuditorID + "&selectedOutletID=" + result.currentOutletID + "&selectedOutletName=" + result.currentOutletName;;
        },
        error: function (err) {
            //alert(err);
        }
    });
    //$.ajax({
    //    type: 'GET',
    //    url: 'LoadNextDataToDetailOutletImageEvaluationScreen',
    //    data: { 'nextOutletID': nextOutletID, 'nextOutletName': nextOutletName, 'nextImageIndex': nextImageIndex},
    //    success: function (result) {

    //        // window.location = window.location.pathname;
    //        //var initialPage = window.location.pathname;
    //        //alert($('#nextOutletID').val());
    //        //var url = location.host + window.location.pathname + "?selectedEvaluationID=";
    //        //location.replace();
    //        //alert(window.location.pathname);
    //        //alert(location.host);
    //        //alert(location);               


    //    }
    //}).done(function () {
    //});
    // }

    //if ($('#PageID').val() == "Detail Outlet Image Review" || $('#PageID').val() == "Chi Tiết Xét Duyệt Đánh Giá") {
    //    $.ajax({
    //        type: 'GET',
    //        url: 'SaveReviewResultIntoCurrentImageID',
    //        data: { 'ImageReviewResult': ImageReviewResult, 'ImageReviewReason': ImageReviewReason, 'sOutletID': sOutletID, 'sImageIndex': sImageIndex },
    //        success: function (result) { }
    //    }).done(function () {
    //    });

    //    $('#selectedOutletID').val(nextOutletID);
    //    $('#selectedOutletName').val(nextOutletName);

    //    $.ajax({
    //        type: 'GET',
    //        url: 'LoadNextDataToDetailOutletImageReviewScreen',
    //        data: { 'nextOutletID': nextOutletID, 'nextOutletName': nextOutletName, 'nextImageIndex': nextImageIndex},
    //        success: function (result) {
    //            window.location = window.location.pathname;
    //        }
    //    }).done(function () {
    //    });
    //}

}
function GetSelectedEvalImage(selectedObj) {

    $('#selectedEvalLargeImage').attr("src", selectedObj.attr('data-image-link'));
    $('#selectedEvalSmallImage').attr("src", selectedObj.attr('data-image-link'));
    //$('#selectedComparedImage').attr("src", selectedObj.attr('data-image-compare'));
    //$('#selectedAvatarImage').attr("src", selectedObj.attr('data-image-avatar'));

    //Luu Thong Tin Truoc Khi Thay Doi Qua Image Khac
    //var ListLyDoChamDiem = "";
    var Reason1 = $('#Reason_1').find(":selected").index().toString();
    var Reason2 = $('#Reason_2').find(":selected").index().toString();
    var Reason3 = $('#Reason_3').find(":selected").index().toString();
    var ReviewReason = $('#ReviewReason').find(":selected").index().toString();

    //ListLyDoChamDiem += $('#Reason_1').find(":selected").index().toString() + ";";
    //ListLyDoChamDiem += $('#Reason_2').find(":selected").index().toString() + ";";
    //ListLyDoChamDiem += $('#Reason_3').find(":selected").index().toString() + ";";
    //ListLyDoChamDiem += $('#ReviewReason').find(":selected").index().toString() + ";";
    //$('#DSLyDoChamDiem').val(ListLyDoChamDiem);

    var isMatchedWithBefore = ($('input[id=isMatchedWithBefore]:checked').length > 0 ? "1" : "0");
    var isCaptured = ($('input[id=isCaptured]:checked').length > 0 ? "1" : "0");
    var isAccepted = ($('input[id=isAccepted]:checked').length > 0 ? "1" : "0");
    var isPassed = ($('input[id=isPassed]:checked').length > 0 ? "1" : "0");

    //var ListKQChamDiem = "";
    //ListKQChamDiem += ($('input[id=isMatchedWithBefore]:checked').length > 0 ? "1" : "0") + ";";
    //ListKQChamDiem += ($('input[id=isCaptured]:checked').length > 0 ? "1" : "0") + ";";
    //ListKQChamDiem += ($('input[id=isAccepted]:checked').length > 0 ? "1" : "0") + ";";
    //ListKQChamDiem += ($('input[id=isPassed]:checked').length > 0 ? "1" : "0") + ";";
    //$('#DSKQChamDiem').val(ListKQChamDiem);

    var sOutletID = $('#selectedOutletID').val();
    var sImageIndex = $('#ImageIndex').val();
    var ListNumericChamDiem = $('#DSNumericChamDiem').val();
    $.ajax({
        type: 'GET',
        url: currentURL + 'Evaluation/SaveEvaluationResultIntoCurrentImageID',
        data: {
            'sOutletID': sOutletID, 'sImageIndex': sImageIndex, 'ListNumericChamDiem': ListNumericChamDiem,
            'Reason1': Reason1, 'Reason2': Reason2, 'Reason3': Reason3, 'ReviewReason': ReviewReason,
            'isMatchedWithBefore': isMatchedWithBefore, 'isCaptured': isCaptured, 'isAccepted': isAccepted, 'isPassed': isPassed
        },
        success: function (result) { }
    }).done(function () {
    });

    var nextOutletID = $('#selectedOutletID').val();
    var nextOutletName = $('#selectedOutletName').val();
    $('#ImageIndex').val(selectedObj.attr('id'));
    var nextImageIndex = parseInt(selectedObj.attr('id'));

    $.ajax({
        url: currentURL + 'Evaluation/LoadNextDataToDetailOutletImageEvaluationScreen',
        data: { 'nextOutletID': nextOutletID, 'nextOutletName': nextOutletName, 'nextImageIndex': nextImageIndex, 'funtion': '' },
        type: 'POST',
        success: function (result) {
            window.location = window.location.pathname + "?selectedEvaluationID=" + result.selectedEvaluationID + "&selectedAuditorID=" + result.selectedAuditorID + "&selectedOutletID=" + result.currentOutletID + "&selectedOutletName=" + result.currentOutletName;;
        },
        error: function (err) {
            //alert(err);
        }
    });
    //$.ajax({
    //    type: 'GET',
    //    url: 'LoadNextDataToDetailOutletImageEvaluationScreen',
    //    data: { 'nextOutletID': nextOutletID, 'nextOutletName': nextOutletName, 'nextImageIndex': nextImageIndex ,'funtion':''},
    //    success: function (result) {
    //        alert(window.location.pathname + "?selectedEvaluationID=" + result.selectedEvaluationID + "&selectedAuditorID=" + result.selectedAuditorID + "&selectedOutletID=" + result.currentOutletID + "&selectedOutletName=" + result.currentOutletName);
    //        //window.location = window.location.pathname + "?selectedEvaluationID=" + result.selectedEvaluationID + "&selectedAuditorID=" + result.selectedAuditorID + "&selectedOutletID=" + result.currentOutletID + "&selectedOutletName=" + result.currentOutletName;
    //        //window.location = window.location.pathname
    //    }
    //}).done(function () {
    //});

}

function StoreNumericEvaluation() {
    var ListNumericChamDiem = "";

    var rows = $('#my_table_numericitem').children();
    for (i = 0; i < rows.length; i++) {
        if ($('#numitem-' + rows[i].children[1].innerHTML).prop('checked')) {

            //alert(rows[i].children[1].innerHTML);

            ListNumericChamDiem = ListNumericChamDiem + rows[i].children[1].innerHTML + ";";
        }
        else
            ListNumericChamDiem = ListNumericChamDiem + "0;";
    }

    //var rows = $('#my_table_numericitem').children();
    //for (i = 0; i < rows.length; i++) {

    //    var roles = rows[i].children[6];
    //    var stt = rows[i].children[1].innerHTML;

    //    var noRoles = $("#participantL-" + i + " option:selected").val();
    //    if (noRoles == "3") {
    //        DanhSachUserRole = DanhSachUserRole + 1;
    //    }
    //}
    //alert(ListNumericChamDiem);
    $('#DSNumericChamDiem').val(ListNumericChamDiem);
}


// Called from SCREEN VSC007: "Xét Duyệt Đánh Giá"
function GoToDetailOutletImageReview(AllOrCurrent) {
    var rows = $('#my_table_DetailReview').children();
    var selectedAuditorID = "";
    var selectedEvaluationID = "";

    if (AllOrCurrent == "All") {
        selectedAuditorID = rows[0].children[5].innerHTML;
        selectedEvaluationID = rows[0].children[2].innerHTML;

        $.ajax({
            type: 'GET',
            url: currentURL + 'Evaluation/GoToDetailOutletImageReview',
            data: { 'selectedEvaluationID': selectedEvaluationID, 'selectedAuditorID': selectedAuditorID },
            success: function (result) { }
        }).done(function () {
        });
    }


    for (i = 0; i < rows.length; i++) {
        rows[i].onclick = function () {
            selectedAuditorID = this.children[5].innerHTML; // Cột 0 là STT, cột 1 là MaAuditor
            selectedEvaluationID = this.children[2].innerHTML;

            $.ajax({
                type: 'GET',
                url: currentURL + 'Evaluation/GoToDetailOutletImageReview',
                data: { 'selectedEvaluationID': selectedEvaluationID, 'selectedAuditorID': selectedAuditorID },
                success: function (result) { }
            }).done(function () {
            });
        }
    }
}
// Called from SCREEN VSC008: "Chi Tiết Xét Duyệt Đánh Giá"
function GetSelectedRevImage(selectedObj) {
    $('#selectedEvalLargeImage').attr("src", selectedObj.attr('data-image-link'));
    $('#selectedEvalSmallImage').attr("src", selectedObj.attr('data-image-link'));
    //$('#selectedComparedImage').attr("src", selectedObj.attr('data-image-compare'));
    //$('#selectedAvatarImage').attr("src", selectedObj.attr('data-image-avatar'));

    //Luu Thong Tin Truoc Khi Thay Doi Qua Image Khac
    var ImageReviewResult = "";
    ImageReviewResult = ($('input[id=ReviewAccept]:checked').length > 0 ? "1" : "0");
    ImageReviewReason = $('#ReviewReason').find(":selected").index().toString();
    $('#ImageReviewResult').val(ImageReviewResult);
    $('#ImageReviewReason').val(ImageReviewReason);

    var sOutletID = $('#selectedOutletID').val();
    var sImageIndex = $('#ImageIndex').val();

    $.ajax({
        type: 'GET',
        url: currentURL + 'Evaluation/SaveReviewResultIntoCurrentImageID',
        data: { 'ImageReviewResult': ImageReviewResult, 'ImageReviewReason': ImageReviewReason, 'sOutletID': sOutletID, 'sImageIndex': sImageIndex },
        success: function (result) { }
    }).done(function () {
    });

    var nextOutletID = $('#selectedOutletID').val();
    var nextOutletName = $('#selectedOutletName').val();
    $('#ImageIndex').val(selectedObj.attr('id'));
    var nextImageIndex = parseInt(selectedObj.attr('id'));

    $.ajax({
        type: 'GET',
        url: currentURL + 'Evaluation/LoadNextDataToDetailOutletImageReviewScreen',
        data: { 'nextOutletID': nextOutletID, 'nextOutletName': nextOutletName, 'nextImageIndex': nextImageIndex },
        success: function (result) {
            window.location = window.location.pathname;
        }
    }).done(function () {
    });

}
function UpdateReviewStatusEmployeeData() {

    var ImageReviewResult = '';
    ImageReviewResult = ($('input[id=ReviewAccept]:checked').length > 0 ? "1" : "0");
    var sOutletID = $('#selectedOutletID').val();
    var sImageIndex = $('#ImageIndex').val();
    $.ajax({
        url: currentURL + 'Evaluation/SaveStatusReviewRateData',
        data: { 'ImageReviewResult': ImageReviewResult, 'ImageReviewReason': $('#ReviewReason').val(), 'sOutletID': sOutletID, 'sImageIndex': sImageIndex },
        type: 'POST',
        success: function (response) {
            //alert(window.location.pathname);
            window.location = window.location.pathname;
        },
        error: function (err) {
            //alert(err);
        }
    });

}
function SaveReviewImageData() {
    var ImageReviewResult = '';
    ImageReviewResult = ($('input[id=ReviewAccept]:checked').length > 0 ? "1" : "0");
    var sOutletID = $('#selectedOutletID').val();
    var sImageIndex = $('#ImageIndex').val();
    var error = "";
    var Reason = $('#Reason_3').val();
    // Kiem cac radio button phai duoc check   
    if (typeof ($('input[name=okay]:checked').val()) == 'undefined') {
        if (error == "") {
            error = Evaluation_ValiCheck_KPI_ApproveImage;
        } else {
            error = error + ", " + Evaluation_ValiCheck_KPI_ApproveImage;
        }
    } else {
        // Kiểm tra Nếu không thì bắt buột chọn lý do.
        if ($('input[name=okay]:checked').val() == 0) {
            if (Reason == '') {
                if (error == "") {
                    error = Evaluation_ValiCheck_Reason
                } else {
                    error = error + ", " + Evaluation_ValiCheck_Reason;
                }
            }
        }
    }
    if (error == "") {
        $.ajax({
            url: currentURL + 'Evaluation/SaveReviewImageData',
            data: { 'ImageReviewResult': ImageReviewResult, 'ImageReviewReason': $('#ReviewReason').val(), 'sOutletID': sOutletID, 'sImageIndex': sImageIndex },
            type: 'GET',
            success: function (response) {
                //alert(window.location.pathname);
                window.location = window.location.pathname;
            },
            error: function (err) {
                //alert(err);
            }
        });
    } else {
        alert(error);
    }
}
// Called from SCREEN VSC009: "Baseline Kỳ Đánh Giá"
function BaseLineProcess() {

    var BaselineByDate = ($('input[id=BaselineByDate]:checked').length > 0 ? "1" : "0");
    var BaselineByDateFrom = $('#BaselineByDateFrom').val();
    var BaselineByDateTo = $('#BaselineByDateTo').val();

    var BaselineByWeek = ($('input[id=BaselineByWeek]:checked').length > 0 ? "1" : "0");
    var BaselineByWeekNo = $('#BaselineByWeekNo').find(":selected").index().toString();
    var BaselineByWeek_MonthYear = $('#BaselineByWeek_MonthYear').val();

    var BaselineByMonth = ($('input[id=BaselineByMonth]:checked').length > 0 ? "1" : "0");
    var BaselineByMonthNo = $('#BaselineByMonthNo').val();

    var ListEvaluationForBaseline = "";
    var rows = $('#my_table_EvaluationForBaseline').children();
    for (i = 0; i < rows.length; i++) {
        if ($('#numitem-' + rows[i].children[2].innerHTML).prop('checked'))
            ListEvaluationForBaseline = ListEvaluationForBaseline + rows[i].children[2].innerHTML + ";";
    }


    $.ajax({
        type: 'GET',
        url: currentURL + 'Evaluation/BaseLineProcess',
        data: {
            'ListEvaluationForBaseline': ListEvaluationForBaseline,
            'BaselineByDate': BaselineByDate,
            'BaselineByWeek': BaselineByWeek,
            'BaselineByMonth': BaselineByMonth,
            'BaselineByDateFrom': BaselineByDateFrom,
            'BaselineByDateTo': BaselineByDateTo,
            'BaselineByWeekNo': BaselineByWeekNo,
            'BaselineByWeek_MonthYear': BaselineByWeek_MonthYear,
            'BaselineByMonthNo': BaselineByMonthNo
        },
        success: function (result) { confirm(result); }
    }).done(function () {
    });
}

function BaseLinedView() {
    var ListEvaluationForBaseline = "";
    var rows = $('#my_table_EvaluationForBaseline').children();
    for (i = 0; i < rows.length; i++) {
        if ($('#numitem-' + rows[i].children[2].innerHTML).prop('checked'))
            ListEvaluationForBaseline = ListEvaluationForBaseline + rows[i].children[2].innerHTML + ";";
    }

    $.ajax({
        type: 'GET',
        url: currentURL + 'Evaluation/BaseLinedView',
        data: {
            'ListEvaluationForBaseline': ListEvaluationForBaseline,
        },
        success: function (result) {
        }
    }).done(function () {
    });
}

function BackToTheMainPage() {
    $.ajax({
        type: 'GET',
        url: currentURL + 'Evaluation/BackToTheMainPage',
        data: {
            'EvalID': "",
        },
        success: function (result) {
        }
    }).done(function () {
    });
}
// Called from SCREEN VSC010: "Đánh Giá Hình Ảnh Tự Động"
function AutoEvaluationProcessing() {
    var answer = confirm("Bạn có muốn bắt đầu tự động đánh giá hay không ?")
    if (answer == false) {
        return;
    }

    $.ajax({
        type: 'GET',
        url: currentURL + 'Evaluation/AutoEvaluationProcessing',
        data: {},
        success: function (result) {
            //document.location.reload();
            window.location = window.location.pathname;
            var refreshId = setInterval(auto_refresh, 1000);
        }
    }).done(function () {
        clearInterval(refreshId);
    });

}

function auto_refresh() {
    var timeinsecond = +$('#TimeRunning').val();
    timeinsecond = timeinsecond + 1;
    $('#TimeRunning').val(timeinsecond.toString());
}

function AutoGoToDetailOutletImageEvaluation() {

    var rows = $('#my_table_DetailAutoEvaluation').children();
    var selectedAuditorID = "";
    var selectedOutletName = "";

    for (i = 0; i < rows.length; i++) {
        rows[i].onclick = function () {
            selectedOutletID = this.children[2].innerHTML;
            selectedAuditorID = "Automatic";
            selectedOutletName = this.children[3].innerHTML;


            $.ajax({
                type: 'GET',
                url: currentURL + 'Evaluation/AutoGoToDetailOutletImageEvaluation',
                data: { 'selectedAuditorID': selectedAuditorID, 'selectedOutletID': selectedOutletID, 'selectedOutletName': selectedOutletName },
                success: function (result) { }
            }).done(function () {
            });
        }
    }
}

function ChooseRole(item) {
    var role = $(item).val();
    var optionLeader = '';
    var rows = $('#my_table_Member').children();
    for (i = 0; i < rows.length; i++) {
        var noRoles = $("#participantL-" + i + " option:selected").val();
        if (noRoles == "2") {
            optionLeader += "<option value='" + rows[i].children[2].innerHTML + "' >" + rows[i].children[3].innerHTML + "</option>";
        }
    }
    
    $("#my_table_Member td select[name='leader-user']").each(function () {
        var value = $(this).val();
        if (value != undefined) {
            $(this).html(optionLeader);
            $(this).children('option').each(function () {
                if ($(this).val() == value) {
                    $(this).attr('selected', true);
                }
            });
        }
    });

    if (role == "3") {
        $(item).parents('td').find("select[name='leader-user']").html(optionLeader);
        $(item).parents('td').find("select[name='leader-user']").show();
        $(item).parents('td').find("span").show();
    } else if (role == "2" || role == "1") {
        $(item).parents('td').find("select[name='leader-user']").html("");
        $(item).parents('td').find("select[name='leader-user']").hide();
        $(item).parents('td').find("span").hide();
    }



}