using System.Collections;
using System.Text;

namespace Pattons_Best
{
   public class GameCommand : IGameCommand
   {
      public GameAction Action { set; get; } = GameAction.Error;
      public GameAction ActionDieRoll { set; get; } = GameAction.Error;
      public string EventActive { set; get; } = "";
      public GameCommand() { }
      public GameCommand(GameAction action, GameAction drAction, string evt )
      {
         EventActive = evt;
         Action = action;
         ActionDieRoll = drAction;
      }
   }
   public class GameCommands : IEnumerable, IGameCommands
   {
      private readonly ArrayList myList;
      public GameCommands() { myList = new ArrayList(); }
      public void Add(IGameCommand gc) { myList.Add(gc); }
      public void Insert(int index, IGameCommand gc) { myList.Insert(index, gc); }
      public int Count { get { return myList.Count; } }
      public void Clear() { myList.Clear(); }
      public bool Contains(IGameCommand gc) { return myList.Contains(gc); }
      public IEnumerator GetEnumerator() { return myList.GetEnumerator(); }
      public int IndexOf(IGameCommand gc) { return myList.IndexOf(gc); }
      public IGameCommand? RemoveAt(int index)
      {
         IGameCommand? gc = myList[index] as IGameCommand;
         if (null == gc)
            return null;
         myList.RemoveAt(index);
         return gc;
      }
      public IGameCommand? this[int index]
      {
         get
         {
            IGameCommand? gc = myList[index] as IGameCommand;
            return gc;
         }
         set { myList[index] = value; }
      }
      public override string ToString()
      {
         StringBuilder sb = new StringBuilder();
         sb.Append("[ ");
         foreach (object o in myList)
         {
            IGameCommand gc = (IGameCommand)o;
            sb.Append(" a=");
            sb.Append(gc.Action.ToString());
            sb.Append(" dra=");
            sb.Append(gc.ActionDieRoll.ToString());
            sb.Append(" e=");
            sb.Append(gc.EventActive);
         }
         sb.Append("]");
         return sb.ToString();
      }
   }
}
