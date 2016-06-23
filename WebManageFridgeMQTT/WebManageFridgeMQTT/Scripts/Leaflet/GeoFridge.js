
var placeMarker_double = L.Icon.Label.extend({
    options: {
        iconUrl: '../Content/Icon/markers/i10.png',
        shadowUrl: null,
        //iconSize: new L.Point(24, 24),
        iconAnchor: new L.Point(0, 1),
        labelAnchor: new L.Point(26, 0),
        wrapperAnchor: new L.Point(12, 13),
        popupAnchor: new L.Point(0, -10),
        labelClassName: 'sweet-deal-label'
    }
});

function labelTextAnchor(text) {
    var n = text.toString().length;
    if (n == 1) {
        return 0;
    }
    else if (n == 2) {
        return -3;
    }
    else if (n == 3) {
        return -5;
    }
    else {
        return 0;
    }
}

function RenderMarker(latlng, label, popup, icon, layer1) {
    var iconURL = '../Content/Icon/markers/' + icon + '.png';
    //alert(iconURL);
    //$('#DEV').append(label + ' - ' + iconURL);
    console.log(label + ' - ' + iconURL);
    var marker = L.marker(latlng, {
        icon: new placeMarker_double({
            labelText: label
            , iconUrl: iconURL
        })
    })
    //if (label != '') {
    //    marker.bindLabel(label, { noHide: false })
    //}
    if (popup != '') {
        marker.bindPopup(popup, { noHide: false });
    }

    if (layer1 != null) {
        layer1.addLayer(marker);
    }

    return marker;
}

function RederPopupShop(data)
{
    var html = '<div>';
    html += '<h3>Thông tin Shop</h3>';
    html += '<p><span>Tên Shop: </span></p>';
    html += '<p><span>Địa chỉ: </span></p>';
    html += '<p><span>Quản lý: </span></p>';
    html += '<p><span>Tủ lạnh: </span></p>';
    html += '</div>';
    return html;
}
