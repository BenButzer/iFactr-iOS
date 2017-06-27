using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using iFactr.Core;
using iFactr.Core.Forms;
using iFactr.Core.Layers;

using ObjCRuntime;
using UIKit;
using CoreGraphics;

namespace iFactr.Touch
{
    public static class LoginLayerExtensions
    {
        private const float DefaultWidth = 300f;
        private const float DefaultHeight = 35f;

        private static UILabel authLabel = new UILabel()
        {
            BackgroundColor = UIColor.Clear,
            Text = TouchFactory.Instance.GetResourceString("AuthLabel"),
        };

        private static UIActivityIndicatorView authActivity = new UIActivityIndicatorView()
        {
            HidesWhenStopped = false
        };

        private class LoginView : UIView
        {
            public LoginView(CGRect frame) : base(frame)
            {
                AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;
            }

            public override void LayoutSubviews()
            {
                base.LayoutSubviews();

                // man, don't even ask
                var subview = Subviews.LastOrDefault();
                if (subview != null)
                {
                    var frame = subview.Frame;
                    frame.Y = 90;
                    subview.Frame = frame;
                }
            }
        }

        internal static void Display(this LoginLayer layer)
        {
            UIViewController controller = new ViewController()
            {
                View = UIDevice.CurrentDevice.CheckSystemVersion(8, 1) && UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone ?
                    new LoginView(UIScreen.MainScreen.ApplicationFrame) : new UIView(UIScreen.MainScreen.ApplicationFrame),
                ModalPresentationStyle = UIModalPresentationStyle.FullScreen,
                ModalTransitionStyle = UIModalTransitionStyle.CrossDissolve,
                Autorotate = UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad
            };

            if (!string.IsNullOrEmpty(layer.LayerStyle.LayerBackgroundImage))
            {
                controller.View.InsertSubview(new UIImageView()
                {
                    Image = UIImage.FromBundle(layer.LayerStyle.LayerBackgroundImage),
                    AutoresizingMask = UIViewAutoresizing.FlexibleMargins,
                    Frame = controller.View.Frame,
                    ContentMode = UIViewContentMode.Center
                }, 0);
            }
            else if (!layer.LayerStyle.LayerBackgroundColor.IsDefaultColor)
            {
                controller.View.BackgroundColor = layer.LayerStyle.LayerBackgroundColor.ToUIColor();
            }

            UIView container = new UIView()
            {
                AutoresizingMask = UIViewAutoresizing.FlexibleMargins
            };

            ModalManager.EnqueueModalTransition(TouchFactory.Instance.TopViewController, controller, true);

            List<UITextField> textFields = new List<UITextField>();
            foreach (TextField field in layer.Items.OfType<Fieldset>().SelectMany(i => i).OfType<TextField>())
            {
                UITextField previous = textFields.LastOrDefault();
                if (previous != null)
                {
                    previous.ReturnKeyType = UIReturnKeyType.Next;
                    previous.ShouldReturn += delegate
                    {
                        textFields[textFields.IndexOf(previous) + 1].BecomeFirstResponder();
                        return false;
                    };
                }

                textFields.Add(CreateTextField(field, (DefaultHeight + 10) * (textFields.Count + 1)));
            }

            UIButton button = new UIButton();
            button.TouchUpInside += (sender, e) => Submit(layer, controller, container, textFields);

            UIImage buttonImage = null;
            if (TouchFactory.Instance.Settings.ContainsKey("LoginButtonImage"))
            {
                buttonImage = UIImage.FromBundle(TouchFactory.Instance.Settings["LoginButtonImage"]);
            }

            if (buttonImage == null)
            {
                buttonImage = TouchStyle.ImageFromResource("loginButton.png");
                button.SetTitle(layer.ActionButtons.FirstOrDefault() == null ? 
				                TouchFactory.Instance.GetResourceString("Login") :
				                layer.ActionButtons.First().Text, UIControlState.Normal);

                button.SetTitleColor(UIColor.White, UIControlState.Normal);
            }

            button.SetBackgroundImage(buttonImage, UIControlState.Normal);
            button.Frame = new CGRect(DefaultWidth - buttonImage.Size.Width,
                    (DefaultHeight + 10) * (textFields.Count + 1), buttonImage.Size.Width, buttonImage.Size.Height);

            container.Frame = new CGRect(0, 0, DefaultWidth, button.Frame.Bottom);
            container.Center = new CGPoint(controller.View.Center.X, controller.View.Center.Y - (button.Frame.Bottom / 2 + 24));
            container.AddSubviews(textFields.ToArray());
            container.AddSubview(button);

            if (layer.BrandImage != null)
            {
                container.AddSubview(new UIImageView()
                {
                    Image = UIImage.FromBundle(layer.BrandImage.Location ?? string.Empty),
                    Center = new CGPoint(container.Frame.Width / 2, DefaultHeight + 10),
                    ContentMode = UIViewContentMode.Bottom
                });
            }

            UITextField last = textFields.LastOrDefault();
            if (last != null)
            {
                last.ReturnKeyType = UIReturnKeyType.Go;
                last.ShouldReturn += delegate
                {
                    Submit(layer, controller, container, textFields);
                    return false;
                };
            }

            authLabel.RemoveFromSuperview();
            authLabel.SizeToFit();
            authLabel.Center = new CGPoint((container.Frame.Width / 2) - (21 / 2), button.Center.Y);
            authLabel.Alpha = 0;
            authLabel.TextColor = layer.LayerStyle.TextColor.IsDefaultColor ? UIColor.Black : layer.LayerStyle.TextColor.ToUIColor();

            authActivity.Frame = new CGRect(authLabel.Frame.Right, authLabel.Frame.Y, 21, 21);
            authActivity.Alpha = 0;
            if (authActivity.RespondsToSelector(new Selector(Selector.GetHandle("color"))))
            {
                authActivity.Color = layer.LayerStyle.TextColor.IsDefaultColor ? UIColor.Black : layer.LayerStyle.TextColor.ToUIColor();
            }

            container.AddSubviews(authLabel, authActivity);
            controller.View.AddSubview(container);
        }

        private static void Submit(LoginLayer layer, UIViewController controller, UIView container, List<UITextField> textFields)
        {
            foreach (UITextField textField in textFields)
            {
                textField.ResignFirstResponder();
            }

            string username = textFields.Count > 0 ? textFields[0].Text ?? string.Empty : string.Empty;
            string password = textFields.Count > 1 ? textFields[1].Text ?? string.Empty : string.Empty;

            iApp.Thread.Start(delegate
            {
                using (new Foundation.NSAutoreleasePool())
                {
                    if (layer.LogIn(username, password))
                    {
                        controller.InvokeOnMainThread(() =>
                        {
                            ModalManager.EnqueueModalTransition(controller.PresentingViewController, null, true);

                            if (layer.LoginLink != null)
                            {
                                layer.LoginLink.LoadIndicatorDelay = -1;
                                iApp.Navigate(layer.LoginLink);
                            }                          
                        });
                    }
                    else
                    {
                        controller.InvokeOnMainThread(() =>
                        {
                            TouchFactory.Instance.StopBlockingUserInput();

                            var alert = new UIAlertView(layer.ErrorText, string.Empty, null, TouchFactory.Instance.GetResourceString("OK"), null);
                            alert.WillDismiss += (sender, e) => AnimateTransparency(container, false);
                            alert.Show();
                        });
                    }

                    authActivity.InvokeOnMainThread(() => authActivity.StopAnimating());
                }
            });

            authActivity.StartAnimating();
            AnimateTransparency(container, true);
        }

        private static UITextField CreateTextField(TextField field, float Y)
        {
            UITextField textField = new LoginTextField(new RectangleF(0, Y, DefaultWidth, DefaultHeight))
            {
                AutocapitalizationType = UITextAutocapitalizationType.None,
                AutocorrectionType = UITextAutocorrectionType.No,
                BackgroundColor = UIColor.White,
                Placeholder = field.Label,
                Text = field.Text,
                TextAlignment = UITextAlignment.Left,
                SecureTextEntry = field.IsPassword,
                KeyboardType = UIKeyboardType.Default
            };

            if (field is EmailField)
            {
                textField.KeyboardType = UIKeyboardType.EmailAddress;
            }
            else if (field is NumericField)
            {
                textField.KeyboardType = UIKeyboardType.NumbersAndPunctuation;
            }

            textField.Layer.CornerRadius = 5f;
			textField.Layer.BorderColor = new UIColor(0.784f, 0.78f, 0.8f, 1).CGColor;
			textField.Layer.BorderWidth = 1f;

            return textField;
        }

        private static void AnimateTransparency(UIView container, bool hideFields)
        {
            UIView.Animate(0.4, () =>
            {
                foreach (UIView field in container.Subviews)
                {
                    if (field is UITextField || field is UIButton)
                    {
                        field.Alpha = hideFields ? 0 : 1;
                    }
                    else if (field == authLabel || field == authActivity)
                    {
                        field.Alpha = hideFields ? 1 : 0;
                    }
                }
            });
        }

        private class LoginTextField : UITextField
        {
            private const int Margin = 5;

            public LoginTextField(CGRect frame) : base(frame) { }

            public override CGRect TextRect(CGRect forBounds)
            {
                return new CGRect(forBounds.X + Margin, forBounds.Y, forBounds.Width - Margin, forBounds.Height);
            }

            public override CGRect EditingRect(CGRect forBounds)
            {
                return new CGRect(forBounds.X + Margin, forBounds.Y, forBounds.Width - Margin, forBounds.Height);
            }
        }
    }
}