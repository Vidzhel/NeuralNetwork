using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using SymbolRecognitionLib.InversionOfControl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SymbolRecognitionLib.ViewModels
{
    public class TrainNetworkViewModel : BaseViewModel
    {
        #region Binded data

        public Command LoadTrainingInputs { get; private set; }
        public Command LoadTrainingLabels { get; private set; }
        public Command LoadTestingInputs { get; private set; }
        public Command LoadTestingLabels { get; private set; }
        public Command Train { get; private set; }
        public Command ClearGraphs { get; private set; }

        private string trainingInputsFileName = "Not loaded";
        public string TrainingInputsFileName
        {
            get { return trainingInputsFileName; }
            set
            {
                trainingInputsFileName = value;
                OnPropertyChanged(nameof(TrainingInputsFileName));
            }
        }

        private string trainingLabelsFileName = "Not loaded";
        public string TrainingLabelsFileName
        {
            get { return trainingLabelsFileName; }
            set
            {
                trainingLabelsFileName = value;
                OnPropertyChanged(nameof(TrainingLabelsFileName));
            }
        }

        private string testingInputsFileName = "Not loaded";
        public string TestingInputsFileName
        {
            get { return testingInputsFileName; }
            set
            {
                testingInputsFileName = value;
                OnPropertyChanged(nameof(TestingInputsFileName));
            }
        }

        private string testingLabelsFileName = "Not loaded";
        public string TestingLabelsFileName
        {
            get { return testingLabelsFileName; }
            set
            {
                testingLabelsFileName = value;
                OnPropertyChanged(nameof(TestingLabelsFileName));
            }
        }

        Regex onlyIntegers = new Regex(@"^\d+$");
        Regex onlyNumbers = new Regex(@"^\d+(?:\,\d+)?$");

        private int? trainingDataCount = 0;
        public string TrainingDataCount
        {
            get { return trainingDataCount == null? "All": trainingDataCount.ToString(); }
            set
            {
                if (string.Equals(value, "All", StringComparison.CurrentCultureIgnoreCase))
                    trainingDataCount = null;
                else if (onlyIntegers.IsMatch(value))
                    trainingDataCount = int.Parse(value);
                else
                    trainingDataCount = 0;

                OnPropertyChanged(nameof(TrainingDataCount));
            }
        }

        private int? testingDataCount = 0;
        public string TestingDataCount
        {
            get { return testingDataCount == null ? "All" : testingDataCount.ToString(); }
            set
            {
                if (string.Equals(value, "All", StringComparison.CurrentCultureIgnoreCase))
                    testingDataCount = null;
                else if (onlyIntegers.IsMatch(value))
                    testingDataCount = int.Parse(value);
                else
                    testingDataCount = 0;

                OnPropertyChanged(nameof(TestingDataCount));
            }
        }

        private float learningRate = 0.0F;
        public string LearningRate
        {
            get { return learningRate.ToString(); }
            set
            {
                if (onlyNumbers.IsMatch(value))
                    learningRate = float.Parse(value);
                else
                    learningRate = 0.0F;

                OnPropertyChanged(nameof(LearningRate));
            }
        }

        private int generations = 0;
        public string Generations
        {
            get { return generations.ToString(); }
            set
            {
                if (onlyIntegers.IsMatch(value))
                    generations = int.Parse(value);
                else
                    generations = 0;

                updateGraph();
                OnPropertyChanged(nameof(Generations));
            }
        }

        private int miniBatchSize = 0;
        public string MiniBatchSize
        {
            get { return miniBatchSize.ToString(); }
            set
            {
                if (onlyIntegers.IsMatch(value))
                    miniBatchSize = int.Parse(value);
                else
                    miniBatchSize = 0;

                OnPropertyChanged(nameof(MiniBatchSize));
            }
        }

        private double regularizationFactor = 0;
        public string RegularizationFactor
        {
            get { return regularizationFactor.ToString(); }
            set
            {
                if (onlyNumbers.IsMatch(value))
                    regularizationFactor = double.Parse(value);
                else
                    regularizationFactor = 0.0;

                OnPropertyChanged(nameof(RegularizationFactor));
            }
        }

        private double accuracyTolerance = 0.0;
        public string AccuracyTolerance
        {
            get { return accuracyTolerance.ToString(); }
            set
            {
                if (onlyNumbers.IsMatch(value))
                    accuracyTolerance = double.Parse(value);
                else
                    accuracyTolerance = 0.0;

                OnPropertyChanged(nameof(AccuracyTolerance));
            }
        }

        private bool monitorTrainingAccuracy;
        public bool MonitorTrainingAccuracy
        {
            get { return monitorTrainingAccuracy; }
            set
            {
                monitorTrainingAccuracy = value;
                OnPropertyChanged(nameof(MonitorTrainingAccuracy));
            }
        }

        private bool monitorTrainingCost;
        public bool MonitorTrainingCost
        {
            get { return monitorTrainingCost; }
            set
            {
                monitorTrainingCost = value;
                OnPropertyChanged(nameof(MonitorTrainingCost));
            }
        }

        private bool useTestingData;
        public bool UseTestingData
        {
            get { return useTestingData; }
            set
            {
                useTestingData = value;
                OnPropertyChanged(nameof(UseTestingData));
            }
        }

        private bool monitorTestingAccuracy;
        public bool MonitorTestingAccuracy
        {
            get { return monitorTestingAccuracy; }
            set
            {
                monitorTestingAccuracy = value;
                OnPropertyChanged(nameof(MonitorTestingAccuracy));
            }
        }

        private bool monitorTestingCost;
        public bool MonitorTestingCost
        {
            get { return monitorTestingCost; }
            set
            {
                monitorTestingCost = value;
                OnPropertyChanged(nameof(MonitorTestingCost));
            }
        }

        private bool trainActive;
        public bool TrainActive
        {
            get { return trainActive; }
            set
            {
                trainActive = value;
                OnPropertyChanged(nameof(TrainActive));
            }
        }

        private bool isTrainingInTheProcess;
        public bool IsTrainingInTheProcess
        {
            get { return isTrainingInTheProcess; }
            set
            {
                isTrainingInTheProcess = value;
                OnPropertyChanged(nameof(IsTrainingInTheProcess));
            }
        }

        private PlotModel trainingDataAccuracy = new PlotModel();
        public PlotModel TrainingDataAccuracy
        {
            get { return trainingDataAccuracy; }
            private set
            {
                trainingDataAccuracy = value;
                OnPropertyChanged(nameof(TrainingDataAccuracy));
            }
        }

        private PlotModel trainingDataCost = new PlotModel();
        public PlotModel TrainingDataCost
        {
            get { return trainingDataCost; }
            private set
            {
                trainingDataCost = value;
                OnPropertyChanged(nameof(TrainingDataCost));
            }
        }

        private PlotModel testingDataAccuracy = new PlotModel();
        public PlotModel TestingDataAccuracy
        {
            get { return testingDataAccuracy; }
            private set
            {
                testingDataAccuracy = value;
                OnPropertyChanged(nameof(TestingDataAccuracy));
            }
        }

        private PlotModel testingDataCost = new PlotModel();
        public PlotModel TestingDataCost
        {
            get { return testingDataCost; }
            private set
            {
                testingDataCost = value;
                OnPropertyChanged(nameof(TestingDataCost));
            }
        }


        #endregion

        double[][] trainingInputs;
        double[][] trainingLabels;

        double[][] testingInputs;
        double[][] testingLabels;

        string trainingInputsFilePath;
        string trainingLabelsFilePath;

        string testingInputsFilePath;
        string testingLabelsFilePath;
        
        int trainingsInRow;

        public TrainNetworkViewModel()
        {
            initCommands();
            initGraphs();
        }

        void initCommands()
        {
            LoadTrainingInputs = new Command(chooseTrainingInputs);
            LoadTrainingLabels = new Command(chooseTrainingLabels);
            LoadTestingInputs = new Command(chooseTestingInputs);
            LoadTestingLabels = new Command(chooseTestingLabel);

            Train = new Command(train);
            ClearGraphs = new Command(clearGraphs);
        }

        #region Command handlers

        #region Load Data

        void chooseTrainingInputs(object obj)
        {
            string filePath = FileManager.OpenFileDialog("Mnist Dataset (*.mnist*)|*.mnist*|User Dataset (*.ninp)|*.ninp*", @"C:\work\C#\Neural Network\NeuralNetwork");

            if (string.IsNullOrEmpty(filePath))
                return;

            trainingInputsFilePath = filePath;
            TrainingInputsFileName = Path.GetFileName(filePath);
            tryActivateTrain();
        }

        void chooseTrainingLabels(object obj)
        {
            string filePath = FileManager.OpenFileDialog("Mnist Dataset (*.mnist*)|*.mnist*|User Dataset (*.nlabl)|*.nlabl*", @"C:\work\C#\Neural Network\NeuralNetwork");

            if (string.IsNullOrEmpty(filePath))
                return;

            trainingLabelsFilePath = filePath;
            TrainingLabelsFileName = Path.GetFileName(filePath);
            tryActivateTrain();
        }

        void chooseTestingInputs(object obj)
        {
            string filePath = FileManager.OpenFileDialog("Mnist Dataset (*.mnist*)|*.mnist*|User Dataset (*.ninp*)|*.ninp*", @"C:\work\C#\Neural Network\NeuralNetwork");

            if (string.IsNullOrEmpty(filePath))
                return;

            testingInputsFilePath = filePath;
            TestingInputsFileName = Path.GetFileName(filePath);
        }


        void chooseTestingLabel(object obj)
        {
            string filePath = FileManager.OpenFileDialog("Mnist Dataset (*.mnist*)|*.mnist*|User Dataset (*.ninp*)|*.ninp*", @"C:\work\C#\Neural Network\NeuralNetwork");

            if (string.IsNullOrEmpty(filePath))
                return;

            testingLabelsFilePath = filePath;
            TestingLabelsFileName = Path.GetFileName(filePath);
        }

        void loadData()
        {
            loadTrainingInputs();
            loadTrainingLabels();
            loadTestingInputs();
            loadTestingLabels();
        }

        void loadTrainingInputs()
        {
            if (string.IsNullOrEmpty(trainingInputsFilePath) || trainingDataCount == 0)
            {
                trainingInputs = null;
                return;
            }

            if (trainingInputsFilePath.Contains(".mnist"))
                trainingInputs = MNISTDataLoader.ReadImages(trainingInputsFilePath, trainingDataCount);
            else
            {
                if (Path.GetExtension(trainingInputsFilePath) == ".gz")
                    trainingInputs = resizeData((double[][])diserealize(MNISTDataLoader.Decompress(new FileInfo(trainingInputsFilePath))), trainingDataCount);
                else
                    trainingInputs = resizeData((double[][])diserealize(trainingInputsFilePath), trainingDataCount);
            }

        }

        void loadTrainingLabels()
        {

            if (string.IsNullOrEmpty(trainingLabelsFilePath) || trainingDataCount == 0)
            {
                trainingLabels = null;
                return;
            }


            if (trainingLabelsFilePath.Contains(".mnist"))
                trainingLabels = MNISTDataLoader.ConvertLabels(MNISTDataLoader.ReadLabels(trainingLabelsFilePath, trainingDataCount));
            else
            {
                if (Path.GetExtension(trainingLabelsFilePath) == ".gz")
                    trainingLabels = resizeData((double[][])diserealize(MNISTDataLoader.Decompress(new FileInfo(trainingLabelsFilePath))), trainingDataCount);
                else
                    trainingLabels = resizeData((double[][])diserealize(trainingLabelsFilePath), trainingDataCount);
            }
        }

        void loadTestingInputs()
        {

            if (string.IsNullOrEmpty(testingInputsFilePath) || testingDataCount == 0)
            {
                testingInputs = null;
                return;
            }

            if (testingInputsFilePath.Contains(".mnist"))
                testingInputs = MNISTDataLoader.ReadImages(testingInputsFilePath, testingDataCount);
            else
            {
                if (Path.GetExtension(testingInputsFilePath) == ".gz")
                    testingInputs = resizeData((double[][])diserealize(MNISTDataLoader.Decompress(new FileInfo(testingInputsFilePath))), testingDataCount);
                else
                    testingInputs = resizeData((double[][])diserealize(testingInputsFilePath), testingDataCount);
            }
        }

        void loadTestingLabels()
        {
            if (string.IsNullOrEmpty(testingLabelsFilePath) || testingDataCount == 0)
            {
                testingLabels = null;
                return;
            }

            if (testingLabelsFilePath.Contains(".mnist"))
                testingLabels = MNISTDataLoader.ConvertLabels(MNISTDataLoader.ReadLabels(testingLabelsFilePath, testingDataCount));
            else
            {
                if (Path.GetExtension(testingLabelsFilePath) == ".gz")
                    testingLabels = resizeData((double[][])diserealize(MNISTDataLoader.Decompress(new FileInfo(testingLabelsFilePath))), testingDataCount);
                else
                    testingLabels = resizeData((double[][])diserealize(testingLabelsFilePath), testingDataCount);
            }

        }

        object diserealize(string filePath)
        {
            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return formatter.Deserialize(stream);
            }
        }

        object diserealize(MemoryStream stream)
        {
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return formatter.Deserialize(stream);
            }
            finally
            {
                stream.Dispose();
            }
        }

        double[][] resizeData(double[][] data, int? size)
        {
            if (!size.HasValue || size > data.Length)
                return data;

            double[][] resizedData = new double[(int)size][];
            Array.Copy(sourceArray: data, 0, destinationArray: resizedData, 0, (int)size);

            return resizedData;
        }

        #endregion

        void clearGraphs(object obj)
        {
            initGraphs();
            trainingsInRow = 0;
        }

        void tryActivateTrain()
        {
            if (!string.IsNullOrEmpty(trainingInputsFilePath) && string.IsNullOrEmpty(trainingLabelsFilePath))
                TrainActive = true;
        }

        void train(object obj)
        {
            IsTrainingInTheProcess = true;
            trainingsInRow++;
            loadData();
            addSeries();
            cheickLoadedData();

            try
            {
                foreach (var monitorData in ApplicationService.GetNeuralNetwork.Train(trainingInputs, trainingLabels, learningRate, generations, miniBatchSize, testingInputs, testingLabels, regularizationFactor, monitorTrainingCost, monitorTrainingAccuracy, monitorTestingCost, monitorTestingAccuracy, accuracyTolerance))
                    populateGraphsWithData(monitorData);
            }
            finally
            {
                IsTrainingInTheProcess = false;
            }
        }
        void cheickLoadedData()
        {
            if (testingInputs == null || testingLabels == null)
            {
                testingInputs = null;
                testingLabels = null;

                MonitorTestingAccuracy = false;
                MonitorTestingCost = false;
            }
        }

        #region Graph related methods

        void initGraphs()
        {
            trainingDataAccuracy = new PlotModel();
            trainingDataAccuracy.Subtitle = "Training data accuracy";
            initAxes(trainingDataAccuracy, "Accuracy");

            trainingDataCost = new PlotModel();
            trainingDataCost.Subtitle = "Training data cost";
            initAxes(trainingDataCost, "Cost");

            testingDataAccuracy = new PlotModel();
            testingDataAccuracy.Subtitle = "Testing data accuracy";
            initAxes(testingDataAccuracy, "Accuracy");

            testingDataCost = new PlotModel();
            testingDataCost.Subtitle = "Training data cost";
            initAxes(testingDataCost, "Cost");

            OnPropertyChanged(nameof(TrainingDataAccuracy));
            OnPropertyChanged(nameof(TrainingDataCost));
            OnPropertyChanged(nameof(TestingDataAccuracy));
            OnPropertyChanged(nameof(TestingDataCost));

            trainingDataAccuracy.InvalidatePlot(true);
        }

        void updateGraph()
        {
            initAxes(trainingDataAccuracy, "Accuracy");
            initAxes(trainingDataCost, "Cost");
            initAxes(testingDataAccuracy, "Accuracy");
            initAxes(testingDataCost, "Cost");

            trainingDataAccuracy.InvalidatePlot(true);
            trainingDataCost.InvalidatePlot(true);
            testingDataAccuracy.InvalidatePlot(true);
            testingDataCost.InvalidatePlot(true);
        }

        void initAxes(PlotModel plotModel, string leftAxisTitle)
        {
            for (int i = 0; i < plotModel.Axes.Count; i++)
                plotModel.Axes.RemoveAt(0);

            var bottomAxis = new LinearAxis();
            bottomAxis.Title = "Generation";
            bottomAxis.Position = AxisPosition.Bottom;
            bottomAxis.Minimum = 0;
            bottomAxis.Maximum = generations == 0 ? 1 : generations;
            bottomAxis.MajorStep = 5;
            bottomAxis.MinorStep = 2.5;

            var leftAxis = new LinearAxis();
            leftAxis.Title = leftAxisTitle;
            leftAxis.Position = AxisPosition.Left;
            leftAxis.Minimum = 0;
            leftAxis.Maximum = 100;
            leftAxis.MajorStep = 10;
            leftAxis.MinorStep = 2.5;

            plotModel.Axes.Add(bottomAxis);
            plotModel.Axes.Add(leftAxis);
        }

        void addSeries()
        {
            string title = trainingsInRow.ToString();

            var trainingDataAccuracySeries = new LineSeries();
            trainingDataAccuracySeries.Title = title;

            var trainingDataCostSeries = new LineSeries();
            trainingDataCostSeries.Title = title;

            var testingDataAccuracySeries = new LineSeries();
            trainingDataAccuracySeries.Title = title;

            var testingDataCostSeries = new LineSeries();
            trainingDataCostSeries.Title = title;

            trainingDataAccuracy.Series.Add(trainingDataAccuracySeries);
            trainingDataCost.Series.Add(trainingDataCostSeries);
            testingDataAccuracy.Series.Add(testingDataAccuracySeries);
            testingDataCost.Series.Add(testingDataCostSeries);
        }

        void populateGraphsWithData(Dictionary<string, double> monitorData)
        {
            int trainingIndex = trainingsInRow - 1;
            int x = (int)monitorData["generation"];

            if (monitorTrainingAccuracy)
            {
                LineSeries series = (LineSeries)trainingDataAccuracy.Series[trainingIndex];

                series.Points.Add(new DataPoint(x, monitorData["trainingDataAccuracy"]));
                //OnPropertyChanged(nameof(TrainingDataAccuracy));
                trainingDataAccuracy.InvalidatePlot(true);
            }
            if (monitorTrainingCost)
            {
                LineSeries series = (LineSeries)trainingDataCost.Series[trainingIndex];

                series.Points.Add(new DataPoint(x, monitorData["trainingDataCost"]));
                OnPropertyChanged(nameof(TrainingDataCost));
                trainingDataCost.InvalidatePlot(true);
            }
            if (monitorTestingAccuracy)
            {
                LineSeries series = (LineSeries)testingDataAccuracy.Series[trainingIndex];

                series.Points.Add(new DataPoint(x, monitorData["testingDataAccuracy"]));
                testingDataAccuracy.InvalidatePlot(true);
            }
            if (monitorTestingCost)
            {
                LineSeries series = (LineSeries)testingDataCost.Series[trainingIndex];

                series.Points.Add(new DataPoint(x, monitorData["testingDataCost"]));
                testingDataCost.InvalidatePlot(true);
            }

        }
        
        #endregion

        #endregion
    }
}
