using System;
using UIKit;
using Foundation;
using System.Drawing;
using CoreGraphics;

namespace iFactr.Touch
{
    public class DrawingCanvas : UIView
    {
        /// <summary>
        /// Gets or sets the image identifier.
        /// </summary>
        public string ImageId
        {
            get { return imageId; }
            internal set
            {
                imageId = value;
                if (!string.IsNullOrWhiteSpace(imageId))
                {
                	drawBitmap = true;
                    canvas = UIImage.FromFile(imageId);
                    SetNeedsDisplay();
                }
            }
        }
        private string imageId;

        /// <summary>
        /// Gets a UIImage representation of the canvas.
        /// </summary>
        public UIImage Image
        {
            get { return isCleared ? null : incrementalImage; }
        }

        public UIColor StrokeColor
        {
            get { return UIColor.FromCGColor(strokeColor); }
            set { strokeColor = value.CGColor; }
        }
        private CGColor strokeColor = UIColor.Black.CGColor;

        public nfloat StrokeThickness
        {
            get { return path.LineWidth; }
            set { path.LineWidth = value; }
        }

        private UIBezierPath path;
        private UIImage canvas, incrementalImage;
        private CGPoint[] points = new CGPoint[5];
        private nuint center;
        private bool isCleared;
		private bool drawBitmap;

        public DrawingCanvas()
        {
            // ensure user interaction in this view so that its subviews will also support user interaction
            UserInteractionEnabled = true;
            MultipleTouchEnabled = false;

            // setup the image view that will be the transparent "canvas" for capturing touches
            Layer.MasksToBounds = true;
            ContentMode = UIViewContentMode.ScaleAspectFill;
            AutoresizingMask = UIViewAutoresizing.All;
            BackgroundColor = UIColor.Clear;

            path = new UIBezierPath();
            path.LineWidth = 3f;

			drawBitmap = true;
			SetNeedsDisplay();
        }

        /// <summary>
        /// Clears the stroked points and re-draws
        /// </summary>
        public void Clear()
        {
            isCleared = true;

            if (canvas != null)
            {
                canvas.Dispose();
                canvas = null;
            }

            if (incrementalImage != null)
            {
                incrementalImage.Dispose();
                incrementalImage = null;
            }

            SetNeedsDisplay();
        }

        public override void Draw(CGRect rect)
        {
			if (drawBitmap)
			{
				DrawBitmap();
			}
			
            if (incrementalImage == null)
            {
                incrementalImage = new UIImage();
            }

            incrementalImage.Draw(rect);
            UIColor.FromCGColor(strokeColor).SetStroke();
            path.Stroke();
            
            if (drawBitmap)
            {
            	path.RemoveAllPoints();
            	center = 0;
				drawBitmap = false;
            }
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            isCleared = false;
            center = 0;
            UITouch _touch = touches.AnyObject as UITouch;
            points[0] = _touch.LocationInView(this);
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            isCleared = false;
            UITouch touch = touches.AnyObject as UITouch;
            CGPoint point = touch.LocationInView(this);
            center++;
            points[center] = point;
            if (center == 4)
            {
                points[3] = new CGPoint((points[2].X + points[4].X) / 2f, (points[2].Y + points[4].Y) / 2f);
                path.MoveTo(points[0]);
                path.AddCurveToPoint(points[3], points[1], points[2]);
                SetNeedsDisplay();
                points[0] = points[3];
                points[1] = points[4];
                center = 1;
            }
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
			drawBitmap = true;
            SetNeedsDisplay();
        }

        public override void TouchesCancelled(NSSet touches, UIEvent evt)
        {
            TouchesEnded(touches, evt);
        }

        public void DrawBitmap()
        {
            isCleared = false;
            UIGraphics.BeginImageContextWithOptions(this.Bounds.Size, false, 0.0f);
            if (canvas != null)
            {
                CGContext context = UIGraphics.GetCurrentContext();
                context.TranslateCTM(0f, this.Bounds.Size.Height);
                context.ScaleCTM(1.0f, -1.0f);
                context.DrawImage(this.Bounds, canvas.CGImage);
                context.ScaleCTM(1.0f, -1.0f);
                context.TranslateCTM(0f, -this.Bounds.Size.Height);
            }
            if (incrementalImage != null)
            {
                incrementalImage.Draw(CGPoint.Empty);
            }
            UIColor.FromCGColor(strokeColor).SetStroke();
            path.Stroke();
            incrementalImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
        }
    }
}

