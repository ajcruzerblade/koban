<html>
    <head>
        <link rel="stylesheet" href="css/report-view.css" type="text/css">
        <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css" integrity="sha384-BVYiiSIFeK1dGmJRAkycuHAHRg32OmUcww7on3RYdg4Va+PmSTsz/K68vbdEjh4u" crossorigin="anonymous">
    </head>    
    <body>
        <div>
            <div class="container col-md-8">
                <div class="col-md-6">

                </div>
                <div class="col-md-6">
                    <table class="table table-striped">
                        @foreach ($reports as $report)
                            <tr>
                                <td class="col-md-8">
                                    <img src="{{URL::to('/')}}/crimes/{{$report['file_name']}}" class="img-responsive"/>
                                </td>
                                <td class="col-md-4">
                                    <p class="text-center">{{$report['report']}}</p>
                                </td>
                            </tr>
                        @endforeach
                    </table>
                </div>
            </div>
        </div>
    </body>
</html