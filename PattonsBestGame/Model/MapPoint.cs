using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Forms;
namespace Pattons_Best
{
   [Serializable]
   public class MapPoint : IMapPoint
   {
      private double myX = 0.0; public double X { get => myX; set => myX = value; }
      private double myY = 0.0; public double Y { get => myY; set => myY = value; }
      public MapPoint() { }
      public MapPoint(double x, double y) { myX = x; myY = y; }
      public override string ToString()
      {
         StringBuilder sb = new StringBuilder("(");
         sb.Append(myX.ToString("####."));
         sb.Append(",");
         sb.Append(myY.ToString("####."));
         sb.Append(")");
         return sb.ToString();
      }
   }
}
