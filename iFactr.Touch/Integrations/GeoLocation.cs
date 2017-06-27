using System;
using CoreLocation;
using UIKit;
using iFactr.Integrations;

namespace iFactr.Touch
{
    public class GeoLocation : CLLocationManager, IGeoLocation
    {
        public event EventHandler<GeoLocationEventArgs> LocationUpdated;
        
        public bool IsActive { get; private set; }

        public void Start()
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                RequestAlwaysAuthorization();
            }

            Delegate = new LocationManagerDelegate();
            StartUpdatingLocation();
            IsActive = true;
        }

        public void Stop()
        {
            Delegate = null;
            StopUpdatingLocation();
            IsActive = false;
        }

        private void OnUpdatedLocation(CLLocation newLocation)
        {
            var handler = LocationUpdated;
            if (handler != null)
            {
                handler(this, new GeoLocationEventArgs(new GeoLocationData(newLocation.Coordinate.Latitude, newLocation.Coordinate.Longitude)));
            }
        }

        private class LocationManagerDelegate : CLLocationManagerDelegate
        {
            public override void UpdatedLocation (CLLocationManager manager, CLLocation newLocation, CLLocation oldLocation)
            {
                ((GeoLocation)manager).OnUpdatedLocation(newLocation);
            }
        }
    }
}

