<?php

use Illuminate\Database\Seeder;
use App\Location;

class LocationsTableSeeder extends Seeder
{
    /**
     * Run the database seeds.
     *
     * @return void
     */
    public function run()
    {
        Location::create([
            'name' => '',
            'long' => 0,
            'lat' => 0
        ]);


        Location::create([
            'name' => '',
            'long' => 0,
            'lat' => 0
        ]);
    }
}
