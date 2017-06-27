using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using CoreGraphics;
using UIKit;

namespace iFactr.Touch
{
    public static class UrlExtensions
    {
        public static bool IsExternal(this NSUrl url)
        {
            return (url.Host == "itunes.apple.com")
                || (url.Host == "phobos.apple.com")
                || (url.Host == "maps.google.com")
                || (url.Scheme == "itms-services")
                || (url.Scheme == "itms")
                || (url.Scheme == "tel")
                || (url.Scheme == "read");
        }
    }
}
