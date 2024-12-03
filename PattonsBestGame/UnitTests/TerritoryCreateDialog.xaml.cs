using System;
using System.Windows;
using System.Windows.Controls;
namespace Pattons_Best
{
    public partial class TerritoryCreateDialog : Window
    {
        public String RadioOutputText { get; set; } = "Open";
        public TerritoryCreateDialog()
        {
            InitializeComponent();
        }
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = (RadioButton)sender;
            RadioOutputText = radioButton.Content.ToString();
        }
    }
}
