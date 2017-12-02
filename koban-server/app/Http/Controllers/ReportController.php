<?php

namespace App\Http\Controllers;

use Illuminate\Http\Request;
use App\Report;

class ReportController extends Controller
{
    public function show($id) {
        return json_encode(
            [
                'result' => 'ok',
                'data' => Report::where('id', $id)->first()
            ]
        );
    }
}
