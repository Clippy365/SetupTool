using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;
using Newtonsoft.Json;
using System.Windows.Forms.Layout;
using System.Collections;
using System.Diagnostics;
using Microsoft.Win32;

namespace SetupTool
{
    public partial class MainWindow : Form
    {
        string[] settings = { "Uninstall OneDrive®", "Uninstall Bloatware", "Change privacy settings to strict", "Disable start menu ads", "Don't show last used files in explorer", "Disable settings cloudsync" };

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Start_btn_Click(object sender, EventArgs e)
        {
            if (!isChocolateyInstalled() && checkedListBoxApps.Items.Count > 0)
                installChocolatey();

            installPackages();

            //The corresponding methods are named (simililarly) to the strings in checkedListBoxSettings
            string[] checkedItems = checkedListBoxSettings.CheckedItems.Cast<string>().ToArray();
            for (int i = 0; i < checkedItems.Length; i++)
            {
                checkedItems[i] = checkedItems[i].Replace(" ", "_"); //Actually the way Microsoft does this
                checkedItems[i] = checkedItems[i].Replace("®", "");
                checkedItems[i] = checkedItems[i].Replace("'", "");
                
                
                Type thisType = this.GetType();
                System.Reflection.MethodInfo theMethod = thisType.GetMethod(checkedItems[i]);
                
                try
                { theMethod.Invoke(this, null); }
                catch(Exception ex)
                { MessageBox.Show(ex.Message); }
            }
        }

        private void Exit_btn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Hashtable list = readApplicationList();

            if (list != null)
            {
                foreach (DictionaryEntry de in list)
                {
                    checkedListBoxApps.Items.Add(de.Key.ToString());
                }
            }
            
            checkedListBoxSettings.Items.AddRange(settings);
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox ab = new AboutBox();
            ab.ShowDialog();
        }

        private void ButtonAddPackage_Click(object sender, EventArgs e)
        {
            AddPackage addp = new AddPackage();
            addp.ShowDialog();
            if(addp.displayName != "" && addp.packageName != "")
                checkedListBoxApps.Items.Add(addp.displayName);
        }

        /// <summary>
        /// Reads out the file "applicationList.json"
        /// </summary>
        /// <returns>An hashtable object with all key/value pairs inside "applicationlist.json"</returns>

        private Hashtable readApplicationList()
        {
            string applicationList = "applicationList.json";
            string fullPath = System.IO.Directory.GetCurrentDirectory() + "\\" + applicationList;

            FileInfo fi = new FileInfo(applicationList);
            if (fi.Exists)
            {
                Hashtable list = JsonConvert.DeserializeObject<Hashtable>(File.ReadAllText(fullPath));
                return list;
            }

            return null;
        }

        
        /// <summary>
        /// Creates the file "applicationList.json" or overwrites an existing one
        /// </summary>
        /// <param name="ht">The Hashtable object that will be written to "applicationList.json"</param>
        private void writeApplicationList(Hashtable ht)
        {
            string applicationList = "applicationList.json";
            string fullPath = System.IO.Directory.GetCurrentDirectory() + "\\" + applicationList;

            FileInfo fi = new FileInfo(applicationList);
            if (fi.Exists)
            {
                try
                {
                    string json = JsonConvert.SerializeObject(ht, Formatting.Indented);
                    System.IO.File.WriteAllText(fullPath, json);
                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }
            }
        }

        private void ButtonDeletePackage_Click(object sender, EventArgs e)
        {
            string[] itemsToDelete = checkedListBoxApps.CheckedItems.Cast<string>().ToArray();
                                   
            if (itemsToDelete != null)
            {
                Hashtable list = readApplicationList();
                foreach (string str in itemsToDelete)
                {
                    list.Remove(str);
                    checkedListBoxApps.Items.Remove(str);
                }
                writeApplicationList(list);
            }
        }

        private void buttonCheckRecommendedSoftware_Click(object sender, EventArgs e)
        {
            //Currently not used
        }

        private void buttonUncheckAllSoftware_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBoxApps.Items.Count; i++)
                checkedListBoxApps.SetItemCheckState(i, CheckState.Unchecked);
        }

        private void buttonCheckRecommendedSettings_Click(object sender, EventArgs e)
        {
            //Currently not used
        }

        private void buttonUncheckAllSettings_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBoxSettings.Items.Count; i++)
                checkedListBoxSettings.SetItemCheckState(i, CheckState.Unchecked);
        }

        
        /// <summary>
        /// Installs all packages, that have been checked in "checkedListBoxApps"
        /// </summary>
        private void installPackages()
        {
            string[] checkedElements = checkedListBoxApps.CheckedItems.Cast<String>().ToArray();
            Hashtable packagesApplicationList = readApplicationList();
            List<string> checkedPackages = new List<string>();

            if (checkedElements.Length < 1)
                return;

            for (int i=0; i < checkedElements.Count<string>(); i++)
            {
                checkedPackages.Add(packagesApplicationList[checkedElements[i]].ToString());
            }

            string chocolateyCommand = "";
            foreach (string str in checkedPackages)
                chocolateyCommand += str + " ";

            chocolateyCommand = "choco install " + chocolateyCommand + "-y";

            executeShellCommand(chocolateyCommand);
        }

        /// <summary>
        /// Checks if chocolatey package manager is installed
        /// </summary>
        /// <returns>True if chocolatey is installed, false if it isn't</returns>
        private bool isChocolateyInstalled()
        {
            FileInfo fi = new FileInfo(@"C:\ProgramData\chocolatey\choco.exe");

            if (fi.Exists)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Installs the chocolatey package manager on the system
        /// </summary>
        private void installChocolatey()
        {
            //This command is from chocolatey.org. It runs an installer using the PowerShell
            string installCommand = "Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))";
            
            executeShellCommand(installCommand);
            Application.Restart(); //TODO: Test
        }

        /// <summary>
        /// Executes a command on a PowerShell
        /// </summary>
        /// <param name="command">The command to be executed</param>
        private void executeShellCommand(string command)
        {
            Process shell = new Process();
            shell.StartInfo.FileName = "powershell.exe";
            shell.StartInfo.RedirectStandardInput = true;
            shell.StartInfo.RedirectStandardOutput = false;
            shell.StartInfo.RedirectStandardError = true;
            shell.StartInfo.UseShellExecute = false;
            shell.StartInfo.CreateNoWindow = false;
            shell.Start();

            shell.StandardInput.WriteLine(command);
            shell.StandardInput.Flush();
            shell.StandardInput.Close();
        }


        public void Uninstall_OneDrive()
        {
            string oneDriveSetupPath = Environment.ExpandEnvironmentVariables("%SYSTEMROOT%");
            oneDriveSetupPath += "\\SysWOW64\\OneDriveSetup.exe";
            FileInfo fi = new FileInfo(oneDriveSetupPath);

            //For 32-bit machines use the 32-bit path
            if (!fi.Exists)
            {
                oneDriveSetupPath = Environment.ExpandEnvironmentVariables("%SYSTEMROOT%");
                oneDriveSetupPath += "\\System32\\OneDriveSetup.exe";
            }

            //Kill all OneDrive processes before uninstalling anything
            Process[] OneDriveProcesses = Process.GetProcessesByName("OneDrive.exe");
            foreach (Process p in OneDriveProcesses)
                p.Kill();


            //Remove OneDrive with OneDriveSetup.exe
            Process oneDriveSetup = new Process();
            oneDriveSetup.StartInfo.FileName = oneDriveSetupPath;
            oneDriveSetup.StartInfo.Arguments = "/uninstall";
            oneDriveSetup.Start();

        }

        /// <summary>
        /// For now this removes a bunch of preinstalled Apps.
        /// </summary>
        public void Uninstall_Bloatware()
        {
            string shellCommand = @"Get-AppxPackage *3dbuilder* | Remove-AppxPackage
                                    Get - AppxPackage * Appconnector * | Remove - AppxPackage
                                    Get - AppxPackage * GAMELOFTSA.Asphalt8Airborne * | Remove - AppxPackage
                                    Get - AppxPackage * CandyCrushSodaSaga * | Remove - AppxPackage
                                    Get - AppxPackage * FarmVille2CountryEscape * | Remove - AppxPackage
                                    Get - AppxPackage * WindowsFeedbackHub * | Remove - AppxPackage
                                    Get - AppxPackage * officehub * | Remove - AppxPackage
                                    Get - AppxPackage * skypeapp * | Remove - AppxPackage
                                    Get - AppxPackage * getstarted * | Remove - AppxPackage
                                    Get - AppxPackage * zunemusic * | Remove - AppxPackage
                                    Get - AppxPackage * windowsmaps * | Remove - AppxPackage
                                    Get - AppxPackage * Messaging * | Remove - AppxPackage
                                    Get - AppxPackage * solitairecollection * | Remove - AppxPackage
                                    Get - AppxPackage * ConnectivityStore * | Remove - AppxPackage
                                    Get - AppxPackage * bingfinance * | Remove - AppxPackage
                                    Get - AppxPackage * zunevideo * | Remove - AppxPackage
                                    Get - AppxPackage * Netflix * | Remove - AppxPackage
                                    Get - AppxPackage * bingnews * | Remove - AppxPackage
                                    Get - AppxPackage * OneConnect * | Remove - AppxPackage
                                    Get - AppxPackage * people * | Remove - AppxPackage
                                    Get - AppxPackage * CommsPhone * | Remove - AppxPackage
                                    Get - AppxPackage * windowsphone * | Remove - AppxPackage
                                    Get - AppxPackage * bingsports * | Remove - AppxPackage
                                    Get - AppxPackage * Office.Sway * | Remove - AppxPackage
                                    Get - AppxPackage * Twitter * | Remove - AppxPackage
                                    Get - AppxPackage * soundrecorder * | Remove - AppxPackage
                                    Get - AppxPackage * bingweather * | Remove - AppxPackage
                                    Get - AppxPackage * TuneInRadio * | Remove - AppxPackage
                                    Get - AppxPackage * Microsoft.AgeCastles * | Remove - AppxPackage
                                    Get - AppxPackage * Drawboard * | Remove - AppxPackage
                                    Get - AppxPackage * Microsoft.XboxIdentityProvider * | Remove - AppxPackage
                                    Get - AppxPackage * xboxapp * | Remove - AppxPackage
                                    Get - AppxPackage * XboxOneSmartGlass * | Remove - AppxPackage
                                    Get - AppxPackage * facebook * | Remove - AppxPackage";

            Process.Start("Powershell.exe", shellCommand);
        }

        /// <summary>
        /// For now this just changes a bunch of registry settings without any granularity. May be changed in the future
        /// </summary>
        public void Change_privacy_settings_to_strict()
        {
            Process regeditProcess = Process.Start("regedit.exe", "/s ChangePrivacySettingsToStrict.reg");
            regeditProcess.WaitForExit();
        }

        public void Disable_start_menu_ads()
        {
            Process regeditProcess = Process.Start("regedit.exe", "/s DisableStartMenuAds.reg");
            regeditProcess.WaitForExit();
        }

        public void Dont_show_last_used_files_in_explorer()
        {
            Process regeditProcess = Process.Start("regedit.exe", "/s DontShowLastFilesInExplorer.reg");
            regeditProcess.WaitForExit();
        }

        /// <summary>
        /// Disables the sync of Windows Settings via cloud
        /// </summary>
        public void Disable_settings_cloudsync()
        {
            Process regeditProcess = Process.Start("regedit.exe", "/s DisableSettingsCloudsync.reg");
            regeditProcess.WaitForExit();
        }
    }
}
