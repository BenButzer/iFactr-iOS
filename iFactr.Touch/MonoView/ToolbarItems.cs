using System;
using System.ComponentModel;
using UIKit;
using iFactr.Core;
using iFactr.UI;

namespace iFactr.Touch
{
	public class ToolbarButton : UIBarButtonItem, IToolbarButton, INotifyPropertyChanged
	{
		public new event EventHandler Clicked;

        public event PropertyChangedEventHandler PropertyChanged;

		public Color ForegroundColor
		{
			get { return base.TintColor.ToColor(); }
            set
            {
                if (value != base.TintColor.ToColor())
                {
                    base.TintColor = value.ToUIColor();

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("ForegroundColor"));
                    }
                }
            }
		}

		public string ImagePath
		{
			get { return imagePath; }
			set
			{
                if (value != imagePath)
                {
    				imagePath = value;
    				base.SetBackgroundImage(string.IsNullOrWhiteSpace(imagePath) ? null : UIImage.FromFile(imagePath), UIControlState.Normal, UIBarMetrics.Default);

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("ImagePath"));
                    }
                }
			}
		}
		private string imagePath;

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

        public override string Title
        {
            get { return base.Title; }
            set
            {
                if (value != base.Title)
                {
                    base.Title = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("Title"));
                    }
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

		public ToolbarButton()
			: base(string.Empty, UIBarButtonItemStyle.Plain, null)
		{
			Enabled = true;

			base.Clicked += (o, e) =>
			{
				var handler = this.Clicked;
				if (handler == null)
				{
					iApp.Navigate(NavigationLink, this.GetSuperview() as MonoCross.Navigation.IMXView);
				}
				else
				{
					handler(Pair ?? this, EventArgs.Empty);
				}
			};
		}
		
		public bool Equals(IToolbarButton other)
		{
			var button = other as iFactr.UI.ToolbarButton;
			if (button != null)
			{
				return button.Equals(this);
			}
			
			return base.Equals(other);
		}
	}

	public class ToolbarSeparator : UIBarButtonItem, IToolbarSeparator, INotifyPropertyChanged
	{
        public event PropertyChangedEventHandler PropertyChanged;

		public Color ForegroundColor
		{
			get { return GetTitleTextAttributes(UIControlState.Normal).TextColor.ToColor(); }
            set
            {
                if (value != GetTitleTextAttributes(UIControlState.Normal).TextColor.ToColor())
                {
                    SetTitleTextAttributes(new UITextAttributes() { TextColor = value.ToUIColor() }, UIControlState.Normal);

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("ForegroundColor"));
                    }
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

		public ToolbarSeparator()
			: base("|", UIBarButtonItemStyle.Plain, null)
		{
            Enabled = false;
			Width = 10;
		}
		
		public bool Equals(IToolbarSeparator other)
		{
			var item = other as iFactr.UI.ToolbarSeparator;
			if (item != null)
			{
				return item.Equals(this);
			}
			
			return base.Equals(other);
		}
	}
}

