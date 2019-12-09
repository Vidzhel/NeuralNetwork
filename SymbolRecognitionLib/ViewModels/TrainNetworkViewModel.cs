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

        private int? trainingInputsCount = 0;
        public string TrainingInputsCount
        {
            get { return trainingInputsCount == null? "All": trainingInputsCount.ToString(); }
            set
            {
                if (string.Equals(value, "All", StringComparison.CurrentCultureIgnoreCase))
                    trainingInputsCount = null;
                else if (onlyIntegers.IsMatch(value))
                    trainingInputsCount = int.Parse(value);
                else
                    trainingInputsCount = 0;

                OnPropertyChanged(nameof(TrainingInputsCount));
            }
        }

        private int? trainingLablesCount = 0;
        public string TrainingLablesCount
        {
            get { return trainingLablesCount == null ? "All" : trainingLablesCount.ToString(); }
            set
            {
                if (string.Equals(value, "All", StringComparison.CurrentCultureIgnoreCase))
                    trainingLablesCount = null;
                else if (onlyIntegers.IsMatch(value))
                    trainingLablesCount = int.Parse(value);
                else
                    trainingLablesCount = 0;

                OnPropertyChanged(nameof(TrainingLablesCount));
            }
        }

        private int? testingInputsCount = 0;
        public string TestingInputsCount
        {
            get { return testingInputsCount == null ? "All" : testingInputsCount.ToString(); }
            set
            {
                if (string.Equals(value, "All", StringComparison.CurrentCultureIgnoreCase))
                    testingInputsCount = null;
                else if (onlyIntegers.IsMatch(value))
                    testingInputsCount = int.Parse(value);
                else
                    testingInputsCount = 0;

                OnPropertyChanged(nameof(TestingInputsCount));
            }
        }

        private int? testingLablesCount = 0;
        public string TestingLablesCount
        {
            get { return testingLablesCount == null ? "All" : testingLablesCount.ToString(); }
            set
            {
                if (string.Equals(value, "All", StringComparison.CurrentCultureIgnoreCase))
                    testingLablesCount = null;
                else if (onlyIntegers.IsMatch(value))
                    testingLablesCount = int.Parse(value);
                else
                    testingLablesCount = 0;

                OnPropertyChanged(nameof(TestingLablesCount));
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

        int trainingsInRow;

        public TrainNetworkViewModel()
        {
            initCommands();
            initGraphs();
        }

        void initCommands()
        {
            LoadTrainingInputs = new Command(loadTrainingInputs);
            LoadTrainingLabels = new Command(loadTrainingLabels);
            LoadTestingInputs = new Command(loadTestingInputs);
            LoadTestingLabels = new Command(loadTestingLabels);

            Train = new Command(train);
            ClearGraphs = new Command(clearGraphs);
        }

        #region Command handlers

        #region Load Data

        void loadTrainingInputs(object obj)
        {
            string filePath = FileManager.OpenFileDialog("Mnist Dataset (*.mnist*)|*.mnist*|User Dataset (*.ninp)|*.ninp*", @"C:\work\C#\Neural Network\NeuralNetwork");

            if (string.IsNullOrEmpty(filePath))
                return;

            if (filePath.Contains(".mnist"))
                trainingInputs = MNISTDataLoader.ReadImages(filePath, trainingInputsCount);
            else
            {
                if (Path.GetExtension(filePath) == ".gz")
                    trainingInputs = resizeData((double[][])diserealize(MNISTDataLoader.Decompress(new FileInfo(filePath))), trainingInputsCount);
                else
                    trainingInputs = resizeData((double[][])diserealize(filePath), trainingInputsCount);
            }

            TrainingInputsFileName = Path.GetFileName(filePath);
            tryActivateTrain();
        }

        void loadTrainingLabels(object obj)
        {
            string filePath = FileManager.OpenFileDialog("Mnist Dataset (*.mnist*)|*.mnist*|User Dataset (*.nlabl)|*.nlabl*", @"C:\work\C#\Neural Network\NeuralNetwork");

            if (string.IsNullOrEmpty(filePath))
                return;

            if (filePath.Contains(".mnist"))
                trainingLabels = MNISTDataLoader.ConvertLabels(MNISTDataLoader.ReadLabels(filePath, trainingLablesCount));
            else
            {
                if (Path.GetExtension(filePath) == ".gz")
                    trainingLabels = resizeData((double[][])diserealize(MNISTDataLoader.Decompress(new FileInfo(filePath))), trainingLablesCount);
                else
                    trainingLabels = resizeData((double[][])diserealize(filePath), trainingLablesCount);
            }

            TrainingLabelsFileName = Path.GetFileName(filePath);
            tryActivateTrain();
        }

        void loadTestingInputs(object obj)
        {
            string filePath = FileManager.OpenFileDialog("Mnist Dataset (*.mnist*)|*.mnist*|User Dataset (*.ninp*)|*.ninp*", @"C:\work\C#\Neural Network\NeuralNetwork");

            if (string.IsNullOrEmpty(filePath))
                return;

            if (filePath.Contains(".mnist"))
                testingInputs = MNISTDataLoader.ReadImages(filePath, testingInputsCount);
            else
            {
                if (Path.GetExtension(filePath) == ".gz")
                    testingInputs = resizeData((double[][])diserealize(MNISTDataLoader.Decompress(new FileInfo(filePath))), testingInputsCount);
                else
                    testingInputs = resizeData((double[][])diserealize(filePath), testingInputsCount);
            }

            TestingInputsFileName = Path.GetFileName(filePath);
        }

        void loadTestingLabels(object obj)
        {
            string filePath = FileManager.OpenFileDialog("Mnist Dataset (*.mnist*)|*.mnist*|User Dataset (*.nlabl)|*.nlabl*", @"C:\work\C#\Neural Network\NeuralNetwork");

            if (string.IsNullOrEmpty(filePath))
                return;

            if (filePath.Contains(".mnist"))
                testingLabels = MNISTDataLoader.ConvertLabels(MNISTDataLoader.ReadLabels(filePath, trainingLablesCount));
            else
            {
                if (Path.GetExtension(filePath) == ".gz")
                    testingLabels = resizeData((double[][])diserealize(MNISTDataLoader.Decompress(new FileInfo(filePath))), trainingLablesCount);
                else
                    testingLabels = resizeData((double[][])diserealize(filePath), trainingLablesCount);
            }

            TestingLabelsFileName = Path.GetFileName(filePath);
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
            if (trainingInputs != null && trainingLabels != null)
                TrainActive = true;
        }

        void train(object obj)
        {
            IsTrainingInTheProcess = true;
            trainingsInRow++;
            addSeries();

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
        }

        void initAxes(PlotModel plotModel, string leftAxisTitle)
        {
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
            trainingDataCost.Series.Add(testingDataCostSeries);
        }

        void populateGraphsWithData(Dictionary<string, double> monitorData)
        {
            int trainingIndex = trainingsInRow - 1;
            int x = (int)monitorData["generation"];

            if (monitorTrainingAccuracy)
            {
                LineSeries series = (LineSeries)trainingDataAccuracy.Series[trainingIndex];

                series.Points.Add(new DataPoint(x, monitorData["trainingDataAccuracy"]));
                OnPropertyChanged(nameof(TrainingDataAccuracy));
            }
            if (monitorTrainingCost)
            {
                LineSeries series = (LineSeries)trainingDataCost.Series[trainingIndex];

                series.Points.Add(new DataPoint(x, monitorData["trainingDataCost"]));
                OnPropertyChanged(nameof(TrainingDataCost));
            }
            if (monitorTestingAccuracy)
            {
                LineSeries series = (LineSeries)testingDataAccuracy.Series[trainingIndex];

                series.Points.Add(new DataPoint(x, monitorData["testingDataAccuracy"]));
            }
            if (monitorTestingCost)
            {
                LineSeries series = (LineSeries)testingDataCost.Series[trainingIndex];

                series.Points.Add(new DataPoint(x, monitorData["testingDataCost"]));
            }

        }

        #endregion
    }
}
