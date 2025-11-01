using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pattons_Best
{
   public interface IGameCommand
   {
      GamePhase Phase { set; get; }
      GameAction Action { set; get; }
      GameAction ActionDieRoll { set; get; }
      string EventActive { set; get; }
      EnumMainImage MainImage { set; get; }
   }
   //==========================================
   public interface IGameCommands : System.Collections.IEnumerable
   {
      int Count { get; }
      void Add(IGameCommand gc);
      void Insert(int index, IGameCommand gc);
      void Clear();
      bool Contains(IGameCommand gc);
      int IndexOf(IGameCommand gc);
      IGameCommand? GetLast();
      IGameCommand? RemoveAt(int index);
      public IGameCommand? RemoveLast();
      IGameCommand? this[int index] { get; set; }
   }
}
