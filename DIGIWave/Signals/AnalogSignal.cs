using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIGIWave.Signals
{
    class AnalogSignal<T> where T : IComparable
    {
        public Queue<Sample<T>> Samples;

        public AnalogSignal()
        {
            Samples = new Queue<Sample<T>>();
        }

        
    }

    class AnalogSignal
    {
        #region Factories
        public static AnalogSignal<int> CreateIntSignal()
        {
            return new AnalogSignal<int>();
        }

        public static AnalogSignal<float> CreateFloatSignal()
        {
            return new AnalogSignal<float>();
        }

        public static AnalogSignal<double> CreateDoubleSignal()
        {
            return new AnalogSignal<double>();
        }
        #endregion
    }
}
