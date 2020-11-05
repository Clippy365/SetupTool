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
using System.Collections;

namespace SetupTool
{
    public partial class AddPackage : Form
    {
        public AddPackage()
        {
            InitializeComponent();
        }

        public string displayName { get { return textBox_displayName.Text; } }
        public string packageName { get { return textBox_packageName.Text; } }

        private void Button_OK_Click(object sender, EventArgs e)
        {
            if (textBox_displayName.Text != "" && textBox_packageName.Text != "")
            {
                string applicationList = "applicationList.json";
                string fullPath = System.IO.Directory.GetCurrentDirectory() + "\\" + applicationList;
                FileInfo fi = new FileInfo(applicationList);
                if (fi.Exists)
                {
                    Hashtable list = JsonConvert.DeserializeObject<Hashtable>(File.ReadAllText(fullPath));
                    
                    //Don't allow duplicates
                    try
                    { list.Add(textBox_displayName.Text, textBox_packageName.Text); }

                    catch(Exception ex)
                    { 
                        MessageBox.Show(ex.Message);
                        return; 
                    }

                    var newJsonObject = JsonConvert.SerializeObject(list, Formatting.Indented);
                    System.IO.File.WriteAllText(@fullPath, newJsonObject);
                    this.Close();
                }

                else
                {
                    Hashtable list = new Hashtable();
                    list.Add(textBox_displayName.Text, textBox_packageName.Text);
                    var JsonObject = JsonConvert.SerializeObject(list, Formatting.Indented);
                    System.IO.File.WriteAllText(@fullPath, JsonObject);
                    this.Close();
                }
            }
            else
                MessageBox.Show("Please fill out both text boxes");
        }

        private void Button_Cancel_Click(object sender, EventArgs e)
        {
            textBox_displayName.Text = "";
            textBox_packageName.Text = "";
            this.Close();
        }
    }
}
