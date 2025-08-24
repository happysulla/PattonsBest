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
         switch (hex.ColorAction)
         {
            case ColorActionEnum.CAE_START:
               myTextBlock.Inlines.Add(new Run("Start at "));
               break;
            case ColorActionEnum.CAE_ENTER:
               myTextBlock.Inlines.Add(new Run("Enter at "));
               break;
            case ColorActionEnum.CAE_RETREAT:
               myTextBlock.Inlines.Add(new Run("Retreat to "));
               break;
            case ColorActionEnum.CAE_STOP:
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "EllipseDisplayDialog(): Reached default with ColorAction=" + hex.ColorAction.ToString());
               return;
         }
         myTextBlock.Inlines.Add(new Run(hex.Date));
         myTextBlock.Inlines.Add(new Run(" "));
         myTextBlock.Inlines.Add(new Run(hex.Time));
         myTextBlock.Inlines.Add(new Run("\nGrid: " + hex.TerritoryName));
         myTextBlock.Inlines.Add(new LineBreak());
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
