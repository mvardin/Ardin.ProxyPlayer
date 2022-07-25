using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telegram.Bot;

namespace Ardin.ProxyPlayer.Engine
{
    public partial class frmMain : Form
    {
        public string CurrentLink { get; set; }
        public bool IsCompleted { get; set; }
        public frmMain()
        {
            InitializeComponent();
        }
        private void btnStart_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        lblStatus.Invoke((MethodInvoker)delegate
                        {
                            lblStatus.Text = $"Last check for {DateTime.Now}";
                        });
                        string path = ConfigurationManager.AppSettings["StoragePath"];
                        string downloadListPath = Path.Combine(Application.StartupPath, "DownloadList.txt");
                        if (File.Exists(downloadListPath))
                        {
                            string[] lines = File.ReadAllLines(downloadListPath);
                            foreach (var toDownload in lines)
                            {
                                IsCompleted = false;
                                CurrentLink = toDownload;
                                Log("start for " + CurrentLink);
                                string filename = Path.GetFileNameWithoutExtension(CurrentLink) + "_original" + Path.GetExtension(CurrentLink);
                                string originalPath = Path.Combine(path, filename);
                                if (!File.Exists(originalPath))
                                {
                                    WebClient webClient = new WebClient();
                                    webClient.DownloadFileAsync(new Uri(CurrentLink), originalPath);
                                    webClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;
                                    webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
                                }
                                else
                                    WebClient_DownloadFileCompleted(sender, null);
                                while (!IsCompleted)
                                {
                                    Thread.Sleep(5 * 1000);
                                }
                            }
                            File.Delete(downloadListPath);
                        }
                        else Thread.Sleep(10 * 1000);
                    }
                    catch (Exception ex)
                    {
                        Log("while", ex);
                    }
                }
            });
        }
        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            lblStatus.Invoke((MethodInvoker)delegate
            {
                lblStatus.Text = $"{(e.BytesReceived / 1024)} kb from {(e.TotalBytesToReceive / 1024)} kb Received , {e.ProgressPercentage} %";
            });
        }
        private void WebClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            string filename = Path.GetFileNameWithoutExtension(CurrentLink) + "_original" + Path.GetExtension(CurrentLink);
            string path = ConfigurationManager.AppSettings["StoragePath"];
            string originalPath = Path.Combine(path, filename);
            string convertedPath = Path.Combine(path, filename.Replace("_original", string.Empty));
            if (!File.Exists(convertedPath))
            {
                try
                {
                    string lame = ConfigurationManager.AppSettings["ffmpegPath"];
                    string args = $"-i \"{originalPath}\" -vf scale=852:480 \"{convertedPath}\"";

                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = lame,
                        Arguments = args
                    };
                    if (false)
                    {
                        startInfo.UseShellExecute = false;
                        startInfo.CreateNoWindow = true;
                    }
                    var p = Process.Start(startInfo);
                    p.WaitForExit();
                }
                catch (Exception ex)
                {
                    Log("Convert", ex);
                }
            }
            try
            {
                TelegramBotClient telegram = new TelegramBotClient("677309981:AAHQUaKSKOqXTvPGCwDJfehufoFOXX8GUx0");
                var sentMessage = telegram.SendTextMessageAsync(77132506, $"https://proxyplayer.drcaptcha.ir?filename={filename.Replace("_original", string.Empty)}").Result;
            }
            catch (Exception ex)
            {
                Log("Telegram", ex);
            }
            IsCompleted = true;
        }
        private void Log(string message, Exception ex = null)
        {
            if (ex != null)
            {
                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                }
                message += " | " + ex.Message;
            }
            if (lbLog.InvokeRequired)
            {
                lbLog.Invoke((MethodInvoker)delegate
                {
                    lbLog.Items.Insert(0, DateTime.Now.ToString() + "\t" + message);
                });
            }
            else
            {
                lbLog.Items.Insert(0, DateTime.Now.ToString() + "\t" + message);
            }
        }
    }
}