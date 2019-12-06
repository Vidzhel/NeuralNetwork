using NeuralNetworkLib;
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


        NeuralNetwork neuralNetwork;
            
        public SymbolsRecognitionViewModel()
        {
            // Init commands
            ProcessUserInput = new Command(processUserInput);
            ClearCanvas = new Command(clearCanvas);
            OnCanvasLoaded = new Command(onCanvasLoaded);

            // Init Neural Network
            neuralNetwork = NeuralNetwork.Load("symbolsRecognitionNetwork.net");

            //double[][] trainingImages;
            //double[][] testingImages;

            //double[][] trainingLables;
            //double[][] testingLabels;
            //MNISTDataLoader.PrepeareData("..//..//..//MNISTDataset", out trainingImages, out testingImages, out trainingLables, out testingLabels, 10, 10);
            //foreach (var img in trainingImages)
            //{
            //    MNISTDataLoader.ConvertImageToBitmap(img).DrawInConsole();
            //}
        }

        #region Command handlers

        #region Processor

        void processUserInput(object obj)
        {
            Output = "";
            Bitmap canvas = renderCanvasToBitmap();

            var locator = new SymbolsLocator(canvas);
            var borderBoxes = locator.Process();

            if (showBorderBoxes)
                drawBorderBoxes(borderBoxes);

            Bitmap[] symbols = extractSymbols(canvas, borderBoxes, 5);
            double[][] inputs = convertToNeuralNetworkInputs(symbols);

            double[][] outputs = neuralNetwork.Evaluate(inputs);
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

        Bitmap[] extractSymbols(Bitmap sourceBmp, List<Rectangle> borderBox, int padding)
        {
            System.Drawing.Imaging.PixelFormat format = sourceBmp.PixelFormat;
            System.Drawing.Image sourceImage = (System.Drawing.Image)sourceBmp.Clone();

            Bitmap[] symbols = new Bitmap[borderBox.Count];

            for (int i = 0; i < borderBox.Count; i++)
            {
                var shape = borderBox[i];

                var size = Math.Max(shape.Width, shape.Height) + 2 * padding;

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
                        // 
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
