using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using iFactr.Core;
using iFactr.Core.Layers;
using iFactr.UI;
using MonoCross.Navigation;
using Foundation;
using UIKit;

namespace iFactr.Touch
{
    public class TabView : UITabBarController, ITabView, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler Rendering;
        
		public Color HeaderColor
        {
            get { return headerColor; }
            set
            {
                if (value != headerColor)
                {
                    headerColor = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("HeaderColor"));
                    }
                }
            }
        }
        private Color headerColor;

        public MetadataCollection Metadata
        {
            get { return metadata ?? (metadata = new MetadataCollection()); }
        }
        private MetadataCollection metadata;

        public PreferredOrientation PreferredOrientations
        {
            get { return preferredOrientations; }
            set
            {
                if (value != preferredOrientations)
                {
                    preferredOrientations = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("PreferredOrientations"));
                    }
                }
            }
        }
        private PreferredOrientation preferredOrientations;
        
        public new int SelectedIndex
        {
            get { return (int)base.SelectedIndex; }
            set
            {
                var controller = (UIViewController)PaneManager.Instance.FromNavContext(Pane.Master, value);
                if (Delegate.ShouldSelectViewController(this, controller))
                {
                    if (value != base.SelectedIndex)
                    {
                        base.SelectedIndex = value;

                        var handler = PropertyChanged;
                        if (handler != null)
                        {
                            handler(this, new PropertyChangedEventArgs("SelectedIndex"));
                        }
                    }
                    Delegate.ViewControllerSelected(this, controller);
                }
            }
        }
    	
        public Color SelectionColor
        {
            get
            {
                return UIDevice.CurrentDevice.CheckSystemVersion(7, 0) ?
                    TabBar.TintColor.ToColor() : TabBar.SelectedImageTintColor.ToColor();
            }
            set
            {
                var color = value.IsDefaultColor ? defaultSelectionColor.ToColor() : value;
                if (color != SelectionColor)
                {
                    if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
                    {
                        TabBar.TintColor = color.ToUIColor();
                    }
                    else
                    {
                        TabBar.SelectedImageTintColor = color.ToUIColor();
                    }

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("SelectionColor"));
                    }
                }
            }
        }

        public IEnumerable<ITabItem> TabItems
        {
            get
			{
				var items = ViewControllers.Select(vc => vc.TabBarItem).OfType<ITabItem>().Select(i => (i.Pair as ITabItem) ?? i);
				foreach (var item in items)
				{
					yield return item;
				}
			}
            set
            {
                int tab = 0;
                
                PaneManager.Instance.Clear(Pane.Master);
                
				if (value == null)
				{
					SetViewControllers(new UIViewController[0], false);
				}
				else
				{
					int maxTabs = UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad ? 8 : 5;
	                UIViewController[] controllers = new UIViewController[value.Count()];
	                foreach (ITabItem tabItem in value)
	                {
						if (tab >= maxTabs - 1 && value.Count() > maxTabs)
						{
                            if (!(MoreNavigationController.ViewControllers.FirstOrDefault() is MoreTabController))
                            {
                                MoreNavigationController.SetViewControllers(new[] { new MoreTabController(this) }, false);
                            }
                            
							controllers[tab] = new UIViewController()  { TabBarItem = TouchFactory.GetNativeObject<UITabBarItem>(tabItem, "tabItem") };
							PaneManager.Instance.AddStack(MoreNavigationController.TopViewController as IHistoryStack, new iApp.AppNavigationContext() { ActiveTab = tab++ });
						}
						else
						{
							var controller = new NavigationController(tab.ToString()) { TabBarItem = TouchFactory.GetNativeObject<UITabBarItem>(tabItem, "tabItem") };
                            if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
                            {
                                controller.NavigationBar.BarTintColor = iApp.Instance.Style.HeaderColor.ToUIColor();
                            }
                            else
                            {
                                controller.NavigationBar.TintColor = iApp.Instance.Style.HeaderColor.IsDefaultColor ? UIColor.Black :
                                    iApp.Instance.Style.HeaderColor.ToUIColor();
                            }

		                    controllers[tab] = controller;
		                    PaneManager.Instance.AddStack(controller, new iApp.AppNavigationContext() { ActiveTab = tab++ });
	                    }
	                }
	                
	                SetViewControllers(controllers, false);
                }

                var handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs("TabItems"));
                }
            }
        }

        public Color TitleColor
        {
            get { return titleColor; }
            set
            {
                if (value != titleColor)
                {
                    titleColor = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("TitleColor"));
                    }
                }
            }
        }
        private Color titleColor;

        public new string Title
        {
            get { return title; }
            set
            {
                if (value != title)
                {
                    title = value;
                    
                    if (NavigationItem != null)
                    {
                        NavigationItem.Title = title ?? string.Empty;
                    }

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("Title"));
                    }
                }
            }
        }
        private string title;

        public double Height { get; private set; }

        public double Width { get; private set; }

		public IPairable Pair
		{
			get { return pair; }
			set
			{
				if (pair == null)
				{
					pair = value;
					pair.Pair = this;
				}
			}
		}
		private IPairable pair;

        public Type ModelType
        {
            get { return model == null ? null : model.GetType(); }
        }

        private object model;
        private bool initialized;
        private UIColor defaultSelectionColor;

        public TabView()
        {
            defaultSelectionColor = UIDevice.CurrentDevice.CheckSystemVersion(7, 0) ? TabBar.TintColor : TabBar.SelectedImageTintColor;

            Delegate = new TabDelegate();
            CustomizableViewControllers = null;
        }
        
        public void SetBackground(Color color)
        {
            TabBar.BackgroundImage = null;
            if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
            {
                TabBar.BarTintColor = color.IsDefaultColor ? null : color.ToUIColor();
            }
            else
            {
                TabBar.TintColor = color.IsDefaultColor ? null : color.ToUIColor();
            }
        }

        public void SetBackground(string imagePath, ContentStretch stretch)
        {
            TabBar.BackgroundImage = UIImage.FromBundle(imagePath);
            if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
            {
                TabBar.BarTintColor = null;
            }
            else
            {
                TabBar.TintColor = null;
            }
        }

        public object GetModel()
        {
            return model;
        }

        public void SetModel(object model)
        {
            this.model = model;
        }

        public void Render()
        {
            var handler = Rendering;
            if (handler != null)
            {
                handler(Pair ?? this, EventArgs.Empty);
            }
        }

		public bool Equals(IView other)
		{
			var view = other as View;
			if (view != null)
			{
				return view.Equals(this);
			}
			
			return base.Equals(other);
		}

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            if (!initialized)
            {
                initialized = true;
                SelectedIndex = PaneManager.Instance.CurrentTab;
            }
        }
		
		public override bool ShouldAutorotate()
		{
			return SelectedViewController == null ? true : SelectedViewController.ShouldAutorotate();
		}
		
		public override UIInterfaceOrientation PreferredInterfaceOrientationForPresentation()
		{
			if (SelectedViewController != null)
			{
				return SelectedViewController.PreferredInterfaceOrientationForPresentation();
			}

            switch (PreferredOrientations)
            {
                case PreferredOrientation.Portrait:
                    return InterfaceOrientation == UIInterfaceOrientation.PortraitUpsideDown ?
                        UIInterfaceOrientation.PortraitUpsideDown : UIInterfaceOrientation.Portrait;
                case PreferredOrientation.Landscape:
                    return InterfaceOrientation == UIInterfaceOrientation.LandscapeRight ?
                        UIInterfaceOrientation.LandscapeRight : UIInterfaceOrientation.LandscapeLeft;
                default:
                    return base.PreferredInterfaceOrientationForPresentation();
            }
		}
		
		public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
		{
			if (SelectedViewController != null && !(SelectedViewController is UIAlertController))
			{
				return SelectedViewController.GetSupportedInterfaceOrientations();
			}
			
			switch (PreferredOrientations)
			{
				case PreferredOrientation.Portrait:
					return UIInterfaceOrientationMask.Portrait | UIInterfaceOrientationMask.PortraitUpsideDown;
				case PreferredOrientation.Landscape:
					return UIInterfaceOrientationMask.Landscape;
				default:
					return UIInterfaceOrientationMask.All;
			}
		}

        public override void ObserveValue(NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context)
        {
            if (keyPath.ToString() == "bounds")
            {
                var frame = ((NSValue)change.ObjectForKey(NSObject.ChangeNewKey)).CGRectValue;
                if (frame.Width != Width)
                {
                    Width = frame.Width;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("Width"));
                    }
                }

                if (frame.Height != Height)
                {
                    Height = frame.Height;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("Height"));
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            UIApplication.SharedApplication.InvokeOnMainThread(() =>
            {
                try
                {
                    TabBar.Layer.RemoveObserver(this, new NSString("bounds"));
                }
                catch (Exception) { }
            });

            base.Dispose(disposing);
        }

        public override void ViewDidLoad()
        {
            Width = TabBar.Frame.Width;
            Height = TabBar.Frame.Height;

            TabBar.Layer.AddObserver(this, new NSString("bounds"), NSKeyValueObservingOptions.New, IntPtr.Zero);
       }

        private class TabDelegate : UITabBarControllerDelegate
        {
            int previousIndex;

            public override bool ShouldSelectViewController(UITabBarController tabBarController, UIViewController viewController)
            {
				var stack = viewController as IHistoryStack;
				if (stack != null)
				{
					Link link = null;
					var tab = viewController.TabBarItem as ITabItem;
					if (viewController == tabBarController.SelectedViewController && tab != null)
					{
						link = (Link)tab.NavigationLink.Clone();
					}
					else if (stack.CurrentView != null)
					{
                        link = new Link(PaneManager.Instance.GetNavigatedURI(stack.CurrentView), new Dictionary<string, string>());
					}
					else if (tab != null)
					{
						link = (Link)tab.NavigationLink.Clone();
					}
					
					if (!PaneManager.Instance.ShouldNavigate(link, Pane.Tabs, NavigationType.Tab))
					{
						return false;
					}
				}
				
                int newIndex = previousIndex = (int)tabBarController.SelectedIndex;
				if (viewController == tabBarController.MoreNavigationController)
				{
					var moreController = tabBarController.MoreNavigationController.ViewControllers.OfType<MoreTabController>().FirstOrDefault();
					if (moreController != null)
					{
						newIndex = moreController.CurrentTab == 0 ? int.MaxValue : moreController.CurrentTab;
					}
				}
				else
				{
                	newIndex = tabBarController.ViewControllers.IndexOf(viewController);
                }

                if (newIndex != previousIndex)
                {
                    iApp.CurrentNavContext.ActiveTab = newIndex;
                    if (PaneManager.IsSplitView)
                    {
                        PaneManager.Instance.FromNavContext(Pane.Detail, newIndex).PopToRoot();
                    }
                }
                else
                {
                    var tab = viewController.TabBarItem as ITabItem;
                    if (tab != null && !tab.RaiseEvent("Selected", EventArgs.Empty))
                    {
                        iApp.Navigate(tab.NavigationLink, tabBarController as ITabView);
                    }
                    return false;
                }

                return true;
            }

            public override void ViewControllerSelected(UITabBarController tabBarController, UIViewController viewController)
            {
                var nav = viewController as UINavigationController;
                var tab = tabBarController.TabBar.SelectedItem as ITabItem;
                if (tab != null && !tab.RaiseEvent("Selected", EventArgs.Empty) && nav.TopViewController == null)
				{
					iApp.Navigate(tab.NavigationLink, tabBarController as ITabView);
				}
            }
        }
		
		private class MoreTabController : UITableViewController, IMXView, IHistoryStack
		{
			public int CurrentTab { get; private set; }

			public string ID { get { return "More"; } }
            
            public IEnumerable<IMXView> Views
            {
                get
                {
                    foreach (var view in NavigationController.ViewControllers.OfType<IMXView>())
                    {
                        var pair = view as IPairable;
                        yield return pair == null ? view : (pair.Pair as IMXView) ?? view;
                    }
                }
            }
	        
	        public IEnumerable<iLayer> History
	        {
				get
				{
					return NavigationController.ViewControllers.OfType<IMXView>()
						.Where(v => v != NavigationController.TopViewController).Select(v => v.GetModel() as iLayer);
				}
	        }
	       	
	        public iLayer CurrentLayer
	        {
	            get { return CurrentView == null ? null : CurrentView.GetModel() as iLayer; }
	        }
	        
	        public IMXView CurrentView
	        {
				get
                {
                    var pair = NavigationController.TopViewController as IPairable;
                    return pair == null ? NavigationController.TopViewController as IMXView : (pair.Pair as IMXView) ?? pair as IMXView;
                }
	        }

            public override UINavigationController NavigationController
            {
                get
                {
                    return base.NavigationController ?? parent.MoreNavigationController;
                }
            }
	        
			private TabView parent;
			
			public MoreTabController(TabView parent)
			{
				this.parent = parent;
				TabBarItem = parent.MoreNavigationController.TopViewController.TabBarItem;
                
                if (NavigationItem != null)
                {
    				NavigationItem.Title = parent.MoreNavigationController.TopViewController.NavigationItem?.Title;
                }
				
				MXContainer.Instance.Views.Add(Guid.NewGuid().ToString(), this);
			}
	        
	        public void PopToLayer(iLayer layer)
	        {
	            var display = NavigationController.ViewControllers.OfType<IMXView>().First(vc => layer.Equals(vc.GetModel()));
	            NavigationController.PopToViewController(display as UIViewController, true);
	        }
	        
	        public iLayer Peek()
	        {
	            return History.LastOrDefault();
	        }
	        
	        public void PushCurrent() { }
	        
			public void Clear(iLayer layer)
	        {
	            PopToRoot();
	        }
	
	        public void InsertView(int index, IMXView view)
	        {
                //increment index so that this controller cannot be removed
	            Parameter.CheckIndex(NavigationController.ViewControllers, "history", ++index);
				
	            var vcs = NavigationController.ViewControllers;
	            vcs[index] = TouchFactory.GetNativeObject<UIViewController>(view, "view");
	            NavigationController.SetViewControllers(vcs, false);
	        }
	
	        public IMXView[] PopToRoot()
	        {
                // to stay consistent with other platforms, the "root" should be considered the view on top of this
                var controllers = NavigationController.PopToViewController(NavigationController.ViewControllers.Length > 1 ? NavigationController.ViewControllers[1] : this, false);
                return controllers == null ? null : controllers.OfType<IMXView>().Select(v => { var p = v as IPairable; return p == null ? v : (p.Pair as IMXView) ?? v; }).ToArray();
	        }
	
	        public IMXView[] PopToView(IMXView view)
	        {
                var controller = TouchFactory.GetNativeObject<UIViewController>(view, "view");
	            Parameter.CheckObjectExists(NavigationController.ViewControllers, "history", controller, "view");
	
//	            regularPop = NavigationController.TopViewController != controller;
                
                var controllers = NavigationController.PopToViewController(controller, true);
                return controllers == null ? null : controllers.OfType<IMXView>().Select(v => { var p = v as IPairable; return p == null ? v : (p.Pair as IMXView) ?? v; }).ToArray();
	        }
	
	        public IMXView PopView()
	        {
                var view = NavigationController.PopViewController(true);
                var pair = view as IPairable;
                if (pair == null)
                {
                    return view as IMXView;
                }
                return (pair.Pair as IMXView) ?? pair as IMXView;
	        }
	
	        public void PushView(IMXView view)
	        {
                var controller = TouchFactory.GetNativeObject<UIViewController>(view, "view");
                if (Views.Count() < 1)
                {
                    NavigationController.PushViewController(this, false);
                    NavigationController.PushViewController(controller, true);
                    controller.NavigationItem.HidesBackButton = false;
                }
                else
                {
	                NavigationController.PushViewController(controller, true);
                }
	        }
	
	        public void ReplaceView(IMXView currentView, IMXView newView)
	        {
                // replacing this view is not allowed
                if (currentView == this)
                {
                    if (NavigationController.ViewControllers.Length == 1)
                    {
                        PushView(newView);
                    }
                    else
                    {
                        InsertView(1, newView);
                    }
                    return;
                }
                
                var controller = TouchFactory.GetNativeObject<UIViewController>(currentView, "currentView");
	            var index = Parameter.CheckObjectExists(NavigationController.ViewControllers, "history", controller, "currentView");
	
	            var vcs = NavigationController.ViewControllers;
	            vcs[index] = TouchFactory.GetNativeObject<UIViewController>(newView, "newView");
	            NavigationController.SetViewControllers(vcs, false);
	        }
			
			public override void ViewWillAppear(bool animated)
			{
				NavigationItem.RightBarButtonItem = null;
                NavigationItem.HidesBackButton = true;
			}         
			
			public override nint RowsInSection(UITableView tableview, nint section)
			{
				return parent.ViewControllers.Length - (parent.TabBar.Items.Length - 1);
			}
			
			public override UITableViewCell GetCell(UITableView tableView, Foundation.NSIndexPath indexPath)
			{
				var tab = parent.TabItems.ElementAt(indexPath.Row + parent.TabBar.Items.Length - 1);
				
				var cell = new UITableViewCell(UITableViewCellStyle.Value1, "tabCell");
				cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
				cell.TextLabel.Text = tab.Title;
				cell.DetailTextLabel.Text = tab.BadgeValue;
                cell.ImageView.Image = tab.ImagePath == null ? null : UIImage.FromBundle(tab.ImagePath);
				return cell;
			}
			
			public override void RowSelected(UITableView tableView, Foundation.NSIndexPath indexPath)
			{
                int index = indexPath.Row + parent.TabBar.Items.Length - 1;
                if (CurrentTab != index)
                {
				    CurrentTab = PaneManager.Instance.CurrentTab = index;
                }

				var tab = parent.TabItems.ElementAt(index);
				if (tab != null)
                {
					if (!tab.RaiseEvent("Selected", EventArgs.Empty))
					{
						iApp.Navigate(tab.NavigationLink, parent as ITabView);
					}
                }
			}

			#region IMXView implementation
			public object GetModel()
			{
				return null;
			}

			public void SetModel(object model)
			{
			}

			public void Render()
			{
			}

			public Type ModelType
			{
				get { return GetType(); }
			}
			#endregion
		}
    }
}

