using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PrizeSelection.Model;

namespace PrizeSelection.Logic
{
    public interface IPrizeResultsTableHelper
    {
        IDictionary<int, int> GetEmptyPrizeResultsSummary(int prizeRowCount);

        IList<PrizeResultRow> CombinePrizeResultTables(IList<PrizeResultRow> table1,
            IList<PrizeResultRow> table2);

    }

    public class PrizeResultsTableHelper : IPrizeResultsTableHelper
    {
        public IDictionary<int, int> GetEmptyPrizeResultsSummary(int prizeRowCount)
        {
            IDictionary<int, int> emptyPrizeResultsSummary = new Dictionary<int, int>(prizeRowCount);

            for (int prizeIndex = 0; prizeIndex < prizeRowCount; prizeIndex++)
            {
                emptyPrizeResultsSummary.Add(prizeIndex + 1, 0); //we start with 0 count of each desired relic
            }

            return emptyPrizeResultsSummary;
        }

        public IList<PrizeResultRow> CombinePrizeResultTables(IList<PrizeResultRow> table1, IList<PrizeResultRow> table2)
        {
            if (table1 == null || table2 == null)
            {
                throw new ArgumentException("prize results tables must not be null");
            }
            //check that both tables have the same number of entries
            if (table1.Count != table2.Count)
            {
                throw new ArgumentException("pull results tables must be the same size");
            }
            //check that both tables have the same schema (prize cat names, prizename, prizeindex
            int rowCount = table1.Count;

            for (int counter = 0; counter < rowCount; counter++)
            {
                PrizeResultRow table1Row = table1[counter];
                PrizeResultRow table2Row = table2[counter];

                if (table1Row.PrizeIndex != table2Row.PrizeIndex ||
                    table1Row.PrizeCategoryName != table2Row.PrizeCategoryName ||
                    table1Row.PrizeName != table2Row.PrizeName)
                {
                    throw new ArgumentException("table1 and table 2 must have the same schema with respect to PrizeIndex, PrizeName, PrizeCategoryName");
                }
            }

            IList<PrizeResultRow> combinedPrizeResultsTable = table1.Zip(table2, (t1, t2) => 
                    new PrizeResultRow()
                    {
                        PrizeIndex = t1.PrizeIndex,
                        PrizeCategoryName = t1.PrizeCategoryName,
                        PrizeName = t1.PrizeName,
                        PrizeSelectedCount = t1.PrizeSelectedCount + t2.PrizeSelectedCount
                    }).ToList();

            return combinedPrizeResultsTable;
        }

    }
}
