using System;
using System.ComponentModel;
using System.Linq;
using CoreGraphics;
using Foundation;
using UIKit;
using iFactr.Core;
using iFactr.UI;

namespace iFactr.Touch
{
    public class BrowserView : UIViewController, IBrowserView, INotifyPropertyChanged
    {
        private const float ToolbarHeight = 32;
        
        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler Activated;

        public event EventHandler Deactivated;

        public event EventHandler<LoadFinishedEventArgs> LoadFinished;

        public event EventHandler Rendering;

        public double Height { get; private set; }

        public double Width { get; private set; }

        public Link BackLink
        {
            get { return backLink; }
            set
            {
                if (value != backLink)
                {
                    backLink = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("BackLink"));
                    }
                }
            }
        }
        private Link backLink;

        public bool CanGoBack
        {
            get { return webView.CanGoBack; }
        }
        private bool canGoBack;

        public bool CanGoForward
        {
            get { return webView.CanGoForward; }
        }
        private bool canGoForward;

        public bool EnableDefaultControls
        {
            get { return !(toolbar == null || toolbar.Superview == null); }
            set
            {
                if (value != EnableDefaultControls)
                {
                    if (value)
                    {
                        if (toolbar == null)
                        {
                            toolbar = new UIToolbar()
                            {
                                AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleTopMargin,
                            };

                            if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
                            {
                                toolbar.BarTintColor = headerColor;
                                toolbar.TintColor = titleColor;
                            }
                            else
                            {
                                toolbar.TintColor = headerColor;
                            }

                            var backButton = new UIBarButtonItem(TouchStyle.ImageFromResource("backIcon.png"),
                                UIBarButtonItemStyle.Plain, delegate { GoBack(); }) { Enabled = canGoBack };

                            var forwardButton = new UIBarButtonItem(TouchStyle.ImageFromResource("forwardIcon.png"),
                                UIBarButtonItemStyle.Plain, delegate { GoForward(); }) { Enabled = canGoForward };

                            var spaceButton = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);

                            toolbar.SetItems(new UIBarButtonItem[] { backButton, spaceButton, forwardButton }, true);
                        }

                        nfloat tabBarHeight = 0;
                        if (TabBarController != null)
                        {
                            tabBarHeight = TabBarController.TabBar.Frame.Height;
                        }

                        toolbar.Frame = new CGRect(0, View.Frame.Height - ToolbarHeight - tabBarHeight,
                            View.Frame.Width, ToolbarHeight);

                        View.Add(toolbar);
                        webView.Frame = new CGRect(0, 0, View.Frame.Width, View.Frame.Height - ToolbarHeight);
                    }
                    else if (toolbar != null)
                    {
                        toolbar.RemoveFromSuperview();
                        webView.Frame = View.Frame;
                    }

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("EnableDefaultControls"));
                    }
                }
            }
        }
        private UIToolbar toolbar;

        public IMenu Menu
        {
            get { return menu; }
            set
            {
                if (value != menu)
                {
                    menu = value;
                    this.SetMenu(menu);

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("Menu"));
                    }
                }
            }
        }
        private IMenu menu;

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

        public Color HeaderColor
        {
            get { return headerColor.ToColor(); }
            set
            {
                if (value != headerColor.ToColor())
                {
                    headerColor = value.IsDefaultColor ? null : value.ToUIColor();
                    if (NavigationController != null && NavigationController.NavigationBar != null && NavigationController.VisibleViewController == this)
                    {
                        if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
                        {
                            NavigationController.NavigationBar.BarTintColor = headerColor;
                        }
                        else
                        {
                            NavigationController.NavigationBar.TintColor = headerColor ?? UIColor.Black;
                        }
                    }

                    if (toolbar != null)
                    {
                        if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
                        {
                            toolbar.BarTintColor = headerColor;
                        }
                        else
                        {
                            toolbar.TintColor = headerColor;
                        }
                    }

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("HeaderColor"));
                    }
                }
            }
        }
        private UIColor headerColor;

        public Color TitleColor
        {
            get { return titleColor.ToColor(); }
            set
            {
                if (value != titleColor.ToColor())
                {
                    titleColor = value.IsDefaultColor ? null : value.ToUIColor();
                    if (NavigationController != null && NavigationController.NavigationBar != null && NavigationController.VisibleViewController == this)
                    {
                        if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
                        {
                            NavigationController.NavigationBar.TintColor = titleColor;
                        }

                        NavigationController.NavigationBar.TitleTextAttributes = new UIStringAttributes() { ForegroundColor = titleColor };
                    }

                    if (toolbar != null && UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
                    {
                        toolbar.TintColor = titleColor;
                    }

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("TitleColor"));
                    }
                }
            }
        }
        private UIColor titleColor;

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

        public string StackID
        {
            get { return stackID; }
            set
            {
                if (value != stackID)
                {
                    stackID = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("StackID"));
                    }
                }
            }
        }
        private string stackID;

        public Pane OutputPane
        {
            get { return outputPane; }
            set
            {
                if (value != outputPane)
                {
                    outputPane = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("OutputPane"));
                    }
                }
            }
        }
        private Pane outputPane;

        public PopoverPresentationStyle PopoverPresentationStyle
        {
            get { return ModalPresentationStyle == UIModalPresentationStyle.FullScreen ? PopoverPresentationStyle.FullScreen : PopoverPresentationStyle.Normal; }
            set
            {
                if (value != PopoverPresentationStyle)
                {
                    ModalPresentationStyle = value == PopoverPresentationStyle.FullScreen ? UIModalPresentationStyle.FullScreen : UIModalPresentationStyle.FormSheet;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("PopoverPresentationStyle"));
                    }
                }
            }
        }

        public IHistoryStack Stack
        {
            get
            {
                var stack = NavigationController as IHistoryStack;
                if (stack != null && PaneManager.Instance.Contains(stack))
                {
                    return stack;
                }

                return null;
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

        public Type ModelType
        {
            get { return model == null ? null : model.GetType(); }
        }

        public ShouldNavigateDelegate ShouldNavigate
        {
            get { return shouldNavigate; }
            set
            {
                if (value != shouldNavigate)
                {
                    shouldNavigate = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("ShouldNavigate"));
                    }
                }
            }
        }
        private ShouldNavigateDelegate shouldNavigate;

        private bool hasAppeared;
        private object model;
        private UIWebView webView;

        public BrowserView()
        {
            webView = new UIWebView(View.Frame) { AutoresizingMask = UIViewAutoresizing.All };
            View.Add(webView);

            webView.LoadStarted += delegate
            {
                NavigationItem.Title = TouchFactory.Instance.GetResourceString("Loading");
                UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;
            };

            webView.LoadFinished += (sender, e) => OnLoadFinished();
            webView.LoadError += (sender, e) =>
            {
                iApp.Log.Error(e.Error.LocalizedDescription);
                OnLoadFinished();
            };
        }

        public void GoBack()
        {
            webView.GoBack();
        }

        public void GoForward()
        {
            webView.GoForward();
        }

        public void LaunchExternal(string url)
        {
            Parameter.CheckUrl(url);
            UIApplication.SharedApplication.OpenUrl(NSUrl.FromString(url));
        }

        public void Load(string url)
        {
            Parameter.CheckUrl(url);

            int queryIndex = url.IndexOf('?');
            string file = queryIndex >= 0 ? url.Substring(0, queryIndex) : url;
            if (iApp.File.Exists(file))
            {
                webView.LoadRequest(NSUrlRequest.FromUrl(new NSUrl(url, NSBundle.MainBundle.BundleUrl)));
            }
            else
            {
                webView.LoadRequest(NSUrlRequest.FromUrl(NSUrl.FromString(url)));
            }
        }

        public void LoadFromString(string html)
        {
            webView.LoadHtmlString(html, null);
        }

        public void Refresh()
        {
            webView.Reload();
        }

        public void Render()
        {
            var handler = Rendering;
            if (handler != null)
            {
                handler(Pair ?? this, EventArgs.Empty);
            }
        }

        public void SetBackground(Color color)
        {
            webView.BackgroundColor = color.IsDefaultColor ? null : color.ToUIColor();
        }

        public void SetBackground(string imagePath, ContentStretch stretch)
        {
        }

        public object GetModel()
        {
            return model;
        }

        public void SetModel(object model)
        {
            this.model = model;
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

        public override UIInterfaceOrientation PreferredInterfaceOrientationForPresentation()
        {
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

        public override void ViewDidLoad()
        {
            Width = View.Frame.Width;
            Height = View.Frame.Height;
       }

        public override void ViewWillAppear(bool animated)
        {
            this.SetMenu(menu);
            this.ConfigureBackButton(BackLink, OutputPane);

            if (NavigationItem != null)
            {
                NavigationItem.Title = title ?? string.Empty;
            }
            
            if (NavigationController != null && NavigationController.NavigationBar != null)
            {
                if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
                {
                    NavigationController.NavigationBar.BarTintColor = headerColor;
                    NavigationController.NavigationBar.TintColor = titleColor;
                }
                else
                {
                    NavigationController.NavigationBar.TintColor = headerColor ?? UIColor.Black;
                }

                NavigationController.NavigationBar.TitleTextAttributes = new UIStringAttributes() { ForegroundColor = titleColor };
            }

            if (toolbar != null && toolbar.Superview != null)
            {
                nfloat tabBarHeight = 0;
                if (TabBarController != null)
                {
                    tabBarHeight = TabBarController.TabBar.Frame.Height;
                }

                toolbar.Frame = new CGRect(0, View.Frame.Height - ToolbarHeight - tabBarHeight,
                    View.Frame.Width, ToolbarHeight);

                webView.Frame = new CGRect(0, 0, View.Frame.Width, View.Frame.Height - ToolbarHeight);
            }
            else
            {
                webView.Frame = View.Frame;
            }
        }

        public override void ViewDidAppear(bool animated)
        {
            var stack = NavigationController as IHistoryStack;
            if (!hasAppeared && (!ModalManager.TransitionInProgress || (stack != null && stack.ID == "Popover")))
            {
                hasAppeared = true;
                var handler = Activated;
                if (handler != null)
                {
                    handler(Pair ?? this, EventArgs.Empty);
                }
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            webView.StopLoading();
            UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
        }

        public override void ViewDidDisappear(bool animated)
        {
            var stack = NavigationController as IHistoryStack;
            if (hasAppeared && (!ModalManager.TransitionInProgress || stack == null || stack.ID == "Popover"))
            {
                hasAppeared = false;
                if (stack == null || stack.CurrentView != this)
                {
                    var handler = Deactivated;
                    if (handler != null)
                    {
                        handler(Pair ?? this, EventArgs.Empty);
                    }
                }
            }
        }

        private void OnLoadFinished()
        {
            NavigationItem.Title = webView.EvaluateJavascript("document.title");
            UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;

            if (canGoBack != webView.CanGoBack)
            {
                canGoBack = webView.CanGoBack;

                var phandler = PropertyChanged;
                if (phandler != null)
                {
                    phandler(this, new PropertyChangedEventArgs("CanGoBack"));
                }
            }

            if (canGoForward != webView.CanGoForward)
            {
                canGoForward = webView.CanGoForward;

                var phandler = PropertyChanged;
                if (phandler != null)
                {
                    phandler(this, new PropertyChangedEventArgs("CanGoForward"));
                }
            }

            if (toolbar != null && toolbar.Items.Length > 2)
            {
                toolbar.Items[0].Enabled = canGoBack;
                toolbar.Items[2].Enabled = canGoForward;
            }

            var handler = LoadFinished;
            if (handler != null)
            {
                handler(Pair ?? this, new LoadFinishedEventArgs(webView.Request == null || webView.Request.Url == null ? null : webView.Request.Url.AbsoluteString));
            }
        }
    }
}

