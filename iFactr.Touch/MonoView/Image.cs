using System;
using System.ComponentModel;
using System.Drawing;

using Foundation;
using UIKit;

using MonoCross;
using MonoCross.Utilities;
using iFactr.UI;
using iFactr.UI.Controls;

using Point = iFactr.UI.Point;
using Size = iFactr.UI.Size;

namespace iFactr.Touch
{
    public class Image : UIImageView, IImage, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [EventDelegate("clicked")]
        public event EventHandler Clicked
        {
            add
            {
                clicked += value;
                UserInteractionEnabled = isEnabled;
            }
            remove
            {
                clicked -= value;
                UserInteractionEnabled = (isEnabled && clicked != null);
            }
        }
        private event EventHandler clicked;

        public event EventHandler Loaded;

        public event ValidationEventHandler Validating;

        public Thickness Margin
        {
            get { return margin; }
            set
            {
                if (value != margin)
                {
                    margin = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("Margin"));
                    }

                    this.SignalCellLayout();
                }
            }
        }
        private Thickness margin;

        public MetadataCollection Metadata
        {
            get { return metadata ?? (metadata = new MetadataCollection()); }
        }
        private MetadataCollection metadata;

        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                if (value != isEnabled)
                {
                    isEnabled = value;
                    UserInteractionEnabled = (isEnabled && clicked != null);

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("IsEnabled"));
                    }
                }
            }
        }
        private bool isEnabled = true;

        public int ColumnIndex
        {
            get { return columnIndex; }
            set
            {
                if (columnIndex != value)
                {
                    columnIndex = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("ColumnIndex"));
                    }

                    this.SignalCellLayout();
                }
            }
        }
        private int columnIndex;
        
        public int ColumnSpan
        {
            get { return columnSpan; }
            set
            {
                if (columnSpan != value)
                {
                    columnSpan = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("ColumnSpan"));
                    }

                    this.SignalCellLayout();
                }
            }
        }
        private int columnSpan;

        public object Parent
        {
            get
            {
                var parent = this.GetSuperview<IPairable>();
                return parent == null ? null : (parent.Pair ?? parent);
            }
        }
        
        public int RowIndex
        {
            get { return rowIndex; }
            set
            {
                if (rowIndex != value)
                {
                    rowIndex = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("RowIndex"));
                    }

                    this.SignalCellLayout();
                }
            }
        }
        private int rowIndex;
        
        public int RowSpan
        {
            get { return rowSpan; }
            set
            {
                if (rowSpan != value)
                {
                    rowSpan = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("RowSpan"));
                    }

                    this.SignalCellLayout();
                }
            }
        }
        private int rowSpan;

        public HorizontalAlignment HorizontalAlignment
        {
            get { return horizontalAlignment; }
            set
            {
                if (horizontalAlignment != value)
                {
                    horizontalAlignment = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("HorizontalAlignment"));
                    }

                    this.SignalCellLayout();
                }
            }
        }
        private HorizontalAlignment horizontalAlignment;

        public VerticalAlignment VerticalAlignment
        {
            get { return verticalAlignment; }
            set
            {
                if (verticalAlignment != value)
                {
                    verticalAlignment = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("VerticalAlignment"));
                    }

                    this.SignalCellLayout();
                }
            }
        }
        private VerticalAlignment verticalAlignment;

        public Visibility Visibility
        {
            get { return visibility; }
            set
            {
                if (value != visibility)
                {
                    visibility = value;
                    base.Hidden = visibility != Visibility.Visible;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("Visibility"));
                    }

                    this.SignalCellLayout();
                }
            }
        }
        private Visibility visibility;

        public ContentStretch Stretch
        {
            get { return stretch; }
            set
            {
                if (value != stretch)
                {
                    stretch = value;
                    switch (stretch)
                    {
                        case iFactr.UI.ContentStretch.Fill:
                            ContentMode = UIViewContentMode.ScaleToFill;
                            break;
                        case iFactr.UI.ContentStretch.None:
                            ContentMode = (Frame.Width < Dimensions.Width || Frame.Height < Dimensions.Height) ?
                                UIViewContentMode.ScaleAspectFit : UIViewContentMode.Center;
                            break;
                        case iFactr.UI.ContentStretch.Uniform:
                            ContentMode = UIViewContentMode.ScaleAspectFit;
                            break;
                        case iFactr.UI.ContentStretch.UniformToFill:
                            ContentMode = UIViewContentMode.ScaleAspectFill;
                            break;
                    }

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("Stretch"));
                    }
                }
            }
        }
        private ContentStretch stretch;

        public string FilePath
        {
            get
            {
                return filePath;
            }
            set
            {
                if (filePath != value)
                {
                    filePath = value;
                    RequestImage(filePath);

                    if (Image != null)
                    {
                        var cell = this.GetSuperview<IGridCell>() as UIView;
                        if (cell != null)
                        {
                            cell.SetNeedsLayout();
                        }
                    }

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("FilePath"));
                        handler(this, new PropertyChangedEventArgs("StringValue"));
                    }
                }
            }
        }
        private string filePath;

        public iFactr.UI.Size Dimensions
        {
            get
            {
                return Image == null ? iFactr.UI.Size.Empty : new iFactr.UI.Size(Image.Size.Width, Image.Size.Height);
            }
        }

        public string StringValue
        {
            get { return filePath; }
        }

        public string ID
        {
            get { return id; }
            set
            {
                if (value != id)
                {
                    id = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("ID"));
                    }
                }
            }
        }
        private string id;

        public string SubmitKey
        {
            get { return submitKey; }
            set
            {
                if (value != submitKey)
                {
                    submitKey = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("SubmitKey"));
                    }
                }
            }
        }
        private string submitKey;

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

        private static UIImage defaultImage = new UIImage();
        private ImageCreationOptions creationOptions;

        public Image()
        {
            AutoresizingMask = UIViewAutoresizing.None;
            Stretch = UI.ContentStretch.None;
            ClipsToBounds = true;

            var weak = new WeakReference<Image>(this);
            var tap = new UITapGestureRecognizer(() =>
            {
                Image instance = null;
                if (weak.TryGetTarget(out instance))
                {
                    var handler = instance.clicked;
                    if (handler != null)
                    {
                        handler(instance.pair ?? instance, EventArgs.Empty);
                    }
                }
            });
            AddGestureRecognizer(tap);
        }

        public Image(ImageCreationOptions options) : this()
        {
            creationOptions = options;
        }

        public Image(IImageData imageData) : this()
        {
            Image = imageData as UIImage;
        }

        public IImageData GetImageData()
        {
            return Image as IImageData;
        }
        
        public Size Measure(Size constraints)
        {
            if (Dimensions == Size.Empty)
            {
                return Size.Empty;
            }

            constraints.Width = Math.Min(constraints.Width, int.MaxValue);
            constraints.Height = Math.Min(constraints.Height, int.MaxValue);
            
            Frame = new RectangleF(0, 0, 0, 0);
            SizeToFit();

            nfloat width = Frame.Width;
            nfloat height = Frame.Height;

            double scale = 1;
            if (constraints.Width < width || stretch != UI.ContentStretch.None)
            {
                scale = constraints.Width / width;
            }
            if (constraints.Height < height || stretch != UI.ContentStretch.None)
            {
                scale = Math.Min(scale, constraints.Height / height);
            }

            return new Size(width * scale, height * scale);
        }
        
        public void SetLocation(Point location, Size size)
        {
            Frame = new RectangleF((float)location.X, (float)location.Y, (float)size.Width, (float)size.Height);
            if (stretch == UI.ContentStretch.None)
            {
                if (Frame.Width < Dimensions.Width || Frame.Height < Dimensions.Height)
                {
                    ContentMode = UIViewContentMode.ScaleAspectFit;
                }
                else
                {
                    ContentMode = UIViewContentMode.Center;
                }
            }
        }

        public void NullifyEvents()
        {
            clicked = null;
            Loaded = null;
            Validating = null;

            UserInteractionEnabled = false;
        }

        public bool Validate(out string[] errors)
        {
            var handler = Validating;
            if (handler != null)
            {
                var args = new ValidationEventArgs(SubmitKey, FilePath, StringValue);
                handler(Pair ?? this, args);

                if (args.Errors.Count > 0)
                {
                    errors = new string[args.Errors.Count];
                    args.Errors.CopyTo(errors, 0);
                    return false;
                }
            }

            errors = null;
            return true;
        }

        public bool Equals (IElement other)
        {
            var control = other as Element;
            if (control != null)
            {
                return control.Equals(this);
            }

            return base.Equals(other);
        }

        private void RequestImage(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                SetValueForKeyPath(defaultImage, new NSString("image"));
                return;
            }

            var image = Device.ImageCache.Get(filePath);
            if (image != null && (creationOptions & ImageCreationOptions.IgnoreCache) == 0)
            {
                SetValueForKeyPath((image as UIImage) ?? defaultImage, new NSString("image"));

                var phandler = PropertyChanged;
                if (phandler != null)
                {
                    phandler(this, new PropertyChangedEventArgs("Dimensions"));
                }

                var handler = Loaded;
                if (handler != null)
                {
                    handler(pair ?? this, EventArgs.Empty);
                }

                return;
            }

            if (System.IO.File.Exists(filePath))
            {
                var uiimage = UIImage.FromFile(filePath);
                image = new ImageData(uiimage.CGImage, uiimage.CurrentScale, uiimage.Orientation, filePath);

                if ((creationOptions & ImageCreationOptions.IgnoreCache) == 0)
                {
                    Device.ImageCache.Add(filePath, image);
                }
                else
                {
                    Device.ImageCache.Remove(filePath);
                }

                SetValueForKeyPath((image as UIImage) ?? defaultImage, new NSString("image"));

                var phandler = PropertyChanged;
                if (phandler != null)
                {
                    phandler(this, new PropertyChangedEventArgs("Dimensions"));
                }

                var handler = Loaded;
                if (handler != null)
                {
                    handler(pair ?? this, EventArgs.Empty);
                }

                return;
            }

            System.Threading.ThreadPool.QueueUserWorkItem((o) =>
            {
                try
                {
                    var uiimage = UIImage.LoadFromData(NSData.FromUrl(NSUrl.FromString(filePath)));
                    UIApplication.SharedApplication.BeginInvokeOnMainThread(() =>
                    {
                        image = new ImageData(uiimage.CGImage, uiimage.CurrentScale, uiimage.Orientation, filePath);
                        if ((creationOptions & ImageCreationOptions.IgnoreCache) == 0)
                        {
                            Device.ImageCache.Add(filePath, image);
                        }
                        else
                        {
                            Device.ImageCache.Remove(filePath);
                        }

                        SetValueForKeyPath((image as UIImage) ?? defaultImage, new NSString("image"));

                        var phandler = PropertyChanged;
                        if (phandler != null)
                        {
                            phandler(this, new PropertyChangedEventArgs("Dimensions"));
                        }

                        var handler = Loaded;
                        if (handler != null)
                        {
                            handler(pair ?? this, EventArgs.Empty);
                        }

                        var superview = this.GetSuperview<iFactr.UI.IGridBase>() as UIView;
                        if (superview != null)
                        {
                            superview.SetNeedsLayout();
                        }
                    });
                }
                catch
                {
                    iFactr.Core.iApp.Log.Error("Unable to load image resource from " + filePath);
                }
            });
        }
    }
}

