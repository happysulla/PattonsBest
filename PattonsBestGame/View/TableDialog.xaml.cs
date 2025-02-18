using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Xml;
using Point = System.Windows.Point;

namespace Pattons_Best
{
   public partial class TableDialog : Window
   {
      public bool CtorError { get; } = false;
      private string myKey = "";
      public string Key { get => myKey; }
      private FlowDocument? myFlowDocumentContent = null;
      public FlowDocument? FlowDocumentContent { get => myFlowDocumentContent; }
      public TableDialog(string key, StringReader sr)
      {
         InitializeComponent();
         try
         {
            XmlTextReader xr = new XmlTextReader(sr);
            myFlowDocumentContent = (FlowDocument)XamlReader.Load(xr);
            myFlowDocumentScrollViewer.Document = myFlowDocumentContent;
            myFlowDocumentScrollViewer.HorizontalAlignment = HorizontalAlignment.Center;
            myKey = key;
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, " e=" + e.ToString() + " sr.content=\n" + sr.ToString());
            CtorError = true;
            return;
         }
      }
      //-------------------------------------------------------------------------
      private void ButtonClose_Click(object sender, RoutedEventArgs e)
      {
         Close();
      }
   }
}
