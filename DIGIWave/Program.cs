using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIGIWave
{
    class Program
    {
        static void Main(string[] args)
        {
            int sleft, sright;
            var wav = StereoWaveFile.OpenWave(@"E:\SDR_Sharp\SDRSharp_20160607_155817Z_315030000Hz_IQ.wav");
            //var state = new SignalState();

            Filters.AnalogRMS<int> filterRMS = new Filters.AnalogRMS<int>() { RMSListSize = 500 };
            Filters.ThresholdTrigger<double> filterTrigger = new Filters.ThresholdTrigger<double>() { Threshold = 5000, Tolerance = 500 };

            long scount = 0;
            while (!wav.EOF)
            {
                wav.readSample(out sleft, out sright);
                filterRMS.SampleInput(sleft,scount);
                if(filterRMS.HasOutput)
                {
                    var rmsOut = filterRMS.SampleOutput();
                    if (scount % 1000000 == 0) Console.WriteLine("Sample {0} - RMS Out: {1}", scount, rmsOut.value);

                    filterTrigger.SampleInput(rmsOut);
                    if (filterTrigger.HasOutput)
                    {
                        //Console.WriteLine("Sample {0} - TRIGGER ALERT: {1} - {2}", scount, filterTrigger.SampleOutput().value, rmsOut.value);
                    }
                }
                scount++;
            }

            Sample<bool> firstOfSet = null;
            int stateCount = 0;
            Sample<bool> s1 = null, s2 = null;
            while (filterTrigger.HasOutput)
            {
                s2 = filterTrigger.SampleOutput();
                if(firstOfSet == null)
                    firstOfSet = s2;

                if (s1 != null)
                {
                    if (s2.TimeOffset - s1.TimeOffset > 5000)
                    {
                        Console.WriteLine("Burst Time Span: {0}", (double)(s2.TimeOffset - firstOfSet.TimeOffset) / (double)wav.SampleRate);
                        Console.WriteLine("Time Span: {0} - {1}",
                            (double)firstOfSet.TimeOffset / (double)wav.SampleRate,
                            (double)s2.TimeOffset / (double)wav.SampleRate);
                        Console.WriteLine("Pulse Count: {0}", stateCount);
                        firstOfSet = null;
                        stateCount = 0;
                    }
                    else
                    {
                        stateCount++;
                    }
                }

                s1 = s2;
            }
            Console.ReadLine();
        }

        static void SampleAverageMain(string[] args)
        {
            int sleft, sright;
            var wav = StereoWaveFile.OpenWave(@"SDR_Sharp\SDRSharp_20160607_155903Z_315030000Hz_IQ.wav");
            //var state = new SignalState();

            int maxLeft = int.MinValue, maxRight = int.MinValue;
            float avgLeft = 0, avgRight = 0;
            int avgCount = 0;

            long cnt = 0;
            while(!wav.EOF)
            {
                wav.readSample(out sleft, out sright);
                if (sleft > maxLeft) maxLeft = sleft;
                if (sright > maxRight) maxRight = sright;
                avgCount++;
                avgLeft = ((avgLeft * avgCount-1) + sleft) / avgCount;
                avgRight = ((avgRight * avgCount) + sright) / avgCount;
                //if(state.AddSample(sleft))
                {
                    //Console.WriteLine("State Changed @ Sample {0} - Time: {1} - {2} - {3}", cnt, (float)cnt / (float)wav.SampleRate, Math.Round(state.lastRMS,4), Math.Round(state.lastAvgRMS,4));
                    //Console.WriteLine("State Changed @ Sample {0} - Time: {1} - {2} - {3} - {4}-  {5} - {6}", cnt, (float)cnt / (float)wav.SampleRate, Math.Round(state.maxSample,4), Math.Round(state.minSample,4), state.waveMaxAverage, state.waveMinAverage, state.waveSampleCount);
                }
                cnt++;
                if (cnt % 1000000 == 0) { Console.WriteLine("{0}/{1} - {2}%", wav.getFilePosition(), wav.getFileLength(), Math.Round(((float)wav.getFilePosition() / (float)wav.getFileLength()) * 100,2)); }
            }

            Console.WriteLine("L: Max: {0}, Avg: {1}", maxLeft, avgLeft);
            Console.WriteLine("R: Max: {0}, Avg: {1}", maxRight, avgRight);
            Console.ReadLine();
        }

        /*
        class SignalState
        {
            List<double> currentWaveSamples = new List<double>();
            
            List<double> waveMaxAverageList = new List<double>();
            List<double> waveMinAverageList = new List<double>();

            public bool positiveCross = false, negativeCross = false;
            public double maxSample, minSample, waveMaxAverage, waveMinAverage;
            public int waveSampleCount; 

            public bool AddSample(double sample)
            {
                positiveCross = positiveCross || sample > 0;
                negativeCross = negativeCross || sample < 0;
                
                if(positiveCross == false || negativeCross == false)
                {
                    // wave not complete
                    currentWaveSamples.Add(sample);
                    if(sample > maxSample) maxSample = sample;
                    if(sample < minSample) minSample= sample;
                    waveSampleCount++;
                    return false;
                }
                else
                {
                    waveMaxAverageList.Add(maxSample);
                    waveMinAverageList.Add(minSample);
                    
                    double wmaAvg = 0;
                    foreach(var wma in waveMaxAverageList) wmaAvg += wma;
                    wmaAvg /= waveMaxAverageList.Count;

                    double wmiaAvg = 0;
                    foreach(var wmi in waveMinAverageList) wmiaAvg += wmi;
                    wmiaAvg /= waveMinAverageList.Count;

                    currentWaveSamples.Clear();

                    return true;
                }
            }
        }*/

        /*
        class SignalState
        {
            Queue<double> curval = new Queue<double>();
            Queue<double> avgRMS = new Queue<double>();
            public int listSize = 10;
            public int avgListSize = 10;
            public float threshold = .50f;

            public double lastRMS { get; private set; }
            public double lastAvgRMS { get; private set; }

            State LastState = State.NotSet;

            enum State
            {
                NotSet, State1, State2
            }

            public bool AddSample(double val)
            {
                bool stateChanged = false; // result

                if (curval.Count == listSize)
                {
                    curval.Enqueue(val);
                    curval.Dequeue();
                }
                else
                {
                    curval.Enqueue(val);
                    return false;
                }
                double accumulation = 0;
                // calculate RMS
                foreach (var f in curval)
                {
                    accumulation += Math.Pow(f, 2);
                }

                double result = Math.Sqrt((1 / (float)curval.Count) * accumulation);
                avgRMS.Enqueue(result);

                if (avgRMS.Count < avgListSize)
                {
                    return false;
                }

                double curavg = 0;
                foreach (var rm in avgRMS)
                    curavg += rm;
                curavg /= avgRMS.Count;

                lastAvgRMS = curavg;
                lastRMS = result;


                double dev = result * threshold;

                bool changed = !((result - dev) < curavg && (result + dev) > curavg);

                if (changed) avgRMS.Clear();

                return changed;

                //State newState = State.NotSet;


                //if (above == true) newState = State.Above;
                //if (below == true) newState = State.Below;

                //if(LastState != State.NotSet)
                //{
                //    stateChanged = LastState != newState;
                //}

                //LastState = newState;                
                //return stateChanged;
            }
        }*/
    }
}
