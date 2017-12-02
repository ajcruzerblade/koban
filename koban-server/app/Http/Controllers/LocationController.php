<?php

namespace App\Http\Controllers;

use Illuminate\Http\Request;
use App\Location;

class LocationController extends Controller
{
    public function show($id = null) {
        $location = Location::select('id', 'name', 'long', 'lat');

        if ($id) {
            $location = $location->where('id', $id);
        }

        return json_encode(
            [
                'result' => 'ok',
                'data' => $location->with('reports')->get()
            ]
        );
    }
}
