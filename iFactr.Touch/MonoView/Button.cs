using System;
using System.ComponentModel;

using CoreGraphics;
using Foundation;
using UIKit;

using iFactr.Core;
using iFactr.UI;
using iFactr.UI.Controls;

using Point = iFactr.UI.Point;
using Size = iFactr.UI.Size;

namespace iFactr.Touch
{
    public class Button : UIButton, IButton, INotifyPropertyChanged
    {
        public event EventHandler Clicked;

        public event PropertyChangedEventHandler PropertyChanged;

        public event ValidationEventHandler Validating;

        public new Color BackgroundColor
        {
            get { return base.BackgroundColor.ToColor(); }
            set
            {
                if (value != base.BackgroundColor.ToColor())
                {
                    base.BackgroundColor = value.IsDefaultColor ? null : value.ToUIColor();

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("BackgroundColor"));
                    }
                }
            }
        }

//        public Color BorderColor
//        {
//            get { return Layer.BorderColor.ToColor(); }
//            set
//            {
//                if (value != BorderColor)
//                {
//                    Layer.BorderColor = value.ToCGColor();
//
//                    var handler = PropertyChanged;
//                    if (handler != null)
//                    {
//                        handler(this, new PropertyChangedEventArgs("BorderColor"));
//                    }
//                }
//            }
//        }

        public Color ForegroundColor
        {
            get { return TitleColor(UIControlState.Normal).ToColor(); }
            set
            {
                var color = value.IsDefaultColor ? new Color(0, 122, 255) : value;
                if (color != TitleColor(UIControlState.Normal).ToColor())
                {
                    SetTitleColor(color.ToUIColor(), UIControlState.Normal);

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("ForegroundColor"));
                    }
                }
            }
        }

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
            get { return base.Enabled; }
            set
            {
                if (value != base.Enabled)
                {
                    base.Enabled = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("IsEnabled"));
                    }
                }
            }
        }

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

        public new HorizontalAlignment HorizontalAlignment
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

        public new VerticalAlignment VerticalAlignment
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

        public new Font Font
        {
            get { return base.Font.ToFont(); }
            set
            {
                var font = value.ToUIFont();
                if (font != base.Font)
                {
                    base.Font = value.ToUIFont();

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("Font"));
                    }
                }
            }
        }

        public new string Title
        {
            get
            {
                return Title(UIControlState.Normal);
            }
            set
            {
                if (value != Title(UIControlState.Normal))
                {
                    SetTitle(value, UIControlState.Normal);

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("Title"));
                        handler(this, new PropertyChangedEventArgs("StringValue"));
                    }
                }
            }
        }

        public IImage Image
        {
            get
            {
                return image as IImage;
            }
            set
            {
                if (value != image)
                {
                    if (image is UIImageView)
                    {
    					((UIImageView)image).RemoveObserver(this, new NSString("image"));
                    }

                    image = TouchFactory.GetNativeObject<UIImageView>(value, "image") as IImage;
    				if (image != null)
    				{
    					((UIImageView)image).AddObserver(this, new NSString("image"), NSKeyValueObservingOptions.OldNew, IntPtr.Zero);
    				}
                    OnImageChanged();

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("Image"));
                    }
                }
            }
        }
        private IImage image;

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

		public override void ObserveValue (NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context)
		{
			OnImageChanged();
		}

        public string StringValue
        {
            get { return Title; }
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
				if (pair == null && value != null)
				{
					pair = value;
					pair.Pair = this;
				}
			}
		}
		private IPairable pair;

        public Button()
        {
            ForegroundColor = new Color();
            TouchUpInside += (sender, e) => OnClicked();
        }
        
        public Size Measure(Size constraints)
        {
            var frame = Frame;
            constraints.Width = Math.Min(constraints.Width, int.MaxValue);
            constraints.Height = Math.Min(constraints.Height, int.MaxValue);
            Frame = new CGRect(CGPoint.Empty, new CGSize((float)constraints.Width, (float)constraints.Height));
            SizeToFit();
            
            var size = new Size(Frame.Width, Frame.Height);
            Frame = frame;
            return size;
        }
        
        public void SetLocation(Point location, Size size)
        {
            Frame = new CGRect((float)location.X, (float)location.Y, (float)size.Width, (float)size.Height);
        }

        public void NullifyEvents()
        {
            Clicked = null;
            Validating = null;
        }

        public bool Validate(out string[] errors)
        {
            var handler = Validating;
            if (handler != null)
            {
                var args = new ValidationEventArgs(SubmitKey, Title, StringValue);
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
        
        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            
            if (image != null && ImageView != null)
            {
                switch (image.Stretch)
                {
                    case iFactr.UI.ContentStretch.Fill:
                        ImageView.ContentMode = UIViewContentMode.ScaleToFill;
                        break;
                    case iFactr.UI.ContentStretch.None:
                        ImageView.ContentMode = UIViewContentMode.Redraw;
                        break;
                    case iFactr.UI.ContentStretch.Uniform:
                        ImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
                        break;
                    case iFactr.UI.ContentStretch.UniformToFill:
                        ImageView.ContentMode = UIViewContentMode.ScaleAspectFill;
                        break;
                }
            }
        }

		protected override void Dispose (bool disposing)
		{
			if (image is UIImageView)
			{
				((UIImageView)image).RemoveObserver(this, new NSString("image"));
			}

            base.Dispose(disposing);
		}

		private void OnClicked()
		{
            var handler = Clicked;
			if (handler != null)
			{
				handler(Pair ?? this, EventArgs.Empty);
			}
			else
			{
				iApp.Navigate(NavigationLink, this.GetSuperview<IListView>());
			}
		}
		
		private void OnImageChanged()
		{
			if (image == null)
			{
				SetImage(null, UIControlState.Normal);
			}
			else
			{
				SetImage(((UIImageView)image).Image, UIControlState.Normal);
				this.SignalCellLayout();
			}
		}
    }
}