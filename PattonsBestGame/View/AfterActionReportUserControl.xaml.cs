using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Pattons_Best
{
   public partial class AfterActionReportUserControl : UserControl
   {
      public bool CtorError { get; } = false;
      public IAfterActionReport Report { get; set; }
      public AfterActionReportUserControl(IAfterActionReport report)
      {
         InitializeComponent();
         Report = report;
         if( false == UpdateReport(report) )
         {
            Logger.Log(LogEnum.LE_ERROR, "AfterActionReportUserControl(): UpdateReport() returned false");
            CtorError = true;
            return;
         }
      }
      public bool UpdateReport(IAfterActionReport report)
      {
         String s = AddSpaces(report.Day);
         mySpanDate.Inlines.Clear();
         mySpanDate.Inlines.Add(new Run(s));
         //----------------------------------
         s = AddSpaces(report.Name);
         mySpanTankName.Inlines.Clear();
         mySpanTankName.Inlines.Add(new Run(s));
         //----------------------------------
         s = AddSpaces(report.Model.ToString());
         mySpanTankModel.Inlines.Clear();
         mySpanTankModel.Inlines.Add(new Run(s));
         //----------------------------------
         s = AddSpaces(report.Day);
         mySpanSituation.Inlines.Clear();
         mySpanSituation.Inlines.Add(new Run(s));
         //----------------------------------
         s = AddSpaces(report.Situation.ToString());
         mySpanWeather.Inlines.Clear();
         mySpanWeather.Inlines.Add(new Run(s));
         //----------------------------------
         myRunCommanderRank.Text = report.Commander.myRank.ToString();
         myRunCommanderName.Text = report.Commander.myName;
         myRunCommanderRating.Text = report.Commander.myRating.ToString();
         myRunGunnerName.Text = report.Gunner.myName;
         myRunGunnerRating.Text = report.Gunner.myRating.ToString();
         myRunLoaderName.Text = report.Loader.myName;
         myRunLoaderRating.Text = report.Loader.myRating.ToString();
         myRunDriverName.Text = report.Driver.myName;
         myRunDriverRating.Text = report.Driver.myRating.ToString();
         myRunAssistantName.Text = report.Assistant.myName;
         myRunAssistantRating.Text = report.Assistant.myRating.ToString();
         //----------------------------------
         myRunAmmo30Calibre.Text = report.Ammo30CalibreMG.ToString();
         myRunAmmo50Calibre.Text = report.Ammo50CalibreMG.ToString();
         myRunAmmoSmokeBombs.Text = report.AmmoSmokeBomb.ToString();
         myRunAmmoSmokeGrenades.Text = report.AmmoSmokeGrenade.ToString();
         myRunAmmoPeriscopes.Text = report.AmmoPeriscope.ToString();
         //----------------------------------
         //----------------------------------
         return true;
      }
      public string AddSpaces(string s)
      {
         StringBuilder sb = new StringBuilder("___");  
         sb.Append(s);
         int count = 47 - s.Length;
         for(int i=0; i< count; ++i )
            sb.Append('_');
         return sb.ToString();
      }
   }
}
