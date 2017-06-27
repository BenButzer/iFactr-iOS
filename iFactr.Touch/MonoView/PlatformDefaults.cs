using System;

using UIKit;

using iFactr.UI;

namespace iFactr.Touch
{
    public class PlatformDefaults : IPlatformDefaults
    {
        public double LargeHorizontalSpacing
        {
            get { return 10; }
        }

        public double LeftMargin
        {
            get { return 15; }
        }

        public double RightMargin
        {
            get { return 15; }
        }

        public double SmallHorizontalSpacing
        {
            get { return 4; }
        }

        public double BottomMargin
        {
            get { return 4; }
        }

        public double LargeVerticalSpacing
        {
            get { return 10; }
        }

        public double SmallVerticalSpacing
        {
            get { return 4; }
        }

        public double TopMargin
        {
            get { return 4; }
        }

        public double CellHeight
        {
            get { return 44; }
        }

        public Font ButtonFont
        {
            get { return LabelFont; }
        }

        public Font DateTimePickerFont
        {
            get { return UIFont.SystemFontOfSize(18).ToFont(); }
        }

        public Font HeaderFont
        {
            get { return LabelFont; }
        }

        public Font LabelFont
        {
            get { return (UIDevice.CurrentDevice.CheckSystemVersion(7, 0) ? UIFont.PreferredBody : UIFont.BoldSystemFontOfSize(18)).ToFont(); }
        }

        public Font MessageBodyFont
        {
            get { return SmallFont + 2; }
        }

        public Font MessageTitleFont
        {
            get { return (UIDevice.CurrentDevice.CheckSystemVersion(7, 0) ? UIFont.FromName("HelveticaNeue-Medium", 17) : UIFont.BoldSystemFontOfSize(18)).ToFont(); }
        }

        public Font SectionHeaderFont
        {
            get { return (UIDevice.CurrentDevice.CheckSystemVersion(7, 0) ? UIFont.SystemFontOfSize(13) : UIFont.BoldSystemFontOfSize(17)).ToFont(); }
        }

        public Font SectionFooterFont
        {
            get { return (UIDevice.CurrentDevice.CheckSystemVersion(7, 0) ? UIFont.SystemFontOfSize(13) : UIFont.BoldSystemFontOfSize(17)).ToFont(); }
        }

        public Font SelectListFont
        {
            get { return UIFont.SystemFontOfSize(18).ToFont(); }
        }

        public Font SmallFont
        {
            get { return (UIDevice.CurrentDevice.CheckSystemVersion(7, 0) ? UIFont.PreferredFootnote : UIFont.SystemFontOfSize(14)).ToFont(); }
        }

        public Font TabFont
        {
            get { return (UIDevice.CurrentDevice.CheckSystemVersion(7, 0) ? UIFont.SystemFontOfSize(10) : UIFont.BoldSystemFontOfSize(10)).ToFont(); }
        }

        public Font TextBoxFont
        {
            get { return UIFont.SystemFontOfSize(17).ToFont(); }
        }

        public Font ValueFont
        {
            get { return LabelFont; }
        }
    }
}

