using System;
using System.ComponentModel;
using Foundation;
using UIKit;
using iFactr.UI;

namespace iFactr.Touch
{
    [StackBehavior(StackBehaviorOptions.ForceRoot | StackBehaviorOptions.HistoryShy)]
    public class VanityView : UIViewController, IView, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler Rendering;

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
                if (value != HeaderColor)
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
                if (value != TitleColor)
                {
                    titleColor = value.IsDefaultColor ? null : value.ToUIColor();
                    if (NavigationController != null && NavigationController.NavigationBar != null && NavigationController.VisibleViewController == this)
                    {
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
        
        public VanityView()
        {
            View = new UIImageView(UIScreen.MainScreen.ApplicationFrame)
            {
                BackgroundColor = UIDevice.CurrentDevice.CheckSystemVersion(7, 0) ? UIColor.White : UIColor.Black,
                ContentMode = UIViewContentMode.Center,
                AutoresizingMask = UIViewAutoresizing.FlexibleDimensions
            };
        }
        
        public void SetBackground(Color color)
        {
            View.BackgroundColor = color.ToUIColor();
        }

        public void SetBackground(string imagePath, ContentStretch stretch)
        {
            var imageView = View as UIImageView;
            imageView.Image = UIImage.FromBundle(imagePath);
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
        
        public override bool ShouldAutorotate()
        {
            return true;
        }
        
        public override UIInterfaceOrientation PreferredInterfaceOrientationForPresentation()
        {
            switch (PreferredOrientations)
            {
                case PreferredOrientation.Portrait:
                    return UIInterfaceOrientation.Portrait | UIInterfaceOrientation.PortraitUpsideDown;
                case PreferredOrientation.Landscape:
                    return UIInterfaceOrientation.LandscapeLeft | UIInterfaceOrientation.LandscapeRight;
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
            View.Layer.RemoveObserver(this, new NSString("bounds"));
        }

        public override void ViewDidLoad()
        {
            Width = View.Frame.Width;
            Height = View.Frame.Height;

            View.Layer.AddObserver(this, new NSString("bounds"), NSKeyValueObservingOptions.New, IntPtr.Zero);
        }

        public override void ViewWillAppear(bool animated)
        {
            if (NavigationItem != null)
            {
                NavigationItem.Title = title ?? string.Empty;
            }
        
            if (NavigationController != null && NavigationController.NavigationBar != null)
            {
                if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
                {
                    NavigationController.NavigationBar.BarTintColor = headerColor;
                }
                else
                {
                    NavigationController.NavigationBar.TintColor = headerColor ?? UIColor.Black;
                }

                NavigationController.NavigationBar.TitleTextAttributes = new UIStringAttributes() { ForegroundColor = titleColor };
            }
        }
    }
}