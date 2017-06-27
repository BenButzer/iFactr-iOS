using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using iFactr.Core;
using iFactr.Core.Layers;
using iFactr.Core.Targets;

using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace iFactr.Touch
{
//    public class SplitView : UISplitViewController
//    {
//		public SplitView ()
//		{
//		}
//		
//		public override void ViewDidLoad ()
//		{
//			UIViewController master = this.ViewControllers[0];
//			UIViewController detail = this.ViewControllers[1];
//			
//			master.ViewDidLoad();
//			detail.ViewDidLoad();
//		}		
//		
//		public override void ViewWillAppear (bool animated)
//		{
//			UIViewController master = this.ViewControllers[0];
//			UIViewController detail = this.ViewControllers[1];
//			
//			master.ViewWillAppear(animated);
//			detail.ViewWillAppear(animated);
//			
//			LayoutViewControllers(InterfaceOrientation);
//		}		
//		
//		public override void ViewDidAppear (bool animated)
//		{
//			UIViewController master = this.ViewControllers[0];
//			UIViewController detail = this.ViewControllers[1];
//			
//			master.ViewDidAppear(animated);
//			detail.ViewDidAppear(animated);
//		}
//		
//		public void LayoutViewControllers(UIInterfaceOrientation orientation)
//		{
//			UIViewController master = this.ViewControllers[0];
//			UIViewController detail = this.ViewControllers[1];
//			
//			if(orientation == UIInterfaceOrientation.Portrait ||
//				orientation == UIInterfaceOrientation.PortraitUpsideDown) 
//			{
//				master.View.Frame = new System.Drawing.CGRect(0, 0, 320, 1004);
//				detail.View.Frame = new System.Drawing.CGRect(321, 0, 448, 1004);
//			}
//			else {
//				master.View.Frame = new System.Drawing.CGRect(0, 0, 320, 748);
//				detail.View.Frame = new System.Drawing.CGRect(321, 0, 704, 748);
//			}
//		}
//		
//		public override void WillAnimateRotation (UIInterfaceOrientation toInterfaceOrientation, double duration)
//		{
//			//base.WillAnimateRotation(UIInterfaceOrientation.LandscapeRight, duration);
//			LayoutViewControllers(toInterfaceOrientation);
//						
//			if (toInterfaceOrientation == UIInterfaceOrientation.LandscapeLeft ||
//			    toInterfaceOrientation == UIInterfaceOrientation.LandscapeRight)
//				base.WillAnimateRotation(toInterfaceOrientation, duration);
//		}
//		
//		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
//		{
//			 return true;
//		}
//	}
	
	#region MGSplitView	
	internal enum MGSplitViewDividerStyle
	{
		Thin,
		PaneSplitter
	}
	
    // TODO: Investigate the feasability of removing MGSplitViewController in favor of the vanilla UISplitViewController.
    // The primary reason it was used was to support split views in portrait orientation, but the vanilla controller
    // appears to support that now.  Real-time resizing of the panes is valuable for debugging but otherwise unnecessary.
    // It's likely that removal will fix some of the old bugs that still linger in the MG code.
	class MGSplitViewController : ViewController
	{
		internal nfloat DefaultThinWidth { get { return 1.0f; } }
		internal nfloat DefaultThickWidth { get { return 12.0f; } }
		
		internal nfloat SplitPosition { get; set; }
		internal nfloat SplitWidth { get; set; }
		internal bool Vertical { get; set; }
		internal bool MasterBeforeDetail { get; set; }
		internal bool AdjustingMaster { get; set; }
		internal MGSplitDividerView DividerView { get; set; }
		internal MGSplitViewDividerStyle DividerStyle { get; set; }
		
		nfloat defaultSplitPosition = 320.0f;
		nfloat defaultCornerRadius = 5.0f;
		UIColor defaultCornerColor = UIDevice.CurrentDevice.CheckSystemVersion(7, 0) ?
			new UIColor(142f / 255f, 142f / 255f, 147f / 255f, 1) : UIColor.Black;
		
		nfloat panesplitterCornerRadius = 0.0f;
		
		nfloat minViewWidth	= 200.0f;
		
		string animationChangeSplitOrientation = "ChangeSplitOrientation";	// Animation ID for internal use.
		string animationChangeSubviewsOrder = "ChangeSubviewsOrder";	// Animation ID for internal use.
		
		UIViewController[] viewControllers;
		UIPopoverController hiddenPopoverController;
		
		bool showsMasterInPortrait = true;
		bool showsMasterInLandscape = true;
		bool reconfigurePopup = false;
		UIBarButtonItem barButtonItem;
		MGSplitCornersView[] cornerViews;
		
		MGSplitViewAppDelegate splitViewDelegate = new MGSplitViewAppDelegate();
		
		string NameOfInterfaceOrientation(UIInterfaceOrientation orientation)
		{
			string orientationName = null;
			switch (orientation) {
				case UIInterfaceOrientation.Portrait:
					orientationName = "Portrait"; // Home button at bottom
					break;
				case UIInterfaceOrientation.PortraitUpsideDown:
					orientationName = "Portrait (Upside Down)"; // Home button at top
					break;
				case UIInterfaceOrientation.LandscapeLeft:
					orientationName = "Landscape (Left)"; // Home button on left
					break;
				case UIInterfaceOrientation.LandscapeRight:
					orientationName = "Landscape (Right)"; // Home button on right
					break;
				default:
					break;
			}
		
			return orientationName;
		}
	
		bool IsLandscape(UIInterfaceOrientation orientation)
		{
			return orientation == UIInterfaceOrientation.LandscapeLeft ||
				orientation == UIInterfaceOrientation.LandscapeRight;
		}
		
		
		bool ShouldShowMaster(UIInterfaceOrientation orientation)
		{
			// Returns true if master view should be shown directly embedded in the splitview, instead of hidden in a popover.
			return (orientation == UIInterfaceOrientation.LandscapeLeft ||
				orientation == UIInterfaceOrientation.LandscapeRight) ?
				showsMasterInLandscape : showsMasterInPortrait;
		}
		
		
		bool ShouldShowMaster()
		{
			return ShouldShowMaster(this.InterfaceOrientation);
		}
		
		
		public bool IsShowingMaster()
		{
			UIViewController masterViewController = viewControllers[0];
			return (ShouldShowMaster() && masterViewController != null &&
			        masterViewController.View != null && masterViewController.View.Superview == View);
		}

        public IEnumerable<UIViewController> GetControllers()
        {
            return viewControllers == null ? null : viewControllers.AsEnumerable();
        }
		
		public MGSplitViewController()
		{
			Setup();
		}
		
		void Setup()
		{
			// Configure default behaviour.
			viewControllers = new UIViewController[2];
//			CGRect divRect = View.Bounds;
//			if (Vertical) {
//				divRect.Y = SplitPosition;
//				divRect.Height = SplitWidth;
//			} else {
//				divRect.X = SplitPosition;
//				divRect.Width = SplitWidth;
//			}
			DividerView = new MGSplitDividerView(this);
			DividerView.BackgroundColor = defaultCornerColor;
			SplitPosition = defaultSplitPosition;
			Vertical = true;
			showsMasterInPortrait = (TouchFactory.Instance.HideMasterPaneInOrientation & MasterOrientation.Portrait) == 0;
			showsMasterInLandscape = (TouchFactory.Instance.HideMasterPaneInOrientation & MasterOrientation.Landscape) == 0;
            MasterBeforeDetail = true;
		}

        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            var detailViewController = viewControllers[1] as UINavigationController;
            if (detailViewController != null)
            {
                if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
                {
                    var color = detailViewController.NavigationBar.BarTintColor;
                    return color == null || color.Brightness() > 0.35f ? UIStatusBarStyle.Default : UIStatusBarStyle.LightContent;
                }
                else
                {
                    var color = detailViewController.NavigationBar.TintColor;
                    return color.Brightness() > 0.35f ? UIStatusBarStyle.Default : UIStatusBarStyle.LightContent;
                }
            }

            return UIStatusBarStyle.Default;
        }
		
		public override void WillRotate(UIInterfaceOrientation toInterfaceOrientation, double duration)
		{
            if (!ShouldShowMaster())
    			AdjustingMaster = true;
			
			UIViewController masterViewController = viewControllers[0];
			UIViewController detailViewController = viewControllers[1];
			
			masterViewController.WillRotate(toInterfaceOrientation, duration);
			detailViewController.WillRotate(toInterfaceOrientation, duration);
		}
		
		
		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
		{
			UIViewController masterViewController = viewControllers[0];
			UIViewController detailViewController = viewControllers[1];
			
			masterViewController.DidRotate(fromInterfaceOrientation);
			detailViewController.DidRotate(fromInterfaceOrientation);
			
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
		
		
		public override void WillAnimateRotation(UIInterfaceOrientation toInterfaceOrientation, double duration)
		{
			UIViewController masterViewController = viewControllers[0];
			UIViewController detailViewController = viewControllers[1];
			
			masterViewController.WillAnimateRotation(toInterfaceOrientation, duration);
			detailViewController.WillAnimateRotation(toInterfaceOrientation, duration);
		
			// Hide popover.
			if (hiddenPopoverController != null && hiddenPopoverController.PopoverVisible) {
				hiddenPopoverController.Dismiss(false);
			}
		
			// Re-tile views.
			reconfigurePopup = true;
			LayoutSubviews(toInterfaceOrientation, true);
		}
		
		
		public override void WillAnimateFirstHalfOfRotation(UIInterfaceOrientation toInterfaceOrientation, double duration)
		{
			UIViewController masterViewController = viewControllers[0];
			UIViewController detailViewController = viewControllers[1];
			
			masterViewController.WillAnimateFirstHalfOfRotation(toInterfaceOrientation, duration);
			detailViewController.WillAnimateFirstHalfOfRotation(toInterfaceOrientation, duration);
		}
		
		
		public override void DidAnimateFirstHalfOfRotation(UIInterfaceOrientation toInterfaceOrientation)
		{
			UIViewController masterViewController = viewControllers[0];
			UIViewController detailViewController = viewControllers[1];
			
			masterViewController.DidAnimateFirstHalfOfRotation(toInterfaceOrientation);
			detailViewController.DidAnimateFirstHalfOfRotation(toInterfaceOrientation);
		}
		
		
		public override void WillAnimateSecondHalfOfRotation(UIInterfaceOrientation fromInterfaceOrientation, double duration)
		{
			UIViewController masterViewController = viewControllers[0];
			UIViewController detailViewController = viewControllers[1];
			
			masterViewController.WillAnimateSecondHalfOfRotation(fromInterfaceOrientation, duration);
			detailViewController.WillAnimateSecondHalfOfRotation(fromInterfaceOrientation, duration);
		}
		
		CGSize SplitViewSizeForOrientation(UIInterfaceOrientation theOrientation)
		{
			UIScreen screen = UIScreen.MainScreen;
			CGRect fullScreenRect = screen.Bounds; // always implicitly in Portrait orientation.
		
			// Find status bar height by checking which dimension of the applicationFrame is narrower than screen bounds.
			// Little bit ugly looking, but it'll still work even if they change the status bar height in future.
			//float statusBarHeight = MAX((fullScreenRect.size.width - appFrame.size.width), (fullScreenRect.size.height - appFrame.size.height));
		
			// Initially assume portrait orientation.
			nfloat width = fullScreenRect.Size.Width;
			nfloat height = fullScreenRect.Size.Height;
		
			// Correct for orientation.
			if (!UIDevice.CurrentDevice.CheckSystemVersion(8, 0) && IsLandscape(theOrientation)) {
				width = height;
				height = fullScreenRect.Size.Width;
			}

			if (!UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
			{
				// Account for status bar, which always subtracts from the height (since it's always at the top of the screen).
				height -= 20;
			}

			return new CGSize(width, height);
		}
		
		void LayoutSubviews(UIInterfaceOrientation theOrientation, bool animate)
		{
			// this is mostly for video playback.  if a video is playing, we need
			// to make sure that it remains on the top when a rotation occurs
			UIView topView = null;
			if (View.Subviews.Count() > 0)
				topView = View.Subviews.Last();
			
			if (reconfigurePopup) {
				ReconfigureForMasterInPopover(!ShouldShowMaster(theOrientation));
			}
			
			// Layout the master, detail and divider views appropriately, adding/removing subviews as needed.
			// First obtain relevant geometry.
			var fullSize = SplitViewSizeForOrientation(theOrientation);
			nfloat width = fullSize.Width;
			nfloat height = fullSize.Height;
			
			// Layout the master, divider and detail views.
			CGRect newFrame = new CGRect(0, 0, width, height);
			UIViewController controller;
			UIView view;
			bool shouldShowMaster = ShouldShowMaster(theOrientation);
			bool masterFirst = MasterBeforeDetail;
			CGRect masterRect, dividerRect, detailRect;
			if (Vertical) {				
				if (masterFirst) {
					if (!ShouldShowMaster()) {
						// Move off-screen.
						newFrame.X -= (SplitPosition + SplitWidth);
					}
		
					newFrame.Width = SplitPosition;
					masterRect = newFrame;
		
					newFrame.X += newFrame.Width;
					newFrame.Width = SplitWidth;
					dividerRect = newFrame;
		
					newFrame.X += newFrame.Width;
					newFrame.Width = width - newFrame.X;
					detailRect = newFrame;
		
				} else {
					if (!ShouldShowMaster()) {
						// Move off-screen.
						newFrame.Width += (SplitPosition + SplitWidth);
					}
		
					newFrame.Width -= (SplitPosition + SplitWidth);
					detailRect = newFrame;
		
					newFrame.X += newFrame.Width;
					newFrame.Width = SplitWidth;
					dividerRect = newFrame;
		
					newFrame.X += newFrame.Width;
					newFrame.Width = SplitPosition;
					masterRect = newFrame;
				}
		
				// Position master.
				controller = viewControllers[0];
				view = controller.View;
				if (view != null) {
					view.Frame = masterRect;
					if (view.Superview == null) {
						View.AddSubview(view);
					}
				}
				ResetBackButton(controller);
		
				// Position divider.
				view = DividerView;
				view.Frame = dividerRect;
				if (view.Superview == null) {
					View.AddSubview(view);
				}
		
				// Position detail.
				controller = viewControllers[1];
				view = controller.View;
				if (view != null) {
					view.Frame = detailRect;
					if (view.Superview == null) {
						View.InsertSubviewAbove(view, viewControllers[0].View);
					} else {
						View.BringSubviewToFront(view);
					}
				}
		
			} else {
				if (masterFirst) {
					if (!ShouldShowMaster()) {
						// Move off-screen.
						newFrame.Y -= (SplitPosition + SplitWidth);
					}
		
					newFrame.Height = SplitPosition;
					masterRect = newFrame;
		
					newFrame.Y += newFrame.Height;
					newFrame.Height = SplitWidth;
					dividerRect = newFrame;
		
					newFrame.Y += newFrame.Height;
					newFrame.Height = height - newFrame.Y;
					detailRect = newFrame;
		
				} else {
					if (!ShouldShowMaster()) {
						// Move off-screen.
						newFrame.Height += (SplitPosition + SplitWidth);
					}
		
					newFrame.Height -= (SplitPosition + SplitWidth);
					detailRect = newFrame;
		
					newFrame.Y += newFrame.Height;
					newFrame.Height = SplitWidth;
					dividerRect = newFrame;
		
					newFrame.Y += newFrame.Height;
					newFrame.Height = SplitPosition;
					masterRect = newFrame;
				}
		
				// Position master.
				controller = viewControllers[0];
				view = controller.View;
				if (view != null) {
					view.Frame = masterRect;
					if (view.Superview == null) {
						View.AddSubview(view);
					}
				}
		
				// Position divider.
				view = DividerView;
				view.Frame = dividerRect;
				if (view.Superview == null) {
					View.AddSubview(view);
				}
		
				// Position detail.
				controller = viewControllers[1];
				view = controller.View;
				if (view == null) {
					view.Frame = detailRect;
					if (view.Superview == null) {
						View.InsertSubviewAbove(view, viewControllers[0].View);
					} else {
						View.BringSubviewToFront(view);
					}
				}
			}

            AdjustingMaster = false;
			
			// Create corner views if necessary.
			MGSplitCornersView leadingCorners; // top/left of screen in Vertical/horizontal split.
			MGSplitCornersView trailingCorners; // bottom/right of screen in Vertical/horizontal split.
			if (cornerViews == null) {
				
				leadingCorners = new MGSplitCornersView();
//				leadingCorners.splitViewController = self;
				leadingCorners.CornerBackgroundColor = defaultCornerColor;
				leadingCorners.CornerRadius = defaultCornerRadius;
				trailingCorners = new MGSplitCornersView();
//				trailingCorners.splitViewController = self;
				trailingCorners.CornerBackgroundColor = defaultCornerColor;
				trailingCorners.CornerRadius = defaultCornerRadius;
				cornerViews = new MGSplitCornersView[2]{leadingCorners, trailingCorners};
		
			} else {
				leadingCorners = cornerViews[0];
				trailingCorners = cornerViews[1];
			}
		
			// Configure and layout the corner-views.
			leadingCorners.CornersPosition = (Vertical) ? MGCornersPosition.LeadingVertical : MGCornersPosition.LeadingHorizontal;
			trailingCorners.CornersPosition = (Vertical) ? MGCornersPosition.TrailingVertical : MGCornersPosition.TrailingHorizontal;
			leadingCorners.AutoresizingMask = (Vertical) ? UIViewAutoresizing.FlexibleBottomMargin : UIViewAutoresizing.FlexibleRightMargin;
			trailingCorners.AutoresizingMask = (Vertical) ? UIViewAutoresizing.FlexibleTopMargin : UIViewAutoresizing.FlexibleLeftMargin;
		
			nfloat x, y, cornersWidth, cornersHeight;
			CGRect leadingRect, trailingRect;
			nfloat radius = leadingCorners.CornerRadius;
			if (Vertical) { // left/right split
				cornersWidth = (radius * 2.0f) + SplitWidth;
				cornersHeight = radius;
				x = ((shouldShowMaster) ? ((masterFirst) ? SplitPosition : width - (SplitPosition + SplitWidth)) : (0 - SplitWidth)) - radius;
				y = 0;
				leadingRect = new CGRect(x, y, cornersWidth, cornersHeight); // top corners
				trailingRect = new CGRect(x, (height - cornersHeight), cornersWidth, cornersHeight); // bottom corners
		
			} else { // top/bottom split
				x = 0;
				y = ((shouldShowMaster) ? ((masterFirst) ? SplitPosition : height - (SplitPosition + SplitWidth)) : (0 - SplitWidth)) - radius;
				cornersWidth = radius;
				cornersHeight = (radius * 2.0f) + SplitWidth;
				leadingRect = new CGRect(x, y, cornersWidth, cornersHeight); // left corners
				trailingRect = new CGRect((width - cornersWidth), y, cornersWidth, cornersHeight); // right corners
			}
		
			leadingCorners.Frame = leadingRect;
			trailingCorners.Frame = trailingRect;
		
			// Ensure corners are visible and frontmost.
			if (leadingCorners.Superview == null) {
				View.InsertSubviewAbove(leadingCorners, viewControllers[1].View);
				View.InsertSubviewAbove(trailingCorners, viewControllers[1].View);
			} else {
				View.BringSubviewToFront(leadingCorners);
				View.BringSubviewToFront(trailingCorners);
			}
			
			if (topView != null)
			{
				topView.Superview.BringSubviewToFront(topView);
			}
//			if (IsLandscape())
//				View.Frame = new CGRect(0, 20, View.Frame.Width, View.Frame.Height);
//			else
//				View.Frame = new CGRect(0, 20, View.Frame.Width, View.Frame.Height);
		}
		
		void LayoutSubviews(bool animate)
		{
			LayoutSubviews(this.InterfaceOrientation, animate);
		}
		
		
		internal void LayoutSubviews()
		{
			LayoutSubviews(this.InterfaceOrientation, true);
		}
		
		
		public override void ViewWillAppear (bool animated)
		{
            AdjustingMaster = true;

			base.ViewWillAppear (animated);
			
			UIViewController masterViewController = viewControllers[0];
			UIViewController detailViewController = viewControllers[1];
		
			if (IsShowingMaster()) {
				masterViewController.ViewWillAppear(animated);
			}
			detailViewController.ViewWillAppear(animated);
		
			reconfigurePopup = true;
		}
		
		
		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			
			UIViewController masterViewController = viewControllers[0];
			UIViewController detailViewController = viewControllers[1];
		
			if (IsShowingMaster()) {
				masterViewController.ViewDidAppear(animated);
			}
			detailViewController.ViewDidAppear(animated);
            LayoutSubviews();
		}
		
		
		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);
		
			UIViewController masterViewController = viewControllers[0];
			UIViewController detailViewController = viewControllers[1];
			
			if (IsShowingMaster()) {
				masterViewController.ViewWillDisappear(animated);
			}
			detailViewController.ViewWillDisappear(animated);

            HidePopover();
		}
		
		
		public override void ViewDidDisappear (bool animated)
		{
			base.ViewDidDisappear (animated);
		
			UIViewController masterViewController = viewControllers[0];
			UIViewController detailViewController = viewControllers[1];
			
			if (IsShowingMaster()) {
				masterViewController.ViewDidDisappear(animated);
			}
			detailViewController.ViewDidDisappear(animated);
		}
		
		internal void UpdateMasterButton()
		{
			UINavigationController detailViewController = viewControllers[1] as UINavigationController;
			
			if (!ShouldShowMaster())
			{
				detailViewController.TopViewController.NavigationItem.SetLeftBarButtonItem(barButtonItem, false);
                detailViewController.TopViewController.NavigationItem.LeftItemsSupplementBackButton = true;
			}
		}
		
		void ReconfigureForMasterInPopover(bool inPopover)
		{
            if (inPopover)
			    AdjustingMaster = true;
			
            UIViewController masterViewController = viewControllers[0];
			UINavigationController detailViewController = viewControllers[1] as UINavigationController;
			reconfigurePopup = false;
		
			if ((inPopover && hiddenPopoverController != null) || (!inPopover && hiddenPopoverController == null) 
			    || masterViewController == null) {
				// Nothing to do.
				return;
			}
		
			if (inPopover && hiddenPopoverController == null && barButtonItem == null)
			{
				// Create and configure popover for our masterViewController.
				masterViewController.ViewWillDisappear(false);
				hiddenPopoverController = new UIPopoverController(masterViewController);
				hiddenPopoverController.DidDismiss += delegate
				{
					var frame = hiddenPopoverController.ContentViewController.View.Frame;
					frame.Height = hiddenPopoverController.PopoverContentSize.Height;
					hiddenPopoverController.ContentViewController.View.Frame = frame;
					AdjustingMaster = true;
				};
				masterViewController.ViewDidDisappear(false);

				barButtonItem = new UIBarButtonItem(TouchStyle.ImageFromResource("menuIcon.png"), UIBarButtonItemStyle.Plain, delegate { ShowMasterPopover (); });
				detailViewController.VisibleViewController.NavigationItem.SetLeftBarButtonItem(barButtonItem, true);
                detailViewController.VisibleViewController.NavigationItem.LeftItemsSupplementBackButton = true;
				// Inform delegate of this state of affairs.
//				if (splitViewDelegate && [splitViewDelegate respondsToSelector:@selector(splitViewController:willHideViewController:withBarButtonItem:forPopoverController:)]) {
//					[(NSObject <MGSplitViewControllerDelegate> *)splitViewDelegate splitViewController:self 
//																		willHideViewController:self.masterViewController 
//																			 withBarButtonItem:_barButtonItem 
//																		  forPopoverController:_hiddenPopoverController];
//				}
//				ShowMasterPopover();
			}
			else if (!inPopover && hiddenPopoverController != null && barButtonItem != null)
			{
				// I know this looks strange, but it fixes a bizarre issue with UIPopoverController leaving masterViewController's views in disarray.
				hiddenPopoverController.PresentFromRect(CGRect.Empty, View, UIPopoverArrowDirection.Any, false);
		
				// Remove master from popover and destroy popover, if it exists.
				hiddenPopoverController.Dismiss(false);
				hiddenPopoverController = null;
		
				// Inform delegate that the _barButtonItem will become invalid.
//				if (splitViewDelegate && [splitViewDelegate respondsToSeCGPoint:@selector(splitViewController:willShowViewController:invalidatingBarButtonItem:)]) {
//					[(NSObject <MGSplitViewControllerDelegate> *)splitViewDelegate splitViewController:self 
//																		willShowViewController:self.masterViewCCGPointler 
//																	 invalidatingBarButtonItem:_barButtonItem];
//				}
		
				// Destroy _barButtonItem.
				detailViewController.VisibleViewController.NavigationItem.SetLeftBarButtonItem(null, true);
				barButtonItem = null;
		
				// Move master view.
				UIView masterView = masterViewController.View;
				if (masterView != null && masterView.Superview != View) {
					masterView.RemoveFromSuperview();
				}
			}
		}
		
		void AnimationDidStop(string animationID, int finished)
		{
			if ((animationID == animationChangeSplitOrientation || 
				 animationID == animationChangeSubviewsOrder) && cornerViews != null) {
				foreach (UIView corner in cornerViews) {
					corner.Hidden = false;
				}
				DividerView.Hidden = false;
			}
		}
		

		internal void ToggleSplitOrientation()
		{
			bool showingMaster = this.IsShowingMaster();
			if (showingMaster) {
				if (cornerViews != null) {
					foreach (UIView corner in cornerViews) {
						corner.Hidden = true;
					}
					DividerView.Hidden = true;
				}
				UIView.BeginAnimations(animationChangeSplitOrientation);
				UIView.SetAnimationDelegate(this);
				UIView.SetAnimationDidStopSelector(new Selector("AnimationDidStop(finished, context)"));
			}
			Vertical = !Vertical;
			if (showingMaster) {
				UIView.CommitAnimations();
			}
		}
		
		
		void ToggleMasterBeforeDetail()
		{
			bool showingMaster = IsShowingMaster();
			if (showingMaster) {
				if (cornerViews != null) {
					foreach (UIView corner in cornerViews) {
						corner.Hidden = true;
					}
					DividerView.Hidden = true;
				}
				UIView.BeginAnimations(animationChangeSubviewsOrder);
				UIView.SetAnimationDelegate(this);
				UIView.SetAnimationDidStopSelector(new Selector("AnimationDidStop(finished, context)"));
			}
			MasterBeforeDetail = !MasterBeforeDetail;
			if (showingMaster) {
				UIView.CommitAnimations();
			}
		}
		
		
		internal void HidePopover()
		{
			if (hiddenPopoverController != null && hiddenPopoverController.PopoverVisible)
			{
				hiddenPopoverController.Dismiss(true);
				reconfigurePopup = true;
				LayoutSubviews();
			}
		}
		
		
		void ToggleMasterView()
		{
			if (hiddenPopoverController != null && hiddenPopoverController.PopoverVisible) {
				hiddenPopoverController.Dismiss(false);
			}
		
			if (!IsShowingMaster()) {
				// We're about to show the master view. Ensure it's in place off-screen to be animated in.
				reconfigurePopup = true;
				ReconfigureForMasterInPopover(false);
				LayoutSubviews();
			}
		
			// This action functions on the current primary orientation; it is independent of the other primary orientation.
			UIView.BeginAnimations("toggleMaster");
			if (IsLandscape(InterfaceOrientation)) {
				showsMasterInLandscape = !showsMasterInLandscape;
			} else {
				showsMasterInPortrait = !showsMasterInPortrait;
			}
			UIView.CommitAnimations();
		}
		
		
		void ShowMasterPopover()
		{
            AdjustingMaster = true;
			if (hiddenPopoverController != null && !(hiddenPopoverController.PopoverVisible)) {
				// Inform delegate.
				if (splitViewDelegate != null && splitViewDelegate.RespondsToSelector(new Selector("WillPresentViewController"))) {
					WillPresentViewController(viewControllers[0]);
				}
		
				// Show popover.
				hiddenPopoverController.PopoverContentSize = new CGSize(hiddenPopoverController.PopoverContentSize.Width, hiddenPopoverController.ContentViewController.View.Frame.Height);
				hiddenPopoverController.PresentFromBarButtonItem(barButtonItem, UIPopoverArrowDirection.Any, true);
			}
            AdjustingMaster = false;
		}
		
		
//		void SetDelegate(MGSplitViewAppDelegate newDelegate)
//		{
//			if (newDelegate != splitViewDelegate) {
//				splitViewDelegate = newDelegate;
//			}
//		}
		
		
		internal void SetShowsMasterInPortrait(bool flag)
		{
			if (flag != showsMasterInPortrait) {
				showsMasterInPortrait = flag;
		
//				if (IsLandscape(InterfaceOrientation)) { // i.e. if this will cause a visual change.
					if (hiddenPopoverController != null && hiddenPopoverController.PopoverVisible) {
						hiddenPopoverController.Dismiss(false);
					}
		
					// Rearrange views.
					reconfigurePopup = true;
					LayoutSubviews();
//				}
			}
		}
		
		
		internal void SetShowsMasterInLandscape(bool flag)
		{
			if (flag != showsMasterInLandscape) {
				showsMasterInLandscape = flag;
		
				if (IsLandscape(InterfaceOrientation)) { // i.e. if this will cause a visual change.
					if (hiddenPopoverController != null && hiddenPopoverController.PopoverVisible) {
						hiddenPopoverController.Dismiss(false);
					}
		
					// Rearrange views.
					reconfigurePopup = true;
					LayoutSubviews();
				}
			}
		}
		
		
		void SetVertical(bool flag)
		{
			if (flag != Vertical) {
				if (hiddenPopoverController != null && hiddenPopoverController.PopoverVisible) {
					hiddenPopoverController.Dismiss(false);
				}
		
				Vertical = flag;
		
				// Inform delegate.
				if (splitViewDelegate != null && splitViewDelegate.RespondsToSelector(new Selector("WillChangeSplitOrientationToVertical"))) {
					WillChangeSplitOrientationToVertical(Vertical);
				}
		
				LayoutSubviews();
			}
		}
		
		
		void SetMasterBeforeDetail(bool flag)
		{
			if (flag != MasterBeforeDetail) {
				if (hiddenPopoverController != null && hiddenPopoverController.PopoverVisible) {
					hiddenPopoverController.Dismiss(false);
				}
		
				MasterBeforeDetail = flag;
		
				if (IsShowingMaster()) {
					LayoutSubviews();
				}
			}
		}
		
		
		void SetSplitPosition(nfloat pos)
		{
			// Check to see if delegate wishes to constrain the position.
			nfloat newPos = pos;
			bool constrained = false;
			var fullSize = SplitViewSizeForOrientation(InterfaceOrientation);
			if (splitViewDelegate != null && splitViewDelegate.RespondsToSelector(new Selector("ConstrainSplitPosition"))) {
				newPos = ConstrainSplitPosition(newPos, fullSize);
				constrained = true; // implicitly trust delegate's response.
		
			} else {
				// Apply default constraints if delegate doesn't wish to participate.
				nfloat minPos = minViewWidth;
				nfloat maxPos = ((Vertical) ? fullSize.Width : fullSize.Height) - (minViewWidth + SplitWidth);
				constrained = (newPos != SplitPosition && newPos >= minPos && newPos <= maxPos);
			}
		
			if (constrained) {
				if (hiddenPopoverController != null && hiddenPopoverController.PopoverVisible) {
					hiddenPopoverController.Dismiss(false);
				}
		
				SplitPosition = newPos;
		
				// Inform delegate.
				if (splitViewDelegate != null && splitViewDelegate.RespondsToSelector(new Selector("WillMoveSplitToPosition"))) {
					WillMoveSplitToPosition(SplitPosition);
				}
		
				if (IsShowingMaster()) {
					LayoutSubviews();
				}
			}
		}
		
		
		void SetSplitPosition(float pos, bool animate)
		{
			bool shouldAnimate = (animate && IsShowingMaster());
			if (shouldAnimate) {
				UIView.BeginAnimations("SplitPosition");
			}
			SetSplitPosition(pos);
			if (shouldAnimate) {
				UIView.CommitAnimations();
			}
		}
		
		
		internal void SetSplitWidth(float width)
		{
			if (width != SplitWidth && width >= 0) {
				SplitWidth = width;
				if (IsShowingMaster()) {
					LayoutSubviews();
				}
			}
		}
		
		
		internal void SetViewControllers(UIViewController[] controllers)
		{
			if (controllers != viewControllers) {
				foreach (UIViewController controller in viewControllers)
				{
					if (controller != null)
						controller.View.RemoveFromSuperview();
				}

				viewControllers = new UIViewController[2];
				if (controllers != null && controllers.Count() >= 2) {
					viewControllers[0] = controllers[0];
					viewControllers[1] = controllers[1];
				}
		
				LayoutSubviews();
			}
		}

		// this split view implementation will swallow custom back buttons
		// in the master pane when rotated to portrait.  they need to be reset.
		void ResetBackButton (UIViewController controller)
		{
			if (controller is UITabBarController)
			{
				controller = ((UITabBarController)controller).SelectedViewController;
			}

			if (controller is UINavigationController)
			{
				controller = ((UINavigationController)controller).VisibleViewController;
			}

			if (controller != null)
			{
				var button = controller.NavigationItem.LeftBarButtonItem;
				controller.NavigationItem.LeftBarButtonItem = null;
				controller.NavigationItem.LeftBarButtonItem = button;
			}
		}
		
		
		void SetMasterViewController(UIViewController master)
		{
			if (viewControllers == null) {
				viewControllers = new UIViewController[2];
			}

			bool changed = true;
			if (viewControllers.Count() > 0 && viewControllers[0] == master) {
				changed = false;
			} else {
				viewControllers[0] = master;
			}
		
			if (changed) {
				LayoutSubviews();
			}
		}
		
		
		void SetDetailViewController(UIViewController detail)
		{
			if (viewControllers == null) {
				viewControllers = new UIViewController[2];
			}
		
			bool changed = true;
			if (viewControllers.Count() > 1 && viewControllers[1] == detail) {
				changed = false;
			} else {
				viewControllers[1] = detail;
			}
		
			if (changed) {
				LayoutSubviews();
			}
		}
		
		
		void SetDividerView(MGSplitDividerView divider)
		{
			if (divider != DividerView) {
				DividerView.RemoveFromSuperview();

				DividerView = divider;
				DividerView.SplitViewController = this;
				DividerView.BackgroundColor = defaultCornerColor;
				if (IsShowingMaster()) {
					LayoutSubviews();
				}
			}
		}
		
		
		internal bool AllowsDraggingDivider()
		{
			if (DividerView != null) {
				return DividerView.AllowsDragging;
			}
		
			return false;
		}
		
		
		void SetAllowsDraggingDivider(bool flag)
		{
			if (this.AllowsDraggingDivider() != flag && DividerView != null) {
				DividerView.AllowsDragging = flag;
			}
		}
		
		
		void SetDividerStyle(MGSplitViewDividerStyle newStyle)
		{
			if (hiddenPopoverController != null && hiddenPopoverController.PopoverVisible) {
				hiddenPopoverController.Dismiss(false);
			}
		
			// We don't check to see if newStyle equals _dividerStyle, because it's a meta-setting.
			// Aspects could have been changed since it was set.
			DividerStyle = newStyle;
		
			// Reconfigure general appearance and behaviour.
			nfloat cornerRadius = 0;
			if (DividerStyle == MGSplitViewDividerStyle.Thin) {
				cornerRadius = defaultCornerRadius;
				SplitWidth = DefaultThinWidth;
				SetAllowsDraggingDivider(false);
		
			} else if (DividerStyle == MGSplitViewDividerStyle.PaneSplitter) {
				cornerRadius = panesplitterCornerRadius;
				SplitWidth = DefaultThickWidth;
				SetAllowsDraggingDivider(true);
			}
		
			// Update divider and corners.
			DividerView.SetNeedsDisplay();
			if (cornerViews != null) {
				foreach (MGSplitCornersView corner in cornerViews) {
					corner.CornerRadius = cornerRadius;
				}
			}
		
			// Layout all views.
			LayoutSubviews();
		}
		
		
		void SetDividerStyle(MGSplitViewDividerStyle newStyle, bool animate)
		{
			bool shouldAnimate = (animate && IsShowingMaster());
			if (shouldAnimate) {
				UIView.BeginAnimations("DividerStyle");
			}
			SetDividerStyle(newStyle);
			if (shouldAnimate) {
				UIView.CommitAnimations();
			}
		}
		
		void WillShowViewController(UIViewController controller)
		{
		}
		
		void InvalidatingBarButtonItem(UIBarButtonItem barButtonItem)
		{
		}
			
		void WillPresentViewController(UIViewController controller)
		{
		}
		
		void WillChangeSplitOrientationToVertical(bool isVertical)
		{
		}
		
		void WillMoveSplitToPosition(nfloat position)
		{
		}
		
		nfloat ConstrainSplitPosition(nfloat newPos, CGSize splitViewSize)
		{
			return newPos;
		}
	}
	
	class MGSplitViewAppDelegate : UIApplicationDelegate
	{
//		internal MGSplitViewController SplitViewController { get; set; }
//		internal RootViewController RootViewController { get; set; }
//		internal DetailViewController DetailViewController { get; set; }
//
//		public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
//		{
//			UIWindow window = new UIWindow(UIScreen.MainScreen.Bounds);
//			window.AddSubview(SplitViewController.View);
//			window.MakeKeyAndVisible();
//			
//			RootViewController.SelectFirstRow();
//			DetailViewController.ConfigureView();
//			
//			SplitViewController.SplitWidth = 15;
//			SplitViewController.DividerView.AllowsDragging = true;
//			
//			return true;
//		}
	}
	
	/// <summary>
	/// The orientation in which the master pane will render as part of the split view controller.
	/// If the master pane does not render within the split view, it will render as a popover.
	/// </summary>
	public enum MasterOrientation
	{
		None = 0,
		Portrait = 1,
		Landscape = 2,
        Both = 3
	}
	#endregion
}
