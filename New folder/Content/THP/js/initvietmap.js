//var vietmap = {};
//vietmap.system = "VietBanDo";
//vietmap.useMarkerCluster = true;
//vietmap.iterationRender = 9;
//vietmap.center = new VLatLng(16, 106);
//vietmap.zoom = 6;
//vietmap.disableUIControl = false;
//vietmap.data = null;
//vietmap.periodmarker = 'mtd';
//vietmap.map = null;
//vietmap.markerClusterer = null;
//vietmap.markers = [];
//vietmap.infoWindow = null;
//vietmap.bounds = null;
//vietmap.circle = null;
//vietmap.statisticMarker = null;
//vietmap.colorBy = "Normal"; //Normal, DRS, Segment, Route
//vietmap.showCircle = false;
//vietmap.showLabels= false;
//vietmap.blueMarkers = 0;
//
//    
//vietmap.init = function() {
//    if (VBrowserIsCompatible())
//    {
//       var map = new VMap(document.getElementById('vietmap_canvas'));
//       map.setCenter(vietmap.center, vietmap.zoom);
//       map.addControl(new VLargeMapControl());
//      
//       vietmap.map = map;
//       
//       
//       //nối các marker
//       /*var pt = new VLatLng(10.8488300,	106.6754600);
//       map.setCenter(pt, 4);
//       var arrLatLngs = [
//                                   new VLatLng(10.99999,	106.20000), 
//                                   new VLatLng(10.55555,	106.0000), 
//                                   new VLatLng(9.99000,         106.50000), 
//                                   new VLatLng(10.80000,	106.80000),
//                                   new VLatLng(11.80000,         106.30000)
//                        ];
//       var polygon = new VPolygon(arrLatLngs, 'black', 0.5, '', 0.3);
//       map.addOverlay(polygon);*/
//       
//       
//      
//      
//      
//       
//       
//    }
//};
//
//vietmap.renderMarkers = function(fromIndex)
//{
//    var toIndex = fromIndex + vietmap.iterationRender;
//    if (toIndex > vietmap.data.length)
//        toIndex = vietmap.data.length;
//    
//    for (var i = fromIndex; i < toIndex; i++) {
//        
//        var latLng = new VLatLng(vietmap.data[i].latitude, vietmap.data[i].longitude);
//
//        var imageUrl = base_url + 'assets/img/blue.png';
//        switch (vietmap.periodmarker)
//        {
//            case 'mtd':
//                if (vietmap.data[i].mtd <= 0 )
//                    imageUrl = base_url + 'assets/img/red.png';
//                break;
//            case 'lm':
//                if (vietmap.data[i].lm <= 0 )
//                    imageUrl = base_url + 'assets/img/red.png';
//                break;
//            case 'p3m':
//                if (vietmap.data[i].p3m <= 0 )
//                    imageUrl = base_url + 'assets/img/red.png';
//                break;
//            case 'p6m':
//                if (vietmap.data[i].p6m <= 0 )
//                    imageUrl = base_url + 'assets/img/red.png';
//                break;
//            case 'p12m':
//                if (vietmap.data[i].p12m <= 0 )
//                    imageUrl = base_url + 'assets/img/red.png';
//                break;
//
//        }
//
//        if (imageUrl == base_url + 'assets/img/blue.png')
//            vietmap.blueMarkers = vietmap.blueMarkers + 1;
//        
//        var marker = new VMarker(latLng, new VIcon(imageUrl, new VSize(16, 16)));
//        
//        var imagesCustomerUrl='';
//        if(vietmap.data[i].file_name !='' && vietmap.data[i].file_name !=null)
//        {
//            imagesCustomerUrl='<td><div class="info-image"><img src="' + base_url + '/images/new/' + vietmap.data[i].file_name + '" height="100px" /></div></td>';
//            
//        }
//        else{
//             imagesCustomerUrl='<td><div class="info-image"><img src="' + base_url + '/images/new/no_image.jpg" height="100px" /></div></td>';   
//        }
//        var infoHtml =  '<div class="info">' + 
//                    '<div class="info-body">' + 
//                    '<table width="350px" border="0" cellpadding="0" cellspacing="0">' +
//                    '<tr>' +
//                    imagesCustomerUrl +
//                 
//                    '<td>' +
//                    '<h3>(' + (i+1) + ') ' + vietmap.data[i].name + '</h3>' +
//                    //'<br/><strong>(Code)Site</strong>: (' + vietmap.data[i].branchcode + ')' + vietmap.data[i].site +
//                    '<br/><strong>OutletID</strong>: ' + vietmap.data[i].code + 
//                    '<br/><strong>Outlet Name</strong>: ' + vietmap.data[i].code + 
//                    '<br/><strong>Attn</strong>: ' + vietmap.data[i].code + 
//                    '<br/><strong>Address</strong>: ' + vietmap.data[i].address + 
//                    '<br/><strong>Salesman</strong>: ' + vietmap.data[i].dsr + 
//                    //'<br/><strong>Lat/Lng</strong>: ' + vietmap.data[i].latitude + ' / ' + vietmap.data[i].longitude +
//                    //'<br/><strong>Sales</strong>: ' + vietmap.data[i].mtd +
//                    '<br/>' +
//                    '</td>' +
//                    '</tr></table>' +
//                    
//                    '<br/><strong>Last Order Infor</strong>: ' +
//                    '<br/>&nbsp;&nbsp;<strong>Order Code</strong>: ' +  vietmap.data[i].order_code +
//                    '<br/>&nbsp;&nbsp;<strong>Order Date</strong>: ' +  vietmap.data[i].created_at +
//                    '<br/>&nbsp;&nbsp;<strong>Drop Size</strong>: ' +  vietmap.data[i].drop_size +
//                    '<br/>&nbsp;&nbsp;<strong>SKU</strong>: ' +  vietmap.data[i].total_sku +
//                    '<br/>&nbsp;&nbsp;<strong>Reason</strong>: ' +  vietmap.data[i].reason +
//                    '<br/>&nbsp;&nbsp;<strong>Time in</strong>: ' +  vietmap.data[i].start_time + 
//                    '<br/>&nbsp;&nbsp;<strong>Time out</strong>: ' +  vietmap.data[i].end_time +
//                    '<br/>&nbsp;&nbsp;<strong>Distance</strong>: ' +  vietmap.data[i].distance +
//                   
//                    '<br/>' +
//                    '<br/>' +
//                    '</div>';
//                
//        vietmap.map.addOverlay(marker);
//        VEvent.addListener(marker, 'click', function(obj, latlng) {
//           obj.openInfoWindow(infoHtml);
//       });
//    }
//    
//    if (toIndex >= vietmap.data.length)
//    {
//      $("div.loading[obj=map]").hide();
//      //if (vietmap.showLabels)
//      //    $("div.labels").show();
//      //else
//       //   $("div.labels").hide();
//      //window.setTimeout(vietmap.time, 0);
//      //window.setTimeout("ChangeColor('"+ pgmap.colorBy + "')", 0);
//    }
//    else
//    {
//        var value = (toIndex * 100)/vietmap.data.length;
//        $("div.loading[obj=map] div.progressbar" ).progressbar({value: value});
//        $("div.loading[obj=map] div.status").text(toIndex + " / " + vietmap.data.length);
//        window.setTimeout("vietmap.renderMarkers(" + toIndex + ")", 0);
//    }
//}
//
//vietmap.showMarkers = function() {
//  vietmap.map.clearOverlays()
//     
//  $("div.loading[obj=map]").show();
//  window.setTimeout("vietmap.renderMarkers(0)", 0);
//};
//
//
//
//
//
//
//
