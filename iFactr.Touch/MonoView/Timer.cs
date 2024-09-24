using System;

using iFactr.UI;

namespace iFactr.Touch
{
	public class Timer : System.Timers.Timer, iFactr.UI.ITimer
	{
		public new event EventHandler Elapsed;

		public bool IsEnabled
		{
			get { return base.Enabled; }
			set { base.Enabled = value; }
		}

		public Timer()
		{
			base.Elapsed += (sender, e) =>
			{
				lock (sender)
                {
                    if (base.Enabled)
                    {
						base.Enabled = false;
        				var handler = Elapsed;
        				if (handler != null)
        				{
        					handler(this, EventArgs.Empty);
        				}
                    }
                }
			};
		}
	}
}

