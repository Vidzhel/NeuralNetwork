# NeuralNetwork

Digits recognition library that is written in C#. During the development I followed [this resource](http://neuralnetworksanddeeplearning.com/)

## Walk through

When you open the application, you will see that the main window is divided into two parts. The left bar is responsible for downloading, storing, creating and displaying the main information of the neural network.

![Main menu](https://i.imgur.com/JIcTdfs.png)

We can see that the right part is inactive, so that it works you need to create or load a neural network. To open a file with a saved network, you need to click "Load Neural Net" and then select the file with the extension ".net". To create a neural network, you must first write the number of neurons on each of the layers in the “Topology” field, which must be at least 3, then select the error and activation functions and click “Create”. After creating or loading a neural network, the most important information will appear, as well as the right part will be activated

![Main menu with loaded project](https://i.imgur.com/mm2IVJ9.png)

The first of the two available tabs allows us to test the loaded neural network.

![Test of neural network](https://i.imgur.com/5BFajlB.png)
![Processing of an image](https://i.imgur.com/oEq0iMB.png)

After drawing a number or several digits, we can click "Process" and the result will appear. The result can be three colors: green, yellow, red. The color depends on the confidence of the neural network in the result, but this does not mean that the result is correct.

Also in this window we can find additional features, such as tracing the found numbers with a rectangle (used for debugging) or classification of results, we can enter the correct answers (without spaces) in the appropriate field that appears after we check the appropriate box, further, if we click to process again, we will see that the index of classified data will increase, and the button to save this data will be activated. We will be able to use the classified data later when we train our neural network.

![Show bounder boxes](https://i.imgur.com/7mcLQN4.png)
![Classify results](https://i.imgur.com/LctqyZ7.png)

On the neural network training tab, we can find quite a few parameters that in one way or another affect the results. The minimum you need to do to start training is to load the training files with input data and shortcuts to them, then the "Train" button is activated. We can also select the amount of data we want to load, or specify "all" if all the data is needed, the number of generations. This is followed by the training speed factor, the adjustment factor, the number of copies of data in the portion, the accuracy threshold, the flags of the activation of graphs. Next, if desired, we can load the data for the tests, determine their number.

!["Train Network" tab](https://i.imgur.com/og2tLjG.png)

After the start of the training, the information will appear on the graphs, of course, if we check the appropriate boxes, the image will automatically scale to the lowest and highest values. After running the training several times, you will see that the graphs are not erased, but overlap each other, which is very convenient for comparison. Click “Clear Graphs” to clear the graphs. During training, the “Cancel” button becomes active, which cancels the training and returns to the previous version of the neural network. It is also possible to return even after the training is over, you ne

![Training stats on the graphs](https://i.imgur.com/FJtqejy.png)
![Overlapping stats on the graphs](https://i.imgur.com/W3xIIWb.png)
