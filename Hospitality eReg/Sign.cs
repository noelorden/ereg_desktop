using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace Hospitality_eReg
{
    public class Sign
    {
        const int MAX_PATH = 255;

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern int GetShortPathName(
            [MarshalAs(UnmanagedType.LPTStr)]
         string path,
            [MarshalAs(UnmanagedType.LPTStr)]
         StringBuilder shortPath,
            int shortPathLength
            );

        private static string GetShortPath(string path)
        {
            var shortPath = new StringBuilder(MAX_PATH);
            GetShortPathName(path, shortPath, MAX_PATH);
            return shortPath.ToString();
        }

        internal static void ClearRecords()
        {
            try
            {
                var parms = new Dictionary<string, string>();
                var url = "http://" + Settings.APIURL + ":" + Settings.APIPORT + "/api/cancel_transfer";
                parms.Add("device_id", Settings.DeviceID);

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    client.DefaultRequestHeaders.Add("Authorization", "8f16771f9f8851b26f4d460fa17de93e2711c7e51337cb8a608a0f81e1c1b6ae");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");
                    HttpResponseMessage response = client.PostAsync(url, new FormUrlEncodedContent(parms)).Result;
                    var tokne = response.Content.ReadAsStringAsync().Result;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            
        }

        static void Connect(String server, int port, String message)
        {
            try
            {
                // Create a TcpClient.
                // Note, for this client to work you need to have a TcpServer 
                // connected to the same address as specified by the server, port
                // combination.
                TcpClient client = new TcpClient(server, port);

                // Translate the passed message into ASCII and store it as a Byte array.
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

                // Get a client stream for reading and writing.
                //  Stream stream = client.GetStream();

                NetworkStream stream = client.GetStream();

                // Send the message to the connected TcpServer. 
                stream.Write(data, 0, data.Length);

                Console.WriteLine("Sent: {0}", message);

                // Receive the TcpServer.response.

                // Buffer to store the response bytes.
                data = new Byte[256];

                // String to store the response ASCII representation.
                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
                //Int32 bytes = stream.Read(data, 0, data.Length);
                //responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                //Console.WriteLine("Received: {0}", responseData);

                // Close everything.
                stream.Close();
                client.Close();
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }

            Console.WriteLine("\n Press Enter to continue...");
            Console.Read();
        }

        internal static void SendRequestIOS(ThreadObject tObject)
        {
            string destFolder = "";
            string pdf = "";
            try
            {
                //ClearRecords();


                //D:/OpentecApps/RedBerry/ReaderV2/EReg/Target/181028
                destFolder = Path.Combine(Settings.TargetFolder, tObject.Folder);
                //destFolder = destFolder.Replace(@"\", @"/");
                //"D:/OpentecApps/RedBerry/ReaderV2/EReg/Monitor/181028/58785_1.pdf"
                pdf = tObject.PDFFile;
                //pdf = pdf.Replace(@"\", @"/");

                string p;
                string f;
                string t;
                if (Tools.GetEnvValue("USBCONNECT").Trim() == "1")
                {
                    p = Path.GetDirectoryName(pdf);
                    f = string.Concat(tObject.Reference, ".pdf");
                    t = Path.Combine(p, f);

                    tObject.FileName = f;
                    try
                    {
                        File.Delete(t);
                    }
                    catch
                    {

                    }

                    File.Copy(pdf, t);

                    pdf = t;
                }

                var parms = new Dictionary<string, string>();
                var url = "http://" + Settings.APIURL + ":" + Settings.APIPORT + "/api/file_transfer";
                parms.Add("device_id", Settings.DeviceID);
                parms.Add("file_upload_path", destFolder);
                parms.Add("file_download_path", pdf);

                List<ERegPushData> lst = new List<ERegPushData>();

                ERegPushData eregData = new ERegPushData()
                {
                    FileName = tObject.FileName,
                    FilePath = pdf,
                    DeviceId = Tools.GetEnvValue("DEVICEID"),
                    Includepdf = 1,
                    FileType = (Tools.GetEnvValue("USBCONNECT") =="1")? "Group" : "Normal",
                    DestinationFolder = destFolder + @"\"
                };

                lst.Add(eregData);

                var json = new JavaScriptSerializer().Serialize(lst.ToArray());

                string jsonStr = json + "#####";

                Int32.TryParse(Tools.GetEnvValue("API_PORT"), out int port);

                Connect(Tools.GetEnvValue("API_IP"), port, jsonStr);

                if (Tools.GetEnvValue("USBCONNECT") == "1") { Application.Exit(); }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

            }
        }

        internal static void SendRequest(ThreadObject tObject)
        {
            string destFolder="";
            string pdf="";
            try
            {
                //ClearRecords();

                
                //D:/OpentecApps/RedBerry/ReaderV2/EReg/Target/181028
                destFolder = Path.Combine(Settings.TargetFolder, tObject.Folder);
                //destFolder = destFolder.Replace(@"\", @"/");

                //"D:/OpentecApps/RedBerry/ReaderV2/EReg/Monitor/181028/58785_1.pdf"
                pdf = tObject.PDFFile;
                //pdf = pdf.Replace(@"\", @"/");

                var parms = new Dictionary<string, string>();
                var url = "http://" + Settings.APIURL + ":" + Settings.APIPORT + "/api/file_transfer";
                parms.Add("device_id", Settings.DeviceID);
                parms.Add("file_upload_path", destFolder);
                parms.Add("file_download_path", pdf);

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    client.DefaultRequestHeaders.Add("Authorization", "8f16771f9f8851b26f4d460fa17de93e2711c7e51337cb8a608a0f81e1c1b6ae");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");
                    HttpResponseMessage response = client.PostAsync(url, new FormUrlEncodedContent(parms)).Result;
                    var tokne = response.Content.ReadAsStringAsync().Result;
                }

            }
            catch(Exception ex)
            {

                MessageBox.Show(ex.Message);

            }
        }

        

        internal static void SendToAndroidx(ThreadObject tObject)
        {
            try
            {

                

                SendRequest(tObject);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

   
        }

        internal static void SendToAndroid(string p)
        {
            try
            {
                string fl;
                string destFolder;
                string java;
                string service;
                string adbPath;
                string commDelay;


                int cd;

                if (int.TryParse( Tools.GetEnvValue("COMMDELAY").Trim(),out cd))
                {
                    commDelay = cd.ToString();
                }
                else
                {
                    commDelay = Settings.COMM_DELAY_DEFAULT.ToString();
                }

                java = GetShortPath(Settings.JavaPath);
                fl = GetShortPath(p);
                destFolder = GetShortPath(Settings.TargetFolder + @"\" + Settings.WorkingFolder);
                destFolder = destFolder.TrimEnd().Substring(0, destFolder.Length );

                service = GetShortPath(Settings.AppPath + @"\DesktopService.jar");
                adbPath = GetShortPath(Settings.AppPath) + @"\adb.exe";

                string contents = "";
                string args = "";

                args = @"-jar """ + service + @""" """ + fl + @""" """ + destFolder + @""" """ + adbPath + @""" """ + commDelay + @"""";
                contents = java + " " + args;



                System.IO.File.WriteAllText(Settings.AppPath + @"\android.bat", contents);
                string batFile = GetShortPath(Settings.AppPath + @"\android.bat");

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
                process.StartInfo.FileName = java;
                process.StartInfo.Arguments = args;
                process.Start();

                //string output = process.StandardOutput.ReadToEnd();
                //process.WaitForExit();

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void DoWork(object obj)
        {
            try
            {
                
                string fullName = null;

                ThreadObject threadObject = (ThreadObject)obj;
                string pdfFile = threadObject.PDFFile;

                System.IO.DirectoryInfo di = null;

                Settings.WorkingFolder = String.Format("{0:yyMMdd}", DateTime.Now);

                if (Tools.GetEnvValue("IOSTABLET").Trim() == "1")
                {
                    di = new DirectoryInfo(Path.Combine(Settings.TargetFolder, threadObject.Folder));
                    SendRequestIOS(threadObject);
                }
                else
                {
                    if (Tools.GetEnvValue("USBCONNECT").Trim() == "1")
                    {
                        di = new DirectoryInfo(Settings.TargetFolder + @"\" + Settings.WorkingFolder);
                        SendToAndroid(pdfFile);
                    }
                    else
                    {
                        di = new DirectoryInfo(Path.Combine(Settings.TargetFolder, threadObject.Folder));

                        SendRequest(threadObject);
                    }
                }

                string wildCard = threadObject.FileName;

                do
                {
                    foreach (FileInfo file in di.EnumerateFiles(wildCard))
                    {
                        fullName = file.FullName;
                        break;
                    }

                    Thread.Sleep(300);
                }
                while (fullName == null);

                threadObject.CloseSign(fullName);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }

    internal class ERegPushData
    {
        string _filePath;

        public string FileData { get; set; }
        public string FileName { get; set; }
        public string FilePath
        {
            set
            {
                _filePath = value;
            }
        }
        public string File
        {
            get
            {
                try
                {
                    if (System.IO.File.Exists(_filePath))
                    {
                        Byte[] bytes = System.IO.File.ReadAllBytes(_filePath);
                        return Convert.ToBase64String(bytes);
                    }
                    else
                    {
                        return "";
                    }

                }
                catch
                {
                    return null;
                }
            }
        }
        public string DeviceId { get; set; }
        public int Includepdf { get; set; }
        public string FileType { get; set; }
        public string DestinationFolder { get; set; }
    }
}
