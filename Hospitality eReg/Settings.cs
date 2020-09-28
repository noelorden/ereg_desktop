using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Hospitality_eReg
{
    public partial class Settings : Form
    {
        public const int NO_OF_CONTROLS=60;
        public const int COMM_DELAY_DEFAULT = 5000;


        public static string DataPath;
        public static string SettingConfigFile;
        public static string ListenFolder;
        public static string TargetFolder;

        public static string WorkingFolder;
        public static string WorkingFile;

        public static string AppPath;
        public static string JavaPath;
        public static string PauseReg;
        public static string ShowCmdWdw;
        public static bool StandAlone;
        public static bool RoomRef;
        public static bool DesignMode1;
        public static bool DebugMode;
        public static string Font1;
        public static int FontSize;
        public static bool MailEnable;
        public static int RoomLength;
        public static string APIURL;
        public static string APIPORT;
        public static string DeviceID;
        public static bool OptSign;
        public static int OptSignX;
        public static int OptSignY;
        public static string Special;

        // Header LOGO Variables
        public static string HeaderLogo;
        public static int HeaderLocationX;
        public static int HeaderLocationY;
        public static bool HeaderFirstPageOnly;

        // Footer LOGO Variables
        public static string FooterLogo;
        public static int FooterLocationX;
        public static int FooterLocationY;
        public static bool FooterFirstPageOnly;

        public static int PageCount;

        public const string MAIL_BODY = "*** This is an automatically generated email, please do not reply ***";

        public Settings()
        {
            InitializeComponent();
        }

        private void TextBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void Label2_Click(object sender, EventArgs e)
        {

        }

        private void Label1_Click(object sender, EventArgs e)
        {

        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void Button3_Click(object sender, EventArgs e)
        {
            Control control;
            string cName;

            ComboBox type;
            TextBox head;
            TextBox field;
            TextBox fldX;
            TextBox fldY;
            TextBox fldWidth;
            TextBox fldHeight;
            TextBox fldPage;
            CheckBox fldShow;
            CheckBox fldLast;
            CheckBox fldDisable;

            bool ret = false;
            string listen = txtListen.Text.Trim();
            string target = txtTarget.Text.Trim();

            try
            {
                if (listen.Length > 0 && target.Length > 0)
                {
                    if (Directory.Exists(listen) && Directory.Exists(target))
                    {
                        Tools.SaveEnvValue("SIGNLISTEN", listen);
                        Tools.SaveEnvValue("SIGNTARGET", target);
                        ret = true;
                    }
                }

                if (chkDisablePrint.Checked)
                {
                    Tools.SaveEnvValue("REMPRINT", "1");
                }
                else
                {
                    Tools.SaveEnvValue("REMPRINT", "");
                }

                if (chkOpSign.Checked)
                {
                    Tools.SaveEnvValue("OPTSIGN", "1");
                    Tools.SaveEnvValue("OPTSIGNX", txtOptSignX.Text.Trim());
                    Tools.SaveEnvValue("OPTSIGNY", txtOptSignY.Text.Trim());
                }
                else
                {
                    Tools.SaveEnvValue("OPTSIGN", "");
                    Tools.SaveEnvValue("OPTSIGNX", "");
                    Tools.SaveEnvValue("OPTSIGNY", "");
                }

                Tools.SaveEnvValue("SIGNFONTSZ", txtFontSize.Text.Trim());
                Tools.SaveEnvValue("SIGNFONT", txtFont.Text.Trim());

                Tools.SaveEnvValue("API_IP", txtAPIIP.Text.Trim());
                Tools.SaveEnvValue("API_PORT", txtAPIPort.Text.Trim());
                Tools.SaveEnvValue("DEVICEID", txtDeviceID.Text.Trim());
                Tools.SaveEnvValue("USBCONNECT", chkUSB.Checked?"1":"");
                Tools.SaveEnvValue("COMMDELAY", txtCommDelay.Text.Trim());

                Tools.SaveEnvValue("LOGOHDR", txtLogoHeader.Text.Trim());
                Tools.SaveEnvValue("LOGOHDRX", txtLogoHeaderX.Text.Trim());
                Tools.SaveEnvValue("LOGOHDRY", txtLogoHeaderY.Text.Trim());
                if (chkLogoHeaderFirstPage.Checked)
                {
                    Tools.SaveEnvValue("LOGOHDRFP", "1");
                }
                else
                {
                    Tools.SaveEnvValue("LOGOHDRFP", "");
                }
                Tools.SaveEnvValue("LOGOFTR", txtLogoFooter.Text.Trim());
                Tools.SaveEnvValue("LOGOFTRX", txtLogoFooterX.Text.Trim());
                Tools.SaveEnvValue("LOGOFTRY", txtLogoFooterY.Text.Trim());
                if (chkLogoFooterFirstPage.Checked)
                {
                    Tools.SaveEnvValue("LOGOFTRFP", "1");
                }
                else
                {
                    Tools.SaveEnvValue("LOGOFTRFP", "");
                }

                Tools.SaveEnvValue("SIGNRMLEN", cboRoomLen.Text);

                if (chkSave.Checked)
                {
                    Tools.SaveEnvValue("SIGNSAVE", "1");
                }
                else
                {
                    Tools.SaveEnvValue("SIGNSAVE", "");
                }

                if (chkPrint.Checked)
                {
                    Tools.SaveEnvValue("SIGNPRINT", "1");
                    Tools.SaveEnvValue("SIGNDPRNT", cboPrinters.Text.ToUpper());
                }
                else
                {
                    Tools.SaveEnvValue("SIGNPRINT", "");
                    Tools.SaveEnvValue("SIGNDPRNT", "");
                }

                if (chkDebug.Checked)
                {
                    Tools.SaveEnvValue("SHOWCMDWDW", "1");
                }
                else
                {
                    Tools.SaveEnvValue("SHOWCMDWDW", "");
                }

                if (chkStandAlone.Checked)
                {
                    Tools.SaveEnvValue("STANDALONE", "1");
                    if (chkUseRoom.Checked)
                    {
                        Tools.SaveEnvValue("USEROOMREF", "1");
                    }
                    else
                    {
                        Tools.SaveEnvValue("USEROOMREF", "");
                    }
                }
                else
                {
                    Tools.SaveEnvValue("USEROOMREF", "");
                    Tools.SaveEnvValue("STANDALONE", "");
                }

                if (chkDesign.Checked)
                {
                    Tools.SaveEnvValue("DESIGNMODE", "1");
                }
                else
                {
                    Tools.SaveEnvValue("DESIGNMODE", "");
                }

                if (chkIOS.Checked)
                {
                    Tools.SaveEnvValue("IOSTABLET", "1");
                }
                else
                {
                    Tools.SaveEnvValue("IOSTABLET", "");
                }

                for (int i = 1; i <= NO_OF_CONTROLS; i++)
                {
                    cName = "fldType" + i.ToString();
                    control = panel1.Controls[cName];
                    type = (ComboBox)control;

                    cName = "fldHead" + i.ToString();
                    control = panel1.Controls[cName];
                    head = (TextBox)control;

                    cName = "fldName" + i.ToString();
                    control = panel1.Controls[cName];
                    field = (TextBox)control;

                    cName = "fldPage" + i.ToString();
                    control = panel1.Controls[cName];
                    fldPage = (TextBox)control;

                    cName = "fldX" + i.ToString();
                    control = panel1.Controls[cName];
                    fldX = (TextBox)control;

                    cName = "fldY" + i.ToString();
                    control = panel1.Controls[cName];
                    fldY = (TextBox)control;

                    cName = "fldW" + i.ToString();
                    control = panel1.Controls[cName];
                    fldWidth = (TextBox)control;

                    cName = "fldH" + i.ToString();
                    control = panel1.Controls[cName];
                    fldHeight = (TextBox)control;

                    cName = "fldShow" + i.ToString();
                    control = panel1.Controls[cName];
                    fldShow = (CheckBox)control;

                    cName = "fldLast" + i.ToString();
                    control = panel1.Controls[cName];
                    fldLast = (CheckBox)control;

                    cName = "fldDisable" + i.ToString();
                    control = panel1.Controls[cName];
                    fldDisable = (CheckBox)control;

                    if (type.Text != "None")
                    {
                        Tools.SaveEnvValue("FLD" + i.ToString() + "TYPE", type.Text);
                        Tools.SaveEnvValue("FLD" + i.ToString() + "X", fldX.Text);
                        Tools.SaveEnvValue("FLD" + i.ToString() + "Y", fldY.Text);
                        Tools.SaveEnvValue("FLD" + i.ToString() + "Width", fldWidth.Text);
                        Tools.SaveEnvValue("FLD" + i.ToString() + "Hgt", fldHeight.Text);
                        Tools.SaveEnvValue("FLD" + i.ToString() + "Head", head.Text);
                        Tools.SaveEnvValue("FLD" + i.ToString() + "Name", field.Text );
                        Tools.SaveEnvValue("FLD" + i.ToString() + "Page", fldPage.Text);
                        if (fldShow.Checked)
                        {
                            Tools.SaveEnvValue("FLD" + i.ToString() + "Show", "1");
                        }
                        else
                        {
                            Tools.SaveEnvValue("FLD" + i.ToString() + "Show", "");
                        }

                        if (fldLast.Checked)
                        {
                            Tools.SaveEnvValue("FLD" + i.ToString() + "Last", "1");
                        }
                        else
                        {
                            Tools.SaveEnvValue("FLD" + i.ToString() + "Last", "");
                        }

                        if (fldDisable.Checked)
                        {
                            Tools.SaveEnvValue("FLD" + i.ToString() + "Dis", "1");
                        }
                        else
                        {
                            Tools.SaveEnvValue("FLD" + i.ToString() + "Dis", "");
                        }

                    }
                    else
                    {
                        Tools.SaveEnvValue("FLD" + i.ToString() + "TYPE", "None");
                        Tools.SaveEnvValue("FLD" + i.ToString() + "X", "");
                        Tools.SaveEnvValue("FLD" + i.ToString() + "Y", "");
                        Tools.SaveEnvValue("FLD" + i.ToString() + "Width", "");
                        Tools.SaveEnvValue("FLD" + i.ToString() + "Hgt", "");
                        Tools.SaveEnvValue("FLD" + i.ToString() + "Head", "");
                        Tools.SaveEnvValue("FLD" + i.ToString() + "Name", "");
                        Tools.SaveEnvValue("FLD" + i.ToString() + "Show", "");
                        Tools.SaveEnvValue("FLD" + i.ToString() + "Last", "");
                        Tools.SaveEnvValue("FLD" + i.ToString() + "Dis", "");
                    }

                }

                if (chkEnableMail.Checked)
                {
                    Tools.SaveEnvValue("MAILENABLE", "1");
                }
                else
                {
                    Tools.SaveEnvValue("MAILENABLE", "");
                }

                if (chkSSL.Checked)
                {
                    Tools.SaveEnvValue("MAILSSL", "1");
                }
                else
                {
                    Tools.SaveEnvValue("MAILSSL", "");
                }

                Tools.SaveEnvValue("MAILNAME", mailName.Text);
                Tools.SaveEnvValue("MAILADD", mailAddress.Text);
                Tools.SaveEnvValue("MAILSERVER", mailServer.Text);
                Tools.SaveEnvValue("MAILPORT", mailPort.Text);
                Tools.SaveEnvValue("MAILID", mailID.Text);


                try
                {
                    if (mailPassword.Text.Trim().Length > 0)
                    {
                        Tools.SaveEnvValue("MAILPW", Tools.PBEnCrypt(mailPassword.Text));
                    }
                    else
                    {
                        Tools.SaveEnvValue("MAILPW", "");
                    }
                    
                }
                catch
                {

                }

                if (txtRecipients.Text.Trim().Length == 0)
                {
                    Tools.SaveEnvValue("RECIPIENTS", "");
                }
                else
                {
                    Tools.SaveEnvValue("RECIPIENTS", Tools.PBEnCrypt(txtRecipients.Text.Trim()));
                }
                
                Tools.SaveEnvValue("AUTO_SUBJ", txtSubject.Text.Trim());
                Tools.SaveEnvValue("HTMLBODY", txtBody.Text.Trim());
                Tools.SaveEnvValue("SIGNATURE", txtSignature.Text.Trim());
                Tools.SaveEnvValue("SIGNLINK", txtSignLink.Text.Trim());

                Tools.SaveEnvValue("SPECIAL", cboSpecial.Text.Trim());

            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            if (ret)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show(this, "Please make sure the directories are correct!", "Directory Paths", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        internal static bool GetSettings()
        {
            try
            {

                AppPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

                APIURL = Tools.GetEnvValue("API_IP");
                APIPORT = Tools.GetEnvValue("API_PORT");
                DeviceID = Tools.GetEnvValue("DEVICEID");
                JavaPath = Tools.GetEnvValue("JAVA_PATH");
                ListenFolder = Tools.GetEnvValue("SIGNLISTEN");
                TargetFolder = Tools.GetEnvValue("SIGNTARGET");
                ShowCmdWdw = Tools.GetEnvValue("SHOWCMDWDW").Trim();
                StandAlone = (Tools.GetEnvValue("STANDALONE").Trim() == "1") ? true : false;
                RoomRef = (Tools.GetEnvValue("USEROOMREF").Trim() == "1") ? true : false;


                DesignMode1 = (Tools.GetEnvValue("DESIGNMODE").Trim() == "1") ? true : false;
                Int32.TryParse(Tools.GetEnvValue("SIGNFONTSZ"), out FontSize);
                Font1 = Tools.GetEnvValue("SIGNFONT");
                MailEnable = (Tools.GetEnvValue("MAILENABLE").Trim() == "1") ? true : false;
                RoomLength = Int32.TryParse(Tools.GetEnvValue("SIGNRMLEN"), out RoomLength) ? RoomLength : 4;

                HeaderLogo = Tools.GetEnvValue("LOGOHDR").Trim();
                Int32.TryParse(Tools.GetEnvValue("LOGOHDRX").Trim(), out HeaderLocationX);
                Int32.TryParse(Tools.GetEnvValue("LOGOHDRY").Trim(), out HeaderLocationY);
                HeaderFirstPageOnly = (Tools.GetEnvValue("LOGOHDRFP").Trim() == "1") ? true : false;

                FooterLogo = Tools.GetEnvValue("LOGOFTR").Trim();
                Int32.TryParse(Tools.GetEnvValue("LOGOFTRX").Trim(), out FooterLocationX);
                Int32.TryParse(Tools.GetEnvValue("LOGOFTRY").Trim(), out FooterLocationY);
                FooterFirstPageOnly = (Tools.GetEnvValue("LOGOFTRFP").Trim() == "1") ? true : false;

                OptSign = (Tools.GetEnvValue("OPTSIGN").Trim() == "1") ? true : false;
                if (OptSign)
                {
                    Int32.TryParse(Tools.GetEnvValue("OPTSIGNX").Trim(), out OptSignX);
                    Int32.TryParse(Tools.GetEnvValue("OPTSIGNY").Trim(), out OptSignY);
                    if (OptSignX == 0 || OptSignY == 0)
                    {
                        OptSign = false;
                    }
                }
                Special = Tools.GetEnvValue("SPECIAL").Trim();

                if (ListenFolder.Length == 0 || TargetFolder.Length == 0)
                {
                    Settings form = new Settings();

                    form.ShowDialog();
                }
                
                return true;
            }
            catch(Exception ex)
            {
                throw ex;
            }

        }

        private void Button2_Click(object sender, EventArgs e)
        {

        }

        private void Button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            this.Text = this.Text + " ("+Environment.UserName+")";
            Control control;
            string cName;

            ComboBox type;
            CheckBox show;
            CheckBox last;
            CheckBox disable;
            TextBox head;
            TextBox field;
            TextBox fldX;
            TextBox fldY;
            TextBox fldWidth;
            TextBox fldHeight;
            TextBox fldPage;


            ComboBox cType;
            TextBox cHead;
            TextBox cField;
            TextBox cX;
            TextBox cY;
            TextBox cW;
            TextBox cH;
            CheckBox cShow;
            CheckBox cLastPage;
            TextBox cPage;
            CheckBox cDisable;

            for (int cIdx = 1; cIdx <= NO_OF_CONTROLS; cIdx++)
            {
                ComboBox comboBox = new ComboBox
                {
                    Name = "fldType" + cIdx.ToString(),
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Location = new System.Drawing.Point(11, ((cIdx - 1) * 82) + 10)
                };
                cType = comboBox;
                cType.Items.AddRange(new string[] { "None", "Field", "Signature!", "Signature", "Other", "MultiLine", "CheckBox", "Timestamp", "RoomNo", "GuestName", "CheckInDate", "ReferenceNo", "OPSignature" });
                panel1.Controls.Add(cType);

                cShow = new CheckBox
                {
                    Name = "fldShow" + cIdx.ToString(),
                    Location = new System.Drawing.Point(145, ((cIdx - 1) * 82) + 10),
                    Width = 80,
                    Text = "User Edit"
                };
                panel1.Controls.Add(cShow);

                cLastPage = new CheckBox
                {
                    Name = "fldLast" + cIdx.ToString(),
                    Location = new System.Drawing.Point(230, ((cIdx - 1) * 82) + 10),
                    Width = 90,
                    Text = "Last Page"
                };
                panel1.Controls.Add(cLastPage);

                cDisable = new CheckBox
                {
                    Name = "fldDisable" + cIdx.ToString(),
                    Location = new System.Drawing.Point(330, ((cIdx - 1) * 82) + 10),
                    Width = 80,
                    Text = "Disable"
                };
                panel1.Controls.Add(cDisable);

                cPage = new TextBox
                {
                    Name = "fldPage" + cIdx.ToString(),
                    Location = new System.Drawing.Point(462, ((cIdx - 1) * 82) + 10),
                    Width = 20
                };
                panel1.Controls.Add(cPage);

                cHead = new TextBox
                {
                    Name = "fldHead" + cIdx.ToString(),
                    Location = new System.Drawing.Point(79, ((cIdx - 1) * 82) + 40),
                    Width = 189
                };
                panel1.Controls.Add(cHead);

                cField = new TextBox
                {
                    Name = "fldName" + cIdx.ToString(),
                    Location = new System.Drawing.Point(79, ((cIdx - 1) * 82) + 66),
                    Width = 189
                };
                panel1.Controls.Add(cField);

                cX = new TextBox
                {
                    Name = "fldX" + cIdx.ToString(),
                    Location = new System.Drawing.Point(331, ((cIdx - 1) * 82) + 38),
                    Width = 48
                };
                panel1.Controls.Add(cX);

                cY = new TextBox
                {
                    Name = "fldY" + cIdx.ToString(),
                    Location = new System.Drawing.Point(331, ((cIdx - 1) * 82) + 66),
                    Width = 48
                };
                panel1.Controls.Add(cY);

                cW = new TextBox
                {
                    Name = "fldW" + cIdx.ToString(),
                    Location = new System.Drawing.Point(435, ((cIdx - 1) * 82) + 39),
                    Width = 48
                };
                panel1.Controls.Add(cW);

                cH = new TextBox
                {
                    Name = "fldH" + cIdx.ToString(),
                    Location = new System.Drawing.Point(435, ((cIdx - 1) * 82) + 66),
                    Width = 48
                };
                panel1.Controls.Add(cH);
            }

            try
            {
                if (Tools.GetEnvValue("REMPRINT").Trim() == "1")
                {
                    chkDisablePrint.Checked = true;
                }

                if (Tools.GetEnvValue("OPTSIGN").Trim() == "1")
                {
                    chkOpSign.Checked = true;
                    txtOptSignX.Text = Tools.GetEnvValue("OPTSIGNX").Trim();
                    txtOptSignY.Text = Tools.GetEnvValue("OPTSIGNY").Trim();
                }
                else
                {
                    chkOpSign.Checked = false;
                    txtOptSignX.Text = "";
                    txtOptSignY.Text = "";
                }

                if (Tools.GetEnvValue("MAILENABLE").Trim() == "1")
                {
                    chkEnableMail.Checked = true;
                }
                else
                {
                    chkEnableMail.Checked = false;
                }

                if (Tools.GetEnvValue("STANDALONE").Trim() == "1")
                {
                    chkStandAlone.Checked = true;
                    chkUseRoom.Visible = true;
                }
                else
                {
                    chkStandAlone.Checked = false;
                }

                if (Tools.GetEnvValue("DESIGNMODE").Trim() == "1")
                {
                    chkDesign.Checked = true;
                }
                else
                {
                    chkDesign.Checked = false;
                }

                if (Tools.GetEnvValue("MAILSSL").Trim() == "1")
                {
                    chkSSL.Checked = true;
                }
                else
                {
                    chkSSL.Checked = false;
                }

                if (Tools.GetEnvValue("IOSTABLET").Trim() == "1")
                {
                    chkIOS.Checked = true;
                    chkUSB.Text = "Group Mode";

                    label93.Hide();
                    txtCommDelay.Hide();
                }
                else
                {
                    chkIOS.Checked = false;
                    chkUSB.Text = "USB Mode";
                }

                txtFont.Text = Tools.GetEnvValue("SIGNFONT").Trim();
                txtFontSize.Text = Tools.GetEnvValue("SIGNFONTSZ").Trim();
                mailName.Text = Tools.GetEnvValue("MAILNAME").Trim();
                mailAddress.Text = Tools.GetEnvValue("MAILADD").Trim();
                mailServer.Text = Tools.GetEnvValue("MAILSERVER").Trim();
                mailPort.Text = Tools.GetEnvValue("MAILPORT").Trim();
                mailID.Text = Tools.GetEnvValue("MAILID").Trim();
                mailPassword.Text = Tools.PBDeCrypt(Tools.GetEnvValue("MAILPW").Trim());

                txtRecipients.Text = Tools.PBDeCrypt(Tools.GetEnvValue("RECIPIENTS").Trim());
                txtSubject.Text = Tools.GetEnvValue("AUTO_SUBJ").Trim();
                txtBody.Text = Tools.GetEnvValue("HTMLBODY").Trim();
                txtSignature.Text = Tools.GetEnvValue("SIGNATURE").Trim();
                txtSignLink.Text = Tools.GetEnvValue("SIGNLINK").Trim();

            }
            catch
            {

            }


            for (int i = 1; i <= NO_OF_CONTROLS; i++)
            {
                cName = "fldType" + i.ToString();
                control = panel1.Controls[cName];
                type = (ComboBox)control;

                cName = "fldShow" + i.ToString();
                control = panel1.Controls[cName];
                show = (CheckBox)control;

                cName = "fldLast" + i.ToString();
                control = panel1.Controls[cName];
                last = (CheckBox)control;

                cName = "fldDisable" + i.ToString();
                control = panel1.Controls[cName];
                disable = (CheckBox)control;

                cName = "fldHead" + i.ToString();
                control = panel1.Controls[cName];
                head = (TextBox)control;

                cName = "fldName" + i.ToString();
                control = panel1.Controls[cName];
                field = (TextBox)control;

                cName = "fldPage" + i.ToString();
                control = panel1.Controls[cName];
                fldPage = (TextBox)control;

                cName = "fldX" + i.ToString();
                control = panel1.Controls[cName];
                fldX = (TextBox)control;

                cName = "fldY" + i.ToString();
                control = panel1.Controls[cName];
                fldY = (TextBox)control;

                cName = "fldW" + i.ToString();
                control = panel1.Controls[cName];
                fldWidth = (TextBox)control;

                cName = "fldH" + i.ToString();
                control = panel1.Controls[cName];
                fldHeight = (TextBox)control;


                type.SelectedIndex = 0;
                show.Checked = false;
                last.Checked = false;
                disable.Checked = false;
                head.Text = "";
                field.Text = "";
                fldX.Text = "";
                fldY.Text = "";
                fldWidth.Text = "";
                fldHeight.Text = "";
                fldPage.Text = "";




                if (Tools.GetEnvValue("FLD" + i.ToString() + "TYPE").Trim().Length > 0 && Tools.GetEnvValue("FLD" + i.ToString() + "TYPE").Trim() != "None")
                {
                    type.Text = Tools.GetEnvValue("FLD" + i.ToString() + "TYPE").Trim();
                    head.Text = Tools.GetEnvValue("FLD" + i.ToString() + "Head").Trim();
                    field.Text = Tools.GetEnvValue("FLD" + i.ToString() + "Name").Trim();
                    fldPage.Text = Tools.GetEnvValue("FLD" + i.ToString() + "Page").Trim();
                    fldX.Text = Tools.GetEnvValue("FLD" + i.ToString() + "X").Trim();
                    fldY.Text = Tools.GetEnvValue("FLD" + i.ToString() + "Y").Trim();
                    fldWidth.Text = Tools.GetEnvValue("FLD" + i.ToString() + "Width").Trim();
                    fldHeight.Text = Tools.GetEnvValue("FLD" + i.ToString() + "Hgt").Trim();

                    if (Tools.GetEnvValue("FLD" + i.ToString() + "Show").Trim() == "1")
                    {
                        show.Checked = true;
                    }

                    if (Tools.GetEnvValue("FLD" + i.ToString() + "Last").Trim() == "1")
                    {
                        last.Checked = true;
                    }

                    if (Tools.GetEnvValue("FLD" + i.ToString() + "Dis").Trim() == "1")
                    {
                        disable.Checked = true;
                    }
                }
            }

            cboRoomLen.Text = Settings.RoomLength.ToString();

            txtListen.Text = Tools.GetEnvValue("SIGNLISTEN");
            txtTarget.Text = Tools.GetEnvValue("SIGNTARGET");

            if (Tools.GetEnvValue("SHOWCMDWDW").Trim() == "1")
            {
                chkDebug.Checked = true;
            }

            int idx = -1;
            int selected = -1;

            idx = idx + 1;
            cboPrinters.Items.Add("");
            foreach (string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
            {
                cboPrinters.Items.Add(printer);

                idx = idx + 1;
                if (Tools.GetEnvValue("SIGNDPRNT").Trim().ToUpper() == printer.ToUpper())
                {
                    selected = idx;
                }
            }
            if (selected > -1)
            {
                cboPrinters.SelectedIndex = selected;
            }

            if (Tools.GetEnvValue("SIGNPRINT").Trim() == "1")
            {
                chkPrint.Checked = true;
            }
            else
            {
                cboPrinters.Enabled = false;
            }

            if (Tools.GetEnvValue("SIGNSAVE").Trim() == "1")
            {
                chkSave.Checked = true;
            }

            txtAPIIP.Text = Tools.GetEnvValue("API_IP").Trim();
            txtAPIPort.Text = Tools.GetEnvValue("API_PORT").Trim();
            txtDeviceID.Text = Tools.GetEnvValue("DEVICEID").Trim();

            chkUSB.Checked = Tools.GetEnvValue("USBCONNECT").Trim() == "1";
            txtCommDelay.Text = Tools.GetEnvValue("COMMDELAY").Trim();

            txtLogoHeader.Text = Tools.GetEnvValue("LOGOHDR").Trim();
            txtLogoHeaderX.Text = Tools.GetEnvValue("LOGOHDRX").Trim();
            txtLogoHeaderY.Text = Tools.GetEnvValue("LOGOHDRY").Trim();
            if (Tools.GetEnvValue("LOGOHDRFP").Trim() == "1")
            {
                chkLogoHeaderFirstPage.Checked = true;
            }
            else
            {
                chkLogoHeaderFirstPage.Checked = false;
            }

            txtLogoFooter.Text = Tools.GetEnvValue("LOGOFTR").Trim();
            txtLogoFooterX.Text = Tools.GetEnvValue("LOGOFTRX").Trim();
            txtLogoFooterY.Text = Tools.GetEnvValue("LOGOFTRY").Trim();
            if (Tools.GetEnvValue("LOGOFTRFP").Trim() == "1")
            {
                chkLogoFooterFirstPage.Checked = true;
            }
            else
            {
                chkLogoFooterFirstPage.Checked = false;
            }
            cboSpecial.Text = Tools.GetEnvValue("SPECIAL").Trim();

        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(txtListen.Text.Trim()))
            {
                folderBrowserDialog1.SelectedPath = txtListen.Text.Trim();
            }
            if (folderBrowserDialog1.ShowDialog(this) == DialogResult.OK)
            {
                txtListen.Text = folderBrowserDialog1.SelectedPath;
            } 
        }

        private void Button2_Click_1(object sender, EventArgs e)
        {
            if (Directory.Exists(txtTarget.Text.Trim()))
            {
                folderBrowserDialog1.SelectedPath = txtTarget.Text.Trim();
            }
            if (folderBrowserDialog1.ShowDialog(this) == DialogResult.OK)
            {
                txtTarget.Text = folderBrowserDialog1.SelectedPath;
            } 
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void ComboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }

        private void ChkPrint_CheckedChanged(object sender, EventArgs e)
        {
            if (chkPrint.Checked)
            {
                cboPrinters.Enabled = true;
            }
            else
            {
                cboPrinters.Enabled = false;
                cboPrinters.SelectedIndex = 0;
            }
        }
        private void CboField1_SelectedIndexChanged(object sender, EventArgs e)
        {


        }

        private void ChkEnableMail_CheckedChanged(object sender, EventArgs e)
        {
            if (!chkEnableMail.Checked)
            {
                chkSSL.Checked = false;
                mailName.Text = "";
                mailAddress.Text = "";
                mailServer.Text = "";
                mailPort.Text = "";
                mailID.Text = "";
                mailPassword.Text = "";
                txtRecipients.Text = "";
                txtSubject.Text = "";
                txtBody.Text = "";
            }

            chkSSL.Enabled = chkEnableMail.Checked;
            mailName.Enabled = chkEnableMail.Checked;
            mailAddress.Enabled = chkEnableMail.Checked;
            mailServer.Enabled = chkEnableMail.Checked;
            mailPort.Enabled = chkEnableMail.Checked;
            mailID.Enabled = chkEnableMail.Checked;
            mailPassword.Enabled = chkEnableMail.Checked;
            txtRecipients.Enabled = chkEnableMail.Checked;
            txtSubject.Enabled = chkEnableMail.Checked;
            txtBody.Enabled = chkEnableMail.Checked;
        }

        private void Button2_Click_2(object sender, EventArgs e)
        {

        }

        private void FldType3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void FldType2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void GroupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void BtnListen_Click(object sender, EventArgs e)
        {
            txtListen.Text = SelectFolder();
        }

        private string SelectFolder()
        {
            string ret = "";
            using (var folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {

                    ret = folderDialog.SelectedPath; // -- your result
                }
            }
            return ret;
        }

        private void BtnTarget_Click(object sender, EventArgs e)
        {
            txtTarget.Text = SelectFolder();
        }

        private void Button1_Click_1(object sender, EventArgs e)
        {
            txtLogoHeader.Text = SelectFile();
        }

        private string SelectFile()
        {
            string ret = "";

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK) // Test result.
            {
                ret = openFileDialog1.FileName;
            }

            return ret;
        }

        private void Button2_Click_3(object sender, EventArgs e)
        {
            txtLogoFooter.Text = SelectFile();
        }

        private void TabPage1_Click(object sender, EventArgs e)
        {

        }

        private void chkUSB_CheckedChanged(object sender, EventArgs e)
        {
            if (!chkIOS.Checked)
            {
                SetUSBConnection();
            }
            
        }

        private void SetUSBConnection()
        {
            txtAPIIP.Text = "";
            txtAPIPort.Text = "";
            txtDeviceID.Text = "";
            txtCommDelay.Text = Settings.COMM_DELAY_DEFAULT.ToString();

            txtAPIIP.Enabled = !chkUSB.Checked;
            txtAPIPort.Enabled = !chkUSB.Checked;
            txtDeviceID.Enabled = !chkUSB.Checked;

            txtCommDelay.Enabled = chkUSB.Checked;
        }

        private void chkIOS_CheckedChanged(object sender, EventArgs e)
        {
            if (chkIOS.Checked)
            {
                chkUSB.Text = "Group Mode";
                label93.Hide();
                txtCommDelay.Hide();
                chkUSB.Checked = false;
            }
            else
            {
                chkUSB.Text = "USB Mode";
                label93.Show();
                txtCommDelay.Show();
            }
            chkUSB.Checked = false;
            txtAPIIP.Enabled = true;
            txtAPIPort.Enabled = true;
            txtDeviceID.Enabled = true;
            txtCommDelay.Enabled = false;
        }

        private void chkStandAlone_CheckedChanged(object sender, EventArgs e)
        {
            if (chkStandAlone.Checked)
            {
                chkUseRoom.Visible = true;
            }
            else
            {
                chkUseRoom.Checked = false;
                chkUseRoom.Visible = false;
            }
        }
    }
}
