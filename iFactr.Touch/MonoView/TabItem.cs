using System;
using System.ComponentModel;

using UIKit;
using Foundation;

using iFactr.UI;

namespace iFactr.Touch
{
    [Preserve(AllMembers = true)]
    public class TabItem : UITabBarItem, ITabItem, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #pragma warning disable 067
        public event EventHandler Selected;
        #pragma warning restore 067

        public string ImagePath
        {
            get { return imagePath; }
            set
            {
                if (value != imagePath)
                {
                    imagePath = value;
                    Image = UIImage.FromBundle(imagePath);

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

        public Color TitleColor
        {
            get { return GetTitleTextAttributes(UIControlState.Normal).TextColor.ToColor(); }
            set
            {
                if (value != GetTitleTextAttributes(UIControlState.Normal).TextColor.ToColor())
                {
                    SetTitleTextAttributes(new UITextAttributes()
                    {
                        Font = GetTitleTextAttributes(UIControlState.Normal).Font,
                        TextColor = value.IsDefaultColor ? null : value.ToUIColor()
                    }, UIControlState.Normal);

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("TitleColor"));
                    }
                }
            }
        }
        
        public Font TitleFont
        {
            get { return GetTitleTextAttributes(UIControlState.Normal).Font.ToFont(); }
            set
            {
                var font = value.ToUIFont();
                if (font != GetTitleTextAttributes(UIControlState.Normal).Font)
                {
                    SetTitleTextAttributes(new UITextAttributes()
                    {
                        Font = value.ToUIFont(),
                        TextColor = GetTitleTextAttributes(UIControlState.Normal).TextColor
                    }, UIControlState.Normal);

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("TitleFont"));
                    }
                }
            }
        }

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

        public override string BadgeValue
        {
            get { return base.BadgeValue; }
            set
            {
                if (value != base.BadgeValue)
                {
                    base.BadgeValue = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("BadgeValue"));
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

        public bool Equals(ITabItem other)
        {
            var item = other as iFactr.UI.TabItem;
            if (item != null)
            {
                return item.Equals(this);
            }
            
            return base.Equals(other);
        }
    }
}

