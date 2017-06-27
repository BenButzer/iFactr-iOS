using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using CoreGraphics;
using UIKit;

namespace iFactr.Touch
{
    static class WebViewExtensions
    {
        public static string GetElementRect (this UIWebView webView, string element)
        {
            string script = @"  var target = {0};
                                var x = 0, y = 0;
                                for (var n = target; n && n.nodeType == 1; n = n.offsetParent) {
                                  x += n.offsetLeft;
                                  y += n.offsetTop;
                                }
                                x + ',' + y + ',' + target.offsetWidth + ',' + target.offsetHeight;";
            String.Format(script, element);
            string result = webView.EvaluateJavascript(script);
            return result;
        }   
		
		public static float GetDocumentHeight (this UIWebView webView)
        {
            try
            {
    			return Convert.ToSingle(webView.EvaluateJavascript(@"document.body.scrollHeight;"));
            }
            catch { return 54; }
        }
		
		public static void ClearVideoContent(this UIWebView webView)
		{
			string script = @"
					   		var elements = document.getElementsByTagName('video');
					   		for(var i=0; i< elements.length; i++){
					      		elements[i].pause();
					   		}";
			webView.EvaluateJavascript(script);
		}
		
		public static void AutoplayVideo(this UIWebView webView)
		{
			string script = @"  var elements = document.getElementsByTagName('video');
								if(elements.length > 0){
									elements[0].play();
								}";
			webView.EvaluateJavascript(script);
		}
    }
}
