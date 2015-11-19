using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Prism.Windows.Mvvm;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238
  
namespace Cinder.Windows.AdminTool.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SignInPage : Page
    {
        public SignInPage()
        {
            this.InitializeComponent();


        }

        private void backButtonImage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            int h = 8;
        }

        private void btnAdditionalOptions_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
