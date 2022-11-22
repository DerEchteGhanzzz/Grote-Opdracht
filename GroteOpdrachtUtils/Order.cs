using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroteOpdrachtUtils
{
    public class Order
    {
        public readonly int frequency;
        public readonly float emptyTime;
        public readonly int containerVol;
        public readonly int containerCount;
        public readonly int orderID;

        public Order(int frequency, float emptyTime, int containerVol, int containerCount, int orderID)
        {
            this.frequency = frequency;
            this.emptyTime = emptyTime;
            this.containerVol = containerVol;
            this.containerCount = containerCount;
            this.orderID = orderID;
        }

        public override string ToString()
        {
            return orderID.ToString();
        }
    }
}
