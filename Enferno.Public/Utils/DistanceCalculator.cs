using System;

namespace Enferno.Public.Utils
{
    public struct GeoPosition
    {
        public double? Longitude { get; set; }
        public double? Latitude { get; set; }

        public GeoPosition(double? longitude, double? latitude)
            : this()
        {
            this.Longitude = longitude;
            this.Latitude = latitude;
        }

        public bool IsValid
        {
            get
            {
                return Longitude.HasValue && Latitude.HasValue;
            }
        }

        public static GeoPosition operator -(GeoPosition p1, GeoPosition p2)
        {
            return new GeoPosition(AbsDiff(p1.Longitude, p2.Longitude), AbsDiff(p1.Latitude, p2.Latitude));
        }

        private static double AbsDiff(double? d1, double? d2)
        {
            return Math.Abs(d1.Value - d2.Value);
        }
    }

    public static class DistanceCalculator
    {
        public const double EarthRadiusInMiles = 3956.0;
        public const double EarthRadiusInKilometers = 6367.0;
        public static double ToRadian(double val) { return val * (Math.PI / 180); }
        public static double DiffRadian(double val1, double val2) { return ToRadian(val2) - ToRadian(val1); }

        public static decimal? GetKilometersBetween(GeoPosition from, GeoPosition to)
        {
            if (to.IsValid && from.IsValid)
            {
                return (decimal)CalcDistance(from.Latitude.Value, from.Longitude.Value, to.Latitude.Value, to.Longitude.Value);
            }
            return null;
        }

        private static double CalcDistance(double lat1, double lng1, double lat2, double lng2)
        {
            const double radius = EarthRadiusInKilometers;
            return radius * 2 * Math.Asin(Math.Min(1, Math.Sqrt((Math.Pow(Math.Sin((DiffRadian(lat1, lat2)) / 2.0), 2.0) + Math.Cos(ToRadian(lat1)) * Math.Cos(ToRadian(lat2)) * Math.Pow(Math.Sin((DiffRadian(lng1, lng2)) / 2.0), 2.0)))));
        }
    }
}
