using SymbolRecognitionLib.InversionOfControl;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Media.Imaging;

namespace SymbolRecognitionLib.ViewModels
{
    public enum FormattingMode
    {
        Append,
        CreateNew
    }

    public class SymbolsRecognitionViewModel : BaseViewModel
    {
        #region Bindnded Data

        public Command ProcessUserInput { get; private set; }
        public Command ClearCanvas { get; private set; }
        public Command OnCanvasLoaded { get; private set; }
        public Command OnOutputBoxLoaded { get; private set; }
        public Command ChangeBrush { get; private set; }
        public Command SaveLabeledData { get; private set; }
        public Command ClearLabeledData { get; private set; }

        RichTextBox outputTextBox;
        InkCanvas canvas;
        DrawingAttributes brush;
        public DrawingAttributes Brush
        {
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

        bool isProcessing;
        public bool IsProcessing
        {
            get
            {
                return isProcessing;
            }

            set
            {
                isProcessing = value;
                OnPropertyChanged(nameof(IsProcessing));
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

        List<double[]> userTrainingData = new List<double[]>();
        List<double[]> userLabeldResults = new List<double[]>();

        public SymbolsRecognitionViewModel()
        {
            // Init commands
            ProcessUserInput = new Command(processUserInput);
            ClearCanvas = new Command(clearCanvas);
            OnCanvasLoaded = new Command(onCanvasLoaded);
            OnOutputBoxLoaded = new Command(onOutputBoxLoaded);
            SaveLabeledData = new Command(saveLabeledData);
            ClearLabeledData = new Command(clearLabeledData);
        }

        #region Command handlers

        #region Image Processor

        void processUserInput(object obj)
        {
            if (isProcessing)
                return;

            Task.Run(() =>
            {
                try
                {
                    IsProcessing = true;

                    formatTextOutput("", System.Windows.Media.Brushes.White, FormattingMode.CreateNew);

                    Bitmap canvas = renderCanvasToBitmap();

                    var locator = new SymbolsLocator(canvas);
                    var borderBoxes = locator.Process();

                    if (showBorderBoxes)
                        drawBorderBoxes(borderBoxes);

                    Bitmap[] symbols = extractSymbols(canvas, borderBoxes);

                    symbols = clearSymbols(symbols, borderBoxes);

                    if (classifyResults)
                        labelResults(symbols);

                    symbols = resizeShapes(symbols);

                    foreach (var item in symbols)
                    {
                        item.DrawInConsole();
                    }
                    double[][] inputs = convertToNeuralNetworkInputs(symbols);

                    double[][] outputs = ApplicationService.GetNeuralNetwork.Evaluate(inputs);
                    processOutputs(outputs);
                }
                finally
                {
                    IsProcessing = false;
                }
            });
        }

        Bitmap renderCanvasToBitmap()
        {
            int height = (int)canvas.ActualHeight - 2;
            int width = (int)canvas.ActualWidth - 2;

            return Application.Current.Dispatcher.Invoke(() =>
            {
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

            });

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

            Application.Current.Dispatcher.Invoke(() =>
            {
                canvas.Strokes.Add(strokes);
            });
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

                int scrXPosition = (int)shape.TopLeftCorner.X;
                int scrYPosition = (int)shape.TopLeftCorner.Y;

                var symbolBmp = new Bitmap(shape.Width, shape.Height);

                using (var gr = Graphics.FromImage(symbolBmp))
                {
                    gr.DrawImage(sourceImage, new System.Drawing.Rectangle(0, 0, shape.Width, shape.Height), new System.Drawing.Rectangle(scrXPosition, scrYPosition, shape.Width, shape.Height), GraphicsUnit.Pixel);
                }

                symbols[i] = symbolBmp;
            }

            return symbols;
        }

        Bitmap[] clearSymbols(Bitmap[] symbols, List<Rectangle> borderBoxes)
        {
            for (int i = 0; i < symbols.Length; i++)
            {
                var symbol = symbols[i];
                var shape = borderBoxes[i];

                for (int row = 0; row < symbol.Height; row++)
                {
                    for (int col = 0; col < symbol.Width; col++)
                    {
                        if (!shape.IsContainsPoint(new Point(col, row) + shape.TopLeftCorner))
                            symbol.SetPixel(col, row, Color.White);
                    }
                }
            }

            return symbols;
        }

        Bitmap[] resizeShapes(Bitmap[] symbols)
        {
            for (int symbol = 0; symbol < symbols.Length; symbol++)
            {
                var symbol2020 = symbols[symbol].CropToSize(20, 20);
                var centroid = GetMassCenterOffset(symbol2020);

                var bmp2828 = new Bitmap(28, 28);

                using (var brush = new SolidBrush(Color.White))
                using (var gfx2828 = Graphics.FromImage(bmp2828))
                {
                    gfx2828.FillRectangle(brush, 0, 0, bmp2828.Width, bmp2828.Height);
                    gfx2828.DrawImage(symbol2020, 4 - (int)centroid.X, 4 - (int)centroid.Y);
                }

                symbols[symbol] = bmp2828;
            }

            return symbols;
        }

        public Point GetMassCenterOffset(Bitmap bitmap)
        {
            var path = new List<Vector2>();
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    var brightness = bitmap.GetPixel(x, y).GetBrightness();
                    if (brightness < 1)
                        path.Add(new Vector2(x, y));
                }
            }
            var centroid = path.Aggregate(Vector2.Zero, (current, point) => current + point) / path.Count();

            return new Point((int)centroid.X - bitmap.Width / 2, (int)centroid.Y - bitmap.Height / 2);
        }

        void labelResults(Bitmap[] symbols, float angle = 15, int offset = 30, float scaleFactor = 1.6F)
        {
            if (string.IsNullOrEmpty(expectedResults) || (expectedResults.Length != symbols.Length && expectedResults.Length != 1))
            {
                ExpectedResults = "";
                return;
            }

            for (int i = 0; i < symbols.Length; i++)
            {
                List<Bitmap> trainingData = new List<Bitmap>(45);
                var symbol = symbols[i].AddPadding(8, 8);

                trainingData.Add(symbol);

                var rotatedRight = symbol.Rotate(-angle);
                var rotatedLeft = symbol.Rotate(angle);

                trainingData.Add(rotatedRight);
                trainingData.Add(rotatedLeft);

                addDistortions(symbol, ref trainingData, offset, scaleFactor);
                addDistortions(rotatedRight, ref trainingData, offset, scaleFactor);
                addDistortions(rotatedLeft, ref trainingData, offset, scaleFactor);

                Bitmap[] compressedTrainingData = resizeShapes(trainingData.ToArray());

                foreach (var item in convertToNeuralNetworkInputs(compressedTrainingData))
                    userTrainingData.Add(item);

                for (int j = 0; j < compressedTrainingData.Length; j++)
                {
                    compressedTrainingData[j].Save("images/image" + j +".jpeg");
                }

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

            foreach (var item in expectedOutputs)
                userLabeldResults.Add(item);

            ExpectedResults = "";

            Application.Current.Dispatcher.Invoke(() =>
            {
                clearCanvas(null);
            });

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

                double[] input = new double[28 * 28];

                for (int row = 0; row < symbol.Height; row++)
                    for (int col = 0; col < symbol.Width; col++)
                    {
                        int pixel = row * 28 + col;
                        double brightness = symbol.GetPixel(col, row).GetBrightness();
                        // Invert and convert to neural network input (0, 2.55) where 0 white, 1 black
                        brightness = (1 - brightness) * 2.55;
                        input[pixel] = brightness;
                    }

                inputs[i] = input;
            }

            return inputs;
        }

        void processOutputs(double[][] outputs)
        {
            System.Windows.Media.Brush brush;

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

                if (maxVal > 0.8)
                    brush = System.Windows.Media.Brushes.Green;
                else
                    brush = System.Windows.Media.Brushes.Red;

                formatTextOutput(resultNum.ToString(), brush, FormattingMode.Append);
            }
        }

        void formatTextOutput(string text, System.Windows.Media.Brush brush, FormattingMode mode)
        {
            TextRange rangeOfWord;

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (mode == FormattingMode.Append)
                    rangeOfWord = new TextRange(outputTextBox.Document.ContentEnd, outputTextBox.Document.ContentEnd);
                else
                {
                    outputTextBox.SelectAll();
                    outputTextBox.Selection.Text = "";
                    rangeOfWord = new TextRange(outputTextBox.Document.ContentStart, outputTextBox.Document.ContentStart);
                }

                rangeOfWord.Text = text;
                rangeOfWord.ApplyPropertyValue(TextElement.ForegroundProperty, brush);
            });

        }

        double[][] convertToNeuralNetworkOutput(int[] num, int repetitions)
        {
            double[][] outputs = new double[num.Length * repetitions][];

            for (int i = 0; i < num.Length; i++)
            {
                double[] output = new double[ApplicationService.GetNeuralNetwork.Layers.Last().OutputsCount];

                output[num[i]] = 1;

                for (int j = 0; j < repetitions; j++)
                {
                    outputs[i * repetitions + j] = output;
                }
            }

            return outputs;
        }
        #endregion

        void saveLabeledData(object obj)
        {
            string filePath = FileManager.OpenSaveFileDialog(initialDirectory: @"C:\work\C#\Neural Network\NeuralNetwork2");

            if (string.IsNullOrEmpty(filePath))
                return;

            // Delete extensions
            filePath = Regex.Replace(filePath, @"\.\w+", "");

            FileManager.SaveFile(userLabeldResults.ToArray(), filePath + ".nlabl");
            FileManager.SaveFile(userTrainingData.ToArray(), filePath + ".ninp");
            clearLabeledData(null);
        }

        void clearLabeledData(object obj)
        {
            userLabeldResults = new List<double[]>();
            userTrainingData = new List<double[]>();
            LabeledDataCount = "0";
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

        void onOutputBoxLoaded(object obj)
        {
            outputTextBox = (RichTextBox)obj;
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
