using NeuralNetworkLib;
using SymbolRecognitionLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbolRecognitionLib.InversionOfControl
{
    public static class ApplicationService
    {
        static AppViewModel appViewModel;
        public static AppViewModel GetAppViewModel { get { if (appViewModel == null) appViewModel = new AppViewModel(); return appViewModel; } }

        public static NeuralNetwork GetNeuralNetwork => GetAppViewModel.NeuralNetwork;
    }
}
