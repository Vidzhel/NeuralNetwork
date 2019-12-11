using NeuralNetworkLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;

namespace SymbolRecognitionLib.ViewModels
{
    public class AppViewModel : BaseViewModel
    {
        public Command LoadNeuralNetwork { get; private set; }
        public Command SaveNeuralNetwork { get; private set; }
        public Command ChangeTab { get; private set; }
        public Command CreateNeuralNetwork { get; private set; }

        string loadedNetworkName = "new_nn.net";
        public string LoadedNetworkName
        {
            get
            {
                return loadedNetworkName;
            }

            set
            {
                loadedNetworkName = value;
                OnPropertyChanged(nameof(LoadedNetworkName));
            }
        }

        bool isNetworkLoaded = false;
        public bool IsNetworkLoaded
        {
            get
            {
                return isNetworkLoaded;
            }

            set
            {
                isNetworkLoaded = value;
                OnPropertyChanged(nameof(IsNetworkLoaded));
            }
        }

        BaseViewModel currentViewModel;
        public BaseViewModel CurrentViewModel
        {
            get { return currentViewModel; }
            set
            {
                if (value != null)
                {
                    currentViewModel = value;
                    OnPropertyChanged(nameof(CurrentViewModel));
                }
            }
        }

        Regex onlyArray = new Regex(@"^\d+ (?:\d+ )*\d+$");
        int[] neuralNetworkTypology;
        public string NeuralNetworkTypology
        {
            get
            {
                return neuralNetworkTypology == null ? "" : arrayToString(neuralNetworkTypology);
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    neuralNetworkTypology = new int[0];
                    CanCreateNetwork = false;
                }
                else if (onlyArray.IsMatch(value))
                {
                    neuralNetworkTypology = stringToArray(value);
                    CanCreateNetwork = true;
                }
                else
                {
                    NeuralNetworkTypology = "";
                    CanCreateNetwork = false;
                }
                OnPropertyChanged(nameof(NeuralNetworkTypology));

            }
        }

        bool canCreateNetwork = false;
        public bool CanCreateNetwork
        {
            get
            {
                return canCreateNetwork;
            }

            set
            {
                canCreateNetwork = value;
                OnPropertyChanged(nameof(CanCreateNetwork));
            }
        }

        public ObservableCollection<Function<double, double>> ActivationFunctions { get; private set; } = new ObservableCollection<Function<double, double>>();
        public ObservableCollection<Function<double, double, double>> CostFunctions { get; private set; } = new ObservableCollection<Function<double, double, double>>();

        int choseActivationFunction = 0;
        public int ChoseActivationFunction
        {
            get
            {
                return choseActivationFunction;
            }

            set
            {
                choseActivationFunction = value;
                OnPropertyChanged(nameof(ChoseActivationFunction));
            }
        }

        int choseCostFunction = 0;
        public int ChoseCostFunction
        {
            get
            {
                return choseCostFunction;
            }

            set
            {
                choseCostFunction = value;
                OnPropertyChanged(nameof(ChoseCostFunction));
            }
        }

        List<BaseViewModel> tabs = new List<BaseViewModel>();

        public NeuralNetwork NeuralNetwork { get; private set; }


        #region Constructor

        public AppViewModel()
        {
            tabs.Add(new SymbolsRecognitionViewModel());
            tabs.Add(new TrainNetworkViewModel());
            CurrentViewModel = tabs[0];

            ActivationFunctions.Add(Function<double, double>.SigmoidFunction);
            CostFunctions.Add(Function<double, double, double>.CrossEntropy);
            CostFunctions.Add(Function<double, double, double>.MeanSquareCost);

            initCommands();
        }

        void initCommands()
        {
            ChangeTab = new Command(changeTab);
            LoadNeuralNetwork = new Command(loadNeuralNetwork);
            SaveNeuralNetwork = new Command(saveNeuralNetwork);
            CreateNeuralNetwork = new Command(createNeuralNetwork);
        }

        #endregion


        #region Commands handlers

        void changeTab(object tabIndex)
        {
            CurrentViewModel = tabs[(int)tabIndex];
        }

        void loadNeuralNetwork(object obj)
        {
            string filePath = FileManager.OpenFileDialog(filter: "Neural Network (*.net)|*.net", @"C:\work\C#\Neural Network\NeuralNetwork", "Choose network to load");

            if (!string.IsNullOrEmpty(filePath))
            {
                NeuralNetwork = NeuralNetwork.Load(filePath);
                LoadedNetworkName = Path.GetFileName(filePath);
                IsNetworkLoaded = true;
            }
        }

        void saveNeuralNetwork(object obj)
        {
            string filePath = FileManager.OpenSaveFileDialog(filter: "Neural Network (*.net)|*.net", @"C:\work\C#\Neural Network\NeuralNetwork");

            if (!string.IsNullOrEmpty(filePath))
            {
                NeuralNetwork.Save(filePath);
                LoadedNetworkName = Path.GetFileName(filePath);
            }
        }

        void createNeuralNetwork(object obj)
        {
            NeuralNetwork = new NeuralNetwork(neuralNetworkTypology, CostFunctions[choseCostFunction], ActivationFunctions[choseActivationFunction]);

            IsNetworkLoaded = true;
            LoadedNetworkName = "new_nn.net";
            NeuralNetworkTypology = "";
        }
        #endregion

        #region helpers

        string arrayToString(int[] array)
        {
            string res = "";

            foreach (var item in array)
                res += item.ToString() + " ";

            return res;
        }

        int[] stringToArray(string str)
        {
            string[] strings = str.Split(' ');
            int[] res = new int[strings.Length];

            for (int i = 0; i < strings.Length; i++)
                res[i] = int.Parse(strings[i]);

            return res;
        }

        #endregion
    }
}
