using System;

namespace SymbolRecognitionLib.ViewModels
{
    public class AppViewModel: BaseViewModel
    {
        SymbolsRecognitionViewModel symbolsRecognition;

        BaseViewModel baseViewModel;
        public BaseViewModel CurrentViewModel
        {
            get { return baseViewModel; }
            set {
                if (value != null)
                {
                    baseViewModel = value;
                    OnPropertyChanged(nameof(CurrentViewModel));
                }
            }
        }

        public AppViewModel(SymbolsRecognitionViewModel symbolsRecognition)
        {
            this.symbolsRecognition = symbolsRecognition;
            CurrentViewModel = symbolsRecognition;
        }

        public AppViewModel()
        {
            symbolsRecognition = new SymbolsRecognitionViewModel();
            CurrentViewModel = symbolsRecognition;
        }
    }
}
