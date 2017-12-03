<!doctype html>
<html lang="{{ app()->getLocale() }}">
    <head>
        <meta charset="utf-8">
        <meta http-equiv="X-UA-Compatible" content="IE=edge">
        <meta name="viewport" content="width=device-width, initial-scale=1">
    </head>
    <body>
        <div id="root">
            <pic></pic>
            <description></description>
            {{--  <thumbnail :resource_url="resource_url" @update="updateResource"></thumbnail>  --}}
            <ul>
                {{--  <li v-for="reprt in reports">{{reprt.report}}</li>  --}}
            </ul>
        </div>

        <script src="https://cdn.jsdelivr.net/npm/vue"></script>
        <script src="https://unpkg.com/axios/dist/axios.min.js"></script>
        <script src="https://cdn.jsdelivr.net/vuejs-paginator/2.0.0/vuejs-paginator.min.js"></script>
        {{--  <script async defer src="https://maps.googleapis.com/maps/api/js?key=AIzaSyD297D02wRzx29xsRBg2Q-yCzwE8yPW39s&callback=initMap"></script>  --}}
        <script src="/js/app.js"></script>
        <script src="/js/components/pic.js"></script>
        <script src="/js/components/desc.js"></script>
        {{--  <script src="/js/components/thumb.js"></script>  --}}
        {{--  <script src="/js/map.js"></script>  --}}
    </body>
</html>
