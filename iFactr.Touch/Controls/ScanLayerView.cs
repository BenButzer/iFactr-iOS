using System;
using System.Collections.Generic;
using System.Threading;

using CoreGraphics;
using UIKit;
using Foundation;
using AVFoundation;

using iFactr.Core;
using MonoCross.Utilities;
using iFactr.Core.Layers;
using iFactr.Core.Controls;
using iFactr.Core.Native;
using iFactr.Core.Targets;
using iFactr.UI;

using Link = iFactr.UI.Link;

namespace iFactr.Touch
{
	public class ScannerView
	{
		public static void Scan(string uri)
        {
            var parameters = HttpUtility.ParseQueryString(uri.Substring(uri.IndexOf('?')));
            if (parameters == null || !parameters.ContainsKey("callback"))
            {
                throw new ArgumentException("Scanner requires a callback URI.");
            }
            else
            {
                uri = parameters["callback"];
            }

			string scanCompleteText = iApp.Factory.GetResourceString("Done");
			Scan(new ScanLayer(null, scanCompleteText, uri));
		}
		public static void Scan(ScanLayer scanLayer)
        {
            ScanController controller = new ScanController(scanLayer);
            var navController = new PopoverNavigationController(controller) { ModalPresentationStyle = UIModalPresentationStyle.FormSheet };
            navController.SetCloseButton(controller);

			if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
			{
				if (!TouchFactory.TheApp.Style.HeaderColor.IsDefaultColor)
				{
					navController.NavigationBar.BarTintColor = TouchFactory.TheApp.Style.HeaderColor.ToUIColor();
				}

				if (!TouchFactory.TheApp.Style.HeaderTextColor.IsDefaultColor)
				{
					navController.NavigationBar.TintColor = TouchFactory.TheApp.Style.HeaderTextColor.ToUIColor();
                    navController.NavigationBar.TitleTextAttributes = new UIStringAttributes() { ForegroundColor = TouchFactory.TheApp.Style.HeaderTextColor.ToUIColor() };
				}
			}
			else
			{
				navController.NavigationBar.TintColor = TouchFactory.TheApp.Style.HeaderColor.ToUIColor();
			}

            ModalManager.EnqueueModalTransition(TouchFactory.Instance.TopViewController, navController, true);
		}

		private class ScanController : UITableViewController
		{
            private ScanLayer _layer;
            private Font _layerFont;
			private UITextView textView;
			private Link callback;
			private string barcodeKey;
			private string barcodeSeparatorChar;
			private string completeButtonText;
			private Color backgroundColor;
			private List<string> barcodesScanned;
            List<string> _displayValues;

			public ScanController(ScanLayer scanLayer)
			{
                _layer = scanLayer;
                string fontName = null;
                double fontSize = 12;
                if (_layer.LayerStyle != null && _layer.LayerStyle.DefaultLabelStyle != null)
                {
                    fontName = _layer.LayerStyle.DefaultLabelStyle.FontFamily;
                    fontSize = _layer.LayerStyle.DefaultLabelStyle.FontSize;
                }                
                if (!string.IsNullOrEmpty(fontName))
                {
                    float size;
                    if (!float.TryParse(fontSize.ToString(), out size))
                    {
                        TouchFactory.Instance.Logger.Warn("Could not parse " + fontSize + "(double) to a float value");
                        size = 12;
                    }
                    _layerFont = TouchStyle.ToFont(UIFont.FromName(fontName, size));
                }

				callback = scanLayer.Callback;
				barcodeKey = ScanLayer.BarcodeKey;
				completeButtonText = scanLayer.Callback.Text ?? iApp.Factory.GetResourceString("Done");
				backgroundColor = _layer.LayerStyle.LayerBackgroundColor;
				barcodeSeparatorChar = ScanLayer.BarcodeSeparatorChar;
				ModalPresentationStyle = UIModalPresentationStyle.FormSheet;
			}

			public override void ViewDidLoad()
			{
				base.ViewDidLoad();
				textView = new UITextView()
				{
					AutocapitalizationType = UITextAutocapitalizationType.None,
					AutocorrectionType = UITextAutocorrectionType.No,
					InputView = new UIView(),
				};
				textView.Changed += TextViewChanged;
				View.AddSubview(textView);

				NavigationItem.RightBarButtonItem = new UIBarButtonItem(completeButtonText, UIBarButtonItemStyle.Done, delegate
				{
					ModalManager.EnqueueModalTransition(NavigationController.PresentingViewController, null, true);

                    string barcode = string.Join(barcodeSeparatorChar, barcodesScanned);
					if (barcodesScanned.Count > 0) 
					{
						// append separator char to end
						barcode += barcodeSeparatorChar;
					}

					
					if (callback.Parameters == null) 
				    {
						callback.Parameters = new Dictionary<string, string>(); 
					}
                    if (_layer.ActionParameters != null) { callback.Parameters.AddRange(_layer.ActionParameters); }
					callback.Parameters[barcodeKey] = barcode;
					iApp.Navigate(callback);					
				});
				
			
				TableView.BackgroundColor = backgroundColor.IsDefaultColor ? UIColor.White : backgroundColor.ToUIColor();
			}

			public override void ViewWillAppear(bool animated)
			{
				base.ViewWillAppear(animated);
				
				barcodesScanned = new List<string>();
                _displayValues = new List<string>();
                TableView.DataSource = new ScanControllerDataSource(_displayValues, _layerFont);

				textView.BecomeFirstResponder();
			}

			private void TextViewChanged(object sender, EventArgs e)
			{
				var contents = textView.Text;
				string lastChar = contents.Substring(contents.Length-1, 1);
				if (lastChar == barcodeSeparatorChar)
				{
					string capturedBarcode = contents.Substring(0, contents.Length-1);
                    var displayValue = _layer.ScannedBarcode(capturedBarcode);
                    
                    _displayValues.Add(displayValue);
                    barcodesScanned.Add(capturedBarcode);
					
					textView.Text = string.Empty;
					TableView.ReloadData();
					var indexPath = NSIndexPath.FromRowSection(TableView.NumberOfRowsInSection(0) - 1, 0);
					TableView.ScrollToRow(indexPath, UITableViewScrollPosition.Bottom, true);
				}
			}

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                return 44;
            }
		}
	
		private class ScanControllerDataSource : UITableViewDataSource
		{
            Font _layerFont;
            
			private List<string> _scannedBarcodes;
            public ScanControllerDataSource(List<string> scannedBarcodes, Font font) : base()
			{
                _layerFont = font;
				if (scannedBarcodes == null) throw new ArgumentNullException();
				_scannedBarcodes = scannedBarcodes;
			}
			
			public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
			{
				//TODO: Dequeue reusable cell instead
				var cell = new UITableViewCell(UITableViewCellStyle.Default, "ScanLayerCell");
				
				if (indexPath.Row < _scannedBarcodes.Count)
				{
					cell.TextLabel.Text = _scannedBarcodes[indexPath.Row];
                    cell.TextLabel.Font = _layerFont.ToUIFont();
                    cell.TextLabel.AdjustsFontSizeToFitWidth = true;
				}
				else
				{
                    //TODO: Move the string to the resource file
                    cell.TextLabel.Text = "error reading code";
                    //TODO: Read from view style
                    cell.TextLabel.TextColor = UIColor.Red;
                    cell.TextLabel.TextAlignment = UITextAlignment.Center;
                    cell.TextLabel.Font = UIFont.ItalicSystemFontOfSize(10f);
				}
				return cell;
			}
			
			public override nint RowsInSection(UITableView tableView, nint section)
			{
				return _scannedBarcodes.Count;
			}
	
		}
	}
	
	public class CameraScannerView
	{
		public static void Scan(CameraScanLayer scanLayer)
		{
			var callback = scanLayer.Callback;
			var ctrl = new CameraScanController(scanLayer, callback, ScanLayer.BarcodeKey) { ModalTransitionStyle = UIModalTransitionStyle.FlipHorizontal };
			var navCtrl = new PopoverNavigationController(ctrl);
            navCtrl.SetCloseButton(ctrl);

			if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
			{
				if (!TouchFactory.TheApp.Style.HeaderColor.IsDefaultColor)
				{
					navCtrl.NavigationBar.BarTintColor = scanLayer.LayerStyle.HeaderColor.ToUIColor();
				}

				if (!TouchFactory.TheApp.Style.HeaderTextColor.IsDefaultColor)
				{
					navCtrl.NavigationBar.TintColor = scanLayer.LayerStyle.HeaderTextColor.ToUIColor();
                    navCtrl.NavigationBar.TitleTextAttributes = new UIStringAttributes() { ForegroundColor = scanLayer.LayerStyle.HeaderTextColor.ToUIColor() };
				}
			}
			else
			{
				navCtrl.NavigationBar.TintColor = scanLayer.LayerStyle.HeaderColor.ToUIColor();
			}						

			ModalManager.EnqueueModalTransition(TouchFactory.Instance.TopViewController, navCtrl, true);
		}
	
		private class CameraScanController : ViewController
		{
            private CameraScanLayer _layer;
            private Font _layerFont;
			private Link _callback;
			private string _barcodeValueKey;
            private AVCaptureSession _captureSession;
            private UIImageView _imageOverlay;
            private UIImageView _imageOverlayScanBlocked;
            private LastValueScanOverlay _lastScanOverlay;
            private AVCaptureVideoPreviewLayer _videoPreviewLayer;
            public int DuplicateWait = 1500;

            public CameraScanController(CameraScanLayer layer, Link callback, string parametersKey)
			{				 
                _layer = layer;
				_callback = callback;
				_barcodeValueKey = parametersKey;
                DuplicateWait = layer.DuplicateTimeout;
//				Autorotate = TouchFactory.Instance.Platform == MobilePlatform.iPad;
				ModalPresentationStyle = UIModalPresentationStyle.FormSheet;

                string fontName = null;
                double fontSize = 12;
                if (_layer.LayerStyle != null && _layer.LayerStyle.DefaultLabelStyle != null)
                {
                    fontName = _layer.LayerStyle.DefaultLabelStyle.FontFamily;
                    fontSize = _layer.LayerStyle.DefaultLabelStyle.FontSize;
                }                
                if (!string.IsNullOrEmpty(fontName))
                {
                    _layerFont = TouchStyle.ToFont(UIFont.FromName(fontName, LastValueScanOverlay.LabelFontSize));
                }
			}

            public void ScanOccurred(string barcode, int occurrences)
            {
                _imageOverlay.Hidden = true;
                _imageOverlayScanBlocked.Hidden = false;

                Device.Thread.QueueWorker((o) => {
                    var wait = 0;
                    if (DuplicateWait > 50) { wait = DuplicateWait - 100; }
                    else { wait = DuplicateWait; }

                    new ManualResetEvent(false).WaitOne(wait);
                    Device.Thread.ExecuteOnMainThread(() => {
                        _imageOverlayScanBlocked.Hidden = true;
                        _imageOverlay.Hidden = false;
                    });
                });

                _lastScanOverlay.SetBarcodeLabel(barcode, occurrences);
            }

			public override void ViewDidLoad()
			{
				base.ViewDidLoad();
				View.BackgroundColor = UIColor.Black;
				

				NSError error = null;
				_captureSession = new AVCaptureSession();
                CameraMetaDataDelegate del = null;
				AVCaptureDevice captureDevice = AVCaptureDevice.GetDefaultDevice(AVMediaType.Video); // update for iOS 13
				if (captureDevice != null)
				{
					var videoInput = AVCaptureDeviceInput.FromDevice(captureDevice, out error);

			    	if (videoInput != null) { _captureSession.AddInput(videoInput); }
					else { iApp.Log.Error("Video capture error: " + error.LocalizedDescription); }
			 
					var metaDataOutput = new AVCaptureMetadataOutput();
					_captureSession.AddOutput(metaDataOutput);
                    
					del = new CameraMetaDataDelegate(this, _layer);
					metaDataOutput.SetDelegate(del, CoreFoundation.DispatchQueue.MainQueue);
	
					metaDataOutput.MetadataObjectTypes = metaDataOutput.AvailableMetadataObjectTypes;
	

//					int yLoc = 0;
//					if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0)) { yLoc = 20; }
					
                    _videoPreviewLayer = new AVCaptureVideoPreviewLayer(_captureSession) { 
                        Frame = View.Bounds,
                        Orientation = (AVCaptureVideoOrientation)InterfaceOrientation,
                    };
					View.Layer.AddSublayer(_videoPreviewLayer);

                    var image = TouchStyle.ImageFromResource("barcode-overlay-sm.png");
                    _imageOverlay = new UIImageView(image)
                    {
                        Frame = View.Frame,
                        ContentMode = UIViewContentMode.Center,
                        AutoresizingMask = UIViewAutoresizing.FlexibleMargins,
                    };
                    View.Add(_imageOverlay);

                    // preload this, and display when scan event occurs
                    var imageScanBlocked = TouchStyle.ImageFromResource("barcode-scanblocked-sm.png");
                    _imageOverlayScanBlocked = new UIImageView(imageScanBlocked)
                    {
                        Frame = View.Frame,
                        ContentMode = UIViewContentMode.Center,
                        AutoresizingMask = UIViewAutoresizing.FlexibleMargins,
                        Hidden = true,
                    };
                    View.Add(_imageOverlayScanBlocked);
				}
				else
				{
					//TODO: Add "Scanner currently not active overlay Image"
				}

                nfloat startVerticalLoc = UIScreen.MainScreen.Bounds.Height- LastValueScanOverlay.ViewHeight;
                _lastScanOverlay = new LastValueScanOverlay(startVerticalLoc, _layerFont);
                View.Add(_lastScanOverlay);

                NavigationItem.LeftBarButtonItem = new UIBarButtonItem("Done", UIBarButtonItemStyle.Done, delegate {

                    string scannedBarcodes = string.Empty;
                    if (del != null && del.Buffer != null && del.Buffer.CurrentBuffer != null)
                    {
                        foreach (var s in del.Buffer.CurrentBuffer)
                        {
                            scannedBarcodes += s + "\r\n";
                        }
                    }
                    if (_callback.Parameters == null) { _callback.Parameters = new Dictionary<string, string>(); }
                    _callback.Parameters[_barcodeValueKey] = scannedBarcodes;
                    iApp.Navigate(_callback);
                    ModalManager.EnqueueModalTransition(TouchFactory.Instance.TopViewController, null, true);
                });
			}

			public override void ViewWillAppear(bool animated)
			{
				base.ViewWillAppear(animated);
                
                if (_videoPreviewLayer != null)
                {
                    _videoPreviewLayer.Frame = View.Bounds;
                }

                View.BackgroundColor = _layer.LayerStyle.LayerBackgroundColor.ToUIColor();
                
				if (_captureSession != null)
					_captureSession.StartRunning();
			}

			public override void ViewWillDisappear(bool animated)
			{
				base.ViewWillDisappear (animated);
				
                if (_captureSession != null)
                {
    				_captureSession.StopRunning();
                }
			}
            
            public override void WillRotate(UIInterfaceOrientation toInterfaceOrientation, double duration)
            {
                _videoPreviewLayer.Orientation = (AVCaptureVideoOrientation)toInterfaceOrientation;
            }
            
            public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
            {
                _videoPreviewLayer.Frame = View.Bounds;

                nfloat startVerticalLoc = View.Frame.Height- _lastScanOverlay.Frame.Height;

                var newPoint = new CGPoint(_lastScanOverlay.Frame.X, startVerticalLoc);
                var newSize = new CGSize(View.Frame.Width, _lastScanOverlay.Frame.Height);
                var newFrame = new CGRect(newPoint, newSize);
                _lastScanOverlay.Frame = newFrame;

            }
		}
	
        private class LastValueScanOverlay : UIView
        {
            private UILabel _barcodeLabel;
            private UIImage _image;
            private UIImage _imageMaxCount;
            private UIImageView _imageView;
            public const float LabelFontSize = 36;
            public const float ViewHeight = 64;

            public LastValueScanOverlay(nfloat yLoc, Font font)
            {
                BackgroundColor = UIColor.FromRGBA(0xFF, 0xFF, 0xFF, 0xF0);
                Frame = new CGRect(0, yLoc, UIScreen.MainScreen.Bounds.Width, ViewHeight);

                _imageMaxCount = TouchStyle.ImageFromResource("barcode-icon-max-count.png");
                _image = TouchStyle.ImageFromResource("barcode-icon.png");
                _imageView = new UIImageView(new CGRect(0, 0, 80, 64)) { // frame matches icon size
                    Image = _image,
                }; 
                this.Add(_imageView);

                const float padding = 4;
                var label_xLoc = _imageView.Frame.Right;
                var label_yLoc = (Frame.Height - LabelFontSize)/2;
                var labelWidth = Frame.Width - label_xLoc - padding;
                var labelHeight = LabelFontSize;

                _barcodeLabel = new UILabel() {
                    Font = TouchStyle.ToUIFont(font),
                    AdjustsFontSizeToFitWidth = true,
                    TextAlignment = UITextAlignment.Left,
                    Frame = new CGRect(label_xLoc, label_yLoc, labelWidth, labelHeight),
                };
                _barcodeLabel.Text = string.Empty;
                this.Add(_barcodeLabel);
            }

            public void SetBarcodeLabel(string barcode, int occurrences) 
            { 
                if (occurrences <= 20)
                {
                    UIImage image = null;
                    string imageResouceName = string.Concat("barcode-icon", occurrences, ".png");
                    try { image = TouchStyle.ImageFromResource(imageResouceName); }
                    catch (Exception e) 
                    {
                        iApp.Log.Debug("Exception loading barcode-icon:\r\n" + e); 
                        image =  _image; 
                    }
                    _imageView.Image = image;
                }
                else { _imageView.Image = _imageMaxCount; }

                _barcodeLabel.Text = barcode; 
            }
        }

		private class CameraMetaDataDelegate : AVCaptureMetadataOutputObjectsDelegate
		{
            private CameraScanController _view;
            private CameraScanLayer _layer;
            internal ScanBuffer Buffer;

            public CameraMetaDataDelegate(CameraScanController view, CameraScanLayer layer)
			{ 
                _view = view;
                _layer = layer;
                Buffer = new ScanBuffer(view.DuplicateWait);
            }
			
			public override void DidOutputMetadataObjects(AVCaptureMetadataOutput captureOutput, AVMetadataObject[] metadataObjects, AVCaptureConnection connection)
			{
				string barcode = string.Empty;

                DateTime start = DateTime.UtcNow;
				foreach (AVMetadataObject metadataObject in metadataObjects)
			    {
                    var metaObject = metadataObject as AVMetadataMachineReadableCodeObject;

                    if (metaObject != null)
                    {
    			        AVMetadataMachineReadableCodeObject readableObject = metaObject;
    					barcode = readableObject.StringValue;
                        iApp.Log.Info("Barcode scanned (Type: " +metadataObject.Type + ") = " + barcode);

                        if (_layer != null) { barcode = _layer.ScannedBarcode(barcode); }

                        if(!string.IsNullOrEmpty(barcode))
                        {
                            int result = Buffer.Add(barcode);
                            if (result > 0)
                            {
                                // update the UI
                                _view.ScanOccurred(barcode, result);
                            }
                        }
                    }
                    else
                    {
                        iApp.Log.Info("Invalid AVMetadataObject type: " + metadataObject.Type.ToString());
                    }
                }
                iApp.Log.Debug("Total Barcodes scanned is: " + Buffer.CurrentBuffer.Length);
			}
		}

        private class ScanBuffer
        {
            private object _lockObj = new object();
            private Dictionary<string, int> _buffer = new Dictionary<string, int>(50);
            private DateTime _lastScanTime = new DateTime(0);
            private string _lockBarcode;
            private int _duplicateWait = 1000;

            /// <summary>Adds a string to the buffer</summary>
            /// <param name="newInput">The string to add</param>
            /// <returns>The number of times the string appears in the buffer</returns>
            public int Add(string newInput)
            {
                DateTime currentScanTime = DateTime.UtcNow;
                if (newInput.CompareTo(_lockBarcode) == 0)
                {
                    if ((currentScanTime - _lastScanTime).TotalMilliseconds < _duplicateWait)
                    {
                        var s = String.Format("REJECTED {0}: Barcode {1} was previously added to the buffer at {2}",
                                        currentScanTime.ToString("ss.f"), newInput, _lastScanTime.ToString("ss.ffff"));
                        iApp.Log.Debug(s);
                        return 0;
                    }
                }

                int result = 0;
                lock (_lockObj)
                {
                    int occurs = 0;
                    if(_buffer.TryGetValue(newInput, out occurs))
                    {
                        _buffer[newInput] = ++occurs;
                    }
                    else 
                    { 
                        occurs = 1;
                        _buffer[newInput] = occurs; 
                    }

                    _lastScanTime = currentScanTime;
                    _lockBarcode = newInput;
                    result = occurs;
                }

                return result;
            }

            /// <summary>Gets the current buffer.</summary>
            /// <returns>The current buffer.</returns>
            public string[] CurrentBuffer
            {
                get
                {
                    string[] result = null;
                    lock (_lockObj) 
                    { 
                        List<string> scans = new List<string>(_buffer.Keys.Count * 2);
                        foreach(KeyValuePair<string, int> kvp in _buffer)
                        {
                            for(int added = 0; added < kvp.Value; added++)
                            {
                                scans.Add(kvp.Key);
                            }
                        }
                        result = scans.ToArray(); 
                    }
                    return result;
                }
            }

            /// <summary>Clear this instance.</summary>
            /// <returns>The buffer before it was cleared</returns>
            public string[] Clear()
            {
                string[] result = null;
                lock (_lockObj)
                {
                    result = CurrentBuffer;
                    _buffer.Clear();
                }
                return result;
            }

            public ScanBuffer() { }
            public ScanBuffer(int duplicateWait) { _duplicateWait = duplicateWait; }
        }
	}
}
