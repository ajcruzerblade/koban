<?php

namespace App\Http\Controllers;

use Illuminate\Http\Request;
use App\Location;
use App\Report;

class CrimeController extends Controller
{
    public function uploadCrime(Request $request) {
        $photoName = time().'.'.$request->img->getClientOriginalExtension();

        $request->img->move(public_path('crimes'), $photoName);

        $location = Location::where('name', $request->location)->first();

        if ($location) {
            $location->reports()->create([
                'file_name' => $photoName,
                'report' => $request->report
            ]);
        }
    }
}
