using System;
using System.Drawing;
using System.IO;
using System.Linq;

using Foundation;
using UIKit;

using iFactr.Core;
using MonoCross.Utilities;
using iFactr.UI;

namespace iFactr.Touch
{
    public class Printer
    {
        public static void Print(string url)
        {
            if (!UIPrintInteractionController.PrintingAvailable)
            {
                new UIAlertView(TouchFactory.Instance.GetResourceString("PrintErrorTitle"),
	                TouchFactory.Instance.GetResourceString("PrintError"), null,
	                TouchFactory.Instance.GetResourceString("Dismiss"), null).Show();

                return;
            }

            string printUrl = null;
            int index = url.IndexOf('?');
            if (index >= 0)
            {
                HttpUtility.ParseQueryString(url.Substring(index)).TryGetValue("url", out printUrl);
            }

            UIPrintInfo printInfo = UIPrintInfo.PrintInfo;
            printInfo.OutputType = UIPrintInfoOutputType.General;
            printInfo.Duplex = UIPrintInfoDuplex.LongEdge;

            if (!string.IsNullOrEmpty(printUrl) && !PrintUrl(printUrl))
            {
				new UIAlertView(TouchFactory.Instance.GetResourceString("PrintErrorTitle"),
	                string.Format(TouchFactory.Instance.GetResourceString("PrintUrlError"), printUrl), null,
	                TouchFactory.Instance.GetResourceString("Dismiss"), null).Show();

                return;
            }

            var navContext = iApp.CurrentNavContext.ActiveLayer.NavContext;
            var stack = PaneManager.Instance.FromNavContext(navContext.NavigatedActivePane, navContext.NavigatedActiveTab) as BaseNavigationController;
            if (stack == null || stack.CurrentView == null)
            {
                return;
            }

            UIViewController controller = TouchFactory.GetNativeObject<UIViewController>(stack.CurrentView, "view");
            UIView view = controller.View;
            if (controller is IBrowserView)
            {
                view = view.Subviews.FirstOrDefault(sv => sv is UIWebView);
            }

            if (!(view is UIWebView) || !PrintUrl(((UIWebView)view).Request.Url.AbsoluteString))
            {
                printInfo.JobName = controller.NavigationItem.Title;
                UIPrintInteractionController.SharedPrintController.PrintInfo = printInfo;
                
                view.ViewPrintFormatter.StartPage = 0;
                UIPrintInteractionController.SharedPrintController.PrintFormatter = view.ViewPrintFormatter;
            }
            
            UIPrintInteractionCompletionHandler completionHandler = FinishPrinting;
            if (TouchFactory.Instance.LargeFormFactor)
            {
				UIViewController top = ModalManager.GetTopmostViewController(null);

				nfloat barHeight = TouchFactory.Instance.IsLandscape ? UIApplication.SharedApplication.StatusBarFrame.Width
					: UIApplication.SharedApplication.StatusBarFrame.Height;

				nfloat centerX = TouchFactory.Instance.IsLandscape ? top.View.Center.Y : top.View.Center.X;

                UIPrintInteractionController.SharedPrintController.PresentFromRectInView(
                    new CoreGraphics.CGRect(centerX, stack.NavigationBar.Frame.Height + barHeight, 1, 1), top.View, true, completionHandler);
            }
            else
            {
                UIPrintInteractionController.SharedPrintController.Present(true, completionHandler);
            }
        }

        private static bool PrintUrl(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                NSUrl item = new NSUrl(url);
                if (UIPrintInteractionController.CanPrint(item))
                {
                    UIPrintInfo printInfo = UIPrintInfo.PrintInfo;
                    printInfo.JobName = url.Substring(url.LastIndexOf(Path.DirectorySeparatorChar) + 1);
                    
                    UIPrintInteractionController.SharedPrintController.PrintInfo = printInfo;
                    UIPrintInteractionController.SharedPrintController.PrintingItem = item;

                    return true;
                }
            }

            return false;
        }
        
        private static void FinishPrinting(UIPrintInteractionController controller, bool completed, NSError error)
        {
            if (completed && error == null)
            {
				new UIAlertView(TouchFactory.Instance.GetResourceString("PrintingTitle"), 
	                TouchFactory.Instance.GetResourceString("Printing"), null, 
	                TouchFactory.Instance.GetResourceString("Dismiss"), null).Show();
            }
            else if (error != null)
            {
				new UIAlertView(TouchFactory.Instance.GetResourceString("Error"), error.LocalizedDescription,
					null, TouchFactory.Instance.GetResourceString("Dismiss"), null).Show();
            }
        }
    }
}

