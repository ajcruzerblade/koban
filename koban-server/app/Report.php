<?php

namespace App;

use Illuminate\Database\Eloquent\Model;

class Report extends Model
{
    protected $guarded = [];
    public $timestamps = false;

    public function location() {
        return $this->belongsTo('App\Location');
    }
}
