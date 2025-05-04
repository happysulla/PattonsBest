using System;

namespace Pattons_Best
{
    public interface IMapPoint
    {
      public static IMapPoint operator +(IMapPoint left, double right)
      {
         left.X += right;
         left.Y += right;
         return left;
      }
      public static IMapPoint operator -(IMapPoint left, double right)
      {
         left.X -= right;
         left.Y -= right;
         return left;
      }
      double X { get; set; }
        double Y { get; set; }
        String ToString();
    }
}
