using System;
using System.ComponentModel;

using CoreGraphics;
using Foundation;
using UIKit;

using iFactr.Core;
using iFactr.UI;
using iFactr.UI.Controls;

using MonoCross.Navigation;

using Point = iFactr.UI.Point;
using Size = iFactr.UI.Size;

namespace iFactr.Touch
{
    public class TimePicker : UIButton, ITimePicker, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public event ValidationEventHandler Validating;

        public event ValueChangedEventHandler<DateTime?> TimeChanged;

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
            get { return TitleColor(UIControlState.Normal).ToColor(); }
            set
            {
                var color = value.IsDefaultColor ? Color.Black : value;
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

        public DateTime? Time
        {
            get { return time; }
            set
            {
                if (this.time != value)
                {
                    var oldValue = this.time;
                    this.time = value;

                    SetTitle();

                    var phandler = PropertyChanged;
                    if (phandler != null)
                    {
                        phandler(this, new PropertyChangedEventArgs("Time"));
                        phandler(this, new PropertyChangedEventArgs("StringValue"));
                    }

                    var handler = TimeChanged;
                    if (handler != null)
                    {
                        handler(Pair ?? this,  new ValueChangedEventArgs<DateTime?>(oldValue, value));
                    }
                    
                    var cell = this.GetSuperview<UITableViewCell>() ?? this.GetSuperview<UIView>();
                    if (cell != null)
                    {
                        cell.SetNeedsLayout();
                    }
                }
            }
        }
        private DateTime? time;

        public string TimeFormat
        {
            get { return timeFormat; }
            set
            {
                if (value != timeFormat)
                {
                    timeFormat = value;
                    SetTitle();

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("TimeFormat"));
                    }
                }
            }
        }
        private string timeFormat;

        public string StringValue
        {
            get
            {
                if (!Time.HasValue)
                    return string.Empty;

                if (TimeFormat == null)
                    return Time.Value.ToShortTimeString();

                return Time.Value.ToString(TimeFormat);
            }
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

        private UIDatePicker datePicker;
        private UIPopoverController popoverController;

        public TimePicker()
        {
            ForegroundColor = Color.Gray;
            TouchUpInside += (sender, e) => { ShowPicker(); };
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
            TimeChanged = null;
        }

        public void ShowPicker()
        {
            if (datePicker == null)
            {
                datePicker = new UIDatePicker()
                {
                    AutoresizingMask = UIViewAutoresizing.FlexibleWidth,
                    Mode = UIDatePickerMode.Time
                };

                datePicker.ValueChanged += (sender, e) =>
                {
                    Time = ((DateTime)datePicker.Date).ToLocalTime();
                };
            }

            var time = (Time.HasValue && Time.Value.Kind == DateTimeKind.Unspecified) ? new DateTime(Time.Value.Ticks, DateTimeKind.Local) : Time;
            datePicker.Date = (NSDate)(Time.HasValue ? time : DateTime.Now);

            CGSize size = datePicker.SizeThatFits(CGSize.Empty);
            datePicker.Frame = new CGRect(0, 0, size.Width, size.Height);

            if (TouchFactory.Instance.LargeFormFactor)
            {
                if (popoverController == null || popoverController.PopoverContentSize != size)
                {
                    var controller = new UIViewController();
                    controller.View.Frame = datePicker.Frame;
                    controller.View.AddSubview(datePicker);
                    
                    if (popoverController != null)
                    {
                        popoverController.Dismiss(false);
                        popoverController.Dispose();
                        popoverController = null;
                    }
                    
                    popoverController = new UIPopoverController(controller);
                    popoverController.SetPopoverContentSize(new CGSize(datePicker.Frame.Width, datePicker.Frame.Height), false);
                }
                
                popoverController.PresentFromRect(Frame, Superview, UIPopoverArrowDirection.Any, false);
            }
            else
            {
                var navController = this.GetSuperview<UINavigationController>();
                if (navController != null)
                {
                    navController.PushViewController(new PickerViewController(datePicker), true);
                }
            }
        }

        public bool Validate(out string[] errors)
        {
            var handler = Validating;
            if (handler != null)
            {
                var args = new ValidationEventArgs(SubmitKey, Time, StringValue);
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
        
        protected override void Dispose(bool disposing)
        {
            UIApplication.SharedApplication.InvokeOnMainThread(() =>
            {
                if (popoverController != null)
                {
                    popoverController.Dismiss(false);
                    popoverController.Dispose();
                    popoverController = null;
                }
            });
            
            base.Dispose(disposing);
        }
        
        private void SetTitle()
        {
            if (!Time.HasValue)
            {
                char[] array = DateTime.MinValue.ToString(TimeFormat ?? "t").ToCharArray();
                for (int i = 0; i < array.Length; i++)
                {
                    if (char.IsLetterOrDigit(array[i]))
                    {
                        array[i] = '_';
                    }
                }
                SetTitle(new string(array), UIControlState.Normal);
            }
            else
            {
                SetTitle(StringValue, UIControlState.Normal);
            }
        }

        private class PickerViewController : UIViewController, IMXView
        {
            public Type ModelType
            {
                get { return null; }
            }

            private UIDatePicker picker;
            
            public PickerViewController (UIDatePicker picker)
            {
                this.picker = picker;
                
                View.BackgroundColor = UIDevice.CurrentDevice.CheckSystemVersion(7, 0) ? UIColor.White : UIColor.Black;
                View.AddSubview(picker);
            }
            
            public override void ViewWillAppear (bool animated)
            {
                base.ViewWillAppear (animated);
                
                SetPickerFrame();
            }
            
            public override void WillAnimateRotation (UIInterfaceOrientation toInterfaceOrientation, double duration)
            {
                base.WillAnimateRotation (toInterfaceOrientation, duration);
                
                SetPickerFrame();
            }

            public object GetModel()
            {
                return null;
            }

            public void SetModel(object model)
            {
            }

            public void Render()
            {
            }
            
            private void SetPickerFrame()
            {
                CGSize size = picker.SizeThatFits(CGSize.Empty);
                picker.Frame = new CGRect(0, (View.Frame.Height - size.Height) / 2, size.Width, size.Height);
            }
        }
    }
}

