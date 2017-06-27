using System;
using System.IO;
using System.Text;
using System.Xml;

using iFactr.Core.Targets.Settings;

using Foundation;

namespace iFactr.Touch
{
	public class TouchSettingsDictionary : SettingsDictionary
	{
		public TouchSettingsDictionary()
        {
            string path = Path.Combine(TouchFactory.Instance.DataPath, "appSettings");
            if (TouchFactory.Instance.File.Exists(path))
            {
                using (var reader = new XmlTextReader(path))
                {
                    // an exception is thrown if the root isn't read before ReadXml is called
                    reader.Read();
                    ReadXml(reader);
                }
            }

            // user defaults and the info.plist should overwrite what's in the file
            Load();
        }

		public override void Clear ()
		{
			base.Clear ();
			Load ();
		}
		
		public override void Load ()
		{
            var prefs = NSUserDefaults.StandardUserDefaults.ToDictionary();
            foreach ( var item in prefs )
            {
                this[item.Key.ToString()] = item.Value.ToString();
            }

            foreach (var item in NSBundle.MainBundle.InfoDictionary)
            {
                if (item.Value is NSArray)
                {
                    string key = item.Key.ToString();

                    this[key] = string.Empty;

                    NSArray array = (NSArray)item.Value;
                    for (uint i = 0; i < array.Count; i++)
                    {
                        this[key] += new NSObject(array.ValueAt(i)).ToString();
                        if (i < array.Count - 1)
                        {
                            this[key] += "|";
                        }
                    }
                }
                else
                {
                    this[item.Key.ToString()] = item.Value.ToString();
                }
            }
        }
		
		public override void Store ()
        {
            using (var writer = new XmlTextWriter(Path.Combine(TouchFactory.Instance.DataPath, "appSettings"), Encoding.UTF8))
            {
                // need a root element or reading it will throw an exception
                writer.WriteStartElement("settings");
                WriteXml(writer);
                writer.WriteEndElement();
            }
        }
	}
}

