using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Foundation;
using MessageUI;
using UIKit;

using iFactr.Core.Integrations;

namespace iFactr.Touch
{
    public class MailComposer
    {
        public static void Compose(string url)
        {
            if (!MFMailComposeViewController.CanSendMail)
                return;

			MailTo mailTo = MailTo.ParseUrl(url);

            var mailComposer = new MFMailComposeViewController();
            mailComposer.MailComposeDelegate = new MailComposeDelegate();
            mailComposer.SetToRecipients(mailTo.EmailTo.ToArray());
            mailComposer.SetSubject(mailTo.EmailSubject);
            mailComposer.SetMessageBody(mailTo.EmailBody, true);

            foreach (var attachment in mailTo.EmailAttachments)
            {
                string path = attachment.Filename;
                if (!File.Exists(path))
                {
                    path = Path.Combine(TouchFactory.Instance.DataPath, attachment.Filename);
                }

                NSData data = NSData.FromFile(path);
                if (data != null)
                {
                    mailComposer.AddAttachmentData(data, attachment.MimeType, attachment.Filename);
                }
            }
            
            ModalManager.EnqueueModalTransition(TouchFactory.Instance.TopViewController, mailComposer, true);
        }

        private class MailComposeDelegate : MFMailComposeViewControllerDelegate
        {
            public MailComposeDelegate()
            {
            }

            public override void Finished (MFMailComposeViewController controller, MFMailComposeResult result, NSError error)
            {
                ModalManager.EnqueueModalTransition(controller.PresentingViewController, null, true);
            }
        }
    }
}
