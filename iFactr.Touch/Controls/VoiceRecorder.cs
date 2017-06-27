using System;
using System.Collections.Generic;
using System.IO;

using AudioToolbox;
using AVFoundation;
using Foundation;
using UIKit;

using iFactr.Core;
using MonoCross.Utilities;

namespace iFactr.Touch
{
	public class AudioPlayer
	{
        private static AVAudioRecorder audioRecorder;
        private static AVAudioPlayer audioPlayer;
        private static string callback;

		public static void Record (string url)
		{
			var parameters = HttpUtility.ParseQueryString (url.Substring (url.IndexOf ('?')));
			if (parameters != null)
			{
				if (parameters.ContainsKey ("callback"))
					callback = parameters ["callback"];
				else
					throw new ArgumentException ("Audio recording requires a callback URI.");
			}

			NSObject[] values = new NSObject[]
            {
                NSNumber.FromInt32((int)AudioFormatType.MPEG4AAC),
                NSNumber.FromInt32(2),
                NSNumber.FromInt32((int)AVAudioQuality.Max)
            };

            NSObject[] keys = new NSObject[]
            {
                AVAudioSettings.AVFormatIDKey,
                AVAudioSettings.AVNumberOfChannelsKey,
                AVAudioSettings.AVEncoderAudioQualityKey
            };

            NSDictionary settings = NSDictionary.FromObjectsAndKeys (values, keys);

            string audioFilePath = Path.Combine(TouchFactory.Instance.TempPath, Guid.NewGuid().ToString() + ".aac");

            NSError error = null;
            audioRecorder = AVAudioRecorder.Create(NSUrl.FromFilename(audioFilePath), new AudioSettings(settings), out error);

            var actionSheet = new UIActionSheet (string.Empty)
			{
				TouchFactory.Instance.GetResourceString("RecordAudio"), 
				TouchFactory.Instance.GetResourceString("Cancel"),
			};
            actionSheet.CancelButtonIndex = 1;
            actionSheet.Style = UIActionSheetStyle.BlackTranslucent;
            actionSheet.ShowInView(TouchFactory.Instance.TopViewController.View);
            actionSheet.Clicked += delegate (object sender, UIButtonEventArgs args)
            {
                switch (args.ButtonIndex)
                {
                    case 0:
                        StartRecording();
                        break;
                }
            };
		}

        public static void Play (string url)
		{
			string command = string.Empty;
			string source = string.Empty;

			var parameters = HttpUtility.ParseQueryString (url.Substring (url.IndexOf ('?')));
			if (parameters != null)
			{
				parameters.TryGetValue ("command", out command);
				parameters.TryGetValue ("source", out source);
			}

			if (!string.IsNullOrEmpty (source))
			{
				if (audioPlayer != null)
				{
					audioPlayer.Stop();
				}

				audioPlayer = AVAudioPlayer.FromUrl(NSUrl.FromFilename(source));
			}

			if (audioPlayer != null)
			{
				if (command == "stop")
				{
					audioPlayer.Stop();
				}
				else if (command == "pause" && audioPlayer.Playing)
				{
					audioPlayer.Pause();
				}
				else
				{
					audioPlayer.Play();
				}
			}
        }

        private static void StartRecording()
        {
            audioRecorder.PrepareToRecord();
            audioRecorder.Record();
            audioPlayer = null;

			var actionSheet = new UIActionSheet (TouchFactory.Instance.GetResourceString("Recording")) 
			{
				TouchFactory.Instance.GetResourceString("StopRecording"),
			};
			actionSheet.Style = UIActionSheetStyle.BlackTranslucent;
			actionSheet.ShowInView(TouchFactory.Instance.TopViewController.View); 
			actionSheet.Dismissed += delegate (object sender, UIButtonEventArgs args)
            {
				DoneRecording();
			};
		}

		private static void DoneRecording()
		{
			audioRecorder.Stop();

			string path = Path.GetFileName(audioRecorder.Url.AbsoluteString);
            var parameters = new Dictionary<string, string>() { { "AudioId", path } };
            iApp.Navigate(callback, parameters);
		}
	}
}
