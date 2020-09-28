using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Hospitality_eReg
{
    public static class Tools
    {
        [DllImport("VMSPB.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.BStr)]
        public static extern string PBEnCrypt([MarshalAs(UnmanagedType.BStr)]string keystr);

        [DllImport("VMSPB.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.BStr)]
        public static extern String PBDeCrypt([MarshalAs(UnmanagedType.BStr)]string keystr);

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetDefaultPrinter(string Name);
       

        public static string GetEnvValue(string envStr)
        {
            string ret = "";

            try
            {
                string envFile;
                string content;

                string param;

                envFile = Settings.AppPath + @"\" + Settings.SettingConfigFile;
  
                if (File.Exists(envFile))
                {
                    content = System.IO.File.ReadAllText(envFile);

                    string[] settings = content.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                    var v = settings.FirstOrDefault(s => s.Substring(0,10).Trim().ToLower().Equals(envStr.Trim().ToLower()));
                    if (v!=null)
                    {
                        ret = v.Substring(11).Trim();
                    }
                }
                return ret;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void SaveEnvValue(string envStr, string envValue)
        {
            try
            {
                string envFile;
                string content;

                string param;

                envFile = Settings.AppPath + @"\" + Settings.SettingConfigFile;
                if (!File.Exists(envFile))
                {
                    System.IO.File.WriteAllText(envFile, "");
                }

                content = System.IO.File.ReadAllText(envFile);
                string[] settings = content.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                bool found = false;

                envStr = envStr.PadRight(10);
                envStr = envStr.Substring(0, 10);
                envValue = envValue.Trim();

                for (int x = 0; x < settings.Length; x++)
                {
                    param = settings[x].Substring(0, 10);
                    if (param.Trim().ToLower() == envStr.Trim().ToLower())
                    {
                        settings[x] = envStr + "," + envValue;
                        found = true;
                    }
                }

                if (!found)
                {
                    int newLength = settings.Length + 1;
                    string[] newsttings = new string[newLength];

                    Array.Copy(settings, newsttings, newLength - 1);

                    newsttings[newLength - 1] = envStr + "," + envValue;
                    content = string.Join(System.Environment.NewLine, newsttings);
                }
                else
                {
                    content = string.Join(System.Environment.NewLine, settings);
                }

                System.IO.File.WriteAllText(envFile, content);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public static MailAccount GetMailAccountDetails()
        {
            try
            {
                MailAccount mAccount = new MailAccount();
                mAccount.ID = Tools.GetEnvValue("MAILID").Trim();
                mAccount.Password = Tools.PBDeCrypt(Tools.GetEnvValue("MAILPW").Trim()).Trim();
                mAccount.SenderEMail = Tools.GetEnvValue("MAILADD").Trim();
                mAccount.SenderName = Tools.GetEnvValue("MAILNAME").Trim();

                return mAccount;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static MailServer GetMailServerDetails()
        {
            try
            {
                MailServer mServer = new MailServer();
                mServer.Host = Tools.GetEnvValue("MAILSERVER").Trim();
                Int32.TryParse(Tools.GetEnvValue("MAILPORT"), out mServer.Port);
                mServer.EnableSSL = (Tools.GetEnvValue("MAILSSL").Trim() == "1") ? true : false;
                mServer.TimeOut = 20000;

                return mServer;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }


}
