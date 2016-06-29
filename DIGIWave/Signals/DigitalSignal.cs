using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIGIWave.Signals
{
    class DigitalSignal
    {
        public Queue<Sample<bool>> Samples;

        public DigitalSignal()
        {
            Samples = new Queue<Sample<bool>>();
        }
    }
}
