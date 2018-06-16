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
    }
}
