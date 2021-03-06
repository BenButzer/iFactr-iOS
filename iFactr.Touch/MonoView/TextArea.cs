using System;
using System.ComponentModel;
using System.Text.RegularExpressions;

using CoreGraphics;
using Foundation;
using UIKit;

using iFactr.UI;
using iFactr.UI.Controls;

using Point = iFactr.UI.Point;
using Size = iFactr.UI.Size;

namespace iFactr.Touch
{
    public class TextArea : UITextView, ITextArea, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler GotFocus;

        public event EventHandler LostFocus;

        public event ValidationEventHandler Validating;

        public event EventHandler<EventHandledEventArgs> ReturnKeyPressed;

        public event ValueChangedEventHandler<string> TextChanged;

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

        public string Expression
        {
            get { return expression == null ? null : expression.ToString(); }
            set
            {
                if (value != Expression)
                {
                    expression = value == null ? null : new Regex(value);

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("Expression"));
                    }
                }
            }
        }
        private Regex expression;

        public new Font Font
        {
            get { return base.Font.ToFont(); }
            set
            {
                var font = value.ToUIFont();
                if (font != base.Font)
                {
                    base.Font = value.ToUIFont();
                    if (placeholderLabel != null)
                    {
                        placeholderLabel.Font = base.Font;
                    }

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("Font"));
                    }
                }
            }
        }

        public bool IsEnabled
        {
            get { return base.Editable; }
            set
            {
                if (value != base.Editable)
                {
                    base.Editable = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("IsEnabled"));
                    }
                }
            }
        }

        public bool IsFocused
        {
            get { return IsFirstResponder; }
        }
        
        public new KeyboardType KeyboardType
        {
            get
            {
                switch (base.KeyboardType)
                {
                    case UIKeyboardType.EmailAddress:
                        return KeyboardType.Email;
                    case UIKeyboardType.DecimalPad:
                        return KeyboardType.PIN;
                    case UIKeyboardType.NumbersAndPunctuation:
                        return KeyboardType.Symbolic;
                    default:
                        return KeyboardType.AlphaNumeric;
                }
            }
            set
            {
                if (value != KeyboardType)
                {
                    switch (value)
                    {
                        case KeyboardType.Email:
                            base.KeyboardType = UIKeyboardType.EmailAddress;
                            break;
                        case KeyboardType.PIN:
                            base.KeyboardType = UIKeyboardType.DecimalPad;
                            break;
                        case KeyboardType.Symbolic:
                            base.KeyboardType = UIKeyboardType.NumbersAndPunctuation;
                            break;
                        default:
                            base.KeyboardType = UIKeyboardType.Default;
                            break;
                    }

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("KeyboardType"));
                    }
                }
            }
        }

        public KeyboardReturnType KeyboardReturnType
        {
            get
            {
                switch (base.ReturnKeyType)
                {
                    case UIReturnKeyType.Done:
                        return KeyboardReturnType.Done;
                    case UIReturnKeyType.Go:
                        return KeyboardReturnType.Go;
                    case UIReturnKeyType.Next:
                        return KeyboardReturnType.Next;
                    case UIReturnKeyType.Search:
                        return KeyboardReturnType.Search;
                    default:
                        return KeyboardReturnType.Default;
                }
            }
            set
            {
                if (value != KeyboardReturnType)
                {
                    switch (value)
                    {
                        case KeyboardReturnType.Done:
                            base.ReturnKeyType = UIReturnKeyType.Done;
                            break;
                        case KeyboardReturnType.Go:
                            base.ReturnKeyType = UIReturnKeyType.Go;
                            break;
                        case KeyboardReturnType.Next:
                            base.ReturnKeyType = UIReturnKeyType.Next;
                            break;
                        case KeyboardReturnType.Search:
                            base.ReturnKeyType = UIReturnKeyType.Search;
                            break;
                        default:
                            base.ReturnKeyType = UIReturnKeyType.Default;
                            break;
                    }

                    if (IsFirstResponder)
                    {
                        ResignFirstResponder();
                        BecomeFirstResponder();
                    }

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("KeyboardReturnType"));
                    }
                }
            }
        }

        public string Placeholder
        {
            get { return placeholderLabel == null ? null : placeholderLabel.Text; }
            set
            {
                if (value != Placeholder)
                {
                    if (placeholderLabel == null)
                    {
                        placeholderLabel = new UILabel()
                        {
                            Font = base.Font,
                            TextColor = new UIColor(0, 0, 0.098f, 0.22f),
                            LineBreakMode = UILineBreakMode.WordWrap,
                            Lines = 0
                        };
                        Add(placeholderLabel);
                    }

                    placeholderLabel.Text = value;
                    placeholderLabel.Alpha = (!string.IsNullOrEmpty(Text) || string.IsNullOrEmpty(value)) ? 0 : 1;
                    PositionPlaceholder();

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("Placeholder"));
                    }
                }
            }
        }

        public Color PlaceholderColor
        {
            get { return placeholderLabel == null ? new Color(56, 0, 0, 24) : placeholderLabel.TextColor.ToColor(); }
            set
            {
                var color = value.IsDefaultColor ? new Color(56, 0, 0, 24) : value;
                if (color != PlaceholderColor)
                {
                    if (placeholderLabel == null)
                    {
                        placeholderLabel = new UILabel()
                        {
                            Font = base.Font,
                            LineBreakMode = UILineBreakMode.WordWrap,
                            Lines = 0,
                            Alpha = 0
                        };
                        Add(placeholderLabel);
                    }

                    placeholderLabel.TextColor = color.ToUIColor();

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("PlaceholderColor"));
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
        
        public TextCompletion TextCompletion
        {
            get
            {
                return (TextCompletion)((AutocorrectionType == UITextAutocorrectionType.No ? 0 : 1) +
                    (AutocapitalizationType == UITextAutocapitalizationType.None ? 0 : 2));
            }
            set
            {
                if (value != TextCompletion)
                {
                    AutocorrectionType = ((value & TextCompletion.OfferSuggestions) == 0) ?
                        UITextAutocorrectionType.No : UITextAutocorrectionType.Default;
                    
                    AutocapitalizationType = ((value & TextCompletion.AutoCapitalize) == 0) ?
                        UITextAutocapitalizationType.None : UITextAutocapitalizationType.Sentences;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("TextCompletion"));
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
                if (value != base.Text)
                {
                    base.Text = value;

                    if (placeholderLabel != null)
                    {
                        placeholderLabel.Alpha = !string.IsNullOrEmpty(Text) ? 0 : 1;
                    }

                    var phandler = PropertyChanged;
                    if (phandler != null)
                    {
                        phandler(this, new PropertyChangedEventArgs("Text"));
                        phandler(this, new PropertyChangedEventArgs("StringValue"));
                    }
                    
                    var handler = TextChanged;
                    if (handler != null)
                    {
                        handler(Pair ?? this, new ValueChangedEventArgs<string>(currentValue, base.Text));
                    }

                    currentValue = base.Text;
                }
            }
        }
        
        public string StringValue
        {
            get { return Text; }
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

        private string currentValue;
        private UILabel placeholderLabel;
        private bool setFocusOnLoad;

        public TextArea()
        {
            base.BackgroundColor = null;

            base.ShouldChangeText += delegate(UITextView textView, NSRange range, string text)
            {
                if (text == "\n")
                {
                    var handler = ReturnKeyPressed;
                    if (handler != null)
                    {
                        var args = new EventHandledEventArgs();
                        handler(pair ?? this, args);
                        if (args.IsHandled)
                        {
                            return false;
                        }
                    }
                }

                string value = currentValue ?? string.Empty;
                try
                {
                    if (range.Length == 0)
                    {
                        value = value.Insert((int)value.Length, text);
                    }
                    else
                    {
                        value = value.Replace(value.Substring((int)range.Location, (int)range.Length), text);
                    }
                }
                catch
                {
                    value = string.Concat(value, text);
                }

                return expression == null ? true : expression.IsMatch(value);
            };

            base.Changed += (sender, e) =>
            {
                if (placeholderLabel != null)
                {
                    placeholderLabel.Alpha = string.IsNullOrEmpty(Text) ? 1 : 0;
                }

				if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0) && SelectedTextRange != null)
				{
					// iOS 7 has a bug where the caret will stay below the bounds of the box.  we need to compensate for it
	            	CGRect line = GetCaretRectForPosition(SelectedTextRange.Start);
					nfloat overflow = line.Y + line.Height - ( ContentOffset.Y + Bounds.Height - ContentInset.Bottom - ContentInset.Top );
					if (overflow > 0)
	    			{
					    var offset = ContentOffset;
					    offset.Y += overflow + 7;
						UIView.Animate(0.2, () => { SetContentOffset(offset, true); });
	    			}
    			}

                var phandler = PropertyChanged;
                if (phandler != null)
                {
                    phandler(this, new PropertyChangedEventArgs("Text"));
                    phandler(this, new PropertyChangedEventArgs("StringValue"));
                }
            	
                var handler = TextChanged;
                if (handler != null)
                {
                    handler(Pair ?? this, new ValueChangedEventArgs<string>(currentValue, Text));
                }

                currentValue = Text;
            };
            
			base.Started += (sender, e) =>
			{
                if (SelectedTextRange != null)
                {
                    ScrollRectToVisible(GetCaretRectForPosition(SelectedTextRange.Start), true);
                }
                
                var phandler = PropertyChanged;
                if (phandler != null)
                {
                    phandler(this, new PropertyChangedEventArgs("IsFocused"));
                }

                var handler = GotFocus;
                if (handler != null)
                {
                    handler(pair ?? this, EventArgs.Empty);
                }
            };

            base.Ended += (sender, e) =>
            {
                var phandler = PropertyChanged;
                if (phandler != null)
                {
                    phandler(this, new PropertyChangedEventArgs("IsFocused"));
                }

                var handler = LostFocus;
                if (handler != null)
                {
                    handler(pair ?? this, EventArgs.Empty);
                }
            };
        }

        public void Focus()
        {
            setFocusOnLoad = (!base.BecomeFirstResponder() && Window == null);
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
            PositionPlaceholder();
        }

        public void NullifyEvents()
        {
            Validating = null;
            GotFocus = null;
            LostFocus = null;
            ReturnKeyPressed = null;
            TextChanged = null;
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

        public bool Equals (IElement other)
		{
            var control = other as Element;
			if (control != null)
			{
				return control.Equals(this);
			}
			
			return base.Equals(other);
		}

        public override void MovedToWindow()
        {
            base.MovedToWindow();
            if (Window != null && setFocusOnLoad)
            {
                setFocusOnLoad = false;
                base.BecomeFirstResponder();
            }
        }

        private void PositionPlaceholder()
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0) && placeholderLabel != null)
            {
                var rect = new NSString(placeholderLabel.Text).GetBoundingRect(new CGSize(Frame.Width, int.MaxValue),
                    NSStringDrawingOptions.UsesLineFragmentOrigin | NSStringDrawingOptions.TruncatesLastVisibleLine,
                    new UIStringAttributes() { Font = placeholderLabel.Font }, new NSStringDrawingContext());
                
                var size = rect.Size;
                var location = GetCaretRectForPosition(BeginningOfDocument).Location;
                location.X += 1;
                location.Y += 1;
                placeholderLabel.Frame = new CGRect(location, size);
            }
        }
    }
}

