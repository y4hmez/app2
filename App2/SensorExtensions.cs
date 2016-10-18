using System;
using Windows.Devices.Sensors;

namespace App2
{
    public static class SensorExtensions
    {
        public static UnitsNet.Pressure Pressure(this BarometerReading reading)
        {
            return UnitsNet.Pressure.FromHectopascals(reading.StationPressureInHectopascals);
        }

        public static UnitsNet.Length GetHeightFromPressure(this UnitsNet.Pressure pressure)
        {
            UnitsNet.Length height = UnitsNet.Length.FromKilometers(44.3308 - (4.94654 * (Math.Pow(pressure.Pascals, 0.190263))));

            return height;
        }
    }
}