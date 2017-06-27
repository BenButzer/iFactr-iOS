using System;
using System.ComponentModel;
using System.Linq;

using iFactr.Core;
using iFactr.UI;

using CoreGraphics;
using Foundation;
using UIKit;

namespace iFactr.Touch
{
    public class CanvasView : UIViewController, ICanvasView, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler Activated;

        public event EventHandler Deactivated;

        public event SaveEventHandler DrawingSaved;

        public event EventHandler Rendering;

        public double Height { get; private set; }

        public double Width { get; private set; }

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

        public Color StrokeColor
        {
            get { return drawingCanvas.StrokeColor.ToColor(); }
            set
            {
                var color = value.IsDefaultColor ? Color.Black : value;
                if (color != drawingCanvas.StrokeColor.ToColor())
                {
                    drawingCanvas.StrokeColor = color.ToUIColor();

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("StrokeColor"));
                    }
                }
            }
        }

        public double StrokeThickness
        {
            get { return drawingCanvas.StrokeThickness; }
            set
            {
                float fValue = (float)value;
                if (fValue != drawingCanvas.StrokeThickness)
                {
                    drawingCanvas.StrokeThickness = fValue;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("StrokeThickness"));
                    }
                }
            }
        }

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

        public IToolbar Toolbar
        {
            get
            {
                var bar = toolbar as IToolbar;
                if (bar == null)
                    return null;

                return (bar.Pair as IToolbar) ?? bar;
            }
            set
            {
                if (value != Toolbar)
                {
                    if (toolbar != null)
                    {
                        toolbar.RemoveFromSuperview();
                    }


                    toolbar = TouchFactory.GetNativeObject<UIToolbar>(value, "toolbar");
                    if (toolbar != null)
                    {
                        toolbar.SizeToFit();
                        toolbar.Frame = new CGRect(0, View.Bounds.Height - toolbar.Frame.Height, View.Bounds.Width, toolbar.Frame.Height);
                        View.Add(toolbar);
                    }

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("Toolbar"));
                    }
                }
            }
        }
        private UIToolbar toolbar;

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

        private bool hasAppeared;
        private object model;
        private DrawingCanvas drawingCanvas;
        private UIImageView drawingImageView;

        public CanvasView()
        {
            PreferredOrientations = TouchFactory.Instance.LargeFormFactor ? PreferredOrientation.PortraitOrLandscape :
                TouchFactory.Instance.IsLandscape ? PreferredOrientation.Landscape : PreferredOrientation.Portrait;

            View = drawingImageView = new UIImageView()
            {
                AutoresizingMask = UIViewAutoresizing.All,
                ContentMode = UIViewContentMode.ScaleAspectFit,
                BackgroundColor = UIColor.White,
                UserInteractionEnabled = true
            };

            drawingCanvas = new DrawingCanvas();
            drawingImageView.AddSubview(drawingCanvas);
        }

        public void Clear()
        {
            drawingCanvas.Clear();
        }

        public void Load(string fileName)
        {
            drawingCanvas.ImageId = fileName;
        }

        public void Save(bool compositeBackground)
        {
            Save(System.IO.Path.Combine(TouchFactory.Instance.TempPath, Guid.NewGuid().ToString() + ".png"), compositeBackground);
        }

        public void Save(string fileName)
        {
            Save(fileName, false);
        }

        public void Save(string fileName, bool compositeBackground)
        {
            NSData imageToSave = null;

            if (compositeBackground && drawingImageView != null && drawingImageView.Image != null)
            {
                if (drawingCanvas != null && drawingCanvas.Image != null)
                {
                    imageToSave = ((TouchCompositor)TouchFactory.Instance.Compositor)
                        .CreateCompositeImage(drawingImageView.Image, drawingCanvas.Image).AsPNG();
                }
                else
                {
                    imageToSave = drawingImageView.Image.AsPNG();
                }
            }
            else if (drawingCanvas != null && drawingCanvas.Image != null)
            {
                imageToSave = drawingCanvas.Image.AsPNG();
            }

            if (imageToSave == null)
            {
                imageToSave = new NSData();
            }

            // if there isn't an image to save but we have an id,
            // then the image must have been cleared and we should delete it
            //            if (imageToSave == null && !string.IsNullOrEmpty(fileName))
            //            {
            //                TouchFactory.Instance.DeleteImage(fileName);
            //            }
            if (imageToSave != null)
            {
                NSError error;
                if (imageToSave.Save(fileName, false, out error))
                {
                    var handler = DrawingSaved;
                    if (handler != null)
                    {
                        handler(Pair ?? this, new SaveEventArgs(fileName));
                    }
                }
            }
        }

        public void SetBackground(Color color)
        {
            drawingImageView.Image = null;
            drawingImageView.BackgroundColor = color.IsDefaultColor ? UIColor.White : color.ToUIColor();
        }

        public void SetBackground(string imagePath, ContentStretch stretch)
        {
            drawingImageView.Image = new UIImage(imagePath);
            switch (stretch)
            {
                case iFactr.UI.ContentStretch.Fill:
                    drawingImageView.ContentMode = UIViewContentMode.ScaleToFill;
                    break;
                case iFactr.UI.ContentStretch.None:
                    drawingImageView.ContentMode = UIViewContentMode.Center;
                    break;
                case iFactr.UI.ContentStretch.Uniform:
                    drawingImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
                    break;
                case iFactr.UI.ContentStretch.UniformToFill:
                    drawingImageView.ContentMode = UIViewContentMode.ScaleAspectFill;
                    break;
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

            if (toolbar != null)
            {
                toolbar.SizeToFit();
                toolbar.Frame = new CGRect(0, View.Bounds.Height - toolbar.Frame.Height, View.Bounds.Width, toolbar.Frame.Height);
            }

            CGRect frame = View.Bounds;

            // create the signature capture view
            drawingImageView.Frame = frame;
            drawingImageView.Layer.MasksToBounds = true;

            // this seems to cause image corruption
            if (drawingImageView.Image != null)
            {
                nfloat widthScaleFactor = drawingImageView.Frame.Width / drawingImageView.Image.Size.Width;
                nfloat heightScaleFactor = drawingImageView.Frame.Height / drawingImageView.Image.Size.Height;
                nfloat scaleFactor = Math.Min((float)widthScaleFactor, (float)heightScaleFactor);
                nfloat width = drawingImageView.Image.Size.Width * scaleFactor;
                nfloat height = drawingImageView.Image.Size.Height * scaleFactor;
                nfloat x = (frame.Width - width) / 2;
                nfloat y = (frame.Height - height) / 2;

                frame = new CGRect(x, y, drawingImageView.Image.Size.Width * scaleFactor,
                                       drawingImageView.Image.Size.Height * scaleFactor);
            }

            drawingCanvas.Frame = frame;
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

        protected override void Dispose(bool disposing)
        {
            UIApplication.SharedApplication.InvokeOnMainThread(() =>
            {
                using (new NSAutoreleasePool())
                {
                    if (drawingCanvas != null)
                    {
                        drawingCanvas.RemoveFromSuperview();
                        drawingCanvas.Dispose();
                        drawingCanvas = null;
                    }

                    if (drawingImageView != null)
                    {
                        drawingImageView.RemoveFromSuperview();
                        drawingImageView.Dispose();
                        drawingImageView = null;
                    }
                }
            });

            base.Dispose(disposing);
        }
    }
}