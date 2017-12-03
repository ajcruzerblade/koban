
<html>
  <head>
    <link href="{!! asset('css/map.css') !!}" media="all" rel="stylesheet" type="text/css" />
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.2.1/jquery.min.js"></script>
  </head>
  <body>
    <h3>Koban</h3>
  </br>
    <div id="group">
      <div id="map" class="column"></div>
      <div id="val"></div>
  </div>
    <script type="text/javascript" src="{!! asset('js/map.js') !!}"></script>
    <script async defer
      src="https://maps.googleapis.com/maps/api/js?key=AIzaSyD297D02wRzx29xsRBg2Q-yCzwE8yPW39s&callback=initMap">
    </script>


  </body>
</html>
