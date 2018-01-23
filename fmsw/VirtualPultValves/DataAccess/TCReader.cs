using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Resources;
using System.Windows;
using VirtualPultValves.Model;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Media;

namespace VirtualPultValves.DataAccess
{
    public class TCReader
    {
        private Stream GetResourceStream(string resourceFile)
        {
            Uri uri = new Uri(resourceFile, UriKind.RelativeOrAbsolute);

            StreamResourceInfo info = Application.GetResourceStream(uri);
            if (info == null || info.Stream == null)
                throw new ApplicationException("Missing resource file: " + resourceFile);

            return info.Stream;
        }
        public List<TCGroupModel> CreateGroupModelList(string name)
        {

            List<TCGroupModel> glst = new List<TCGroupModel>();
            using (Stream stream = GetResourceStream("pack://application:,,,/VirtualPultValves;component/Data/XMLDataTC.xml"))
            using (XmlReader xmlRdr = new XmlTextReader(stream))
            {


                var doc = XDocument.Load(xmlRdr);
                var root = doc.Root;
                var Value = root.Elements("XAMLViewTC");

                foreach (var val in Value)
                {
                    if ((string)val.Attribute("Key") == name)
                    {

                        var _XAMLViewTCGroup = val.Elements("XAMLViewTCGroup");

                        foreach (var XAMLViewTCGroup in _XAMLViewTCGroup)
                        {
                            var lstDColorModel = XAMLViewTCGroup.Elements("DColorModel");
                            List<TransporantModel> lst = new List<TransporantModel>();
                            foreach (var DColorModel in lstDColorModel)
                            {
                                TransporantModel TVM = new TransporantModel();
                                TVM.TransporantName = (string)DColorModel.Attribute("TransporantName");
                                if (DColorModel.Attribute("TextSize")!=null)
                                 TVM.TextSize = (int)DColorModel.Attribute("TextSize");
                                TVM.ActiveColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString((string)DColorModel.Attribute("ActiveColor")));// new SolidColorBrush()   DColorModel.Attribute("ActiveColor"));// (Brush)DColorModel.Attribute("ActiveColor");
                                TVM.PassiveColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString((string)DColorModel.Attribute("PassiveColor")));
                                var _st = DColorModel.Attribute("ValTC");

                                if (_st != null)
                                {
                                    TVM.TCModelBinding(_st.Value);

                                 }
                                lst.Add(TVM);

                            }
                            TCGroupModel gvm = new TCGroupModel((string)XAMLViewTCGroup.Attribute("GroupName"), lst);
                            glst.Add(gvm);
                        }

                    }
                }
            }
            return glst;

        }
    }
}
