using System;

using iFactr.Core;
using iFactr.UI;

using UIKit;

namespace iFactr.Touch
{
    public class Alert : IAlert
    {
        public event AlertResultEventHandler Dismissed;

        public Link CancelLink { get; set; }

        public Link OKLink { get; set; }

        public string Message { get; private set; }

        public string Title { get; private set; }

        public AlertButtons Buttons { get; private set; }

        public Alert(string message, string title, AlertButtons buttons)
            : base()
        {
            Message = message;
            Title = title;
            Buttons = buttons;
        }

        public void Show()
        {
            var alert = UIAlertController.Create(Title, Message, UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create(TouchFactory.Instance.GetResourceString(Buttons == AlertButtons.YesNo ? "Yes" : "OK"), UIAlertActionStyle.Default, (o) =>
            {
                var handler = Dismissed;
                if (handler != null)
                {
                    handler(this, new AlertResultEventArgs(Buttons == AlertButtons.YesNo ? AlertResult.Yes : AlertResult.OK));
                }
                else
                {
                    iApp.Navigate(OKLink);
                }
            }));

            if (Buttons != AlertButtons.OK)
            {
                alert.AddAction(UIAlertAction.Create(TouchFactory.Instance.GetResourceString(Buttons == AlertButtons.YesNo ? "No" : "Cancel"), UIAlertActionStyle.Cancel, (o) =>
                {
                    var handler = Dismissed;
                    if (handler != null)
                    {
                        handler(this, new AlertResultEventArgs(Buttons == AlertButtons.YesNo ? AlertResult.No : AlertResult.Cancel));
                    }
                    else
                    {
                        iApp.Navigate(CancelLink);
                    }
                }));
            }

            ModalManager.EnqueueModalTransition(ModalManager.GetTopmostViewController(null), alert, true);
        }
    }

    public class AlertLegacy : UIAlertView, IAlert
    {
        public new event AlertResultEventHandler Dismissed;

		public Link CancelLink { get; set; }

		public Link OKLink { get; set; }

        public AlertButtons Buttons
        {
            get { return ButtonCount > 1 ? AlertButtons.OKCancel : AlertButtons.OK; }
        }

        public AlertLegacy(string message, string title, AlertButtons buttons)
            : base(title, message, null, TouchFactory.Instance.GetResourceString("OK"),
                   buttons == AlertButtons.OKCancel ? new[] { TouchFactory.Instance.GetResourceString("Cancel") } : null)
        {
            base.Dismissed += (sender, e) =>
            {
                var handler = Dismissed;
                if (handler != null)
                {
                    handler(this, new AlertResultEventArgs((AlertResult)(int)e.ButtonIndex));
                }
				else if (e.ButtonIndex == (int)AlertResult.OK)
				{
					iApp.Navigate(OKLink);
				}
				else if (e.ButtonIndex == (int)AlertResult.Cancel)
				{
					iApp.Navigate(CancelLink);
				}
            };
        }
    }
}

