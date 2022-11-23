using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroteOpdrachtUtils
{
    public class ScoreCalculations
    {

        public static float GetMaxCost(List<Node>[] orders)
        {
            float score = 0;
            foreach(var orderList in orders)
            {
                foreach (var order in orderList)
                {
                    score += 3 * order.emptyTime;
                }
            }
            return score;
        }

        public static float CalculateScore(List<Node>[,] solutionMatrix)
        {
            float score = 0;
            // bereken de score
            return score;
        }
    }
}
