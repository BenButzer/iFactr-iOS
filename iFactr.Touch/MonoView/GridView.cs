using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using CoreGraphics;
using Foundation;
using UIKit;

using iFactr.Core;
using iFactr.UI;
using iFactr.UI.Controls;

namespace iFactr.Touch
{
    public class GridView : UIViewController, IGridView, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler Activated;

        public event EventHandler Deactivated;

        public event EventHandler Rendering;

        public event SubmissionEventHandler Submitting;

        public double Height { get; private set; }

        public double Width { get; private set; }

        public MetadataCollection Metadata
        {
            get { return metadata ?? (metadata = new MetadataCollection()); }
        }
        private MetadataCollection metadata;

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

        public ColumnCollection Columns
        {
            get { return ScrollView.Columns; }
        }

        public IEnumerable<IElement> Children
        {
            get { return ScrollView.Children; }
        }

        public Thickness Padding
        {
            get { return padding; }
            set
            {
                if (value != padding)
                {
                    padding = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("Padding"));
                    }
                }
            }
        }
        private Thickness padding;

        public RowCollection Rows
        {
            get { return ScrollView.Rows; }
        }

        public bool HorizontalScrollingEnabled
        {
            get { return ScrollView.ShowsHorizontalScrollIndicator; }
            set
            {
                if (ScrollView.ShowsHorizontalScrollIndicator != value)
                {
                    ScrollView.ShowsHorizontalScrollIndicator = value;
                    ScrollView.SetNeedsLayout();

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("HorizontalScrollingEnabled"));
                    }
                }
            }
        }

        public bool VerticalScrollingEnabled
        {
            get { return ScrollView.ShowsVerticalScrollIndicator; }
            set
            {
                if (ScrollView.ShowsVerticalScrollIndicator != value)
                {
                    ScrollView.ShowsVerticalScrollIndicator = value;
                    ScrollView.SetNeedsLayout();

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("VerticalScrollingEnabled"));
                    }
                }
            }
        }

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

        public ValidationErrorCollection ValidationErrors { get; private set; }

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

        private GridViewScroller ScrollView
        {
            get { return View as GridViewScroller; }
        }

        private bool hasAppeared;
        private object model;
        private bool keyboardVisible;

        public GridView()
        {
            ValidationErrors = new ValidationErrorCollection();
            View = new GridViewScroller(View.Frame);
        }

        public void AddChild(IElement element)
        {
            UIView view = TouchFactory.GetNativeObject<UIView>(element, "element");
            if (view != null)
            {
                ScrollView.AddSubview(view);

                var handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs("Children"));
                }
            }
        }

        public void RemoveChild(IElement element)
        {
            UIView view = TouchFactory.GetNativeObject<UIView>(element, "element");
            if (view != null)
            {
                view.RemoveFromSuperview();

                var handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs("Children"));
                }
            }
        }

        public IDictionary<string, string> GetSubmissionValues()
        {
            var submitValues = new Dictionary<string, string>();
            foreach (var control in ScrollView.Children.OfType<IControl>().Where(c => c.ShouldSubmit()))
            {
                string[] errors;
                if (!control.Validate(out errors))
                {
                    ValidationErrors[control.SubmitKey] = errors;
                }
                else
                {
                    ValidationErrors.Remove(control.SubmitKey);
                }

                submitValues[control.SubmitKey] = control.StringValue;
            }

            return submitValues;
        }

        public void SetBackground(Color color)
        {
            if (ScrollView.BackgroundView is UIImageView)
            {
                View.Subviews[0] = new UIView(View.Frame);
            }

            ScrollView.BackgroundView.BackgroundColor = color.IsDefaultColor ? UIColor.White : color.ToUIColor();
        }

        public void SetBackground(string imagePath, ContentStretch stretch)
        {
            var imageView = ScrollView.BackgroundView as UIImageView;
            if (imageView == null)
            {
                ScrollView.BackgroundView = imageView = new UIImageView(View.Frame);
            }

            imageView.Image = new UIImage(imagePath);
            switch (stretch)
            {
            case iFactr.UI.ContentStretch.Fill:
                imageView.ContentMode = UIViewContentMode.ScaleToFill;
                break;
            case iFactr.UI.ContentStretch.None:
                imageView.ContentMode = UIViewContentMode.Center;
                break;
            case iFactr.UI.ContentStretch.Uniform:
                imageView.ContentMode = UIViewContentMode.ScaleAspectFit;
                break;
            case iFactr.UI.ContentStretch.UniformToFill:
                imageView.ContentMode = UIViewContentMode.ScaleAspectFill;
                break;
            }
        }

        public void Submit(string url)
        {
            Submit(new Link(url));
        }

        public void Submit(Link link)
        {
            if (link == null)
                return;

            if (link.Parameters == null)
            {
                link.Parameters = new Dictionary<string, string>();
            }

            var submitValues = new Dictionary<string, string>();
            foreach (var control in ScrollView.Children.OfType<IControl>().Where(c => c.ShouldSubmit()))
            {
                string[] errors;
                if (!control.Validate(out errors))
                {
                    ValidationErrors[control.SubmitKey] = errors;
                }
                else
                {
                    ValidationErrors.Remove(control.SubmitKey);
                }

                submitValues[control.SubmitKey] = control.StringValue;
            }

            SubmissionEventArgs args = new SubmissionEventArgs(link, ValidationErrors);

            var handler = Submitting;
            if (handler != null)
            {
                handler(Pair ?? this, args);
            }

            if (args.Cancel)
                return;

            foreach (string id in submitValues.Keys)
            {
                link.Parameters[id] = submitValues[id];
            }

            iApp.Navigate(link, this);
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

            View.SetNeedsLayout();
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

        public override bool ShouldAutorotate()
        {
            return true;
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

            RemoveNotifications();

            NSNotificationCenter.DefaultCenter.AddObserver(this, new ObjCRuntime.Selector("onKeyboardWillShow:"), UIKeyboard.WillShowNotification, null);
            NSNotificationCenter.DefaultCenter.AddObserver(this, new ObjCRuntime.Selector("onKeyboardWillHide:"), UIKeyboard.WillHideNotification, null);
            NSNotificationCenter.DefaultCenter.AddObserver(this, new ObjCRuntime.Selector("onTextDidBeginEditing:"), UITextField.TextDidBeginEditingNotification, null);
            NSNotificationCenter.DefaultCenter.AddObserver(this, new ObjCRuntime.Selector("onTextDidBeginEditing:"), UITextView.TextDidBeginEditingNotification, null);
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
            RemoveNotifications();
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

		[Export("onKeyboardWillShow:")]
        private void OnKeyboardWillShow(NSNotification notification)
        {
            keyboardVisible = true;
            
            NSObject value = null;
            notification.UserInfo.TryGetValue(UIKeyboard.BoundsUserInfoKey, out value);
            if (value == null)
            {
                return;
            }

            var responder = ScrollView.GetSubview<UIView>(v => v.IsFirstResponder);
            if (responder == null)
            {
                return;
            }

            var nsvalue = NSValue.ValueFromPointer(value.Handle);
            var kbFrame = nsvalue.RectangleFValue;

            var insets = ScrollView.ContentInset;
            insets.Bottom = kbFrame.Height;

            if (!UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
            {
                var tabController = ScrollView.GetSuperview<UITabBarController>();
                if (tabController != null)
                {
                    insets.Bottom -= tabController.TabBar.Frame.Height;
                }
            }

            ScrollView.ContentInset = insets;
            ScrollView.ScrollIndicatorInsets = ScrollView.ContentInset;

            nfloat y = ScrollView.Frame.Height - ScrollView.ContentInset.Bottom - responder.Frame.Height;
            var frame = ScrollView.ConvertRectFromView(responder.Bounds, responder);
            if (frame.Y + 10 > ScrollView.ContentOffset.Y + y)
            {
                ScrollView.SetContentOffset(new CGPoint(ScrollView.ContentOffset.X, frame.Y - y + 10), true);
            }
            else if (frame.Y - 10 < ScrollView.ContentOffset.Y + ScrollView.ContentInset.Top)
            {
                ScrollView.SetContentOffset(new CGPoint(ScrollView.ContentOffset.X, NMath.Max(frame.Y - ScrollView.ContentInset.Top - 10, -ScrollView.ContentInset.Top)), true);
            }
        }

		[Export("onKeyboardWillHide:")]
        private void OnKeyboardWillHide(NSNotification notification)
        {
            keyboardVisible = false;

            var insets = ScrollView.ContentInset;
            insets.Bottom = 0;

            if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
            {
              var tabController = ScrollView.GetSuperview<UITabBarController>();
              if (tabController != null)
              {
                  insets.Bottom = tabController.TabBar.Frame.Height;
              }
            }

            ScrollView.ContentInset = insets;
            ScrollView.ScrollIndicatorInsets = ScrollView.ContentInset;
        }

		[Export("onTextDidBeginEditing:")]
        private void OnTextDidBeginEditing(NSNotification notification)
        {
            if (keyboardVisible)
            {
                var responder = notification.Object as UIView;
                if (notification.Object == null)
                {
                    return;
                }

                nfloat y = ScrollView.Frame.Height - ScrollView.ContentInset.Bottom - responder.Frame.Height;
                var frame = ScrollView.ConvertRectFromView(responder.Bounds, responder);
                if (frame.Y + 10 > ScrollView.ContentOffset.Y + y)
                {
                    ScrollView.SetContentOffset(new CGPoint(ScrollView.ContentOffset.X, frame.Y - y + 10), true);
                }
                else if (frame.Y - 10 < ScrollView.ContentOffset.Y + ScrollView.ContentInset.Top)
                {
                    ScrollView.SetContentOffset(new CGPoint(ScrollView.ContentOffset.X, NMath.Max(frame.Y - ScrollView.ContentInset.Top - 10, -ScrollView.ContentInset.Top)), true);
                }
            }
        }

        private void RemoveNotifications()
        {
            NSNotificationCenter.DefaultCenter.RemoveObserver(this, UIKeyboard.WillShowNotification, null);
            NSNotificationCenter.DefaultCenter.RemoveObserver(this, UIKeyboard.WillHideNotification, null);
            NSNotificationCenter.DefaultCenter.RemoveObserver(this, UITextField.TextDidBeginEditingNotification, null);
            NSNotificationCenter.DefaultCenter.RemoveObserver(this, UITextView.TextDidBeginEditingNotification, null);
        }

        private class GridViewScroller : UIScrollView, IGridBase
        {
            public UIView BackgroundView
            {
                get { return Subviews.FirstOrDefault(); }
                set
                {
                    if (value == null)
                    {
                        throw new ArgumentNullException();
                    }

                    if (Subviews.Length > 0 && !(Subviews[0] is IControl))
                    {
                        Subviews[0].RemoveFromSuperview();
                    }

                    base.InsertSubview(value, 0);
                }
            }

            public ColumnCollection Columns { get; private set; }

            public IEnumerable<IElement> Children
            {
                get
                {
                    var controls = Subviews.OfType<IElement>().Select(c => (c.Pair as IElement) ?? c);
                    foreach (var control in controls)
                    {
                        yield return control;
                    }
                }
            }

            public Thickness Padding { get; set; }

            public RowCollection Rows { get; private set; }

            public GridViewScroller(CGRect frame) : base(frame)
            {
                Columns = new ColumnCollection();
                Rows = new RowCollection();

                BackgroundColor = null;
                BackgroundView = new UIView(frame) { BackgroundColor = UIColor.White };
            }

            public void AddChild(IElement element)
            {
                UIView view = TouchFactory.GetNativeObject<UIView>(element, "element");
                if (view != null)
                {
                    AddSubview(view);
                }
            }

            public void RemoveChild(IElement element)
            {
                UIView view = TouchFactory.GetNativeObject<UIView>(element, "element");
                if (view != null)
                {
                    view.RemoveFromSuperview();
                }
            }

            public override void LayoutSubviews()
            {
                base.LayoutSubviews();

                BackgroundView.Frame = new CGRect(ContentOffset.X, ContentOffset.Y, Frame.Width, Frame.Height);

                UI.Size maxSize;
                maxSize.Width = (ShowsHorizontalScrollIndicator ? double.PositiveInfinity : Frame.Width) - ContentInset.Left - ContentInset.Right;
                maxSize.Height = (ShowsVerticalScrollIndicator ? double.PositiveInfinity : Frame.Height) - ContentInset.Bottom - ContentInset.Top;

                var size = this.GetSuperview<IGridBase>().PerformLayout(new iFactr.UI.Size(Frame.Width - ContentInset.Left - ContentInset.Right, Frame.Height - ContentInset.Top - ContentInset.Bottom), maxSize);
                if (size.Width != ContentSize.Width || size.Height != ContentSize.Height)
                {
                    ContentSize = new CGSize((float)size.Width, (float)size.Height);
                }
            }
        }
    }
}

