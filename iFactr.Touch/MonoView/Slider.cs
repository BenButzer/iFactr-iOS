using System;
using System.ComponentModel;
using System.Drawing;

using UIKit;

using iFactr.UI;
using iFactr.UI.Controls;
 
using Point = iFactr.UI.Point;
using Size = iFactr.UI.Size;

namespace iFactr.Touch
{
    public class Slider : UISlider, ISlider, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public event ValidationEventHandler Validating;

        public new event ValueChangedEventHandler<double> ValueChanged;

        public Color MaximumTrackColor
        {
            get { return base.MaximumTrackTintColor.ToColor(); }
            set
            {
                if (value != base.MaximumTrackTintColor.ToColor())
                {
                    base.MaximumTrackTintColor = value.IsDefaultColor ? null : value.ToUIColor();

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("MaximumTrackColor"));
                    }
                }
            }
        }
        
        public Color MinimumTrackColor
        {
            get { return base.MinimumTrackTintColor.ToColor(); }
            set
            {
                if (value != base.MinimumTrackTintColor.ToColor())
                {
                    base.MinimumTrackTintColor = value.IsDefaultColor ? null : value.ToUIColor();

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("MinimumTrackColor"));
                    }
                }
            }
        }

        public new double Value
        {
            get { return base.Value; }
            set
            {
                var fValue = (float)value;
                if (fValue != base.Value)
                {
                    base.Value = fValue;

                    var phandler = PropertyChanged;
                    if (phandler != null)
                    {
                        phandler(this, new PropertyChangedEventArgs("Value"));
                        phandler(this, new PropertyChangedEventArgs("StringValue"));
                    }

                    var handler = this.ValueChanged;
                    if (handler != null)
                    {
                        handler(Pair ?? this, new ValueChangedEventArgs<double>(currentValue, Value));
                    }

                    currentValue = base.Value;
                }
            }
        }

        public new double MaxValue
        {
            get { return base.MaxValue; }
            set
            {
                var fValue = (float)value;
                if (fValue != base.MaxValue)
                {
                    bool valueChanged = base.Value > fValue;
                    base.MaxValue = fValue;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("MaxValue"));
                    }

                    if (valueChanged && handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("Value"));
                    }
                }
            }
        }

        public new double MinValue
        {
            get { return base.MinValue; }
            set
            {
                var fValue = (float)value;
                if (fValue != base.MinValue)
                {
                    bool valueChanged = base.Value < fValue;
                    base.MinValue = fValue;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("MinValue"));
                    }

                    if (valueChanged && handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("Value"));
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
        
        public string StringValue
        {
            get { return Value.ToString(); }
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

        private double currentValue;

        public Slider()
        {
            base.ValueChanged += (sender, e) =>
            {
                var phandler = PropertyChanged;
                if (phandler != null)
                {
                    phandler(this, new PropertyChangedEventArgs("Value"));
                    phandler(this, new PropertyChangedEventArgs("StringValue"));
                }

                var handler = this.ValueChanged;
                if (handler != null)
                {
                    handler(Pair ?? this, new ValueChangedEventArgs<double>(currentValue, Value));
                }

                currentValue = Value;
            };
        }
        
        public Size Measure(Size constraints)
        {
            var frame = Frame;
            constraints.Width = Math.Min(constraints.Width, int.MaxValue);
            constraints.Height = Math.Min(constraints.Height, int.MaxValue);
            Frame = new RectangleF(PointF.Empty, new SizeF((float)constraints.Width, (float)constraints.Height));
            SizeToFit();
            
            var size = new Size(Frame.Width, Frame.Height);
            Frame = frame;
            return size;
        }
        
        public void SetLocation(Point location, Size size)
        {
            Frame = new RectangleF((float)location.X, (float)location.Y, (float)size.Width, (float)size.Height);
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
                var args = new ValidationEventArgs(SubmitKey, Value, StringValue);
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
    }
}

