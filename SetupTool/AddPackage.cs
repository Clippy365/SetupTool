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

        private void Button_OK_Click(object sender, EventArgs e)
        {
            string applicationList = "applicationList.json";
            string fullPath = System.IO.Directory.GetCurrentDirectory() + "\\" + applicationList;
            FileInfo fi = new FileInfo(applicationList);
            if (fi.Exists)
            {
                Hashtable list = JsonConvert.DeserializeObject<Hashtable>(File.ReadAllText(fullPath));
                list.Add(textBox_displayName.Text, textBox_packageName.Text);
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

        private void Button_Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
