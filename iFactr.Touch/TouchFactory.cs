using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using iFactr.Core;
using iFactr.Core.Layers;
using iFactr.Core.Native;
using iFactr.Core.Styles;
using iFactr.Core.Targets;
using iFactr.Core.Targets.Settings;
using MonoCross.Utilities;
using MonoCross;
using MonoCross.Utilities.Encryption;
using MonoCross.Utilities.Storage;
using MonoCross.Utilities.ImageComposition;
using MonoCross.Utilities.Logging;
using MonoCross.Utilities.Threading;
using iFactr.Integrations;
using iFactr.UI;
using iFactr.UI.Controls;
using iFactr.UI.Instructions;
using MonoCross.Navigation;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using UIKit;

namespace iFactr.Touch
{
    /// <summary>
    /// Represents the method that will handle the CustomItemRequested event.
    /// </summary>
    public delegate UITableViewCell CustomItemHandler(ICustomItem item, UITableViewCell cell, UITableView tableView);

    /// <summary>
    /// Represents the method that will handle the CustomItemHeightRequested event.
    /// </summary>
    public delegate float CustomItemHeightHandler(ICustomItem item, UITableView tableView);
    
    /// <summary>
    /// This class represents the binding factory for the Touch target.
    /// </summary>
    /// <remarks>
    /// <para></para>
    /// <para><img src="TouchFactory.cd"/></para>
    /// <para></para>
    /// <para>In order to use the TouchFactory within a container, you need to attach a
    /// handler to the OnLayerLoadComplete event that will then output the layer using
    /// the Touch bindings.  You then create a new instance of your application and pass
    /// it into TouchFactory.TheApp.  To then kick-off application execution, the
    /// NavigateOnLoad string that contains the URL of the initial location to navigate
    /// is passed to iApp.Navigate which will use the NavigationMappings to identify
    /// which layer has registered the URL template that matches this URL, and then
    /// calls the the Load event on the layer, which will execute any business logic
    /// contained on that layer.</para>
    /// <para></para>
    /// <code lang="C#">iApp.OnLayerLoadComplete += (iLayer layer) =&gt; {
    /// InvokeOnMainThread(delegate { TouchFactory.Instance.OutputLayer(layer); } );  };
    /// TouchFactory.TheApp = new BestSellers.App();
    /// iApp.Navigate(TouchFactory.TheApp.NavigateOnLoad);</code>
    /// <para></para>
    /// <para>When the layer has completed loading and is ready to be rendered using the
    /// Touch bindings, the OnLayerLoadComplete event will fire which will then call the
    /// OutputLayer method on the factory in the main thread passing in the layer to be
    /// displayed.</para></remarks>
    [Preserve(AllMembers = true)]
    public class TouchFactory : NativeFactory
    {
        #region TouchFactory Members
        /// <summary>
        /// Occurs when a custom item is requested.
        /// </summary>
        public event CustomItemHandler CustomItemRequested;

        /// <summary>
        /// Occurs when the height of a custom item requested.
        /// </summary>
        [Obsolete]
        public event CustomItemHeightHandler CustomItemHeightRequested;
        
        #region ITargetFactory Members
        public override Instructor Instructor
        {
            get { return instructor ?? (instructor = new TouchInstructor()); }
            set { instructor = value; }
        }
        private Instructor instructor;

        /// <summary>
        /// Gets the application path.
        /// </summary>
        public override string ApplicationPath
        {
            get { return Device.ApplicationPath; }
        }

        /// <summary>
        /// Gets the data path.
        /// </summary>
        public override string DataPath
        {
            get { return Device.DataPath; }
        }

        /// <summary>
        /// Gets the temp path.
        /// </summary>
        public override string TempPath
        {
            get { return Path.GetTempPath(); }
        }
        
        /// <summary>
        ///  Gets the application settings. 
        /// </summary>
        public override ISettings Settings
        {
            get
            {
                if (_settings == null)
                {
                    _settings = new TouchSettingsDictionary();
                }

                return _settings;
            }
        }

        /// <summary>
        /// Gets the image compositor.
        /// </summary>
        public override ICompositor Compositor
        {
            get
            {
                if (_compositor == null)
                {
                    _compositor = new TouchCompositor();
                }
                
                return _compositor;
            }
        }
  
        /// <summary>
        /// Gets the file interface.
        /// </summary>
        public override IFile File
        {
            get { return Device.File; }
        }
        
        /// <summary>
        /// Gets the encryption interface.
        /// </summary>
        public override IEncryption Encryption 
        {
            get { return Device.Encryption; }
        }

        /// <summary>
        ///  Gets the application IThread implementation. 
        /// </summary>
        public override IThread Thread
        {
            get { return Device.Thread; }
        }

        /// <summary>
        /// Gets the target bindings.
        /// </summary>
        public override MobileTarget Target
        {
            get { return MobileTarget.Touch; }
        }

        /// <summary>
        /// Gets a unique identifier for the device running the application.
        /// </summary>
        public override string DeviceId
        {
            get { return UIDevice.CurrentDevice.IdentifierForVendor.AsString(); }
        }
        #endregion
        
        /// <summary>
        /// Gets the singleton instance of the TouchFactory.
        /// </summary>
        public static new TouchFactory Instance
        {
            get { return (TouchFactory) MXContainer.Instance; }
        }

		/// <summary>
		/// Gets the window for displaying the application.
		/// </summary>
		public static UIWindow KeyWindow { get; private set; }
        
		/// <summary>
		/// Gets the primary view controller of the application display. 
		/// </summary>
        public UIViewController TopViewController
        {
            get
            {
                if (topViewController == null)
                {
                    InitializeViews();
                }

                return topViewController;
            }
            private set
            {
                topViewController = value;
            }
        }
        private UIViewController topViewController;
		
		/// <summary>
		/// Gets whether or not the factory is using LargeFormFactor logic. 
		/// </summary>
		public override bool LargeFormFactor
        {
            get { return Platform == MobilePlatform.iPad; }
        }
		
		/// <summary>
		/// Gets the default style for each layer. 
		/// </summary>
		public override Style Style
        {
			get
            {
                return _style ?? (_style = new Style()
                {
                    SubTextColor = new Color("8f8f8f"),
                    SecondarySubTextColor = new Color("8f8f8f")
                });
            }
		}
		Style _style;
		
		/// <summary>
		/// Gets whether or not the device is in landscape orientation.
		/// </summary>
		public bool IsLandscape
        {
			get
            {
            	return (TopViewController.InterfaceOrientation == UIInterfaceOrientation.LandscapeLeft ||
			    	TopViewController.InterfaceOrientation == UIInterfaceOrientation.LandscapeRight);
            }
		}
		
		/// <summary>
		/// Gets or sets whether the factory is busy and should ignore additional navigate calls.
		/// </summary>
        [Obsolete]
		public bool IsLoading
		{
			get { return isLoading; }
			set
			{
				isLoading = value;
				if (!value && LoadingHud != null)
				{
					LoadingHud.StopAnimating();
				}
			}
		}
		private bool isLoading;
		
		/// <summary>
		/// Gets or sets the orientations in which the master pane will not render
		/// with the split view controller and instead will render in a popover.
		/// Default is None.
		/// </summary>
		public MasterOrientation HideMasterPaneInOrientation
		{
			get { return hideMasterPaneInOrientation; }
			set
			{
				hideMasterPaneInOrientation = value;
				if (SplitViewController != null)
				{
					SplitViewController.SetShowsMasterInLandscape((value & MasterOrientation.Landscape) == 0);
					SplitViewController.SetShowsMasterInPortrait((value & MasterOrientation.Portrait) == 0);
				}
			}
		}
		private MasterOrientation hideMasterPaneInOrientation = MasterOrientation.None;
		
		/// <summary>
		/// Gets or sets whether or not the user is allowed to resize the
		/// master and detail panes of the split view.  Defaults to false.
		/// </summary>
		public bool AllowSplitViewResizing
		{
			get { return allowSplitViewResizing; }
			set
			{ 
				allowSplitViewResizing = value;
				if (SplitViewController != null)
				{
					SplitViewController.DividerView.AllowsDragging = value;
					SplitViewController.LayoutSubviews();
				}
			}
		}
		private bool allowSplitViewResizing = false;

        internal UITabBarController TabBarController { get; set; }

        internal MGSplitViewController SplitViewController { get; set; }

        internal LoadSpinner LoadingHud { get; set; }
        #endregion
		
        #region TouchFactory Constructors
        static TouchFactory()
        {
        	Device.Initialize(new TouchDevice());
        	
            AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e)
            {
                Instance.LogUnhandledException((Exception)e.ExceptionObject);
            };
            
            UIDevice.CurrentDevice.BeginGeneratingDeviceOrientationNotifications();
            
            TargetFactory.Initialize(new TouchFactory());

            iApp.Log.Info("Running iOS " + UIDevice.CurrentDevice.SystemVersion);

            KeyWindow = new UIWindow(UIScreen.MainScreen.Bounds);
			KeyWindow.MakeKeyAndVisible();
            KeyWindow.RootViewController = new UIViewController();

            Instance.LanguageCode = NSLocale.CurrentLocale.CollatorIdentifier.Replace('_', '-');
            
            iApp.VanityImagePath = (Instance.Settings.ContainsKey("UILaunchImageFile") ?
                  Instance.Settings["UILaunchImageFile"].Replace(".png", string.Empty) : string.Empty) + "-Detail.png";
        }

        // MonoTouch loves to JIT this if it isn't explicitly declared.
        public TouchFactory() { }
        #endregion

        #region TouchFactory Methods
        #region Public Methods
        /// <summary>
        /// Initializes the Touch factory instance with the given iApp type,
        /// automatically performing all necessary operations and navigations.
        /// If this method is used, nothing else is required in the container.
        /// </summary>
        /// <param name='appType'>
        /// The type of the iApp subclass to instantiate.
        /// </param>
        public static void Initialize(Type appType)
        {
            if (!appType.IsSubclassOf(typeof(iApp)))
            {
                throw new NotSupportedException("The type that is passed in must be a subclass of iApp");
            }
            
            TheApp = (iApp)Activator.CreateInstance(appType);
            iApp.Navigate(TheApp.NavigateOnLoad);
        }

        /// <summary>
        /// Initializes the Touch factory instance.
        /// </summary>
        /// Nothing is needed here.  It's just to force the static constructor.
        public static void Initialize() { }

        /// <summary>
        /// Initializes the views.
        /// </summary>
        [Obsolete("This method is not necessary to call and will be removed in a future framework version.")]
        public void InitializeViews()
        {
            if (topViewController != null)
                return;

            UIViewController master = topViewController = PaneManager.Instance.FromNavContext(Pane.Master, 0) as UIViewController;
            if (master == null)
            {
                master = topViewController = new NavigationController("0");
                PaneManager.Instance.AddStack((IHistoryStack)master, new iApp.AppNavigationContext() { ActivePane = Pane.Master });
            }
            
            var popover = new PopoverNavigationController();
            popover.PushViewController(new VanityView(), false);
            
            PaneManager.Instance.AddStack(popover, new iApp.AppNavigationContext() { ActivePane = Pane.Popover });

            if (LargeFormFactor && PaneManager.IsSplitView)
            {
                topViewController = SplitViewController = new MGSplitViewController();
                
                // create nav controller for detail pane
                var detail = new NavigationController("Detail");
                if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
                {
                    detail.NavigationBar.TintColor = UIColor.Black;
                }

                PaneManager.Instance.AddStack(detail, new iApp.AppNavigationContext() { ActivePane = Pane.Detail });

                // setup the tab or nav view controller for Master Pane
                SplitViewController.SetViewControllers(new UIViewController[]
                {
                    master, detail, popover
                });
            }

            SetRoot(topViewController);
        }

        /// <summary>
        /// Retrieves a full file path for a media file ID.
        /// </summary>
        /// <returns>
        /// The full path to the media file for the specified ID.
        /// </returns>
        /// <param name='mediaId'>
        /// A media file identifier. Returns null if no media file is found for the given ID.
        /// </param>
        public override string RetrieveImage(string mediaId)
        {
            string path = iApp.File.GetFileNames(TempPath).FirstOrDefault(f => f.EndsWith(mediaId));
            if (path == null)
            {
                foreach (string directory in iApp.File.GetDirectoryNames(TempPath))
                {
                    path = iApp.File.GetFileNames(directory).FirstOrDefault(f => f.EndsWith(mediaId));
                    if (path != null)
                        break;
                }
            }

            return path;
        }
        
        /// <summary>
        /// Pops all but the active tab to their root views.
        /// </summary>
        public void PopAllInactiveTabsToRoot()
        {
            if (TabBarController == null)
                return;

            var master = PaneManager.Instance.FromNavContext(Pane.Master, PaneManager.Instance.CurrentTab) as UINavigationController;
            foreach (var controller in TabBarController.ViewControllers.OfType<UINavigationController>())
            {
                if (master != controller)
                {
                    controller.PopToRootViewController(false);
                    if (controller.ViewControllers.Count() > 0)
                        controller.ViewControllers.First().ViewWillAppear(false);
                }
            }
        }
		#endregion

        #region Non-Public Methods
        internal static T GetNativeObject<T>(object obj, string objName)
            where T : class
        {
            if (typeof(T) != typeof(UIViewController) && obj == null)
                return null;

            return TargetFactory.GetNativeObject(obj, objName, typeof(T)) as T;
        }

        protected override bool OnOutputLayer(iLayer layer)
        {   
            if (layer is LoginLayer)
            {
                ((LoginLayer)layer).Display();
                return true;
            }

            if (layer is CameraScanLayer)
			{
				CameraScannerView.Scan(layer as CameraScanLayer);
				return true;
			}
			
            if (layer is ScanLayer)
			{
                ScannerView.Scan((ScanLayer)layer);
				return true;
			}

			return HandleIntegration(layer as Browser) || base.OnOutputLayer(layer);
        }

        protected override void OnSetDefinitions()
        {
            Register<IPlatformDefaults>(typeof(Touch.PlatformDefaults));
            Register<IGeoLocation>(typeof(Touch.GeoLocation));
            Register<ICompass>(typeof(Touch.Compass));
            Register<IAccelerometer>(typeof(Touch.Accelerometer));
            Register<IGridView>(typeof(Touch.GridView));
            Register<IListView>(UIDevice.CurrentDevice.CheckSystemVersion(8, 0) ? typeof(Touch.ListView) : typeof(Touch.ListViewLegacy));
            Register<ITabView>(typeof(Touch.TabView));
            Register<IBrowserView>(typeof(Touch.BrowserView));
            Register<ICanvasView>(typeof(Touch.CanvasView));
            Register<IView>(typeof(VanityView));
            Register<ISectionHeader>(UIDevice.CurrentDevice.CheckSystemVersion(8, 0) ? typeof(SectionHeaderFooter) : typeof(SectionHeaderFooterLegacy));
            Register<ISectionFooter>(UIDevice.CurrentDevice.CheckSystemVersion(8, 0) ? typeof(SectionHeaderFooter) : typeof(SectionHeaderFooterLegacy));
            Register<IGridCell>(typeof(Touch.GridCell));
            Register<IRichContentCell>(typeof(Touch.RichContentCell));
            Register<ILabel>(typeof(Touch.Label));
            Register<IImage>(typeof(Touch.Image));
            Register<IButton>(typeof(Touch.Button));
            Register<ITextBox>(typeof(Touch.TextBox));
            Register<IPasswordBox>(typeof(Touch.PasswordBox));
            Register<ITextArea>(typeof(Touch.TextArea));
            Register<IDatePicker>(typeof(Touch.DatePicker));
            Register<ITimePicker>(typeof(Touch.TimePicker));
            Register<ISelectList>(typeof(Touch.SelectList));
            Register<ISwitch>(typeof(Touch.Switch));
            Register<ISlider>(typeof(Touch.Slider));
            Register<IMenu>(UIDevice.CurrentDevice.CheckSystemVersion(8, 0) ? typeof(Touch.Menu) : typeof(Touch.MenuLegacy));
            Register<IMenuButton>(typeof(Touch.MenuButton));
            Register<IToolbar>(typeof(Touch.Toolbar));
            Register<IToolbarButton>(typeof(Touch.ToolbarButton));
            Register<IToolbarSeparator>(typeof(Touch.ToolbarSeparator));
            Register<ITabItem>(typeof(Touch.TabItem));
            Register<ISearchBox>(typeof(Touch.SearchBox));
            Register<IAlert>(UIDevice.CurrentDevice.CheckSystemVersion(8, 0) ? typeof(Touch.Alert) : typeof(Touch.AlertLegacy));
            Register<ITimer>(typeof(Touch.Timer));
        }

        protected override void OnOutputView(IMXView view)
        {
            var pairable = view as IPairable;
            if (!(view is UIViewController) && (pairable == null || !(pairable.Pair is UIViewController)))
            {
                iApp.Log.Debug("Cannot output a view whose native component is not a UIViewController.");
                return;
            }

            var controller = GetNativeObject<UIViewController>(view, "view") as UITabBarController;
            if (controller != null)
            {
                if (TopViewController.PresentedViewController != null)
                {
                    ModalManager.EnqueueModalTransition(TopViewController, null, true);
                }

				if (KeyWindow.RootViewController is MGSplitViewController)
				{
					SplitViewController.SetViewControllers(new[]
					{
						controller,
						(UIViewController)PaneManager.Instance.FromNavContext(Pane.Detail, 0),
						(UIViewController)PaneManager.Instance.FromNavContext(Pane.Popover, 0)
					});
				}
				else
				{
                	SetRoot(TopViewController = controller);
                }
            }
            else
            {
                PaneManager.Instance.DisplayView(view);
            }
        }

        protected override void OnBeginBlockingUserInput()
        {
            if (!KeyWindow.Subviews.Any(v => v is InteractionBlockerView))
            {
                KeyWindow.Add(new InteractionBlockerView());
            }
        }

        protected override void OnStopBlockingUserInput()
        {
            for (int i = KeyWindow.Subviews.Length - 1; i >= 0; i--)
            {
                var subview = KeyWindow.Subviews[i];
                if (subview is InteractionBlockerView)
                {
                    subview.RemoveFromSuperview();
                }
            }
        }

        protected override void OnControllerLoadBegin(IMXController controller, IMXView fromView)
        {
            base.OnControllerLoadBegin(controller, fromView);

            if (topViewController == null)
            {
                InitializeViews();
            }
        }

        protected override void OnShowLoadIndicator(string title)
        {
            if (topViewController == null)
            {
                InitializeViews();
            }

            if (LoadingHud == null)
            {
                LoadingHud = new LoadSpinner(GetResourceString("Loading"));
            }

            LoadingHud.Title = title;
            LoadingHud.StartAnimating();
        }

        protected override void OnHideLoadIndicator()
        {
            if (LoadingHud != null)
            {
                LoadingHud.StopAnimating();
            }
        }

        protected override object OnGetCustomItem(ICustomItem item, iLayer layer, IListView view, object recycledCell)
        {
            var controller = GetNativeObject<UIViewController>(view, "view") as UITableViewController;
            var cell = GetNativeObject<UITableViewCell>(Converter.ConvertToCell(item, layer.LayerStyle, view, null), "item");

            if (CustomItemRequested != null)
            {
                return CustomItemRequested(item, cell, controller == null ? null : controller.TableView);
            }

            return cell;
        }

        protected override double GetLineHeight(Font font)
        {
            return font.ToUIFont().LineHeight;
        }

        internal static void NotifyOrientationChanged(iApp.Orientation orientation)
        {
            Instance.OnOrientationChanged(orientation);
        }

        internal bool IsSupportedOrientation(UIInterfaceOrientation orientation)
        {
            return Settings.ContainsKey("UISupportedInterfaceOrientations") &&
                Settings["UISupportedInterfaceOrientations"].Split('|').Contains("UIInterfaceOrientation" + orientation.ToString());
        }

        /// <summary>
        /// Stores an image to filesystem, overwriting if a file already exists.
        /// </summary>
        /// <returns>
        /// The image ID.
        /// </returns>
        /// <param name='imageData'>
        /// The image data.
        /// </param>
        /// <param name='imageId'>
        /// An optional image identifier. If null, a GUID will be assigned as the image ID.
        /// </param>
        internal string StoreImage(NSData imageData, string imageId = null)
        {   
            if (null == imageId)
            {
                imageId = Guid.NewGuid().ToString() + ".png";
            }
            
            string path = Path.Combine(TempPath, imageId);
            
            // EXIT if file is not saved
            NSError error;
            if (!imageData.Save(path, false, out error)) 
            { 
                iApp.Log.Error(string.Format("An error occurred saving {0} to file system: {1}", imageId, error.ToString()));
                return null; 
            }
            
            return imageId;
        }
        
        internal UIImage GetImage(string imageId)
        {
            try
            {
                return string.IsNullOrEmpty(imageId) ? null : UIImage.FromFile(Path.Combine(TempPath, imageId));
            }
            catch
            {
                iApp.Log.Error("Failed to get " + imageId);
                return null;
            }
        }
        
        internal void DeleteImage(string imageId)
        {
            try
            {
				if (!string.IsNullOrEmpty(imageId))
				{
                	File.Delete(Path.Combine(TempPath, imageId));
				}
            }
            catch
            {
                iApp.Log.Error("Failed to delete " + imageId);
            }
        }

        private void SetRoot(UIViewController controller)
        {
            if (LoadingHud != null)
            {
                LoadingHud.StopAnimating();
                LoadingHud.Dispose();
                LoadingHud = null;
            }
            KeyWindow.RootViewController = controller;
        }

        private bool HandleIntegration(Browser layer)
        {
            if (layer == null || layer.Url == null)
            {
                return false;
            }

            // mailto won't be encoded correctly
            if (layer.Url.StartsWith("mailto"))
            {
                MailComposer.Compose(layer.Url);
                return true;
            }

            NSUrl url = NSUrl.FromString(System.Web.HttpUtility.UrlPathEncode(layer.Url));
            if (url == null)
            {
                return false;
            }

            switch (url.Scheme)
            {
                case "image":
                case "videorecording":
                {
                    ImagePicker.GetMedia(url.AbsoluteString);
                    return true;
                }
                case "audiorecording":
                case "voicerecording":
                {
                    AudioPlayer.Record(url.AbsoluteString);
                    return true;
                }
				case "audio":
				{
					AudioPlayer.Play(url.AbsoluteString);
					return true;
				}
				case "video":
				{
					VideoPlayer.Play(url.AbsoluteString);
					return true;
				}
				case "scan":
				{
					ScannerView.Scan(url.AbsoluteString);
					return true;
				}
                case "print":
                {
                    Printer.Print(url.AbsoluteString);
                    return true;
                }
                default:
                {
                    if (url.IsExternal())
                    {
                        if (!UIApplication.SharedApplication.OpenUrl(url))
                        {
						    var av = new UIAlertView(GetResourceString("Error"),
						        string.Format(GetResourceString("SchemeNotSupported"), url.Scheme),
                                null, GetResourceString("OK"), null);

                            av.Show();
                        }
                        return true;
                    }
                    return false;
                }
            }
        }
        #endregion
        #endregion
	}
}
