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
        Location::firstOrCreate([
            'name' => 'Area 1',
            'long' => 120.989413,
            'lat' => 14.552131
        ]);


        Location::firstOrCreate([
            'name' => 'Area 2',
            'long' => 120.986451,
            'lat' => 14.551705
        ]);
    }
}
