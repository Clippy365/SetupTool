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

using static System.Windows.Forms.ListBox;

namespace SetupTool
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Start_btn_Click(object sender, EventArgs e)
        {

        }

        private void Exit_btn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Hashtable list = readApplicationList();
            foreach (DictionaryEntry de in list)
            {
                checkedListBoxApps.Items.Add(de.Key.ToString());
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
            checkedListBoxApps.Items.Add(addp.displayName);
        }

        // 
        // Summary:
        //      Reads out the file "applicationList.json"

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

        // 
        // Summary:
        //      Creates the file "applicationList.json" or overwrites an existing one
        // 
        // Parameters:
        //      ht:
        //          The Hashtable object that will be written to "applicationList.json"
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
    }
}
