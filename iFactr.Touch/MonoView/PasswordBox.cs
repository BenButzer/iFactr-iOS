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
    public class PasswordBox : UITextField, IPasswordBox, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler GotFocus;

        public event EventHandler LostFocus;

        public event ValidationEventHandler Validating;

        public event ValueChangedEventHandler<string> PasswordChanged;

        public event EventHandler<EventHandledEventArgs> ReturnKeyPressed;

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

        public new Font Font
        {
            get { return base.Font.ToFont(); }
            set
            {
                var font = value.ToUIFont();
                if (font != base.Font)
                {
                    base.Font = value.ToUIFont();
                    AttributedPlaceholder = new NSAttributedString(base.Placeholder ?? string.Empty, base.Font, placeholderColor.ToUIColor());

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

        public bool IsFocused
        {
            get { return IsFirstResponder; }
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

        public new string Placeholder
        {
            get { return base.Placeholder; }
            set
            {
                if (value != base.Placeholder)
                {
                    base.Placeholder = value;
                    AttributedPlaceholder = new NSAttributedString(base.Placeholder ?? string.Empty, base.Font, placeholderColor.ToUIColor());

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
            get { return placeholderColor; }
            set
            {
                var color = value.IsDefaultColor ? new Color(56, 0, 0, 24) : value;
                if (color != placeholderColor)
                {
                    placeholderColor = color;
                    AttributedPlaceholder = new NSAttributedString(base.Placeholder ?? string.Empty, base.Font, placeholderColor.ToUIColor());

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("PlaceholderColor"));
                    }
                }
            }
        }
        private Color placeholderColor = new Color(56, 0, 0, 24);

        public string Password
        {
            get { return base.Text; }
            set
            {
                if (value != base.Text)
                {
                    base.Text = value;

                    var phandler = PropertyChanged;
                    if (phandler != null)
                    {
                        phandler(this, new PropertyChangedEventArgs("Password"));
                        phandler(this, new PropertyChangedEventArgs("StringValue"));
                    }

                    var handler = PasswordChanged;
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

        private string currentValue;
        private bool setFocusOnLoad;

        public PasswordBox()
        {
            base.SecureTextEntry = true;
            base.ClearButtonMode = UITextFieldViewMode.WhileEditing;
            base.AutocapitalizationType = UITextAutocapitalizationType.None;
            base.AutocorrectionType = UITextAutocorrectionType.No;
            base.VerticalAlignment = UIControlContentVerticalAlignment.Center;

            base.ShouldChangeCharacters += delegate(UITextField textField, NSRange range, string newString)
            {
                string value = currentValue ?? string.Empty;
                try
                {
                    if (range.Length == 0)
                    {
                        value = value.Insert((int)value.Length, newString);
                    }
                    else
                    {
                        value = value.Replace(value.Substring((int)range.Location, (int)range.Length), newString);
                    }
                }
                catch
                {
                    value = string.Concat(value, newString);
                }

                return expression == null ? true : expression.IsMatch(value);
            };

            base.EditingChanged += (sender, e) =>
            {
                var phandler = PropertyChanged;
                if (phandler != null)
                {
                    phandler(this, new PropertyChangedEventArgs("Password"));
                    phandler(this, new PropertyChangedEventArgs("StringValue"));
                }

                var handler = PasswordChanged;
                if (handler != null)
                {
                    handler(Pair ?? this, new ValueChangedEventArgs<string>(currentValue, Text));
                }

                currentValue = Text;
            };
            
            base.ShouldReturn += delegate
            {
                bool shouldReturn = true;
                var handler = ReturnKeyPressed;
                if (handler != null)
                {
                    var args = new EventHandledEventArgs();
                    handler(pair ?? this, args);
                    shouldReturn = !args.IsHandled;
                }

                if (shouldReturn)
                {
                    ResignFirstResponder();
                    return true;
                }

                return false;
            };

            base.EditingDidBegin += (sender, e) =>
            {
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

            base.EditingDidEnd += (sender, e) =>
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
        }

        public void NullifyEvents()
        {
            Validating = null;
            GotFocus = null;
            LostFocus = null;
            PasswordChanged = null;
            ReturnKeyPressed = null;
        }

        public bool Validate(out string[] errors)
        {
            var handler = Validating;
            if (handler != null)
            {
                var args = new ValidationEventArgs(SubmitKey, Password, StringValue);
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
    }
}

