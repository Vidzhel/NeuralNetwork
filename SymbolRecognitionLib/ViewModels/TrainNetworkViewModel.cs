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
using System.Threading;
using System.Threading.Tasks;

namespace SymbolRecognitionLib.ViewModels
{
    public enum GraphType
    {
        Accuracy,
        Cost
    }

    public class TrainNetworkViewModel : BaseViewModel
    {
        #region Binded data

        public Command LoadTrainingInputs { get; private set; }
        public Command LoadTrainingLabels { get; private set; }
        public Command LoadTestingInputs { get; private set; }
        public Command LoadTestingLabels { get; private set; }
        public Command Train { get; private set; }
        public Command CancelTraining { get; private set; }
        public Command RestoreLastNetVersion { get; private set; }
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
            get { return trainingDataCount == null ? "All" : trainingDataCount.ToString(); }
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

                initGraphs();
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

        private double accuracyTolerance = 0.5;
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

        private double trainingProgress;
        public double TrainingProgress
        {
            get { return trainingProgress; }
            set
            {
                trainingProgress = value;
                OnPropertyChanged(nameof(TrainingProgress));
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
        public double accuracyMin = double.MaxValue;
        public double accuracyMax;

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
        public double costMin = double.MaxValue;
        public double costMax;

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
        CancellationTokenSource cancellationTokenSource;
        CancellationToken trainingCancellationToken;

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
            CancelTraining = new Command(cancelTraining);
            RestoreLastNetVersion = new Command(restoreNet);
            ClearGraphs = new Command(clearGraphs);
        }

        #region Command handlers

        #region Load Data

        void chooseTrainingInputs(object obj)
        {
            string filePath = FileManager.OpenFileDialog("Mnist Dataset (*.mnist*)|*.mnist*|User Dataset (*.ninp*)|*.ninp*", @"C:\work\C#\Neural Network\NeuralNetwork");

            if (string.IsNullOrEmpty(filePath))
                return;

            trainingInputsFilePath = filePath;
            TrainingInputsFileName = Path.GetFileName(filePath);
            tryActivateTrain();
        }

        void chooseTrainingLabels(object obj)
        {
            string filePath = FileManager.OpenFileDialog("Mnist Dataset (*.mnist*)|*.mnist*|User Dataset (*.nlabl*)|*.nlabl*", @"C:\work\C#\Neural Network\NeuralNetwork");

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
                    trainingInputs = resizeData((double[][])diserealize(FileManager.Decompress(trainingInputsFilePath)), trainingDataCount);
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
                    trainingLabels = resizeData((double[][])diserealize(FileManager.Decompress(trainingLabelsFilePath)), trainingDataCount);
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
                    testingInputs = resizeData((double[][])diserealize(FileManager.Decompress(testingInputsFilePath)), testingDataCount);
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
                    testingLabels = resizeData((double[][])diserealize(FileManager.Decompress(testingLabelsFilePath)), testingDataCount);
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


        void tryActivateTrain()
        {
            if (!string.IsNullOrEmpty(trainingInputsFilePath) && !string.IsNullOrEmpty(trainingLabelsFilePath) && !IsTrainingInTheProcess)
            {
                TrainActive = true;
                return;
            }

            TrainActive = false;
        }

        void cheickLoadedTestingData()
        {
            if (testingInputs == null || testingLabels == null)
            {
                testingInputs = null;
                testingLabels = null;

                MonitorTestingAccuracy = false;
                MonitorTestingCost = false;
            }
        }

        void train(object obj)
        {
            if (IsTrainingInTheProcess)
                return;

            Task.Run(() =>
            {
                IsTrainingInTheProcess = true;

                try
                {
                    TrainingProgress = 0;
                    double progressStep = 95.0 / generations;

                    prepareTraining();
                    ApplicationService.BackupNetwork();

                    TrainingProgress = 5;

                    foreach (var monitorData in ApplicationService.GetNeuralNetwork.Train(trainingInputs, trainingLabels, learningRate, generations, miniBatchSize, trainingCancellationToken, testingInputs, testingLabels, regularizationFactor, monitorTrainingCost, monitorTrainingAccuracy, monitorTestingCost, monitorTestingAccuracy, accuracyTolerance))
                    {
                        populateGraphsWithData(monitorData);
                        TrainingProgress += progressStep;
                    }
                }
                catch (OperationCanceledException e)
                {
                    ApplicationService.RestoreNetwork();
                }
                catch (Exception e)
                {
                }
                finally
                {
                    IsTrainingInTheProcess = false;
                    tryActivateTrain();
                    cancellationTokenSource.Dispose();
                }
            });
        }

        void prepareTraining()
        {
            tryActivateTrain();
            trainingsInRow++;

            loadData();
            if (trainingInputs == null || trainingLabels == null)
                throw new Exception();

            cheickLoadedTestingData();
            addSeries();

            cancellationTokenSource = new CancellationTokenSource();
            trainingCancellationToken = cancellationTokenSource.Token;
        }

        void cancelTraining(object obj)
        {
            if (!isTrainingInTheProcess)
                return;

            cancellationTokenSource.Cancel();
        }

        void restoreNet(object obj)
        {
            ApplicationService.RestoreNetwork();
        }

        void clearGraphs(object obj)
        {
            initGraphs();
            trainingsInRow = 0;
        }

        #region Graph related methods

        void initGraphs()
        {
            Task.Run(() =>
            {

                TrainingDataAccuracy = new PlotModel();
                trainingDataAccuracy.Subtitle = "Training data accuracy";
                initAxes(trainingDataAccuracy, GraphType.Accuracy);

                TrainingDataCost = new PlotModel();
                trainingDataCost.Subtitle = "Training data cost";
                initAxes(trainingDataCost, GraphType.Cost);

                TestingDataAccuracy = new PlotModel();
                testingDataAccuracy.Subtitle = "Testing data accuracy";
                initAxes(testingDataAccuracy, GraphType.Accuracy);

                TestingDataCost = new PlotModel();
                testingDataCost.Subtitle = "Training data cost";
                initAxes(testingDataCost, GraphType.Cost);

                accuracyMax = 0;
                costMax = 0;
                accuracyMin = double.MaxValue;
                costMin = double.MaxValue;
            });
        }

        void updateGraphs()
        {
            trainingDataAccuracy.InvalidatePlot(true);
            trainingDataCost.InvalidatePlot(true);
            testingDataAccuracy.InvalidatePlot(true);
            testingDataCost.InvalidatePlot(true);
        }

        void initAxes(PlotModel plotModel, GraphType type)
        {
            for (int i = 0; i < plotModel.Axes.Count; i++)
                plotModel.Axes.RemoveAt(0);

            var bottomAxis = new LinearAxis();
            bottomAxis.Title = "Generation";
            bottomAxis.Position = AxisPosition.Bottom;
            bottomAxis.Minimum = 0;
            bottomAxis.Maximum = generations == 0 ? 1 : generations - 1;
            bottomAxis.MajorStep = 2;
            bottomAxis.MinorStep = 1;
            bottomAxis.MajorGridlineColor = OxyColors.Gray;
            bottomAxis.MajorGridlineStyle = LineStyle.Dot;

            var leftAxis = new LinearAxis();
            leftAxis.Position = AxisPosition.Left;
            leftAxis.MajorGridlineColor = OxyColors.Gray;
            leftAxis.MajorGridlineStyle = LineStyle.Dot;

            if (type == GraphType.Accuracy)
            {
                leftAxis.Title = "Accuracy";
                leftAxis.Minimum = 50;
                leftAxis.Maximum = 100;
                leftAxis.MajorStep = 5;
                leftAxis.MinorStep = 2.5;
            }
            else
            {
                leftAxis.Title = "Cost";
                leftAxis.Minimum = 0;
                leftAxis.Maximum = 2;
                leftAxis.MajorStep = 0.2;
                leftAxis.MinorStep = 0.1;
            }

            plotModel.Axes.Add(bottomAxis);
            plotModel.Axes.Add(leftAxis);
        }

        void addSeries(byte transparancy = 64, double strokeThickness = 3)
        {
            string title = trainingsInRow.ToString();

            var color = trainingDataAccuracy.DefaultColors[(trainingsInRow - 1) % trainingDataAccuracy.DefaultColors.Count];
            OxyColor fill = OxyColor.FromArgb(transparancy, color.R, color.G, color.B);

            var trainingDataAccuracySeries = new AreaSeries();
            trainingDataAccuracySeries.Title = title;
            trainingDataAccuracySeries.Fill = fill;
            trainingDataAccuracySeries.StrokeThickness = strokeThickness;

            var trainingDataCostSeries = new AreaSeries();
            trainingDataCostSeries.Title = title;
            trainingDataCostSeries.Fill = fill;
            trainingDataCostSeries.StrokeThickness = strokeThickness;

            var testingDataAccuracySeries = new AreaSeries();
            testingDataAccuracySeries.Title = title;
            testingDataAccuracySeries.Fill = fill;
            testingDataAccuracySeries.StrokeThickness = strokeThickness;

            var testingDataCostSeries = new AreaSeries();
            testingDataCostSeries.Title = title;
            testingDataCostSeries.Fill = fill;
            testingDataCostSeries.StrokeThickness = strokeThickness;

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
                double y = monitorData["trainingDataAccuracy"];
                populateModel(trainingDataAccuracy, x, y, trainingIndex, GraphType.Accuracy);
            }
            if (monitorTrainingCost)
            {
                double y = monitorData["trainingDataCost"];
                populateModel(trainingDataCost, x, y, trainingIndex, GraphType.Cost);
            }
            if (monitorTestingAccuracy)
            {
                double y = monitorData["testingDataAccuracy"];
                populateModel(testingDataAccuracy, x, y, trainingIndex, GraphType.Accuracy);

            }
            if (monitorTestingCost)
            {
                double y = monitorData["testingDataCost"];
                populateModel(testingDataCost, x, y, trainingIndex, GraphType.Cost);

            }

            updateZoom(trainingDataAccuracy, accuracyMin, accuracyMax, 5);
            updateZoom(trainingDataCost, costMin, costMax, 0.2);
            updateZoom(testingDataAccuracy, accuracyMin, accuracyMax, 5);
            updateZoom(testingDataCost, costMin, costMax, 0.2);

            updateGraphs();
        }

        void populateModel(PlotModel model, double x, double y, int trainingIndex, GraphType type)
        {
            LineSeries series = (LineSeries)model.Series[trainingIndex];
            series.Points.Add(new DataPoint(x, y));

            if (type == GraphType.Accuracy)
            {
                if (y != 0)
                    accuracyMin = Math.Min(y, accuracyMin);
                accuracyMax = Math.Max(y, accuracyMax);
            }
            else
            {
                if (y != 0)
                    costMin = Math.Min(y, costMin);
                costMax = Math.Max(y, costMax);
            }
        }

        void updateZoom(PlotModel model, double min, double max, double padding = 5)
        {
            min = min == double.MaxValue ? 0 : min;

            foreach (var axis in model.Axes)
            {
                if (axis.Position != AxisPosition.Left)
                    continue;

                axis.Minimum = min - padding;
                axis.Maximum = max + padding;
            }
        }

        #endregion

        #endregion
    }
}
