using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PrizeSelection.Model;

namespace PrizeSelection.Logic
{
    public interface IResultsFormatter
    {
        IList<string> GeneratePrizeNamesList(int prizeNamesToGenerateCount, string prizeCategoryName);

        #region Deprecated
        //IDictionary<int, string> GetRelicKeyToRelicNameMapping(int bannerRelicCount, IList<string> relicNames);

        //IList<NamedRelicResultItem> SpliceRelicNamesIntoPullResults(IDictionary<int, string> relicKeyToRelicNameMapping, IDictionary<int, int> pullResultsTable); 
        #endregion
    }

    public class ResultsFormatter : IResultsFormatter
    {

        public IList<string> GeneratePrizeNamesList(int prizeNamesToGenerateCount, string prizeCategoryName)
        {
            //string unnamedPrizeCategoryDefault = "UnnamedPrizeCategory";
            string unnamedPrizeNamePrefix = "Unnamed";

            IList<string> generatedNames = new List<string>();

            for (int key = 1; key <= prizeNamesToGenerateCount; key++)
            {
                if (!String.IsNullOrWhiteSpace(prizeCategoryName))
                {
                    generatedNames.Add($"{unnamedPrizeNamePrefix} {prizeCategoryName} - {key}");
                }
                else
                {
                    generatedNames.Add($"{unnamedPrizeNamePrefix} - {key}");
                }
            }

            return generatedNames;
        }

        #region Deprecated
        //public IDictionary<int, string> GetRelicKeyToRelicNameMapping(int bannerRelicCount, IList<string> relicNames)
        //{
        //    IDictionary<int, string> relicKeyToRelicNameMapping = new Dictionary<int, string>(bannerRelicCount);
        //    string unnamedRelicPrefix = "UnnamedRelic - ";

        //    for (int relicIndex = 0; relicIndex < bannerRelicCount; relicIndex++)
        //    {
        //        relicKeyToRelicNameMapping.Add(relicIndex + 1, String.Empty);
        //    }

        //    if (relicNames != null && relicNames.Any())
        //    {
        //        //even if more relic names are provided, we will ignore the ones that have a higher index than bannerRelicCount
        //        for (int key = 1; key <= bannerRelicCount; key++)
        //        {
        //            if (key < relicNames.Count + 1)
        //            {
        //                relicKeyToRelicNameMapping[key] = relicNames[key - 1];
        //            }
        //        }
        //    }

        //    //there might be less names provided (or none) than we have relic, so name other relics with the default pattern
        //    int unnamedRelicNumber = 1;
        //    for (int key = 1; key <= bannerRelicCount; key++)
        //    {
        //        if (relicKeyToRelicNameMapping[key] == String.Empty)
        //        {
        //            relicKeyToRelicNameMapping[key] = $"{unnamedRelicPrefix}{unnamedRelicNumber}";
        //            unnamedRelicNumber++;
        //        }
        //    }

        //    return relicKeyToRelicNameMapping;
        //}

        //public IList<NamedRelicResultItem> SpliceRelicNamesIntoPullResults(IDictionary<int, string> relicKeyToRelicNameMapping, IDictionary<int, int> pullResultsTable)
        //{
        //    //validations
        //    if (relicKeyToRelicNameMapping == null || pullResultsTable == null)
        //    {
        //        throw new ArgumentException("relicKeyToRelicNameMapping and pullResultsTable must both be non null");
        //    }
        //    if (relicKeyToRelicNameMapping.Keys.Count != pullResultsTable.Count)
        //    {
        //        throw new ArgumentException("relicKeyToRelicNameMapping and pullResultsTable must have the same number of items");
        //    }

        //    int tableSize = relicKeyToRelicNameMapping.Keys.Count;

        //    IList<NamedRelicResultItem> pullResultsWithRelicNames = new List<NamedRelicResultItem>();

        //    for (int key = 1; key <= tableSize; key++)
        //    {
        //        pullResultsWithRelicNames.Add(new NamedRelicResultItem() { RelicKey = key, RelicName = relicKeyToRelicNameMapping[key], RelicCount = pullResultsTable[key] });
        //    }

        //    return pullResultsWithRelicNames;
        //} 
        #endregion
    }
}
