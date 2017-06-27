using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using iFactr.Core;
using iFactr.Core.Layers;
using iFactr.UI;

using Foundation;
using UIKit;
using MonoCross.Navigation;

namespace iFactr.Touch
{
    class ViewController : UIViewController
    {
        public virtual bool Autorotate { get; set; }

        public ViewController()
        {
            Autorotate = true;
        }

        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations() 
        {
            var orientations = UIApplication.SharedApplication.SupportedInterfaceOrientationsForWindow(TouchFactory.KeyWindow); 
            if (!Autorotate && (orientations & UIInterfaceOrientationMask.Portrait) == 0) 
            { 
                throw new NotSupportedException("This view only allows Portrait orientation, but Portrait is not one of the supported orientations."); 
            } 

            return Autorotate ? orientations : UIInterfaceOrientationMask.Portrait; 
        }
    }

    abstract class BaseNavigationController : UINavigationController, IHistoryStack
    {
		public string ID { get; private set; }
		
        public virtual IEnumerable<IMXView> Views
        {
            get
            {
                foreach (var view in ViewControllers.OfType<IMXView>())
                {
                    var pair = view as IPairable;
                    yield return pair == null ? view : (pair.Pair as IMXView) ?? view;
                }
            }
        }

        public IEnumerable<iLayer> History
        {
			get { return ViewControllers.OfType<IMXView>().Where(v => v != TopViewController).Select(v => v.GetModel() as iLayer); }
        }
        
        public iLayer CurrentLayer
        {
            get { return CurrentView == null ? null : CurrentView.GetModel() as iLayer; }
        }
        
        public virtual IMXView CurrentView
        {
			get
            {
                var pair = TopViewController as IPairable;
                return pair == null ? TopViewController as IMXView : (pair.Pair as IMXView) ?? pair as IMXView;
            }
        }
        
		public Pane Pane
		{
			get
			{
				int tab;
				if (int.TryParse(ID, out tab))
				{
					return Pane.Master;
				}
				
				Pane pane;
				return Enum.TryParse(ID, out pane) ? pane : Pane.Master;
			}
		}
        
        private bool regularPop;

		public BaseNavigationController(string id)
        {
			ID = id;
            Delegate = new NavigationDelegate();

            if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
            {
                View.BackgroundColor = UIColor.White;
            }
        }

        // this crazy voodoo is the bane of my existance
        [Export("navigationBar:shouldPopItem:")]
        public bool ShouldPopItem(UINavigationBar navigationBar, UINavigationItem item)
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                if (regularPop)
                {
                    regularPop = false;
                    return true;
                }

                if (VisibleViewController == null || ViewControllers.Length < 2)
                    return true;

                var view = ViewControllers[ViewControllers.Length - 2] as IMXView;
                if (view != null)
                {
                    var link = new Link(PaneManager.Instance.GetNavigatedURI(view), new Dictionary<string, string>());
                    if (!PaneManager.Instance.ShouldNavigate(link, Pane, NavigationType.Back))
                    {
                        return false;
                    }
                }

                base.PopViewController(true);
                return true;
            }

            if (!regularPop)
            {
				if (VisibleViewController == null || ViewControllers.Length < 2)
                    return true;
                
				var view = ViewControllers[ViewControllers.Length - 2] as IMXView;
				if (view != null)
				{
                    var link = new Link(PaneManager.Instance.GetNavigatedURI(view), new Dictionary<string, string>());
	                if (!PaneManager.Instance.ShouldNavigate(link, Pane, NavigationType.Back))
	                {
	                    return false;
	                }
                }
                regularPop = true;
                base.PopViewController(true);
                return false;
            }
            
            regularPop = false;
            return true;
        }
        
        public override UIViewController[] PopToRootViewController(bool animated)
        {
			UIViewController[] vcs = null;
			if (VisibleViewController != ViewControllers.FirstOrDefault())
			{
				regularPop = TopViewController != ViewControllers.FirstOrDefault();
				vcs = base.PopToRootViewController(animated);
			}
			
			return vcs;
        }
        
        public override UIViewController[] PopToViewController(UIViewController viewController, bool animated)
        {
			if (VisibleViewController != viewController)
			{
				regularPop = TopViewController != viewController;
				if (ViewControllers.Contains(viewController))
					return base.PopToViewController(viewController, animated);
				else
					return PopToRootViewController(animated);
			}
			return null;
        }
        
        public override UIViewController PopViewController(bool animated)
        {
            regularPop = TopViewController != null;
            return base.PopViewController(animated);
        }
        
        public override bool ShouldAutorotate()
		{
			return VisibleViewController == null ? true : VisibleViewController.ShouldAutorotate();
		}
        
        public override UIInterfaceOrientation PreferredInterfaceOrientationForPresentation()
		{
			return VisibleViewController == null ? base.PreferredInterfaceOrientationForPresentation() :
				VisibleViewController.PreferredInterfaceOrientationForPresentation();
		}
        
        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
		{
			return VisibleViewController == null || VisibleViewController is UIAlertController ?
                UIInterfaceOrientationMask.All : VisibleViewController.GetSupportedInterfaceOrientations();
		}
        
        public void PopToLayer(iLayer layer)
        {
			var display = ViewControllers.OfType<IMXView>().First(vc => layer.Equals(vc.GetModel()));
            PopToViewController(display as UIViewController, true);
        }
        
        public iLayer Peek()
        {
            return History.LastOrDefault();
        }
        
        public void PushCurrent() { }
        
        public virtual void Clear(iLayer layer)
        {
            PopToRoot();
        }

        public void InsertView(int index, IMXView view)
        {
            Parameter.CheckIndex(ViewControllers, "history", index);

            var vcs = ViewControllers;
            vcs[index] = TouchFactory.GetNativeObject<UIViewController>(view, "view");
            SetViewControllers(vcs, false);
        }

        public virtual IMXView[] PopToRoot()
        {
            var controllers = PopToRootViewController(false);
            return controllers == null ? null : controllers.OfType<IMXView>().Select(v => { var p = v as IPairable; return p == null ? v : (p.Pair as IMXView) ?? v; }).ToArray();
        }

        public IMXView[] PopToView(IMXView view)
        {
            var controller = TouchFactory.GetNativeObject<UIViewController>(view, "view");

            // 8 is not accurately reflecting what controllers are in the stack after a replace.
            // this means the check will fail even though it should pass.
            // forego the check for now until we find a way to fix it.  works fine on 7.
            if (!UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                Parameter.CheckObjectExists(ViewControllers, "history", controller, "view");
            }

            var controllers = PopToViewController(controller, true);
            return controllers == null ? null : controllers.OfType<IMXView>().Select(v => { var p = v as IPairable; return p == null ? v : (p.Pair as IMXView) ?? v; }).ToArray();
        }

        public virtual IMXView PopView()
        {
            var view = PopViewController(true);
            var pair = view as IPairable;
            if (pair == null)
            {
                return view as IMXView;
            }
            return (pair.Pair as IMXView) ?? pair as IMXView;
        }

        public virtual void PushView(IMXView view)
        {
            PushViewController(TouchFactory.GetNativeObject<UIViewController>(view, "view"), ViewControllers.Any(vc => !(vc is VanityView)));
        }

        public virtual void ReplaceView(IMXView currentView, IMXView newView)
        {
            var controller = TouchFactory.GetNativeObject<UIViewController>(currentView, "currentView");
            var index = Parameter.CheckObjectExists(ViewControllers, "history", controller, "currentView");

            var vcs = ViewControllers;
            vcs[index] = TouchFactory.GetNativeObject<UIViewController>(newView, "newView");
            SetViewControllers(vcs, false);
        }

        private class NavigationDelegate : UINavigationControllerDelegate
        {
            public override void WillShowViewController (UINavigationController navigationController, UIViewController viewController, bool animated)
            {
                if (TouchFactory.Instance.SplitViewController == null)
                {
                    if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
                    {
                        var color = navigationController.NavigationBar.BarTintColor;
                        navigationController.NavigationBar.BarStyle = color == null || color.Brightness() > 0.35f ? UIBarStyle.Default : UIBarStyle.Black;
                    }
                    else
                    {
                        var color = navigationController.NavigationBar.TintColor;
                        navigationController.NavigationBar.BarStyle = color.Brightness() > 0.35f ? UIBarStyle.Default : UIBarStyle.Black;
                    }
                }
                else
                {
                    TouchFactory.Instance.SplitViewController.UpdateMasterButton();

                    if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
                    {
                        TouchFactory.Instance.SplitViewController.SetNeedsStatusBarAppearanceUpdate();
                    }
                }
            }

            public override void DidShowViewController(UINavigationController navigationController, UIViewController viewController, bool animated)
            {
                if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
                {
                    ((BaseNavigationController)navigationController).regularPop = false;
                };
            }
        }
    }

	class NavigationController : BaseNavigationController
	{
		public NavigationController(string id) : base(id) { }
		
		public override void DidRotate (UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate (fromInterfaceOrientation);
			
			// we don't want to fire the event here if there's a splitviewcontroller
			// because it would cause the event to be fired once for each active controller
			if (TouchFactory.Instance.SplitViewController == null)
			{
				switch (InterfaceOrientation)
				{
				case UIInterfaceOrientation.Portrait:
					TouchFactory.NotifyOrientationChanged(iApp.Orientation.Portrait);
					break;
				case UIInterfaceOrientation.PortraitUpsideDown:
					TouchFactory.NotifyOrientationChanged(iApp.Orientation.PortraitUpsideDown);
					break;
				case UIInterfaceOrientation.LandscapeLeft:
					TouchFactory.NotifyOrientationChanged(iApp.Orientation.LandscapeLeft);
					break;
				case UIInterfaceOrientation.LandscapeRight:
					TouchFactory.NotifyOrientationChanged(iApp.Orientation.LandscapeRight);
					break;
				}
			}
		}
	}
	
	/// <summary>
	/// A UINavigationController for popovers that supports landscape orientation.
	/// </summary>
	class PopoverNavigationController : BaseNavigationController
	{
        public UIBarButtonItem CloseButton
        {
            get
            {
                return closeButton ?? (closeButton = new UIBarButtonItem(TouchFactory.Instance.GetResourceString("Close"), UIBarButtonItemStyle.Done,
                delegate
                {
                    ModalManager.EnqueueModalTransition(this.PresentingViewController, null, true);
                }));
            }
        }
        private UIBarButtonItem closeButton;

        public override bool DisablesAutomaticKeyboardDismissal
        {
            get { return false; }
        }

        public override IEnumerable<IMXView> Views
        {
            get { return isActive ? base.Views : new[] { ViewControllers.FirstOrDefault() as IMXView }; }
        }

        public override IMXView CurrentView
        {
            get { return isActive ? base.CurrentView : ViewControllers.FirstOrDefault() as IMXView; }
        }

        private bool isActive;

		public PopoverNavigationController() : base("Popover")
        {
        }

		public PopoverNavigationController(UIViewController root) : this()
		{
			if (root != null)
			{
				PushViewController(root, false);
			}
		}

        public override void ViewWillAppear(bool animated)
        {
            isActive = true;
            base.ViewWillAppear(animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            isActive = false;
            base.ViewWillDisappear(animated);
        }

		public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            if (PresentingViewController == null)
            {
                PopToRootViewController(false);
            }
		}
        
        public override IMXView[] PopToRoot()
		{
            var views = Views.Skip(1).ToArray();
			if (PresentingViewController != null)
			{
                ModalManager.EnqueueModalTransition(PresentingViewController, null, true);
            }
            else
            {
                views = base.PopToRoot();
            }
            
            return views;
		}
		
		public override IMXView PopView()
		{
			if (ViewControllers.Count(vc => !(vc is VanityView)) < 2 && PresentingViewController != null)
			{
                var view = base.PopViewController(false);
				ModalManager.EnqueueModalTransition(PresentingViewController, null, true);
                
                var pair = view as IPairable;
                if (pair == null)
                {
                    return view as IMXView;
                }
                return (pair.Pair as IMXView) ?? pair as IMXView;
			}
            else
            {
                return base.PopView();
            }
		}
        
        public override void PushView(IMXView view)
		{
			base.PushView(view);

			if (this.PresentingViewController == null)
			{
                var entry = view as IHistoryEntry;
                if (entry != null)
                {
                    ModalPresentationStyle = entry.PopoverPresentationStyle == PopoverPresentationStyle.FullScreen ?
                        UIModalPresentationStyle.FullScreen : UIModalPresentationStyle.FormSheet;
                }

				ModalManager.EnqueueModalTransition(TouchFactory.Instance.TopViewController, this, true);
			}
		}

        public void SetCloseButton(UIViewController controller)
        {
            controller.NavigationItem.LeftItemsSupplementBackButton = !controller.NavigationItem.HidesBackButton;
            
            List<UIBarButtonItem> buttons = new List<UIBarButtonItem>()
            {
                CloseButton
            };
            
            if (controller.NavigationItem.LeftBarButtonItem != null &&
                controller.NavigationItem.LeftBarButtonItem.Style != UIBarButtonItemStyle.Done)
            {
                buttons.Insert(0, controller.NavigationItem.LeftBarButtonItem);
            }
            
            controller.NavigationItem.SetLeftBarButtonItems(buttons.ToArray(), false);
        }
	}

    //class StackedViewController : PSStackedViewController, INavigationController, IHistoryStack
    //{
    //    public IEnumerable<IMXView> History
    //    {
    //        get { return ViewControllers.OfType<IMXView>(); }
    //    }

    //    public iLayer CurrentLayer
    //    {
    //        get 
    //        { 
    //            var view = CurrentView as IMXView<iLayer>;
    //            return view == null ? null : view.Model;
    //        }
    //    }

    //    public IMXView CurrentView
    //    {
    //        get { return ViewControllers.LastOrDefault() as IMXView; }
    //    }

    //    public StackedViewController()
    //        : base(new UITableViewController(UITableViewStyle.Plain))
    //    {
    //        var root = RootViewController as UITableViewController;
    //        root.TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;

    //        Delegate = new StackedDelegate();
    //    }

    //    [Obsolete]
    //    public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
    //    {
    //        return TouchFactory.Instance.IsSupportedOrientation(toInterfaceOrientation);
    //    }

    //    public override void PushViewController(UIViewController controller, bool animated)
    //    {
    //        controller.View.Frame = new CGRect(0, 0, TouchFactory.Instance.LargeFormFactor ? 420 : 280, controller.View.Frame.Height);
    //        base.PushViewController(controller, animated);
    //    }

    //    public void DisplayLayer(iLayer layer, IMXView newPanel)
    //    {
    //        if (newPanel == null)
    //        {
    //            UIViewController controller = (UIViewController)layer.ViewController();
    //            PushViewController(controller, true);
    //        }
    //        else
    //        {
    //            PopToViewController(newPanel as UIViewController, true);
    //        }
    //    }

    //    public void PopToLayer(iLayer layer)
    //    {
    //        var display = ViewControllers.OfType<IMXView<iLayer>>().First(vc => vc.Model == layer);
    //        PopToViewController(display as UIViewController, true);
    //    }

    //    public IMXView Peek()
    //    {
    //        return History.LastOrDefault();
    //    }

    //    public void PushCurrent() { }

    //    public void Clear(iLayer layer)
    //    {
    //        PopToRootViewController(false);
    //    }

    //    private class StackedDelegate : PSStackedViewDelegate
    //    {
    //        public override void DidPanViewController(PSStackedViewController stackedView, UIViewController viewController, int offset)
    //        {
    //            if (stackedView.VisibleViewControllers.FirstOrDefault(v => v == viewController) == null)
    //            {
    //                stackedView.PopViewController(viewController, false);
    //            }
    //        }
    //    }
    //}
}