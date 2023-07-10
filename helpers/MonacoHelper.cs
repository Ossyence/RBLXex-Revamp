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
using System.IO;

namespace RBLXex.Helpers
{
    internal class MonacoHelper
    {
        public static string GetCode(WebBrowser editor)
        {
            return editor.InvokeScript("GetText", new string[0]).ToString();
        }

        public static void SetCode(WebBrowser editor, string content)
        {
            editor.InvokeScript("SetText", new object[] { content });
        }

        public static void AddIntellisense(WebBrowser editor, string method, string kind)
        {
            editor.InvokeScript("AddIntellisense", new object[] { method, kind, method, method });
        }
    }
}
