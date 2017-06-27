using System;
using System.Drawing;

using CoreGraphics;
using Foundation;
using UIKit;

namespace iFactr.Touch
{
	public class LoadSpinner : IDisposable
	{
		UIActivityIndicatorView activity;
        UIAlertView alertView;
		UIView view;
		UILabel label;
	
		public string Title
		{
			get { return alertView == null ? label.Text : alertView.Title; }
			set
			{
				if (alertView == null)
				{
					label.Text = value ?? string.Empty;
				}
				else
				{
					alertView.Title = value ?? string.Empty;
				}
			}
		}

		public LoadSpinner(string title, string message)
		{
			activity = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge);;

			if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
			{
				activity.Color = UIColor.Black;

				label = new UILabel()
				{
					Lines = 1,
					LineBreakMode = UILineBreakMode.TailTruncation,
					TextAlignment = UITextAlignment.Center,
					TextColor = UIColor.Black
				};

				view = new LoadView()
				{
					BackgroundColor = new UIColor(1, 1, 1, 0.85f),
					Transform = TouchFactory.KeyWindow.RootViewController.View.Transform
				};

				view.Layer.CornerRadius = 10;
				view.Layer.BorderColor = new CoreGraphics.CGColor(0.875f, 0.875f, 0.875f);
				view.Layer.BorderWidth = 1;
				view.Add(label);
				view.Add(activity);

                // flexible margins don't work on iOS 7 and transforms never change on iOS 8, so we have to deal with each separately
                if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
                {
                    view.AutoresizingMask = UIViewAutoresizing.FlexibleMargins;
                }
                else
                {
				    TouchFactory.KeyWindow.RootViewController.View.AddObserver(view, new NSString("transform"), NSKeyValueObservingOptions.New, System.IntPtr.Zero);
                }
			}
			else
			{
				activity.Center = new CGPoint(140, 70);
				alertView = new UIAlertView(new RectangleF(0, 0, 300, 200));
				alertView.Add(activity);
			}

			Title = title;
		}
	
		public LoadSpinner(string title) : this(title, null) {}
	
		public void StartAnimating()
		{
			if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
			{
                var frame = new NSString(label.Text).GetBoundingRect(new CGSize(240, 54),
                        NSStringDrawingOptions.UsesLineFragmentOrigin | NSStringDrawingOptions.TruncatesLastVisibleLine,
                        new UIStringAttributes() { Font = label.Font }, new NSStringDrawingContext());

				frame = new CGRect(CGPoint.Empty, frame.Size);
				frame.Height = 54;

				label.Frame = new CGRect(activity.Frame.Width + 14, frame.Y, frame.Width, frame.Height);
				activity.Center = new CGPoint(activity.Frame.Width / 2 + 10, frame.Height / 2);

				frame.Width += activity.Frame.Width + (frame.Width == 0 ? 20 : 34);
				view.Frame = TouchFactory.Instance.IsLandscape && !UIDevice.CurrentDevice.CheckSystemVersion(8, 0) ? new CGRect(frame.Y, frame.X, frame.Height, frame.Width) : frame;
				view.Center = TouchFactory.KeyWindow.RootViewController.View.Center;
                view.Transform = TouchFactory.KeyWindow.RootViewController.View.Transform;

				TouchFactory.KeyWindow.Add(view);
				TouchFactory.KeyWindow.BringSubviewToFront(view);
			}
			else
			{
				alertView.Show();
			}
            activity.StartAnimating();
		}

		public void StopAnimating()
		{
			if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
			{
				view.RemoveFromSuperview();
			}
			else
			{
				alertView.DismissWithClickedButtonIndex(-1, false);
			}
            activity.StopAnimating();
        }

        public void Dispose()
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0) && !UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                TouchFactory.KeyWindow.RootViewController.View.RemoveObserver(view, new NSString("transform"));
            }
        }

		private class LoadView : UIView
		{
			public override void ObserveValue(NSString keyPath, NSObject ofObject, NSDictionary change, System.IntPtr context)
			{
				if (keyPath.ToString() == "transform")
				{
					this.Transform = ((NSValue)change.ObjectForKey(NSObject.ChangeNewKey)).CGAffineTransformValue;
				}
			}
		}
    }

    internal class InteractionBlockerView : UIView
    {
        public InteractionBlockerView()
        {
            UserInteractionEnabled = true;
            Frame = UIScreen.MainScreen.ApplicationFrame;
        }
    }
}
