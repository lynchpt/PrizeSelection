using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PrizeSelection.Model;

namespace PrizeSelection.Logic
{
    public interface IPrizeSelectionTableHelper
    {
        IList<PrizeSelectionRow> GetPrizeSelectionTable(IList<PrizeCategorySpecification> prizeCategorySpecifications);

        PrizeCategorySpecification CreatePrizeCategorySpecification(string prizeCategoryName, double prizeAcquisitionRateForPrizeCategory, int prizesInPrizeCategoryCount);

        PrizeCategorySpecification CreatePrizeCategorySpecification(string prizeCategoryName, double prizeAcquisitionRateForPrizeCategory, IList<string> prizeCategoryPrizeNames);

        bool IsPrizeSelectionTableValid(IList<PrizeSelectionRow> prizeSelectionTable);
    }

    public class PrizeSelectionTableHelper : IPrizeSelectionTableHelper
    {
        #region Class Variables

        readonly IResultsFormatter _resultsFormatter;
        #endregion

        #region Constructors

        public PrizeSelectionTableHelper(IResultsFormatter resultsFormatter)
        {
            _resultsFormatter = resultsFormatter;
        }
        #endregion

        //lower indexes are "higher up" in the table, and require larger numbers to hit
        //the value part of the dictionary (the double) represents the lower bound of the random number needed to hit this relic
        public IList<PrizeSelectionRow> GetPrizeSelectionTable(IList<PrizeCategorySpecification> prizeCategorySpecifications)
        {
            #region validations
            if (prizeCategorySpecifications == null || !prizeCategorySpecifications.Any())
            {
                throw new ArgumentException("prizeCategorySpecifications must be greater non null and have 1 or more members");
            }
            if (prizeCategorySpecifications.Any(ps => ps.ProbabilityExtentForEntireCategory < 0.0) || prizeCategorySpecifications.Any(ps => ps.ProbabilityExtentForEntireCategory > 1.0))
            {
                throw new ArgumentException($"prizeCategorySpecifications must be between 0 and 1");
            }
            if (prizeCategorySpecifications.Any(ps => ps.PrizeCount <= 0))
            {
                throw new ArgumentException($"PrizeCount must be between greater than 0");
            }
            #endregion

            int currentMaxPrizeIndex = 0; //this is the point from which we start adding other probability keys to the bottom.
            double currentProbabilityLowerBound = 1.0; //this is the point from which we start adding other probability ranges to the bottom.

            IList<PrizeSelectionRow> prizeSelectionTable = new List<PrizeSelectionRow>();


            foreach (var prizeCategorySpecification in prizeCategorySpecifications)
            {
                double probabilityIncrement = prizeCategorySpecification.ProbabilityExtentForEntireCategory / prizeCategorySpecification.PrizeCount;
                for (int counter = 0; counter < prizeCategorySpecification.PrizeCount; counter++)
                {
                    //pullCategoryProbabilityTable.Add(relicIndex + initialMaxRelicKey + 1, initialLowerBound - (probabilityIncrement * (relicIndex + 1))); //we start from the top prize and work down the table
                    PrizeSelectionRow row = new PrizeSelectionRow()
                                            {
                                                PrizeIndex = counter + currentMaxPrizeIndex + 1,
                                                PrizeProbabilityLowerBound = currentProbabilityLowerBound - (probabilityIncrement * (counter + 1)),
                                                PrizeCategoryName = prizeCategorySpecification.PrizeCategoryName,
                                                PrizeName = prizeCategorySpecification.PrizeNames[counter]
                                            };

                    prizeSelectionTable.Add(row);
                }

                currentProbabilityLowerBound = prizeSelectionTable.OrderBy(row => row.PrizeIndex).Last().PrizeProbabilityLowerBound;
                currentMaxPrizeIndex = prizeSelectionTable.OrderBy(row => row.PrizeIndex).Last().PrizeIndex;
            }

            //correct last row for 0 lower bound if that is what is clearly intended:
            AdjustFinalPrizeSelectionRow(prizeSelectionTable[currentMaxPrizeIndex - 1]);

            return prizeSelectionTable;
        }

        public PrizeCategorySpecification CreatePrizeCategorySpecification(string prizeCategoryName, double probabilityExtentForEntireCategory, int prizesInPrizeCategoryCount)
        {
            if (String.IsNullOrWhiteSpace(prizeCategoryName))
            {
                throw new ArgumentException($"prizeCategoryName must not be null or whitespace");
            }

            IList<string> prizeCategoryPrizeNames = _resultsFormatter.GeneratePrizeNamesList(prizesInPrizeCategoryCount, prizeCategoryName);

            PrizeCategorySpecification row = new PrizeCategorySpecification()
                                              {
                                                  PrizeCategoryName = prizeCategoryName,
                                                  ProbabilityExtentForEntireCategory = probabilityExtentForEntireCategory,
                                                  PrizeCount = prizesInPrizeCategoryCount,
                                                  PrizeNames = prizeCategoryPrizeNames
                                              };

            return row;
        }

        public PrizeCategorySpecification CreatePrizeCategorySpecification(string prizeCategoryName,
            double probabilityExtentForEntireCategory, IList<string> prizeCategoryPrizeNames)
        {
            if (String.IsNullOrWhiteSpace(prizeCategoryName))
            {
                throw new ArgumentException($"prizeCategoryName must not be null or whitespace");
            }
            if (prizeCategoryPrizeNames == null)
            {
                throw new ArgumentException($"prizeCategoryPrizeNames must not be null");
            }     
            if (!prizeCategoryPrizeNames.Any())
            {
                throw new ArgumentException($"prizeCategoryPrizeNames must have at least one entry");
            }


            PrizeCategorySpecification row = new PrizeCategorySpecification()
                                              {
                                                  PrizeCategoryName = prizeCategoryName,
                                                  ProbabilityExtentForEntireCategory = probabilityExtentForEntireCategory,
                                                  PrizeCount = prizeCategoryPrizeNames.Count(),
                                                  PrizeNames = prizeCategoryPrizeNames
                                              };

            return row;
        }

        public bool IsPrizeSelectionTableValid(IList<PrizeSelectionRow> prizeSelectionTable)
        {
            bool isValid = true;

            int prizeIndexPrior = 0;
            int prizeIndexCurrent = 1;

            double prizeProbabilityLowerBoundPrior = 1;
            double prizeProbabilityLowerBoundCurrent = 0;

            foreach (var prizeSelectionRow in prizeSelectionTable)
            {
                prizeProbabilityLowerBoundCurrent = prizeSelectionRow.PrizeProbabilityLowerBound;
                prizeIndexCurrent = prizeSelectionRow.PrizeIndex;

                //prize rows are sorted in index order, descending, and each index is exactly 1 higher
                if (prizeIndexCurrent != prizeIndexPrior + 1)
                {
                    isValid = false;
                }

                //probability lower bound in range
                if (prizeProbabilityLowerBoundCurrent >= 1.0 || prizeProbabilityLowerBoundCurrent < 0)
                {
                    isValid = false;
                }

                //probability lower bound monotonically decreasing as we work "down" the prize table
                if (prizeProbabilityLowerBoundPrior <= prizeProbabilityLowerBoundCurrent)
                {
                    isValid = false;
                }

                prizeIndexPrior++;
                prizeProbabilityLowerBoundPrior = prizeProbabilityLowerBoundCurrent;
            }

         

            return isValid;
        }

        #region Private Methods

        //due to the use of doubles, sometimes small rounding errors occur and a final prize selection row
        //that should have a prob lower bound of 0 will have one + or minus 1 x 10-10 or smaller.
        //we will replace that with true 0
        private void AdjustFinalPrizeSelectionRow(PrizeSelectionRow prizeSelectionRow)
        {
            if (Math.Abs(prizeSelectionRow.PrizeProbabilityLowerBound) < 0.0000000001)
            {
                prizeSelectionRow.PrizeProbabilityLowerBound = 0;
            }
        }
        #endregion
    }
}
