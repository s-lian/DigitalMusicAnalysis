using System;
using System.Numerics;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using NAudio.Utils;
using FFTW.NET;

namespace DigitalMusicAnalysis
{
    public class timefreq
    {
        public float[][] timeFreqData;
        public int wSamp;
        public Complex[] twiddles;
        public int NUM_OF_PROCESSOR = 8;


        public timefreq(float[] x, int windowSamp) //taking big array as an input, 48k samples per second for the entier sound
        {

            int ii;
            double pi = 3.14159265;
            Complex i = Complex.ImaginaryOne;
            this.wSamp = windowSamp;
            twiddles = new Complex[wSamp];

            //sequentil version 

            for (ii = 0; ii < wSamp; ii++)
            {
                double a = 2 * pi * ii / (double)wSamp;
                twiddles[ii] = Complex.Pow(Complex.Exp(-i), (float)a);
            }


            timeFreqData = new float[wSamp / 2][];

            int nearest = (int)Math.Ceiling((double)x.Length / (double)wSamp);
            nearest = nearest * wSamp;

            Complex[] compX = new Complex[nearest];
            for (int kk = 0; kk < nearest; kk++)
            {
                if (kk < x.Length)
                {
                    compX[kk] = x[kk]; // copy the the array of floating point we pass in into complex array
                }
                else
                {
                    compX[kk] = Complex.Zero;
                }
            }

            int cols = 2 * nearest / wSamp;
            for (int jj = 0; jj < wSamp / 2; jj++)
            {
                timeFreqData[jj] = new float[cols];
            }


            timeFreqData = stft(compX, wSamp);


            float[][] stft(Complex[] x, int wSamp)
            {


                int N = x.Length;
                float fftMax = 0;

                // Create an array to store the STFT results
                float[][] Y = new float[wSamp / 2][];




                for (int ll = 0; ll < wSamp / 2; ll++)
                {
                    Y[ll] = new float[2 * (int)Math.Floor((double)N / (double)wSamp)];
                }

                // Parallelize the processing of different sections of the input data
                Parallel.For(0, 2 * (int)Math.Floor((double)N / (double)wSamp) - 1, new ParallelOptions { MaxDegreeOfParallelism = NUM_OF_PROCESSOR }, ii =>
            {
                Complex[] temp = new Complex[wSamp];
                Complex[] tempFFT = new Complex[wSamp];
                float localFftMax = 0;  // Create a local maximum for this thread

                for (int jj = 0; jj < wSamp; jj++)
                {
                    temp[jj] = x[ii * (wSamp / 2) + jj];
                }

                tempFFT = fft(temp, tempFFT);

                for (int kk = 0; kk < wSamp / 2; kk++)
                {
                    Y[kk][ii] = (float)Complex.Abs(tempFFT[kk]);

                    if (Y[kk][ii] > localFftMax)
                    {
                        localFftMax = Y[kk][ii];
                    }
                }



                if (localFftMax > fftMax)
                {
                    fftMax = localFftMax;
                }

            });

                // Normalize the results after all processing is complete
                Parallel.For(0, 2 * (int)Math.Floor((double)N / (double)wSamp) - 1, new ParallelOptions { MaxDegreeOfParallelism = NUM_OF_PROCESSOR }, ii =>
            {
                for (int kk = 0; kk < wSamp / 2; kk++)
                {
                    Y[kk][ii] /= fftMax;
                }
            });

               
                return Y;
            }

           


            Complex[] fft(Complex[] temp, Complex[] tempFFT)
            {
                using (var pinIn = new PinnedArray<Complex>(temp))
                using (var pinOut = new PinnedArray<Complex>(tempFFT))
                {
                    DFT.FFT(pinIn, pinOut); // computes the fft using the FFTW algorithm instead of the recursive algorithm present, DFT.FFT is a C# wrapper that calls DLL's that contain the FFTW code
                }


                return tempFFT;
            }

        }
    }
}
