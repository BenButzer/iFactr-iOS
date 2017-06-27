using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using CoreGraphics;
using Foundation;
using UIKit;

using iFactr.Core;
using MonoCross.Utilities;

namespace iFactr.Touch
{
	public class ImagePicker
	{
		private const string Camera = "camera";
		private const string Gallery = "gallery";
		private const string CallbackUri = "callback";

		private static UIImagePickerController picker;
        private static string callback;

        static ImagePicker()
        {
            picker = new UIImagePickerController();
        }

		public static void GetMedia(string url)
		{
            bool cameraEnabled = true;
            bool galleryEnabled = true;

			var parameters = HttpUtility.ParseQueryString(url.Substring(url.IndexOf('?')));
			if (parameters != null)
			{
				if (parameters.ContainsKey(CallbackUri))
					callback = parameters[CallbackUri];
                else
                    throw new ArgumentException("Image capture requires a callback URI.");
                
				if (parameters.ContainsKey(Camera))
					bool.TryParse(parameters[Camera], out cameraEnabled);
                
				if (parameters.ContainsKey(Gallery))
					bool.TryParse(parameters[Gallery], out galleryEnabled);
			}

            if (UIImagePickerController.IsSourceTypeAvailable(UIImagePickerControllerSourceType.Camera))
            {
                // this must be set to camera before capture mode can be set
                picker.SourceType = UIImagePickerControllerSourceType.Camera;
            }
            else
            {
                cameraEnabled = false;
            }

            if (!UIImagePickerController.IsSourceTypeAvailable(UIImagePickerControllerSourceType.PhotoLibrary))
                galleryEnabled = false;

            picker.Delegate = new ImagePickerDelegate();

            string[] buttons = null;
            if (url.StartsWith("videorecording"))
            {
                buttons = new string[]
				{
					TouchFactory.Instance.GetResourceString("RecordVideo"),
					TouchFactory.Instance.GetResourceString("ChooseVideo"),
				};
                picker.MediaTypes = new string[] { MobileCoreServices.UTType.Movie };

                if (cameraEnabled)
                    picker.CameraCaptureMode = UIImagePickerControllerCameraCaptureMode.Video;
            }
            else
            {
				buttons = new string[]
				{
					TouchFactory.Instance.GetResourceString("TakePhoto"),
					TouchFactory.Instance.GetResourceString("ChoosePhoto"),
				};
                picker.MediaTypes = new string[] { MobileCoreServices.UTType.Image };

                if (cameraEnabled)
                    picker.CameraCaptureMode = UIImagePickerControllerCameraCaptureMode.Photo;
            }

			if (cameraEnabled)
			{
                if (galleryEnabled)
                {
                    var alert = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);
                    if (alert == null)
                    {
                        var actionSheet = new UIActionSheet (string.Empty)
                        {
                            buttons[0],
                            buttons[1],
                            TouchFactory.Instance.GetResourceString("Cancel"),
                        };
                        actionSheet.CancelButtonIndex = actionSheet.ButtonCount - 1;
                        actionSheet.Style = UIActionSheetStyle.BlackTranslucent;
                        actionSheet.ShowInView(TouchFactory.Instance.TopViewController.View); 
                        actionSheet.Clicked += delegate (object sender, UIButtonEventArgs args)
                        {
                            switch (args.ButtonIndex)
                            {
                                case 0:
                                    StartCamera();
                                    break;
                                case 1:
                                    StartGallery();
                                    break;
                            }
                        };
                    }
                    else
                    {
                        if (alert.PopoverPresentationController != null)
                        {
                            alert.PopoverPresentationController.PermittedArrowDirections = 0;
                            alert.PopoverPresentationController.SourceView = TouchFactory.Instance.TopViewController.View;
                            alert.PopoverPresentationController.SourceRect = new CGRect(TouchFactory.Instance.TopViewController.View.Center, CGSize.Empty);
                        }
                        alert.AddAction(UIAlertAction.Create(buttons[0], UIAlertActionStyle.Default, (o) => StartCamera()));
                        alert.AddAction(UIAlertAction.Create(buttons[1], UIAlertActionStyle.Default, (o) => StartGallery()));
                        alert.AddAction(UIAlertAction.Create(TouchFactory.Instance.GetResourceString("Cancel"), UIAlertActionStyle.Cancel, null));
                        ModalManager.EnqueueModalTransition(ModalManager.GetTopmostViewController(null), alert, true);
                    }
        }
				else 
                {
                    StartCamera();
                }
			}
			else if (galleryEnabled)
            {
                StartGallery();
            }
		}
        
        private static void StartCamera()
        {
            picker.SourceType = UIImagePickerControllerSourceType.Camera;
            PresentPicker();
        }
        
        private static void StartGallery()
        {
            picker.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;
            PresentPicker();
        }
        
        private static void PresentPicker()
        {
            picker.AllowsEditing = true;
            ModalManager.EnqueueModalTransition(TouchFactory.Instance.TopViewController, picker, true);;
        }

        private class ImagePickerDelegate : UIImagePickerControllerDelegate
        {
            public ImagePickerDelegate()
            {
            }

            public override void FinishedPickingMedia (UIImagePickerController picker, NSDictionary info)
            {
                ModalManager.EnqueueModalTransition(picker.PresentingViewController, null, true);

                Dictionary<string, string> parameters;
                NSString mediaType = (NSString)info.ObjectForKey(UIImagePickerController.MediaType);

                iApp.Factory.BeginBlockingUserInput();
                iApp.Factory.ActivateLoadTimer();
                if (mediaType == MobileCoreServices.UTType.Image)
                {
                    ThreadPool.QueueUserWorkItem((image) =>
                    {
                        using (new NSAutoreleasePool())
                        {
                            string path = TouchFactory.Instance.StoreImage(((UIImage)image).AsPNG());
                            InvokeOnMainThread(() =>
                            { 
                                parameters = new Dictionary<string, string>() { { "PhotoImage", path } };
                                iApp.Navigate(callback, parameters);
                            });
                        }
                    }, info.ObjectForKey(UIImagePickerController.EditedImage));
                }
                else if (mediaType == MobileCoreServices.UTType.Movie)
                {
                    NSUrl url = info.ObjectForKey(UIImagePickerController.MediaURL) as NSUrl;
                    string path = url.AbsoluteString.Remove(0, url.AbsoluteString.IndexOf(TouchFactory.Instance.TempPath) + TouchFactory.Instance.TempPath.Length);
                    parameters = new Dictionary<string, string>() { { "VideoId", path } };
                    iApp.Navigate(callback, parameters);
                }
            }
        }
	}
}