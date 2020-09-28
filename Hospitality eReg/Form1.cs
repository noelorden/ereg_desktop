
using ERegEmail;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Topaz.MultiPlatformSDK.Embed.FormFields;
using Topaz.MultiPlatformSDK.Embed.PDFDocuments;
using Topaz.MultiPlatformSDK.Embed.PDFVerfier;

namespace Hospitality_eReg
{
    public partial class Form1 : Form
    {

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        const int MAX_PATH = 255;

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern int GetShortPathName(
            [MarshalAs(UnmanagedType.LPTStr)]
         string path,
            [MarshalAs(UnmanagedType.LPTStr)]
         StringBuilder shortPath,
            int shortPathLength
            );

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        private static extern int ShowWindow(IntPtr hWnd, uint Msg);

        private const uint SW_MINIMIZE = 0x06;
        private const uint SW_RESTORE = 0x09;

        private Progress.Progress _progress;

        public delegate void CloseSignDelegate(object obj);
        private delegate void PDFFocusDelegate(object sender, EventArgs e);

        public static string FileName="";

        private string _novaFile;
        private string _folder;
        private string _file;
        private int _fileIncrement;


        private bool FieldInserted = false;

        private bool _alreadySaved = false;
        private string _finalDocument;

        public Form1(string fname)
        {
            InitializeComponent();

            try
            {
                DrawPanelFields();

                this.Text = String.Concat("eSigner ", "V.", Application.ProductVersion.ToString());

                DateTime dtNow = DateTime.Now;
                _file = (new TimeSpan(dtNow.Hour, dtNow.Minute, dtNow.Second).TotalSeconds).ToString("00000");

                _folder = DateTime.Now.ToString("yyMMdd");
                Directory.CreateDirectory(Path.Combine(Settings.ListenFolder, _folder));
                Directory.CreateDirectory(Path.Combine(Settings.TargetFolder, _folder));

                _novaFile = fname;
                LoadFile2(_novaFile);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private string GetFilePath()
        {
            try
            {
                var fName = _file + "_" + _fileIncrement.ToString() + ".pdf";
                var sourcePath = Path.Combine(Settings.ListenFolder, _folder);
                var fullFilePath = Path.Combine(sourcePath, fName);

                return fullFilePath;
            }
            catch (Exception ex)
            {
                throw new Exception("GetFilePath. " + ex.Message);
            }
        }

        private void LoadFile2(string sourceFile)
        {
            try
            {
                _fileIncrement++;
                var fullFilePath = GetFilePath();

                File.Copy(sourceFile, fullFilePath);

                while (!File.Exists(fullFilePath)){
                    Thread.Sleep(100);
                }

                LoadPDFFile(fullFilePath);
            }
            catch (Exception ex)
            {
                throw new Exception("Load File. " + ex.Message);
            }
        }
        
        private void CopySourceToWorkingFile(string source){
            try
            {
                string dirTemp;
                Settings.WorkingFolder = String.Format("{0:yyMMdd}", DateTime.Now);
                Settings.WorkingFile = String.Format("{0:HHmmssfffff}", DateTime.Now) + ".pdf";

                dirTemp = Settings.ListenFolder + @"\" + Settings.WorkingFolder;

                Directory.CreateDirectory(dirTemp);
                File.Copy(source, dirTemp + @"\" + Settings.WorkingFile);

                dirTemp = Settings.TargetFolder + @"\" + Settings.WorkingFolder;
                Directory.CreateDirectory(dirTemp);

            }
            catch(Exception ex)
            {
                throw ex;
                
            }
        }

        private void PauseWhileFileIsLocked(string file)
        {
            try
            {
                if (File.Exists(file))
                {
                    FileInfo fl = new FileInfo(file);
                    while (IsFileLocked(fl))
                    {
                        Thread.Sleep(100);
                        Application.DoEvents();
                    }
                }

            }
            catch
            {

            }
        }

        /// <summary>
        /// This is to release all variables and Delete files before terminating the Application
        /// </summary>
        private void CleanUp(bool deleteFile=true)
        {
            if (this.WindowState != FormWindowState.Minimized &&
                this.WindowState != FormWindowState.Maximized)
            { 
                Tools.SaveEnvValue("SIGNHEIGHT", this.Height.ToString());
                Tools.SaveEnvValue("SIGNWIDTH", this.Width.ToString());

                Tools.SaveEnvValue("SIGNLOCX", this.Location.X.ToString());
                Tools.SaveEnvValue("SIGNLOCY", this.Location.Y.ToString());
            }

            LoadDummyFile();

            if (deleteFile) { DeleteLastLoadedFile(); }
        }

        /// <summary>
        /// Last loaded file is not needed for troubleshooting or reference
        /// </summary>
        private void DeleteLastLoadedFile()
        {
            try
            {
                File.Delete(GetFilePath());
            }
            catch
            {

            }
        }

        protected virtual bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            return false;
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        private void LoadFile()
        {
            int retry = 10;
            int x = 0;

            try
            {

                string fName = Settings.ListenFolder + @"\" + Settings.WorkingFolder + @"\" + Settings.WorkingFile;
                string tName = Settings.ListenFolder + @"\" + Settings.WorkingFolder + @"\disp" + Settings.WorkingFile;

                File.Copy(fName, tName);
                if (File.Exists(fName))
                {
                    FileInfo fl = new FileInfo(fName);
                    while (IsFileLocked(fl) && x <retry)
                    {
                        x++;
                        MessageBox.Show("File still in used");
                        Thread.Sleep(1000);
                    }

                    LoadDummyFile();
                    LoadPDFFile(tName);

                    Application.DoEvents();
                }
                else
                {
                    MessageBox.Show("File does NOT exist " + fName);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void LoadPDFFile(string fName)
        {
            try
            {
                webBrowser1.Navigate(fName + "#toolbar=0");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void LoadDummyFile()
        {
            try
            {
                webBrowser1.DocumentText = "";
                webBrowser1.Navigate("about:blank");

                PauseWhileFileIsLocked(GetFilePath());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void DrawPanelFields()
        {
            string typeStr;
            int position = -1;
            int posY = 0;
            Label l;
            Point p;
            TextBox txt;
            Button btn;
            Font font;
            bool show;

            font = new Font("Tahoma", 8.0f, FontStyle.Bold);

            for (int i = 1; i <= Settings.NO_OF_CONTROLS; i++)
            {
                if (Tools.GetEnvValue("FLD" + i.ToString() + "SHOW").Trim() == "1")
                {
                    show = true;
                }
                else
                {
                    show = false;
                }
 
                typeStr = Tools.GetEnvValue("FLD" + i.ToString() + "TYPE").Trim();

                if (typeStr == "OPSignature")
                {
                    position = position + 1;

                    posY = 50 * position + 20;
                    btn = new Button
                    {
                        Text = Tools.GetEnvValue("FLD" + i.ToString() + "Head").Trim(),
                        Name = "Btn " + Tools.GetEnvValue("FLD" + i.ToString() + "Name").Trim()
                    };
                    p = new Point(10, posY);
                    btn.Location = p;
                    btn.Width = 285;
                    btn.Height = 34;
                    btn.Font = font;
                    btn.TextAlign = ContentAlignment.MiddleCenter;
                    btn.ForeColor = Color.Black;
                    btn.Click += Btn_Click;
                    panel2.Controls.Add(btn);
                }
                if (show && (typeStr != "None" && typeStr != "Signature!" && typeStr != "Signature" && typeStr != "CheckBox"))
                {
                    position = position + 1;

                    posY = 25 * position + 10;
                    l = new Label
                    {
                        Text = Tools.GetEnvValue("FLD" + i.ToString() + "Head").Trim(),
                        Name = "Label " + Tools.GetEnvValue("FLD" + i.ToString() + "Name").Trim()
                    };
                    p = new Point(10, posY);
                    l.Location = p;
                    l.AutoSize = true;
                    l.Font = font;
                    l.TextAlign = ContentAlignment.MiddleCenter;
                    l.ForeColor = Color.Black;
                    panel2.Controls.Add(l);

                    txt = new TextBox
                    {
                        Name = Tools.GetEnvValue("FLD" + i.ToString() + "Name").Trim()
                    };
                    ;
                    p = new Point(100, posY);// + 20);
                    txt.Location = p;
                    txt.Width = panel2.Width-150;
                    txt.Height = 27;
                    txt.Enabled = true;
                    txt.Font = font;
                    //txt.TextAlign = HorizontalAlignment.Center;
                    txt.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
                    if (typeStr == "MultiLine")
                    {
                        txt.Multiline = true;
                        txt.AcceptsReturn = true;
                        txt.ScrollBars = ScrollBars.Vertical;
                    }
                    panel2.Controls.Add(txt);

                }
            }
        }

        private void Btn_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;

            int x, y, w, h;

            for (int i = 1; i <= Settings.NO_OF_CONTROLS; i++)
            {
                if (btn.Name.ToLower() == ("Btn " + Tools.GetEnvValue("FLD" + i.ToString() + "Name").Trim()).ToLower())
                {
                    x = Convert.ToInt32(Tools.GetEnvValue("FLD" + i.ToString() + "X"));
                    y = Convert.ToInt32(Tools.GetEnvValue("FLD" + i.ToString() + "Y"));
                    w = Convert.ToInt32(Tools.GetEnvValue("FLD" + i.ToString() + "Width"));
                    h = Convert.ToInt32(Tools.GetEnvValue("FLD" + i.ToString() + "Hgt"));

                    TopazForm frm = new TopazForm(btn.Name.ToLower(), x, y, w, h);

                    frm.SignCompletted += Frm_SignCompletted;
                    frm.ShowDialog(this);

                    break;
                }
            }
        }

        private void Frm_SignCompletted(string name, string file, int x, int y, int width, int height)
        {
            string tempFile;

            tempFile = Path.Combine(Settings.AppPath, "sigTempFile.pdf");

            File.Delete(tempFile);

            File.Copy(GetFilePath(), tempFile);

            LoadDummyFile();

            File.Delete(GetFilePath());

            PDFIText.InsertJPEG2(
                                tempFile,
                                GetFilePath(),
                                file,
                                x,
                                y,
                                width,
                                height,
                                true);
            _finalDocument = GetFilePath();
            LoadFile2(GetFilePath());

        }

        private void CloseSign(object obj)
        {
            if (InvokeRequired)
            {
                CloseSignDelegate method = new CloseSignDelegate(CloseSign);
                Invoke(method, obj);
                return;
            }

            try
            {

                if (obj != null)
                {

                    //pythonService.Kill();

                    PDFVerify verify;
                    ISignatureVerificationDetails details;

                    string filename = (string)obj;

                    //CheckSignAndVerify(filename);

                    #region GETDATA

                    byte[] formFieldsDocumentBytes = null;
                    formFieldsDocumentBytes = File.ReadAllBytes(filename);
                    PDFDocument pdfDocument = new PDFDocument(formFieldsDocumentBytes);

                    PDFTextBoxField txtBoxField = new PDFTextBoxField(pdfDocument);

                    string typeStr, name;
                    bool show;
                    TextBox txt;
                    int x;
                    int y;
                    int width;
                    int height;
                    bool last;

                    for (int i = 1; i <= Settings.NO_OF_CONTROLS; i++)
                    {
                        last = false;

                        if (Tools.GetEnvValue("FLD" + i.ToString() + "SHOW").Trim() == "1")
                        {
                            show=true;
                        }else{
                            show = false;
                        }

                        typeStr = Tools.GetEnvValue("FLD" + i.ToString() + "TYPE").Trim();
                        if (show && (typeStr == "Field" || typeStr == "Other"))
                        {
                            name = Tools.GetEnvValue("FLD" + i.ToString() + "Name").Trim();

                            txtBoxField.Metadata.FieldName = name.ToUpper();
                            
                            Control ctn = panel2.Controls[name];
                            txt = (TextBox)ctn;
                            txt.Text = txtBoxField.GetValue();
  
                            typeStr = Tools.GetEnvValue("FLD" + i.ToString() + "TYPE").Trim();
                            name = Tools.GetEnvValue("FLD" + i.ToString() + "NAME").Trim();
                            x = Convert.ToInt32(Tools.GetEnvValue("FLD" + i.ToString() + "X"));
                            y = Convert.ToInt32(Tools.GetEnvValue("FLD" + i.ToString() + "Y"));
                            width = Convert.ToInt32(Tools.GetEnvValue("FLD" + i.ToString() + "Width"));
                            height = Convert.ToInt32(Tools.GetEnvValue("FLD" + i.ToString() + "Hgt"));

                            if (Tools.GetEnvValue("FLD" + i.ToString() + "Last") == "1") { last = true; }

                            for (int pg = 0; pg <= Settings.PageCount; pg++)
                            {
                                if (last && pg > 0)
                                {
                                    break;
                                }
                                else
                                {
                                    if (last) { pg = Settings.PageCount - 1; }
                                }
                           
                                //bool ret = InsertTextboxField(pdfDocument, FieldInserted, name, pg , typeStr, x, y, width, height, show);
                            }
                        }
                        else if (typeStr == "Signature!")
                        {
                            verify = new PDFVerify(pdfDocument);
                            name = Tools.GetEnvValue("FLD" + i.ToString() + "NAME").Trim();
                            details = verify.VerifySignature("Signature");
                            if (details != null)
                            {
                                string resulti = null;
                                switch (details.Result)
                                {
                                    case SignatureVerificationResults.SignatureInvalid:
                                        resulti = "Signature invalid";
                                        break;
                                    case SignatureVerificationResults.SignatureValidDocumentChanged:
                                        resulti = "Signature valid but document changed";
                                        break;                                                     
                                    case SignatureVerificationResults.SignatureValidDocumentNotChanged:
                                        resulti = "Signature valid and document not changed";
                                        break;
                                }
                                
                            }
                            else
                            {
                                MessageBox.Show("NOT FOUND. " + verify.ErrorMessage);
                            }
                        }
                        else if (typeStr == "Operator")
                        {

                        }
                    }

                    pdfDocument.Close();
                    pdfDocument.Dispose();
                    #endregion

                    _finalDocument = filename;
                                        
                    LoadFile2(filename);

                    button1.Enabled = true;
                    button2.Enabled = true;
                    button3.Enabled = true;
                    button1.BackColor = Color.FromArgb(186, 66, 76);
                    button2.BackColor = Color.FromArgb(186, 66, 76);
                    button3.BackColor = Color.FromArgb(186, 66, 76);
                }

                ExtensionMethods.FlashNotification(this);
            }
            catch (Exception ex) 
            {
                this.Show();
                MessageBox.Show("Error in Document. Please sign again." + ex.Message);
            }
        }

        private void CheckSignAndVerify(string fileName)
        {
            string typeStr;
            string name;
            for (int i = 1; i <= Settings.NO_OF_CONTROLS; i++)
            {

                typeStr = Tools.GetEnvValue("FLD" + i.ToString() + "TYPE").Trim();
                if (typeStr == "Signature!" )
                {
                    name = Tools.GetEnvValue("FLD" + i.ToString() + "Name").Trim();
                    VerifySignature(fileName, name);
                }
            }
        }

        public static void VerifySignature(string fileName, string sigName)
        {
            byte[] inputDocument = File.ReadAllBytes(fileName);
            // initialize the PDFDocument class with input document
            PDFDocument document = new PDFDocument(inputDocument);
            // initialize the PDFVerify class with the PDFDocument object
            PDFVerify verify = new PDFVerify(document);
            ISignatureVerificationDetails details =
            verify.VerifySignature(sigName);
            if (details != null)
            {
                string result = null;
                switch (details.Result)
                {
                    case SignatureVerificationResults.SignatureInvalid:
                        result = "Signature invalid";
                        break;
                    case SignatureVerificationResults.SignatureValidDocumentChanged:
                        result = "Signature valid but document changed";
                        break;
                    case SignatureVerificationResults.SignatureValidDocumentNotChanged:
                        result = "Signature valid and document not changed";
                        break;
                }
                Console.WriteLine("Result: {0}", result);
                Console.WriteLine("Message: {0}", details.Message);
                Console.WriteLine("Revision: {0}", details.Revision.ToString());
                Console.WriteLine("TotalRevisions: {0}",
                details.TotalRevisions.ToString());
            }
            else
                Console.WriteLine("Error: {0}", verify.ErrorMessage);
        }

        private void SendEmail(string filename)
        {
            string cName, typeStr, name;
            Control control;
            ComboBox type;
            TextBox txt;

            string email = "";
            #region GetEmailID
            for (int i = 1; i <= 10; i++)
            {
                cName = "fldType" + i.ToString();
                control = panel1.Controls[cName];
                type = (ComboBox)control;

                cName = "fldName" + i.ToString();
                control = panel1.Controls[cName];
                txt = (TextBox)control;


                typeStr = Tools.GetEnvValue("FLD" + i.ToString() + "TYPE").Trim();
                if (typeStr == "Field")
                {
                    name = Tools.GetEnvValue("FLD" + i.ToString() + "Name").Trim();

                    if (name == "FIELD_41")
                    {
                        Control ctn = panel2.Controls[name];
                        txt = (TextBox)ctn;
                        email = txt.Text.ToString() ;
                        break;
                    }


                }
            }
            #endregion

            formMail form = new formMail(filename, txtRoomNo.Text.Trim(), txtCheckInDate.Text.Trim(), txtGuestName.Text.Trim(), email);

            if (form.ShowDialog(this) == DialogResult.OK) {
                this.WindowState = FormWindowState.Minimized;
                SaveDB();
            }else{

            }
        }

        private void SendToPrinterx(string fileName, string printer)
        {
            try
            {
                
                string printFile;
                printFile = Settings.AppPath + @"\eRegPrint.pdf";

                if (File.Exists(printFile))
                {
                    File.Delete(printFile);
                    Thread.Sleep(1000);
                }

                File.Copy(fileName, printFile);

                var pi = new ProcessStartInfo(printFile)
                {
                    UseShellExecute = true
                };

                if (printer.Length > 0)
                {
                    pi.Verb = "PrintTo";
                    pi.Arguments = printer;
                }
                else
                {
                    pi.Verb = "print";
                }

                var process = System.Diagnostics.Process.Start(pi);

                MessageBox.Show("eReg Form has been sent to the Printed", "eReg Printed", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, (MessageBoxOptions)0x40000);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "eReg Printed", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, (MessageBoxOptions)0x40000);
            }
        }

        private void Button10_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Button9_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (Settings.RoomRef && txtRoomNo.Text.Length == 0)
                {
                    MessageBox.Show(this, "Room No is Required.", "Save Form", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    txtRoomNo.Focus();
                    return;
                }
                else if (!Settings.RoomRef && txtComment.Text.Length == 0)
                {
                    MessageBox.Show(this, "Confirmation No is Required.", "Save Form", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    txtComment.Focus();
                    return;
                }

               
                LoadDummyFile();

                AddFields();

                FieldInserted = true;

                button1.Enabled = false;
                button2.Enabled = false;
                button3.Enabled = false;
                button1.BackColor = Color.Gray;
                button2.BackColor = Color.Gray;
                button3.BackColor = Color.Gray;

                _alreadySaved = false;

                //

                if (Settings.DesignMode1)
                {
                    LoadFile2(GetFilePath());

                    button1.Enabled = true;
                    button2.Enabled = true;
                    button3.Enabled = true;
                    button1.BackColor = Color.FromArgb(186, 66, 76);
                    button2.BackColor = Color.FromArgb(186, 66, 76);
                    button3.BackColor = Color.FromArgb(186, 66, 76);
                }
                else
                {
                    txtRoomNo.Enabled = false;
                    SendFileToDevice();
                }

                

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }



        private void SendFileToDevice()
        {
            try
            {
                
                LoadDummyFile();
                
                string fName = GetFilePath();
                
                CloseSignDelegate closeSign = new CloseSignDelegate(CloseSign);
                ThreadObject threadObject = new ThreadObject
                {
                    CloseSign = closeSign,
                    PDFFile = fName,
                    Folder = _folder,
                    FileName = Path.GetFileName(fName),
                    Reference = txtComment.Text
                };
                
                Sign sign = new Sign();
                Thread doneWorkThread = new Thread(sign.DoWork)
                {
                    IsBackground = true
                };
                
                // Start Python
                //pythonService = RunPythonService();

                doneWorkThread.Start(threadObject);
                
                // Loop until worker thread activates. 
                while (!doneWorkThread.IsAlive) ;

                button1.Enabled = false;

                if (this.WindowState != FormWindowState.Minimized)
                {
                    ShowWindow(this.Handle, SW_MINIMIZE);
                }
                

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private Process RunPythonService()
        {
            Process process = new Process();
            // Redirect the output stream of the child process.

            if (Settings.ShowCmdWdw == "1")
            {
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.RedirectStandardOutput = false;
            }
            else
            {
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
            }

            process.StartInfo.CreateNoWindow = true;

            //process.StartInfo.FileName = batFile;
            string python = GetShortPath(Path.Combine(Settings.AppPath, @"EregAPI\python.exe"));
            string pyc = GetShortPath(Path.Combine(Settings.AppPath, @"EregAPI\API\run.pyc"));

            process.StartInfo.FileName = python;
            process.StartInfo.Arguments = pyc;

            process.Start();

            return process;
        }

        private void AddFields()
        {
            try
            {
                string TempFile = GetFilePath();

                byte[] formFieldsDocumentBytes = null;
                formFieldsDocumentBytes = File.ReadAllBytes(TempFile);

                PDFDocument pdfDocument = new PDFDocument(formFieldsDocumentBytes);

                string typeStr;

                for (int i = 1; i <= Settings.NO_OF_CONTROLS; i++)
                {
                    typeStr = Tools.GetEnvValue("FLD" + i.ToString() + "TYPE").Trim();
                    if (typeStr != "None")
                    {
                        //name = Tools.GetEnvValue("FLD" + i.ToString() + "NAME").Trim();

                        InsertField(
                            pdfDocument, i);
                    }
                }

                if (txtComment.Text.Length > 0)
                {

                }
                
                File.WriteAllBytes(TempFile, pdfDocument.GetBytes());
                pdfDocument.Close();
  
            }
            catch (Exception ex)
            {
                throw (ex);
            } 
    
            // get the document bytes and save
            
        }

        private void PopulateFields()
        {
            try
            {
                string TempFile = Settings.ListenFolder + @"\" + Settings.WorkingFolder + @"\" + Settings.WorkingFile;

                byte[] formFieldsDocumentBytes = null;
                formFieldsDocumentBytes = File.ReadAllBytes(TempFile);

                PDFDocument pdfDocument = new PDFDocument(formFieldsDocumentBytes);

                string typeStr;
                string name;

                for (int i = 1; i <= Settings.NO_OF_CONTROLS; i++)
                {
                    typeStr = Tools.GetEnvValue("FLD" + i.ToString() + "TYPE").Trim();
                    if (typeStr != "None")
                    {
                        name = Tools.GetEnvValue("FLD" + i.ToString() + "Name").Trim();

                        PopulateField(
                            pdfDocument, i);
                    }
                }




                File.WriteAllBytes(TempFile, pdfDocument.GetBytes());
                pdfDocument.Close();

                //MessageBox.Show(TempFile);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

            // get the document bytes and save

        }

        private bool PopulateField(PDFDocument pdfDocument, int i)
        {
            bool ret = false;

            string typeStr;
            TextBox txt;
            string name;
            int x, y, width, height;
            Control ctn;

            if (pdfDocument != null)
            {
                typeStr = Tools.GetEnvValue("FLD" + i.ToString() + "TYPE").Trim();
                name = Tools.GetEnvValue("FLD" + i.ToString() + "NAME").Trim();

                x = Convert.ToInt32(Tools.GetEnvValue("FLD" + i.ToString() + "X"));
                y = Convert.ToInt32(Tools.GetEnvValue("FLD" + i.ToString() + "Y"));
                width = Convert.ToInt32(Tools.GetEnvValue("FLD" + i.ToString() + "Width"));
                height = Convert.ToInt32(Tools.GetEnvValue("FLD" + i.ToString() + "Height"));

                if (typeStr == "None")
                {

                }
                else if (typeStr == "Signature" || typeStr == "Signature!")
                {
                }
                else if (typeStr == "CheckBox")
                {
                }
                else
                {
                    PDFTextBoxField txtBoxField = new PDFTextBoxField(pdfDocument);
                    try
                    {
                        txtBoxField.Metadata.FieldName = name.ToUpper();

                        MessageBox.Show("'" + txtBoxField.Metadata.FieldName + "'" + System.Environment.NewLine + "'" + name + "'");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    


                    switch (typeStr)
                    {
                        case "Other":
                        case "MultiLine":
                        case "Field":
                            ctn = panel2.Controls[name];
                            txt = (TextBox)ctn;
                            MessageBox.Show(txtBoxField.Metadata.FieldName + ":= " + txtBoxField.GetValue());
                            if (txt.Text.Trim().Length > 0)
                            {
                                string value = txt.Text;
                                bool result = txtBoxField.Fill(value);
                                if (result)
                                {
                                    MessageBox.Show(txt.Text + " After Assignment:= " + txtBoxField.GetValue());
                                }
                                else
                                {
                                    MessageBox.Show(txtBoxField.ErrorMessage);
                                }

                            }
                            break;
                        


                            //ctn = panel2.Controls[name];
                            //txt = (TextBox)ctn;

                            //if (txt.Text.Trim().Length > 0) { MessageBox.Show(txt.Text); txtBoxField.Fill(txt.Text); }
                            //break;
                        default:
                            break;
                    }

                    //txtBoxField.Metadata.Visibility = VisibilityOptions.Visible;

                    //ret = txtBoxField.Add();
                }

            }

            return ret;
        }

        private bool InsertField(PDFDocument pdfDocument, int i)
        {
            try
            {

                bool ret = false;

                string typeStr;
                string name = "";
                int x = 0, y = 0, width = 0, height = 0;
                bool show = false;
                bool last = false;
                bool disable = false;

                if (pdfDocument != null)
                {
                    typeStr = Tools.GetEnvValue("FLD" + i.ToString() + "TYPE").Trim();
                    
                    Int32.TryParse(Tools.GetEnvValue("FLD" + i.ToString() + "Page"), out int fldPage);


                    if (typeStr == "None")
                    {
                        return false;
                    }
                    else
                    {
                        name = Tools.GetEnvValue("FLD" + i.ToString() + "NAME").Trim();
                        x = Convert.ToInt32(Tools.GetEnvValue("FLD" + i.ToString() + "X"));
                        y = Convert.ToInt32(Tools.GetEnvValue("FLD" + i.ToString() + "Y"));
                        width = Convert.ToInt32(Tools.GetEnvValue("FLD" + i.ToString() + "Width"));
                        height = Convert.ToInt32(Tools.GetEnvValue("FLD" + i.ToString() + "Hgt"));

                        

                        if (Tools.GetEnvValue("FLD" + i.ToString() + "Show") == "1") { show = true; }
                        if (Tools.GetEnvValue("FLD" + i.ToString() + "Last") == "1") { last = true; }
                        if (Tools.GetEnvValue("FLD" + i.ToString() + "Dis") == "1") { disable = true; }
                    }
                    
                    if (typeStr == "None" || typeStr == "OPSignature")
                    {

                    }
                    else if (typeStr == "Signature" || typeStr == "Signature!")
                    {
                        if (fldPage > 0)
                        {
                            PDFSignField signField = new PDFSignField(pdfDocument);
                            signField.Metadata.FieldName = name.ToUpper() + (fldPage).ToString();
                            signField.Metadata.StartingX = x;
                            signField.Metadata.StartingY = y;
                            signField.Metadata.Width = width;
                            signField.Metadata.Height = height;
                            signField.Metadata.PageNumber = fldPage;
                            signField.Metadata.Orientation = OrientationOptions.ZeroDegrees;
                            signField.Metadata.Visibility = VisibilityOptions.Visible;
                            signField.Metadata.Required = true;
                            signField.Style.BorderStyle = BorderStyles.Inset;

                            ret = signField.Add();
                        }
                        else
                        {
                            for (int pg = 1; pg <= Settings.PageCount; pg++)
                            {

                                if (Tools.GetEnvValue("SIGNFRSTPG") == "1")
                                {
                                    if (pg > 1) { break; }
                                }
                                else
                                {
                                    if (last && pg > 1)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        if (last) { pg = Settings.PageCount; }
                                    }
                                }



                                PDFSignField signField = new PDFSignField(pdfDocument);
                                signField.Metadata.FieldName = name.ToUpper() + (pg).ToString();
                                signField.Metadata.StartingX = x;
                                signField.Metadata.StartingY = y;
                                signField.Metadata.Width = width;
                                signField.Metadata.Height = height;
                                signField.Metadata.PageNumber = pg;
                                signField.Metadata.Orientation = OrientationOptions.ZeroDegrees;
                                signField.Metadata.Visibility = VisibilityOptions.Visible;
                                signField.Metadata.Required = true;
                                signField.Style.BorderStyle = BorderStyles.Inset;

                                ret = signField.Add();
                            }
                        }
                    }
                    else if (typeStr == "CheckBox")
                    {
                        PDFCheckBoxField checkBoxField;

                        if (fldPage > 0)
                        {
                            checkBoxField = new PDFCheckBoxField(pdfDocument);
                            // set the PDFCheckBox field Metadata parameters
                            checkBoxField.Metadata.StartingX = x;
                            checkBoxField.Metadata.StartingY = y;
                            checkBoxField.Metadata.Height = height;
                            checkBoxField.Metadata.Width = width;
                            checkBoxField.Metadata.PageNumber = fldPage;
                            checkBoxField.Metadata.Tooltip = "";
                            checkBoxField.Metadata.FieldName = name.ToUpper();
                            checkBoxField.Metadata.Visibility = VisibilityOptions.Visible;
                            checkBoxField.Metadata.Orientation = OrientationOptions.ZeroDegrees;
                            checkBoxField.Metadata.ReadOnly = false;
                            checkBoxField.Metadata.Required = false;
                            checkBoxField.Metadata.Locked = false;
                            // set the PDFCheckBox field style parameters
                            checkBoxField.Style.BorderColor = System.Drawing.Color.Empty;
                            checkBoxField.Style.BorderWidth = BorderWidthOptions.Thin;
                            checkBoxField.Style.FillColor = System.Drawing.Color.Empty;
                            checkBoxField.Style.FontColor = System.Drawing.Color.Empty;
                            checkBoxField.Style.FontFace = FontFaces.Helvetica;
                            checkBoxField.Style.FontSize = 0;
                            checkBoxField.Style.BorderStyle = BorderStyles.Inset;
                            checkBoxField.CheckedByDefault = false;
                            checkBoxField.ExportValue = "Yes";
                            checkBoxField.CheckStyle = CheckStyles.Check;
                            // add the CheckBox field
                            bool result = checkBoxField.Add();
                        }
                        else
                        {
                            for (int pg = 1; pg <= Settings.PageCount; pg++)
                            {
                                if (Tools.GetEnvValue("CHCKFRSTPG") == "1")
                                {
                                    if (pg > 1) { break; }
                                }
                                else
                                {
                                    if (last && pg > 1)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        if (last) { pg = Settings.PageCount; }
                                    }
                                }



                                checkBoxField = new PDFCheckBoxField(pdfDocument);
                                // set the PDFCheckBox field Metadata parameters
                                checkBoxField.Metadata.StartingX = x;
                                checkBoxField.Metadata.StartingY = y;
                                checkBoxField.Metadata.Height = height;
                                checkBoxField.Metadata.Width = width;
                                checkBoxField.Metadata.PageNumber = pg;
                                checkBoxField.Metadata.Tooltip = "";
                                checkBoxField.Metadata.FieldName = name.ToUpper();
                                checkBoxField.Metadata.Visibility = VisibilityOptions.Visible;
                                checkBoxField.Metadata.Orientation = OrientationOptions.ZeroDegrees;
                                checkBoxField.Metadata.ReadOnly = false;
                                checkBoxField.Metadata.Required = false;
                                checkBoxField.Metadata.Locked = false;
                                // set the PDFCheckBox field style parameters
                                checkBoxField.Style.BorderColor = System.Drawing.Color.Empty;
                                checkBoxField.Style.BorderWidth = BorderWidthOptions.Thin;
                                checkBoxField.Style.FillColor = System.Drawing.Color.Empty;
                                checkBoxField.Style.FontColor = System.Drawing.Color.Empty;
                                checkBoxField.Style.FontFace = FontFaces.Helvetica;
                                checkBoxField.Style.FontSize = 0;
                                checkBoxField.Style.BorderStyle = BorderStyles.Inset;
                                checkBoxField.CheckedByDefault = false;
                                checkBoxField.ExportValue = "Yes";
                                checkBoxField.CheckStyle = CheckStyles.Check;
                                // add the CheckBox field
                                bool result = checkBoxField.Add();

                            }
                        }
                        
                    }
                    else
                    {
                        if (fldPage > 0)
                        {
                            ret = InsertTextboxField(pdfDocument, FieldInserted, name, fldPage, typeStr, x, y, width, height, show);
                        }
                        else
                        {
                            for (int pg = 1; pg <= Settings.PageCount; pg++)
                            {


                                if (Tools.GetEnvValue("TBOXFRSTPG") == "1")
                                {
                                    if (pg > 1) { break; }
                                }
                                else
                                {
                                    if (last && pg < (Settings.PageCount))
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        if (last) { pg = Settings.PageCount; }

                                    }
                                }

                                ret = InsertTextboxField(pdfDocument, FieldInserted, name, pg, typeStr, x, y, width, height, show, disable);
                            }
                        }
                        
                    }
                }

                return ret;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw (ex);
            }


        }

        private bool InsertTextboxField(PDFDocument pdfDocument, bool FieldInserted, string name, int page, string typeStr, int x, int y, int width, int height, bool show, bool disable=false)
        {
            string tempData;


            PDFTextBoxField txtBoxField = new PDFTextBoxField(pdfDocument);

            // Set the Field Metadata

            txtBoxField.Metadata.FieldName = name.ToUpper();
            if (txtBoxField.GetValue() == null)
            {
                tempData = " ";
            }
            else
            {
                tempData = txtBoxField.GetValue().Trim() + " ";
            }
            

            if (FieldInserted)
            {
                try
                {
                    txtBoxField.Delete();
                }
                catch
                {

                }
            }



            txtBoxField.Metadata.StartingX = x;
            txtBoxField.Metadata.StartingY = y;
            txtBoxField.Metadata.Width = width;
            txtBoxField.Metadata.Height = height;

            txtBoxField.Metadata.PageNumber = page;
            txtBoxField.Metadata.Orientation = OrientationOptions.ZeroDegrees;
            txtBoxField.TextAlign = TextAlignOptions.Left;
            txtBoxField.Metadata.ReadOnly = disable;
            txtBoxField.DefaultValue = tempData;
            txtBoxField.Fill(tempData);

            switch (typeStr)
            {
                case "Timestamp":
                case "Other":
                case "MultiLine":
                case "Field":
                case "RoomNo":
                case "GuestName":
                case "CheckInDate":
                case "ReferenceNo":
                    if (typeStr == "MultiLine")
                    {
                        txtBoxField.IsMultiline = true;
                        txtBoxField.ScrollText = true;
                    }
                    else
                    {
                        txtBoxField.IsMultiline = false;
                        txtBoxField.ScrollText = false;
                    }

                    if (typeStr == "Timestamp")
                    {
                        txtBoxField.Metadata.ReadOnly = true;
                        txtBoxField.Style.FontColor = Color.Gray;
                        txtBoxField.Style.FontFace = FontFaces.TimesRoman;
                        if (Settings.FontSize > 0)
                        {
                            txtBoxField.Style.FontSize = (ushort)(Settings.FontSize - 2);
                        }
                        else
                        {
                            txtBoxField.Style.FontSize = 9 - 2;
                        }

                        DateTime timeStamp = DateTime.Now;
                        tempData = "Digitally Signed on " + timeStamp.ToString("ddMMMyy") + " at " + timeStamp.ToString("HH:mm:ss");
                    }
                    else
                    {
                        switch (typeStr)
                        {
                            case "RoomNo":
                                tempData = txtRoomNo.Text.Trim();
                                txtBoxField.Metadata.ReadOnly = true;
                                break;
                            case "GuestName":
                                tempData = txtGuestName.Text.Trim();
                                
                                txtBoxField.Metadata.ReadOnly = true;
                                break;
                            case "CheckInDate":
                                tempData = txtCheckInDate.Text.Trim();
                                txtBoxField.Metadata.ReadOnly = true;
                                break;
                            case "ReferenceNo":
                                tempData = txtReference.Text.Trim();
                                txtBoxField.Metadata.ReadOnly = true;
                                break;
                            default:
                                if (Settings.DesignMode1)
                                {
                                    //txtBoxField.Style.FillColor = Color.Aqua;
                                }
                                break;
                        }

                        txtBoxField.Style.FontColor = Color.Black;
                        txtBoxField.Style.FontFace = FontFaces.TimesRoman;
                        if (Settings.FontSize > 0)
                        {
                            txtBoxField.Style.FontSize = (ushort)Settings.FontSize;
                        }
                        else
                        {
                            txtBoxField.Style.FontSize = 9;
                        }



                    }

                    if (typeStr == "RoomNo" || typeStr == "GuestName" || typeStr == "CheckInDate" || typeStr == "ReferenceNo" || typeStr == "Timestamp")
                    {
                        SetStaticData(txtBoxField, tempData);
                    }
                    else if (!show)
                    {

                        txtBoxField.DefaultValue = tempData;
                    }
                    else
                    {
                        SetData(txtBoxField, name);



                    }



                    break;


                default:
                    break;
            }


            txtBoxField.Metadata.Visibility = VisibilityOptions.Visible;

            return txtBoxField.Add();
        }

        private void SetData(PDFTextBoxField txtBoxField, string name)
        {
            Control ctn = panel2.Controls[name];
            TextBox txt = (TextBox)ctn;

            txtBoxField.DefaultValue = txt.Text;
            txtBoxField.Fill(txt.Text);
            if (txt.Text.Trim().Length > 0)
            {
                txtBoxField.Style.FillColor = Color.White;
            }
        }

        private void SetStaticData(PDFTextBoxField txtBoxField, string value)
        {
            txtBoxField.DefaultValue = value;
            txtBoxField.Fill(value);
            if (value.Trim().Length > 0)
            {
                txtBoxField.Style.FillColor = Color.White;
            }
        }

        private bool InsertField(PDFDocument pdfDocument, string type, int x, int y, int width, int height)
        {
            bool ret = false;

            if (pdfDocument != null)
            {
                if (type == "Signature" || type == "Signature!")
                {
                    PDFSignField signField = new PDFSignField(pdfDocument);
                    signField.Metadata.FieldName = type.ToUpper();
                    signField.Metadata.StartingX = x;
                    signField.Metadata.StartingY = y;
                    signField.Metadata.Width = width;
                    signField.Metadata.Height = height;
                    signField.Metadata.PageNumber = 1;
                    signField.Metadata.Orientation = OrientationOptions.ZeroDegrees;
                    signField.Metadata.Visibility = VisibilityOptions.Visible;
                    signField.Metadata.Required = true;

                    ret = signField.Add();
                }
                else
                {

                    PDFTextBoxField txtBoxField = new PDFTextBoxField(pdfDocument);


                    // Set the Field Metadata
                    txtBoxField.Metadata.FieldName = type.ToUpper();
                    txtBoxField.Metadata.StartingX = x;
                    txtBoxField.Metadata.StartingY = y;
                    txtBoxField.Metadata.Width = width;
                    txtBoxField.Metadata.Height = height;
                    txtBoxField.Metadata.PageNumber = 1;
                    txtBoxField.Metadata.Orientation = OrientationOptions.ZeroDegrees;
                    txtBoxField.TextAlign = TextAlignOptions.Left;

                    txtBoxField.DefaultValue = "";

                    switch (type)
                    {
                        case "Address":
                            txtBoxField.IsMultiline = true;
                            txtBoxField.ScrollText = true;
                            break;
                        case "EMail":
                            txtBoxField.Metadata.Required = true;
                            if (txtEmail.Text.Trim().Length > 0) { txtBoxField.DefaultValue = txtEmail.Text; }
                            break;
                        case "Telephone":
                            if (txtTelephone.Text.Trim().Length > 0) { txtBoxField.DefaultValue = txtTelephone.Text; }
                            break;
                        case "Other":
                        case "MultiLine":
                            if (txtDocumentNo.Text.Trim().Length > 0) { txtBoxField.DefaultValue = txtDocumentNo.Text; }
                            break;
                        case "DOB":
                            if (txtBirthDate.Text.Trim().Length > 0) { txtBoxField.DefaultValue = txtBirthDate.Text; }
                            break;
                    }

                    txtBoxField.Metadata.Visibility = VisibilityOptions.Visible;

                    ret = txtBoxField.Add();
                }

            }

            return ret;
        }

        private string GetFileName()
        {
            try
            {
                return "[" + Path.GetFileName(GetFilePath()) + "] ";
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private string GetFileName(string fname)
        {
            try
            {
                return "[" + Path.GetFileName(fname) + "] ";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool _skipCheck = false;
        private bool CheckClose()
        {
            if (!_alreadySaved)
            {
                if (Tools.GetEnvValue("IOSTABLET").Trim() == "1" && Tools.GetEnvValue("USBCONNECT").Trim() == "1")
                {
                    CleanUp();
                    return false;
                }
                else
                {
                    if (MessageBox.Show(this, "Do you wish to cancel Document Signing?", "Close Application", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                    {

                        CleanUp(false);

                        return false;
                    }
                    else
                    {
                        _skipCheck = true;
                        return true;
                    }
                }
                    

                
            }
            else
            {
                CleanUp();
                return false;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

            e.Cancel = CheckClose();
        }

        private void MenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void MenuStrip1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);

            }
        }

        private void SettingsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void QuitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
            Application.Exit();
        }

        private void SettingsToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Settings form = new Settings();

            form.ShowDialog(this);
        }

        private void TxtRoomNo_TextChanged(object sender, EventArgs e)
        {
            
            
        }

        private void CleaData()
        {
            Control control;
            string cName;

            ComboBox type;
            TextBox field;

            string typeStr;
            TextBox txt;
            string name;

            for (int i = 1; i <= 10; i++)
            {
                cName = "fldType" + i.ToString();
                control = panel1.Controls[cName];
                type = (ComboBox)control;

                cName = "fldName" + i.ToString();
                control = panel1.Controls[cName];
                field = (TextBox)control;


                typeStr = Tools.GetEnvValue("FLD" + i.ToString() + "TYPE").Trim();
                if (typeStr == "Field")
                {
                    name = Tools.GetEnvValue("FLD" + i.ToString() + "Name").Trim();
                    Control ctn = panel2.Controls[name];
                    txt = (TextBox)ctn;
                    txt.Text = "";
                }
            }
        }

        private void WriteData(SqlDataReader record)
        {
            Control control;
            string cName;

            ComboBox type;
            TextBox field;

            string typeStr;
            TextBox txt;
            string name;

            for (int i = 1; i <= 10; i++)
            {
                cName = "fldType" + i.ToString();
                control = panel1.Controls[cName];
                type = (ComboBox)control;

                cName = "fldName" + i.ToString();
                control = panel1.Controls[cName];
                field = (TextBox)control;


                typeStr = Tools.GetEnvValue("FLD" + i.ToString() + "TYPE").Trim();
                if (typeStr == "Field")
                {
                    name = Tools.GetEnvValue("FLD" + i.ToString() + "NAME").Trim();
                    Control ctn = panel2.Controls[name];
                    txt = (TextBox)ctn;
                    txt.Text = record[name].ToString();
                }
            }
        }

        private void TmrFocus_Tick(object sender, EventArgs e)
        {
            tmrFocus.Enabled = false;
            txtRoomNo.Focus();
        }

                private void Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Button2_Click(object sender, EventArgs e)
        {

            try
            {
                this.WindowState = FormWindowState.Minimized ;
                _progress = new Progress.Progress();

                _progress.ShowProgress(this);

                SendAutomatedMailAndSave();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "eReg Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

        }
        
        private void SendAutomatedMailAndSave()
        {
            try
            {
                if (Tools.GetEnvValue("RECIPIENTS").Trim().Length > 0)
                {
                    #region GetStaticSettings                    
                    if (Settings.DesignMode1)
                    {
                        _finalDocument = GetFilePath();
                    }
                    #endregion

                    #region GetMailObject

                    MailObject mailObject = new MailObject
                    {
                        Server = Tools.GetMailServerDetails(),
                        Account = Tools.GetMailAccountDetails(),

                        Recipients = Tools.PBDeCrypt(Tools.GetEnvValue("RECIPIENTS").Trim()).Trim(),
                        Subject = Tools.GetEnvValue("AUTO_SUBJ").Trim()
                    };

                    string body = File.ReadAllText(Tools.GetEnvValue("HTMLBODY").Trim());
                    mailObject.Body = Formatter.FormatEmailBody(body, txtRoomNo.Text.Trim(), txtCheckInDate.Text.Trim(), txtGuestName.Text.Trim(), "");
                    mailObject.Signature = Tools.GetEnvValue("SIGNATURE").Trim();
                    mailObject.SignLink = Tools.GetEnvValue("SIGNLINK").Trim();

                    string[] files = { _finalDocument };
                    mailObject.FileAttachments = files;

                    #endregion

                    Mail mail = new Mail(mailObject);
                    mail.SendMailCompletted += Mail_SendMailCompletted;
                    Thread workerThread = new Thread(mail.SendMail);

                    workerThread.Start(null);
                    while (!workerThread.IsAlive) ;
                }else{
                    SaveDB();
                    _progress.CloseProgress();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

        void Mail_SendMailCompletted(MailResult arg)
        {
            if (InvokeRequired)
            {
                Mail.MailComplettedDelegate method = new Mail.MailComplettedDelegate(Mail_SendMailCompletted);
                Invoke(method, arg);
                return;
            }

            try
            {
                if (arg.Success)
                {
                    SaveDB();
                }
                else
                {
                    MessageBox.Show(this, arg.Message, "eReg Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "eReg Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            _progress.CloseProgress();
        }

        private void SaveDB()
        {
            try
            {
                if (Settings.DesignMode1)
                {
                    _finalDocument = GetFilePath();
                }
                // Save Document using API



                _alreadySaved = true;

                CleanUp();
                Application.Exit();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void Label10_Click(object sender, EventArgs e)
        {

        }

        private void Button3_Click(object sender, EventArgs e)
        {
            if (Settings.DesignMode1)
            {
                _finalDocument = GetFilePath();
            }
            SendEmail(_finalDocument);            
        }

        private void Button4_Click_1(object sender, EventArgs e)
        {

            

            if (Tools.GetEnvValue("SIGNPRINT").Trim() == "1")
            {
                webBrowser1.Print();
            }
            else
            {
                webBrowser1.Print();
            }
        }

        private static string GetShortPath(string path)
        {
            var shortPath = new StringBuilder(MAX_PATH);
            GetShortPathName(path, shortPath, MAX_PATH);
            return shortPath.ToString();
        }

        

        private void button1_MouseLeave(object sender, EventArgs e)
        {
            //((Button)sender).BackColor = Color.FromArgb(186, 66, 76);
        }

        private void button5_Click(object sender, EventArgs e)
        {

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            try
            {
                LoadFile2(_novaFile);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error!" + ex.Message);
            }
            
        }

        private void button6_Click(object sender, EventArgs e)
        {
            webBrowser1.ShowSaveAsDialog();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (button7.Tag.ToString()== "eye")
            {
                button7.Tag = "eyeclosed";
                panel2.Show();
                button7.Image = Hospitality_eReg.Properties.Resources.eyeclose2;
                this.Refresh();
            }
            else
            {
                button7.Tag = "eye";
                panel2.Hide();
                button7.Image = Hospitality_eReg.Properties.Resources.eye;
                this.Refresh();
            }
        }
    }

    public class ThreadObject
    {
        public Hospitality_eReg.Form1.CloseSignDelegate CloseSign;
        public string PDFFile;
        public string Folder;
        public string FileName;
        public string Reference;
    }

    public class FieldObject
    {
        public string FieldName;
        public string FieldData;
    }
}
