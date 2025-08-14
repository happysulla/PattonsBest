using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Pattons_Best
{
   public partial class EllipseDisplayDialog : Window
   {
      public EllipseDisplayDialog(EnteredHex hex)
      {
         InitializeComponent();
         //-------------------------------------------------------------
         myTextBlock.Inlines.Add(new Run(hex.Date));
         myTextBlock.Inlines.Add(new Run(" "));
         myTextBlock.Inlines.Add(new Run(hex.Time));
         myTextBlock.Inlines.Add(new Run("Area: " + hex.TerritoryName));
         myTextBlock.Inlines.Add(new LineBreak());
         //-------------------------------------------------------------
         switch (hex.ColorAction)
         {
            case ColorActionEnum.CAE_START:
               myTextBlock.Inlines.Add(new Run("Start location"));
               break;
            case ColorActionEnum.CAE_ENTER:
               myTextBlock.Inlines.Add(new Run("Enter area"));
               break;
            case ColorActionEnum.CAE_STOP:
               myTextBlock.Inlines.Add(new Run("Exit area"));
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "EllipseDisplayDialog(): Reached default with ColorAction=" + hex.ColorAction.ToString());
               return;
         }
         //-------------------------------------------------------------
         //if(ColorActionEnum.CAE_START != hex.ColorAction)
         //{
         //   StringBuilder sb = new StringBuilder();
         //   sb.Append(" occurred at ");
         //   sb.Append(hex.Date);
         //   sb.Append(" ");
         //   sb.Append(hex.Time);
         //   myTextBlock.Inlines.Add(new Run());
         //}
         //-------------------------------------------------------------
      }
   }
}
