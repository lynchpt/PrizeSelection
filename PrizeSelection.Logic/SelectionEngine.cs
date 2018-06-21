using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using PrizeSelection.Model;

namespace PrizeSelection.Logic
{
    public interface ISelectionEngine
    {       
        IList<PrizeResultRow> SelectPrizes(IList<SelectionDomain> selectionDomains, Random random = null);

        IList<PrizeResultRow> SelectPrizes(IList<SelectionDomain> selectionDomains, int selectionCount, Random random = null);
    }

    public class SelectionEngine : ISelectionEngine
    {
        #region Class Variables

        private readonly IPrizeSelectionTableHelper _prizeSelectionTableHelper;
        private readonly IPrizeResultsTableHelper _prizeResultsTableHelper;
        private readonly ILogger<ISelectionEngine> _logger;
        #endregion

        #region Constructors

        public SelectionEngine(IPrizeSelectionTableHelper prizeSelectionTableHelper, IPrizeResultsTableHelper prizeResultsTableHelper,
            ILogger<ISelectionEngine> logger)
        {
            _prizeSelectionTableHelper = prizeSelectionTableHelper;
            _prizeResultsTableHelper = prizeResultsTableHelper;
            _logger = logger;
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

            Stopwatch sw = Stopwatch.StartNew();

            //Set up variables and structures      

            if (random == null)
            {
                random = new Random();
            }

            IList<PrizeNameCategoryPair> uniquePrizeNames = GetUniquePrizeNameCategoryPairs(selectionDomains);

            int finalPrizeRowCount = uniquePrizeNames.Count();

            IList<PrizeResultRow> prizeResultTable = new List<PrizeResultRow>(finalPrizeRowCount);

            List<IDictionary<int, int>> resultSummariesList = new List<IDictionary<int, int>>();


            //perform the actual selection and counting
            for(int domainCounter = 0; domainCounter < selectionDomains.Count; domainCounter++)
            {
                SelectionDomain currentSelectionDomain = selectionDomains[domainCounter];

                //for intermediate use because we need to update the prize counts in a performant way.
                IDictionary<int, int> resultsSummary = _prizeResultsTableHelper.GetEmptyPrizeResultsSummary(currentSelectionDomain.PrizeSelectionTable.Count);

                for (int counter = 0; counter < currentSelectionDomain.PrizesToSelectFromDomainCount; counter++)
                {
                    PrizeSelectionRow selectedPrizeRow = SelectPrizeFromPrizeTable(currentSelectionDomain.PrizeSelectionTable, random.NextDouble());

                    if (selectedPrizeRow != null)
                    {
                        resultsSummary[selectedPrizeRow.PrizeIndex]++;
                    }
                }

                resultSummariesList.Add(resultsSummary);
            }

            //now that we have the counts of selected prizes by index, we need to translate them back to the more
            //informative structure of the prize results table

            //generate table with all the data except counts and indexes
            prizeResultTable = uniquePrizeNames.Select(psr =>
                                    new PrizeResultRow()
                                    {
                                        PrizeCategoryName = psr.CategoryName,
                                        PrizeIndex = 0,
                                        PrizeName = psr.PrizeName,
                                        PrizeSelectedCount = 0
                                    }).ToList();

            //put in indexes
            for (int i = 0; i < finalPrizeRowCount; i++)
            {
                prizeResultTable[i].PrizeIndex = i + 1;
            }

            

            //now insert the counts; assumption is that the keys of the generated resultsSummary dictionary must match in number
            //and start at the same index (1), as the largestSelectionDomain which is used to prepopulate the prizeResultTable
            for (int domainCounter = 0; domainCounter < selectionDomains.Count; domainCounter++)
            {
                //since the resultsSummary dictionaries don't have names embedded, we can only relate the counts
                //to prize name in context of the associated selection domain.
                SelectionDomain currentSelectionDomain = selectionDomains[domainCounter];
                IDictionary<int, int> resultsSummary = resultSummariesList[domainCounter];

                //walk through prizeResultTable one prize at a time, ADDING in counts

                //walk through non zero resultsSummary one prize at a time, linking to prize name and ADDING in counts
                for (int item = 0; item < resultsSummary.Count; item++)
                {
                    if (resultsSummary[item + 1] > 0)
                    {
                        //get prize name from selection domain prizeSelectionTable
                        string prizeName = currentSelectionDomain.PrizeSelectionTable
                            .Where(r => r.PrizeIndex == (item + 1)).Select(r => r.PrizeName).Single();

                        //now map prizeName back to prizeResultTable
                        PrizeResultRow rowToUpdate = prizeResultTable.Single(p => p.PrizeName == prizeName);

                        rowToUpdate.PrizeSelectedCount += resultsSummary[item + 1];
                    }
                }

            }
            sw.Stop();
            _logger.LogDebug($"finished a selection operation in {sw.ElapsedMilliseconds} milliseconds");

            return prizeResultTable;
        }

        public IList<PrizeResultRow> SelectPrizes(IList<SelectionDomain> selectionDomains, int selectionCount, Random random = null)
        {

            #region Validations       
            if (selectionCount > 100)
            {
                _logger.LogWarning($"someone tried an invalid selectionCount: {selectionCount}");
                throw new ArgumentException($"selectionCount must be 100 or LESS");
            }
            #endregion

            //Set up variables and structures

            //since we are calling other SelectPrizes method rapidly in succession, we can't let each method call generate its
            //own Random, since likely they would start with the same seed. By instantiating just one here, one seed is used
            //and subsequent uses in the child SelectPrize method calls will give random numbers
            if (random == null)
            {
                random = new Random();
            }

            IList<PrizeResultRow> combinedPrizeResultTable = new List<PrizeResultRow>();

            _logger.LogDebug($"ready to perform selectionCount selections: {selectionCount}");
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


        #region Private Methods
        //null row = no matching prizes were found
        private PrizeSelectionRow SelectPrizeFromPrizeTable(IList<PrizeSelectionRow> prizeSelectionTable, double randomNumber)
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

        private IList<PrizeNameCategoryPair> GetUniquePrizeNameCategoryPairs(IList<SelectionDomain> selectionDomains)
        {
            //final count of prize rows in results is distict union of prize names
            List<PrizeNameCategoryPair> prizeNames = selectionDomains.SelectMany(sd => sd.PrizeSelectionTable).
                Select(r => new PrizeNameCategoryPair(r.PrizeName, r.PrizeCategoryName)).ToList();

            //clean up list to allow for better distincting. Main cleanup is if a prize is in multiple categories, 
            //each its categeory names should be replaced by "Multi-Category"
            var categoriesByPrizes = from pn in prizeNames group pn.CategoryName by pn.PrizeName into g select new { PrizeName = g.Key, Categories = g.ToList() };
            var multiCatPrizeNames = categoriesByPrizes.Where(gm => gm.Categories.Count > 1).Select(m => m.PrizeName).ToList();

            foreach (var item in prizeNames)
            {
                if (multiCatPrizeNames.Contains(item.PrizeName))
                {
                    item.CategoryName = "Multi-Category";
                }
            }

            //now we can distint
            IList<PrizeNameCategoryPair> uniquePrizeNames = prizeNames.Distinct(new PrizeNameCategoryPairEqualityComparer()).ToList();

            return uniquePrizeNames;
        }
        #endregion

        private class PrizeNameCategoryPair
        {
            public PrizeNameCategoryPair(string prizeName, string categoryName)
            {
                PrizeName = prizeName;
                CategoryName = categoryName;
            }

            public string PrizeName { get; set; }
            public string CategoryName { get; set; }
        }

        private class PrizeNameCategoryPairEqualityComparer : IEqualityComparer<PrizeNameCategoryPair>
        {
            public bool Equals(PrizeNameCategoryPair x, PrizeNameCategoryPair y)
            {
                if (x == null && y == null) return true;
                if(x == null || y == null) return false;
                if (x.PrizeName == y.PrizeName && x.CategoryName == y.CategoryName) return true;
                else return false;

            }

            public int GetHashCode(PrizeNameCategoryPair obj)
            {
                int hashCode = obj.PrizeName.GetHashCode() ^ obj.CategoryName.GetHashCode();
                return hashCode.GetHashCode();
            }
        }
    }
}
