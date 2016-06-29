using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIGIWave.Filters
{
    class ThresholdTrigger<T> where T : IComparable
    {
        public T Threshold;
        public int Tolerance = 0;
         
        int state;
        Event LastState = 0;
        Signals.DigitalSignal FilterOutput = new Signals.DigitalSignal();

        //Queue<Sample<bool>> FilterOutput = new Queue<Sample<bool>>();

        enum Event
        {
            NotSet = 0,
            CrossAbove = 1,
            CrossBelow = 2,
            //Cross = 3,
        }

        public bool HasOutput
        {
            get { return FilterOutput.Samples.Count > 0; }
        }

        public void SampleInput(T val, long TimeOffset)
        {
            SampleInput(new Sample<T> { TimeOffset = TimeOffset, value = val });
        }
        protected double _uthresh = double.NaN, _lthresh = double.NaN;
        protected double UpperThreshold {
            get
            {
                if(double.IsNaN(_uthresh))
                    _uthresh = Convert.ToDouble(Threshold) + Tolerance;
                return _uthresh;
            }
        }

        protected double LowerThreshold
        {
            get
            {
                if (double.IsNaN(_lthresh))
                    _lthresh = Convert.ToDouble(Threshold) - Tolerance;
                return _lthresh;
            }
        }

        public void SampleInput(Sample<T> sample)
        {
            int above = 0, below = 0;

            above = sample.value.CompareTo(UpperThreshold);
            if(above != 0)
                below = sample.value.CompareTo(LowerThreshold);

            if(LastState != Event.NotSet)
            {
                Event e = LastState;
                if (above > 0) e = Event.CrossAbove;
                else if (below < 0) e = Event.CrossBelow;
                if(e != Event.NotSet && LastState != e)
                {
                     bool sampleValue = (e == Event.CrossAbove ? true : false);
                    AddOutputSample(sampleValue, sample.TimeOffset);
                    LastState = e;
                }
            }
            else
            {
                Event e = LastState;
                if (above > 0) e = Event.CrossAbove;
                else if (below < 0) e = Event.CrossBelow;
                LastState = e;
            }
        }

        public Sample<bool> SampleOutput()
        {
            return FilterOutput.Samples.Dequeue();
        }

        protected void AddOutputSample(bool val, long TimeOffset)
        {

            FilterOutput.Samples.Enqueue(
                    new Sample<bool>
                    {
                        TimeOffset = TimeOffset,
                        value = val
                    }
                );
        }
    }
}
