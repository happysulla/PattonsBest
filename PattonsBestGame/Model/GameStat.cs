using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Pattons_Best
{
   [Serializable]
   public class GameStat
   {
      public int myNumGames = 0;                    // number of games this stat is tracking
      public int myNumWins = 0;                     // number of games won
      public GameStat()
      {
      }
      public void Clear()
      {
         myNumGames = 0;                    // Number of games this stat is tracking
      }
   }
}
