using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using UIKit;

using iFactr.Core;
using iFactr.UI;

namespace iFactr.Touch
{
    public class Toolbar : UIToolbar, IToolbar, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;   

		public new Color BackgroundColor
		{
			get { return base.BarTintColor.ToColor(); }
			set
            {
                if (value != base.BarTintColor.ToColor())
                {
                    base.BarTintColor = value.IsDefaultColor ? null : value.ToUIColor();

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
			get { return foregroundColor.ToColor(); }
			set
			{
                if (value != foregroundColor.ToColor())
                {
                    var uicolor = value.IsDefaultColor ? null : value.ToUIColor();
    				SetColor(uicolor);
                    foregroundColor = uicolor;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("ForegroundColor"));
                    }
                }
			}
		}
		private UIColor foregroundColor;

		public IEnumerable<IToolbarItem> PrimaryItems
		{
			get
			{
				if (Items == null)
					yield break;

				int index = 0;
				for (int i = 0; i < Items.Length; i++)
				{
					var item = Items[i];
					if (item == flexibleSpace)
					{
						index = i;
					}
					else if (i > index)
					{
						var ti = item as IToolbarItem;
						if (ti != null)
						{
							yield return (ti.Pair as IToolbarItem) ?? ti;
						}
					}
				}
			}
			set
			{
				var array = new UIBarButtonItem[value == null ? 0 : value.Count() + SecondaryItems.Count() + 1];
				int index = 0;
				foreach (var item in SecondaryItems)
				{
					array[index++] = TouchFactory.GetNativeObject<UIBarButtonItem>(item, "toolbarItem");
				}

				array[index++] = flexibleSpace;

				if (value != null)
				{
					foreach (var item in value)
					{
						array[index++] = TouchFactory.GetNativeObject<UIBarButtonItem>(item, "toolbarItem");
					}
				}

				Items = array;
				SetColor(foregroundColor);

                var handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs("PrimaryItems"));
                }
			}
		}
		
		public IEnumerable<IToolbarItem> SecondaryItems
		{
			get
			{
				if (Items == null)
					yield break;

				for (int i = 0; i < Items.Length; i++)
				{
					var item = Items[i];
					if (item == flexibleSpace)
					{
						break;
					}

					var ti = item as IToolbarItem;
					if (ti != null)
					{
						yield return (ti.Pair as IToolbarItem) ?? ti;
					}
				}
			}
			set
			{
				var array = new UIBarButtonItem[value == null ? 0 : value.Count() + PrimaryItems.Count() + 1];
				int index = 0;
				
				if (value != null)
				{
					foreach (var item in value)
					{
						array[index++] = TouchFactory.GetNativeObject<UIBarButtonItem>(item, "toolbarItem");
					}
				}

				array[index++] = flexibleSpace;

				foreach (var item in PrimaryItems)
				{
					array[index++] = TouchFactory.GetNativeObject<UIBarButtonItem>(item, "toolbarItem");
				}

				Items = array;
				SetColor(foregroundColor);

                var handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs("SecondaryItems"));
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

		private UIBarButtonItem flexibleSpace;

		public Toolbar()
		{
			AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleTopMargin;

			flexibleSpace = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);
		}

		public bool Equals(IToolbar other)
		{
			var toolbar = other as iFactr.UI.Toolbar;
			if (toolbar != null)
			{
				return toolbar.Equals(this);
			}
			
			return base.Equals(other);
		}

        private void SetColor(UIColor color)
		{
			if (base.Items != null)
			{
				foreach (var item in base.Items)
				{
                    if (item is IToolbarItem && (item.TintColor.IsDefaultColor() || item.TintColor == foregroundColor))
					{
						item.TintColor = color;
					}
				}
			}
		}
	}
}

