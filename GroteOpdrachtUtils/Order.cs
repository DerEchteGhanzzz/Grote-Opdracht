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
        public readonly int matrixID;

        public Order(int frequency, float emptyTime, int containerVol, int containerCount, int orderID, int matrixID)
        {
            this.frequency = frequency;
            this.emptyTime = emptyTime;
            this.containerVol = containerVol;
            this.containerCount = containerCount;
            this.orderID = orderID;
            this.matrixID = matrixID;
        }

        public override string ToString()
        {
            return orderID.ToString();
        }
    }

    public class Depot : Order
    {
        public Depot() base : (-1, 30, 0, 0, 0, 287)
        { 

        }
    }

}
