using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace XmlToCSharp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            txtCSharp.Text = Generate(txtXml.Text);
        }

        public string Generate(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                input = @"<currency>
<currency_code>gbp</currency_code>
<currency_number>826</currency_number>
<currency_pre_symbol>&#163;</currency_pre_symbol>
<currency_post_symbol/>
</currency>";
            }

            JobList = new List<XmlNode>();
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(input);
            StringBuilder sb = new StringBuilder();


            string kk = GenerateOneClass(xml.LastChild);
            sb.AppendLine(kk);

            for (int i = 0; i < JobList.Count; i++)
            {
                sb.AppendLine();
                kk = GenerateOneClass(JobList[i]);
                sb.AppendLine(kk);
            }
            return sb.ToString();
            //input.Dump();
        }

        private HashSet<string> ClassList = new HashSet<string>();
        private List<XmlNode> JobList = new List<XmlNode>();
        private string GenerateOneClass(XmlNode rootNode)
        {
            var className = rootNode.Name;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[Serializable]");
            //sb.AppendLine(string.Format("[SerializeAs(Name = \"{0}\")]", rootNode.Name));
            //sb.AppendLine(string.Format("[XmlRoot(\"{0}\")]", rootNode.Name));

            sb.AppendLine("public class " + CancelUnderLine(className));
            sb.AppendLine("{");
            var childNodes = new List<XmlNode>();
            for (int i = 0; i < rootNode.ChildNodes.Count; i++)
            {
                childNodes.Add(rootNode.ChildNodes[i]);
            }

            string parentName = className;
            var query = from it in childNodes group it by it.Name into g select g;
            foreach (var item in query)
            {
                var node = item.FirstOrDefault();
                sb.AppendLine("");
                //sb.AppendLine(string.Format("[SerializeAs(Name = \"{0}\")]", node.Name));
                //sb.AppendLine(string.Format("[DeserializeAs(Name = \"{0}\")]", node.Name));
                //sb.AppendLine(string.Format("[XmlElement(ElementName =\"{0}\")]", node.Name));
                string propertyName = RemoveParentName(node.Name, parentName);
                propertyName = CancelUnderLine(propertyName);
                string type = "string";
                if (node.ChildNodes.Count > 1)
                {
                    type = propertyName;
                    if (!ClassList.Contains(propertyName))
                    {
                        ClassList.Add(propertyName);
                        JobList.Add(node);
                    }
                }
                if (item.Count() > 1)
                {
                    type = string.Format("List<{0}>", propertyName);
                    propertyName = propertyName + "List";
                }
                sb.AppendLine(string.Format("public {0} {1}", type, propertyName) + " {get;set;}");
            }
            sb.AppendLine("}");
            return sb.ToString();
        }

        public string CancelUnderLine(string input)
        {
            string[] temp = input.Split('_');
            for (int i = 0; i < temp.Length; i++)
            {
                temp[i] = TitleCase(temp[i]);
            }
            return string.Join("", temp);
        }

        public string TitleCase(string input)
        {
            return input.Substring(0, 1).ToUpper() + input.Substring(1);
        }

        private string RemoveParentName(string name, string parent)
        {
            return name;
            ///RestClient does not support deserilazer good
            //   return name.Replace(parent + "_", "");
        }

    }
}
