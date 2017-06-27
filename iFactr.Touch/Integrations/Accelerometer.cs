using System;
using System.Collections.Generic;

using CoreMotion;
using Foundation;

using iFactr.Integrations;

namespace iFactr.Touch
{
    public class Accelerometer : CMMotionManager, IAccelerometer
    {
        public event EventHandler<AccelerometerEventArgs> ValuesUpdated;

        public bool IsActive
        {
            get { return AccelerometerActive; }
        }

        public Accelerometer()
        {
            AccelerometerUpdateInterval = 1 / 60;
        }

        public void Start()
        {
            StartAccelerometerUpdates(NSOperationQueue.CurrentQueue, async delegate(CMAccelerometerData data, NSError error)
            {
                if (AccelerometerActive)
                {
                    var handler = ValuesUpdated;
                    if (handler != null)
                    {
                        var accel = data.Acceleration;
                        handler(this, new AccelerometerEventArgs(new AccelerometerData(accel.X, accel.Y, accel.Z)));
                    }
                }
            });
        }

        public void Stop()
        {
            StopAccelerometerUpdates();
        }
    }
}

