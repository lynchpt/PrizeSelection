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

        #region Deprecated
        //IDictionary<int, double> GetDefaultPullProbabilityTable(int bannerRelicCount, double pullCategoryRelicAcquisitionRate);

        //IDictionary<int, double> GetPullProbabilityTable(
        //    IList<PrizeCategorySpecification> pullProbabilitySpecifications,
        //    IDictionary<int, double> targetPullCategoryProbabilityTable);

        //bool IsPullProbabilityTableValid(IDictionary<int, double> pullProbabilityTable);

        #endregion
    }

    public class PrizeSelectionTableHelper : IPrizeSelectionTableHelper
    {
        #region Class Variables
        IResultsFormatter _resultsFormatter;
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

        #region Deprecated
        ////assumes even distribution of relics across the probability space
        ////lower indexes are "higher up" in the table, and require larger numbers to hit
        ////the value part of the dictionary (the double) represents the lower bound of the random number needed to hit this relic
        //public IDictionary<int, double> GetDefaultPullProbabilityTable(int bannerRelicCount, double pullCategoryRelicAcquisitionRate)
        //{
        //    IDictionary<int, double> pullCategoryProbabilityTable = new Dictionary<int, double>(bannerRelicCount);

        //    if (bannerRelicCount <= 0)
        //    {
        //        throw new ArgumentException("bannerRelicCount must be greater than 0");
        //    }
        //    if (pullCategoryRelicAcquisitionRate < 0.0 || pullCategoryRelicAcquisitionRate > 1.0)
        //    {
        //        throw new ArgumentException($"pullCategoryRelicAcquisitionRate must be between 0 and 1");
        //    }

        //    double probabilityIncrement = pullCategoryRelicAcquisitionRate / bannerRelicCount;

        //    for (int relicIndex = 0; relicIndex < bannerRelicCount; relicIndex++)
        //    {
        //        pullCategoryProbabilityTable.Add(relicIndex + 1, 1 - (probabilityIncrement * (relicIndex + 1))); //we start from the top prize and work down the table
        //    }

        //    return pullCategoryProbabilityTable;
        //}

        ////if targetPullCategoryProbabilityTable is not null, the data from pullProbabilitySpecifications will be added to the bottom
        //public IDictionary<int, double> GetPullProbabilityTable(IList<PrizeCategorySpecification> pullProbabilitySpecifications, IDictionary<int, double> targetPullCategoryProbabilityTable)
        //{
        //    #region validations
        //    if (pullProbabilitySpecifications == null || !pullProbabilitySpecifications.Any())
        //    {
        //        throw new ArgumentException("pullProbabilitySpecifications must be greater non null and have 1 or more members");
        //    }
        //    if (pullProbabilitySpecifications.Any(ps => ps.ProbabilityExtentForEntireCategory < 0.0) || pullProbabilitySpecifications.Any(ps => ps.ProbabilityExtentForEntireCategory > 1.0))
        //    {
        //        throw new ArgumentException($"pullCategoryRelicAcquisitionRate must be between 0 and 1");
        //    }
        //    if (pullProbabilitySpecifications.Any(ps => ps.PrizeCount <= 0))
        //    {
        //        throw new ArgumentException($"PrizeCount must be between greater than 0");
        //    }
        //    #endregion

        //    double initialLowerBound = 0.0; //this is the point from which we start adding other probability ranges to the bottom.
        //    int initialMaxRelicKey = 0; //this is the point from which we start adding other probability keys to the bottom.
        //    IDictionary<int, double> pullCategoryProbabilityTable = new Dictionary<int, double>(); //will get replaced if another table is passed in

        //    if (targetPullCategoryProbabilityTable != null)
        //    {
        //        //we need our lower bound to be from the bottom of the passed in probability table
        //        initialLowerBound = targetPullCategoryProbabilityTable.OrderBy(pcpt => pcpt.Key).Last().Value;
        //        initialMaxRelicKey = targetPullCategoryProbabilityTable.OrderBy(pcpt => pcpt.Key).Last().Key;
        //        pullCategoryProbabilityTable = targetPullCategoryProbabilityTable;
        //    }
        //    else
        //    {
        //        initialLowerBound = 1.0; //this is the point from which we start adding other probability ranges to the bottom.
        //    }

        //    foreach (var pullProbabilitySpecification in pullProbabilitySpecifications)
        //    {
        //        double probabilityIncrement = pullProbabilitySpecification.ProbabilityExtentForEntireCategory / pullProbabilitySpecification.PrizeCount;
        //        for (int relicIndex = 0; relicIndex < pullProbabilitySpecification.PrizeCount; relicIndex++)
        //        {
        //            pullCategoryProbabilityTable.Add(relicIndex + initialMaxRelicKey + 1, initialLowerBound - (probabilityIncrement * (relicIndex + 1))); //we start from the top prize and work down the table
        //        }

        //        initialLowerBound = pullCategoryProbabilityTable.OrderBy(pcpt => pcpt.Key).Last().Value;
        //        initialMaxRelicKey = pullCategoryProbabilityTable.OrderBy(pcpt => pcpt.Key).Last().Key;
        //    }

        //    return pullCategoryProbabilityTable;
        //}
        //probabilities must monotonically decrease as you go down the table
        //public bool IsPullProbabilityTableValid(IDictionary<int, double> pullProbabilityTable)
        //{
        //    bool isValid = true;

        //    int tableSize = pullProbabilityTable.Keys.Count;
        //    for (int key = 1; key <= tableSize; key++)
        //    {
        //        double pullProbability = pullProbabilityTable[key];

        //        if (pullProbability >= 1.0 || pullProbability < 0)
        //        {
        //            isValid = false;
        //        }

        //        if (key > 1)
        //        {
        //            double pullProbabilityPrevious = pullProbabilityTable[key - 1];
        //            if (pullProbabilityPrevious <= pullProbability)
        //            {
        //                isValid = false;
        //            }
        //        }
        //    }

        //    return isValid;
        //}

        #endregion
    }
}
