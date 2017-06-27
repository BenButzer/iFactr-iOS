using System;
using System.Collections.Generic;

using CoreLocation;
using UIKit;

using iFactr.Core;
using iFactr.Core.Utilities;

namespace iFactr.Touch
{
    public class Geolocation
    {
        private static CLLocationManager locator;
        private static string callback;

        static Geolocation()
        {
            locator = new CLLocationManager();
        }

        public static void GetValues (string url)
        {
            var parameters = HttpUtility.ParseQueryString(url.Substring(url.IndexOf('?')));
            if (parameters == null || !parameters.ContainsKey("callback"))
            {
                throw new ArgumentException("Geolocation requires a callback URI.");
            }
            else
            {
                callback = parameters["callback"];
            }

            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                locator.RequestAlwaysAuthorization();
            }

            locator.Delegate = new LocationManagerDelegate();
            locator.StartUpdatingLocation();
        }

        private class LocationManagerDelegate : CLLocationManagerDelegate
        {
            public LocationManagerDelegate()
            {
            }


            public override void UpdatedLocation (CLLocationManager manager, CLLocation newLocation, CLLocation oldLocation)
            {
                manager.Delegate = null;
                manager.StopUpdatingLocation();

                var parameters = new Dictionary<string, string>()
                {
                    { "Lat", newLocation.Coordinate.Latitude.ToString() },
                    { "Lon", newLocation.Coordinate.Longitude.ToString() }
                };

                iApp.Navigate(callback, parameters);
            }
        }
    }
}

