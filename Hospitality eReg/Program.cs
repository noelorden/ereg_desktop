using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Hospitality_eReg
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]

        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                if (args.Length > 0)
                {
                    if (args.Length > 2)
                    {
                        SelectDocument docForm = new SelectDocument(args);
                        DialogResult dlgResult = docForm.ShowDialog();
                        if (dlgResult == DialogResult.OK)
                        {
                            Settings.SettingConfigFile = docForm.FileName ;
                        }
                        else
                        {
                            throw new Exception("No document selected.");
                        }
                    }
                    else
                    {
                        Settings.SettingConfigFile = args[1];
                    }

                    

                    if (Settings.GetSettings())
                    {

                        
                        Settings.PageCount = PDFIText.GetPDFPageCount(args[0]);

                        string folder = Path.GetDirectoryName(args[0]);
                        string file = Path.GetFileName(args[0]);
                        string tempFile = folder + @"\_" + file;

                        if (Settings.HeaderLogo.Length > 0 && File.Exists(Settings.HeaderLogo))
                        {

                            if (PDFIText.InsertJPEG(
                                args[0],
                                tempFile,
                                Settings.HeaderLogo,
                                Settings.HeaderLocationX,
                                Settings.HeaderLocationY,
                                Settings.HeaderFirstPageOnly))
                            {
                                
                                
                                while (File.Exists(args[0]))
                                {
                                    try
                                    {
                                        File.Delete(args[0]);
                                    }
                                    catch
                                    {

                                    }
                                }
                                File.Move(tempFile, args[0]);

                                
                                
                            }
                        }

                        if (Settings.HeaderLogo.Length > 0 && File.Exists(Settings.HeaderLogo))
                        {

                            if (PDFIText.InsertJPEG(
                                args[0],
                                tempFile,
                                Settings.HeaderLogo,
                                Settings.HeaderLocationX,
                                Settings.HeaderLocationY,
                                Settings.HeaderFirstPageOnly))
                            {
                                while (File.Exists(args[0]))
                                {
                                    try
                                    {
                                        File.Delete(args[0]);
                                    }
                                    catch
                                    {

                                    }
                                }
                                File.Move(tempFile, args[0]);



                            }
                        }

                        if (Settings.FooterLogo .Length > 0 && File.Exists(Settings.FooterLogo))
                        {

                            if (PDFIText.InsertJPEG(
                                args[0],
                                tempFile,
                                Settings.FooterLogo,
                                Settings.FooterLocationX,
                                Settings.FooterLocationY,
                                Settings.FooterFirstPageOnly))
                            {
                                while (File.Exists(args[0]))
                                {
                                    try
                                    {
                                        File.Delete(args[0]);
                                    }
                                    catch
                                    {

                                    }
                                }
                                File.Move(tempFile, args[0]);


                            }
                        }

                        try
                        {
                            string windowsUserID = Environment.UserName.Trim();

                            if (Settings.OptSign)
                            {
                                if (File.Exists(tempFile))
                                {
                                    File.Delete(tempFile);
                                    while (File.Exists(tempFile))
                                    {

                                    }
                                }

                                string signPath = Settings.DataPath;
                                string signFile = Path.Combine(signPath, "..", "..","OPSigns", windowsUserID + ".jpg");
                                                                
                                if (File.Exists(signFile))
                                {

                                    PDFIText.InsertJPEG(
                                    args[0],
                                    tempFile,
                                    signFile,
                                    Settings.OptSignX,
                                    Settings.OptSignY,
                                    true);

                                    File.Delete(args[0]);
                                    while (File.Exists(args[0]))
                                    {

                                    }
                                    File.Move(tempFile, args[0]);
                                }

                            }
                        }
                        catch
                        {

                        }
                        Application.Run(new Form1(args[0]));
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "RedBerry Sign", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }

    }
}
