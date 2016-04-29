
var placeMarker_double = L.Icon.Label.extend({
    options: {
        iconUrl: '',
        shadowUrl: null,
        iconAnchor: new L.Point(0, 0),
        wrapperAnchor: new L.Point(12, 13),
        labelClassName: 'placeMarks-label'
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
    var marker = L.marker(latlng, {
        icon: new placeMarker_double({
            labelText: ''
            , labelAnchor: new L.Point(labelTextAnchor(''), -1)
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
