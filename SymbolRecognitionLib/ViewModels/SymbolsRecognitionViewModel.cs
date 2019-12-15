using NeuralNetworkLib;
using SymbolRecognitionLib.InversionOfControl;
using SymbolsRecognitionLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media.Imaging;

namespace SymbolRecognitionLib.ViewModels
{
    public class SymbolsRecognitionViewModel : BaseViewModel
    {
        #region Bindnded Data

        public Command ProcessUserInput { get; private set; }
        public Command ClearCanvas { get; private set; }
        public Command OnCanvasLoaded { get; private set; }
        public Command ChangeBrush { get; private set; }
        public Command SaveLabeledData { get; private set; }

        InkCanvas canvas;
        DrawingAttributes brush;
        public DrawingAttributes Brush {
            get
            {
                return brush;
            }
            set
            {
                brush = value;
                OnPropertyChanged(nameof(Brush));
            }
        }

        string output;
        public string Output {
            get
            {
                return output;
            }

            set
            {
                output = value;
                OnPropertyChanged(nameof(Output));
            }
        }

        string expectedResults = "";
        public string ExpectedResults
        {
            get
            {
                return expectedResults;
            }

            set
            {
                expectedResults = value;
                OnPropertyChanged(nameof(ExpectedResults));
            }
        }

        Regex onlyNumbers = new Regex(@"^\d+$");
        int brushSize = 10;
        public string BrushSize
        {
            get
            {
                return brushSize.ToString();
            }
            set
            {
                if (onlyNumbers.IsMatch(value))
                    brushSize = int.Parse(value);
                else
                    brushSize = 5;

                OnPropertyChanged(nameof(BrushSize));
                changeBrush(null);
            }
        }

        bool isBrush = true;
        public bool IsBrush
        {
            get
            {
                return isBrush;
            }
            set
            {
                isBrush = value;
                OnPropertyChanged(nameof(IsBrush));
                changeBrush(null);
            }
        }

        bool showBorderBoxes = false;
        public bool ShowBorderBoxes
        {
            get
            {
                return showBorderBoxes;
            }
            set
            {
                showBorderBoxes = value;
                OnPropertyChanged(nameof(ShowBorderBoxes));
            }
        }

        bool classifyResults = false;
        public bool ClassifyResults
        {
            get
            {
                return classifyResults;
            }

            set
            {
                classifyResults = value;
                OnPropertyChanged(nameof(ClassifyResults));
            }
        }


        string labeledDataCount = "0";
        public string LabeledDataCount
        {
            get
            {
                return labeledDataCount;
            }

            set
            {
                labeledDataCount = value;
                OnPropertyChanged(nameof(LabeledDataCount));
            }
        }
        #endregion

        List<double[][]> userTrainingData = new List<double[][]>();
        List<double[][]> userLabeldResults = new List<double[][]>();

        public SymbolsRecognitionViewModel()
        {
            // Init commands
            ProcessUserInput = new Command(processUserInput);
            ClearCanvas = new Command(clearCanvas);
            OnCanvasLoaded = new Command(onCanvasLoaded);
            SaveLabeledData = new Command(saveLabeledData);

            double[][] trainingInputs;
            double[][] testingInputs;

            double[][] trainingLabels;
            double[][] testingLabels;
            MNISTDataLoader.PrepeareData("..//..//..//MNISTDataset", out trainingInputs, out testingInputs, out trainingLabels, out testingLabels, 10, 10);
            foreach (var img in trainingInputs)
            {
                MNISTDataLoader.ConvertImageToBitmap(img).DrawInConsole();
            }
        }

        #region Command handlers

        #region Image Processor

        void processUserInput(object obj)
        {
            Output = "";
            Bitmap canvas = renderCanvasToBitmap();

            var locator = new SymbolsLocator(canvas);
            var borderBoxes = locator.Process();

            if (showBorderBoxes)
                drawBorderBoxes(borderBoxes);

            Bitmap[] symbols = extractSymbols(canvas, borderBoxes);

            if (classifyResults)
                labelResults(symbols);

            symbols = compressShapes(symbols);
            double[][] inputs = convertToNeuralNetworkInputs(symbols);

            double[][] outputs = ApplicationService.GetNeuralNetwork.Evaluate(inputs);
            processOutputs(outputs);
        }

        Bitmap renderCanvasToBitmap()
        {
            int height = (int)canvas.ActualHeight - 2;
            int width = (int)canvas.ActualWidth - 2;

            RenderTargetBitmap renderer = new RenderTargetBitmap(width, height, 96.0, 96.0, System.Windows.Media.PixelFormats.Default);

            renderer.Render(canvas);
            Bitmap canvasBmp;

            using (MemoryStream stream = new MemoryStream())
            {
                BitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderer));
                encoder.Save(stream);

                canvasBmp = new Bitmap(stream);
                // Because the stream bitmap exists only when the stream is open, we copy the bitmap in order to safely close the stream
                canvasBmp = new Bitmap(canvasBmp);
            }

            return canvasBmp;
        }

        void drawBorderBoxes(List<Rectangle> borderBoxes)
        {
            StrokeCollection strokes = new StrokeCollection();
            DrawingAttributes brush = new DrawingAttributes();
            brush.Color = System.Windows.Media.Colors.Red;

            foreach (var box in borderBoxes)
            {
                var topLeftPoint = new System.Windows.Input.StylusPoint(box.TopLeftCorner.X, box.TopLeftCorner.Y);
                var bottomRightPoint = new System.Windows.Input.StylusPoint(box.BottomRightCorner.X, box.BottomRightCorner.Y);
                var topRightPoint = new System.Windows.Input.StylusPoint(box.BottomRightCorner.X, box.TopLeftCorner.Y);
                var bottomLeftPoint = new System.Windows.Input.StylusPoint(box.TopLeftCorner.X, box.BottomRightCorner.Y);

                var topBorder = new System.Windows.Input.StylusPointCollection();
                topBorder.Add(topLeftPoint);
                topBorder.Add(topRightPoint);

                var bottomBorder = new System.Windows.Input.StylusPointCollection();
                bottomBorder.Add(bottomLeftPoint);
                bottomBorder.Add(bottomRightPoint);

                var leftBorder = new System.Windows.Input.StylusPointCollection();
                leftBorder.Add(topLeftPoint);
                leftBorder.Add(bottomLeftPoint);

                var rightBorder = new System.Windows.Input.StylusPointCollection();
                rightBorder.Add(topRightPoint);
                rightBorder.Add(bottomRightPoint);

                var topStroke = new Stroke(topBorder);
                topStroke.DrawingAttributes = brush;

                var bottomStroke = new Stroke(bottomBorder);
                bottomStroke.DrawingAttributes = brush;

                var leftStroke = new Stroke(leftBorder);
                leftStroke.DrawingAttributes = brush;

                var rightStroke = new Stroke(rightBorder);
                rightStroke.DrawingAttributes = brush;

                strokes.Add(topStroke);
                strokes.Add(bottomStroke);
                strokes.Add(leftStroke);
                strokes.Add(rightStroke);
            }

            canvas.Strokes.Add(strokes);
        }

        /// <summary>
        /// Extracts symbols from the source bitmap
        /// </summary>
        Bitmap[] extractSymbols(Bitmap sourceBmp, List<Rectangle> borderBoxes)
        {
            System.Drawing.Imaging.PixelFormat format = sourceBmp.PixelFormat;
            System.Drawing.Image sourceImage = (System.Drawing.Image)sourceBmp.Clone();

            Bitmap[] symbols = new Bitmap[borderBoxes.Count];

            for (int i = 0; i < borderBoxes.Count; i++)
            {
                var shape = borderBoxes[i];

                int scrXPosition = shape.TopLeftCorner.X;
                int scrYPosition = shape.TopLeftCorner.Y;

                var symbolBmp = new Bitmap(shape.Width, shape.Height);

                using (var gr = Graphics.FromImage(symbolBmp))
                using (var brush = new SolidBrush(Color.White))
                {
                    gr.FillRectangle(brush, 0, 0, shape.Width, shape.Height);
                    gr.DrawImage(sourceImage, new System.Drawing.Rectangle(0, 0, shape.Width, shape.Height), new System.Drawing.Rectangle(scrXPosition, scrYPosition, shape.Width, shape.Height), GraphicsUnit.Pixel);
                }

                symbols[i] = symbolBmp;
            }

            return symbols;
        }

        Bitmap[] compressShapes(Bitmap[] symbols, int padding = 3)
        {
            for (int symbol = 0; symbol < symbols.Length; symbol++)
            {
                symbols[symbol] = ((Bitmap)symbols[symbol].GetThumbnailImage(28 - padding*2, 28 - padding*2, null, IntPtr.Zero)).AddPadding(padding, padding);
            }

            return symbols;
        }

        void labelResults(Bitmap[] symbols, float angle = 15, int offset = 10, float scaleFactor = 2)
        {
            if (string.IsNullOrEmpty(expectedResults) || ( expectedResults.Length != symbols.Length && expectedResults.Length != 1))
            {
                ExpectedResults = "";
                return;
            }
                

            for (int i = 0; i < symbols.Length; i++)
            {
                List<Bitmap> trainingData = new List<Bitmap>(42);
                var symbol = symbols[i].AddPadding(8, 8);

                trainingData.Add(symbol);

                var rotatedRight = symbol.Rotate(-angle);
                var rotatedLeft = symbol.Rotate(angle);

                trainingData.Add(rotatedRight);
                trainingData.Add(rotatedLeft);

                addDistortions(symbol, ref trainingData, offset, scaleFactor);
                addDistortions(rotatedRight, ref trainingData, offset, scaleFactor);
                addDistortions(rotatedLeft, ref trainingData, offset, scaleFactor);

                Bitmap[] compressedTrainingData = compressShapes(trainingData.ToArray());
                userTrainingData.Add(convertToNeuralNetworkInputs(compressedTrainingData));

                foreach (var item in compressedTrainingData)
                {
                    item.DrawInConsole();
                }
            }

            int[] num = new int[symbols.Length];
            double[][] expectedOutputs;

            if (expectedResults.Length == symbols.Length)
            {
                for (int symbol = 0; symbol < symbols.Length; symbol++)
                    num[symbol] = int.Parse(new string(expectedResults[symbol], 1));

                expectedOutputs = convertToNeuralNetworkOutput(num, 45);
            }
            else
            {
                expectedOutputs = convertToNeuralNetworkOutput(new int[] { expectedResults[0] }, 45 * symbols.Length);
            }
            TODO check user labels
            userLabeldResults.Add(expectedOutputs);

            ExpectedResults = "";
            clearCanvas(null);

            LabeledDataCount = userTrainingData.Count.ToString();
        }

        void addDistortions(Bitmap symbol, ref List<Bitmap> storage, int offset, float scaleFactor)
        {
            storage.Add(symbol.Scale(scaleFactor, 1));
            storage.Add(symbol.Scale(1, scaleFactor));

            var skewHorizontalyBottom = symbol.SkewHorizontaly(offset, HorizonlSkew.Bottom);
            var skewHorizontalyTop = symbol.SkewHorizontaly(offset, HorizonlSkew.Top);

            storage.Add(skewHorizontalyBottom);
            storage.Add(skewHorizontalyTop);

            var skewVerticalyLeft = symbol.SkewVeritcaly(offset, VerticalSkew.Left);
            var skewVerticalyRight = symbol.SkewVeritcaly(offset, VerticalSkew.Right);

            storage.Add(skewVerticalyLeft);
            storage.Add(skewVerticalyRight);


            storage.Add(skewHorizontalyBottom.Scale(scaleFactor, 1));
            storage.Add(skewHorizontalyBottom.Scale(1, scaleFactor));

            storage.Add(skewHorizontalyTop.Scale(scaleFactor, 1));
            storage.Add(skewHorizontalyTop.Scale(1, scaleFactor));

            storage.Add(skewVerticalyLeft.Scale(scaleFactor, 1));
            storage.Add(skewVerticalyLeft.Scale(1, scaleFactor));

            storage.Add(skewVerticalyRight.Scale(scaleFactor, 1));
            storage.Add(skewVerticalyRight.Scale(1, scaleFactor));
        }

        double[][] convertToNeuralNetworkInputs(Bitmap[] symbols)
        {

            double[][] inputs = new double[symbols.Length][];

            for (int i = 0; i < symbols.Length; i++)
            {
                var symbol = symbols[i];
                //symbol.DrawInConsole();

                double[] input = new double[28 * 28];

                for (int row = 0; row < symbol.Height; row++)
                    for (int col = 0; col < symbol.Width; col++)
                    {
                        int pixel = row * 28 + col;
                        double pixelBrightness = symbol.GetPixel(col, row).GetBrightness();

                        // Inverse all white pixels to black
                        // Convert to appropriate format (0 - white, 2.55 - black)
                        input[pixel] = pixelBrightness == 1 ? 0 : (1 - pixelBrightness) * 2.55;
                    }
                
                inputs[i] = input;
            }

            return inputs;
        }

        void processOutputs(double[][] outputs)
        {
            foreach (var output in outputs)
            {
                double maxVal = double.MinValue;
                int resultNum = 0;

                for (int value = 0; value < output.Length; value++)
                    if (output[value] > maxVal)
                    {
                        maxVal = output[value];
                        resultNum = value;
                    }

                Output += resultNum;
            }
        }
        
        double[][] convertToNeuralNetworkOutput(int[] num, int repetitions)
        {
            double[][] outputs = new double[num.Length * repetitions][];

            for (int i = 0; i < num.Length; i++)
            {
                double[] output = new double[ApplicationService.GetNeuralNetwork.Layers[0].InputsCount];

                output[num[i]] = 1;

                for (int j = 0; j < repetitions; j++)
                {
                    outputs[i*repetitions + j] = output;
                }
            }

            return outputs;
        }
        #endregion

        void saveLabeledData(object obj)
        {
            string filePath = FileManager.OpenSaveFileDialog();

            if (string.IsNullOrEmpty(filePath))
                return;

            FileManager.SaveFile(userLabeldResults, filePath + ".nlabl");
            FileManager.SaveFile(userTrainingData, filePath + ".ninp");
        }

        void clearCanvas(object obj)
        {
            canvas.Strokes = new StrokeCollection();
        }

        void onCanvasLoaded(object obj)
        {
            canvas = (InkCanvas)obj;
            changeBrush(null);
        }

        void changeBrush(object obj)
        {
            var brush = new DrawingAttributes();
            brush.Color = isBrush ? System.Windows.Media.Colors.Black : System.Windows.Media.Colors.White;
            brush.Height = brushSize;
            brush.Width = brushSize;

            Brush = brush;
        }
        #endregion
    }
}
