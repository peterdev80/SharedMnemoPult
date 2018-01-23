using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Resources;
using System.Windows;
using System.Xml;
using System.Xml.Linq;

namespace VirtualPultValves.DataAccess
{
    public class DataReader
    {
        private Stream GetResourceStream(string resourceFile)
        {
            Uri uri = new Uri(resourceFile, UriKind.RelativeOrAbsolute);

            StreamResourceInfo info = Application.GetResourceStream(uri);
            if (info == null || info.Stream == null)
                throw new ApplicationException("Missing resource file: " + resourceFile);

            return info.Stream;
        }
        public string ReadVHName(string fileName,string element)
        {
            using (Stream stream = GetResourceStream(fileName))
            using (XmlReader xmlRdr = new XmlTextReader(stream))
            {
                return (from varL in XDocument.Load(xmlRdr).Element("root").Elements(element)
                        select (string)varL.Attribute("Name")).ToList<string>()[0];

            }


        }
        public List<string> GetVariableName(string fileName, string TypeVar, string TargetVar)
        {
            using (Stream stream = GetResourceStream(fileName))
            using (XmlReader xmlRdr = new XmlTextReader(stream))
            {
                return (from varL in XDocument.Load(xmlRdr).Element("ROOT").Elements("Values")
                        where (string)varL.Attribute("State") == TargetVar
                        from vark in varL.Elements("Value")
                        where (string)vark.Attribute("Type") == TypeVar
                        select (string)vark.Attribute("VarState")).ToList();


            }

        }
        public Dictionary<string, string> GetCommandName(string fileName)
        {
            using (Stream stream = GetResourceStream(fileName))
            using (XmlReader xmlRdr = new XmlTextReader(stream))
            {
                return (from varL in XDocument.Load(xmlRdr).Element("ROOT").Elements("Values")
                        where (string)varL.Attribute("State") == "Send"
                        from vark in varL.Elements("Value")
                        select new KeyValuePair<string, string>((string)vark.Attribute("VarState"), (string)vark.Attribute("Type"))).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);


            }

        }

        public Dictionary<string, string> GetValvesShowCommand(string filename)
        {
            using (Stream stream = GetResourceStream(filename))
            using (XmlReader xmlRdr = new XmlTextReader(stream))
            {
                var v = (from varL in XDocument.Load(xmlRdr).Element("Values").Elements("Value")
                         where (string)varL.Attribute("Visible") == "True"
                         select new KeyValuePair<string, string>((string)varL.Attribute("ValvesName"), (string)varL.Attribute("Locate"))).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                return v;
            }
        }





     

    }
}
