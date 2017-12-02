<?php

namespace App;

use Illuminate\Database\Eloquent\Model;

class Location extends Model
{
    protected $guarded = [];
    public $timestamps = false;

    public function reports() {
        return $this->hasMany('App\Report');
    }
}
