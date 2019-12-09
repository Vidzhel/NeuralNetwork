using SymbolRecognitionLib.InversionOfControl;
using System.Windows;

namespace SymbolsRecognition
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            DataContext = ApplicationService.GetAppViewModel;
        }
    }
}
