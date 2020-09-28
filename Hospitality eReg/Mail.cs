using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace Hospitality_eReg
{
    internal class Mail
    {
        private readonly string SEPARATOR = ",";

        public delegate void MailComplettedDelegate(MailResult arg);

        public event MailComplettedDelegate SendMailCompletted;

        private MailObject _mailObject;

        public Mail(MailObject mailObject)
        {
            _mailObject = mailObject;
        }

        protected virtual void OnSendMailCompletted(MailResult s)
        {            
            SendMailCompletted(s);
        }

        private string GetRecipients(string recipients)
        {
            string ret = "";
            try
            {
                string[] recipientArr = recipients.Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
                ret = String.Join(SEPARATOR, recipientArr);
                return ret;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private MailMessage GetMailMessage(MailObject mailObject)
        {

            try
            {
                

                MailMessage mm = new MailMessage();
                mm.BodyEncoding = UTF8Encoding.UTF8;
                mm.IsBodyHtml = true;
                mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

                MailAddress senderMailAddress=new MailAddress(mailObject.Account.SenderEMail, mailObject.Account.SenderName);
                mm.From = senderMailAddress;
                mm.ReplyToList.Add(senderMailAddress);
                mm.To.Add(GetRecipients(mailObject.Recipients));
                mm.Subject = mailObject.Subject;

                mm.Headers.Add ("Disposition-Notification-To", mailObject.Account.SenderEMail); 
                //for delivery receipt
                mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnSuccess | DeliveryNotificationOptions.OnFailure;

                Attachment att;
                foreach (string litem in mailObject.FileAttachments)
                {
                    att = new Attachment(litem);
                    mm.Attachments.Add(att);
                }

                AlternateView alternateView;
                string htmlBody = mailObject.Body;

                if (mailObject.Signature.Trim().Length>0)
                {

                    LinkedResource resource = new LinkedResource(mailObject.Signature.Trim());
                    resource.ContentId = Guid.NewGuid().ToString();

                    htmlBody = String.Format(htmlBody + @"<br><a href='" + mailObject.SignLink + @"'><img src=""cid:{0}"" /></a>", resource.ContentId);

                    alternateView = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);

                    alternateView.LinkedResources.Add(resource);
                }
                else
                {

                    alternateView = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);
                }
  
                mm.AlternateViews.Add(alternateView);
        
                return mm;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SendMail(object obj)
        {
            MailResult res = null;
            try
            {
                MailObject mailObject = _mailObject;

                SmtpClient client = new SmtpClient();
                client.Host = mailObject.Server.Host;
                client.Port = mailObject.Server.Port;
                client.EnableSsl = mailObject.Server.EnableSSL;
                client.Timeout = mailObject.Server.TimeOut;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                
                client.Credentials = new System.Net.NetworkCredential(mailObject.Account.ID, mailObject.Account.Password);
                
                client.Send(GetMailMessage(mailObject));

                res = new MailResult();
                res.Required = true;
                res.Success = true;
                res.Message = "Mail Sent";
                OnSendMailCompletted(res);
            }
            catch (Exception ex)
            {
                res = new MailResult();
                res.Required = true;
                res.Success = false;
                res.Message = ex.Message;

                OnSendMailCompletted(res);
            }
            
        }
    }

    public class MailServer{
        public string Host;
        public int Port;
        public bool EnableSSL;
        public int TimeOut;
    }

    public class MailAccount{
        public string ID;
        public string Password;
        public string SenderEMail;
        public string SenderName;
    }

    public class MailObject
    {
        public MailServer Server;
        public MailAccount Account;

        public string Recipients;
        public string Subject;
        public string Body;
        public string Signature;
        public string SignLink;
        public string[] FileAttachments;

    }

    public class MailResult
    {
        public bool Required;
        public bool Success;
        public string Message;
    }
}
