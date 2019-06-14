using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Xml;

namespace NuGetPackageManager.Controls.Helpers
{
    //useful for extracting control templates from ui elements
    public class XamlExportHelper
    {
        public static string Save(object element)
        {
            // Create an XmlWriter
            StringBuilder sb = new StringBuilder();

            XmlWriterSettings xmlSettings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "    ",
                NewLineOnAttributes = true
            };

            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            XamlWriter.Save(element, writer);

            return sb.ToString();
        }
    }
}
