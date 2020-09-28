using ERegEmail;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Hospitality_eReg
{
    public partial class formMail : Form
    {
        

        public delegate void MailSent(bool success, string ErrorMssg = "");

        string _attachment;
        string _emailTo;
        string _roomNo;
        string _checkInDate;
        string _guestName;


        public formMail(string attachment, string roomNo, string checkInDate, string guestName, string emailTo)
        {
            InitializeComponent();

            string path = Path.GetDirectoryName(attachment);
            DateTime dt = DateTime.Now;

            _attachment = path + @"\" + Path.GetFileNameWithoutExtension(Settings.SettingConfigFile) + "_" + Path.GetFileName(attachment);
            File.Copy(attachment, _attachment);
            //_attachment = attachment;
            _roomNo = roomNo;
            _checkInDate = checkInDate;
            _guestName = guestName;
            _emailTo = emailTo;

            txtFrom.Text = Tools.GetEnvValue("MAILNAME").Trim() + " <" + Tools.GetEnvValue("MAILADD").Trim() + ">";

            MailAttachments att = new MailAttachments(_attachment);
            listBox1.Items.Add(att);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            btnSend.Enabled = false;
            button1.Enabled = false;
            label4.Visible = true;
            progressBar1.Visible = true;
            Cursor prevCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                if (txtEMail.Text.Trim().Length > 3) { SendMail(); };
            }
            catch(Exception ex)
            {
                btnSend.Enabled = true;
                button1.Enabled = true;
                label4.Visible = false;
                progressBar1.Visible = false;
                MessageBox.Show(this, ex.Message, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            Cursor.Current = prevCursor;
        }

        private void SendMail()
        {

            #region GetMailObject

            MailObject mailObject = new MailObject();
            mailObject.Server = Tools.GetMailServerDetails();
            mailObject.Account = Tools.GetMailAccountDetails();
            if (Tools.GetEnvValue("RECIPIENTS").Trim().Length > 0)
            {
                mailObject.Recipients = txtEMail.Text.Trim() + "," + Tools.PBDeCrypt(Tools.GetEnvValue("RECIPIENTS").Trim()).Trim();
            }
            else
            {
                mailObject.Recipients = txtEMail.Text.Trim() ;
            }

            mailObject.Subject = txtSubject.Text.Trim();

            string body = File.ReadAllText(Tools.GetEnvValue("HTMLBODY").Trim());

            mailObject.Body = Formatter.FormatEmailBody(body, _roomNo, _checkInDate, _guestName, _emailTo);

            mailObject.Signature = Tools.GetEnvValue("SIGNATURE").Trim();

            mailObject.SignLink = Tools.GetEnvValue("SIGNLINK").Trim();

            string[] files = new string[listBox1.Items.Count];
            int idx = 0;

            foreach (MailAttachments litem in listBox1.Items)
            {
                files[idx] = litem.GetFilePath();
                idx++;
            }
            mailObject.FileAttachments = files;

            #endregion

            Mail mail = new Mail(mailObject);
            mail.SendMailCompletted += mail_SendMailCompletted;

           

            Thread workerThread = new Thread(mail.SendMail);

            workerThread.Start(null);
            while (!workerThread.IsAlive) ;
        }

        private void mail_SendMailCompletted(MailResult arg)
        {
            if (InvokeRequired)
            {
                Mail.MailComplettedDelegate method = new Mail.MailComplettedDelegate(mail_SendMailCompletted);
                Invoke(method, arg);
                return;
            }

            try
            {
                //this.WindowState = FormWindowState.Normal;
                if (arg.Success)
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show(this, arg.Message, "Send Mail Failed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                label4.Visible = false;
                progressBar1.Visible = false;
                Cursor.Current = Cursors.Default;
                btnSend.Enabled = true;
                button1.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "SendMail Error " + ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            btnSend.Enabled = true;
            label4.Visible = false;
            progressBar1.Visible = false;

        }

        private void MailDone(bool success, string ErrorMssg)
        {
            if (InvokeRequired)
            {
                MailSent method = new MailSent(MailDone);
                Invoke(method, success, ErrorMssg);
                return;
            }

            if (success)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show(this, ErrorMssg, "Send Mail Failed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            label4.Visible = false;
            progressBar1.Visible = false;
            Cursor.Current = Cursors.Default;
            btnSend.Enabled = true;
        }


        private void Email_Load(object sender, EventArgs e)
        {
            // 
            txtEMail.Text = _emailTo;
            txtSubject.Text = Tools.GetEnvValue("AUTO_SUBJ").Trim();
            txtEMail.Focus();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                string attFile = openFileDialog1.FileName;
                MailAttachments att = new MailAttachments(attFile);
                listBox1.Items.Add(att);
            }
        }

        private void formMail_FormClosing(object sender, FormClosingEventArgs e)
        {
            
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
    }

    public class MailAttachments
    {
        public string _fileName;
        public string _filePath;

        public MailAttachments(string filePath)
        {
            _filePath = filePath;
            _fileName = Path.GetFileNameWithoutExtension(filePath);
        }

        public override string ToString()
        {
            return _fileName;
        }

        public string GetFilePath()
        {
            return _filePath;
        }
    }

    public class SendMailClass
    {

        SendMailObject _sendmailobject;

        public void SendMail(Object obj)
        {
            _sendmailobject = (SendMailObject)obj;

            try
            {   
                SmtpClient client = new SmtpClient(Tools.GetEnvValue("MAILSERVER").Trim(), Convert.ToInt32(Tools.GetEnvValue("MAILPORT").Trim()));
                client.Port = Convert.ToInt32(Tools.GetEnvValue("MAILPORT").Trim());

                client.Host = Tools.GetEnvValue("MAILSERVER").Trim();

                client.EnableSsl = (Tools.GetEnvValue("MAILSSL").Trim() == "1") ? true : false;

                int tout;
                Int32.TryParse(Tools.GetEnvValue("MAIL_TOUT"), out tout);
                if (tout == 0) { tout = 20000; }
                client.Timeout = tout;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                
                client.Credentials = new System.Net.NetworkCredential(Tools.GetEnvValue("MAILID").Trim(), Tools.PBDeCrypt(Tools.GetEnvValue("MAILPW").Trim()).Trim());

                MailMessage mm = new MailMessage(Tools.GetEnvValue("MAILID").Trim(), _sendmailobject.Email, _sendmailobject.Subject, _sendmailobject.Body);

                string recipient = Tools.GetEnvValue("RECIPIENTS").Trim();
                if (recipient.Length > 0)
                {
                    recipient = Tools.PBDeCrypt(recipient).Trim();
                    string[] emails = recipient.Split(',');
                    foreach (string m in emails)
                    {
                        mm.Bcc.Add(new MailAddress(m.Trim()));
                    }
                }

                Attachment att;
                MailAttachments attItem;
                foreach (var litem in _sendmailobject.ListAttachments.Items)
                {
                    attItem = (MailAttachments)litem;
                    att = new Attachment(attItem.GetFilePath());
                    mm.Attachments.Add(att);
                }


                mm.BodyEncoding = UTF8Encoding.UTF8;
                mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                client.Send(mm);
                
                _sendmailobject.MailDone(true);
            }
            catch (Exception ex)
            {
                _sendmailobject.MailDone(false, ex.Message);
            }

        }
    }

    public class SendMailObject
    {
        public Hospitality_eReg.formMail.MailSent MailDone;
        public string Email;
        public ListBox ListAttachments;
        public string Subject;
        public string Body;
    }
}
