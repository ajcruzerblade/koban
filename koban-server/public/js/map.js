

$(document).ready(function(){

  $.ajax({
    url: "http://localhost:8000/api/location",
    success: function(result){

       var data = JSON.parse(result);

       if(data['data'][0]['reports'].length > 0) {

         var latlong = `${data['data'][0]['lat']},${data['data'][0]['long']}`;
         initMap(latlong);
       }

   }});
});


function initMap(latlong='14,12') {
  var uluru = {lat: 14.0, lng: 120.0};

  var map = new google.maps.Map(document.getElementById('map'), {
    zoom: 17,
    center: uluru
  });

  geocodeLatLng(map, latlong);
}


function geocodeLatLng(map, latlong) {

  var geocoder = new google.maps.Geocoder;
  var infowindow = new google.maps.InfoWindow;

        console.log(latlong);
        var poop = String(latlong);
        var latlngStr = poop.split(',', 2);
        var latlng = {lat: parseFloat(latlngStr[0]), lng: parseFloat(latlngStr[1])};
        console.log(latlng);
        geocoder.geocode({'location': latlng}, function(results, status) {
          if (status === 'OK') {
            if (results[0]) {
              map.setZoom(15);
              var marker = new google.maps.Marker({
                position: latlng,
                map: map,
                center:latlng
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
