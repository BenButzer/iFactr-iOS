using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using CoreGraphics;
using UIKit;

using iFactr.Core;
using iFactr.UI;
using iFactr.UI.Controls;

namespace iFactr.Touch
{
    public class GridCell : UITableViewCell, IGridCell, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [EventDelegate("selected")]
        public new event EventHandler Selected
        {
            add
            {
                selected += value;
                SetAccessory();
            }
            remove
            {
                selected -= value;
                SetAccessory();
            }
        }
        private event EventHandler selected;
        
		[EventDelegate("accessorySelected")]
        public event EventHandler AccessorySelected
        {
            add
            {
                accessorySelected += value;
                SetAccessory();
            }
            remove
            {
                accessorySelected -= value;
                SetAccessory();
            }
        }
        private event EventHandler accessorySelected;
        
        public Link AccessoryLink
        {
            get { return accessoryLink; } 
            set
            {
                if (value != accessoryLink)
                {
                    accessoryLink = value;
                    SetAccessory();

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("AccessoryLink"));
                    }
                }
            }
        }
        private Link accessoryLink;

        public new Color BackgroundColor
        {
            get { return base.BackgroundView == null ? new Color() : base.BackgroundView.BackgroundColor.ToColor(); }
            set
            {
                if (value != BackgroundColor)
                {
                    if (base.BackgroundView == null)
                    {
                        base.BackgroundView = new UIView();
                    }

                    base.BackgroundView.BackgroundColor = value.IsDefaultColor ? null : value.ToUIColor();

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("BackgroundColor"));
                    }
                }
            }
        }

        public ColumnCollection Columns { get; private set; }

        public double MaxHeight
        {
            get { return maxHeight; }
            set
            {
                if (value != maxHeight)
                {
                    maxHeight = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("MaxHeight"));
                    }
                }
            }
        }
        private double maxHeight;

        public double MinHeight
        {
            get { return minHeight; }
            set
            {
                if (value != minHeight)
                {
                    minHeight = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("MinHeight"));
                    }
                }
            }
        }
        private double minHeight;

        public MetadataCollection Metadata
        {
            get { return metadata ?? (metadata = new MetadataCollection()); }
        }
        private MetadataCollection metadata;
        
        public Color SelectionColor
        {
            get { return SelectedBackgroundView == null ? new Color() : SelectedBackgroundView.BackgroundColor.ToColor(); }
            set
            {
                if (value != SelectionColor)
                {
                    if (value.IsDefaultColor)
                    {
                        SelectedBackgroundView = null;
                    }
                    else
                    {
                        SelectedBackgroundView = new UIView() { BackgroundColor = value.ToUIColor() };
                    }

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("SelectionColor"));
                    }
                }
            }
        }
        
        public new SelectionStyle SelectionStyle
        {
			get { return selectionStyle; }
            set
            {
                if (value != selectionStyle)
                {
    				selectionStyle = value;
    					
    				base.SelectionStyle = value == SelectionStyle.Default || value == SelectionStyle.HighlightOnly ?
    					UITableViewCellSelectionStyle.Blue : UITableViewCellSelectionStyle.None;
                    
                    SetAccessory();

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("SelectionStyle"));
                    }
                }
            }
        }
		private SelectionStyle selectionStyle;
        
        public Link NavigationLink
        {
            get { return navigationLink; }
            set
            {
                if (value != navigationLink)
                {
                    navigationLink = value;
                    SetAccessory();

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("NavigationLink"));
                    }
                }
            }
        }
        private Link navigationLink;

        public Thickness Padding
        {
            get { return padding; }
            set
            {
                if (value != padding)
                {
                    padding = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("Padding"));
                    }
                }
            }
        }
        private Thickness padding;

        public RowCollection Rows { get; private set; }
        
        public IEnumerable<IElement> Children
        {
            get
			{
                var controls = ContentView.Subviews.OfType<IElement>().Select(c => (c.Pair as IElement) ?? c);
				foreach (var control in controls)
				{
					yield return control;
				}
			}
        }

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
        
        public GridCell() : base(UITableViewCellStyle.Default, ListView.CellId.ToString())
        {
            Columns = new ColumnCollection();
            Rows = new RowCollection();
        }
        
        public void AddChild(IElement element)
        {
            UIView view = TouchFactory.GetNativeObject<UIView>(element, "element");
            if (view != null)
            {
                ContentView.AddSubview(view);

                // for backward compatibility with ICustomItems that use ViewWithTag()
                for (int i = 2; i < ContentView.Subviews.Length; i++)
                {
                    ContentView.Subviews[i].Tag = i - 1;
                }

                var handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs("Children"));
                }
            }
        }

        public void NullifyEvents()
        {
            selected = null;
            accessorySelected = null;
        }
        
        public void RemoveChild(IElement element)
        {
            UIView view = TouchFactory.GetNativeObject<UIView>(element, "element");
            if (view != null)
            {
                view.RemoveFromSuperview();

                var handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs("Children"));
                }
            }
        }
        
        public void Select()
        {
            SetSelected(true, false);

            var tableView = this.GetSuperview<UITableView>();
            if (tableView != null)
            {
                var path = tableView.IndexPathForCell(this);
                foreach (var selectedPath in tableView.IndexPathsForSelectedRows)
                {
                    if (selectedPath.Section != path.Section || selectedPath.Row != path.Row)
                    {
                        tableView.DeselectRow(selectedPath, false);
                    }
                }
            }
            else
            {
                var tv = this.GetSuperview<TableView>();
                if (tv != null && tv.IndexPathForSelectedRow != null)
                {
                    var path = tv.IndexPathForCell(this);
                    if (path.Section != tv.IndexPathForSelectedRow.Section || path.Row != tv.IndexPathForSelectedRow.Row)
                    {
                        tv.DeselectRow(tv.IndexPathForSelectedRow, false);
                    }
                }
            }
            
            var handler = selected;
            if (handler != null)
            {
                handler(Pair ?? this, EventArgs.Empty);
            }
            else
            {
                iApp.Navigate(NavigationLink, this.GetSuperview<MonoCross.Navigation.IMXView>());
            }
        }

		public bool Equals(ICell other)
		{
			var cell = other as Cell;
			if (cell != null)
			{
				return cell.Equals(this);
			}
			
			return base.Equals(other);
		}

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            // I do not like going off of the frame width.  The ContentView frame width would be preferred,
            // but we would first need to ensure that its value is correctly adjusted in case of a disclosure.
            // This should happen automatically, but it doesn't always happen in time for PerformLayout.
            // There also seem to be cases where the adjusted value is not entirely accurate.
            float width = (float)Frame.Width;
            if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
            {
                width -= (Accessory == UITableViewCellAccessory.DetailDisclosureButton ? 67 :
                    Accessory == UITableViewCellAccessory.DisclosureIndicator ? 33 : 0);
            }
            else
            {
                width -= (Accessory == UITableViewCellAccessory.DetailDisclosureButton ? 33 :
                    Accessory == UITableViewCellAccessory.DisclosureIndicator ? 20 : 0);
            }

            width = Math.Max(width, 0);
            var size = this.PerformLayout(new iFactr.UI.Size(width, Math.Max(MinHeight - 1, 0)), new iFactr.UI.Size(width, Math.Max(MaxHeight - 1, 0)));

            ContentView.Frame = new CGRect(0, 0, (float)size.Width, (float)size.Height);
            Frame = new CGRect(Frame.Location, new CGSize(Frame.Width, ContentView.Frame.Height));
            
            var disclosure = this.GetSubview<UIControl>(c => c.Description.StartsWith("<UITableViewCellDetailDisclosureView"));
            if (disclosure != null)
            {
                disclosure.TouchUpInside -= SelectAccessory;
                disclosure.TouchUpInside += SelectAccessory;
            }
        }

        // don't override RemoveFromSuperview per https://bugzilla.xamarin.com/show_bug.cgi?id=13430
        // things seem to behave fine without it anyway.
//        public override void RemoveFromSuperview()
//        {
//            for (int i = 0; i < ContentView.Subviews.Length; i++)
//            {
//                var view = ContentView.Subviews[i];
//                if (view.IsFirstResponder)
//                {
//                    view.ResignFirstResponder();
//                    break;
//                }
//            }
//            base.RemoveFromSuperview();
//        }
        
        private void SetAccessory()
        {
            if ((accessoryLink != null && accessoryLink.Address != null) || accessorySelected != null)
            {
                Accessory = UITableViewCellAccessory.DetailDisclosureButton;
            }
            else if (((navigationLink != null && navigationLink.Address != null) || selected != null) &&
            	(selectionStyle == SelectionStyle.Default || selectionStyle == SelectionStyle.IndicatorOnly))
            {
                Accessory = UITableViewCellAccessory.DisclosureIndicator;
            }
            else
            {
                Accessory = UITableViewCellAccessory.None;
            }
        }
        
        private void SelectAccessory(object sender, EventArgs args)
        {
            var handler = accessorySelected;
            if (handler != null)
            {
                handler(Pair ?? this, EventArgs.Empty);
            }
            else
            {
                iApp.Navigate(AccessoryLink, this.GetSuperview<MonoCross.Navigation.IMXView>());
            }
        }
    }
}

