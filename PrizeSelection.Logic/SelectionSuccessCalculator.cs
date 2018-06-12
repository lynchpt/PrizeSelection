using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PrizeSelection.Model;

namespace PrizeSelection.Logic
{
    public interface ISelectionSuccessCalculator
    {
        double GetChanceToMeetSuccessCriteriaForFixedSelectionCount(IDictionary<int, int> successCriteria, IList<SelectionDomain> selectionDomains, int selectionCount,  Random random = null);

        double GetChanceToMeetSuccessCriteriaSubsetForFixedSelectionCount(IDictionary<int, int> successCriteria, IList<SelectionDomain> selectionDomains, int selectionCount, int subsetSize, Random random = null);


        PrizeSelectionsForSuccessInfo GetResultsForPullsUntilSuccess(IDictionary<int, int> successCriteria, IList<SelectionDomain> selectionDomains, Random random = null);


        PrizeSelectionsForSuccessInfo GetResultsForPullsUntilSuccessSubset(IDictionary<int, int> successCriteria, IList<SelectionDomain> selectionDomains, int subsetSize, Random random = null);

        //bool DoPrizeResultsMeetSuccessCriteria(IList<PrizeResultRow> prizeResultTable, IDictionary<int, int> successCriteria);

        #region Deprecated
        //double GetChanceToMeetSuccessCriteriaForFixedPulls(IDictionary<int, int> successCriteria, int numberOfPulls, int bannerRelicCount, IList<SelectionDomain> pullCategoryParameterSets, Random random = null);
        //PrizeSelectionsForSuccessInfo GetResultsForPullsUntilSuccess(IDictionary<int, int> successCriteria, int bannerRelicCount, IList<SelectionDomain> pullCategoryParameterSets, Random random = null);
        //bool DoPullResultsMeetSuccessCriteria(IDictionary<int, int> pullResults, IDictionary<int, int> successCriteria);

        #endregion
    }

    public class SelectionSuccessCalculator : ISelectionSuccessCalculator
    {
        #region Class Variables

        private readonly IPrizeResultsTableHelper _prizeResultsTableHelper;
        private readonly ISelectionEngine _selectionEngine;
        #endregion

        #region Constructors

        public SelectionSuccessCalculator(IPrizeResultsTableHelper prizeResultsTableHelper, ISelectionEngine selectionEngine)
        {
            _prizeResultsTableHelper = prizeResultsTableHelper;
            _selectionEngine = selectionEngine;
        }
        #endregion


        //Chance that making X selections from the PrizeSelectionTable will select all the desired prizes in the desired amounts
        //criteria are met if for each prize index with non zero count in criteria, 
        //the corresponding prize index in the prize results table has a count >= the criteria count
        public double GetChanceToMeetSuccessCriteriaForFixedSelectionCount(IDictionary<int, int> successCriteria, IList<SelectionDomain> selectionDomains, 
            int selectionCount,  Random random = null)
        {
            double trials = 10000; //harcoded so user can't DOS
            int successes = 0;

            if (random == null)
            {
                random = new Random();
            }

            for (int trial = 0; trial < trials; trial++)
            {
                IList<PrizeResultRow> trialCombinedPrizeResultsTable = new List<PrizeResultRow>(); //initialize; this will get overwritten in first trial

                for (int counter = 0; counter < selectionCount; counter++)
                {
                    IList<PrizeResultRow> localPrizeResultsTable = _selectionEngine.SelectPrizes(selectionDomains, random);

                    if (counter == 0)
                    {
                        trialCombinedPrizeResultsTable = localPrizeResultsTable;
                    }
                    else
                    {
                        trialCombinedPrizeResultsTable =
                            _prizeResultsTableHelper.CombinePrizeResultTables(trialCombinedPrizeResultsTable,
                                localPrizeResultsTable);
                    }

                    if (DoPrizeResultsMeetSuccessCriteria(trialCombinedPrizeResultsTable, successCriteria))
                    {
                        successes++;
                        break;
                    }
                }
            }

            double successChance = successes / trials;
            return successChance;
        }

        //Chance that making X selections from the PrizeSelectionTable will select at least subsetSize of the desired prizes in the desired amounts.
        //criteria are met if for at least subsetSize of the specified success prizes with non zero count in criteria, 
        //the corresponding prize index in the prize results table has a count >= the criteria count
        public double GetChanceToMeetSuccessCriteriaSubsetForFixedSelectionCount(IDictionary<int, int> successCriteria, IList<SelectionDomain> selectionDomains,
            int selectionCount, int subsetSize, Random random = null)
        {
            double trials = 10000; //harcoded so user can't DOS
            int successes = 0;

            if (random == null)
            {
                random = new Random();
            }

            for (int trial = 0; trial < trials; trial++)
            {
                IList<PrizeResultRow> trialCombinedPrizeResultsTable = new List<PrizeResultRow>(); //initialize; this will get overwritten in first trial

                for (int counter = 0; counter < selectionCount; counter++)
                {
                    IList<PrizeResultRow> localPrizeResultsTable = _selectionEngine.SelectPrizes(selectionDomains, random);

                    if (counter == 0)
                    {
                        trialCombinedPrizeResultsTable = localPrizeResultsTable;
                    }
                    else
                    {
                        trialCombinedPrizeResultsTable =
                            _prizeResultsTableHelper.CombinePrizeResultTables(trialCombinedPrizeResultsTable,
                                localPrizeResultsTable);
                    }

                    if (DoPrizeResultsMeetSuccessCriteriaSubset(trialCombinedPrizeResultsTable, successCriteria, subsetSize))
                    {
                        successes++;
                        break;
                    }
                }
            }

            double successChance = successes / trials;
            return successChance;
        }

        public PrizeSelectionsForSuccessInfo GetResultsForPullsUntilSuccess(IDictionary<int, int> successCriteria,
            IList<SelectionDomain> selectionDomains, Random random = null)
        {
            PrizeSelectionsForSuccessInfo successInfo = new PrizeSelectionsForSuccessInfo();
            int trials = 10000; //harcoded so user can't DOS
            IList<int> selectionsRequiredForTrialSuccess = new List<int>();

            if (random == null)
            {
                random = new Random();
            }

            for (int trial = 0; trial < trials; trial++)
            {
                IList<PrizeResultRow> trialCombinedPrizeResultsTable = new List<PrizeResultRow>(); //initialize; this will get overwritten in first trial

                int counter = 1;
                do
                {
                    IList<PrizeResultRow> localPrizeResultsTable = _selectionEngine.SelectPrizes(selectionDomains, random);

                    if (counter == 1)
                    {
                        trialCombinedPrizeResultsTable = localPrizeResultsTable;
                    }
                    else
                    {  
                        trialCombinedPrizeResultsTable =
                            _prizeResultsTableHelper.CombinePrizeResultTables(trialCombinedPrizeResultsTable, localPrizeResultsTable);
                    }
             
                    if (DoPrizeResultsMeetSuccessCriteria(trialCombinedPrizeResultsTable, successCriteria))
                    {
                        selectionsRequiredForTrialSuccess.Add(counter);
                        break;
                    }

                    counter++;

                } while (true);
            };

            //now that all the trials have recorded the number of pulls required to succeed, we can calculate the statistics
            successInfo.TrialsConducted = trials;
            successInfo.MinPullsRequired = selectionsRequiredForTrialSuccess.Min();
            successInfo.MaxPullsRequired = selectionsRequiredForTrialSuccess.Max();
            successInfo.MeanPullsRequired = selectionsRequiredForTrialSuccess.Average();
            successInfo.ModePullsRequired = GetModeFromList(selectionsRequiredForTrialSuccess);
            successInfo.MedianPullsRequired = GetMedianFromList(selectionsRequiredForTrialSuccess);

            return successInfo;

        }

        public PrizeSelectionsForSuccessInfo GetResultsForPullsUntilSuccessSubset(IDictionary<int, int> successCriteria,
                IList<SelectionDomain> selectionDomains, int subsetSize, Random random = null)
        {
            PrizeSelectionsForSuccessInfo successInfo = new PrizeSelectionsForSuccessInfo();
            int trials = 10000; //harcoded so user can't DOS
            IList<int> selectionsRequiredForTrialSuccess = new List<int>();

            for (int trial = 0; trial < trials; trial++)
            {
                IList<PrizeResultRow> trialCombinedPrizeResultsTable = new List<PrizeResultRow>(); //initialize; this will get overwritten in first trial

                if (random == null)
                {
                    random = new Random();
                }

                int counter = 1;
                do
                {
                    IList<PrizeResultRow> localPrizeResultsTable = _selectionEngine.SelectPrizes(selectionDomains, random);

                    if (counter == 1)
                    {
                        trialCombinedPrizeResultsTable = localPrizeResultsTable;
                    }
                    else
                    {
                        trialCombinedPrizeResultsTable =
                            _prizeResultsTableHelper.CombinePrizeResultTables(trialCombinedPrizeResultsTable, localPrizeResultsTable);
                    }

                    if (DoPrizeResultsMeetSuccessCriteriaSubset(trialCombinedPrizeResultsTable, successCriteria, subsetSize))
                    {
                        selectionsRequiredForTrialSuccess.Add(counter);
                        break;
                    }

                    counter++;

                } while (true);
            };

            //now that all the trials have recorded the number of pulls required to succeed, we can calculate the statistics
            successInfo.TrialsConducted = trials;
            successInfo.MinPullsRequired = selectionsRequiredForTrialSuccess.Min();
            successInfo.MaxPullsRequired = selectionsRequiredForTrialSuccess.Max();
            successInfo.MeanPullsRequired = selectionsRequiredForTrialSuccess.Average();
            successInfo.ModePullsRequired = GetModeFromList(selectionsRequiredForTrialSuccess);
            successInfo.MedianPullsRequired = GetMedianFromList(selectionsRequiredForTrialSuccess);

            return successInfo;

        }
        #region Private Methods

        private bool DoPrizeResultsMeetSuccessCriteria(IList<PrizeResultRow> prizeResultTable, IDictionary<int, int> successCriteria)
        {
            //validations
            if (prizeResultTable == null || successCriteria == null)
            {
                throw new ArgumentException("prizeResultTable and successCriteria must both be non null");
            }
            if (prizeResultTable.Count != successCriteria.Keys.Count)
            {
                throw new ArgumentException("prizeResultTable and successCriteria must both contain the same number of items");
            }

            bool isSuccess = true;

            int tableSize = prizeResultTable.Count;

            for (int key = 1; key <= tableSize; key++)
            {
                if (successCriteria[key] > 0)
                {
                    if (prizeResultTable[key - 1].PrizeSelectedCount < successCriteria[key])
                    {
                        isSuccess = false;
                        break;
                    }
                }
            }

            return isSuccess;
        }

        //this is for answering "success is any two of the specified three prizes" type scenarios
        private bool DoPrizeResultsMeetSuccessCriteriaSubset(IList<PrizeResultRow> prizeResultTable, IDictionary<int, int> successCriteria, int subsetSize)
        {
            //validations
            if (prizeResultTable == null || successCriteria == null)
            {
                throw new ArgumentException("prizeResultTable and successCriteria must both be non null");
            }
            if (prizeResultTable.Count != successCriteria.Keys.Count)
            {
                throw new ArgumentException("prizeResultTable and successCriteria must both contain the same number of items");
            }

            int specifiedPrizeCount = successCriteria.Count(kvp => kvp.Value > 0);
            if (subsetSize >= specifiedPrizeCount)
            {
                throw new ArgumentException("subsetSize must be smaller than the number of prizes provided non zero values in the successCriteria");
            }

            bool isSuccess = false;

            int tableSize = prizeResultTable.Count;

            int prizesMeetingSpecifiedTargetCount = 0;

            for (int key = 1; key <= tableSize; key++)
            {               
                if (successCriteria[key] > 0)
                {
                    if (prizeResultTable[key - 1].PrizeSelectedCount >= successCriteria[key])
                    {
                        prizesMeetingSpecifiedTargetCount++;
                    }
                }
            }

            if (prizesMeetingSpecifiedTargetCount >= subsetSize)
            {
                isSuccess = true;
            }

            return isSuccess;
        }


        private int GetModeFromList(IList<int> numberList)
        {
            var groups = numberList.GroupBy(v => v);
            int maxCount = groups.Max(g => g.Count());
            int mode = groups.First(g => g.Count() == maxCount).Key;

            return mode;
        }

        private double GetMedianFromList(IList<int> numberList)
        {
            int numberCount = numberList.Count();
            int halfIndex = numberList.Count() / 2;
            var sortedNumbers = numberList.OrderBy(n => n);
            double median;

            if ((numberCount % 2) == 0)
            {
                median = (sortedNumbers.ElementAt(halfIndex) + sortedNumbers.ElementAt(halfIndex - 1)) / 2.0;
            }
            else
            {
                median = sortedNumbers.ElementAt(halfIndex);
            }

            return median;
        } 
        #endregion

        #region Deprecated

        //public double GetChanceToMeetSuccessCriteriaForFixedPulls(IDictionary<int, int> successCriteria, int numberOfPulls, int bannerRelicCount, IList<SelectionDomain> pullCategoryParameterSets, Random random = null)
        //{
        //    double trials = 10000; //harcoded so user can't DOS
        //    int successes = 0;

        //    for (int trial = 0; trial < trials; trial++)
        //    {
        //        IDictionary<int, int> trialPullResults = _prizeResultsTableHelper.GetEmptyPrizeResultsSummary(successCriteria.Keys.Count);

        //        for (int pullCounter = 0; pullCounter < numberOfPulls; pullCounter++)
        //        {
        //            IDictionary<int, int> pullResults = _selectionEngine.SimulateBulkPullGeneric(bannerRelicCount, pullCategoryParameterSets, random);
        //            trialPullResults = _prizeResultsTableHelper.CombinePullResultsTables(pullResults, trialPullResults);
        //            if (DoPullResultsMeetSuccessCriteria(trialPullResults, successCriteria))
        //            {
        //                successes++;
        //                break;
        //            }
        //        }
        //    }

        //    double successChance = successes / trials;
        //    return successChance;
        //}

        //public PrizeSelectionsForSuccessInfo GetResultsForPullsUntilSuccess(IDictionary<int, int> successCriteria, int bannerRelicCount, IList<SelectionDomain> pullCategoryParameterSets, Random random = null)
        //{
        //    PrizeSelectionsForSuccessInfo successInfo = new PrizeSelectionsForSuccessInfo();
        //    int trials = 10000; //harcoded so user can't DOS
        //    IList<int> pullsRequiredForTrialSuccess = new List<int>();

        //    for (int trial = 0; trial < trials; trial++)
        //    {
        //        IDictionary<int, int> trialPullResults = _prizeResultsTableHelper.GetEmptyPrizeResultsSummary(successCriteria.Keys.Count);

        //        int pullCounter = 1;
        //        do
        //        {
        //            IDictionary<int, int> pullResults = _selectionEngine.SimulateBulkPullGeneric(bannerRelicCount, pullCategoryParameterSets, random);
        //            trialPullResults = _prizeResultsTableHelper.CombinePullResultsTables(pullResults, trialPullResults);
        //            if (DoPullResultsMeetSuccessCriteria(trialPullResults, successCriteria))
        //            {
        //                pullsRequiredForTrialSuccess.Add(pullCounter);
        //                break;
        //            }
        //            pullCounter++;
        //        } while (true);
        //    };

        //    //now that all the trials have recorded the number of pulls required to succeed, we can calculate the statistics
        //    successInfo.TrialsConducted = trials;
        //    successInfo.MinPullsRequired = pullsRequiredForTrialSuccess.Min();
        //    successInfo.MaxPullsRequired = pullsRequiredForTrialSuccess.Max();
        //    successInfo.MeanPullsRequired = pullsRequiredForTrialSuccess.Average();
        //    successInfo.ModePullsRequired = GetModeFromList(pullsRequiredForTrialSuccess);
        //    successInfo.MedianPullsRequired = GetMedianFromList(pullsRequiredForTrialSuccess);

        //    return successInfo;
        //}

        ////criteria are met if for each relic key with non zero count in criteria, the corresponding relic key in the pull results has a count >= the criteria count
        //public bool DoPullResultsMeetSuccessCriteria(IDictionary<int, int> pullResults, IDictionary<int, int> successCriteria)
        //{
        //    //validations
        //    if (pullResults == null || successCriteria == null)
        //    {
        //        throw new ArgumentException("pullResults and successCriteria must both be non null");
        //    }
        //    if (pullResults.Keys.Count != successCriteria.Keys.Count)
        //    {
        //        throw new ArgumentException("pullResults and successCriteria must both contain the same number of items");
        //    }

        //    bool isSuccess = true;

        //    int tableSize = pullResults.Keys.Count;

        //    for (int key = 1; key <= tableSize; key++)
        //    {
        //        if (successCriteria[key] > 0)
        //        {
        //            if (pullResults[key] < successCriteria[key])
        //            {
        //                isSuccess = false;
        //            }
        //        }
        //    }

        //    return isSuccess;
        //} 
        #endregion
    }
}
