
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
using WpfAnimatedGif;

namespace Pattons_Best
{
   public partial class EventViewerCrewSetup : UserControl
   {
      public delegate bool EndCrewMgrCallback();
      private const int MAX_GRID_LEN = 5;
      private const int STARTING_ASSIGNED_ROW = 8;
      private const bool IS_ENABLE = true;
      private const bool NO_ENABLE = false;
      private const bool IS_STATS = true;
      private const bool NO_STATS = false;
      private const bool NO_ADORN = false;
      private const bool IS_CURSOR = true;
      private const bool NO_CURSOR = false;
      public enum E071Enum
      {
         ROLL_RATING,
         ROLL_RATING_AUTO,
         ASSIGN_CREWMEN,
         SHOW_RESULTS,
         END
      };
      public bool CtorError { get; } = false;
      private EndCrewMgrCallback? myCallback = null;
      private E071Enum myState = E071Enum.ROLL_RATING;
      private int myMaxRowCount = 5;
      private int myRollResultRowNum = 0;
      private bool myIsRollInProgress = false;
      //---------------------------------------------------
      private class GridRow
      {
         public ICrewMember? myCrewMember;
         public int myDieRoll;
         public GridRow()
         {
            myCrewMember = null;
            myDieRoll = Utilities.NO_RESULT;
         }
      };
      private GridRow[] myGridRows = new GridRow[MAX_GRID_LEN]; // five possible crew members
      //---------------------------------------------------
      private IGameInstance? myGameInstance;
      private readonly Canvas? myCanvas;
      private readonly ScrollViewer? myScrollViewer;
      private RuleDialogViewer? myRulesMgr;
      private IDieRoller? myDieRoller;
      //---------------------------------------------------
      private IMapItems myAssignables = new MapItems();    // listing of new crewmen 
      private ICrewMember? myCrewMemberDragged = null;
      //---------------------------------------------------
      private readonly Dictionary<string, Cursor> myCursors = new Dictionary<string, Cursor>();
      private readonly DoubleCollection myDashArray = new DoubleCollection();
      private readonly SolidColorBrush mySolidColorBrushBlack = new SolidColorBrush() { Color = Colors.Black };
      private readonly FontFamily myFontFam = new FontFamily("Tahoma");
      //-------------------------------------------------------------------------------------
      public EventViewerCrewSetup(IGameInstance? gi, Canvas? c, ScrollViewer? sv, RuleDialogViewer? rdv, IDieRoller dr)
      {
         InitializeComponent();
         //--------------------------------------------------
         if (null == gi) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerCrewSetup(): gi=null");
            CtorError = true;
            return;
         }
         myGameInstance = gi;
         //--------------------------------------------------
         if (null == c) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerCrewSetup(): c=null");
            CtorError = true;
            return;
         }
         myCanvas = c;
         //--------------------------------------------------
         if (null == sv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerCrewSetup(): sv=null");
            CtorError = true;
            return;
         }
         myScrollViewer = sv;
         //--------------------------------------------------
         if (null == rdv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerCrewSetup(): rdv=null");
            CtorError = true;
            return;
         }
         myRulesMgr = rdv;
         //--------------------------------------------------
         if (null == dr)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerCrewSetup(): dr=true");
            CtorError = true;
            return;
         }
         myDieRoller = dr;
         //--------------------------------------------------
         myDashArray.Add(4);  // used for dotted lines
         myDashArray.Add(2);  // used for dotted lines
         myGrid.MouseDown += Grid_MouseDown;
      }
      public bool AssignNewCrewRatings(EndCrewMgrCallback callback)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "AssignNewCrewRatings(): myGameInstance=null");
            return false;
         }
         if (null == myCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "AssignNewCrewRatings(): myCanvas=null");
            return false;
         }
         if (null == myScrollViewer)
         {
            Logger.Log(LogEnum.LE_ERROR, "AssignNewCrewRatings(): myScrollViewer=null");
            return false;
         }
         if (null == myRulesMgr)
         {
            Logger.Log(LogEnum.LE_ERROR, "AssignNewCrewRatings(): myRulesMgr=null");
            return false;
         }
         if (null == myDieRoller)
         {
            Logger.Log(LogEnum.LE_ERROR, "AssignNewCrewRatings(): myDieRoller=null");
            return false;
         }
         //--------------------------------------------------
         myGridRows = new GridRow[MAX_GRID_LEN];
         myState = E071Enum.ROLL_RATING;
         Option option = myGameInstance.Options.Find("AutoRollNewMembers");
         if (true == option.IsEnabled)
            myState = E071Enum.ROLL_RATING_AUTO;
         //--------------------------------------------------
         myMaxRowCount = myGameInstance.NewMembers.Count;
         myIsRollInProgress = false;
         myRollResultRowNum = 0;
         myCallback = callback;
         System.Windows.Point hotPoint = new System.Windows.Point(Utilities.theMapItemOffset, Utilities.theMapItemOffset); // set the center of the MapItem as the hot point for the cursor
         myCursors.Clear();
         int i = 0;
         foreach (ICrewMember cm in myGameInstance.NewMembers) // AssignNewCrewRatings()
         {
            myAssignables.Add(cm);
            myGridRows[i] = new GridRow();
            Button b = CreateButton(cm, IS_ENABLE, false, NO_STATS, NO_ADORN, IS_CURSOR);
            myCursors[cm.Name] = Utilities.ConvertToCursor(b, hotPoint);
            ++i;
         }
         //--------------------------------------------------
         if (false == UpdateGrid())
         {
            Logger.Log(LogEnum.LE_ERROR, "AssignNewCrewRatings(): UpdateGrid() return false");
            return false;
         }
         myScrollViewer.Content = myGrid;
         return true;
      }
      private bool UpdateGrid()
      {
         if (false == UpdateEndState())
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateEndState() returned false");
            return false;
         }
         if (E071Enum.END == myState)
            return true;
         if (false == UpdateUserInstructions())
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateUserInstructions() returned false");
            return false;
         }
         if (false == UpdateAssignablePanel())
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateAssignablePanel() returned false");
            return false;
         }
         if (false == UpdateCheckBoxPanel())
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGrid():  UpdateCheckBoxPanel() returned false");
            return false;
         }
         if (false == UpdateGridRows())
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateGridRows() returned false");
            return false;
         }
         return true;
      }
      private bool UpdateEndState()
      {
         if (E071Enum.END == myState)
         {
            for (int i = 0; i < myMaxRowCount; i++)
            {
               ICrewMember? crewMember = myGridRows[i].myCrewMember as ICrewMember;
               if( null == crewMember )
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateEndState(): crewMember=null");
                  return false;
               }
               crewMember.Rating = (int) Math.Ceiling((double)(myGridRows[i].myDieRoll)/2.0);
            }
            //----------------------------------
            if (null == myGameInstance)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEndState(): myGameInstance=null");
               return false;
            }
            myGameInstance.NewMembers.Clear();
            Logger.Log(LogEnum.LE_SHOW_CREW_CLEAR, "EventViewerCrewSetup.UpdateEndState(): clearing NewMebers");
            //----------------------------------
            if (null == myCallback)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEndState(): myCallback=null");
               return false;
            }
            if (false == myCallback())
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEndState(): myCallback() returned false");
               return false;
            }
         }
         return true;
      }
      private bool UpdateUserInstructions()
      {
         myTextBlockInstructions.Inlines.Clear();
         switch (myState)
         {
            case E071Enum.ROLL_RATING:
               myTextBlockInstructions.Inlines.Add(new Run("Roll all dice for all rows."));
               break;
            case E071Enum.ROLL_RATING_AUTO:
               myTextBlockInstructions.Inlines.Add(new Run("Roll one die and auto-roll all others."));
               break;
            case E071Enum.ASSIGN_CREWMEN:
               myTextBlockInstructions.Inlines.Add(new Run("Click and drag crew members to rectangles."));
               break;
            case E071Enum.SHOW_RESULTS:
               myTextBlockInstructions.Inlines.Add(new Run("Click image to continue."));
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateUserInstructions(): reached default state=" + myState.ToString());
               return false;
         }
         return true;
      }
      private bool UpdateAssignablePanel()
      {
         myStackPanelAssignable.Children.Clear(); // clear out assignable panel 
         switch (myState)
         {
            case E071Enum.ROLL_RATING:
            case E071Enum.ROLL_RATING_AUTO:
               foreach (ICrewMember cm in myAssignables)
               {
                  Button b = CreateButton(cm, NO_ENABLE, false, IS_STATS, NO_ADORN, NO_CURSOR);
                  myStackPanelAssignable.Children.Add(b);
               }
               break;
            case E071Enum.ASSIGN_CREWMEN:
               foreach (ICrewMember cm in myAssignables)
               {
                  bool isRectangleBorderAdded = false; // If dragging a map item, show rectangle around that MapItem
                  if (null != myCrewMemberDragged && cm.Name == myCrewMemberDragged.Name)
                     isRectangleBorderAdded = true;
                  Button b = CreateButton(cm, IS_ENABLE, isRectangleBorderAdded, IS_STATS, NO_ADORN, NO_CURSOR);
                  myStackPanelAssignable.Children.Add(b);
               }
               break;
            case E071Enum.SHOW_RESULTS:
               Image img1 = new Image { Name = "Continue", Source = MapItem.theMapImages.GetBitmapImage("Continue"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img1);
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateAssignablePanel(): reached default s=" + myState.ToString());
               return false;
         }
         return true;
      }
      private bool UpdateCheckBoxPanel()
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateCheckBoxPanel(): myGameInstance=null");
            return false;
         }
         myStackPanelCheckMarks.Children.Clear();
         CheckBox cb = new CheckBox() { FontSize = 12, IsEnabled = false, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
         cb.Content = "Click to roll for one member and auto-roll rest";
         Option option = myGameInstance.Options.Find("AutoRollNewMembers");
         cb.IsChecked = option.IsEnabled;
         //------------------------------------
         bool isRollMade = false;
         for(int i=0; i < myMaxRowCount; ++i )
         {
            GridRow row = myGridRows[i];
            if (Utilities.NO_RESULT < row.myDieRoll)
               isRollMade = true;
         }
         if( false == isRollMade ) // if any roll is made, user cannot change state of checkbox
         {
            cb.Checked += CheckBox_Checked;
            cb.Unchecked += CheckBox_Unchecked;
            cb.IsEnabled = true;
         }
         myStackPanelCheckMarks.Children.Add(cb);
         return true;
      }
      private bool UpdateGridRows()
      {
         //------------------------------------------------------------
         // Clear out existing Grid Row data
         List<UIElement> results = new List<UIElement>();
         foreach (UIElement ui in myGrid.Children)
         {
            int rowNum = Grid.GetRow(ui);
            if (STARTING_ASSIGNED_ROW <= rowNum)
               results.Add(ui);
         }
         foreach (UIElement ui1 in results)
            myGrid.Children.Remove(ui1);
         //------------------------------------------------------------
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            int rowNum = i + STARTING_ASSIGNED_ROW;
            GridRow row = myGridRows[i];
            //------------------------------------
            if (null == row.myCrewMember)
            {
               Rectangle r = new Rectangle()
               {
                  Visibility = Visibility.Visible,
                  Stroke = mySolidColorBrushBlack,
                  Fill = Brushes.Transparent,
                  StrokeThickness = 2.0,
                  StrokeDashArray = myDashArray,
                  Width = Utilities.ZOOM * Utilities.theMapItemSize,
                  Height = Utilities.ZOOM * Utilities.theMapItemSize
               };
               myGrid.Children.Add(r);
               Grid.SetRow(r, rowNum);
               Grid.SetColumn(r, 0);
            }
            else
            {
               Button b = CreateButton(row.myCrewMember, IS_ENABLE, false, IS_STATS, NO_ADORN, NO_CURSOR);
               myGrid.Children.Add(b);
               Grid.SetRow(b, rowNum);
               Grid.SetColumn(b, 0);
            }
            //------------------------------------------
            if (Utilities.NO_RESULT == row.myDieRoll)
            {
               BitmapImage bmi = new BitmapImage();
               bmi.BeginInit();
               bmi.UriSource = new Uri(MapImage.theImageDirectory + "DieRollWhite.gif", UriKind.Absolute);
               bmi.EndInit();
               Image img = new Image { Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
               ImageBehavior.SetAnimatedSource(img, bmi);
               myGrid.Children.Add(img);
               Grid.SetRow(img, rowNum);
               Grid.SetColumn(img, 1);
            }
            else
            {
               string dieRollLabel = myGridRows[i].myDieRoll.ToString();
               Label label = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = dieRollLabel };
               myGrid.Children.Add(label);
               Grid.SetRow(label, rowNum);
               Grid.SetColumn(label, 1);
               //-------------------------------
               int rating = (int)Math.Ceiling((double)row.myDieRoll / 2.0);
               Label labelResult = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = rating.ToString() };
               myGrid.Children.Add(labelResult);
               Grid.SetRow(labelResult, rowNum);
               Grid.SetColumn(labelResult, 2);
            }
         }
         return true;
      }
      //------------------------------------------------------------------------------------
      private Button CreateButton(ICrewMember cm, bool isEnabled, bool isRectangleAdded, bool isStatsShown, bool isAdornmentsShown, bool isCursor)
      {
         System.Windows.Controls.Button b = new System.Windows.Controls.Button { };
         b.Name = cm.Role;
         if (true == isCursor)
         {
            b.Width = Utilities.theMapItemSize;
            b.Height = Utilities.theMapItemSize;
         }
         else
         {
            b.Width = Utilities.ZOOM * Utilities.theMapItemSize;
            b.Height = Utilities.ZOOM * Utilities.theMapItemSize;
         }
         if (false == isRectangleAdded)
         {
            b.BorderThickness = new Thickness(0);
         }
         else
         {
            b.BorderThickness = new Thickness(1);
            b.BorderBrush = Brushes.Black;
         }
         b.Background = new SolidColorBrush(Colors.Transparent);
         b.Foreground = new SolidColorBrush(Colors.Transparent);
         if (true == isEnabled)
         {
            b.IsEnabled = isEnabled;
            b.Click += this.Button_Click;
         }
         CrewMember.SetButtonContent(b, cm); // This sets the image as the button's content
         return b;
      }
      public void ShowDieResults(int dieRoll)
      {
         int i = myRollResultRowNum - STARTING_ASSIGNED_ROW;
         if (i < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowCombatResults(): 0 > i=" + i.ToString());
            return;
         }
         myGridRows[i].myDieRoll = dieRoll;
         //------------------------------------

         if (E071Enum.ROLL_RATING_AUTO == myState )
         {
            myState = E071Enum.ASSIGN_CREWMEN;
            for (int j = 0; j < myMaxRowCount; ++j)
            {
               if (Utilities.NO_RESULT == myGridRows[j].myDieRoll)
               {
                  myGridRows[j].myDieRoll = Utilities.RandomGenerator.Next(10);
                  if (0 == myGridRows[j].myDieRoll)
                     myGridRows[j].myDieRoll = 10;
               }
            }
         }
         else
         {
            myState = E071Enum.ASSIGN_CREWMEN;
            for (int j = 0; j < myMaxRowCount; ++j)
            {
               if (Utilities.NO_RESULT == myGridRows[j].myDieRoll)
                  myState = E071Enum.ROLL_RATING;
            }
         }
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): UpdateGrid() return false");
         myIsRollInProgress = false;
      }
      //---------------------Controller Function--------------------------------------------
      private void ButtonRule_Click(object sender, RoutedEventArgs e)
      {
         if (null == myRulesMgr)
         {
            Logger.Log(LogEnum.LE_ERROR, "ButtonRule_Click(): myRulesMgr=null");
            return;
         }
         Button b = (Button)sender;
         string key = (string)b.Content;
         if (false == myRulesMgr.ShowRule(key))
            Logger.Log(LogEnum.LE_ERROR, "ButtonRule_Click(): myRulesMgr.ShowRule() returned false key=" + key);
      }
      private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): myGameInstance=null");
            return;
         }
         if (null == myCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): myCanvas=null");
            return;
         }
         if (null == myScrollViewer)
         {
            Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): myScrollViewer=null");
            return;
         }
         if (null == myRulesMgr)
         {
            Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): myRulesMgr=null");
            return;
         }
         if (null == myDieRoller)
         {
            Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): myDieRoller=null");
            return;
         }
         //--------------------------------------------------
         System.Windows.Point p = e.GetPosition((UIElement)sender);
         HitTestResult result = VisualTreeHelper.HitTest(myGrid, p);  // Get the Point where the hit test occurrs
         foreach (UIElement ui in myGrid.Children)
         {
            if (null != myCrewMemberDragged) // If dragging something, check if dragged to rectangle either in StackPanel or GridRow
            {
               if (ui is StackPanel panel)  // First check all rectangles in the myStackPanelAssignable
               {
                  foreach (UIElement ui1 in panel.Children)
                  {
                     if (ui1 is Rectangle rect)
                     {
                        if (result.VisualHit == rect)
                        {
                           myAssignables.Add(myCrewMemberDragged);
                           myGrid.Cursor = Cursors.Arrow;
                           myCrewMemberDragged = null;
                           if (false == UpdateGrid())
                              Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
                           return;
                        }
                     }
                  }
               }
               else if (ui is Rectangle rect) // next check all rectangles in the grid rows
               {
                  if (result.VisualHit == rect)
                  {
                     myGrid.Cursor = Cursors.Arrow;
                     int rowNum = Grid.GetRow(rect);
                     int i = rowNum - STARTING_ASSIGNED_ROW;
                     myAssignables.Remove(myCrewMemberDragged);
                     myGridRows[i].myCrewMember = myCrewMemberDragged;
                     myCrewMemberDragged = null;
                     //--------------------------------------------------
                     if (E071Enum.ASSIGN_CREWMEN == myState) // if any crewman is not assigned, continue in same state
                     {
                        myState = E071Enum.SHOW_RESULTS;
                        for (int j = 0; j < myMaxRowCount; ++j)
                        {
                           if (null == myGridRows[j].myCrewMember)
                              myState = E071Enum.ASSIGN_CREWMEN;
                        }
                     }
                     //--------------------------------------------------
                     if (false == UpdateGrid())
                        Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
                     return;
                  }
               }
            } // end if (null != myCrewMemberDragged)
            else //----------------------NOT DRAGGING-------------------------------------
            {
               if (ui is StackPanel panel)
               {
                  foreach (UIElement ui1 in panel.Children)
                  {
                     if (ui1 is Image img) // Check all images within the myStackPanelAssignable
                     {
                        if (result.VisualHit == img)
                        {
                           if ("Continue" == img.Name)
                              myState = E071Enum.END;
                           if ("DieRoll" == img.Name)
                           {
                              if (false == myIsRollInProgress)
                              {
                                 myIsRollInProgress = true;
                                 RollEndCallback callback = ShowDieResults;
                                 myDieRoller.RollMovingDie(myCanvas, callback);
                                 img.Visibility = Visibility.Hidden;
                              }
                              return;
                           }
                           if (false == UpdateGrid())
                              Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
                           return;
                        }
                     }
                  }
               }
               if (ui is Image img1) // next check all images within the Grid Rows
               {
                  if (result.VisualHit == img1)
                  {
                     if (false == myIsRollInProgress)
                     {
                        myRollResultRowNum = Grid.GetRow(img1);
                        myIsRollInProgress = true;
                        RollEndCallback callback = ShowDieResults;
                        myDieRoller.RollMovingDie(myCanvas, callback);
                        img1.Visibility = Visibility.Hidden;
                     }
                     return;
                  }
               }
            }
         }
      }
      private void Button_Click(object sender, RoutedEventArgs e)
      {
         Button b = (Button)sender;
         int rowNum = Grid.GetRow(b);
         if (null != myCrewMemberDragged) 
         {
            ICrewMember? crewMember = (ICrewMember)myCrewMemberDragged;
            if (null == crewMember)
            {
               Logger.Log(LogEnum.LE_ERROR, "Button_Click(): crewMember=null");
               return;
            }
            if (crewMember.Role != b.Name) // dropping on another button
            {
               if (STARTING_ASSIGNED_ROW <= rowNum) // only support dropping on grid row - all other drops just disable the move
               {
                  int i = rowNum - STARTING_ASSIGNED_ROW;
                  #pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                    ICrewMember cm = myGridRows[i].myCrewMember;
                  #pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                  if ( null == cm)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "Button_Click(): myGridRows[i].myMapItem=null for b.Name=" + b.Name);
                     return;
                  }
                  myAssignables.Add(cm);   // add existing one back to myAssignables
                  myAssignables.Remove(myCrewMemberDragged); // Remove dragged map item from myAssignables
                  myGridRows[i].myCrewMember = myCrewMemberDragged;   // grid row updated
               }
            }
            myCrewMemberDragged = null;
            myGrid.Cursor = Cursors.Arrow;
         }
         else
         {
            foreach(ICrewMember cm in myAssignables)
            {
               if( cm.Role == b.Name )
                  myCrewMemberDragged = cm;
            }
            if (null == myCrewMemberDragged) // When finish dragging all Crewmembers, cannot drag anymore
               return;
            myGrid.Cursor = myCursors[myCrewMemberDragged.Name]; // change cursor of button being dragged
            if (STARTING_ASSIGNED_ROW <= rowNum)
            {
               int i = rowNum - STARTING_ASSIGNED_ROW;
               myGridRows[i].myCrewMember = null;
            }
            else
            {
               myAssignables.Remove(b.Name);
            }
         }
         //--------------------------------------------------
         if (E071Enum.ASSIGN_CREWMEN == myState) // if any crewman is not assigned, continue in same state
         {
            myState = E071Enum.SHOW_RESULTS;
            for (int j = 0; j < myMaxRowCount; ++j)
            {
               if (null == myGridRows[j].myCrewMember)
                  myState = E071Enum.ASSIGN_CREWMEN;
            }
         }
         //--------------------------------------------------
         if (false == UpdateGrid())
         {
            Logger.Log(LogEnum.LE_ERROR, "Button_Click(): UpdateGrid() return false");
            return;
         }
      }
      private void CheckBox_Checked(object sender, RoutedEventArgs e)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckBox_Checked(): myGameInstance=null");
            return;
         }
         //---------------------------
         CheckBox cb = (CheckBox)sender;
         cb.IsChecked = true;
         Option option = myGameInstance.Options.Find("AutoRollNewMembers");
         option.IsEnabled = true;
         myState = E071Enum.ROLL_RATING_AUTO;
         //---------------------------
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "CheckBox_Checked(): UpdateGrid() return false");
      }
      private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckBox_Unchecked(): myGameInstance=null");
            return;
         }
         //---------------------------
         CheckBox cb = (CheckBox)sender;
         cb.IsChecked = false;
         Option option = myGameInstance.Options.Find("AutoRollNewMembers");
         option.IsEnabled = false;
         myState = E071Enum.ROLL_RATING;
         //---------------------------
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "CheckBox_Unchecked(): UpdateGrid() return false");
      }
   }
}
