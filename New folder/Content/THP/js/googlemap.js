var geocoder = null;
var map = null;
var marker = null;
var zoomlevel = 15; 

function map_initialize(){    
    var options = {
        zoom : zoomlevel,
        panControl: false,
        mapTypeControl: false,
        center :new google.maps.LatLng($("#txtLat").val(), $("#txtLng").val()),
        mapTypeId: google.maps.MapTypeId.ROADMAP
    };
    
    map = new google.maps.Map(document.getElementById("map_canvas"), options);
    
    map.enableKeyDragZoom({boxStyle: {border: "4px solid #0046AD",backgroundColor: "transparent",opacity: 1.0}});
    
    google.maps.event.trigger(map, 'resize');
}

$(document).ready(function(){        
    map_initialize();
    
    $('#map_advprint').click(function(){
        $( "#pagesetup").dialog({
            height: 160,modal: true,resizable:false, 
            buttons:[
                {
                    text : 'OK',
                    click: function(){                        
                        $("#pagesetup").dialog('close');
                        /*
                        $("#printpreview").dialog({
                            modal:true,
                            width:$(document).width()-50,
                            height:$(document).height()-50,
                            resizable:false,
                            open: function(){
                                
                            },
                            close: function(){
                                $('#printtemp').remove();
                            }
                        });*/
                        var size = ' width=100% height=100% ', width = 0, height = 0;
						var zoom = map.getZoom();
                        switch ($('#pagesize').val()){
                            case 'A0':
								//zoom = 17;
                                width=4493;height=3178;
                                size = ' width=4493 height=3178 ';
                                break;
                            case 'A1':
								//zoom = 16;
                                width=3178;height=2245;
                                size = ' width=3178 height=2245 ';
                                break;
                            case 'A2':
								//zoom = 15;
                                width=2245;height=1587;
                                size = ' width=2245 height=1587 ';
                                break;
                            case 'A3':
								//zoom = 14;
                                width=1587;height=1122;
                                size = ' width=1587 height=1122 ';
                                break;
                            default:								
                                width=1122;height=793;
                                size = ' width=1122 height=793 ';
                                break;
                        }

                        var center = map.getCenter();
                        var params = '?ms=' + (new Date()).getTime() + '&clat=' + center.lat() + '&clng=' + center.lng() +
                                        '&zoom=' + zoom + '&width=' + width + '&height=' + height;

                        //$("#printpreview").append('<iframe id=printtemp ' + size + ' src="' + $("#printpreview").attr('map') + params + '" ></iframe>');
                        window.open($("#printpreview").attr('map') + params, 'printview');
                    }
                },
                {
                    text: 'Cancel',
                    click: function(){$( "#pagesetup").dialog('close');}
                }
            ]
		});
    });
    
    
});
