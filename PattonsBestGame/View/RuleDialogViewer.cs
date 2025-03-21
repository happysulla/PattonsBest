using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfAnimatedGif;

namespace Pattons_Best
{
   public class RuleDialogViewer
   {
      private const double theExtraWidth = 0;
      public bool CtorError { get; } = false;
      private Dictionary<string, string> myRules = new Dictionary<string, string>();
      public Dictionary<string, string> Rules { get => myRules; }
      private Dictionary<string, string> myTables = new Dictionary<string, string>();
      public Dictionary<string, string> Tables { get => myTables; }
      private Dictionary<string, string> myEvents = new Dictionary<string, string>();
      public Dictionary<string, string> Events { get => myEvents; set => myEvents = value; } // this property is used for unit testing - 05COnfigFileMgrUnitTesting
      private Dictionary<string, TableDialog?> myTableDialogs = new Dictionary<string, TableDialog?>();
      private Dictionary<string, BannerDialog?> myBannerDialogs = new Dictionary<string, BannerDialog?>();
      private Dictionary<string, BannerDialog?> myEventDialogs = new Dictionary<string, BannerDialog?>();
      private IGameEngine myGameEngine;
      private IGameInstance myGameInstance;
      public IGameInstance GameInstance{ set => myGameInstance = value; } // the game instance changes when a Game is loaded
      //--------------------------------------------------------------------
      public RuleDialogViewer(IGameInstance gi, IGameEngine ge)
      {
         myGameInstance = gi;
         myGameEngine = ge;
         if (false == CreateTables())
         {
            Logger.Log(LogEnum.LE_ERROR, "RuleDialogViewer(): CreateTables() returned false");
            CtorError = true;
            return;
         }
         if (false == CreateRules())
         {
            Logger.Log(LogEnum.LE_ERROR, "RuleDialogViewer(): CreateRules() returned false");
            CtorError = true;
            return;
         }
      }
      public string GetRuleTitle(string key)
      {
         try
         {
            if (null == myRules)
            {
               Logger.Log(LogEnum.LE_ERROR, "GetRuleTitle(): myRules=null for key=" + key);
               return "ERROR";
            }
            string multilineString = myRules[key];
            int indexOfStart = multilineString.IndexOf(key);
            if (-1 == indexOfStart)
            {
               Logger.Log(LogEnum.LE_ERROR, "GetRuleTitle(): IndexOf() returned -1 for key=" + key);
               return "ERROR";
            }
            indexOfStart += key.Length + 1; // add one to get past preceeding space
            string startOfTitle = multilineString.Substring(indexOfStart);
            int indexOfEnd = startOfTitle.IndexOf('<');
            string title = startOfTitle.Substring(0, indexOfEnd);
            return title;
         }
         catch (Exception e2)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetRuleTitle(): e=" + e2.ToString() + " for key=" + key);
            return "ERROR";
         }
      }
      public string GetEventTitle(string key)
      {
         try
         {
            if (null == myEvents)
            {
               Logger.Log(LogEnum.LE_ERROR, "GetEventTitle(): myEvents=null for key=" + key);
               return "ERROR";
            }
            string multilineString = myEvents[key];
            int indexOfStart = multilineString.IndexOf(key);
            if (-1 == indexOfStart)
            {
               Logger.Log(LogEnum.LE_ERROR, "GetEventTitle(): IndexOf() returned -1 for key=" + key);
               return "ERROR";
            }
            indexOfStart += key.Length + 1; // add one to get past preceeding space
            string startOfTitle = multilineString.Substring(indexOfStart);
            int indexOfEnd = startOfTitle.IndexOf('<');
            string title = startOfTitle.Substring(0, indexOfEnd);
            return title;
         }
         catch (Exception e2)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetEventTitle(): e=" + e2.ToString() + " for key=" + key);
            return "ERROR";
         }
      }
      public string GetTableTitle(string key)
      {
         return key;
      }
      public bool ShowRule(string key)
      {
         try
         {
            BannerDialog? dialog = myBannerDialogs[key];
            if (null != dialog)
            {
               if( false == dialog.IsVisible )
                  dialog.Show();
               dialog.Activate(); // bring to top
               dialog.Topmost = true;
               dialog.Topmost = false;
               dialog.Focus();
               return true;
            }
         }
         catch (System.Collections.Generic.KeyNotFoundException)
         {
            // do nothing. This is expected first time dialog is created
         }
         try
         {
            if (null == myRules)
            {
               Logger.Log(LogEnum.LE_ERROR, "ShowRule(): myRules=null for key=" + key);
               return false;
            }
            StringBuilder sb = new StringBuilder();
            if( true == BannerDialog.theIsCheckBoxChecked)
               sb.Append(@"<TextBlock xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' Name='myTextBlockDisplay' xml:space='preserve' Width='555' Height='690' FontFamily='Georgia' FontSize='20' TextWrapping='WrapWithOverflow' IsHyphenationEnabled='true' HorizontalAlignment='Left' VerticalAlignment='Top' Margin='15,0,0,0'>");
            else
               sb.Append(@"<TextBlock xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' Name='myTextBlockDisplay' xml:space='preserve' Width='555' Height='690' FontFamily='Modern 20' FontSize='20' TextWrapping='WrapWithOverflow' IsHyphenationEnabled='true' HorizontalAlignment='Left' VerticalAlignment='Top' Margin='15,0,0,0'>");
            sb.Append(myRules[key]);
            sb.Append(@"</TextBlock>");
            StringReader sr = new StringReader(sb.ToString());
            BannerDialog dialog = new BannerDialog(key, sr);
            if (true == dialog.CtorError)
            {
               Logger.Log(LogEnum.LE_ERROR, "ShowRule(): BannerDialog() returned false");
               return false;
            }
            dialog.Closed += BannerDialog_Closed;
            foreach (Inline inline in dialog.TextBoxDiplay.Inlines)
            {
               if (inline is InlineUIContainer)
               {
                  InlineUIContainer ui = (InlineUIContainer)inline;
                  if (ui.Child is Button b)
                     b.Click += Button_Click;
               }
            }
            myBannerDialogs[key] = dialog;
            dialog.Show();
            return true;
         }
         catch (Exception e2)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowRule(): e=" + e2.ToString() + " for key=" + key);
            return false;
         }
      }
      public bool ShowTable(string key)
      {
         try
         {
            TableDialog? dialog = myTableDialogs[key];
            if (null != dialog)
            {
               dialog.Activate(); // bring to top
               dialog.Focus();
               return true;
            }
         }
         catch (System.Collections.Generic.KeyNotFoundException)
         {
            // do nothing. This is expected first time dialog is created
         }
         try
         {
            if (null == myTables)
            {
               Logger.Log(LogEnum.LE_ERROR, "ShowTable(): myRules=null for key=" + key);
               return false;
            }
            StringBuilder sb = new StringBuilder();
            sb.Append(@"<FlowDocument xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' Name='myFlowDocument' TextAlignment='Center'>");
            sb.Append(myTables[key]);
            sb.Append(@"</FlowDocument>");
            StringReader sr = new StringReader(sb.ToString());
            TableDialog dialog = new TableDialog(key, sr);
            if (true == dialog.CtorError)
            {
               Logger.Log(LogEnum.LE_ERROR, "ShowTable(): TableDialog() ctor error");
               return false;
            }
            dialog.Closed += TableDialog_Closed;
            IEnumerable<Button> buttons = FindButtons(dialog.myFlowDocumentScrollViewer.Document);
            foreach (Button button in buttons)
               button.Click += Button_Click;
            IEnumerable<Image> images = FindImages(dialog.myFlowDocumentScrollViewer.Document);
            foreach (Image iamge in images)
            {
               string fullImagePath = MapImage.theImageDirectory + Utilities.RemoveSpaces(iamge.Name) + ".gif";
               System.Windows.Media.Imaging.BitmapImage bitImage = new BitmapImage();
               bitImage.BeginInit();
               bitImage.UriSource = new Uri(fullImagePath, UriKind.Absolute);
               bitImage.EndInit();
               iamge.Source = bitImage;
            }
            myTableDialogs[key] = dialog;
            dialog.Show();
            dialog.MaxWidth = dialog.MinWidth + theExtraWidth;
            dialog.MaxHeight = dialog.MinHeight + theExtraWidth;
            return true;
         }
         catch (Exception e2)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowTable(): e=" + e2.ToString() + " for key=" + key);
            return false;
         }
      }
      public bool ShowEvent(string key)
      {
         if (true == myGameInstance.IsGridActive)
         {
            if (false == ShowEventDialog(key))
            {
               Logger.Log(LogEnum.LE_ERROR, "ShowEvent():  ShowEventDialog() returned false");
               return false;
            }
         }
         else
         {
            myGameInstance.EventDisplayed = key;
            GameAction action = GameAction.UpdateEventViewerDisplay;
            myGameEngine.PerformAction(ref myGameInstance, ref action);
         }
         return true;
      }
      public bool ShowEventDialog(string key)
      {
         try
         {
            BannerDialog? dialog = myBannerDialogs[key];
            if (null != dialog)
            {
               dialog.Activate(); // bring to top
               dialog.Focus();
               return true;
            }
         }
         catch (System.Collections.Generic.KeyNotFoundException)
         {
            // do nothing. This is expected first time dialog is created
         }
         try
         {
            if (null == myRules)
            {
               Logger.Log(LogEnum.LE_ERROR, "ShowEventDialog(): myRules=null for key=" + key);
               return false;
            }
            StringBuilder sb = new StringBuilder();
            if (true == BannerDialog.theIsCheckBoxChecked)
               sb.Append(@"<TextBlock xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' Name='myTextBlockDisplay' xml:space='preserve' Width='555' Height='690' FontFamily='Georgia' FontSize='20' TextWrapping='WrapWithOverflow' IsHyphenationEnabled='true' HorizontalAlignment='Left' VerticalAlignment='Top' Margin='15,0,0,0'>");
            else
               sb.Append(@"<TextBlock xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' Name='myTextBlockDisplay' xml:space='preserve' Width='555' Height='690' FontFamily='Modern 20' FontSize='20' TextWrapping='WrapWithOverflow' IsHyphenationEnabled='true' HorizontalAlignment='Left' VerticalAlignment='Top' Margin='15,0,0,0'>");
            sb.Append(myEvents[key]);
            sb.Append(@"</TextBlock>");
            StringReader sr = new StringReader(sb.ToString());
            BannerDialog dialog = new BannerDialog(key, sr);
            if (true == dialog.CtorError)
            {
               Logger.Log(LogEnum.LE_ERROR, "ShowEventDialog(): BannerDialog() returned false");
               return false;
            }
            dialog.Closed += EventDialog_Closed;
            foreach (Inline inline in dialog.TextBoxDiplay.Inlines)
            {
               if (inline is InlineUIContainer)
               {
                  InlineUIContainer ui = (InlineUIContainer)inline;
                  if (ui.Child is Button b)
                  {
                     b.Click += Button_Click1;
                  }
                  else if(ui.Child is Image img)
                  {
                     string imageName = img.Name;
                     if (true == img.Name.Contains("Continue"))
                        imageName = "Continue";
                     string fullImagePath = MapImage.theImageDirectory + Utilities.RemoveSpaces(imageName) + ".gif";
                     System.Windows.Media.Imaging.BitmapImage bitImage = new BitmapImage();
                     bitImage.BeginInit();
                     bitImage.UriSource = new Uri(fullImagePath, UriKind.Absolute);
                     bitImage.EndInit();
                     img.Source = bitImage;
                     ImageBehavior.SetAnimatedSource(img, img.Source);
                  }
               }
            }
            myEventDialogs[key] = dialog;
            dialog.Show();
            return true;
         }
         catch (Exception e2)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowRule(): e=" + e2.ToString() + " for key=" + key);
            return false;
         }
      }
      //--------------------------------------------------------------------
      private bool CreateRules()
      {
         try
         {
            string filename = ConfigFileReader.theConfigDirectory + "Rules.txt";
            ConfigFileReader cfr = new ConfigFileReader(filename);
            if (true == cfr.CtorError)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateRules(): cfr.CtorError=true");
               return false;
            }
            myRules = cfr.Entries;
            if (0 == myRules.Count)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateRules(): myRules.Count=0");
               return false;
            }
            return true;
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateRules(): e=" + e.ToString());
            return false;
         }
      }
      private bool CreateTables()
      {
         try
         {
            string filename = ConfigFileReader.theConfigDirectory + "Tables.txt"; 
            ConfigFileReader cfr = new ConfigFileReader(filename); // combat calender is added in this method
            if (true == cfr.CtorError)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateTables(): cfr.CtorError=true");
               return false;
            }
            myTables = cfr.Entries;
            if (0 == myTables.Count)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateTables(): myTables.Count=0");
               return false;
            }
            return true;
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateTables(): e=" + e.ToString());
            return false;
         }
      }
      private IEnumerable<Button> FindButtons(FlowDocument document)
      {
         return document.Blocks.SelectMany(block => FindButtons(block));
      }
      private IEnumerable<Button> FindButtons(Block block)
      {
         if (block is Paragraph)
         {
            List<Button> buttons = new List<Button>();
            var para = ((Paragraph)block).Inlines;
            foreach (var i in para)
            {
               if (i is InlineUIContainer)
               {
                  InlineUIContainer? inlineUiContainer = i as InlineUIContainer;
                  if( null == inlineUiContainer )
                  {
                     Logger.Log(LogEnum.LE_ERROR, "FindButtons(): inlineUiContainer=null");
                  }
                  else
                  {
                     Button? button = inlineUiContainer.Child as Button;
                     if (null != button)
                        buttons.Add(button);
                  }
               }
               else if (i is Figure)
               {
                  Figure? figure = i as Figure;
                  if( null == figure )
                  {
                     Logger.Log(LogEnum.LE_ERROR, "FindButtons(): figure=null");
                  }
                  else
                  {
                     var buttons1 = figure.Blocks.SelectMany(blocks => FindButtons(blocks));
                     buttons.AddRange(buttons1);
                  }
               }
               else if (i is Floater)
               {
                  Floater? floater = i as Floater;
                  if( null != floater)
                  {
                     var buttons2 = floater.Blocks.SelectMany(blocks => FindButtons(blocks));
                     buttons.AddRange(buttons2);
                  }
                  else
                  {
                     Logger.Log(LogEnum.LE_ERROR, "FindButtons(): floater=null");
                  }
               }
            }
            return buttons;
         }
         if (block is Table)
         {
            return ((Table)block).RowGroups.SelectMany(x => x.Rows).SelectMany(x => x.Cells).SelectMany(x => x.Blocks).SelectMany(innerBlock => FindButtons(innerBlock));
         }
         if (block is BlockUIContainer)
         {
            BlockUIContainer blockUIContainer = (BlockUIContainer)block;
            Button? b = blockUIContainer.Child as Button;
            if( null == b )
            {
               Logger.Log(LogEnum.LE_ERROR, "FindButtons(): b=null");
               return new List<Button>();
            }
            else
            {
               return new List<Button>(new[] { b });
            }
         }
         if (block is List)
         {
            return ((List)block).ListItems.SelectMany(listItem => listItem.Blocks.SelectMany(innerBlock => FindButtons(innerBlock)));
         }
         throw new InvalidOperationException("Unknown block type: " + block.GetType());
      }
      private IEnumerable<Image> FindImages(FlowDocument document)
      {
         return document.Blocks.SelectMany(block => FindImages(block));
      }
      private IEnumerable<Image> FindImages(Block block)
      {
         if (block is Paragraph)
         {
            List<Image> images = new List<Image>();
            var para = ((Paragraph)block).Inlines;
            foreach (var i in para)
            {
               if (i is InlineUIContainer)
               {
                  InlineUIContainer? inlineUiContainer = i as InlineUIContainer;
                  if (null == inlineUiContainer)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "FindImages(): inlineUiContainer=null");
                  }
                  else
                  {
                     Image? img = inlineUiContainer.Child as Image;
                     if (null != img)
                        images.Add(img);
                  }
               }
               else if (i is Figure)
               {
                  Figure? figure = i as Figure;
                  if (null == figure)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "FindImages(): figure=null");
                  }
                  else
                  {
                     var img = figure.Blocks.SelectMany(blocks => FindImages(blocks));
                     images.AddRange(img);
                  }
               }
               else if (i is Floater)
               {
                  Floater? floater = i as Floater;
                  if (null != floater)
                  {
                     var img = floater.Blocks.SelectMany(blocks => FindImages(blocks));
                     images.AddRange(img);
                  }
                  else
                  {
                     Logger.Log(LogEnum.LE_ERROR, "FindImages(): floater=null");
                  }
               }
            }
            return images;
         }
         if (block is Table)
         {
            return ((Table)block).RowGroups.SelectMany(x => x.Rows).SelectMany(x => x.Cells).SelectMany(x => x.Blocks).SelectMany(innerBlock => FindImages(innerBlock));
         }
         if (block is BlockUIContainer)
         {
            BlockUIContainer blockUIContainer = (BlockUIContainer)block;
            Image? img = blockUIContainer.Child as Image;
            if (null == img)
            {
               Logger.Log(LogEnum.LE_ERROR, "FindImages(): img=null");
               return new List<Image>();
            }
            else
            {
               return new List<Image>(new[] { img });
            }
         }
         if (block is List)
         {
            return ((List)block).ListItems.SelectMany(listItem => listItem.Blocks.SelectMany(innerBlock => FindImages(innerBlock)));
         }
         throw new InvalidOperationException("Unknown block type: " + block.GetType());
      }
      //--------------------------------------------------------------------
      private void BannerDialog_Closed(object? sender, EventArgs e)
      {
         if (null == sender)
            return;
         BannerDialog dialog = (BannerDialog)sender;
         foreach (Inline inline in dialog.TextBoxDiplay.Inlines)
         {
            if (inline is InlineUIContainer)
            {
               InlineUIContainer ui = (InlineUIContainer)inline;
               if (ui.Child is Button b)
                  b.Click -= Button_Click;
            }
         }
         myBannerDialogs[dialog.Key] = null;
         if (true == dialog.IsReopen)
            this.ShowRule(dialog.Key);
      }
      private void EventDialog_Closed(object? sender, EventArgs e)
      {
         if (null == sender)
            return;
         BannerDialog dialog = (BannerDialog)sender;
         foreach (Inline inline in dialog.TextBoxDiplay.Inlines)
         {
            if (inline is InlineUIContainer)
            {
               InlineUIContainer ui = (InlineUIContainer)inline;
               if (ui.Child is Button b)
                  b.Click -= Button_Click1;
            }
         }
         if (true == dialog.IsReopen)
            this.ShowEventDialog(dialog.Key);
      }
      private void TableDialog_Closed(object? sender, EventArgs e)
      {
         if( null == sender) return;
         TableDialog dialog = (TableDialog)sender;
         myTableDialogs[dialog.Key] = null;
      }
      private void Button_Click(object sender, RoutedEventArgs e)
      {
         Button b = (Button)sender;
         string key = (string)b.Content;
         if (true == key.StartsWith("A")) // rules based click
         {
            IAfterActionReport? aar = myGameInstance.Reports.GetLast();
            if (null == aar)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateView():  gi.Reports.GetLast()=null");
               return;
            }
            AfterActionReportUserControl aarUserControl = new AfterActionReportUserControl(aar);
            if (true == aarUserControl.CtorError)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateView(): AfterActionReportUserControl CtorError=true");
               return;
            }
            AfterActionDialog dialogAAR = new AfterActionDialog(aar);
            dialogAAR.Show();
         }
         else if (true == key.StartsWith("r")) // rules based click
         {
            if (false == ShowRule(key))
            {
               Logger.Log(LogEnum.LE_ERROR, "Button_Click(): ShowRule() returned false");
               return;
            }
         }
         else if (true == key.StartsWith("e")) // event based click
         {
            if (false == ShowEvent(key))
            {
               Logger.Log(LogEnum.LE_ERROR, "Button_Click():  ShowEvent() returned false");
               return;
            }
         }
         else  // table based click
         {
            if (false == ShowTable(key))
            {
               Logger.Log(LogEnum.LE_ERROR, "Button_Click():  ShowTable() returned false");
               return;
            }
         }
      }
      private void Button_Click1(object sender, RoutedEventArgs e)
      {
         Button b = (Button)sender;
         string key = (string)b.Content;
         if (true == key.StartsWith("r")) // rules based click
         {
            if (false == ShowRule(key))
            {
               Logger.Log(LogEnum.LE_ERROR, "Button_Click(): ShowRule() returned false");
               return;
            }
         }
         else if (true == key.StartsWith("e")) // event based click
         {
            if (false == ShowEventDialog(key))
            {
               Logger.Log(LogEnum.LE_ERROR, "Button_Click():  ShowEvent() returned false");
               return;
            }
         }
         else  // table based click
         {
            if (false == ShowTable(key))
            {
               Logger.Log(LogEnum.LE_ERROR, "Button_Click():  ShowTable() returned false");
               return;
            }
         }

      }
   }
}
