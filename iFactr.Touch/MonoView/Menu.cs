using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using MonoCross.Navigation;

using Foundation;
using UIKit;

using iFactr.Core;
using iFactr.UI;
using Color = iFactr.UI.Color;

namespace iFactr.Touch
{
    public class Menu : UIAlertController, IMenu, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Color BackgroundColor
        {
            get { return backgroundColor; }
            set
            {
                if (value != backgroundColor)
                {
                    backgroundColor = value;//base.BackgroundColor = value.ToUIColor(); } // wait for conditional styling

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("BackgroundColor"));
                    }
                }
            }
        }
        private Color backgroundColor;

        public Color ForegroundColor
        {
            get { return foregroundColor; }
            set
            {
                if (value != foregroundColor)
                {
                    foregroundColor = value;//foregroundColor = value.IsDefaultColor ? defaultForegroundColor : value.ToUIColor(); } // wait for conditional styling

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("ForegroundColor"));
                    }
                }
            }
        }
        private Color foregroundColor;

        public Color SelectionColor
        {
            get { return selectionColor; }
            set
            {
                if (value != selectionColor)
                {
                    selectionColor = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("SelectionColor"));
                    }
                }
            }
        }
        private Color selectionColor;

        public string ImagePath
        {
            get { return imagePath; }
            set
            {
                if (value != imagePath)
                {
                    imagePath = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("ImagePath"));
                    }
                }
            }
        }
        private string imagePath;

        public new string Title
        {
            get { return title; }
            set
            {
                if (value != title)
                {
                    title = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("Title"));
                    }
                }
            }
        }
        private string title;

        public int ButtonCount
        {
            get { return menuButtons.Count; }
        }

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

        private List<IMenuButton> menuButtons;

        public Menu() : base()
        {
            menuButtons = new List<IMenuButton>();
            AddAction(UIAlertAction.Create(TouchFactory.Instance.GetResourceString("Cancel"), UIAlertActionStyle.Cancel, null));
        }

        public void Add(IMenuButton menuButton)
        {
            if (menuButton == null)
            {
                return;
            }

            menuButtons.Add(menuButton);

            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs("ButtonCount"));
            }
        }

        public IMenuButton GetButton(int index)
        {
            return menuButtons[index];
        }

        public bool Equals(IMenu other)
        {
            var menu = other as iFactr.UI.Menu;
            if (menu != null)
            {
                return menu.Equals(this);
            }

            return base.Equals(other);
        }

        protected override void Dispose(bool disposing)
        {
            UIApplication.SharedApplication.InvokeOnMainThread(() =>
            {
                for (int i = 0; i < Actions.Length; i++)
                {
                    Actions[i].Dispose();
                }
            });

            menuButtons.Clear();

            base.Dispose(disposing);
        }

        public override void ViewWillAppear(bool animated)
        {
            UIViewController presenter = null;

            var splitController = PresentingViewController as MGSplitViewController;
            if (splitController != null && PopoverPresentationController != null)
            {
                foreach (var controller in splitController.GetControllers())
                {
                    var viewController = controller;
                    var navController = controller as UINavigationController;
                    var tabController = controller as UITabBarController;
                    if (tabController != null)
                    {
                        viewController = tabController.SelectedViewController;
                        navController = viewController as UINavigationController;
                    }

                    if (navController != null)
                    {
                        viewController = navController.TopViewController;
                    }

                    if (viewController != null && viewController.NavigationItem != null && viewController.NavigationItem.RightBarButtonItems != null &&
                        viewController.NavigationItem.RightBarButtonItems.Any(b => b == PopoverPresentationController.BarButtonItem))
                    {
                        presenter = viewController;
                    }
                }
            }

            if (presenter == null)
            {
                var tabController = PresentingViewController as UITabBarController;
                presenter = tabController == null ? PresentingViewController : tabController.SelectedViewController;

                var navController = presenter as UINavigationController;
                if (navController != null)
                {
                    presenter = navController.TopViewController;
                }
            }

            if (Actions.Length - 1 < menuButtons.Count)
            {
                var weakPresenter = new WeakReference(presenter);

                for (int i = Actions.Length - 1; i < menuButtons.Count; i++)
                {
                    var menuButton = menuButtons[i];
                    AddAction(UIAlertAction.Create(menuButton.Title, UIAlertActionStyle.Default, (o) =>
                    {
                        var item = TouchFactory.GetNativeObject<UIBarButtonItem>(menuButton, "menuButton");
                        if (item != null && item.Target != null && item.Action != null)
                        {
                            item.Target.PerformSelector(item.Action, weakPresenter.Target as NSObject, 0);
                        }
                    }));
                }
            }
        }
    }
    
    public class MenuLegacy : UIActionSheet, IMenu, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public new Color BackgroundColor
        {
            get { return backgroundColor; }
            set
            {
                if (value != backgroundColor)
                {
                    backgroundColor = value;//base.BackgroundColor = value.ToUIColor(); } // wait for conditional styling

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("BackgroundColor"));
                    }
                }
            }
        }
        private Color backgroundColor;

        public Color ForegroundColor
        {
            get { return foregroundColor; }
            set
            {
                if (value != foregroundColor)
                {
                    foregroundColor = value;//foregroundColor = value.IsDefaultColor ? defaultForegroundColor : value.ToUIColor(); } // wait for conditional styling

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("ForegroundColor"));
                    }
                }
            }
        }
        private Color foregroundColor;

        public Color SelectionColor
        {
            get { return selectionColor; }
            set
            {
                if (value != selectionColor)
                {
                    selectionColor = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("SelectionColor"));
                    }
                }
            }
        }
        private Color selectionColor;

        public string ImagePath
        {
            get { return imagePath; }
            set
            {
                if (value != imagePath)
                {
                    imagePath = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("ImagePath"));
                    }
                }
            }
        }
        private string imagePath;

        public override string Title
        {
            get { return title; }
            set
            {
                if (value != title)
                {
                    title = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("Title"));
                    }
                }
            }
        }
        private string title;

        public new int ButtonCount
        {
            get { return (int)base.ButtonCount; }
        }

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

        private List<IMenuButton> menuButtons;
        private UIView parentView;
        private UIBarButtonItem parentItem;
//        private UIColor defaultForegroundColor;

        public MenuLegacy()
        {
            menuButtons = new List<IMenuButton>();
            
            // override default per porter
            if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0) && UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad)
            {
                base.BackgroundColor = new UIColor(1, 1, 1, 1f);
            }
            
            Clicked += ActionSheetClicked;
        }

        public override void ShowInView(UIView view)
        {
            parentView = view;
            parentItem = null;

            base.ShowInView(view);
            
//            foreach (var button in Subviews.OfType<UIButton>())
//            {
//                if (defaultForegroundColor == null)
//                {
//                    defaultForegroundColor = button.CurrentTitleColor;
//                    if (foregroundColor == null)
//                    {
//                        foregroundColor = defaultForegroundColor;
//                    }
//                }
//
//                button.SetTitleColor(foregroundColor, UIControlState.Normal);
//            }
        }

        public override void ShowFrom(UIBarButtonItem item, bool animated)
        {
            parentItem = item;
            parentView = null;

            base.ShowFrom(item, animated);
        }

        public void Add(IMenuButton menuButton)
        {
            if (menuButton != null)
            {
                menuButtons.Add(menuButton);
                AddButton(menuButton.Title);

                var handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs("ButtonCount"));
                }
            }
        }

        public IMenuButton GetButton(int index)
        {
            return menuButtons[index];
        }

        public bool Equals(IMenu other)
        {
            var menu = other as iFactr.UI.Menu;
            if (menu != null)
            {
                return menu.Equals(this);
            }

            return base.Equals(other);
        }

        private void ActionSheetClicked(object sender, UIButtonEventArgs e)
        {
            if (e.ButtonIndex < menuButtons.Count)
            {
                var item = TouchFactory.GetNativeObject<UIBarButtonItem>(menuButtons[(int)e.ButtonIndex], "menuButton");
                if (item != null && item.Target != null && item.Action != null)
                {
                    UIViewController obj = null;
                    if (parentView != null)
                    {
                        obj = parentView.GetSuperview<IListView>() as UIViewController;
                    }
                    else if (parentItem != null)
                    {
                        obj = parentItem.GetSuperview();
                    }

                    item.Target.PerformSelector(item.Action, obj, 0);
                }
            }
        }
    }

    public class MenuButton : UIBarButtonItem, IMenuButton, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public new event EventHandler Clicked;

        public string ImagePath
        {
            get { return imagePath; }
            set
            {
                if (value != imagePath)
                {
                    imagePath = value;

                    if (iApp.File.Exists(value))
                    {
                        base.Image = new UIImage(imagePath);
                    }
                    else
                    {
                        iApp.Log.Platform("File not found at " + value);
                    }

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("ImagePath"));
                    }
                }
            }
        }
        private string imagePath;

        public Link NavigationLink
        {
            get { return navigationLink; }
            set
            {
                if (value != navigationLink)
                {
                    navigationLink = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("NavigationLink"));
                    }
                }
            }
        }
        private Link navigationLink;

        public override string Title
        {
            get { return base.Title; }
            set
            {
                if (value != base.Title)
                {
                    base.Title = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("Title"));
                    }
                }
            }
        }

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

        public MenuButton(string title)
        {
            Title = title;
            base.Clicked += OnClicked;
        }

        public bool Equals(IMenuButton other)
        {
            var item = other as iFactr.UI.MenuButton;
            if (item != null)
            {
                return item.Equals(this);
            }

            return base.Equals(other);
        }

        private void OnClicked(object sender, EventArgs e)
        {
            var handler = Clicked;
            if (handler != null)
            {
                handler(Pair ?? this, EventArgs.Empty);
            }
            else
            {
                iFactr.Core.iApp.Navigate(NavigationLink, (sender as IMXView) ?? this.GetSuperview() as IMXView);
            }
        }
    }
}

