using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace hopfield_network
{
    public partial class Form1 : Form
    {
        private Bitmap bm;
        private const string samplesPath = @"../../Samples/";
        private const string filesToClassify = @"../../Distorted/";
        private int countOfSamples, vecSize;
        private int[][] X_marix;
        private double[,] W_matrix;
        private List<string> sampleNames;
        private const int iterations = 100;
        private const int e = 10;

        public Form1()
        {
            InitializeComponent();
            bm = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            sampleNames = new List<string>();
            LoadSamples();
            TrainNetwork();
        }

        private void LoadSamples()
        {
            string[] files = Directory.GetFiles(samplesPath);
            countOfSamples = files.Count();
            X_marix = new int[countOfSamples][];

            for (int i = 0; i < countOfSamples; i++)
            {
                Bitmap b = new Bitmap(files[i]);
                X_marix[i] = new int[b.Width * b.Height];

                sampleNames.Add(files[i].Substring(files[i].Length - 5, 1));

                for (int j = 0; j < b.Height; j++)
                    for (int k = 0; k < b.Width; k++)
                        X_marix[i][(j * b.Width) + k] = b.GetPixel(k, j).ToArgb() == 0 ? -1 : 1;
            }
        }

        private void TrainNetwork()
        {
            vecSize = X_marix[0].Length;
            W_matrix = new double[vecSize, vecSize];

            for (int i = 0; i < vecSize; i++)
            {
                for (int j = 0; j < vecSize; j++)
                {
                    if (i == j)
                        W_matrix[i, j] = 0;
                    else
                    {
                        double sum = 0;

                        // Hebb learning
                        for (int k = 0; k < countOfSamples; k++)
                            sum += X_marix[k][i] * X_marix[k][j];

                        W_matrix[i, j] = sum / vecSize;
                    }
                }
            }
        }

        private int[] GetVector()
        {
            int[] vector = new int[vecSize];

            for (int j = 0; j < bm.Height; j++)
                for (int i = 0; i < bm.Width; i++)
                    vector[(j * bm.Width) + i] = bm.GetPixel(i, j).ToArgb() == 0 ? -1 : 1;

            return vector;
        }

        private int[] GetArrayCopy(int[] arr)
        {
            int[] res = new int[arr.Length];

            for (int i = 0; i < arr.Length; i++)
                res[i] = arr[i];

            return res;
        }

        private bool EquivalenceCheck(int[] arr1, int[] arr2)
        {
            if (arr1.Length != arr2.Length)
                return false;

            for (int i = 0; i < arr1.Length; i++)
                if (arr1[i] != arr2[i])
                    return false;

            return true;
        }

        private bool ErrorGuard(int[] arr1, int[] arr2)
        {
            int errorCount = 0;

            if (arr1.Length != arr2.Length)
                return false;

            for (int i = 0; i < arr1.Length; i++)
            {
                if (arr1[i] != arr2[i])
                    errorCount++;

                if (errorCount > e)
                    return false;
            }

            return true;
        }

        private void RecognizeVector(int[] y)
        {
            for (int iter = 0; iter < iterations; iter++)
            {
                int[] oldY = GetArrayCopy(y);

                for (int j = 0; j < vecSize; j++)
                {
                    double sum = 0;

                    for (int i = 0; i < vecSize; i++)
                        sum += W_matrix[i, j] * y[i];

                    // Activation function
                    y[j] = Math.Sign(sum);
                }

                if (EquivalenceCheck(y, oldY))
                    break;
            }

            for (int k = 0; k < countOfSamples; k++)
            {
                if (ErrorGuard(y, X_marix[k]))
                {
                    MessageBox.Show(sampleNames[k] + " is Recognised", "Result", MessageBoxButtons.OK);

                    Bitmap b = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                    Graphics g = Graphics.FromImage(b);
                    pictureBox1.Image = b;

                    for (int i = 0; i < y.Length; i++)
                    {
                        if (y[i] == 1)
                        {
                            Rectangle rect = new Rectangle((i % 10) * 20, (i / 10) * 20, 20, 20);
                            g.FillRectangle(Brushes.Black, rect);
                        }
                    }

                    g.Dispose();
                    pictureBox1.Invalidate();

                    return;
                }
            }

            MessageBox.Show("Unknown Letter", "Result", MessageBoxButtons.OK);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = Path.Combine(Directory.GetParent(Directory.GetParent(
                   Directory.GetCurrentDirectory()).ToString()).ToString(), "Distorted");

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                bm = new Bitmap(openFileDialog1.FileName);
                Bitmap b = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                Graphics g = Graphics.FromImage(b);
                pictureBox1.Image = b;

                for (int j = 0; j < bm.Height; j++)
                {
                    for (int i = 0; i < bm.Width; i++)
                    {
                        if (bm.GetPixel(i, j).ToArgb() != 0)
                        {
                            Rectangle rect = new Rectangle(i * 20, j * 20, 20, 20);
                            g.FillRectangle(Brushes.Black, rect);
                        }
                    }
                }

                g.Dispose();
                pictureBox1.Invalidate();
            }
        }

        private void recognizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RecognizeVector(GetVector());
        }
    }
}
