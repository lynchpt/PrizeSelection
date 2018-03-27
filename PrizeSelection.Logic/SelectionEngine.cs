using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PrizeSelection.Model;

namespace PrizeSelection.Logic
{
    public interface ISelectionEngine
    {       
        IList<PrizeResultRow> SelectPrizes(IList<SelectionDomain> selectionDomains, Random random = null);

        IList<PrizeResultRow> SelectPrizes(IList<SelectionDomain> selectionDomains, int selectionCount, Random random = null);

        PrizeSelectionRow SelectPrizeFromPrizeTable(IList<PrizeSelectionRow> prizeSelectionTable, double randomNumber);

        #region Deprecated
        //IDictionary<int, int> SimulateBulkPullGeneric(int bannerRelicCount, IList<SelectionDomain> selectionDomains, Random random = null);

        //int GetRelicKeyFromProbabilityTableForGivenNumber(IDictionary<int, double> probabilityTable, double rolledNumber);

        //
        #endregion
    }

    public class SelectionEngine : ISelectionEngine
    {
        #region Class Variables

        private readonly IPrizeSelectionTableHelper _prizeSelectionTableHelper;
        private readonly IPrizeResultsTableHelper _prizeResultsTableHelper;
        #endregion

        #region Constructors

        public SelectionEngine(IPrizeSelectionTableHelper prizeSelectionTableHelper, IPrizeResultsTableHelper prizeResultsTableHelper)
        {
            _prizeSelectionTableHelper = prizeSelectionTableHelper;
            _prizeResultsTableHelper = prizeResultsTableHelper;
        }
        #endregion

        public IList<PrizeResultRow> SelectPrizes(IList<SelectionDomain> selectionDomains, Random random = null)
        {
            #region Validations
            foreach (var selectionDomain in selectionDomains)
            {
                if (selectionDomain.PrizesToSelectFromDomainCount <= 0)
                {
                    throw new ArgumentException($"PrizesToSelectFromDomainCount for SelectFromDomain {selectionDomain.SelectionDomainName} must be greater than 0");
                }
                if (!_prizeSelectionTableHelper.IsPrizeSelectionTableValid(selectionDomain.PrizeSelectionTable))
                {
                    throw new ArgumentException($"PrizeSelectionTable for selectionDomain {selectionDomain.SelectionDomainName} was invalid");
                }
            }
            #endregion

            //Set up variables and structures

            IList<PrizeResultRow> prizeResultTable = new List<PrizeResultRow>();


            int prizeRowCount = selectionDomains.Max(sd => sd.PrizeSelectionTable.Count);
            SelectionDomain largestSelectionDomain = selectionDomains.First(sd => sd.PrizeSelectionTable.Count == prizeRowCount);


            //for intermediate use because we need to update the prize counts in a performant way.
            IDictionary<int, int> resultsSummary = _prizeResultsTableHelper.GetEmptyPrizeResultsSummary(prizeRowCount);

            if (random == null)
            {
                random = new Random();
            }

            //perform the actual selection and counting
            foreach (var selectionDomain in selectionDomains)
            {
                for (int counter = 0; counter < selectionDomain.PrizesToSelectFromDomainCount; counter++)
                {
                    PrizeSelectionRow selectedPrizeRow = SelectPrizeFromPrizeTable(selectionDomain.PrizeSelectionTable, random.NextDouble());

                    if (selectedPrizeRow != null)
                    {
                        resultsSummary[selectedPrizeRow.PrizeIndex]++;
                    }
                }
            }

            //now that we have the counts of selected prizes by index, we need to translate them back to the more
            //informative structure of the prize results table

            //generate table with all the data except counts
            prizeResultTable = largestSelectionDomain.PrizeSelectionTable.Select(psr => 
                                    new PrizeResultRow()
                                    {
                                        PrizeCategoryName = psr.PrizeCategoryName,
                                        PrizeIndex = psr.PrizeIndex,
                                        PrizeName = psr.PrizeName,
                                        PrizeSelectedCount = 0
                                    }).ToList();

            //now insert the counts; assumption is that the keys of the generated resultsSummary dictionary must match in number
            //and start at the same index (1), as the largestSelectionDomain which is used to prepopulate the prizeResultTable
            foreach (var prizeResultRow in prizeResultTable)
            {
                prizeResultRow.PrizeSelectedCount = resultsSummary[prizeResultRow.PrizeIndex];
            }

            return prizeResultTable;
        }

        public IList<PrizeResultRow> SelectPrizes(IList<SelectionDomain> selectionDomains, int selectionCount, Random random = null)
        {

            //Set up variables and structures

            //since we are calling other SelectPrizes method rapidly in succession, we can't let each method call generate its
            //own Random, since likely they would start with the same seed. By instantiating just one here, one seed is used
            //and subsequent uses in the child SelectPrize method calls will give random numbers
            if (random == null)
            {
                random = new Random();
            }

            IList<PrizeResultRow> combinedPrizeResultTable = new List<PrizeResultRow>();

            for (int counter = 0; counter < selectionCount; counter++)
            {
                IList<PrizeResultRow> prizeResultTable = SelectPrizes(selectionDomains, random);

                if (counter == 0)
                {
                    combinedPrizeResultTable = prizeResultTable;
                }
                else
                {
                    combinedPrizeResultTable =
                        _prizeResultsTableHelper.CombinePrizeResultTables(combinedPrizeResultTable, prizeResultTable);
                }


            }
            

            return combinedPrizeResultTable;
        }

        //null row = no matching prizes were found
        public PrizeSelectionRow SelectPrizeFromPrizeTable(IList<PrizeSelectionRow> prizeSelectionTable, double randomNumber)
        {       
            PrizeSelectionRow selectedPrize = null;

            //scan table from the top down until we find the first prize whose lower bound is less than or equal to than the provided roll; 
            //the prize we want is the one immediately lower in the table.

            //top down - assume already sorted!!
            foreach (PrizeSelectionRow prizeSelectionRow in prizeSelectionTable)
            {
                if (prizeSelectionRow.PrizeProbabilityLowerBound <= randomNumber)
                {
                    selectedPrize = prizeSelectionRow;
                    break;
                }

            }

            return selectedPrize;
        }

        #region Deprecated
        //public IDictionary<int, int> SimulateBulkPullGeneric(int bannerRelicCount, IList<SelectionDomain> selectionDomains, Random random = null)
        //{
        //    #region Validations
        //    foreach (var selectionDomain in selectionDomains)
        //    {
        //        if (selectionDomain.PrizesToSelectFromDomainCount <= 0)
        //        {
        //            throw new ArgumentException($"PrizesToSelectFromDomainCount for SelectFromDomain {selectionDomain.SelectionDomainName} must be greater than 0");
        //        }
        //        if (!_prizeSelectionTableHelper.IsPullProbabilityTableValid(selectionDomain.ProbabilityTable))
        //        {
        //            throw new ArgumentException($"ProbabilityTable for SelectFromDomain {selectionDomain.SelectionDomainName} was invalid");
        //        }
        //    }
        //    #endregion

        //    IDictionary<int, int> simulatedBulkPullResultsTable = _prizeResultsTableHelper.GetEmptyPrizeResultsSummary(bannerRelicCount);
        //    if (random == null)
        //    {
        //        random = new Random();
        //    }

        //    foreach (var selectionDomain in selectionDomains)
        //    {
        //        for (int counter = 0; counter < selectionDomain.PrizesToSelectFromDomainCount; counter++)
        //        {
        //            int selectedRelicKey = GetRelicKeyFromProbabilityTableForGivenNumber(selectionDomain.ProbabilityTable, random.NextDouble());

        //            if (selectedRelicKey > 0)
        //            {
        //                simulatedBulkPullResultsTable[selectedRelicKey]++;
        //            }
        //        }
        //    }

        //    return simulatedBulkPullResultsTable;
        //}

        ////0 = no matching relics were found
        //public int GetRelicKeyFromProbabilityTableForGivenNumber(IDictionary<int, double> probabilityTable, double rolledNumber)
        //{
        //    int selectedRelicKey = 0;

        //    //scan table from the top down until we find the first relic whose lower bound is less than or equal to than the provided roll; 
        //    //the relic we want is the one immediately lower in the table.
        //    int tableMaxKey = probabilityTable.Count;

        //    //top down
        //    for (int key = 1; key <= tableMaxKey; key++)
        //    {
        //        if (probabilityTable[key] <= rolledNumber) //we have a hit!
        //        {
        //            selectedRelicKey = key;
        //            break;
        //        }
        //    }

        //    return selectedRelicKey;
        //} 
        #endregion
    }
}
