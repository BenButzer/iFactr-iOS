using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using MonoCross;
using MonoCross.Utilities;

using CoreGraphics;
using Foundation;
using UIKit;
using ImageIO;

namespace iFactr.Touch
{
    public class ImageData : UIImage, IImageData
    {
        private NSDictionary info;
        private string filePath;

        public ImageData(CGImage image, nfloat scale, UIImageOrientation orientation, NSDictionary info)
            : base(image, scale, orientation)
        {
            this.info = info;
        }

        public ImageData(CGImage image, nfloat scale, UIImageOrientation orientation, string filePath)
            : base(image, scale, orientation)
        {
            this.filePath = filePath;
        }

        public byte[] GetBytes()
        {
            return AsPNG().ToArray();
        }

        public IExifData GetExifData()
        {
            if (info != null)
            {
                return new ExifData(info.ObjectForKey(UIImagePickerController.MediaMetadata) as NSDictionary);
            }

            if (filePath != null)
            {
                NSUrl url = System.IO.File.Exists(filePath) ? NSUrl.FromFilename(filePath) : NSUrl.FromString(filePath);
                if (url != null)
                {
                    CGImageSource source = CGImageSource.FromUrl(url);
                    if (source != null)
                    {
                        return new ExifData(source.CopyProperties((NSDictionary)null, 0));
                    }
                }
            }

            return new ExifData();
        }

        public void Save(string filePath, ImageFileFormat format)
        {
            if (format == ImageFileFormat.JPEG)
            {
                AsJPEG().Save(filePath, true);
            }
            else
            {
                AsPNG().Save(filePath, true);
            }
        }
    }

    public class ExifData : IExifData
    {
        public double Aperture { get; private set; }

        public int ColorSpace { get; private set; }

        public DateTime DateTime { get; private set; }

        public DateTime DateTimeDigitized { get; private set; }

        public DateTime DateTimeOriginal { get; private set; }

        public double DPIHeight { get; private set; }

        public double DPIWidth { get; private set; }

        public string ExposureProgram { get; private set; }

        public double ExposureTime { get; private set; }

        public int Flash { get; private set; }

        public double FNumber { get; private set; }

        public double FocalLength { get; private set; }

        public string Manufacturer { get; private set; }

        public string Model { get; private set; }

        public int Orientation { get; private set; }

        public double PixelHeight { get; private set; }

        public double PixelWidth { get; private set; }

        public double ShutterSpeed { get; private set; }

        public double XResolution { get; private set; }

        public double YResolution { get; private set; }

        private NSDictionary exifData;

        public ExifData()
        {
        }

        public ExifData(NSDictionary data)
        {
            exifData = data;
            ExtractExifData(data);
        }

        public IDictionary<string, object> GetRawData()
        {
            var raw = new Dictionary<string, object>();
            ExtractRawData(exifData, raw);
            return raw;
        }

        private void ExtractRawData(NSDictionary dictionary, Dictionary<string, object> raw)
        {
            if (dictionary == null)
            {
                return;
            }

            foreach (var key in dictionary.Keys)
            {
                var obj = dictionary.ObjectForKey(key);
                var dict = obj as NSDictionary;
                if (dict != null)
                {
                    ExtractRawData(dict, raw);
                    continue;
                }

                raw[key.Description] = obj;
            }
        }

        private void ExtractExifData(NSDictionary dictionary)
        {
            if (dictionary == null)
            {
                return;
            }

            foreach (var key in dictionary.Keys)
            {
                var obj = dictionary.ObjectForKey(key);
                var dict = obj as NSDictionary;
                if (dict != null)
                {
                    ExtractExifData(dict);
                    continue;
                }

                switch (key.Description)
                {
                    case "ApertureValue":
                        Aperture = obj.Description.TryParseDouble();
                        break;
                    case "ColorSpace":
                        ColorSpace = obj.Description.TryParseInt32();
                        break;
                    case "DateTime":
                        DateTime = DateTime.ParseExact(obj.Description, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture);
                        break;
                    case "DateTimeDigitized":
                        DateTimeDigitized = DateTime.ParseExact(obj.Description, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture);
                        break;
                    case "DateTimeOriginal":
                        DateTimeOriginal = DateTime.ParseExact(obj.Description, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture);
                        break;
                    case "DPIHeight":
                        DPIHeight = obj.Description.TryParseDouble();
                        break;
                    case "DPIWidth":
                        DPIWidth = obj.Description.TryParseDouble();
                        break;
                    case "ExposureProgram":
                        ExposureProgram = obj.Description;
                        break;
                    case "ExposureTime":
                        ExposureTime = obj.Description.TryParseDouble();
                        break;
                    case "Flash":
                        Flash = obj.Description.TryParseInt32();
                        break;
                    case "FNumber":
                        FNumber = obj.Description.TryParseDouble();
                        break;
                    case "FocalLength":
                        FocalLength = obj.Description.TryParseDouble();
                        break;
                    case "Make":
                        Manufacturer = obj.Description;
                        break;
                    case "Model":
                        Model = obj.Description;
                        break;
                    case "Orientation":
                        Orientation = obj.Description.TryParseInt32();
                        break;
                    case "PixelWidth":
                    case "PixelXDimension":
                        PixelWidth = obj.Description.TryParseDouble();
                        break;
                    case "PixelHeight":
                    case "PixelYDimension":
                        PixelHeight = obj.Description.TryParseDouble();
                        break;
                    case "ShutterSpeedValue":
                        ShutterSpeed = obj.Description.TryParseDouble();
                        break;
                    case "XResolution":
                        XResolution = obj.Description.TryParseDouble();
                        break;
                    case "YResolution":
                        YResolution = obj.Description.TryParseDouble();
                        break;
                }
            }
        }
    }
}

