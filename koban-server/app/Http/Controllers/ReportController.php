<?php

namespace App\Http\Controllers;

use Illuminate\Http\Request;
use App\Report;

class ReportController extends Controller
{
    public function show($id = null) {
        $report = Report::with('location');

        if ($id) {
            $report = $report->where('id', $id);
        }

        return json_encode(
            [
                'result' => 'ok',
                'data' => $report->get()
            ]
        );
    }

    public function index() {
        $reports = Report::with('location')->get();

        return view('report-view', ['reports' => $reports]);
    }
}
