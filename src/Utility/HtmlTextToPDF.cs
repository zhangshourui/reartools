//using iTextSharp.text;
//using iTextSharp.text.pdf;
//using iTextSharp.tool.xml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public class HtmlTextToPDF
    {
        private static Log5 log = new Log5();
        /// <summary>
        /// HTML文本内容转换为PDF
        /// </summary>
        /// <param name="strHtml">HTML文本内容</param>
        /// <param name="savePath">PDF文件保存的路径</param>
        /// <returns></returns>
        public static bool HtmlTextConvertToPdf(string strHtml, string savePath)
        {
            bool flag = false;
            try
            {
                string htmlPath = HtmlTextConvertFile(strHtml);

                flag = HtmlConvertToPdf(htmlPath, savePath);
                File.Delete(htmlPath);
            }
            catch
            {
                flag = false;
            }
            return flag;
        }

        public static string HtmlTextConvertToPdfV2(string title, string strHtml)
        {
            //string savePath = @"D:\webapps\lcs-share\pdfshare\PDF\"+DateTime.Now.ToString("yyyy-MM-dd")+"\\" + title + ".pdf";
            string savePath = AppDomain.CurrentDomain.BaseDirectory + @"\pdfshare\PDF\" + DateTime.Now.ToString("yyyy-MM-dd") + "\\" + title + ".pdf";
            try
            {
                string htmlPath = HtmlTextConvertFile2(strHtml);
                HtmlConvertToPdf(htmlPath, savePath);
                File.Delete(htmlPath);

            }
            catch
            {
                savePath = string.Empty;
            }
            return savePath;
        }

        /// <summary>
        /// HTML转换为PDF
        /// </summary>
        /// <param name="htmlPath">可以是本地路径，也可以是网络地址</param>
        /// <param name="savePath">PDF文件保存的路径</param>
        /// <returns></returns>
        public static bool HtmlConvertToPdf(string htmlPath, string savePath)
        {
            bool flag = false;
            CheckFilePath(savePath);

            ///这个路径为程序集的目录，因为我把应用程序 wkhtmltopdf.exe 放在了程序集同一个目录下
            string exePath = AppDomain.CurrentDomain.BaseDirectory.ToString() + @"lib\wkhtmltopdf.exe";
            if (!File.Exists(exePath))
            {
                throw new Exception("No application wkhtmltopdf.exe was found.");
            }

            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo();
                processStartInfo.FileName = exePath;
                processStartInfo.WorkingDirectory = Path.GetDirectoryName(exePath);
                processStartInfo.UseShellExecute = false;
                processStartInfo.CreateNoWindow = true;
                processStartInfo.RedirectStandardInput = true;
                processStartInfo.RedirectStandardOutput = true;
                processStartInfo.RedirectStandardError = true;
                processStartInfo.Arguments = GetArguments(htmlPath, savePath);

                Process process = new Process();
                process.StartInfo = processStartInfo;
                process.Start();
                process.WaitForExit();

                ///用于查看是否返回错误信息
                //StreamReader srone = process.StandardError;
                //StreamReader srtwo = process.StandardOutput;
                //string ss1 = srone.ReadToEnd();
                //string ss2 = srtwo.ReadToEnd();
                //srone.Close();
                //srone.Dispose();
                //srtwo.Close();
                //srtwo.Dispose();

                process.Close();
                process.Dispose();

                flag = true;
            }
            catch
            {
                flag = false;
            }
            return flag;
        }

        /// <summary>
        /// 获取命令行参数
        /// </summary>
        /// <param name="htmlPath"></param>
        /// <param name="savePath"></param>
        /// <returns></returns>
        private static string GetArguments(string htmlPath, string savePath)
        {
            if (string.IsNullOrEmpty(htmlPath))
            {
                throw new Exception("HTML local path or network address can not be empty.");
            }

            if (string.IsNullOrEmpty(savePath))
            {
                throw new Exception("The path saved by the PDF document can not be empty.");
            }

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("--page-size A4 ");//当使用页面参数时，宽高参数无效  
            //stringBuilder.Append(" --page-height 100 ");        //页面高度100mm
            //stringBuilder.Append(" --page-width 100 ");         //页面宽度100mm
            stringBuilder.Append(" --header-spacing 3 ");//当使用页面参数时，宽高参数无效  
            stringBuilder.Append(" --header-center 中酒云图审批专用单据 ");  //设置居中显示页眉
            stringBuilder.Append(" --page-size A4 ");//当使用页面参数时，宽高参数无效  
            stringBuilder.Append(" --header-line ");         //页眉和内容之间显示一条直线
            stringBuilder.Append(" --footer-center \"Page [page] of [topage]\" ");    //设置居中显示页脚
            stringBuilder.Append(" --footer-line ");       //页脚和内容之间显示一条直线
            stringBuilder.Append(" --print-media-type ");       //生成的 PDF 文档的保存路径
            stringBuilder.Append(" " + htmlPath + " ");       //本地 HTML 的文件路径或网页 HTML 的URL地址
            stringBuilder.Append(" " + savePath + " ");       //生成的 PDF 文档的保存路径
            return stringBuilder.ToString();
        }

        /// <summary>
        /// 验证保存路径
        /// </summary>
        /// <param name="savePath"></param>
        private static void CheckFilePath(string savePath)
        {
            string ext = string.Empty;
            string path = string.Empty;
            string fileName = string.Empty;

            ext = Path.GetExtension(savePath);
            if (string.IsNullOrEmpty(ext) || ext.ToLower() != ".pdf")
            {
                throw new Exception("Extension error:This method is used to generate PDF files.");
            }

            fileName = Path.GetFileName(savePath);
            if (string.IsNullOrEmpty(fileName))
            {
                throw new Exception("File name is empty.");
            }

            try
            {
                path = savePath.Substring(0, savePath.IndexOf(fileName));
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch
            {
                throw new Exception("The file path does not exist.");
            }
        }

        /// <summary>
        /// HTML文本内容转HTML文件
        /// </summary>
        /// <param name="strHtml">HTML文本内容</param>
        /// <returns>HTML文件的路径</returns>
        public static string HtmlTextConvertFile(string strHtml)
        {
            if (string.IsNullOrEmpty(strHtml))
            {
                throw new Exception("HTML text content cannot be empty.");
            }

            try
            {
                //string path = AppDomain.CurrentDomain.BaseDirectory.ToString() + @"html\";
                string path = @"E:\WebApps\static.lcs.zhongjiu.cn\pdfshare\html\" + DateTime.Now.ToString("yyyy-MM-dd") + "\\";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string fileName = path + DateTime.Now.ToString("yyyyMMddHHmmssfff") + new Random().Next(1000, 10000) + ".html";
                FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8);
                streamWriter.Write(strHtml);
                streamWriter.Flush();

                streamWriter.Close();
                streamWriter.Dispose();
                fileStream.Close();
                fileStream.Dispose();
                return fileName;
            }
            catch
            {
                throw new Exception("HTML text content error.");
            }
        }

        public static string HtmlTextConvertFile2(string strHtml)
        {
            if (string.IsNullOrEmpty(strHtml))
            {
                throw new Exception("HTML text content cannot be empty.");
            }

            try
            {
                //string path = @"E:\WebApps\static.lcs.zhongjiu.cn\pdfshare\html\" + DateTime.Now.ToString("yyyy-MM-dd") + "\\";
                string path = AppDomain.CurrentDomain.BaseDirectory + @"\pdfshare\html\" + DateTime.Now.ToString("yyyy-MM-dd") + "\\";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string fileName = path + DateTime.Now.ToString("yyyyMMddHHmmssfff") + new Random().Next(1000, 10000) + ".html";
                FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8);
                streamWriter.Write(strHtml);
                streamWriter.Flush();

                streamWriter.Close();
                streamWriter.Dispose();
                fileStream.Close();
                fileStream.Dispose();
                return fileName;
            }
            catch
            {
                throw new Exception("HTML text content error.");
            }
        }
    }
}
