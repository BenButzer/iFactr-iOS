using System;
using System.Collections;
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
    public class SelectList : UIButton, ISelectList, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public event ValidationEventHandler Validating;

        public event ValueChangedEventHandler<object> SelectionChanged;

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
        // Microsoft.iOS Conversion: Font goes to TitleLabel
        public new Font Font
        {
            get { return base.TitleLabel.Font.ToFont(); }
            set
            {
                var font = value.ToUIFont();
                if (font != base.TitleLabel.Font)
                {
                    base.TitleLabel.Font = value.ToUIFont();

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

        public IEnumerable Items
        {
            get { return items; }
            set
            {
                if (value != items)
                {
                    items = value;
                    SelectedIndex = items == null || items.Count() == 0 ? -1 : Math.Max(SelectedIndex, 0);

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("Items"));
                    }
                }
            }
        }
        private IEnumerable items;

        public int SelectedIndex
        {
            get
            {
                if (Items == null || selectedItem == null)
                    return selectedIndex;

                return Items.IndexOf(selectedItem);
            }
            set
            {
                if (Items == null)
                {
                    if (value != selectedIndex)
                    {
                        selectedIndex = value;

                        var handler = PropertyChanged;
                        if (handler != null)
                        {
                            handler(this, new PropertyChangedEventArgs("SelectedIndex"));
                        }
                    }
                }
                else if (value < 0)
                {
                    selectedIndex = value;
                    SelectedItem = null;
                }
                else
                {
                    Parameter.CheckIndex(Items, "Items", value);
                    SelectedItem = Items.ElementAt(value);
                }
            }
        }
        private int selectedIndex = -1;

        public object SelectedItem
        {
            get { return selectedItem; }
            set
            {
                if (selectedItem != value)
                {
                    var oldValue = selectedItem;
                    selectedItem = value;
                    selectedIndex = SelectedIndex;
                    
                    SetTitle(StringValue, UIControlState.Normal);

                    var phandler = PropertyChanged;
                    if (phandler != null)
                    {
                        phandler(this, new PropertyChangedEventArgs("SelectedItem"));
                        phandler(this, new PropertyChangedEventArgs("SelectedIndex"));
                        phandler(this, new PropertyChangedEventArgs("StringValue"));
                    }

                    var handler = SelectionChanged;
                    if (handler != null)
                    {
                        handler(Pair ?? this, new ValueChangedEventArgs<object>(oldValue, value));
                    }
                    
                    var cell = this.GetSuperview<UITableViewCell>() ?? this.GetSuperview<UIView>();
					if (cell != null)
					{
						cell.SetNeedsLayout();
					}
                }
            }
        }
        private object selectedItem;

        public string StringValue
        {
            get { return SelectedItem == null ? null : SelectedItem.ToString(); }
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

        public SelectList()
        {
            ForegroundColor = Color.Gray;

            TouchUpInside += (sender, e) => ShowList();
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
            SelectionChanged = null;
        }

        public void ShowList()
        {
            UINavigationController controller = this.GetSuperview<UINavigationController>();
            if (controller != null)
            {
                controller.PushViewController(new SelectListController(this), true);
            }
        }

        public bool Validate(out string[] errors)
        {
            var handler = Validating;
            if (handler != null)
            {
                var args = new ValidationEventArgs(SubmitKey, SelectedItem, StringValue);
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

        private class SelectListController : UITableViewController, IMXView
        {
            public Type ModelType
            {
                get { return null; }
            }
            
            public SelectListController(SelectList list)
                : base(UITableViewStyle.Grouped)
            {
                var source = new SelectListSource(list);
                source.ItemSelected += (index) =>
                {
                    list.SelectedIndex = index;
                    if (NavigationController != null)
                    {
                        NavigationController.PopViewController(true);
                    }
                };
                
                TableView.Source = source;
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
        }
        
        private class SelectListSource : UITableViewSource
        {
            public Action<int> ItemSelected;
            
            private static NSString ekey = new NSString("SelectListItem");

            private IEnumerable values;
            private int selectedIndex;
            private UIFont font;
            
            public SelectListSource(SelectList list)
            {
                values = list.Items;
                selectedIndex = list.SelectedIndex;
                font = list.Font.ToUIFont();
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.DequeueReusableCell("cellKey");
                if (cell == null)
                {
                    cell = new UITableViewCell(UITableViewCellStyle.Default, "cellKey");
                    cell.TextLabel.Lines = 0; // Allows multiple lines
                    cell.TextLabel.LineBreakMode = UILineBreakMode.WordWrap; // Enables word wrapping
                }

                if (indexPath.Row >= values.Count() || indexPath.Row < 0)
                    return cell;

                cell.Accessory = indexPath.Row == selectedIndex ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;

                var value = values.ElementAt(indexPath.Row);
                cell.TextLabel.Text = value == null ? null : value.ToString();
                cell.TextLabel.TextAlignment = UITextAlignment.Left;
                cell.TextLabel.Font = UIFont.SystemFontOfSize(17);

                return cell;
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                // Retrieve the text for the cell at the given indexPath
                NSString text = (NSString)(values.ElementAt(indexPath.Row)?.ToString() ?? "");

                // Define the font and the width constraints
                UIFont font = UIFont.SystemFontOfSize(17); // Use the font used in the cell
                nfloat maxWidth = tableView.Bounds.Width - 30; // Adjust based on your cell's padding/margins

                // Calculate the height of the text
                nfloat height = HeightForText(text, font, maxWidth);

                // Ensure a minimum height
                return (nfloat)Math.Max(height, 44);
            }

            private nfloat HeightForText(NSString text, UIFont font, nfloat width)
            {
                var constraintSize = new CoreGraphics.CGSize(width, nfloat.MaxValue);
                var boundingBox = text.GetBoundingRect(constraintSize, NSStringDrawingOptions.UsesLineFragmentOrigin, new UIStringAttributes { Font = font }, null);
                return (nfloat)Math.Ceiling(boundingBox.Height) + 20; // Add padding as needed
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                return values == null ? 0 : values.Count();
            }
            
            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                tableView.DeselectRow(indexPath, true);

                NSIndexPath[] paths = new[]
                {
                    NSIndexPath.FromRowSection(selectedIndex, 0),
                    indexPath
                };
                
                GetCell(tableView, paths[0]).Accessory = UITableViewCellAccessory.None;
                GetCell(tableView, paths[1]).Accessory = UITableViewCellAccessory.Checkmark;
                selectedIndex = indexPath.Row;
                
                if (ItemSelected != null)
                    ItemSelected(selectedIndex);
            }
        }
    }
}

