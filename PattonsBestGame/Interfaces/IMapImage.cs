using System.Windows.Controls;
using System.Windows.Media.Imaging;
using WpfAnimatedGif;

namespace Pattons_Best
{
   public interface IMapImage
   {
      string Name { get; set; }
      bool IsAnimated { get; set; }
      Image ImageControl { get; set; }
      ImageAnimationController? AnimationController { get; set; }
   }
   public interface IMapImages : System.Collections.IEnumerable
   {
      int Count { get; }
      void Add(IMapImage mii);
      void Insert(int index, IMapImage mii);
      void Clear();
      bool Contains(IMapImage mii);
      int IndexOf(IMapImage mii);
      void Remove(IMapImage mii);
      IMapImage? RemoveAt(int index);
      IMapImage? this[int index] { get; set; }
      IMapImage? Find(string pathToMatch);
      BitmapImage? GetBitmapImage(string name);
   }
}
