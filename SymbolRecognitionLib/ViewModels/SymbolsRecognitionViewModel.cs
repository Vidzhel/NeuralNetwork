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
        public Command ProcessUserInput { get; private set; }
        public Command ClearCanvas { get; private set; }
        public Command OnCanvasLoaded { get; private set; }
        public Command ChangeBrush { get; private set; }

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

        string expectedResults;
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

        List<double[][]> userTrainingData = new List<double[][]>();
        List<double[][]> userLabledResults = new List<double[][]>();

        public SymbolsRecognitionViewModel()
        {
            // Init commands
            ProcessUserInput = new Command(processUserInput);
            ClearCanvas = new Command(clearCanvas);
            OnCanvasLoaded = new Command(onCanvasLoaded);

            // Init Neural Network
            //neuralNetwork = NeuralNetwork.Load("symbolsRecognitionNetwork.net");
            //TestNeuralNetwork.Test();
            //double[][] trainingInputs;
            //double[][] testingInputs;

            //double[][] trainingLables;
            //double[][] testingLabels;
            //MNISTDataLoader.PrepeareData("..//..//..//MNISTDataset", out trainingInputs, out testingInputs, out trainingLables, out testingLabels, 10, 10);
            //foreach (var img in trainingInputs)
            //{
            //    MNISTDataLoader.ConvertImageToBitmap(img).Rotate(15).DrawInConsole();
            //}
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
        /// Extracts symbols from the source bitmap and places them into the square of appropriate size
        /// </summary>
        Bitmap[] extractSymbols(Bitmap sourceBmp, List<Rectangle> borderBox)
        {
            System.Drawing.Imaging.PixelFormat format = sourceBmp.PixelFormat;
            System.Drawing.Image sourceImage = (System.Drawing.Image)sourceBmp.Clone();

            Bitmap[] symbols = new Bitmap[borderBox.Count];

            for (int i = 0; i < borderBox.Count; i++)
            {
                var shape = borderBox[i];

                var size = Math.Max(shape.Width, shape.Height);

                int horizontalPadding = (size - shape.Width) / 2;
                int verticalPadding = (size - shape.Height) / 2;

                int scrXPosition = shape.TopLeftCorner.X;
                int scrYPosition = shape.TopLeftCorner.Y;

                var symbolBmp = new Bitmap(size, size);

                using (var gr = Graphics.FromImage(symbolBmp))
                using (var brush = new SolidBrush(Color.White))
                {
                    gr.FillRectangle(brush, 0, 0, size, size);
                    gr.DrawImage(sourceImage, new System.Drawing.Rectangle(horizontalPadding, verticalPadding, shape.Width, shape.Height), new System.Drawing.Rectangle(scrXPosition, scrYPosition, shape.Width, shape.Height), GraphicsUnit.Pixel);
                }

                symbols[i] = (Bitmap)symbolBmp.GetThumbnailImage(28, 28, null, IntPtr.Zero);
            }

            return symbols;
        }

        void labelResults(Bitmap[] symbols, float angle = 15, int offset = 10, float scaleFactor = 2)
        {
            if (expectedResults.Length != symbols.Length && expectedResults.Length != 1)
                throw new Exception();

            for (int i = 0; i < symbols.Length; i++)
            {
                List<Bitmap> trainingData = new List<Bitmap>(42);
                var symbol = symbols[i];

                trainingData.Add(symbol);

                var rotatedRight = symbol.Rotate(-angle);
                var rotatedLeft = symbol.Rotate(angle);

                trainingData.Add(rotatedRight);
                trainingData.Add(rotatedLeft);

                addDistortions(symbol, ref trainingData, offset, scaleFactor);
                addDistortions(rotatedRight, ref trainingData, offset, scaleFactor);
                addDistortions(rotatedLeft, ref trainingData, offset, scaleFactor);

                userTrainingData.Add(convertToNeuralNetworkInputs(trainingData.ToArray()));
            }

            int[] num = new int[symbols.Length];
            double[][] expectedOutputs;

            if (expectedResults.Length == symbols.Length)
            {
                for (int symbol = 0; symbol < symbols.Length; symbol++)
                    num[symbol] = int.Parse(new string(expectedResults[symbol], 1));

                expectedOutputs = convertToNeuralNetworkOutput(num, 42);
            }
            else
            {

            }

            //userLabledResults.Add(expectedOutputs);
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
                symbol.DrawInConsole();

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
