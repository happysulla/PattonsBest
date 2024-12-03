﻿using System;
using System.Collections;
using System.Data.Common;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using WpfAnimatedGif;

namespace Pattons_Best
{
   [Serializable]
   public class MapImage : IMapImage
   {
      public static string theImageDirectory = "";      
      public System.Windows.Media.Imaging.BitmapImage? myBitmapImage = null;
      public string Name { get; set; } = "";
      public bool IsAnimated { get; set; } = false;
      public Image? ImageControl { get; set; } = null;
      public ImageAnimationController AnimationController { get; set; }
      //--------------------------------------------
      public MapImage()
      {
      }
      public MapImage(string imageName)
      {
         string fullImagePath = MapImage.theImageDirectory + Utilities.RemoveSpaces(imageName) + ".gif";
         try
         {
            Name = imageName;
            myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(fullImagePath, UriKind.Absolute);
            myBitmapImage.EndInit();
            ImageControl = new Image { Source = myBitmapImage, Stretch = Stretch.Fill, Name = imageName };
            if( null == ImageControl)
            {
               Logger.Log(LogEnum.LE_ERROR, "MapImage(): 0 imageName=" + imageName );
               return;
            }
            ImageBehavior.SetAnimatedSource(ImageControl, myBitmapImage);
            ImageBehavior.SetAutoStart(ImageControl, true);
            ImageBehavior.SetRepeatBehavior(ImageControl, new RepeatBehavior(1));
            ImageBehavior.AddAnimationCompletedHandler(ImageControl, ImageAnimationLoaded);
         }
         catch (DirectoryNotFoundException dirException)
         {
            Logger.Log(LogEnum.LE_ERROR, "MapImage(): 1 imageName=" + fullImagePath + "\n" + dirException.ToString());
            return;
         }
         catch (FileNotFoundException fileException)
         {
            Logger.Log(LogEnum.LE_ERROR, "MapImage(): 2  imageName=" + fullImagePath + "\n" + fileException.ToString());
            return;
         }
         catch (IOException ioException)
         {
            Logger.Log(LogEnum.LE_ERROR, "MapImage(): 3 imageName=" + fullImagePath + "\n" + ioException.ToString());
            return;
         }
         catch ( Exception e )
         {
            Logger.Log(LogEnum.LE_ERROR, "MapImage(): 4 imageName=" + fullImagePath + "\n" + e.ToString());
            return;
         }
      }
      public MapImage(MapImage mii)
      {
         Name = mii.Name;
         myBitmapImage = mii.myBitmapImage;
         ImageControl = mii.ImageControl;
         AnimationController = mii.AnimationController;
      }
      private void ImageAnimationLoaded(object sender, RoutedEventArgs e)
      {
         try
         {
            ImageControl = (Image)sender;
            // Logger.Log(LogEnum.LE_GAME_INIT, "ImageAnimationLoaded(): name=" + ImageControl.Name);
            AnimationController = ImageBehavior.GetAnimationController(ImageControl);
            if (null == AnimationController)
               Logger.Log(LogEnum.LE_ERROR, "ImageAnimationCompleted(): controller=null");
            else
               IsAnimated = true;
         }
         catch (DirectoryNotFoundException dirException)
         {
            Logger.Log(LogEnum.LE_ERROR, "ImageAnimationLoaded(): 1 imageName=" + ImageControl.Name + "\n" + dirException.ToString());
            return;
         }
         catch (FileNotFoundException fileException)
         {
            Logger.Log(LogEnum.LE_ERROR, "ImageAnimationLoaded(): 2 imageName=" + ImageControl.Name + "\n" + fileException.ToString());
            return;
         }
         catch (IOException ioException)
         {
            Logger.Log(LogEnum.LE_ERROR, "ImageAnimationLoaded(): 3 imageName=" + ImageControl.Name + "\n" + ioException.ToString());
            return;
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "ImageAnimationLoaded(): 4 imageName=" + ImageControl.Name + "\n" + ex.ToString());
            return;
         }
      }
   }
   //--------------------------------------------------------------------------
   [Serializable]
   public class MapImages : IEnumerable, IMapImages
   {
      private readonly ArrayList myList;
      public MapImages() { myList = new ArrayList(); }
      public void Add(IMapImage mii) { myList.Add(mii); }
      public IMapImage RemoveAt(int index)
      {
         IMapImage mii = (IMapImage)myList[index];
         myList.RemoveAt(index);
         return mii;
      }
      public void Insert(int index, IMapImage mii) { myList.Insert(index, mii); }
      public int Count { get { return myList.Count; } }
      public void Clear() { myList.Clear(); }
      public bool Contains(IMapImage mii) { return myList.Contains(mii); }
      public IEnumerator GetEnumerator() { return myList.GetEnumerator(); }
      public int IndexOf(IMapImage mii) { return myList.IndexOf(mii); }
      public void Remove(IMapImage mii) { myList.Remove(mii); }
      public IMapImage this[int index]
      {
         get { return (IMapImage)(myList[index]); }
         set { myList[index] = value; }
      }
      public IMapImage Find(string pathToMatch)
      {
         foreach (Object o in myList)
         {
            IMapImage mii = (IMapImage)o;
            if (mii.Name == pathToMatch)
               return mii;
         }
         return null;
      }
      public BitmapImage GetBitmapImage(string imageName)
      {
         foreach (Object o in myList)
         {
            MapImage mii = (MapImage)o;
            if (mii.Name == imageName)
               return mii.myBitmapImage;
         }
         MapImage miiToAdd = new MapImage(imageName);
         myList.Add(miiToAdd);
         return miiToAdd.myBitmapImage;
      }
   }
}
