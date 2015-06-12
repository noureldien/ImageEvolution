using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Threading;
using System.Windows.Controls;
using OpenCvSharp;
using OpenCvSharp.Blob;
using System.Runtime.InteropServices;
using OpenCvSharp.CPlusPlus;

namespace ImageEvolution
{
    /// <summary>
    /// Responsible of Image Processing.
    /// </summary>
    public class Tracker
    {
        #region Private Variables

        private readonly int screenHeight = System.Windows.Forms.SystemInformation.VirtualScreen.Height;
        private readonly int screenWidth = System.Windows.Forms.SystemInformation.VirtualScreen.Width;
        private readonly int GenerationNo = 100;

        private int timerIntervalTime = 30;
        private int counter = 0;
        private Label labelFrameCounter;

        private bool timerInProgress = false;
        private System.Windows.Forms.Timer fpsTimer;
        private Thread workingThread;
        private Random random;
        private IplImage[] children;

        private IplImage frame;
        private IplImage grayFrame;
        private IplImage parent;
        private CvWindow window1;
        private CvWindow window2;
        private CvSize size;

        #endregion

        #region Constructor

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="labelFrameCounter"></param>
        public Tracker(Label labelFrameCounter)
        {
            this.labelFrameCounter = labelFrameCounter;
            Initialize();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// used to dispose any object created from this class
        /// </summary>
        public void Dispose()
        {
            if (workingThread.IsAlive)
            {
                workingThread.Abort();
            }

            if (workingThread != null)
            {
                workingThread = null;
            }

            if (window1 != null)
            {
                window1.Close();
                window1.Dispose();
                window1 = null;
            }

            if (window2 != null)
            {
                window2.Close();
                window2.Dispose();
                window2 = null;
            }
        }

        /// <summary>
        /// Start mainThread, that starts tracking
        /// </summary>
        public void StartProcessing()
        {
            workingThread.Start();
            fpsTimer.Start();
            timerInProgress = true;
        }

        /// <summary>
        /// Stop mainThread, that stops tracking
        /// </summary>
        public void StopProcessing()
        {
            if (workingThread.IsAlive)
            {
                workingThread.Abort();
            }
            fpsTimer.Stop();
            timerInProgress = false;
        }

        /// <summary>
        /// If processing or not.
        /// </summary>
        public bool IsProcessing()
        {
            return timerInProgress;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Initialize Camera, timer and some objects.
        /// </summary>
        private void Initialize()
        {
            // for generating random numbers
            random = new Random();
            children = new IplImage[GenerationNo];

            // initialize mainTimer
            workingThread = new Thread(new ThreadStart(Process));

            // initialize timer used to count frames per seconds of the camera
            fpsTimer = new System.Windows.Forms.Timer();
            fpsTimer.Interval = 1000;
            fpsTimer.Tick += new EventHandler((object obj, EventArgs eventArgs) =>
            {
                labelFrameCounter.Content = counter.ToString();
            });

            // windows to view what's going on
            window1 = new CvWindow("Camera 1", WindowMode.KeepRatio);

            string imagePath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Images\Monalisa.png");
            frame = new IplImage(imagePath);
            size = new CvSize(frame.Width, frame.Height);

            int offset = 10;
            window1.Resize(size.Width, size.Height);
            window1.Move(screenWidth - (size.Width + offset) * 1, 20);

            window2 = new CvWindow("Camera 2", WindowMode.KeepRatio);
            window2.Resize(size.Width, size.Height);
            window2.Move(screenWidth - (size.Width + offset) * 2, 20);

            grayFrame = new IplImage(size, BitDepth.U8, 1);
            Cv.CvtColor(frame, grayFrame, ColorConversion.BgrToGray);
            parent = new IplImage(size, BitDepth.U8, 1);

            // show image on the separate window
            window1.Image = grayFrame;
        }

        /// <summary>
        /// Image Processing. It is done using OpenCVSharp Library.
        /// </summary>
        private void Process()
        {
            // increment counter
            counter++;

            // new generation
            Generation(parent);

            // best fit
            IplImage child = this.BestChild(grayFrame, children);

            // as a parent
            parent = child;

            // show the new parent
            window2.Image = parent;

            // recursive
            Process();
        }

        /// <summary>
        /// Create new generation.
        /// </summary>
        /// <param name="image"></param>
        private void Generation(IplImage image)
        {
            for (int i = 0; i < GenerationNo; i++)
            {
                // random no of vertixes
                int vertixes = (int)((4.0 * random.NextDouble()) + 1);

                // genrate points
                OpenCvSharp.CPlusPlus.Point[] points = new OpenCvSharp.CPlusPlus.Point[vertixes];
                for (int j = 0; j < vertixes; j++)
                {
                    int y = (int)(image.Height * random.NextDouble());
                    int x = (int)(image.Width * random.NextDouble());

                    points[j] = new OpenCvSharp.CPlusPlus.Point(x, y);
                }

                // color scale
                byte colorInt = (byte)(255.0 * random.NextDouble());
                Scalar color = new Scalar(colorInt);

                List<List<OpenCvSharp.CPlusPlus.Point>> polygon = new List<List<OpenCvSharp.CPlusPlus.Point>>();
                polygon.Add(points.ToList());

                // draw polygon
                children[i] = image.Clone();
                Cv2.FillPoly(Cv2.CvArrToMat(children[i]), polygon, color);
            }
        }

        /// <summary>
        /// Find best fit in the generation.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="children"></param>
        /// <returns></returns>
        private IplImage BestChild(IplImage parent, IplImage[] children)
        {
            double[] distances = new double[GenerationNo];

            for (int i = 0; i < GenerationNo; i++)
            {
                distances[i] = Cv2.Norm(Cv2.CvArrToMat(parent), Cv2.CvArrToMat(children[i]));
            }

            int index = Array.IndexOf(distances, distances.Min());
            return children[index];
        }

        #endregion
    }
}