using System;
using System.Linq;

using iFactr.Core;
using iFactr.UI;

using Foundation;
using UIKit;

namespace iFactr.Touch
{
    public static class UIViewExtensions
    {
        public static T GetSubview<T>(this UIView view, Func<T, bool> predicate = null)
            where T : UIView
        {
            foreach (var subview in view.Subviews)
            {
                if (subview is T && (predicate == null || predicate.Invoke((T)subview)))
                {
                    return (T)subview;
                }
            }
            
            foreach (var subview in view.Subviews)
            {
                var sub = subview.GetSubview<T>(predicate);
                if (sub != null)
                {
                    return sub;
                }
            }
            
            return null;
        }
        
        public static UIViewController GetSuperview(this UIBarButtonItem item)
        {
			foreach (var stack in iFactr.Core.PaneManager.Instance.OfType<UINavigationController>().Where(s => s.NavigationBar.TopItem != null))
            {
                if ((stack.NavigationBar.TopItem.RightBarButtonItems != null && stack.NavigationBar.TopItem.RightBarButtonItems.Any(b => b == item)) ||
                    (stack.NavigationBar.TopItem.LeftBarButtonItems != null && stack.NavigationBar.TopItem.LeftBarButtonItems.Any(b => b == item)))
                {
                    return stack.TopViewController;
                }
            }

            foreach (var controller in iFactr.Core.PaneManager.Instance.OfType<UINavigationController>().Select(nc => nc.TopViewController))
            {
                if (controller != null && item.GetSuperview(controller.View) != null)
                {
                    return controller;
                }
            }

            return null;
        }

        private static UIView GetSuperview(this UIBarButtonItem item, UIView view)
        {
            var toolbar = view as UIToolbar;
            if (toolbar != null && toolbar.Items.Any(b => b == item))
            {
                return view;
            }

            foreach (var subview in view.Subviews)
            {
                var superview = item.GetSuperview(subview);
                if (superview != null)
                {
                    return superview;
                }
            }

            return null;
        }

        public static T GetSuperview<T>(this UIView view)
            where T : class
        {
            if (view is UINavigationBar && view.Superview != null)
            {
                var navController = view.Superview.GetSuperview<UINavigationController>();
                if (navController != null)
                {
                    return navController.VisibleViewController as T;
                }
            }

            if (view.NextResponder is T)
            {
                return view.NextResponder as T;
            }
            if (view.Superview != null)
            {
                return view.Superview.GetSuperview<T>();
            }
            
            return default(T);
        }
        
        internal static void SignalCellLayout(this UIView view)
        {
            var cell = view.GetSuperview<IGridCell>() as UIView;
            if (cell != null)
            {
                cell.SetNeedsLayout();
            }
            else
            {
                var superview = view.GetSuperview<IGridView>() as UIViewController;
                if (superview != null && superview.View != null)
                {
                    superview.View.SetNeedsLayout();
                }
            }
        }

        public static void ConfigureBackButton(this UIViewController controller, Link backLink, Pane outputPane)
        {
            var backButton = new UIBarButtonItem(string.IsNullOrEmpty(controller.NavigationItem.Title) ?
                TouchFactory.Instance.GetResourceString("Back") : controller.NavigationItem.Title, UIBarButtonItemStyle.Plain, null);

            controller.NavigationItem.BackBarButtonItem = backButton;
            controller.NavigationItem.LeftBarButtonItem = null;

            var stack = controller.NavigationController as IHistoryStack ?? PaneManager.Instance.FromNavContext(outputPane, PaneManager.Instance.CurrentTab);
            if (stack != null && !stack.CanGoBack())
            {
                controller.NavigationItem.SetHidesBackButton(true, false);
            }
            else if (backLink == null && controller.NavigationItem.HidesBackButton)
            {
                controller.NavigationItem.HidesBackButton = false;
            }

            if (backLink != null)
            {
                var weak = new WeakReference(controller);
                backButton = controller.CreateButton(backLink);
                backButton.Clicked += delegate(object sender, EventArgs e)
                {
                    var historyStack = (weak.Target as UIViewController)?.NavigationController as IHistoryStack;
                    if (historyStack != null)
                    {
                        historyStack.HandleBackLink(backLink, outputPane);
                    }
                };

                controller.NavigationItem.SetHidesBackButton(true, false);
                controller.NavigationItem.SetLeftBarButtonItem(backButton, false);
            }

            var popover = controller.NavigationController as PopoverNavigationController;
            if (popover != null)
            {
                popover.SetCloseButton(controller);
            }
        }

        public static void SetMenu(this UIViewController controller, IMenu menu)
        {
            if (menu != null && menu.ButtonCount > 0)
            {
                if (menu.ButtonCount > 1)
                {
                    bool isNew = false;
                    if (menu.ImagePath != null)
                    {
                        try
                        {
                            controller.NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIImage.FromBundle(menu.ImagePath), UIBarButtonItemStyle.Plain, null);
                            isNew = true;
                        }
                        catch
                        {
                        }
                    }
                    else if (!string.IsNullOrEmpty(menu.Title))
                    {
                        controller.NavigationItem.RightBarButtonItem = new UIBarButtonItem(menu.Title, UIBarButtonItemStyle.Plain, null);
                        isNew = true;
                    }

                    if (controller.NavigationItem.RightBarButtonItem == null)
                    {
                        controller.NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Action);
                        isNew = true;
                    }

                    if (isNew)
                    {
                        controller.NavigationItem.RightBarButtonItem.Clicked += (o, e) =>
                        {
                            var alert = (menu as UIAlertController) ?? (menu.Pair as UIAlertController);
                            if (alert != null)
                            {
                                if (alert.PopoverPresentationController != null)
                                {
                                    alert.PopoverPresentationController.BarButtonItem = controller.NavigationItem.RightBarButtonItem;
                                }
                                ModalManager.EnqueueModalTransition(ModalManager.GetTopmostViewController(null), alert, true);
                            }
                            else
                            {
                                var action = (menu as UIActionSheet) ?? (menu.Pair as UIActionSheet);
                                if (action != null)
                                {
                                    action.ShowFrom(controller.NavigationItem.RightBarButtonItem, true);
                                }
                            }
                        };
                    }

                    var sheet = (menu as UIActionSheet) ?? (menu.Pair as UIActionSheet);
                    if (sheet != null)
                    {
                        if (sheet.CancelButtonIndex <= 0)
                        {
                            sheet.Add(TouchFactory.Instance.GetResourceString("Cancel"));
                            sheet.CancelButtonIndex = sheet.ButtonCount - 1;
                        }
                    }
                }
                else
                {
                    controller.NavigationItem.RightBarButtonItem = TouchFactory.GetNativeObject<UIBarButtonItem>(menu.GetButton(0), "menuButton");
                }
            }
            else
            {
                controller.NavigationItem.RightBarButtonItem = null;
            }
        }
        
        public static void ClearDetailPane(this UIViewController controller)
        {
            if (TouchFactory.Instance.SplitViewController != null && !ModalManager.TransitionInProgress &&
                controller.NavigationController == PaneManager.Instance.FromNavContext(Pane.Master, PaneManager.Instance.CurrentTab) &&
                !TouchFactory.Instance.SplitViewController.AdjustingMaster)
            {
                PaneManager.Instance.FromNavContext(Pane.Detail, 0).PopToRoot();
            }
        }

        public static UIBarButtonItem CreateButton(this UIViewController controller, Link button)
        {
            UIBarButtonItem actionButton = null;
            if (button.ImagePath != null)
            {
                try
                {
                    actionButton = new UIBarButtonItem(UIImage.FromBundle(button.ImagePath), UIBarButtonItemStyle.Plain, null);
                }
                catch
                {
                    iApp.Log.Error(string.Format("Error loading button image '{0}'", button.ImagePath));
                }
            }
            if (actionButton == null)
            {
                actionButton = new UIBarButtonItem(button.Text ?? string.Empty, UIBarButtonItemStyle.Plain, null);
            }

            return actionButton;
        }
    }
}