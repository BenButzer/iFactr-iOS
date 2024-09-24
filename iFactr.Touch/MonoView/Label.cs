using System;
using System.ComponentModel;

using CoreGraphics;
using Foundation;
using UIKit;

using iFactr.UI;
using iFactr.UI.Controls;

using Point = iFactr.UI.Point;
using Size = iFactr.UI.Size;

namespace iFactr.Touch
{
    public class Label : UILabel, ILabel, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public event ValidationEventHandler Validating;

        public event ValueChangedEventHandler<string> ValueChanged;

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

        public new TextAlignment TextAlignment
        {
            get
            {
                switch (base.TextAlignment)
                {
                    case UITextAlignment.Center:
                        return TextAlignment.Center;
                    case UITextAlignment.Justified:
                        return TextAlignment.Justified;
                    case UITextAlignment.Right:
                        return TextAlignment.Right;
                    default:
                        return TextAlignment.Left;
                }
            }
            set
            {
                if (value != TextAlignment)
                {
                    switch (value)
                    {
                        case TextAlignment.Center:
                            base.TextAlignment = UITextAlignment.Center;
                            break;
                        case TextAlignment.Justified:
                            base.TextAlignment = UITextAlignment.Justified;
                            break;
                        case TextAlignment.Left:
                            base.TextAlignment = UITextAlignment.Left;
                            break;
                        case TextAlignment.Right:
                            base.TextAlignment = UITextAlignment.Right;
                            break;
                    }

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("TextAlignment"));
                    }
                }
            }
        }

        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                if (base.Text != value)
                {
                    string oldValue = base.Text;
                    base.Text = value;

                    var phandler = PropertyChanged;
                    if (phandler != null)
                    {
                        phandler(this, new PropertyChangedEventArgs("Text"));
                        phandler(this, new PropertyChangedEventArgs("StringValue"));
                    }

                    var handler = this.ValueChanged;
                    if (handler != null)
                    {
                        handler(Pair ?? this, new ValueChangedEventArgs<string>(oldValue, Text));
                    }

                    this.SignalCellLayout();
                }
            }
        }

        public Color ForegroundColor
        {
            get { return base.TextColor.ToColor(); }
            set
            {
                var color = value.IsDefaultColor ? Color.Black : value;
                if (color != base.TextColor.ToColor())
                {
                    base.TextColor = color.ToUIColor();

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("ForegroundColor"));
                    }
                }
            }
        }

        public Color HighlightColor
        {
            get { return base.HighlightedTextColor.ToColor(); }
            set
            {
                if (value != HighlightColor)
                {
                    base.HighlightedTextColor = value.IsDefaultColor ? null : value.ToUIColor();

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("HighlightColor"));
                    }
                }
            }
        }

        public new int Lines
        {
            get { return (int)base.Lines; }
            set
            {
                if (value != base.Lines)
                {
                    base.Lines = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("Lines"));
                    }
                }
            }
        }

        public string StringValue
        {
            get { return base.Text; }
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
        
        public Label() : base(new CGRect(0, 0, 0, 0))
        {
            Lines = 0;
            LineBreakMode = UILineBreakMode.TailTruncation;
            BackgroundColor = UIColor.Clear;

            if (!UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
            {
                HighlightedTextColor = UIColor.White;
            }
        }
        
        public Size Measure(Size constraints)
        {
            if (string.IsNullOrEmpty(Text))
            {
                return Size.Empty;
            }
            
            constraints.Width = Math.Min(constraints.Width, int.MaxValue);
            constraints.Height = Math.Min(constraints.Height, int.MaxValue);
            
            var lineHeight = Lines > 0 ? base.Font.LineHeight * Lines : int.MaxValue;

            var size = CGSize.Empty;
            if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
            {
                size = MeasureString(Text, new CGSize((float)constraints.Width, lineHeight)).Size;
            }

            return new Size((double)size.Width, Math.Min((double)size.Height, constraints.Height));
        }
        
        public void SetLocation(Point location, Size size)
        {
            // Crashlytics Oct 2022 Fatal Exception: CALayerInvalidGeometry defensive code IsNan result of 0/0 IsInfinity result of any other n/0
            // Ultimately is is the responsibility up the call stack, but I see a couple of questionable but scary pieces of code in Gridcell 
            if ((location != null) && (size != null))
            {
                double z = 0;
                if (double.IsNaN(location.X) || double.IsInfinity(location.X))
                { location.X = z; }
                if (double.IsNaN(location.Y) || double.IsInfinity(location.Y))
                { location.Y = z; }
                if (double.IsNaN(size.Width) || double.IsInfinity(size.Width))
                { size.Width = z; }
                if (double.IsNaN(size.Height) || double.IsInfinity(size.Height))
                { size.Height = z; }

                Frame = new CGRect((float)location.X, (float)location.Y, (float)size.Width, (float)size.Height);
            }
        }

        public void NullifyEvents()
        {
            Validating = null;
            ValueChanged = null;
        }

        public bool Validate(out string[] errors)
        {
            var handler = Validating;
            if (handler != null)
            {
                var args = new ValidationEventArgs(SubmitKey, Text, StringValue);
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

		public bool Equals(IElement other)
		{
			var control = other as Element;
			if (control != null)
			{
				return control.Equals(this);
			}

			return base.Equals(other);
		}

        public override void DrawText(CoreGraphics.CGRect rect)
        {
            // by default, the label renders its text in the center of the frame.
            // we want the text to render at the top of the frame, so we must shrink the text bounds to just encompass the text.
            if (VerticalAlignment == VerticalAlignment.Stretch && !string.IsNullOrEmpty(Text))
            {
                if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
                {
                    rect = MeasureString(Text, rect.Size);
                }
            }

            base.DrawText(rect);
        }

        public CGRect MeasureString(string text, CGSize size)
        {
            var rect = new NSString(text).GetBoundingRect(size,
                NSStringDrawingOptions.UsesLineFragmentOrigin | NSStringDrawingOptions.TruncatesLastVisibleLine,
                new UIStringAttributes() { Font = base.Font }, new NSStringDrawingContext());

            // 64-bit measuring truncates size values to 3 decimal places.
            // If we don't round up, it may lead to inappropriate text truncation.
            rect.Width += 0.001f;
            rect.Height += 0.001f;
            return rect;
        }
    }
}