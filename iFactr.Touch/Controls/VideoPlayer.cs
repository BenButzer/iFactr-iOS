using System;
using System.IO;

using Foundation;
using MediaPlayer;

using MonoCross.Utilities;

namespace iFactr.Touch
{
	public class VideoPlayer
	{
		public static void Play(string url)
		{
			bool autoplay = true;
			var parameters = HttpUtility.ParseQueryString(url.Substring(url.IndexOf('?')));
			if (parameters != null)
			{
				parameters.TryGetValue("source", out url);

				string play = null;
				parameters.TryGetValue("autoplay", out play);
				if (play != null)
				{
					bool.TryParse(play, out autoplay);
				}
			}

			if (url == null)
			{
				throw new ArgumentNullException("source", "Video playback requires a source parameter to be specified");
			}

			var controller = new MPMoviePlayerViewController(File.Exists(url) ? NSUrl.FromFilename(url) : NSUrl.FromString(url));
			controller.MoviePlayer.ControlStyle = MPMovieControlStyle.Fullscreen;
			controller.MoviePlayer.Fullscreen = true;
			controller.MoviePlayer.ShouldAutoplay = autoplay;

			TouchFactory.Instance.TopViewController.PresentMoviePlayerViewController(controller);
		}
	}
}

