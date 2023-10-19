using System;
using System.Numerics;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using NAudio.Utils;

namespace DigitalMusicAnalysis
{
    public class timefreq
    {
        public float[][] timeFreqData;
        public int wSamp;
        public Complex[] twiddles;
        public int NUM_OF_PROCESSOR = Environment.ProcessorCount;

        public timefreq(float[] x, int windowSamp) //taking big array as an input, 48k samples per second for the entier sound
        {

            int ii;
            double pi = 3.14159265;
            Complex i = Complex.ImaginaryOne;
            this.wSamp = windowSamp;
            twiddles = new Complex[wSamp];


            Stopwatch basicParallelStopTime = new Stopwatch();

             basicParallelStopTime.Start();
            Parallel.For(0, wSamp, ii =>
            {
                double a = 2 * pi * ii / (double)wSamp;
                twiddles[ii] = Complex.Pow(Complex.Exp(-i), (float)a);

            });

             basicParallelStopTime.Stop();
             string exapmpestring = $"exmaple time in milliseconds {basicParallelStopTime.ElapsedMilliseconds}\n ";
             Debug.WriteLine(exapmpestring);
            //-------------------------------------------------------------------------------------------------------------//


            // using MaxDegreeofParallelism

            /*  Stopwatch useMaxDegreeOfParallelism = new Stopwatch();
             useMaxDegreeOfParallelism.Start();
             Parallel.For(0, wSamp, new ParallelOptions { MaxDegreeOfParallelism = NUM_OF_PROCESSOR }, ii =>
             {
                 double a = 2 * pi * ii / (double)wSamp;
                 twiddles[ii] = Complex.Pow(Complex.Exp(-i), (float)a);

             });
             useMaxDegreeOfParallelism.Stop();
             string exapmpestring = $"exmaple time in milliseconds {useMaxDegreeOfParallelism.ElapsedMilliseconds}\n ";
             Debug.WriteLine(exapmpestring);*/
            //-------------------------------------------------------------------------------------------------------------//


            //sequentil version 
            /*Stopwatch sequentialVersion = new Stopwatch();
            sequentialVersion.Start();
             for (ii = 0; ii < wSamp; ii++)
            {
                double a = 2 * pi * ii / (double)wSamp;
                twiddles[ii] = Complex.Pow(Complex.Exp(-i), (float)a);
            }
            sequentialVersion.Stop();
            string sequentialTimeLog = $"exmaple time in milliseconds {sequentialVersion.ElapsedMilliseconds}\n ";
            Debug.WriteLine(sequentialTimeLog);*/
            //-------------------------------------------------------------------------------------------------------------//


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

            Stopwatch stftStopWatch = new Stopwatch();
            stftStopWatch.Start();
            timeFreqData = stft(compX, wSamp);
            stftStopWatch.Stop();

            string benchmarkFile = @"C:\Users\steph\OneDrive - Queensland University of Technology\Documents\CAB 401\benchmark3.txt";
            string seconds = $"STFT in secods: {stftStopWatch.Elapsed.TotalSeconds}\n ";
            string milliseconds = $"STFT in milliseconds: {stftStopWatch.ElapsedMilliseconds}\n ";

            using (StreamWriter sw = File.AppendText(benchmarkFile)) //records bench mark specs
            {
                sw.WriteLine(seconds);
                sw.WriteLine(milliseconds);
            }

            Debug.WriteLine($"STFT time: {stftStopWatch.ElapsedMilliseconds}");

        }

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
        Parallel.For(0, 2 * (int)Math.Floor((double)N / (double)wSamp) - 1, ii =>
        {
            Complex[] temp = new Complex[wSamp];
            Complex[] tempFFT = new Complex[wSamp];
            float localFftMax = 0;  // Create a local maximum for this thread

            for (int jj = 0; jj < wSamp; jj++)
            {
                temp[jj] = x[ii * (wSamp / 2) + jj];
            }

            tempFFT = fft(temp);

            for (int kk = 0; kk < wSamp / 2; kk++)
            {
                Y[kk][ii] = (float)Complex.Abs(tempFFT[kk]);

                if (Y[kk][ii] > localFftMax)
                {
                    localFftMax = Y[kk][ii];
                }
            }

            // Use a lock to update the global maximum 'fftMax' safely
            lock (Y)
            {
                if (localFftMax > fftMax)
                {
                    fftMax = localFftMax;
                }
            }
        });

        // Normalize the results after all processing is complete
        Parallel.For(0, 2 * (int) Math.Floor((double)N / (double)wSamp) - 1, ii =>
        {
            for (int kk = 0; kk < wSamp / 2; kk++)
            {
                Y[kk][ii] /= fftMax;
            }
        });

        return Y;
    }
            

            // sequential version 

            /*int ii = 0;
            int jj = 0;
            int kk = 0;

            int N = x.Length;
            float fftMax = 0;

            // Create an array to store the STFT results
            float[][] Y = new float[wSamp / 2][];

            for (int ll = 0; ll < wSamp / 2; ll++)
            {
                Y[ll] = new float[2 * (int)Math.Floor((double)N / (double)wSamp)];
            }

            Complex[] temp = new Complex[wSamp];
            Complex[] tempFFT = new Complex[wSamp];

            // break up the the entire sound into little part 
            //when ii=0 we do the first 2480 sample

            for (ii = 0; ii < 2 * Math.Floor((double)N / (double)wSamp) - 1; ii++)
            {

                for (jj = 0; jj < wSamp; jj++)
                {
                    temp[jj] = x[ii * (wSamp / 2) + jj];
                }

                tempFFT = fft(temp);

                for (kk = 0; kk < wSamp / 2; kk++)
                {
                    Y[kk][ii] = (float)Complex.Abs(tempFFT[kk]);

                    if (Y[kk][ii] > fftMax)
                    {
                        fftMax = Y[kk][ii];
                    }
                }


            }

            for (ii = 0; ii < 2 * Math.Floor((double)N / (double)wSamp) - 1; ii++)
            {
                for (kk = 0; kk < wSamp / 2; kk++)
                {
                    Y[kk][ii] /= fftMax;
                }
            }

            return Y;
        }*/



        Complex[] fft(Complex[] x)
        {
            int ii = 0;
            int kk = 0;
            int N = x.Length;

            Complex[] Y = new Complex[N];

            // NEED TO MEMSET TO ZERO?

            if (N == 1)
            {
                Y[0] = x[0];
            }
            else
            {

                Complex[] E = new Complex[N / 2];
                Complex[] O = new Complex[N / 2];
                Complex[] even = new Complex[N / 2];
                Complex[] odd = new Complex[N / 2];

                for (ii = 0; ii < N; ii++)
                {

                    if (ii % 2 == 0)
                    {
                        even[ii / 2] = x[ii];
                    }
                    if (ii % 2 == 1)
                    {
                        odd[(ii - 1) / 2] = x[ii];
                    }
                }

                E = fft(even);
                O = fft(odd);

                for (kk = 0; kk < N; kk++)
                {
                    Y[kk] = E[(kk % (N / 2))] + O[(kk % (N / 2))] * twiddles[kk * wSamp / N];
                }
            }

            return Y;
        }

    }
}
