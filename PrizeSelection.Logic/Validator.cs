using System;
using System.Collections.Generic;
using System.Text;

namespace PrizeSelection.Logic
{
    public interface IValidator
    {
        bool IsPullProbabilityTableValid(IDictionary<int, double> pullProbabilityTable);
    }

    public class Validator
    {
        //probabilities must monotonically decrease as you go down the table
        public bool IsPullProbabilityTableValid(IDictionary<int, double> pullProbabilityTable)
        {
            bool isValid = true;

            int tableSize = pullProbabilityTable.Keys.Count;
            for (int key = 1; key <= tableSize; key++)
            {
                double pullProbability = pullProbabilityTable[key];

                if (pullProbability >= 1.0 || pullProbability < 0)
                {
                    isValid = false;
                }

                if (key > 1)
                {
                    double pullProbabilityPrevious = pullProbabilityTable[key - 1];
                    if (pullProbabilityPrevious <= pullProbability)
                    {
                        isValid = false;
                    }
                }
            }

            return isValid;
        }
    }
}
