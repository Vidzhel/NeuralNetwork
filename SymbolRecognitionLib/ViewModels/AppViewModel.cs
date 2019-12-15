using NeuralNetworkLib;
using SymbolRecognitionLib.InversionOfControl;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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

        string costFuncitonName;
        public string CostFuncitonName
        {
            get
            {
                return costFuncitonName;
            }

            set
            {
                costFuncitonName = value;
                OnPropertyChanged(nameof(CostFuncitonName));
            }
        }

        string activationFunctionName;
        public string ActivationFunctionName
        {
            get
            {
                return activationFunctionName;
            }

            set
            {
                activationFunctionName = value;
                OnPropertyChanged(nameof(ActivationFunctionName));
            }
        }

        string neuralNetworkTypology;
        public string NeuralNetworkTypology
        {
            get
            {
                return neuralNetworkTypology;
            }

            set
            {
                neuralNetworkTypology = value;
                OnPropertyChanged(nameof(NeuralNetworkTypology));
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
        int[] newNeuralNetworkTypology;
        public string NewNeuralNetworkTypology
        {
            get
            {
                return newNeuralNetworkTypology == null ? "" : arrayToString(newNeuralNetworkTypology);
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    newNeuralNetworkTypology = new int[0];
                    CanCreateNetwork = false;
                }
                else if (onlyArray.IsMatch(value))
                {
                    newNeuralNetworkTypology = stringToArray(value);
                    CanCreateNetwork = true;
                }
                else
                {
                    NewNeuralNetworkTypology = "";
                    CanCreateNetwork = false;
                }
                OnPropertyChanged(nameof(NewNeuralNetworkTypology));

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
        public NeuralNetwork BackupedNetwork { get; private set; }


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


        public void RestoreNetwork()
        {
            if (BackupedNetwork != null)
                NeuralNetwork = new NeuralNetwork(BackupedNetwork);
        }

        public void BackupNetwork()
        {
            BackupedNetwork = new NeuralNetwork(NeuralNetwork);
        }

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

            loadNetworkInfo();
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
            NeuralNetwork = new NeuralNetwork(newNeuralNetworkTypology, CostFunctions[choseCostFunction], ActivationFunctions[choseActivationFunction]);

            IsNetworkLoaded = true;
            LoadedNetworkName = "new_nn.net";
            NewNeuralNetworkTypology = "";
            loadNetworkInfo();
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

        void loadNetworkInfo()
        {
            CostFuncitonName = NeuralNetwork.CostFunction.Name;
            ActivationFunctionName = NeuralNetwork.Layers.FirstOrDefault()?.Neurons.First().ActivationFunction.Name ?? "None";

            int[] typology = new int[NeuralNetwork.Layers.Count];

            for (int layer = 0; layer < typology.Length; layer++)
                typology[layer] = NeuralNetwork.Layers[layer].OutputsCount;

            NeuralNetworkTypology = arrayToString(typology);
        }

        #endregion
    }
}
