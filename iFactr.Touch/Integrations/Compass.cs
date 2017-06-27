using System;
using CoreLocation;
using UIKit;
using iFactr.Integrations;

namespace iFactr.Touch
{
    public class Compass : CLLocationManager, ICompass
    {
        public event EventHandler<HeadingEventArgs> HeadingUpdated;
        
        public bool IsActive { get; private set; }

        public void Start()
        {
            Delegate = new LocationManagerDelegate();
            StartUpdatingHeading();
            IsActive = true;
        }

        public void Stop()
        {
            Delegate = null;
            StopUpdatingHeading();
            IsActive = false;
        }

        private void OnUpdatedHeading(CLHeading newHeading)
        {
            var handler = HeadingUpdated;
            if (handler != null)
            {
                handler(this, new HeadingEventArgs(new HeadingData(newHeading.TrueHeading, newHeading.MagneticHeading)));
            }
        }

        private class LocationManagerDelegate : CLLocationManagerDelegate
        {
            public override void UpdatedHeading(CLLocationManager manager, CLHeading newHeading)
            {
                ((Compass)manager).OnUpdatedHeading(newHeading);
            }
        }
    }
}

