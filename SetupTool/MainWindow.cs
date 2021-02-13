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
        private int ttIndex = 0;
        private bool checkedForFirefox = false;

        // This string array has to be in the same order as the elements in checkedListBoxSettings in order to show the correct ToolTip
        private string[] ToolTipCaptions = { "Always turns on the NumLock key on your keyboard. Turned off by default", "Turn off certain settings like Windows telemetry, advertisements over Bluetooth or syncing clipboard contents via cloud. Turned on by default", "Only applies if you are logged in with an online account and turns off cloud synchronization of settings like Desktop background, etc. Turned on by default", "Disables ads in the Windows start menu. Turned on by default (except Windows 10 Enterprise)", "Turns off showing last used files and folder in file explorer. Turned on by default", "Installs and enables the Windows Subsystem for Linux version 2. Requires a reboot. Not installed by default", "Shows all file extensions (like *.pdf), no matter if the file extension is known or not. Turned off by default", "Always show all tray icons in the task bar. Turned off by default", "Uninstall apps like groove music or Candy Crush", "Uninstall OneDrive®" };

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Start_btn_Click(object sender, EventArgs e)
        {
            if (!isChocolateyInstalled() && checkedListBoxApps.SelectedItems.Count > 0)
                installChocolatey();

            installPackages();

            //The corresponding methods are named (simililarly) to the strings in checkedListBoxSettings
            string[] checkedItems = checkedListBoxSettings.CheckedItems.Cast<string>().ToArray();
            for (int i = 0; i < checkedItems.Length; i++)
            {
                checkedItems[i] = checkedItems[i].Replace(" ", "_");
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
            SortedList<string, string> list = readApplicationList();

            if (list != null)
            {
                foreach (KeyValuePair<string, string> kvp in list)
                {
                    checkedListBoxApps.Items.Add(kvp.Key.ToString());
                }
            }

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

        private SortedList<string, string> readApplicationList()
        {
            string applicationList = "applicationList.json";
            string fullPath = System.IO.Directory.GetCurrentDirectory() + "\\" + applicationList;

            FileInfo fi = new FileInfo(applicationList);
            if (fi.Exists)
            {
                SortedList<string, string> list = JsonConvert.DeserializeObject<SortedList<string, string>>(File.ReadAllText(fullPath));
                return list;
            }

            return null;
        }

        
        /// <summary>
        /// Creates the file "applicationList.json" or overwrites an existing one
        /// </summary>
        /// <param name="ht">The Hashtable object that will be written to "applicationList.json"</param>
        private void writeApplicationList(SortedList<string, string> sl)
        {
            string applicationList = "applicationList.json";
            string fullPath = System.IO.Directory.GetCurrentDirectory() + "\\" + applicationList;

            FileInfo fi = new FileInfo(applicationList);
            if (fi.Exists)
            {
                try
                {
                    string json = JsonConvert.SerializeObject(sl, Formatting.Indented);
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
                SortedList<string, string> list = readApplicationList();
                foreach (string str in itemsToDelete)
                {
                    list.Remove(str);
                    checkedListBoxApps.Items.Remove(str);
                }
                writeApplicationList(list);
            }
        }


        private void buttonUncheckAllSoftware_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBoxApps.Items.Count; i++)
                checkedListBoxApps.SetItemCheckState(i, CheckState.Unchecked);
        }

        private void buttonCheckAllSettings_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBoxSettings.Items.Count; i++)
                checkedListBoxSettings.SetItemCheckState(i, CheckState.Checked);
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
            SortedList<string, string> packagesApplicationList = readApplicationList();
            List<string> checkedPackages = new List<string>();

            if (checkedElements.Length < 1)
                return;

            for (int i=0; i < checkedElements.Count<string>(); i++)
            {
                try { checkedPackages.Add(packagesApplicationList[checkedElements[i]].ToString()); }
                catch(Exception ex) { }
            }

            string chocolateyCommand = "";
            foreach (string str in checkedPackages)
                chocolateyCommand += str + " ";

            if (!string.IsNullOrEmpty(chocolateyCommand))
            {
                //Using full path, so no restart of SetupTool is necessary
                string choco = Environment.ExpandEnvironmentVariables("%SYSTEMDRIVE%") + @"\ProgramData\chocolatey\bin\choco.exe";
                chocolateyCommand = choco + " install " + chocolateyCommand + "-y";
                executeShellCommand(chocolateyCommand);
            }

            // Add Adblock Plus to Firefox if checked (Chrome doesn't support extension sideloading)
            int ffindex = checkedListBoxApps.Items.IndexOf("Mozilla Firefox");
            string sysdrive = System.Environment.ExpandEnvironmentVariables("%SYSTEMDRIVE%");
            if (checkedListBoxApps.GetItemChecked(ffindex + 1))
                executeShellCommand(sysdrive + @"'\Program Files\Mozilla Firefox\firefox.exe' https://addons.mozilla.org/firefox/downloads/latest/1865/addon-1865-latest.xpi");

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
        }

        /// <summary>
        /// Executes a command on a PowerShell
        /// </summary>
        /// <param name="command">The command to be executed</param>
        /// <param name="FileName">The process name to start (default is powershell.exe)</param>
        private void executeShellCommand(string command)
        {
            var processInfo = new ProcessStartInfo { FileName = "powershell.exe", Arguments = command };
            using (var process = Process.Start(processInfo))
            {
                process.WaitForExit();
            }
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
        public void Uninstall_bloatware()
        {
            string[] apps = {   // default Windows 10 apps
                                "Microsoft.3DBuilder",
                                "Microsoft.Appconnector",
                                "Microsoft.BingFinance",
                                "Microsoft.BingNews",
                                "Microsoft.BingSports",
                                "Microsoft.BingTranslator",
                                "Microsoft.BingWeather",
                                "Microsoft.GamingServices",
                                "Microsoft.Microsoft3DViewer",
                                "Microsoft.MicrosoftOfficeHub",
                                "Microsoft.MicrosoftPowerBIForWindows",
                                "Microsoft.MicrosoftSolitaireCollection",
                                "Microsoft.MinecraftUWP",
                                "Microsoft.NetworkSpeedTest",
                                //"Microsoft.Office.OneNote",
                                "Microsoft.People",
                                "Microsoft.Print3D",
                                "Microsoft.SkypeApp",
                                "Microsoft.Wallet",
                                "Microsoft.WindowsAlarms",
                                //"Microsoft.WindowsCamera",
                                "microsoft.windowscommunicationsapps",
                                "Microsoft.WindowsMaps",
                                "Microsoft.WindowsPhone",
                                "Microsoft.WindowsSoundRecorder",
                                "Microsoft.Xbox.TCUI",
                                "Microsoft.XboxApp",
                                //"Microsoft.XboxGameOverlay",
                                //"Microsoft.XboxGamingOverlay",
                                "Microsoft.XboxSpeechToTextOverlay",
                                "Microsoft.YourPhone",
                                "Microsoft.ZuneMusic",
                                "Microsoft.ZuneVideo",

                                // Threshold 2 apps,
                                "Microsoft.CommsPhone",
                                "Microsoft.ConnectivityStore",
                                "Microsoft.GetHelp",
                                "Microsoft.Getstarted",
                                "Microsoft.Messaging",
                                "Microsoft.Office.Sway",
                                "Microsoft.OneConnect",
                                "Microsoft.WindowsFeedbackHub",

                                // Creators Update apps,
                                "Microsoft.Microsoft3DViewer",
                                "Microsoft.MSPaint",

                                // Redstone apps,
                                "Microsoft.BingFoodAndDrink",
                                "Microsoft.BingHealthAndFitness",
                                "Microsoft.BingTravel",
                                "Microsoft.WindowsReadingList",

                                // Redstone 5 apps,
                                "Microsoft.MixedReality.Portal",
                                "Microsoft.ScreenSketch",
                                //"Microsoft.XboxGamingOverlay",
                                "Microsoft.YourPhone",

                                // non-Microsoft,
                                "2FE3CB00.PicsArt-PhotoStudio",
                                "46928bounde.EclipseManager",
                                "4DF9E0F8.Netflix",
                                "613EBCEA.PolarrPhotoEditorAcademicEdition",
                                "6Wunderkinder.Wunderlist",
                                "7EE7776C.LinkedInforWindows",
                                "89006A2E.AutodeskSketchBook",
                                "9E2F88E3.Twitter",
                                "A278AB0D.DisneyMagicKingdoms",
                                "A278AB0D.MarchofEmpires",
                                "ActiproSoftwareLLC.562882FEEB491",
                                "CAF9E577.Plex",
                                "ClearChannelRadioDigital.iHeartRadio",
                                "D52A8D61.FarmVille2CountryEscape",
                                "D5EA27B7.Duolingo-LearnLanguagesforFree",
                                "DB6EA5DB.CyberLinkMediaSuiteEssentials",
                                "DolbyLaboratories.DolbyAccess",
                                "DolbyLaboratories.DolbyAccess",
                                "Drawboard.DrawboardPDF",
                                "Facebook.Facebook",
                                "Fitbit.FitbitCoach",
                                "Flipboard.Flipboard",
                                "GAMELOFTSA.Asphalt8Airborne",
                                "KeeperSecurityInc.Keeper",
                                "NORDCURRENT.COOKINGFEVER",
                                "PandoraMediaInc.29680B314EFC2",
                                "Playtika.CaesarsSlotsFreeCasino",
                                "ShazamEntertainmentLtd.Shazam",
                                "SlingTVLLC.SlingTV",
                                "SpotifyAB.SpotifyMusic",
                                "ThumbmunkeysLtd.PhototasticCollage",
                                "TuneIn.TuneInRadio",
                                "WinZipComputing.WinZipUniversal",
                                "XINGAG.XING",
                                "flaregamesGmbH.RoyalRevolt2",
                                "king.com.*",
                                "king.com.BubbleWitch3Saga",
                                "king.com.CandyCrushSaga",
                                "king.com.CandyCrushSodaSaga",
                                //ab hier neu
                                "SpotifyAB.SpotifyMusic_zpdnekdrzrea0!Spotify",
                                "A278AB0D.MarchofEmpires_h6adky7gbf63m!App",
                                "ROBLOXCorporation.ROBLOX_55nm5eh3cm0pr!App",
                                "828B5831.HiddenCityMysteryofShadows_ytsefhwckbdv6!App",
                                "1424566A.147190DF3DE79_5byw4zywtsh80!App"
                              };

            string shellCommand = "";
            foreach (string str in apps)
            {
                shellCommand += "Get-AppxPackage " + str + " | Remove-AppxPackage\n"; 
            }

            executeShellCommand(shellCommand);

            cleanUpStartMenu();

        }
    

        /// <summary>
        /// For now this just changes a bunch of registry settings without any granularity. May be changed in the future
        /// </summary>
        public void Change_privacy_settings_to_strict()
        {
            ImportRegFile("ChangePrivacySettingsToStrict.reg");
        }

        public void Disable_start_menu_ads()
        {
            ImportRegFile("DisableStartMenuAds.reg");
        }

        public void Dont_show_last_used_files_in_file_explorer()
        {
            ImportRegFile("DontShowLastFilesInExplorer.reg");
        }

        /// <summary>
        /// Disables the sync of Windows Settings via cloud
        /// </summary>
        public void Disable_settings_cloudsync()
        {
            ImportRegFile("DisableSettingsCloudsync.reg");
        }


        /// <summary>
        /// The Numlock key on the numblock of a keyboard will always be on if set
        /// </summary>
        public void Always_turn_on_numlock()
        {
            ImportRegFile("AlwaysTurnOnNumlock.reg");
        }

        /// <summary>
        /// Windows does hide file extensions for known filetypes by default. This setting will always show the file extension
        /// </summary>
        public void Show_all_file_extensions()
        {
            ImportRegFile("ShowAllFileExtensions.reg");
        }
            
        /// <summary>
        /// Windows does not show all tray icons by default. This setting will always show all tray icons
        /// </summary>
        public void Show_all_tray_icons()
        {
            ImportRegFile("ShowAllTrayIcons.reg");
        }


        /// <summary>
        /// Disables the mouse acceleration, which is turned on by default. Regfile from https://gist.github.com/razorenov/ea9e2c85aecf2606009328bd79b6c890#file-mouse_fix-reg
        /// </summary>
        //public void Disable_mouse_acceleration()
        //{
        //    Process regeditProcess = Process.Start("regedit.exe", "/s DisableMouseAcceleration.reg");
        //    regeditProcess.WaitForExit();
        //}

        /// <summary>
        /// Imports a start menu tile layout via registry key (Chromium Edge, MS Store and Photos App)
        /// </summary>
        public void cleanUpStartMenu()
        {
            try
            { Registry.CurrentUser.DeleteSubKeyTree(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\CloudStore\Store\Cache\DefaultAccount", true); }

            catch(Exception ex)
            { MessageBox.Show(ex.Message); }

            ImportRegFile("StartMenuLayout.reg");

            MessageBox.Show("You will have to log out and log back in, to finish start menu cleanup");
        }


        /// <summary>
        /// Imports a .reg file and sets the corresponding registry keys and values
        /// </summary>
        /// <param name="regFile">The reg file to merge with the Windows registry. Has to end with ".reg"</param>
        public void ImportRegFile(string regFile)
        {
            Process regeditProcess = Process.Start("regedit.exe", "/s RegFiles\\" + regFile);
            regeditProcess.WaitForExit();
        }


        /// <summary>
        /// Shows a tooltip for every checkedListBoxItem when hovered over with the mouse
        /// </summary>
        private void ShowToolTip(object sender, MouseEventArgs e)
        {
            if (ttIndex != this.checkedListBoxSettings.IndexFromPoint(e.Location))
            {
                ttIndex = checkedListBoxSettings.IndexFromPoint(checkedListBoxSettings.PointToClient(MousePosition));
                if (ttIndex > -1)
                {
                    toolTip1.SetToolTip(checkedListBoxSettings, ToolTipCaptions[ttIndex].ToString());
                }
            }
        }

        /// <summary>
        /// Installs and enables the Windows Subsystem for Linux version 2
        /// </summary>
        public void Enable_WSL2()
        {
            // Install WSL1
            executeShellCommand("dism.exe /online /enable-feature /featurename:Microsoft-Windows-Subsystem-Linux /all /norestart");
            
            // Install Virtual machine platform
            executeShellCommand("dism.exe /online /enable-feature /featurename:VirtualMachinePlatform /all /norestart");

            // Install the Linux kernel update package
            executeShellCommand("(New-Object System.Net.WebClient).DownloadFile('https://wslstorestorage.blob.core.windows.net/wslblob/wsl_update_x64.msi', 'wsl_update_x64.msi')");
            executeShellCommand("msiexec /i wsl_update_x64.msi /passive");

            // Set WSL 2 as default version
            executeShellCommand("wsl --set-default-version 2");
        }

        
        /// <summary>
        /// Checks if Chrome or Firefox is selected and adds the option to add Adblock Plus if so
        /// </summary>
        public void checkForFirefox()
        {

            if (!checkedForFirefox && checkedListBoxApps.CheckedItems.Contains("Mozilla Firefox"))
            {
                int index = checkedListBoxApps.Items.IndexOf("Mozilla Firefox");
                checkedListBoxApps.Items.Insert(index + 1, "    Adblock Plus for Mozilla Firefox");
                //checkedListBoxApps.Items.Insert(index + 2, "    Block Cookie banners");
                checkedForFirefox = true;
            }

        }

        private void checkedListBoxApps_Click(object sender, ItemCheckEventArgs e)
        {
            //checkForChromeOrFirefox();
        }

        private void checkedListBoxApps_Click(object sender, EventArgs e)
        {
            checkForFirefox();
        }
    }
}
