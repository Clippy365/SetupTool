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
        string[] settings = { "Uninstall OneDrive®", "Uninstall Bloatware", "Change privacy settings to strict", "Disable start menu ads", "Don't show last used files in explorer" };

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
        /// For now this just changes a bunch of registry keys without any granularity. May be changed in the future
        /// </summary>
        public void Change_privacy_settings_to_strict()
        {
            string[] HKCUKeysToCreate   = { @"Software\Microsoft\Windows\CurrentVersion\Explorer\ComDlg32\CIDSizeMRU\", @"Software\Microsoft\Windows\CurrentVersion\Explorer\ComDlg32\LastVisitedPidlMRU\", @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\BagMRU", @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\BagMRU\1", @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\BagMRU\1\0", @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\BagMRU\1", @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\BagMRU\1\0", @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\BagMRU", @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\BagMRU\1", @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\BagMRU\1\0", @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\BagMRU\1", @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\BagMRU\1\0", @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\BagMRU", @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\BagMRU\1", @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\BagMRU\1\0", @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\BagMRU\1", @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\BagMRU\1\0", @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\Bags\2\ComDlg\{885A186E-A440-4ADA-812B-DB871B942259}", @"Software\Classes\CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}\Instance", @"Software\Classes\CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}\Instance", @"Software\Microsoft\Windows\CurrentVersion\Explorer\RunMRU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\ComDlg32\LastVisitedPidlMRU\", @"Software\Microsoft\Windows\CurrentVersion\Explorer\ComDlg32\OpenSavePidlMRU\cfg", @"Software\Microsoft\Windows\CurrentVersion\Explorer\ComDlg32\OpenSavePidlMRU\*", @"Software\Microsoft\Windows\CurrentVersion\Explorer\CIDOpen\Modules\GlobalSettings\ProperTreeModuleInner", @"Software", @"SOFTWARE\Microsoft", @"SOFTWARE\Microsoft\Windows", @"SOFTWARE\Microsoft\Windows\CurrentVersion", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\CIDOpen", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\CIDOpen\Modules", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\CIDOpen\Modules\GlobalSettings", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\CIDOpen\Modules\GlobalSettings\ProperTreeModuleInner", @"Software\Microsoft\Windows\CurrentVersion\Explorer\Modules\NavPane", @"Software\Microsoft\Windows\CurrentVersion\Explorer\Modules\NavPane" };
            string[] HKLMKeysToCreate   = { @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\DelegateFolders" };
            string[] HKCRKeysToCreate   = { @"CLSID\{4336a54d-038b-4685-ab02-99bb52d3fb8b}\Instance", @"CLSID\{4336a54d-038b-4685-ab02-99bb52d3fb8b}\Instance" };
            string[] HKCUValuesToDelete = { @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\ComDlg32\LastVisitedPidlMRU\0", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\ComDlg32\CIDSizeMRU\0", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\ComDlg32\CIDSizeMRU\0", @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\Bags\2\ComDlg\{885A186E-A440-4ADA-812B-DB871B942259}\GroupCollapseState" };
            string[] HKCUValuesToSet    = { @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\Bags\2\Shell\SniffedFolderType", @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\BagMRU\NodeSlots", @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\BagMRU\MRUListEx", @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\BagMRU\NodeSlots", @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\BagMRU\MRUListEx", @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\BagMRU\NodeSlots", @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\BagMRU\MRUListEx", @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\Bags\2\ComDlg\{885A186E-A440-4ADA-812B-DB871B942259}\Version", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\ComDlg32\LastVisitedPidlMRU\0", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\ComDlg32\LastVisitedPidlMRU\MRUListEx", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\ComDlg32\CIDSizeMRU\0", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\ComDlg32\CIDSizeMRU\0", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\ComDlg32\CIDSizeMRU\MRUListEx", @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\Bags\2\ComDlg\{885A186E-A440-4ADA-812B-DB871B942259}\Mode", @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\Bags\2\ComDlg\{885A186E-A440-4ADA-812B-DB871B942259}\LogicalViewMode", @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\Bags\2\ComDlg\{885A186E-A440-4ADA-812B-DB871B942259}\FFlags", @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\Bags\2\ComDlg\{885A186E-A440-4ADA-812B-DB871B942259}\IconSize", @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\Bags\2\ComDlg\{885A186E-A440-4ADA-812B-DB871B942259}\Sort", @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\Bags\2\ComDlg\{885A186E-A440-4ADA-812B-DB871B942259}\ColInfo", @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\Bags\2\ComDlg\{885A186E-A440-4ADA-812B-DB871B942259}\GroupView", @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\Bags\2\ComDlg\{885A186E-A440-4ADA-812B-DB871B942259}\GroupByKey:FMTID", @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\Bags\2\ComDlg\{885A186E-A440-4ADA-812B-DB871B942259}\GroupByKey:PID", @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\Bags\2\ComDlg\{885A186E-A440-4ADA-812B-DB871B942259}\GroupByDirection", @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\Bags\2\ComDlg\{885A186E-A440-4ADA-812B-DB871B942259}\Mode", @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\Bags\2\ComDlg\{885A186E-A440-4ADA-812B-DB871B942259}\FFlags", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\CIDOpen\Modules\GlobalSettings\ProperTreeModuleInner\ProperTreeModuleInner", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Modules\NavPane\ExpandedState", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Modules\NavPane\ExpandedState", @"SOFTWARE\Microsoft\Windows\CurrentVersion\AdvertisingInfo\Enabled", @"SOFTWARE\Microsoft\Input\TIPC\Enabled", @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager\SubscribedContent-353698Enabled", @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager\SubscribedContent-338388Enabled", @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager\SubscribedContent-338389Enabled", @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager\SubscribedContent-338393Enabled", @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager\SubscribedContent-353694Enabled", @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager\SubscribedContent-353696Enabled", @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\userAccountInformation\Value", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced\Start_TrackProgs", @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\appDiagnostics\Value", @"SOFTWARE\Policies\Microsoft\Edge\ConfigureDoNotTrack", @"SOFTWARE\Policies\Microsoft\Edge\PaymentMethodQueryEnabled", @"SOFTWARE\Policies\Microsoft\Edge\SendSiteInfoToImproveServices", @"SOFTWARE\Policies\Microsoft\Edge\MetricsReportingEnabled", @"SOFTWARE\Policies\Microsoft\Edge\PersonalizationReportingEnabled", @"SOFTWARE\Policies\Microsoft\Edge\AddressBarMicrosoftSearchInBingProviderEnabled", @"SOFTWARE\Policies\Microsoft\Edge\UserFeedbackAllowed", @"SOFTWARE\Policies\Microsoft\Edge\AutofillCreditCardEnabled", @"SOFTWARE\Policies\Microsoft\Edge\AutofillAddressEnabled", @"SOFTWARE\Policies\Microsoft\Edge\LocalProvidersEnabled", @"SOFTWARE\Policies\Microsoft\Edge\SearchSuggestEnabled", @"Software\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppContainer\Storage\microsoft.microsoftedge_8wekyb3d8bbwe\MicrosoftEdge\Main\DoNotTrack", @"Software\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppContainer\Storage\microsoft.microsoftedge_8wekyb3d8bbwe\MicrosoftEdge\FlipAhead\FPEnabled", @"Software\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppContainer\Storage\microsoft.microsoftedge_8wekyb3d8bbwe\MicrosoftEdge\Main\ShowSearchSuggestionsGlobal", @"Software\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppContainer\Storage\microsoft.microsoftedge_8wekyb3d8bbwe\MicrosoftEdge\ServiceUI\EnableCortana", @"Software\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppContainer\Storage\microsoft.microsoftedge_8wekyb3d8bbwe\MicrosoftEdge\ServiceUI\ShowSearchHistory\(Default)", @"SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync\Groups\BrowserSettings\Enabled", @"SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Credentials\Enabled", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Windows Search\CortanaConsent", @"SOFTWARE\Microsoft\Personalization\Settings\AcceptedPrivacyPolicy", @"SOFTWARE\Microsoft\InputPersonalization\RestrictImplicitInkCollection", @"SOFTWARE\Microsoft\InputPersonalization\RestrictImplicitTextCollection", @"SOFTWARE\Microsoft\InputPersonalization\TrainedDataStore\HarvestContacts", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Privacy\TailoredExperiencesWithDiagnosticDataEnabled", @"SOFTWARE\Microsoft\Windows\CurrentVersion\DeliveryOptimization\SystemSettingsDownloadMode", @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager\SystemPaneSuggestionsEnabled", @"SOFTWARE\Microsoft\Siuf\Rules\NumberOfSIUFInPeriod", @"SOFTWARE\Microsoft\Siuf\Rules\PeriodInNanoSeconds", @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager\SilentInstalledAppsEnabled", @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager\SoftLandingEnabled", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Search\BingSearchEnabled" };
            string[] HKLMValuesToSet    = { @"SOFTWARE\Policies\Microsoft\Windows\TabletPC\PreventHandwritingDataSharing", @"SOFTWARE\Policies\Microsoft\Windows\HandwritingErrorReports\PreventHandwritingErrorReports", @"SOFTWARE\Policies\Microsoft\Windows\AppCompat\DisableInventory", @"SOFTWARE\Policies\Microsoft\Windows\Personalization\NoLockScreenCamera", @"SOFTWARE\Microsoft\Windows\CurrentVersion\AdvertisingInfo\Enabled", @"SOFTWARE\Microsoft\PolicyManager\current\device\System\AllowExperimentation", @"SOFTWARE\Microsoft\PolicyManager\current\device\Bluetooth\AllowAdvertising", @"SOFTWARE\Microsoft\Windows\Windows Error Reporting\Disabled", @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\userAccountInformation\Value", @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\appDiagnostics\Value", @"SOFTWARE\Policies\Microsoft\Windows\AppCompat\DisableUAR", @"System\CurrentControlSet\Services\DiagTrack\Start", @"System\CurrentControlSet\Services\dmwappushservice\Start", @"System\CurrentControlSet\Control\WMI\Autologger\AutoLogger-Diagtrack-Listener\Start", @"SOFTWARE\Microsoft\PolicyManager\current\device\Browser\AllowAddressBarDropdown", @"SOFTWARE\Policies\Microsoft\Edge\UserFeedbackAllowed", @"SOFTWARE\Policies\Microsoft\Edge\AutofillCreditCardEnabled", @"SOFTWARE\Policies\Microsoft\InputPersonalization\AllowInputPersonalization", @"SOFTWARE\Policies\Microsoft\Windows\Windows Search\AllowSearchToUseLocation", @"SOFTWARE\Policies\Microsoft\Windows\Windows Search\DisableWebSearch", @"SOFTWARE\Policies\Microsoft\Windows\Windows Search\ConnectedSearchUseWeb", @"SOFTWARE\Microsoft\Speech_OneCore\Preferences\ModelDownloadAllowed", @"SOFTWARE\Policies\Microsoft\Windows\Windows Search\AllowCloudSearch", @"SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors\DisableLocation", @"SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors\DisableWindowsLocationProvider", @"SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors\DisableLocationScripting", @"SOFTWARE\Policies\Microsoft\Windows\DataCollection\AllowTelemetry", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\DataCollection\AllowTelemetry", @"SOFTWARE\Policies\Microsoft\Windows\AppCompat\AITEnable", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Privacy\TailoredExperiencesWithDiagnosticDataEnabled", @"SOFTWARE\Microsoft\Windows\CurrentVersion\DeliveryOptimization\Config\DODownloadMode", @"SOFTWARE\Policies\Microsoft\Windows\DeliveryOptimization\DODownloadMode", @"SOFTWARE\Policies\Microsoft\Speech\AllowSpeechModelUpdate" };

            //TODO: Test if this works
            Array.Sort(HKCUKeysToCreate);
            Array.Sort(HKLMKeysToCreate);
            Array.Sort(HKCRKeysToCreate);
            Array.Sort(HKCUValuesToDelete);
            Array.Sort(HKCUValuesToSet);
            Array.Sort(HKLMValuesToSet);

            foreach (string str in HKCUKeysToCreate)
            {   
                RegistryKey key = null;
                key = Registry.CurrentUser.CreateSubKey(str);
                key.Close();
            }

            foreach (string str in HKLMKeysToCreate)
            {
                RegistryKey key = null;
                key = Registry.LocalMachine.CreateSubKey(str);
                key.Close();
            }

            foreach (string str in HKCRKeysToCreate)
            {
                RegistryKey key = null;
                key = Registry.ClassesRoot.CreateSubKey(str);
                key.Close();
            }

            foreach (string str in HKCUValuesToDelete)
            {
                Registry.CurrentUser.DeleteValue(str);
            }

            //TODO continue with setting the registy values. Create object array first
            for(int i = 0; i < HKCUKeysToCreate.Length; i++)
            {
                RegistryKey key = null;
                key = Registry.CurrentUser.CreateSubKey(HKCUKeysToCreate[i]);
                Registry.CurrentUser.SetValue(key, HKCUValuesToSet[i]);
            }
        }

        public void Disable_start_menu_ads()
        {

        }

        public void Dont_show_last_used_files_in_explorer()
        {

        }
    }
}
