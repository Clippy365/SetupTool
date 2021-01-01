using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Win32;

namespace SetupTool
{
    /// <summary>
    /// This class represents a registry value. It can be used for example to store information of recorded registry settings and apply them to a Windows machine.
    /// </summary>
    class RegistryValue
    {
        private string root;
        private string subKey;
        private int length;
        private RegistryValueKind registryValueKind;
        private object data;

        RegistryValue(string root, string subKey, RegistryValueKind registryValueKind, object data, int length = -1)
        {
            this.root = root;
            this.subKey = subKey;
            this.registryValueKind = registryValueKind;
            this.data = data;

            if (registryValueKind == RegistryValueKind.DWord)
                this.length = 4;

            else if (registryValueKind == RegistryValueKind.QWord)
                this.length = 8;

            else
                this.length = length;
        }
    }
}
