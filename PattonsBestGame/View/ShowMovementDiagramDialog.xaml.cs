
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
using System.Windows.Shapes;

namespace Pattons_Best
{
   public partial class ShowMovementDiagramDialog : Window
   {
      public delegate void EndMovementDialogCallback();
      private EndMovementDialogCallback myCallback;
      public ShowMovementDiagramDialog(EndMovementDialogCallback callback)
      {
         myCallback = callback;
         InitializeComponent();
         Image img = new Image() { Name = "MapBattleMove", HorizontalAlignment = HorizontalAlignment.Center, Source = MapItem.theMapImages.GetBitmapImage("MapBattleMove") };
         myViewBox.Child = img;  
         Grid.SetRow(img, 1);
         Grid.SetColumn(img, 0);
      }
      private void Window_Closed(object sender, EventArgs e)
      {
         myCallback();
      }
   }
}
