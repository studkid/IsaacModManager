using Microsoft.WindowsAPICodePack.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace Final {
    internal class Mod : IComparable {
        private String id;
        private String name;
        private String description;
        private String version;
        private Boolean enabled;
        private String path;

        //Default Constructor
        public Mod() {
            this.id = "0";
            this.name = "";
            this.description = "";
            this.version = "0";
            this.enabled = false;
            this.path = "";
        }

        //Constructor reading from xmlNode and boolean
        public Mod(XmlNode xmlNode, Boolean enab, String path) {
            this.id = xmlNode["id"].InnerText;
            this.name = xmlNode["name"].InnerText;
            this.description = xmlNode["description"].InnerText;
            this.version = xmlNode["version"].InnerText;
            this.enabled = enab;
            this.path = path;
        }

        //Properties
        public String Id => id;
        public String Name => name;
        public String Description => description;
        public String Version => version;
        public Boolean Enabled { get => enabled; set => enabled = value; }
        public String Path => path;

        //Get mod object to String
        override
        public String ToString() => name;

        //Get full test String
        public String ToFullString() => $"ID: {id}\nName: {name}\nDescription: {description}\nVersion: {version}\nEnabled: {enabled}";

        //Implementation for IComparable
        public int CompareTo(object obj) {
            if(obj == null) return -1;

            Mod mod = obj as Mod;
            if(mod !=  null) {
                return this.Name.CompareTo(mod.Name);
            }
            else {
                return -1;
            }
        }
    }
}
