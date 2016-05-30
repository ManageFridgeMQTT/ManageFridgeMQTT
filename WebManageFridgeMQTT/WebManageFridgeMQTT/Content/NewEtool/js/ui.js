$(document).ready(function () {
    $('.datepicker').datepicker({
        dateFormat: 'dd/mm/yy'
    }).on("change", function (e) {
        var curDate = $(this).val();
        try {
            $.datepicker.parseDate('dd/mm/yy', curDate);
        }
        catch (e) {
            alert("Date to Validate is invalid.");
            var maxDate = new Date();
            maxDate.setDate(maxDate.getDate());
            $(this).datepicker("setDate", maxDate);
        }
    });
    $('.datepicker').datepicker("widget").css({ "z-index": 100 });

    $(document).ajaxStart(function () {
        $(".loading").show();
    });
    $(document).ajaxStop(function () {
        $(".loading").hide();
    });

    //$("html").addClass("nav-mini");

    checkNav();
    checkScreen();
    checkTheme();
    //checkScreenMode();
    checkTableFixed();
    setThumbScroller();
    setTabContentHeight();
    setMapOptionHeight();
    $(window).resize(function (resize) {
        if (resize.isTrigger !== 3) {
            checkNav();
            checkScreen();
            checkTableFixed();
            checkScheduleHeight();
            setThumbScroller();
            setTabContentHeight();
            setMapOptionHeight();
        }
    });
    if ($('div').hasClass('draggable')) {
        $('.draggable').draggable();
    }

    $('table').trigger('pageSet', 1);

    $('.btn-edit').click(function () {
        $('.sortable li, table tr').removeClass('selected');
        $(this).closest('table').find('tr').removeClass('selected');
        $(this).closest('li, tr').addClass('selected');
    });
    $('tbody tr').click(function () {
        if (!$(this).closest('table').hasClass('tbl-transparent') && !$(this).closest('table').hasClass('table-raw')) {
            $(this).closest('table').find('tr').removeClass('selected');
            $(this).addClass('selected');
            $.tablesorter.window_resize();
        }
    });
    $('.type').change(function () {
        target = $(this).attr('data-target');
        if (target == '#routine') {
            $('#sale-area-treeview').hide();
        } else {
            $('#sale-area-treeview').show();
        }
    });
    $('.btn-map-note').click(function () {
        setMapOptionHeight();
    });
    $('a[data-toggle="tab"]').click(function () {
        $.tablesorter.window_resize();
    });

    /* Language status*/
    $('.log-box>ul>li.language>a').click(function () {
        if ($(this).hasClass('active')) {
            $(this).removeClass('active');
        } else {
            $(this).addClass('active');
        }
    });
    $('.log-box>ul>li.language>a+*').click(function () {
        $(this).removeClass('active');
    });
    /* Navigation status */
    $('.menu-box').on('click', '.btn-navigation', function () {
        if ($('html').hasClass('nav-mini')) {
            $('html').removeClass('nav-mini');
        } else {
            $('html').addClass('nav-mini');
        }
        // Check tablesorter width
        setTimeout(function () {
            $.tablesorter.window_resize();
        }, 700);
        //nav_width = $('')

    });
    $('.navigation ul li a').click(function () {
        is_active = 0;
        if ($(this).hasClass('active')) {
            is_active = 1;
        }
        $('.navigation ul li a').removeClass('active');
        if (!is_active) {
            $(this).addClass('active');
        }
        setTimeout(function () {
            checkNav();
        }, 300);

    });

    $('.content, .header').click(function () {
        if ($('html').hasClass('nav-mini')) {
            $('.navigation a').removeClass('active');
        }
    });
    $('.content').click(function () {
        if ($(window).width() < 1170 && !$('html').hasClass('nav-mini')) {
            $('html').addClass('nav-mini');
        }
    });

    /* Notice toggle */
    $('.notice>a').click(function () {
        if (!$(this).hasClass('active')) {
            $(this).addClass('active');
        } else {
            $(this).removeClass('active');
        }
    });
    $('.notice .btn-close').click(function () {
        $(this).closest('.notice').find('a.active').removeClass('active');
    });

    /* Change theme color */
    $('.circle').click(function () {
        theme = $(this).attr('theme');
        $('body').removeClass('red orange yellow cyan blue magenta');
        $('body').addClass(theme);
        $.cookie('theme', theme, { expires: 7, path: '/' });
    });

    /* Toggle box */
    $("body").on('click', '.toggle-button', function () {
        temp = $(this);
        if ($(this).closest('.toggle-box').hasClass('disable')) {
            $(this).closest('.toggle-box.disable').removeClass('disable');
        } else {
            $(this).closest('.toggle-box').find('.toggle-content').height('auto');
            setTimeout(function () {
                $(temp).closest('.toggle-box').addClass('disable');
            }, 300);
        }
    });

    /* Full screen toggle */

    $('.btn-screenmode').click(function () {
        if ($('body').hasClass('fullscreen')) {
            $('body').removeClass('fullscreen');
            fullscreen = 0;
        } else {
            $('body').addClass('fullscreen');
            fullscreen = 1;
        }
        $.cookie('fullscreen', fullscreen, { expires: 7, path: '/' });
    });


    /* Report master */
    $('.font-family').change(function () {
        value = $('.font-family option:selected').val();
        $('.content-body').attr('font-family', value);
    });
    $('.font-size').change(function () {
        value = $('.font-size option:selected').val();
        $('.content-body').attr('font-size', value);
    });
    $('.align').click(function () {
        align = $(this).attr('align');
        $('.content-body').removeClass('align-left align-center align-right');
        $('.content-body').addClass('align-' + align);
    });

    /* Rotate image */
    $('.clockwise').click(function () {
        if (!$(this).hasClass('block')) {
            $(this).addClass('block');
            setImageRoller($(this));
            $(this).closest('.img-big-box').find('.img-big-insider img').css({ 'transform': 'rotate(' + (img_degree + 90) + 'deg)' });
            $(this).removeClass('block');
        }

    });
    $('.counter-clockwise').click(function () {
        if (!$(this).hasClass('block')) {
            $(this).addClass('block');
            setImageRoller($(this));
            $(this).closest('.img-big-box').find('.img-big-insider img').css({ 'transform': 'rotate(' + (img_degree - 90) + 'deg)' });
            $(this).removeClass('block');
        }
    });

    /* Refresh page */
    $('.btn-refresh').click(function () {
        location.reload();
    });

    /* Check all */
    $('.check-all').click(function () {
        current_value = $(this).prop('checked');
        $(this).closest('.table-box').find('[type="checkbox"]').prop('checked', current_value);
    });

    /* Kendo customize */
    $('body').on('click', '.k-widget.k-dropdown.k-header, .k-edit-field, .k-link', function () {
        $(this).removeClass('k-state-border-up k-state-border-down');
        $(this).find('*').removeClass('k-state-border-up k-state-border-down');
        position = $(this).offset();
        $('.k-animation-container').css({ 'top': position.top + $(this).height() });
    });

});

/* Set image roller */
img_degree = 0;
is_Vertical = [true, true];
function setImageRoller(obj) {
    img_height = $(obj).closest('.img-big-box').find('.img-big-insider img').height();
    img_width = $(obj).closest('.img-big-box').find('.img-big-insider img').width();
    img_degree = $(obj).closest('.img-big-box').find('.img-big-insider img').rotationInfo().deg;
    if (img_degree > -315 && img_degree <= -225) {
        img_degree = -270;
    } else if (img_degree > -225 && img_degree <= -135) {
        img_degree = -180;
    } else if (img_degree > -135 && img_degree <= -45) {
        img_degree = -90;
    } else if (img_degree > -45 && img_degree <= 45) {
        img_degree = 0;
    } else if (img_degree > 45 && img_degree <= 135) {
        img_degree = 90;
    } else if (img_degree > 135 && img_degree <= 225) {
        img_degree = 180;
    } else if (img_degree > 225 && img_degree <= 315) {
        img_degree = 270;
    } else {
        img_degree = 0;
    }
    if ($(obj).closest('.img-big-box').hasClass('img-current-template')) {
        temp = 0;
    } else {
        temp = 1;
    }
    if (is_Vertical[temp] = !is_Vertical[temp]) {
        $(obj).closest('.img-big-box').find('img').css({ 'width': '100%', 'max-height': 'none', 'margin-right': 0 });
    } else {
        $(obj).closest('.img-big-box').find('img').css({ 'width': 'auto', 'max-height': img_width, 'margin-right': -70 });
    }
}

/* Set Height of Tab Content */
function setTabContentHeight() {
    if ($('.tab-content').hasClass('tab-fixed')) {
        content_insider_height = $('.content>.insider').height();
        content_header_height = $('.content-header').height();
        content_footer_height = $('.content-footer').height();
        tab_header_height = $('.tab-header').height();
        tab_content_height = content_insider_height - content_header_height - content_footer_height - tab_header_height - 40;
        if (tab_content_height > 100 && $('.tab-content').width() > 750) {
            $('.tab-content').height(tab_content_height);
        } else {
            $('.tab-content').height('auto');
        }
    }
}

/* Check and set height of table box */
function checkTableFixed() {
    if ($('.table-box').hasClass('wrapper')) {
        content_insider_height = $('.content>.insider').height();
        content_header_height = $('.content-header').height();
        content_body_title_height = 0;
        if ($('div').hasClass('content-body-title')) {
            content_body_title_height = $('.content-body-title').height();
        }
        content_footer_height = 0;
        if ($('div').hasClass('content-footer')) {
            content_footer_height = $('.content-footer').height();
        }
        //console.log(content_body_title_height);
        table_fixed_height = content_insider_height - content_header_height - content_footer_height - content_body_title_height - 20;
        if (table_fixed_height > 200 && $('.content .insider').width() > 750) {
            $('.table-box').css({ 'max-height': table_fixed_height });
        } else {
            $('.table-box').css({ 'max-height': 'none' });
        }
        $.each($('.table-box.wrapper'), function () {
            if ($(this).attr('freeze')) {
                freeze = $(this).attr('freeze');
                $(this).find('>table').tablesorter({
                    headerTemplate: '{content} {icon}', // Add icon for various themes
                    widgets: ['uitheme', 'zebra', 'stickyHeaders', 'stickyFooters', 'scroller'], //'filter', 
                    widgetOptions: {
                        scroller_fixedColumns: freeze,
                        stickyHeaders_attachTo: $(this).closest('.wrapper')
                    }
                });
            } else {
                $(this).find('>table').tablesorter({
                    headerTemplate: '{content} {icon}', // Add icon for various themes
                    widgets: ['uitheme', 'zebra', 'stickyHeaders', 'stickyFooters'], //, 'filter'
                    widgetOptions: {
                        stickyHeaders_attachTo: $(this).closest('.wrapper')
                    }
                });
            }
            if ($(this).parent('div').find('.table-footer').length && !$(this).closest('.calculated').length) {
                footer_height = $(this).parent('div').find('.table-footer').height();
                $(this).closest('.wrapper').css({ 'padding-bottom': footer_height, 'margin-bottom': 0 - footer_height - 2 });
                $(this).closest('.wrapper').addClass('calculated');
                //console.log(max_height);
            }
        });
    }
    if ($('.tab-content').hasClass('wrapper')) {
        freeze = parseInt($('.tab-content').attr('freeze'));
        content_insider_height = $('.content>.insider').height();
        content_header_height = $('.content-header').height();
        tab_header_height = $('.tab-header').height();
        table_fixed_height = content_insider_height - content_header_height - tab_header_height - 20;
        sticky_header_height = $('.tablesorter-scroller>.tablesorter-scroller-header').height();
        $.each($('.tab-content .tablesorter'), function () {
                $('.tab-content .tablesorter').tablesorter({
                    headerTemplate: '{content} {icon}', // Add icon for various themes
                    widgets: ['uitheme', 'zebra', 'stickyHeaders', 'stickyFooters'], //, 'filter'
                    widgetOptions: {
                        stickyHeaders_attachTo: '.wrapper', // or $('.wrapper')
                    }
                }).tablesorterPager({
				    container: $(".ts-pager"),
				    cssGoto: ".pagenum",
				    prev: '.prev',
				    next: '.next',
				    size: 5,
				    removeRows: false,
				    output: ' {startRow} to {endRow} of {totalRows}' //'Showing {startRow} to {endRow} of {totalRows} entries' //'{startRow} - {endRow} / {filteredRows} ({totalRows})'  //'/ {totalPages}'
				});
        });

        //Check height of tablesorter
        $('.tablesorter-scroller>.tablesorter-scroller-table').css({ 'height': table_fixed_height - 90, 'max-height': 'none' });

        if (table_fixed_height > 100 && $('.tab-content').width() > 750) {
            $('.tab-content').height(table_fixed_height);
            scroller_height = table_fixed_height - 70;
        } else {
            $('.tab-content').height('auto');
            scroller_height = null;
        }
    }
    /*
	setTimeout(function() {
		tab_content_width = $('.tab-content').width();
		table_fixed_width = $('.tablesorter-scroller>div>.tablesorter-scroller-table>table').width();
		table_scroll_width = tab_content_width - table_fixed_width;
		//$('.tablesorter-scroller>.tablesorter-scroller-header').width(table_scroll_width);
		//$('.tablesorter-scroller>.tablesorter-scroller-table').width(table_scroll_width);
		//$('.tablesorter-scroller-table').height(scroller_height);
		console.log(tab_content_width);
		console.log(table_fixed_width);
	}, 500);
	*/
}

/* Check fullscreen mode */
function checkScreenMode() {
    if ($.cookie('fullscreen')) {
        $('body').addClass('fullscreen');
    }
}

/* Check navigation height */
navbox_height = $('.nav-box>ul').height();
function checkNav() {
    var submenu_height = $('.navigation').height() - navbox_height - 280;
    $('.navigation ul li a.active+ul').css({ 'max-height': submenu_height });
}

/* Check to set navigation status belong to width of screen */
function checkScreen() {
    var screen_width = $(window).width();
    if (!$('html').hasClass('nav-mini') && screen_width <= 970) {
        $('html').addClass('nav-mini');
    }
    if ($('html').hasClass('nav-mini') && screen_width > 970) {
        //$('html').removeClass('nav-mini');
    }
}

/* Check and set theme by cookie */
function checkTheme() {
    if (theme = $.cookie('theme')) {
        $('body').removeClass('red orange yellow cyan blue magenta');
        $('body').addClass(theme);
    }
}

/* Scheduler resize */
scheduler_height = 500;
function checkScheduleHeight() {
    if ($('.content .insider div').hasClass('scheduler')) {
        var head_height = 50;
        content_insider_height = $('.content .insider').height();
        content_header_height = $('.content-header').height();
        scheduler_height = content_insider_height - content_header_height - head_height;
        //$('.scheduler').height(scheduler_height);
        //$('.k-scheduler-times:nth-child(2)').height(scheduler_height - 150);
        //$('.k-scheduler-content').height(scheduler_height - 100);
        //$('.k-scheduler-table').height(scheduler_height - 50);
        if (scheduler_height < 200) {
            scheduler_height = 500;
        }
    }
}

/* Rotate info */
$.fn.rotationInfo = function () {
    var el = $(this),
        tr = el.css("-webkit-transform") || el.css("-moz-transform") || el.css("-ms-transform") || el.css("-o-transform") || '',
        info = { rad: 0, deg: 0 };
    if (tr = tr.match('matrix\\((.*)\\)')) {
        tr = tr[1].split(',');
        if (typeof tr[0] != 'undefined' && typeof tr[1] != 'undefined') {
            info.rad = Math.atan2(tr[1], tr[0]);
            info.deg = parseFloat((info.rad * 180 / Math.PI).toFixed(1));
        }
    }
    return info;
};

/* Set thumb scroller */
function setThumbScroller() {
    if ($('div').hasClass('img-scroll-box')) {
        $.each($('.img-scroll-box'), function () {
            img_item = $(this).find('img').length;
            img_width = $(this).find('.img-mini-box').width();
            $(this).find('ul').width(img_item * (img_width + 10));
        });
    }
}

/*Set map option height */
function setMapOptionHeight() {
    if ($('.content .insider div').hasClass('map-option')) {
        content_insider_height = $('.content .insider').height();
        block_header_height = $('.block-header').height();
        block_content_header_height = $('.block-content-header').height();
        map_note_height = $('.map-note').height() ? 20 : 50;
        $('.block-content-body').css({ 'max-height': content_insider_height - block_header_height - block_content_header_height - map_note_height - 100 });
    }
}