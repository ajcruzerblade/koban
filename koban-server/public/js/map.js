function initMap() {
    var uluru = {
        lat: 14.552131,
        lng: 120.989413
    };
    var map = new google.maps.Map(document.getElementById('map'), {
        zoom: 17,
        center: uluru
    });

    geocodeLatLng(map, '14.552131,120.989413');
}


function geocodeLatLng(map, latlng) {

    var geocoder = new google.maps.Geocoder;
    var infowindow = new google.maps.InfoWindow;


    var input = latlng;
    var latlngStr = input.split(',', 2);
    var latlng = {
        lat: parseFloat(latlngStr[0]),
        lng: parseFloat(latlngStr[1])
    };

    geocoder.geocode({
        'location': latlng
    }, function(results, status) {
        if (status === 'OK') {
            if (results[0]) {
                map.setZoom(15);
                var marker = new google.maps.Marker({
                    position: latlng,
                    map: map,
                    center: latlng
                });
                infowindow.setContent(results[0].formatted_address);
                infowindow.open(map, marker);
            } else {
                window.alert('No results found');
            }
        } else {
            window.alert('Geocoder failed due to: ' + status);
        }
    });
}