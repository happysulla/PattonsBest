﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Pattons_Best
{
   public class RuleDialogViewer
   {
      public bool CtorError { get; } = false;
      private Dictionary<string, string> myRules = new Dictionary<string, string>();
      public Dictionary<string, string> Rules { get => myRules; }
      private Dictionary<string, string> myTables = new Dictionary<string, string>();
      public Dictionary<string, string> Tables { get => myTables; }
      private Dictionary<string, string> myEvents = new Dictionary<string, string>();
      public Dictionary<string, string> Events { get => myEvents; set => myEvents = value; } // this property is used for unit testing - 05COnfigFileMgrUnitTesting
      private Dictionary<string, TableDialog> myTableDialogs = new Dictionary<string, TableDialog>();
      private Dictionary<string, BannerDialog> myBannerDialogs = new Dictionary<string, BannerDialog>();
      private Dictionary<string, BannerDialog> myEventDialogs = new Dictionary<string, BannerDialog>();
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
      public bool ShowRule(string key)
      {
         try
         {
            BannerDialog dialog = myBannerDialogs[key];
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
               Logger.Log(LogEnum.LE_ERROR, "ShowRule(): myRules=null for key=" + key);
               return false;
            }
            StringBuilder sb = new StringBuilder();
            if( true == BannerDialog.theIsCheckBoxChecked)
               sb.Append(@"<TextBlock xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' Name='myTextBlockDisplay' xml:space='preserve' Width='555' Height='690' FontFamily='Georgia' FontSize='20' TextWrapping='WrapWithOverflow' IsHyphenationEnabled='true' HorizontalAlignment='Left' VerticalAlignment='Top' Margin='15,0,0,0'>");
            else
               sb.Append(@"<TextBlock xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' Name='myTextBlockDisplay' xml:space='preserve' Width='555' Height='690' FontFamily='Old English Text MT' FontSize='20' TextWrapping='WrapWithOverflow' IsHyphenationEnabled='true' HorizontalAlignment='Left' VerticalAlignment='Top' Margin='15,0,0,0'>");
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
            TableDialog dialog = myTableDialogs[key];
            dialog.Activate(); // bring to top
            dialog.Focus();
            return true;
         }
         catch (System.Collections.Generic.KeyNotFoundException e1)
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
            sb.Append(@"<FlowDocument xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' Name='myFlowDocument'>");
            sb.Append(myTables[key]);
            sb.Append(@"</FlowDocument>");
            StringReader sr = new StringReader(sb.ToString());
            TableDialog dialog = new TableDialog(key, sr);
            if (true == dialog.CtorError)
            {
               Logger.Log(LogEnum.LE_ERROR, "ShowTable(): TableDialog() ctor error");
               return false;
            }
            switch (key)
            {
               case "t001":
                  dialog.Title = "Combat Calander";
                  dialog.myFlowDocumentScrollViewer.Width = 1300;
                  dialog.myFlowDocumentScrollViewer.Height = 900;
                  break;
               case "t220":
                  dialog.Title = "r220 Combat Table";
                  dialog.myFlowDocumentScrollViewer.Width = 410;
                  dialog.myFlowDocumentScrollViewer.Height = 410;
                  break;
               case "t226":
                  dialog.Title = "r226 Treasure Table";
                  dialog.myFlowDocumentScrollViewer.Width = 400;
                  dialog.myFlowDocumentScrollViewer.Height = 380;
                  break;
               default:
                  Logger.Log(LogEnum.LE_ERROR, "ShowTable(): reached default key=" + key);
                  return false;
            }
            IEnumerable<Button> buttons = FindButtons(dialog.myFlowDocumentScrollViewer.Document);
            foreach (Button button in buttons)
               button.Click += Button_Click;
            myTableDialogs[key] = dialog;
            dialog.Show();
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
            BannerDialog dialog = myBannerDialogs[key];
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
               sb.Append(@"<TextBlock xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' Name='myTextBlockDisplay' xml:space='preserve' Width='555' Height='690' FontFamily='Old English Text MT' FontSize='20' TextWrapping='WrapWithOverflow' IsHyphenationEnabled='true' HorizontalAlignment='Left' VerticalAlignment='Top' Margin='15,0,0,0'>");
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
                     b.Click += Button_Click1;
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
            ConfigFileReader cfr = new ConfigFileReader(filename);
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
                     if (null == button)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "FindButtons(): button=null");
                     }
                     else
                     {
                        buttons.Add(button);
                     }
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
      private void Button_Click(object sender, RoutedEventArgs e)
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
         else if (true == key.StartsWith("t")) // rules based click
         {
            if (false == ShowTable(key))
            {
               Logger.Log(LogEnum.LE_ERROR, "Button_Click():  ShowTable() returned false");
               return;
            }
         }
         else if (true == key.StartsWith("e")) // rules based click
         {
            if (false == ShowEvent(key))
            {
               Logger.Log(LogEnum.LE_ERROR, "Button_Click():  ShowEvent() returned false");
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
         else if (true == key.StartsWith("t")) // rules based click
         {
            if (false == ShowTable(key))
            {
               Logger.Log(LogEnum.LE_ERROR, "Button_Click():  ShowTable() returned false");
               return;
            }
         }
         else if (true == key.StartsWith("e")) // rules based click
         {
            if (false == ShowEventDialog(key))
            {
               Logger.Log(LogEnum.LE_ERROR, "Button_Click():  ShowEvent() returned false");
               return;
            }
         }
      }
   }
}
