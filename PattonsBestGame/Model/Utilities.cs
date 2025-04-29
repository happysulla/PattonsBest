using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using System.Xml;
using System.Data.Common;
using System.Threading;
namespace Pattons_Best
{
   public class Utilities
   {
      private static int NUM_NICK_NAMES = 126;
      public static String[] theNickNames = new string[126]
      { "MOTHER", "LITTLE WHILLIE", "TIN TURTLE", "WHIPPET", "DREADNAUGHT", "FLOATING COFFIN", "BAD BOY", "THE INVINCIBLE", "RONSON", "TOMMY COOKER",
         "GIANT", "CASE CLOSED", "AND JUSTICE FOR ALL", "BIG DOG", "CRASHING THRU EM", "BLACK LABEL", "CORONA", "CHARIOT OF HELL", "CAUSE WE CAN", "CARNAGE", "ALWAYS CRAZY",
         "BETTER RUN AND HIDE", "ACHTONG BABY", "CABALLO LOCO", "ANCIENT ONE", "CLOUD NINE", "BOHICA", "CHRONIC PAIN", "COMPLIMENTS OF USA", "BLIZHITCH", "ASHES TO ASHES", "BRAGGING RIGHTS",
         "BERLIN OR BUST", "BLACK SPOT", "HANNIBAL", "CYCLOPS", "BAD MAMA JAMA", "CROWN ROYAL", "COWBOYS FROM HELL", "BLOOD SHED", "SECOND TO NONE", "CROOKLYN", "BLACK DEATH",
         "A TIME TO KILL", "HOSTILE", "AMERICAN MADE", "CRIMINAL INTENT", "BRING IT ON", "BLOOD LUST", "CRAZY TRAIN", "ACE", "ALL TEXAN", "COLD STEEL", "BATTLEWAGON",
         "BUT MISTER, WHY", "A CAN OF WOOPASS", "BLOOD IN BLOOD OUT", "ABSOLUTE 120 PROOF", "COPPERHEAD ROAD", "BLACK WIDOW", "BOSS HOG", "CAN'T KEEP EM", "CRUEL INTENTIONS", "COLD BEER", "CANCER STICK",
         "AIN'T SKEERED", "BOUNTY HUNTER", "CALIFORNIA DREAMIN", "CHOPA CABANA", "COLOSUS", "ALL C N EYE", "BUFFALO SOLDIERS", "THE BEAST", "AFTERMATH", "APOCALYPSE", "COMBAT TESTED",
         "CRAZY HORSE", "COJO", "A TANK", "BRAVEHEART", "CLASS 6", "HUCKLEBERRY", "CHARLIE MIKE", "BLACK KNIGHT YOU AIN'T", "COMANCHE", "WIDOW MAKER", "AMERICAN BADASS",
         "AMERIICAN HELLRAISERS", "WE BAD", "CAMEL TOE", "COMING HOME SOON", "ABSOLUTE KRIEG", "CHAOTIC", "ASSASSIN", "BALL BUSTERS INC", "AKA GLAIATOR", "BEANER ON BOARD", "CONO",
         "IRON KNIGHTS", "THE END", "BULLDOG", "CAN'T GET RIGHT", "CHOCOLATE CITY", "RENEGADE", "ANOTHER EPISODE", "CONCUSSION", "AGGRAVATED ASSAULT", "ABONDON ALL HOPE", "AMERICAN BAD ASS",
         "DADDY'S BELT", "DIPLOMACY FAILED", "GROUND ZERO", "HOLD MY BEER", "ARMAGEDDON", "ATOMIC BLONDE", "BEATS WALKING", "BARBIE DREAMHOUSE", "BYE FELICIA", "CANDY MAN", "CAPITAL PUNISHMENT",
         "CRUEL INTENTIONS", "DIRTY DEEDS", "MULLIGAN", "STAR LORD", "DROPPED AS A BABY", "CRIPPLING DEPRESSION"};
      public const int NO_RESULT = -100;
      public const int STACK = 3;
      public const double ZOOM = 1.25;
      private const int NUM_RANDOM_GEN = 13;
      //--------------------------------------------
      public static SolidColorBrush theBrushBlood = new SolidColorBrush();
      public static SolidColorBrush theBrushRegion = new SolidColorBrush();
      public static SolidColorBrush theBrushRegionClear = new SolidColorBrush();
      public static SolidColorBrush theBrushControlButton = new SolidColorBrush();
      public static SolidColorBrush theBrushScrollViewerActive = new SolidColorBrush();
      public static SolidColorBrush theBrushScrollViewerInActive = new SolidColorBrush();
      //--------------------------------------------
      public static int MapItemNum { set; get; } = 0;
      //--------------------------------------------
      public static Double ZoomCanvas { get; set; } = 1.5;
      public static Double theMapItemOffset = 20;
      public static Double theMapItemSize = 40;  // size of a MapItem black
      public static int theStackSize = 1000;
      //--------------------------------------------
      static private int theRandomIndex = 0;
      private static readonly Random[] theRandoms = new Random[NUM_RANDOM_GEN]; // default seed is System time 
      static public Random RandomGenerator
      {
         get 
         {
            int newIndex = theRandomIndex + 3;
            if (NUM_RANDOM_GEN <= newIndex)
               newIndex = newIndex - NUM_RANDOM_GEN;
            theRandomIndex = newIndex;
            for (int i = 0; i < 4; i++)
               Thread.Sleep(theRandoms[theRandomIndex].Next(3));
            return theRandoms[theRandomIndex]; 
         }
      }
      static public string GetNickName()
      {
         int random = RandomGenerator.Next(NUM_NICK_NAMES);
         return theNickNames[random];
      }
      static public string GetTime(int hour, int min)
      {
         StringBuilder sb = new StringBuilder();
         if (hour < 10)
            sb.Append('0');
         sb.Append(hour.ToString());
         sb.Append(':');
         if( min < 15 )
            sb.Append('0');
         sb.Append(min.ToString());
         return sb.ToString();
      }
      //--------------------------------------------
      // Utilities Functions
      public static void InitializeRandomNumGenerators()
      {
         theRandoms[0] = new Random(); // default seed is System time 
         for (int i = 1; i < NUM_RANDOM_GEN; i++)
         {
            int seed = 265535;
            for (int j = 0; j < i; j++)
            {
               seed += theRandoms[j].Next(seed);
               if (5 < j)
                  break;
            }
            theRandoms[i] = new Random(seed);
         }
      }
      public static string RemoveSpaces(string aLine)
      {
         string[] aStringArray1 = aLine.Split(new char[] { '"' });
         int length = aStringArray1.Length;
         if (0 == length % 2)
            throw new Exception("Syntax error: Invalid number of quotes");
         for (int i = 0; i < aStringArray1.Length; i += 2)
         {
            string aSubString = "";
            string[] aStringArray2 = aStringArray1[i].Split(new char[] { ' ' });
            foreach (string aString in aStringArray2)
               aSubString += aString;
            aStringArray1[i] = aSubString;
         }
         StringBuilder sb = new StringBuilder();
         foreach (string aString in aStringArray1)
            sb.Append(aString);
         aLine = sb.ToString();
         return aLine;
      }
      public static bool IsSubstring(string parentString, string substring) // Find if S2 is a substring of S1
      {
         int lenParentString = parentString.Length;
         int lenSubstring = substring.Length;
         for (int i = 0; i <= lenParentString - lenSubstring; i++) // A loop to slide pat[] one by one
         {
            int j;
            for (j = 0; j < lenSubstring; j++)  // For current index i, check for pattern match 
               if (parentString[i + j] != substring[j])
                  break;
            if (j == lenSubstring)
               return true;
         }
         return false;
      }
      public static System.Windows.Input.Cursor ConvertToCursor(UIElement control, System.Windows.Point hotSpot)
      {
         //--------------------------------------------
         // convert FrameworkElement to PNG stream
         var pngStream = new MemoryStream();
         control.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
         System.Windows.Rect rect = new System.Windows.Rect(0, 0, control.DesiredSize.Width, control.DesiredSize.Height);
         RenderTargetBitmap rtb = new RenderTargetBitmap((int)control.DesiredSize.Width, (int)control.DesiredSize.Height, 96, 96, PixelFormats.Pbgra32);
         control.Arrange(rect);
         rtb.Render(control);
         //--------------------------------------------
         PngBitmapEncoder png = new PngBitmapEncoder();
         png.Frames.Add(BitmapFrame.Create(rtb));
         png.Save(pngStream);
         //--------------------------------------------
         // write cursor header info
         var cursorStream = new MemoryStream();
         cursorStream.Write(new byte[2] { 0x00, 0x00 }, 0, 2);                               // ICONDIR: Reserved. Must always be 0.
         cursorStream.Write(new byte[2] { 0x02, 0x00 }, 0, 2);                               // ICONDIR: Specifies image type: 1 for icon (.ICO) image, 2 for cursor (.CUR) image. Other values are invalid
         cursorStream.Write(new byte[2] { 0x01, 0x00 }, 0, 2);                               // ICONDIR: Specifies number of images in the file.
         cursorStream.Write(new byte[1] { (byte)control.DesiredSize.Width }, 0, 1);          // ICONDIRENTRY: Specifies image width in pixels. Can be any number between 0 and 255. Value 0 means image width is 256 pixels.
         cursorStream.Write(new byte[1] { (byte)control.DesiredSize.Height }, 0, 1);         // ICONDIRENTRY: Specifies image height in pixels. Can be any number between 0 and 255. Value 0 means image height is 256 pixels.
         cursorStream.Write(new byte[1] { 0x00 }, 0, 1);                                     // ICONDIRENTRY: Specifies number of colors in the color palette. Should be 0 if the image does not use a color palette.
         cursorStream.Write(new byte[1] { 0x00 }, 0, 1);                                     // ICONDIRENTRY: Reserved. Should be 0.
         cursorStream.Write(new byte[2] { (byte)hotSpot.X, 0x00 }, 0, 2);                    // ICONDIRENTRY: Specifies the horizontal coordinates of the hotspot in number of pixels from the left.
         cursorStream.Write(new byte[2] { (byte)hotSpot.Y, 0x00 }, 0, 2);                    // ICONDIRENTRY: Specifies the vertical coordinates of the hotspot in number of pixels from the top.
         cursorStream.Write(new byte[4] {                                                    // ICONDIRENTRY: Specifies the size of the image's data in bytes
                                          (byte)((pngStream.Length & 0x000000FF)),
                                          (byte)((pngStream.Length & 0x0000FF00) >> 0x08),
                                          (byte)((pngStream.Length & 0x00FF0000) >> 0x10),
                                          (byte)((pngStream.Length & 0xFF000000) >> 0x18)
                                       }, 0, 4);
         cursorStream.Write(new byte[4] {                                                    // ICONDIRENTRY: Specifies the offset of BMP or PNG data from the beginning of the ICO/CUR file
                                          (byte)0x16,
                                          (byte)0x00,
                                          (byte)0x00,
                                          (byte)0x00,
                                       }, 0, 4);

         // copy PNG stream to cursor stream
         pngStream.Seek(0, SeekOrigin.Begin);
         pngStream.CopyTo(cursorStream);

         // return cursor stream
         cursorStream.Seek(0, SeekOrigin.Begin);
         return new System.Windows.Input.Cursor(cursorStream);
      }
      public static String Serialize<T>(T t)
      {
         try // XML serializer does not work for Interfaces
         {
            StringWriter sw = new StringWriter();
            XmlWriter xw = XmlWriter.Create(sw);
            new XmlSerializer(typeof(T)).Serialize(xw, t);
            return sw.GetStringBuilder().ToString();
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "Serialize(): e=" + e.ToString());
            return "";
         }
      }
      public static T? Deserialize<T>(String s_xml)
      {
         try // XML serializer does not work for Interfaces
         {
            StringReader reader = new StringReader(s_xml);
            XmlReader xw = XmlReader.Create(reader);
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            object? obj = serializer.Deserialize(xw);  
            if(null == obj)
            {
               var type = typeof(T);
               Logger.Log(LogEnum.LE_ERROR, "Deserialize(): serializer returned null for s=" + s_xml + " T=" + type.ToString());
               return default;
            }
            return (T)obj;
         }
         catch (DirectoryNotFoundException dirException)
         {
            var type = typeof(T);
            Logger.Log(LogEnum.LE_ERROR, "Deserialize(): s=" + s_xml + " T=" + type.ToString() + "\ndirException=" + dirException.ToString());
            return default;
         }
         catch (FileNotFoundException fileException)
         {
            var type = typeof(T);
            Logger.Log(LogEnum.LE_ERROR, "Deserialize(): s=" + s_xml + " T=" + type.ToString() + "\nfileException=" + fileException.ToString());
            return default;
         }
         catch (IOException ioException)
         {
            var type = typeof(T);
            Logger.Log(LogEnum.LE_ERROR, "Deserialize(): s=" + s_xml + " T=" + type.ToString() + "\nioException=" + ioException.ToString());
            return default;
         }
         catch (Exception ex)
         {
            var type = typeof(T);
            Logger.Log(LogEnum.LE_ERROR, "Deserialize(): s=" + s_xml + " T=" + type.ToString() + "\nex=" + ex.ToString());
            return default;
         }
      }
      public static bool IsEnemyUnit(IMapItem mi)
      {
         if (true == mi.Name.Contains("LW"))
            return true;
         else if ((true == mi.Name.Contains("ATG")) || (true == mi.Name.Contains("Pak")))
            return true;
         else if ((true == mi.Name.Contains("SPG")) || (true == mi.Name.Contains("STuGIIIg")))
            return true;
         else if ((true == mi.Name.Contains("TANK")) || (true == mi.Name.Contains("PzVI")))
            return true;
         else if (true == mi.Name.Contains("MG"))
            return true;
         else if (true == mi.Name.Contains("TRUCK"))
            return true;
         else if ((true == mi.Name.Contains("PSW")) || (true == mi.Name.Contains("SPW")))
            return true;
         else if (true == mi.Name.Contains("MARDER"))
            return true;
         else if (true == mi.Name.Contains("PzIV"))
            return true;
         else if (true == mi.Name.Contains("PzV"))
            return true;
         else if ((true == mi.Name.Contains("JdgPzIV")) || (true == mi.Name.Contains("JdgPz38t")))
            return true;
         return false;
      }
   }
}
