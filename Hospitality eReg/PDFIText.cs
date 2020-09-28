using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Hospitality_eReg
{
    public static class PDFIText
    {
        public static int GetPDFPageCount(string fileName)
        {

            try
            {
                PdfReader pdfReader = new PdfReader(fileName);
                int numberOfPages = pdfReader.NumberOfPages;
                return numberOfPages;
            }
            catch(Exception ex)
            {
                throw ex;
            }

        }

        public static void KillProcess(string fileName)
        {

            Process tool = new Process();
            tool.StartInfo.FileName = "handle.exe";
            tool.StartInfo.Arguments = fileName + " /accepteula";
            tool.StartInfo.UseShellExecute = false;
            tool.StartInfo.RedirectStandardOutput = true;
            tool.Start();
            tool.WaitForExit();
            string outputTool = tool.StandardOutput.ReadToEnd();

            string matchPattern = @"(?<=\s+pid:\s+)\b(\d+)\b(?=\s+)";
            foreach (Match match in Regex.Matches(outputTool, matchPattern))
            {
                Process.GetProcessById(int.Parse(match.Value)).Kill();
            }
        }

        public static bool InsertJPEG(string fileName, string targetFileName, string imgName, int locX, int locY, bool firstPageOnly = false)
        {
            try
            {

                using (Stream inputPdfStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (Stream inputImageStream = new FileStream(imgName, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (Stream outputPdfStream = new FileStream(targetFileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var reader = new PdfReader(inputPdfStream);
                    var stamper = new PdfStamper(reader, outputPdfStream);

                    var image = iTextSharp.text.Image.GetInstance(inputImageStream);

                    for (int x = 1; (x - 1) < reader.NumberOfPages; x++)
                    {
                        var pdfContentByte = stamper.GetOverContent(x);
                        image.SetAbsolutePosition(locX, locY);
                        //image.ScaleAbsolute(200, 150);
                        pdfContentByte.AddImage(image);

                        if (firstPageOnly && x == 1) { break; }
                    }

                    stamper.Close();
                }

                return true;
            }
            catch(Exception ex)
            {
                throw ex;
            }

        }

        public static bool InsertJPEG2(string fileName, string targetFileName, string imgName, int locX, int locY, int width, int height, bool firstPageOnly = false)
        {
            try
            {

                using (Stream inputPdfStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (Stream inputImageStream = new FileStream(imgName, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (Stream outputPdfStream = new FileStream(targetFileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var reader = new PdfReader(inputPdfStream);
                    var stamper = new PdfStamper(reader, outputPdfStream);

                    var image = iTextSharp.text.Image.GetInstance(inputImageStream);

                    for (int x = 1; (x - 1) < reader.NumberOfPages; x++)
                    {
                        var pdfContentByte = stamper.GetOverContent(x);
                        image.SetAbsolutePosition(locX, locY);
                        image.ScaleAbsolute(width/2, height/2);
                        pdfContentByte.AddImage(image);

                        if (firstPageOnly && x == 1) { break; }
                    }

                    stamper.Close();
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }

    public class PDFWrapper
    {

    }
}
