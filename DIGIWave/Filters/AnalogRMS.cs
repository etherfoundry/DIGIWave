using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DIGIWave.Filters
{
    class AnalogRMS<T> where T : IComparable
    {
        public int RMSListSize { get; set; }
        Queue<double> input = new Queue<double>();
        Signals.AnalogSignal<double> FilterOutput = Signals.AnalogSignal.CreateDoubleSignal();
        
        public AnalogRMS()
        {
            RMSListSize = 100;
        }

        public bool HasOutput
        {
            get { return FilterOutput.Samples.Count > 0;  }
        }

        public void SampleInput(T val, long TimeOffset)
        {
            SampleInput(new Sample<T> { TimeOffset = TimeOffset, value = val });
        }

        public void SampleInput(Sample<T> sample)
        {
            double sval = Convert.ToDouble(sample.value);
            input.Enqueue(sval);
            if(input.Count > RMSListSize)
            {
                FilterOutput.Samples.Enqueue(
                    new Sample<double>
                    {
                        TimeOffset = sample.TimeOffset,
                        value = CalculateRMS(sval)
                    });
                input.Dequeue();
            }
        }

        public Sample<double> SampleOutput()
        {
            return FilterOutput.Samples.Dequeue();
        }

        protected double accumulation;
        
        protected double CalculateRMS(double newval)
        {
            //double accumulation = 0;
            // calculate RMS
            //foreach (var f in input)
            //{
            //    accumulation += (f*f); // f^2
            //}
            if (accumulation != 0)
            {
                accumulation += newval * newval;
                double pval = input.Peek();
                accumulation -= pval*pval;
            }
            else
            {
                foreach (var f in input)
                    accumulation += (f*f); // f^2
            }
            
            double result = Math.Sqrt((1d / input.Count) * accumulation);
            
            return result;
        }

        /*
        static Func<T, T, T> multiplyFn;
        static double Multiply(T a, T b)
        {
            if(multiplyFn == null)
            {
                //TODO: re-use delegate!
                // declare the parameters
                ParameterExpression paramA = Expression.Parameter(typeof(T), "a"),
                    paramB = Expression.Parameter(typeof(T), "b");
                // add the parameters together
                BinaryExpression body = Expression.Multiply(paramA, paramB);
                // compile it
                ///Func<T, T, T> 
                    multiplyFn = Expression.Lambda<Func<T, T, T>>(body, paramA, paramB).Compile();
            }
            
            // call it
            return Convert.ToDouble(multiplyFn(a, b));
        }*/
    }
}
