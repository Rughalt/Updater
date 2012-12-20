using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Updater
{
    public partial class UpdaterForm : Form
    {
        private string ApplicationName;

        private string ApplicationNameLong;

        private bool ApplicationNameSet = false;

        private string ApplicationRestartPath;

        private int CurrentFile = 0;
        private bool DownloadCanceled = false;

        private bool Downloading = false;

        private string InstallPath = "";

        private string LastSwitch;

        private string LocalFileName = "";

        private WebClient LocalWebClient = new WebClient();

        private WebClient LocalWebSearchClient = new WebClient();

        private StreamWriter LogWriter;
        private int NumberOfFiles = 0;
        private bool Silent = false;

        private Process UpdaterProcess = new Process();

        private string UpdateServer;

        private bool UpdateServerSet = false;

        private string UpdateURI = "";

        private String[] UpdateUrls;
        private bool UseInstallPath = false;

        private string VersionString;

        private bool VersionStringSet = false;

        public UpdaterForm()
        {
            InitializeComponent();
            LogWriter = new StreamWriter(Application.StartupPath + "\\Update.log");
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            if (LocalWebClient.IsBusy)
            {
                DownloadCanceled = true;
                LocalWebClient.CancelAsync();
            }
            else
            {
                LogWriter.Close();
                Process.Start(ApplicationRestartPath);
                Application.Exit();
            }
        }

        private void DownloadUpdate(string UpdatePath)
        {
            File.Delete(Application.StartupPath + "\\updated.temp");
            CurrentFile = 0;
            DownloadCanceled = false;
            LocalFileName = Path.GetTempPath() + "\\" + ApplicationName + ".exe";
            UpdateUrls = UpdatePath.Split('~');
            NumberOfFiles = UpdateUrls.Length;
            LogWriter.WriteLine("Files to be updated (" + NumberOfFiles.ToString() + ")");
            foreach (String UpdatePathPart in UpdateUrls)
            {
                LogWriter.WriteLine("\t" + UpdatePathPart);
            }
            KillProc(UpdateUrls[0]);
            LogWriter.Write("Downloading " + UpdateServer + "/" + ApplicationName + "/" + UpdateUrls[0]);
            LogWriter.WriteLine(" to " + Application.StartupPath + "\\" + UpdateUrls[0]);
            LocalWebClient.DownloadFileAsync(new Uri(UpdateServer + "/" + ApplicationName + "/" + UpdateUrls[0]), Application.StartupPath + "\\updated.temp");
            label1.Text = "Downloading update";
        }

        private void KillProc(string ProcName)
        {
            try
            {
                foreach (Process proc in Process.GetProcessesByName(ProcName.Replace(".exe", "")))
                {
                    proc.Kill();
                    LogWriter.WriteLine("Killing process: " + ProcName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void LocalWebClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            File.Delete(Application.StartupPath + "\\" + UpdateUrls[CurrentFile]);
            File.Move(Application.StartupPath + "\\updated.temp", Application.StartupPath + "\\" + UpdateUrls[CurrentFile]);
            if (!DownloadCanceled)
            {
                LogWriter.WriteLine("Fished downloading");
                CurrentFile++;
                if (CurrentFile < NumberOfFiles)
                {
                    KillProc(UpdateUrls[CurrentFile]);
                    LogWriter.Write("Downloading " + UpdateServer + "/" + ApplicationName + "/" + UpdateUrls[CurrentFile]);
                    LogWriter.WriteLine(" to " + Application.StartupPath + "\\" + UpdateUrls[CurrentFile]);
                    LocalWebClient.DownloadFileAsync(new Uri(UpdateServer + "/" + ApplicationName + "/" + UpdateUrls[CurrentFile]), Application.StartupPath + "\\updated.temp");
                }
                else
                {
                    label1.Text = "Update installed";
                    CancelButton.Text = "Close";
                }
            }
            else
            {
                label1.Text = "Update canceled";
                CancelButton.Text = "Close";
                System.IO.File.Delete(LocalFileName);
            }
        }

        private void LocalWebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            //((ListViewItem)e.UserState).SubItems[3].Text = e.ProgressPercentage.ToString() + "%";
            progressBar1.Value = e.ProgressPercentage;

            //DownloadSpeed.Text = (e.BytesReceived / 1024).ToString() + "/" + (e.TotalBytesToReceive / 1024).ToString() + " KB";
        }

        private void UpdateChecker_DoWork(object sender, DoWorkEventArgs e)
        {
            label1.Text = "Checking for updates";
            foreach (string Argument in Environment.GetCommandLineArgs())
            {
                if (LastSwitch == "v")
                {
                    VersionString = Argument;
                    LastSwitch = "";
                }
                if (LastSwitch == "rs")
                {
                    ApplicationRestartPath = Argument;
                    LastSwitch = "";
                }
                else if (LastSwitch == "an")
                {
                    ApplicationName = Argument;
                    LastSwitch = "";
                }

                else if (LastSwitch == "an")
                {
                    UseInstallPath = true;
                    InstallPath = Argument;
                    LastSwitch = "";
                }
                else if (LastSwitch == "al")
                {
                    ApplicationNameLong = Argument;

                    LastSwitch = "";
                }
                else if (LastSwitch == "s")
                {
                    UpdateServer = Argument;
                    LastSwitch = "";
                }
                else
                {
                    switch (Argument)
                    {
                        case "-v":
                            LastSwitch = "v";
                            break;

                        case "-sl":
                            Silent = true;
                            break;

                        case "-rs":
                            LastSwitch = "rs";
                            break;

                        case "-an":
                            LastSwitch = "an";
                            break;

                        case "-al":
                            LastSwitch = "al";
                            break;

                        case "-s":
                            LastSwitch = "s";
                            break;

                        case "-ip":
                            LastSwitch = "ip";
                            break;

                        default:
                            break;
                    }
                }
            }
            TextLabel.Text = "Updating " + ApplicationNameLong;
            String UpdateFileString = LocalWebSearchClient.DownloadString(UpdateServer + "/" + ApplicationName + ".fupd");
            int UpdateVersion = Convert.ToInt32(UpdateFileString.Split(";".ToCharArray()[0])[0]);
            if (UpdateVersion > Convert.ToInt32(VersionString))
            {
                DownloadUpdate(UpdateFileString.Split(";".ToCharArray()[0])[1]);
            }
            else
            {
                label1.Text = "No update found";

                CancelButton.Text = "Close";
            }
        }

        private void UpdaterForm_Load(object sender, EventArgs e)
        {
            LocalWebClient.DownloadFileCompleted += new AsyncCompletedEventHandler(LocalWebClient_DownloadFileCompleted);
            LocalWebClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(LocalWebClient_DownloadProgressChanged);
        }

        private void UpdaterForm_Shown(object sender, EventArgs e)
        {
            UpdateChecker.RunWorkerAsync();
        }

        private void UpdaterProcess_Exited(object sender, EventArgs e)
        {
            label1.Text = "Update installed";
            CancelButton.Text = "Close";
        }
    }
}