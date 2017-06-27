using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using iFactr.UI;

using CoreGraphics;
using Foundation;
using UIKit;

namespace iFactr.Touch
{
    public static class TouchStyle
    {
        public static CGColor ToCGColor(this Color color)
        {
            return new CGColor(((float)color.R) / 255, 
                               ((float)color.G) / 255, 
                               ((float)color.B) / 255,
                               ((float)color.A) / 255);
        }

        public static Color ToColor(this CGColor cgcolor)
        {
            if (cgcolor == null)
                return new Color();
            
            nfloat a, r, g, b;
            UIColor.FromCGColor(cgcolor).GetRGBA(out r, out g, out b, out a);

            return new Color((byte)(a * 255), (byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
        }

        public static UIColor ToUIColor(this Color color)
        {
            return new UIColor(((nfloat)color.R) / 255, 
			                   ((nfloat)color.G) / 255, 
			                   ((nfloat)color.B) / 255,
			                   ((nfloat)color.A) / 255);
        }

        public static Color ToColor(this UIColor uicolor)
        {
            if (uicolor == null)
                return new Color(0, 0, 0, 0);

            nfloat a, r, g, b;
            uicolor.GetRGBA(out r, out g, out b, out a);

            return new Color((byte)(a * 255), (byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
        }
		
		public static bool IsDefaultColor(this UIColor color)
		{
            return color == null ? true : color.CGColor.Components.All(c => c == 0);
		}

        public static nfloat Brightness(this UIColor color)
        {
            if (color == null)
                return 0;
            
            nfloat red, green, blue, alpha;
            color.GetRGBA(out red, out green, out blue, out alpha);
            return (red / 1) * 0.3f + (green / 1) * 0.59f + (blue / 1) * 0.11f;
        }

        public static UIFont ToUIFont(this Font font)
        {
			List<string> names = font.Name == null ? new List<string>() { UIFont.SystemFontOfSize(UIFont.SystemFontSize).Name }
                : UIFont.FontNamesForFamilyName(font.Name).ToList();

            string name = null;
            try
            {
                if ((font.Formatting & FontFormatting.Bold) != 0)
                {
                    if ((font.Formatting & FontFormatting.Italic) != 0)
                    {
                        name = names.FirstOrDefault(n => n.Contains("-BoldItalic") || n.Contains("-BoldOblique"));
                    }
                    else
                    {
                        name = names.FirstOrDefault(n => n.EndsWith("-Bold")) ?? names.FirstOrDefault(n => n.Contains("-Bold"));
                    }
                }
                else if ((font.Formatting & FontFormatting.Italic) != 0)
                {
                    name = names.FirstOrDefault(n => n.Contains("-Italic") || n.Contains("-Oblique"));
                }

                if (name == null)
                {
                    name = names.FirstOrDefault(n => !n.Contains('-') || n.Contains("-Regular") || n.EndsWith("Medium"));
                }
            }
            catch
            {
            }

            if (name == null)
            {
                if ((font.Formatting & FontFormatting.Bold) != 0)
                    return UIFont.BoldSystemFontOfSize((float)font.Size);
                if ((font.Formatting & FontFormatting.Italic) != 0)
                    return UIFont.ItalicSystemFontOfSize((float)font.Size);
                return UIFont.SystemFontOfSize((float)font.Size);
            }

            return UIFont.FromName(name, (float)font.Size);
        }

        public static Font ToFont(this UIFont uifont)
        {
			var font = new Font(uifont.FamilyName, uifont.PointSize, FontFormatting.Normal);
			if (uifont.Name.Contains("-Bold"))
			{
				if (uifont.Name.Contains("-BoldItalic") || uifont.Name.Contains("-BoldOblique"))
				{
					font.Formatting = FontFormatting.BoldItalic;
				}
				else
				{
					font.Formatting = FontFormatting.Bold;
				}
			}
			else if (uifont.Name.Contains("-Italic") || uifont.Name.Contains("-Oblique"))
			{
				font.Formatting = FontFormatting.Italic;
			}
			
			return font;
        }

		public static UIImage ImageFromResource(string name)
		{
			UIImage img = UIImage.FromResource(null, UIScreen.MainScreen.Scale > 1 ? name.Insert(name.LastIndexOf('.'), "@2x") : name);
			return new UIImage(img.CGImage, UIScreen.MainScreen.Scale, UIImageOrientation.Up);
		}
    }
}
