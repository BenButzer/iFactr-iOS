using AVFoundation;
using CoreFoundation;

using iFactr.Core;
using MonoCross.Utilities;
using iFactr.UI;


namespace iFactr.Touch
{
    public class CameraScannerSplitView : UIViewController
    {
        //
        // Static Fields
        //
        private const string CellId = "CameraScannerSplitViewResultsCell";

        //
        // Fields
        //
        public int DuplicateWait = 15000;

        private CameraScannerSplitView.CameraMetaDataDelegate _cameraMetaDataDelegate;

        private AVCaptureVideoPreviewLayer _videoPreviewLayer;

        private UIImageView _imageOverlayScanBlocked;

        private UIImageView _imageOverlay;

        private UITableView _resultsView;

        private Link _callback;

        private string _barcodeValueKey;

        private AVCaptureSession _captureSession;

        private UIView _cameraView;

        //
        // Properties
        //
        public UIColor BackgroundColor
        {
            get;
            set;
        }

        //
        // Constructors
        //
        public CameraScannerSplitView(Link callback, string parametersKey)
        {
            this._callback = callback;
            this._barcodeValueKey = parametersKey;
            this.ModalPresentationStyle = UIModalPresentationStyle.FormSheet;
        }

        //
        // Methods
        //
        private void AddNoCameraAccessLabels()
        {
            nfloat width = this._imageOverlay.Frame.Width;
            nfloat nfloat = (this._imageOverlay.Frame.Height - 30f) / 2 - 20;
            UILabel uILabel = new UILabel
            {
                Text = "Camera Disabled",
                TextAlignment = UITextAlignment.Center,
                Frame = new CGRect(0, nfloat, width, 30f),
                AutoresizingMask = UIViewAutoresizing.All
            };
            this._imageOverlay.Add(uILabel);
            UILabel view = new UILabel
            {
                Text = "Please enable the camera in the privacy section of Settings.",
                TextAlignment = UITextAlignment.Center,
                LineBreakMode = UILineBreakMode.WordWrap,
                Lines = 0,
                Frame = new CGRect(0, nfloat + uILabel.Frame.Height + 10, width, 60f),
                AutoresizingMask = UIViewAutoresizing.All
            };
            this._imageOverlay.Add(view);
        }

        private void AddCameraTryAgainLabels()
        {
            nfloat width = this._imageOverlay.Frame.Width;
            nfloat nfloat = (this._imageOverlay.Frame.Height - 30f) / 2 - 20;
            UILabel uILabel = new UILabel
            {
                Text = "Camera Readying",
                TextAlignment = UITextAlignment.Center,
                Frame = new CGRect(0, nfloat, width, 30f),
                AutoresizingMask = UIViewAutoresizing.All
            };
            this._imageOverlay.Add(uILabel);
            UILabel view = new UILabel
            {
                Text = "If you just granted access for the first time, hit done and then scan again.",
                TextAlignment = UITextAlignment.Center,
                LineBreakMode = UILineBreakMode.WordWrap,
                Lines = 0,
                Frame = new CGRect(0, nfloat + uILabel.Frame.Height + 10, width, 60f),
                AutoresizingMask = UIViewAutoresizing.All
            };
            this._imageOverlay.Add(view);
        }

        public override void DidReceiveMemoryWarning()
        {
            Device.Log.Warn("CameraScanSplitView Received memory warning from OS", new object[0]);
        }

        public virtual UITableViewCell GetCell(UITableView tableView, string barcode)
        {
            UITableViewCell uITableViewCell = tableView.DequeueReusableCell("CameraScannerSplitViewResultsCell");
            return new UITableViewCell(UITableViewCellStyle.Default, "CameraScannerSplitViewResultsCell")
            {
                TextLabel =  {
                    Lines = 0,
                    Text = barcode
                }
            };
        }

        public virtual nfloat GetCellHeight(NSIndexPath index)
        {
            return 44;
        }

        public virtual string ScannedBarcode(string barcode)
        {
            return barcode;
        }

        public virtual string ScanOccurred(string barcode)
        {
            this._imageOverlay.Hidden = true;
            this._imageOverlayScanBlocked.Hidden = false;
            Device.Thread.QueueWorker(delegate (object o)
            {
                int millisecondsTimeout;
                if (this.DuplicateWait > 100)
                {
                    millisecondsTimeout = this.DuplicateWait - 100;
                }
                else
                {
                    millisecondsTimeout = this.DuplicateWait;
                }
                new ManualResetEvent(false).WaitOne(millisecondsTimeout);
                Device.Thread.ExecuteOnMainThread(delegate
                {
                    this._imageOverlayScanBlocked.Hidden = true;
                    this._imageOverlay.Hidden = false;
                });
            });
            if (barcode != null)
            {
                CameraScannerSplitView.CameraListSource cameraListSource = this._resultsView.Source as CameraScannerSplitView.CameraListSource;
                if (cameraListSource != null)
                {
                    cameraListSource.AddBarcode(barcode);
                }
                else
                {
                    Device.Log.Error("Error displaying the camera scanner results view", new object[0]);
                }
            }
            else
            {
                Device.Log.Debug("Barcode value was null, ignoring the scan", new object[0]);
            }
            return barcode;
        }

        private void SetupVideoPreviewLayer()
        {
            this._videoPreviewLayer = new AVCaptureVideoPreviewLayer(this._captureSession)
            {
                Frame = this._cameraView.Bounds,
                VideoGravity = AVLayerVideoGravity.ResizeAspect,
                BackgroundColor = UIColor.White.CGColor
            };
            this._cameraView.Layer.AddSublayer(this._videoPreviewLayer);
            this._cameraView.Layer.AffineTransform = CGAffineTransform.MakeTranslation(0, this.View.Frame.Height / 4 * -1);
        }

        public void ViewDidLoad(bool isEvUser)
        {
            base.ViewDidLoad();
            this.View.BackgroundColor = UIColor.White;
            int v = 0;
            if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
            {
                v = 20;
            }
            nfloat height = this.View.Frame.Height / 2 - v;
            CGRect frame = new CGRect(0, v, this.View.Frame.Width, height);
            UIImage image = TouchStyle.ImageFromResource("barcode-overlay-sm.png");
            this._imageOverlay = new UIImageView(image)
            {
                Frame = frame,
                ContentMode = UIViewContentMode.Center,
                AutoresizingMask = (UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleBottomMargin)
            };
            this.View.Add(this._imageOverlay);
            UIImage image2 = TouchStyle.ImageFromResource("barcode-scanblocked-sm.png");
            this._imageOverlayScanBlocked = new UIImageView(image2)
            {
                Frame = frame,
                ContentMode = UIViewContentMode.Center,
                AutoresizingMask = (UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleBottomMargin),
                Hidden = true
            };
            this.View.Add(this._imageOverlayScanBlocked);
            this._cameraView = new UIView
            {
                AutoresizingMask = (UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleBottomMargin)
            };
            this._cameraView.Frame = frame;

            AVAuthorizationStatus authorizationStatus = AVCaptureDevice.GetAuthorizationStatus(AVAuthorizationMediaType.Video);


            //AVCaptureDevice device = AVCaptureDevice.GetDefaultDevice(); // update for iOS 13


            AVCaptureDevice device = null;

            if (AVCaptureDevice.GetDefaultDevice(AVCaptureDeviceType.BuiltInDualCamera, AVMediaTypes.Video, AVCaptureDevicePosition.Back) != null)
            {
                device = AVCaptureDevice.GetDefaultDevice(AVCaptureDeviceType.BuiltInDualCamera, AVMediaTypes.Video, AVCaptureDevicePosition.Back);
            }
            else if (AVCaptureDevice.GetDefaultDevice(AVCaptureDeviceType.BuiltInWideAngleCamera, AVMediaTypes.Video, AVCaptureDevicePosition.Back) != null)
            {
                device = AVCaptureDevice.GetDefaultDevice(AVCaptureDeviceType.BuiltInWideAngleCamera, AVMediaTypes.Video, AVCaptureDevicePosition.Back);
            }
            else
            {
                throw new Exception("Missing expected back camera device.");
            }


            if (authorizationStatus == AVAuthorizationStatus.Authorized)
            {


                NSError nSError;
                AVCaptureDeviceInput aVCaptureDeviceInput = AVCaptureDeviceInput.FromDevice(device, out nSError);
                if (aVCaptureDeviceInput != null)
                {
                    this._captureSession = new AVCaptureSession();
                    this._captureSession.AddInput(aVCaptureDeviceInput);
                    AVCaptureMetadataOutput aVCaptureMetadataOutput = new AVCaptureMetadataOutput();
                    this._captureSession.AddOutput(aVCaptureMetadataOutput);
                    this._cameraMetaDataDelegate = new CameraScannerSplitView.CameraMetaDataDelegate(this);
                    aVCaptureMetadataOutput.SetDelegate(this._cameraMetaDataDelegate, DispatchQueue.MainQueue);
                    // Update in 2019 for iOS13 and iPhone 11, explicit types and request access
                    // Add DeviceMatrix 2021
                    // Microsoft.iOS Conversion: 

                    if (isEvUser)
                    {
                        aVCaptureMetadataOutput.MetadataObjectTypes = AVMetadataObjectType.QRCode |
                            AVMetadataObjectType.Code128Code | AVMetadataObjectType.UPCECode |
                            AVMetadataObjectType.EAN13Code | AVMetadataObjectType.DataMatrixCode;
                    }
                    else
                    {
                        aVCaptureMetadataOutput.MetadataObjectTypes = AVMetadataObjectType.Code128Code |
                            AVMetadataObjectType.UPCECode | AVMetadataObjectType.EAN13Code;
                    }

                }
            }
            else if (authorizationStatus == AVAuthorizationStatus.NotDetermined)
            {
                AVCaptureDevice.RequestAccessForMediaType(AVAuthorizationMediaType.Video, (granted)
                     =>
                    {
                        if (!granted)
                        {
                            Device.Log.Error("ViewDidLoadBase ScanLayer RequestAccessForMediaType not granted!");
                        }
                        else
                        {
                            Device.Log.Error("ViewDidLoadBase ScanLayer RequestAccessForMediaType granted!");
                        }
                    });
            }
            else
            {
                Device.Log.Error("Not Authorized! Status: " + authorizationStatus.ToString());
            }


            if (authorizationStatus >= AVAuthorizationStatus.NotDetermined && authorizationStatus <= AVAuthorizationStatus.Authorized)
            {
                switch ((int)authorizationStatus)
                {
                    case 0:
                        Device.Log.Warn("Camera Access is not yet authorized on main thread", new object[0]);
                        this.AddCameraTryAgainLabels();
                        break;
                    // this does not happen in time on the main thread
                    //AVCaptureDevice.RequestAccessForMediaType(AVMediaType.Video, delegate (bool result)
                    //{
                    //    Device.Thread.ExecuteOnMainThread(delegate
                    //    {
                    //        if (result)
                    //        {
                    //            this.SetupVideoPreviewLayer();
                    //        }
                    //        else {
                    //            this.AddNoCameraAccessLabels();
                    //        }
                    //    });
                    //});
                    //break;
                    case 1:
                        Device.Log.Warn("Camera Access is restricted", new object[0]);
                        this.AddNoCameraAccessLabels();
                        break;
                    case 2:
                        this.AddNoCameraAccessLabels();
                        break;
                    case 3:
                        this.SetupVideoPreviewLayer();
                        break;
                }
            }
            this.View.InsertSubviewBelow(this._cameraView, this._imageOverlay);
            CGRect frame2 = new CGRect(0, frame.Bottom, this.View.Frame.Width, this.View.Frame.Height - frame.Height);
            this._resultsView = new UITableView(frame2, UITableViewStyle.Plain)
            {
                AutoresizingMask = (UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleTopMargin | UIViewAutoresizing.FlexibleHeight)
            };
            this._resultsView.Source = new CameraScannerSplitView.CameraListSource(this);
            this.View.Add(this._resultsView);
            this.NavigationItem.LeftBarButtonItem = new UIBarButtonItem("Done", UIBarButtonItemStyle.Done, delegate
            {
                string text = string.Empty;
                if (this._resultsView.Source != null)
                {
                    try
                    {
                        string text2 = string.Empty;
                        List<string> list = (this._resultsView.Source as CameraScannerSplitView.CameraListSource).ScannedBarcodes();
                        foreach (string current in list)
                        {
                            if (!string.IsNullOrEmpty(current))
                            {
                                text2 = text2 + current + "\r\n";
                            }
                        }
                        text = text2;
                    }
                    catch (Exception arg)
                    {
                        Device.Log.Error("This error occurred while parsing barcodes scanned: \r\n" + arg, new object[0]);
                    }
                }
                if (this._callback.Parameters == null)
                {
                    this._callback.Parameters = new Dictionary<string, string>();
                }
                this._callback.Parameters[this._barcodeValueKey] = text;
                iApp.Navigate(this._callback);
                ModalManager.EnqueueModalTransition(TouchFactory.Instance.TopViewController, null, true);
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            if (this._videoPreviewLayer != null)
            {
                this._videoPreviewLayer.Frame = this.View.Bounds;
                if (this._videoPreviewLayer.Connection != null)
                {
                    this._videoPreviewLayer.Connection.VideoOrientation = (AVCaptureVideoOrientation)this.InterfaceOrientation;
                }
            }
            this.View.BackgroundColor = this.BackgroundColor;
            if (this._captureSession != null)
            {
                this._captureSession.StartRunning();
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            if (this._captureSession != null)
            {
                this._captureSession.StopRunning();
            }
            this._cameraMetaDataDelegate = null;
        }

        public override void ViewWillLayoutSubviews()
        {
            base.ViewWillLayoutSubviews();
            if (this._videoPreviewLayer != null)
            {
                this._videoPreviewLayer.Frame = this.View.Bounds;
                this._cameraView.Layer.AffineTransform = CGAffineTransform.MakeTranslation(0, this.View.Frame.Height / 4 * -1);
            }
        }

        public override void WillRotate(UIInterfaceOrientation toInterfaceOrientation, double duration)
        {
            if (this._videoPreviewLayer != null)
            {
                this._videoPreviewLayer.Connection.VideoOrientation = (AVCaptureVideoOrientation)toInterfaceOrientation;
            }
        }

        //
        // Nested Types
        //
        private class CameraListSource : UITableViewSource
        {
            private WeakReference<CameraScannerSplitView> _view;

            private readonly List<string> _list;

            private const string DefaultCellId = "CameraListSourceDefaultCellId";

            public CameraListSource(CameraScannerSplitView view)
            {
                this._list = new List<string>();
                this._view = new WeakReference<CameraScannerSplitView>(view);
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                UITableViewCell uITableViewCell = null;
                CameraScannerSplitView cameraScannerSplitView;
                if (_view.TryGetTarget(out cameraScannerSplitView))
                {
                    uITableViewCell = cameraScannerSplitView.GetCell(tableView, this._list[indexPath.Row]);
                }
                if (uITableViewCell == null)
                {
                    uITableViewCell = tableView.DequeueReusableCell("CameraListSourceDefaultCellId");
                    if (uITableViewCell == null)
                    {
                        uITableViewCell = new UITableViewCell(UITableViewCellStyle.Default, "CameraListSourceDefaultCellId");
                    }
                    uITableViewCell.TextLabel.Text = this._list[indexPath.Row];
                }
                return uITableViewCell;
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                nfloat result = 60;
                CameraScannerSplitView cameraScannerSplitView;
                if (_view.TryGetTarget(out cameraScannerSplitView))
                {
                    result = cameraScannerSplitView.GetCellHeight(indexPath);
                }
                return result;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                return this._list.Count;
            }

            public void AddBarcode(string barcode)
            {
                if (string.IsNullOrEmpty(barcode))
                {
                    return;
                }
                this._list.Insert(0, barcode);
                CameraScannerSplitView cameraScannerSplitView;
                if (_view.TryGetTarget(out cameraScannerSplitView))
                {
                    cameraScannerSplitView._resultsView.InsertRows(new NSIndexPath[] {
                        NSIndexPath.FromRowSection (0, 0)
                    }, UITableViewRowAnimation.Automatic);
                }
            }

            public List<string> ScannedBarcodes()
            {
                if (this._list == null)
                {
                    return new List<string>();
                }
                return this._list;
            }
        }

        private class CameraMetaDataDelegate : AVCaptureMetadataOutputObjectsDelegate
        {
            private readonly CameraScannerSplitView _view;

            internal CameraScannerSplitView.ScanBuffer Buffer;

            public CameraMetaDataDelegate(CameraScannerSplitView view)
            {
                if (view == null)
                {
                    throw new ArgumentNullException("view", "The Delegate needs a view");
                }
                this._view = view;
                this.Buffer = new CameraScannerSplitView.ScanBuffer(view.DuplicateWait);
            }

            public override void DidOutputMetadataObjects(AVCaptureMetadataOutput captureOutput, AVMetadataObject[] metadataObjects, AVCaptureConnection connection)
            {
                string text = string.Empty;
                for (int i = 0; i < metadataObjects.Length; i++)
                {
                    AVMetadataObject aVMetadataObject = metadataObjects[i];
                    AVMetadataMachineReadableCodeObject aVMetadataMachineReadableCodeObject = aVMetadataObject as AVMetadataMachineReadableCodeObject;
                    if (aVMetadataMachineReadableCodeObject != null)
                    {
                        AVMetadataMachineReadableCodeObject aVMetadataMachineReadableCodeObject2 = aVMetadataMachineReadableCodeObject;
                        text = aVMetadataMachineReadableCodeObject2.StringValue;
                        text = this._view.ScannedBarcode(text);
                        if (!string.IsNullOrEmpty(text))
                        {
                            int num = this.Buffer.Add(text);
                            if (num > 0)
                            {
                                this._view.ScanOccurred(text);
                            }
                        }
                    }
                    else
                    {
                        iApp.Log.Info("Invalid AVMetadataObject type: " + aVMetadataObject.Type.ToString(), new object[0]);
                    }
                }
            }
        }

        private class ScanBuffer
        {
            private object _lockObj = new object();

            private Dictionary<string, int> _buffer = new Dictionary<string, int>(50);

            private DateTime _lastScanTime = new DateTime(0L);

            private string _lockBarcode;

            private int _duplicateWait = 1000;

            public string[] CurrentBuffer
            {
                get
                {
                    string[] result = null;
                    object lockObj = this._lockObj;
                    lock (lockObj)
                    {
                        List<string> list = new List<string>(this._buffer.Keys.Count * 2);
                        foreach (KeyValuePair<string, int> current in this._buffer)
                        {
                            for (int i = 0; i < current.Value; i++)
                            {
                                list.Add(current.Key);
                            }
                        }
                        result = list.ToArray();
                    }
                    return result;
                }
            }

            public ScanBuffer()
            {
            }

            public ScanBuffer(int duplicateWait)
            {
                this._duplicateWait = duplicateWait;
            }

            public int Add(string newInput)
            {
                DateTime utcNow = DateTime.UtcNow;
                if (newInput.CompareTo(this._lockBarcode) == 0 && (utcNow - this._lastScanTime).TotalMilliseconds < (double)this._duplicateWait)

                
                {
                    string text = string.Format("REJECTED {0}: Barcode {1} was previously added to the buffer at {2}", utcNow.ToString("ss.f"), newInput, this._lastScanTime.ToString("ss.ffff"));
                    iApp.Log.Debug(text, new object[0]);
                    return 0;
                }
                int result = 0;
                object lockObj = this._lockObj;
                lock (lockObj)
                {
                    int num = 0;
                    if (this._buffer.TryGetValue(newInput, out num))
                    {
                        num = (this._buffer[newInput] = num + 1);
                    }
                    else
                    {
                        num = 1;
                        this._buffer[newInput] = num;
                    }
                    this._lastScanTime = utcNow;
                    this._lockBarcode = newInput;
                    result = num;
                }
                return result;
            }

            public string[] Clear()
            {
                string[] result = null;
                object lockObj = this._lockObj;
                lock (lockObj)
                {
                    result = this.CurrentBuffer;
                    this._buffer.Clear();
                }
                return result;
            }
        }
    }
}
