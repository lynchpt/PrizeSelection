using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using PrizeSelection.Logic;
using PrizeSelection.Model;

namespace PrizeSelection.Test
{
    [TestClass]
    public class PrizeSelectionTests
    {
        #region Class Variables
        private IResultsFormatter _resultsFormatter;
        private IPrizeSelectionTableHelper _prizeSelectionTableHelper;
        private IPrizeResultsTableHelper _prizeResultsTableHelper;
        private ISelectionEngine _selectionEngine;
        private ISelectionSuccessCalculator _selectionSuccessCalculator;
        private static Random _random;
        #endregion

        [ClassInitialize]
        public static void InitializeTestClass(TestContext testContext)
        {
            _random = new Random();
        }


        [TestInitialize]
        public void InitializeTest()
        {
            _resultsFormatter = new ResultsFormatter();
            _prizeSelectionTableHelper = new PrizeSelectionTableHelper(_resultsFormatter);
            _prizeResultsTableHelper = new PrizeResultsTableHelper();
            _selectionEngine = new SelectionEngine(_prizeSelectionTableHelper, _prizeResultsTableHelper);
            _selectionSuccessCalculator = new SelectionSuccessCalculator(_prizeResultsTableHelper, _selectionEngine);
        }



        #region ResultsFormatter
        [TestMethod]
        public void ResultsFormatter_GeneratePrizeNamesList_Success()
        {
            IList<string> generatedNames = _resultsFormatter.GeneratePrizeNamesList(10, "TestPrizeCategory");

            Assert.AreEqual(10, generatedNames.Count);
        }

        [TestMethod]
        public void ResultsFormatter_GeneratePrizeNamesList_NoCategory_Success()
        {
            IList<string> generatedNames = _resultsFormatter.GeneratePrizeNamesList(10, String.Empty);

            Assert.AreEqual(10, generatedNames.Count);
        }

        #endregion

        //tests creating prize category spec tables - IPrizeSelectionTableHelper.CreatePrizeCategorySpecification Methods
        #region Prize Category Spec Creation Tests

        [TestMethod]
        public void CreatePrizeCategorySpecification_WithCount_Success()
        {
            PrizeCategorySpecification spec =
                _prizeSelectionTableHelper.CreatePrizeCategorySpecification("TestCat", 0.2, 5);

            Assert.AreEqual(5, spec.PrizeNames.Count());
        }

        [TestMethod]
        public void CreatePrizeCategorySpecification_WithNames_Success()
        {
            PrizeCategorySpecification spec =
                _prizeSelectionTableHelper.CreatePrizeCategorySpecification("TestCat", 0.2, new List<string>() { "Sword", "Spear", "Dagger" });

            Assert.AreEqual(3, spec.PrizeNames.Count());
        }

        [TestMethod]
        public void CreatePrizeCategorySpecification_FFRK_Simple_Guaranteed_WithName_Success()
        {
            // 5 or 6* total
            //category 0 = on banner 5 or 6 ; 14.0% ; scaled = 60/60 * (14.0/14.0) = 1.0
            //total must be 60/60

            string onBannerFiveOrSixStarPrizeCategoryName = "5/6 *";

            double onBannerFiveOrSixStarRate = 1; // = 1.0

            IList<string> prizeNames = GetFestBannerRelicNamesSimple(1);

            //TEST
            IList<PrizeCategorySpecification> specs = new List<PrizeCategorySpecification>()
                        {
                            _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
                                onBannerFiveOrSixStarPrizeCategoryName,
                                onBannerFiveOrSixStarRate,
                                prizeNames)
                        };

            Assert.IsNotNull(specs);
            Assert.AreEqual(prizeNames.Count, specs.First().PrizeNames.Count);

            WritePrizePrizeCategorySpecificationTableTextToConsole(specs);
        }

        [TestMethod]
        public void CreatePrizeCategorySpecification_FFRK_Simple_Guaranteed_WithoutName_Success()
        {
            // 5 or 6* total
            //category 0 = on banner 5 or 6 ; 14.0% ; scaled = 60/60 * (14.0/14.0) = 1.0
            //total must be 60/60

            string onBannerFiveOrSixStarPrizeCategoryName = "5/6 *";

            double onBannerFiveOrSixStarRate = 1; // = 1.0

            int onBannerFiveOrSixStarCategoryPrizeCountDefault = 14;

            //TEST
            IList<PrizeCategorySpecification> specs = new List<PrizeCategorySpecification>()
                                                      {
                                                          _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
                                                              onBannerFiveOrSixStarPrizeCategoryName,
                                                              onBannerFiveOrSixStarRate,
                                                              onBannerFiveOrSixStarCategoryPrizeCountDefault)
                                                      };

            Assert.IsNotNull(specs);
            Assert.AreEqual(onBannerFiveOrSixStarCategoryPrizeCountDefault, specs.First().PrizeNames.Count);

            WritePrizePrizeCategorySpecificationTableTextToConsole(specs);
        }

        [TestMethod]
        public void CreatePrizeCategorySpecification_FFRK_Detailed_Guaranteed_WithName_Success()
        {
            string onBannerFiveOrSixStaPrizeCategoryName = "5/6*";
            string offBannerSixStarPrizeCategoryName = "OffBan 6*";
            string offBannerFiveStarPrizeCategoryName = "OffBan 5*";

            double onBannerFiveOrSixStarRate = 1 * (14.0 / 14.04); // = 0.997150997 etc
            double offBannerSixStarRate = (0.02 / 14.04); // = .001424501 etc
            double offBannerFiveStarRate = (0.02 / 14.04); // = .001424501 etc

            IList<string> prizeNames = GetFestBannerRelicNamesSimple(1);

            //TEST
            IList<PrizeCategorySpecification> specs = new List<PrizeCategorySpecification>()
                        {
                            _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
                                onBannerFiveOrSixStaPrizeCategoryName,
                                onBannerFiveOrSixStarRate,
                                prizeNames),

                            _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
                                offBannerSixStarPrizeCategoryName,
                                offBannerSixStarRate,
                                new List<string>(){offBannerSixStarPrizeCategoryName}),

                            _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
                                offBannerFiveStarPrizeCategoryName,
                                offBannerFiveStarRate,
                                new List<string>(){offBannerFiveStarPrizeCategoryName})
                        };


            Assert.IsNotNull(specs);
            Assert.AreEqual(prizeNames.Count, specs.First().PrizeNames.Count);

            WritePrizePrizeCategorySpecificationTableTextToConsole(specs);
        }

        [TestMethod]
        public void CreatePrizeCategorySpecification_FFRK_Detailed_Guaranteed_WithoutName_Success()
        {
            // 5 or 6* total
            //category 0 = on banner 5 or 6 ; 14.0% ; scaled = 60/60 * (14.0/14.04) = 0.997150997 etc
            //category 1 = off banner 6; .02 % ; scaled = 60/60 * (0.02/14.04) = .001424501 etc
            //category 2 = of banner 4; .02 % ;  scaled = 60/60 * (0.02/14.04) = .001424501 etc
            //total must be 60/60

            string onBannerFiveOrSixStaPrizeCategoryName = "5/6*";
            string offBannerSixStarPrizeCategoryName = "OffBan 6*";
            string offBannerFiveStarPrizeCategoryName = "OffBan 5*";

            double onBannerFiveOrSixStarRate = 1 * (14.0 / 14.04); // = 0.997150997 etc
            double offBannerSixStarRate = (0.02 / 14.04); // = .001424501 etc
            double offBannerFiveStarRate = (0.02 / 14.04); // = .001424501 etc

            int onBannerFiveOrSixStarCategoryPrizeCountDefault = 14;
            int offBannerSixStarCategoryPrizeCountDefault = 1;
            int offBannerFiveStarCategoryPrizeCountDefault = 1;

            //TEST
            IList<PrizeCategorySpecification> specs = new List<PrizeCategorySpecification>()
                        {
                            _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
                                onBannerFiveOrSixStaPrizeCategoryName,
                                onBannerFiveOrSixStarRate,
                                onBannerFiveOrSixStarCategoryPrizeCountDefault),

                            _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
                                offBannerSixStarPrizeCategoryName,
                                offBannerSixStarRate,
                                offBannerSixStarCategoryPrizeCountDefault),

                            _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
                                offBannerFiveStarPrizeCategoryName,
                                offBannerFiveStarRate,
                                offBannerFiveStarCategoryPrizeCountDefault)
                        };


            Assert.IsNotNull(specs);
            Assert.AreEqual(onBannerFiveOrSixStarCategoryPrizeCountDefault, specs.First().PrizeNames.Count);

            WritePrizePrizeCategorySpecificationTableTextToConsole(specs);
        }

        [TestMethod]
        public void CreatePrizeCategorySpecification_FFRK_Simple_Variable_WithName_Success()
        {
            string onBannerFiveOrSixStaPrizeCategoryName = "5/6 *";

            double onBannerFiveOrSixStarRate = 7.0 / 60.0; // = 0.11666666666666666666666666666667

            int onBannerFiveOrSixStarCategoryPrizeCountDefault = 14;

            IList<string> prizeNames = GetFestBannerRelicNamesSimple(1);

            //TEST
            IList<PrizeCategorySpecification> specs = new List<PrizeCategorySpecification>()
                        {
                            _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
                                onBannerFiveOrSixStaPrizeCategoryName,
                                onBannerFiveOrSixStarRate,
                                prizeNames)
                        };


            Assert.IsNotNull(specs);
            Assert.AreEqual(prizeNames.Count, specs.First().PrizeNames.Count);

            WritePrizePrizeCategorySpecificationTableTextToConsole(specs);
        }

        [TestMethod]
        public void CreatePrizeCategorySpecification_FFRK_Simple_Variable_WithoutName_Success()
        {
            string onBannerFiveOrSixStaPrizeCategoryName = "5/6 *";

            double onBannerFiveOrSixStarRate = 7.0 / 60.0; // = 0.11666666666666666666666666666667

            int onBannerFiveOrSixStarCategoryPrizeCountDefault = 14;

            //TEST
            IList<PrizeCategorySpecification> specs = new List<PrizeCategorySpecification>()
                        {
                            _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
                                onBannerFiveOrSixStaPrizeCategoryName,
                                onBannerFiveOrSixStarRate,
                                onBannerFiveOrSixStarCategoryPrizeCountDefault)
                        };


            Assert.IsNotNull(specs);
            Assert.AreEqual(onBannerFiveOrSixStarCategoryPrizeCountDefault, specs.First().PrizeNames.Count);

            WritePrizePrizeCategorySpecificationTableTextToConsole(specs);
        }

        [TestMethod]
        public void CreatePrizeCategorySpecification_FFRK_Detailed_Variable_WithName_Success()
        {
            string onBannerFiveOrSixStaPrizeCategoryName = "5/6 *";

            double onBannerFiveOrSixStarRate = 7.0 / 60.0; // = 0.11666666666666666666666666666667

            IList<string> prizeNames = GetFestBannerRelicNamesSimple(1);

            //TEST
            IList<PrizeCategorySpecification> specs = new List<PrizeCategorySpecification>()
                        {
                            _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
                                onBannerFiveOrSixStaPrizeCategoryName,
                                onBannerFiveOrSixStarRate,
                                prizeNames)
                        };


            Assert.IsNotNull(specs);
            Assert.AreEqual(prizeNames.Count, specs.First().PrizeNames.Count);

            WritePrizePrizeCategorySpecificationTableTextToConsole(specs);
        }

        [TestMethod]
        public void CreatePrizeCategorySpecification_FFRK_Detailed_Variable_WithoutName_Success()
        {
            string onBannerFiveOrSixStaPrizeCategoryName = "5/6 *";

            double onBannerFiveOrSixStarRate = 7.0 / 60.0; // = 0.11666666666666666666666666666667

            int onBannerFiveOrSixStarCategoryPrizeCountDefault = 14;

            //TEST
            IList<PrizeCategorySpecification> specs = new List<PrizeCategorySpecification>()
                        {
                            _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
                                onBannerFiveOrSixStaPrizeCategoryName,
                                onBannerFiveOrSixStarRate,
                                onBannerFiveOrSixStarCategoryPrizeCountDefault)
                        };

            Assert.IsNotNull(specs);
            Assert.AreEqual(onBannerFiveOrSixStarCategoryPrizeCountDefault, specs.First().PrizeNames.Count);

            WritePrizePrizeCategorySpecificationTableTextToConsole(specs);
        }
        #endregion

        //test creating prize selection tables - IPrizeSelectionTableHelper.GetPrizeSelectionTable
        #region Prize Selection Table Tests

        [TestMethod]
        public void CreatePrizeSelectionTable_FFRK_Simple_Guaranteed_WithName_Success()
        {
            IList<PrizeCategorySpecification> specs = GetFFRKPrizeCategorySpecifications_Simple_Guaranteed_WithName(1);

            //TEST
            IList<PrizeSelectionRow> prizeSelectionTable = _prizeSelectionTableHelper.GetPrizeSelectionTable(specs);

            Assert.IsNotNull(prizeSelectionTable);

            WritePrizePrizeSelectionTableTextToConsole(prizeSelectionTable);
        }

        [TestMethod]
        public void CreatePrizeSelectionTable_FFRK_Simple_Guaranteed_WithoutName_Success()
        {
            IList<PrizeCategorySpecification> specs = GetFFRKPrizeCategorySpecifications_Simple_Guaranteed_WithoutName();

            //TEST
            IList<PrizeSelectionRow> prizeSelectionTable = _prizeSelectionTableHelper.GetPrizeSelectionTable(specs);

            Assert.IsNotNull(prizeSelectionTable);

            WritePrizePrizeSelectionTableTextToConsole(prizeSelectionTable);
        }

        [TestMethod]
        public void CreatePrizeSelectionTable_FFRK_Detailed_Guaranteed_WithName_Success()
        {
            IList<PrizeCategorySpecification> specs = GetFFRKPrizeCategorySpecifications_Detailed_Guaranteed_WithName(1);

            //TEST
            IList<PrizeSelectionRow> prizeSelectionTable = _prizeSelectionTableHelper.GetPrizeSelectionTable(specs);

            Assert.IsNotNull(prizeSelectionTable);

            WritePrizePrizeSelectionTableTextToConsole(prizeSelectionTable);
        }

        [TestMethod]
        public void CreatePrizeSelectionTable_FFRK_Detailed_Guaranteed_WithoutName_Success()
        {
            IList<PrizeCategorySpecification> specs = GetFFRKPrizeCategorySpecifications_Detailed_Guaranteed_WithoutName();

            //TEST
            IList<PrizeSelectionRow> prizeSelectionTable = _prizeSelectionTableHelper.GetPrizeSelectionTable(specs);

            Assert.IsNotNull(prizeSelectionTable);

            WritePrizePrizeSelectionTableTextToConsole(prizeSelectionTable);
        }

        [TestMethod]
        public void CreatePrizeSelectionTable_FFRK_Simple_Variable_WithName_Success()
        {
            IList<PrizeCategorySpecification> specs = GetFFRKPrizeCategorySpecifications_Simple_Variable_WithName(1);

            //TEST
            IList<PrizeSelectionRow> prizeSelectionTable = _prizeSelectionTableHelper.GetPrizeSelectionTable(specs);

            Assert.IsNotNull(prizeSelectionTable);

            WritePrizePrizeSelectionTableTextToConsole(prizeSelectionTable);
        }

        [TestMethod]
        public void CreatePrizeSelectionTable_FFRK_Simple_Variable_WithoutName_Success()
        {
            IList<PrizeCategorySpecification> specs = GetFFRKPrizeCategorySpecifications_Simple_Variable_WithoutName();

            //TEST
            IList<PrizeSelectionRow> prizeSelectionTable = _prizeSelectionTableHelper.GetPrizeSelectionTable(specs);

            Assert.IsNotNull(prizeSelectionTable);

            WritePrizePrizeSelectionTableTextToConsole(prizeSelectionTable);
        }

        [TestMethod]
        public void CreatePrizeSelectionTable_FFRK_Detailed_Variable_WithName_Success()
        {
            IList<PrizeCategorySpecification> specs = GetFFRKPrizeCategorySpecifications_Detailed_Variable_WithName(1);

            //TEST
            IList<PrizeSelectionRow> prizeSelectionTable = _prizeSelectionTableHelper.GetPrizeSelectionTable(specs);

            Assert.IsNotNull(prizeSelectionTable);

            WritePrizePrizeSelectionTableTextToConsole(prizeSelectionTable);
        }

        [TestMethod]
        public void CreatePrizeSelectionTable_FFRK_Detailed_Variable_WithoutName_Success()
        {
            IList<PrizeCategorySpecification> specs = GetFFRKPrizeCategorySpecifications_Detailed_Variable_WithoutName();

            //TEST
            IList<PrizeSelectionRow> prizeSelectionTable = _prizeSelectionTableHelper.GetPrizeSelectionTable(specs);

            Assert.IsNotNull(prizeSelectionTable);

            WritePrizePrizeSelectionTableTextToConsole(prizeSelectionTable);
        }
        #endregion

        //test creating prize result tables
        #region SelectionEngine

        [TestMethod]
        public void SelectPrizes_Single_FFRK_Simple_WithNames_Success()
        {
            IList<PrizeCategorySpecification> specsFFRKSimpleGuaranteedWithNames = 
                GetFFRKPrizeCategorySpecifications_Simple_Guaranteed_WithName(1);

            IList<PrizeSelectionRow> prizeSelectionRowsFFRKSimpleGuaranteedWithNames =
                _prizeSelectionTableHelper.GetPrizeSelectionTable(specsFFRKSimpleGuaranteedWithNames);

            IList<PrizeCategorySpecification> specsFFRKSimpleVariableWithNames =
                GetFFRKPrizeCategorySpecifications_Simple_Variable_WithName(1);

            IList<PrizeSelectionRow> prizeSelectionRowsFFRKSimpleVariableWithNames =
                _prizeSelectionTableHelper.GetPrizeSelectionTable(specsFFRKSimpleVariableWithNames);

            IList<SelectionDomain> selectionDomains = new List<SelectionDomain>()
                                                      {
                                                          new SelectionDomain()
                                                          {
                                                              PrizesToSelectFromDomainCount = 1,
                                                              SelectionDomainName = "Guaranteed",
                                                              PrizeSelectionTable = prizeSelectionRowsFFRKSimpleGuaranteedWithNames
                                                          },

                                                          new SelectionDomain()
                                                          {
                                                              PrizesToSelectFromDomainCount = 10,
                                                              SelectionDomainName = "Variable",
                                                              PrizeSelectionTable = prizeSelectionRowsFFRKSimpleVariableWithNames
                                                          }

                                                      };

            IList<PrizeResultRow> prizeResultsTable = _selectionEngine.SelectPrizes(selectionDomains);

            Assert.IsNotNull(prizeResultsTable);

            WritePrizeResultTableTextToConsole(prizeResultsTable);
        }

        [TestMethod]
        public void SelectPrizes_Single_FFRK_Simple_WithoutNames_Success()
        {
            IList<PrizeCategorySpecification> specsFFRKSimpleGuaranteedWithoutNames =
                GetFFRKPrizeCategorySpecifications_Simple_Guaranteed_WithoutName();

            IList<PrizeSelectionRow> prizeSelectionRowsFFRKSimpleGuaranteedWithoutNames =
                _prizeSelectionTableHelper.GetPrizeSelectionTable(specsFFRKSimpleGuaranteedWithoutNames);

            IList<PrizeCategorySpecification> specsFFRKSimpleVariableWithoutNames =
                GetFFRKPrizeCategorySpecifications_Simple_Variable_WithoutName();

            IList<PrizeSelectionRow> prizeSelectionRowsFFRKSimpleVariableWithoutNames =
                _prizeSelectionTableHelper.GetPrizeSelectionTable(specsFFRKSimpleVariableWithoutNames);

            IList<SelectionDomain> selectionDomains = new List<SelectionDomain>()
                                                      {
                                                          new SelectionDomain()
                                                          {
                                                              PrizesToSelectFromDomainCount = 1,
                                                              SelectionDomainName = "Guaranteed",
                                                              PrizeSelectionTable = prizeSelectionRowsFFRKSimpleGuaranteedWithoutNames
                                                          },

                                                          new SelectionDomain()
                                                          {
                                                              PrizesToSelectFromDomainCount = 10,
                                                              SelectionDomainName = "Variable",
                                                              PrizeSelectionTable = prizeSelectionRowsFFRKSimpleVariableWithoutNames
                                                          }

                                                      };

            IList<PrizeResultRow> prizeResultsTable = _selectionEngine.SelectPrizes(selectionDomains);

            Assert.IsNotNull(prizeResultsTable);

            WritePrizeResultTableTextToConsole(prizeResultsTable);
        }

        [TestMethod]
        public void SelectPrizes_Single_FFRK_Detailed_WithNames_Success()
        {
            IList<PrizeCategorySpecification> specsFFRKDetailedGuaranteedWithNames =
                GetFFRKPrizeCategorySpecifications_Detailed_Guaranteed_WithName(1);

            IList<PrizeSelectionRow> prizeSelectionRowsFFRKDetailedGuaranteedWithNames =
                _prizeSelectionTableHelper.GetPrizeSelectionTable(specsFFRKDetailedGuaranteedWithNames);

            IList<PrizeCategorySpecification> specsFFRKDetailedVariableWithNames =
                GetFFRKPrizeCategorySpecifications_Detailed_Variable_WithName(1);

            IList<PrizeSelectionRow> prizeSelectionRowsFFRKDetailedVariableWithNames =
                _prizeSelectionTableHelper.GetPrizeSelectionTable(specsFFRKDetailedVariableWithNames);

            IList<SelectionDomain> selectionDomains = new List<SelectionDomain>()
                                                      {
                                                          new SelectionDomain()
                                                          {
                                                              PrizesToSelectFromDomainCount = 1,
                                                              SelectionDomainName = "Guaranteed",
                                                              PrizeSelectionTable = prizeSelectionRowsFFRKDetailedGuaranteedWithNames
                                                          },

                                                          new SelectionDomain()
                                                          {
                                                              PrizesToSelectFromDomainCount = 10,
                                                              SelectionDomainName = "Variable",
                                                              PrizeSelectionTable = prizeSelectionRowsFFRKDetailedVariableWithNames
                                                          }

                                                      };

            IList<PrizeResultRow> prizeResultsTable = _selectionEngine.SelectPrizes(selectionDomains);

            Assert.IsNotNull(prizeResultsTable);

            WritePrizeResultTableTextToConsole(prizeResultsTable);
        }

        [TestMethod]
        public void SelectPrizes_Single_FFRK_Detailed_WithoutNames_Success()
        {
            IList<PrizeCategorySpecification> specsFFRKDetailedGuaranteedWithoutNames =
                GetFFRKPrizeCategorySpecifications_Detailed_Guaranteed_WithoutName();

            IList<PrizeSelectionRow> prizeSelectionRowsFFRKDetailedGuaranteedWithoutNames =
                _prizeSelectionTableHelper.GetPrizeSelectionTable(specsFFRKDetailedGuaranteedWithoutNames);

            IList<PrizeCategorySpecification> specsFFRKDetailedVariableWithoutNames =
                GetFFRKPrizeCategorySpecifications_Detailed_Variable_WithoutName();

            IList<PrizeSelectionRow> prizeSelectionRowsFFRKDetailedVariableWithoutNames =
                _prizeSelectionTableHelper.GetPrizeSelectionTable(specsFFRKDetailedVariableWithoutNames);

            IList<SelectionDomain> selectionDomains = new List<SelectionDomain>()
                                                      {
                                                          new SelectionDomain()
                                                          {
                                                              PrizesToSelectFromDomainCount = 1,
                                                              SelectionDomainName = "Guaranteed",
                                                              PrizeSelectionTable = prizeSelectionRowsFFRKDetailedGuaranteedWithoutNames
                                                          },

                                                          new SelectionDomain()
                                                          {
                                                              PrizesToSelectFromDomainCount = 10,
                                                              SelectionDomainName = "Variable",
                                                              PrizeSelectionTable = prizeSelectionRowsFFRKDetailedVariableWithoutNames
                                                          }

                                                      };

            IList<PrizeResultRow> prizeResultsTable = _selectionEngine.SelectPrizes(selectionDomains);

            Assert.IsNotNull(prizeResultsTable);

            WritePrizeResultTableTextToConsole(prizeResultsTable);
        }



        [TestMethod]
        public void SelectPrizes_Multi_FFRK_Simple_WithNames_Success()
        {
            int multiPullCount = 4;

            IList<PrizeCategorySpecification> specsFFRKSimpleGuaranteedWithNames =
                GetFFRKPrizeCategorySpecifications_Simple_Guaranteed_WithName(1);

            IList<PrizeSelectionRow> prizeSelectionRowsFFRKSimpleGuaranteedWithNames =
                _prizeSelectionTableHelper.GetPrizeSelectionTable(specsFFRKSimpleGuaranteedWithNames);

            IList<PrizeCategorySpecification> specsFFRKSimpleVariableWithNames =
                GetFFRKPrizeCategorySpecifications_Simple_Variable_WithName(1);

            IList<PrizeSelectionRow> prizeSelectionRowsFFRKSimpleVariableWithNames =
                _prizeSelectionTableHelper.GetPrizeSelectionTable(specsFFRKSimpleVariableWithNames);

            IList<SelectionDomain> selectionDomains = new List<SelectionDomain>()
                                                      {
                                                          new SelectionDomain()
                                                          {
                                                              PrizesToSelectFromDomainCount = 1,
                                                              SelectionDomainName = "Guaranteed",
                                                              PrizeSelectionTable = prizeSelectionRowsFFRKSimpleGuaranteedWithNames
                                                          },

                                                          new SelectionDomain()
                                                          {
                                                              PrizesToSelectFromDomainCount = 10,
                                                              SelectionDomainName = "Variable",
                                                              PrizeSelectionTable = prizeSelectionRowsFFRKSimpleVariableWithNames
                                                          }

                                                      };

            IList<PrizeResultRow> prizeResultsTable = _selectionEngine.SelectPrizes(selectionDomains, multiPullCount);

            Assert.IsNotNull(prizeResultsTable);

            WritePrizeResultTableTextToConsole(prizeResultsTable);
        }

        [TestMethod]
        public void SelectPrizes_Multi_FFRK_Simple_WithoutNames_Success()
        {
            int multiPullCount = 4;

            IList<PrizeCategorySpecification> specsFFRKSimpleGuaranteedWithoutNames =
                 GetFFRKPrizeCategorySpecifications_Simple_Guaranteed_WithoutName();

            IList<PrizeSelectionRow> prizeSelectionRowsFFRKSimpleGuaranteedWithoutNames =
                _prizeSelectionTableHelper.GetPrizeSelectionTable(specsFFRKSimpleGuaranteedWithoutNames);

            IList<PrizeCategorySpecification> specsFFRKSimpleVariableWithoutNames =
                GetFFRKPrizeCategorySpecifications_Simple_Variable_WithoutName();

            IList<PrizeSelectionRow> prizeSelectionRowsFFRKSimpleVariableWithoutNames =
                _prizeSelectionTableHelper.GetPrizeSelectionTable(specsFFRKSimpleVariableWithoutNames);

            IList<SelectionDomain> selectionDomains = new List<SelectionDomain>()
                                                      {
                                                          new SelectionDomain()
                                                          {
                                                              PrizesToSelectFromDomainCount = 1,
                                                              SelectionDomainName = "Guaranteed",
                                                              PrizeSelectionTable = prizeSelectionRowsFFRKSimpleGuaranteedWithoutNames
                                                          },

                                                          new SelectionDomain()
                                                          {
                                                              PrizesToSelectFromDomainCount = 10,
                                                              SelectionDomainName = "Variable",
                                                              PrizeSelectionTable = prizeSelectionRowsFFRKSimpleVariableWithoutNames
                                                          }

                                                      };

            IList<PrizeResultRow> prizeResultsTable = _selectionEngine.SelectPrizes(selectionDomains, multiPullCount);

            Assert.IsNotNull(prizeResultsTable);

            WritePrizeResultTableTextToConsole(prizeResultsTable);
        }

        [TestMethod]
        public void SelectPrizes_Multi_FFRK_Detailed_WithNames_Success()
        {
            int multiPullCount = 4;

            IList<PrizeCategorySpecification> specsFFRKDetailedGuaranteedWithNames =
                GetFFRKPrizeCategorySpecifications_Detailed_Guaranteed_WithName(1);

            IList<PrizeSelectionRow> prizeSelectionRowsFFRKDetailedGuaranteedWithNames =
                _prizeSelectionTableHelper.GetPrizeSelectionTable(specsFFRKDetailedGuaranteedWithNames);

            IList<PrizeCategorySpecification> specsFFRKDetailedVariableWithNames =
                GetFFRKPrizeCategorySpecifications_Detailed_Variable_WithName(1);

            IList<PrizeSelectionRow> prizeSelectionRowsFFRKDetailedVariableWithNames =
                _prizeSelectionTableHelper.GetPrizeSelectionTable(specsFFRKDetailedVariableWithNames);

            IList<SelectionDomain> selectionDomains = new List<SelectionDomain>()
                                                      {
                                                          new SelectionDomain()
                                                          {
                                                              PrizesToSelectFromDomainCount = 1,
                                                              SelectionDomainName = "Guaranteed",
                                                              PrizeSelectionTable = prizeSelectionRowsFFRKDetailedGuaranteedWithNames
                                                          },

                                                          new SelectionDomain()
                                                          {
                                                              PrizesToSelectFromDomainCount = 10,
                                                              SelectionDomainName = "Variable",
                                                              PrizeSelectionTable = prizeSelectionRowsFFRKDetailedVariableWithNames
                                                          }

                                                      };

            IList<PrizeResultRow> prizeResultsTable = _selectionEngine.SelectPrizes(selectionDomains, multiPullCount);

            Assert.IsNotNull(prizeResultsTable);

            WritePrizeResultTableTextToConsole(prizeResultsTable);
        }

        [TestMethod]
        public void SelectPrizes_Multi_FFRK_Detailed_WithoutNames_Success()
        {
            int multiPullCount = 4;

            IList<PrizeCategorySpecification> specsFFRKDetailedGuaranteedWithoutNames =
                GetFFRKPrizeCategorySpecifications_Detailed_Guaranteed_WithoutName();

            IList<PrizeSelectionRow> prizeSelectionRowsFFRKDetailedGuaranteedWithoutNames =
                _prizeSelectionTableHelper.GetPrizeSelectionTable(specsFFRKDetailedGuaranteedWithoutNames);

            IList<PrizeCategorySpecification> specsFFRKDetailedVariableWithoutNames =
                GetFFRKPrizeCategorySpecifications_Detailed_Variable_WithoutName();

            IList<PrizeSelectionRow> prizeSelectionRowsFFRKDetailedVariableWithoutNames =
                _prizeSelectionTableHelper.GetPrizeSelectionTable(specsFFRKDetailedVariableWithoutNames);

            IList<SelectionDomain> selectionDomains = new List<SelectionDomain>()
                                                      {
                                                          new SelectionDomain()
                                                          {
                                                              PrizesToSelectFromDomainCount = 1,
                                                              SelectionDomainName = "Guaranteed",
                                                              PrizeSelectionTable = prizeSelectionRowsFFRKDetailedGuaranteedWithoutNames
                                                          },

                                                          new SelectionDomain()
                                                          {
                                                              PrizesToSelectFromDomainCount = 10,
                                                              SelectionDomainName = "Variable",
                                                              PrizeSelectionTable = prizeSelectionRowsFFRKDetailedVariableWithoutNames
                                                          }

                                                      };

            IList<PrizeResultRow> prizeResultsTable = _selectionEngine.SelectPrizes(selectionDomains, multiPullCount);

            Assert.IsNotNull(prizeResultsTable);

            WritePrizeResultTableTextToConsole(prizeResultsTable);
        }

        #endregion

        #region Selection Success Calculator
        
        [DataTestMethod]
        [DataRow(2, 4)]
        [DataRow(3, 4)]
        [DataRow(4, 4)]
        [DataRow(2, 6)]
        [DataRow(3, 6)]
        [DataRow(4, 6)]

        public void GetChanceToMeetSuccessCriteriaForFixedSelectionCount_FFRK_Simple_Success(int prizesSought, int selectionCount)
        {            
            //success criteria
            int prizeCountOnBanner = 14; //14 prizes on banner
            IDictionary<int, int> successCriteria = _prizeResultsTableHelper.GetEmptyPrizeResultsSummary(prizeCountOnBanner);

            for (int i = 1; i <= prizesSought; i++)
            {
                successCriteria[i] = 1;
            }


            //selectionDomains
            IList<PrizeCategorySpecification> specsFFRKSimpleGuaranteedWithoutNames =
                GetFFRKPrizeCategorySpecifications_Simple_Guaranteed_WithoutName();

            IList<PrizeSelectionRow> prizeSelectionRowsFFRKSimpleGuaranteedWithoutNames =
                _prizeSelectionTableHelper.GetPrizeSelectionTable(specsFFRKSimpleGuaranteedWithoutNames);

            IList<PrizeCategorySpecification> specsFFRKSimpleVariableWithoutNames =
                GetFFRKPrizeCategorySpecifications_Simple_Variable_WithoutName();

            IList<PrizeSelectionRow> prizeSelectionRowsFFRKSimpleVariableWithoutNames =
                _prizeSelectionTableHelper.GetPrizeSelectionTable(specsFFRKSimpleVariableWithoutNames);

            IList<SelectionDomain> selectionDomains = new List<SelectionDomain>()
                                                      {
                                                          new SelectionDomain()
                                                          {
                                                              PrizesToSelectFromDomainCount = 1,
                                                              SelectionDomainName = "Guaranteed",
                                                              PrizeSelectionTable = prizeSelectionRowsFFRKSimpleGuaranteedWithoutNames
                                                          },

                                                          new SelectionDomain()
                                                          {
                                                              PrizesToSelectFromDomainCount = 10,
                                                              SelectionDomainName = "Variable",
                                                              PrizeSelectionTable = prizeSelectionRowsFFRKSimpleVariableWithoutNames
                                                          }

                                                      };

            double results = _selectionSuccessCalculator.GetChanceToMeetSuccessCriteriaForFixedSelectionCount(
                successCriteria, selectionDomains, selectionCount);

            Assert.AreNotEqual(0, results);

            Console.WriteLine($"Chance for {selectionCount} prize selections to meet success criteria: {results}");
        }


        [DataTestMethod]
        [DataRow(2, 4)]
        [DataRow(3, 4)]
        [DataRow(4, 4)]
        [DataRow(2, 6)]
        [DataRow(3, 6)]
        [DataRow(4, 6)]
        public void GetChanceToMeetSuccessCriteriaForFixedSelectionCount_FFRK_Detailed_Success(int prizesSought, int selectionCount)
        {
            //success criteria
            int prizeCountOnBanner = 16; //16 prizes on banner
            IDictionary<int, int> successCriteria = _prizeResultsTableHelper.GetEmptyPrizeResultsSummary(prizeCountOnBanner);

            for (int i = 1; i <= prizesSought; i++)
            {
                successCriteria[i] = 1;
            }


            //selectionDomains
            IList<PrizeCategorySpecification> specsFFRKDetailedGuaranteedWithoutNames =
                GetFFRKPrizeCategorySpecifications_Detailed_Guaranteed_WithoutName();

            IList<PrizeSelectionRow> prizeSelectionRowsFFRKDetailedGuaranteedWithoutNames =
                _prizeSelectionTableHelper.GetPrizeSelectionTable(specsFFRKDetailedGuaranteedWithoutNames);

            IList<PrizeCategorySpecification> specsFFRKDetailedVariableWithoutNames =
                GetFFRKPrizeCategorySpecifications_Detailed_Variable_WithoutName();

            IList<PrizeSelectionRow> prizeSelectionRowsFFRKDetailedVariableWithoutNames =
                _prizeSelectionTableHelper.GetPrizeSelectionTable(specsFFRKDetailedVariableWithoutNames);

            IList<SelectionDomain> selectionDomains = new List<SelectionDomain>()
                                                      {
                                                          new SelectionDomain()
                                                          {
                                                              PrizesToSelectFromDomainCount = 1,
                                                              SelectionDomainName = "Guaranteed",
                                                              PrizeSelectionTable = prizeSelectionRowsFFRKDetailedGuaranteedWithoutNames
                                                          },

                                                          new SelectionDomain()
                                                          {
                                                              PrizesToSelectFromDomainCount = 10,
                                                              SelectionDomainName = "Variable",
                                                              PrizeSelectionTable = prizeSelectionRowsFFRKDetailedVariableWithoutNames
                                                          }

                                                      };

            double results = _selectionSuccessCalculator.GetChanceToMeetSuccessCriteriaForFixedSelectionCount(
                successCriteria, selectionDomains, selectionCount);

            Assert.AreNotEqual(0, results);

            Console.WriteLine($"Chance for {selectionCount} prize selections to meet success criteria: {results}");
        }

        [DataTestMethod]
        [DataRow(2, 4, 1)]
        [DataRow(3, 4, 2)]
        [DataRow(4, 4, 2)]
        [DataRow(2, 6, 1)]
        [DataRow(3, 6, 2)]
        [DataRow(4, 6, 2)]
        public void GetChanceToMeetSuccessCriteriaSubsetForFixedSelectionCount_FFRK_Detailed_Success(int prizesSought, int selectionCount, int subsetSize)
        {
            //success criteria
            int prizeCountOnBanner = 16; //16 prizes on banner
            IDictionary<int, int> successCriteria = _prizeResultsTableHelper.GetEmptyPrizeResultsSummary(prizeCountOnBanner);

            for (int i = 1; i <= prizesSought; i++)
            {
                successCriteria[i] = 1;
            }

            //selectionDomains
            IList<PrizeCategorySpecification> specsFFRKDetailedGuaranteedWithoutNames =
                GetFFRKPrizeCategorySpecifications_Detailed_Guaranteed_WithoutName();

            IList<PrizeSelectionRow> prizeSelectionRowsFFRKDetailedGuaranteedWithoutNames =
                _prizeSelectionTableHelper.GetPrizeSelectionTable(specsFFRKDetailedGuaranteedWithoutNames);

            IList<PrizeCategorySpecification> specsFFRKDetailedVariableWithoutNames =
                GetFFRKPrizeCategorySpecifications_Detailed_Variable_WithoutName();

            IList<PrizeSelectionRow> prizeSelectionRowsFFRKDetailedVariableWithoutNames =
                _prizeSelectionTableHelper.GetPrizeSelectionTable(specsFFRKDetailedVariableWithoutNames);

            IList<SelectionDomain> selectionDomains = new List<SelectionDomain>()
                                                      {
                                                          new SelectionDomain()
                                                          {
                                                              PrizesToSelectFromDomainCount = 1,
                                                              SelectionDomainName = "Guaranteed",
                                                              PrizeSelectionTable = prizeSelectionRowsFFRKDetailedGuaranteedWithoutNames
                                                          },

                                                          new SelectionDomain()
                                                          {
                                                              PrizesToSelectFromDomainCount = 10,
                                                              SelectionDomainName = "Variable",
                                                              PrizeSelectionTable = prizeSelectionRowsFFRKDetailedVariableWithoutNames
                                                          }

                                                      };

            double results = _selectionSuccessCalculator.GetChanceToMeetSuccessCriteriaSubsetForFixedSelectionCount(
                successCriteria, selectionDomains, selectionCount, subsetSize);

            Assert.AreNotEqual(0, results);

            Console.WriteLine($"Chance for {selectionCount} prize selections to meet success criteria: {results}");
        }


        [DataTestMethod]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(4)]
        [DataRow(5)]
        [DataRow(6)]

        public void GetResultsForPullsUntilSuccess_Simple_Success(int prizesSought)
        {
            //success criteria
            int prizeCountOnBanner = 14; //14 prizes on banner
            IDictionary<int, int> successCriteria = _prizeResultsTableHelper.GetEmptyPrizeResultsSummary(prizeCountOnBanner);

            for (int i = 1; i <= prizesSought; i++)
            {
                successCriteria[i] = 1;
            }


            //selectionDomains
            IList<PrizeCategorySpecification> specsFFRKSimpleGuaranteedWithoutNames =
                GetFFRKPrizeCategorySpecifications_Simple_Guaranteed_WithoutName();

            IList<PrizeSelectionRow> prizeSelectionRowsFFRKSimpleGuaranteedWithoutNames =
                _prizeSelectionTableHelper.GetPrizeSelectionTable(specsFFRKSimpleGuaranteedWithoutNames);

            IList<PrizeCategorySpecification> specsFFRKSimpleVariableWithoutNames =
                GetFFRKPrizeCategorySpecifications_Simple_Variable_WithoutName();

            IList<PrizeSelectionRow> prizeSelectionRowsFFRKSimpleVariableWithoutNames =
                _prizeSelectionTableHelper.GetPrizeSelectionTable(specsFFRKSimpleVariableWithoutNames);

            IList<SelectionDomain> selectionDomains = new List<SelectionDomain>()
                                                      {
                                                          new SelectionDomain()
                                                          {
                                                              PrizesToSelectFromDomainCount = 1,
                                                              SelectionDomainName = "Guaranteed",
                                                              PrizeSelectionTable = prizeSelectionRowsFFRKSimpleGuaranteedWithoutNames
                                                          },

                                                          new SelectionDomain()
                                                          {
                                                              PrizesToSelectFromDomainCount = 10,
                                                              SelectionDomainName = "Variable",
                                                              PrizeSelectionTable = prizeSelectionRowsFFRKSimpleVariableWithoutNames
                                                          }

                                                      };

            PrizeSelectionsForSuccessInfo results = _selectionSuccessCalculator.GetResultsForPullsUntilSuccess(successCriteria, selectionDomains);

            Assert.IsNotNull(results);

            Console.WriteLine(results.ToString());
        }

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(4)]
        [DataRow(5)]
        [DataRow(6)]

        public void GetResultsForPullsUntilSuccess_Detailed_Success(int prizesSought)
        {
            //success criteria
            int prizeCountOnBanner = 16; //16 prizes on banner
            IDictionary<int, int> successCriteria = _prizeResultsTableHelper.GetEmptyPrizeResultsSummary(prizeCountOnBanner);

            for (int i = 1; i <= prizesSought; i++)
            {
                successCriteria[i] = 1;
            }


            //selectionDomains

            IList<PrizeCategorySpecification> specsFFRKDetailedGuaranteedWithoutNames =
                GetFFRKPrizeCategorySpecifications_Detailed_Guaranteed_WithoutName();

            IList<PrizeSelectionRow> prizeSelectionRowsFFRKDetailedGuaranteedWithoutNames =
                _prizeSelectionTableHelper.GetPrizeSelectionTable(specsFFRKDetailedGuaranteedWithoutNames);

            IList<PrizeCategorySpecification> specsFFRKDetailedVariableWithoutNames =
                GetFFRKPrizeCategorySpecifications_Detailed_Variable_WithoutName();

            IList<PrizeSelectionRow> prizeSelectionRowsFFRKDetailedVariableWithoutNames =
                _prizeSelectionTableHelper.GetPrizeSelectionTable(specsFFRKDetailedVariableWithoutNames);

            IList<SelectionDomain> selectionDomains = new List<SelectionDomain>()
                                                      {
                                                          new SelectionDomain()
                                                          {
                                                              PrizesToSelectFromDomainCount = 1,
                                                              SelectionDomainName = "Guaranteed",
                                                              PrizeSelectionTable = prizeSelectionRowsFFRKDetailedGuaranteedWithoutNames
                                                          },

                                                          new SelectionDomain()
                                                          {
                                                              PrizesToSelectFromDomainCount = 10,
                                                              SelectionDomainName = "Variable",
                                                              PrizeSelectionTable = prizeSelectionRowsFFRKDetailedVariableWithoutNames
                                                          }

                                                      };

            PrizeSelectionsForSuccessInfo results = _selectionSuccessCalculator.GetResultsForPullsUntilSuccess(successCriteria, selectionDomains);

            Assert.IsNotNull(results);

            Console.WriteLine(results.ToString());
        }

        [DataTestMethod]
        [DataRow(3, 1)]
        [DataRow(3, 2)]
        [DataRow(4, 2)]
        [DataRow(4, 3)]
        public void GetResultsForPullsUntilSuccessSubset_Detailed_Success(int prizesSought, int subsetSize)
        {
            //success criteria
            int prizeCountOnBanner = 16; //16 prizes on banner
            IDictionary<int, int> successCriteria = _prizeResultsTableHelper.GetEmptyPrizeResultsSummary(prizeCountOnBanner);

            for (int i = 1; i <= prizesSought; i++)
            {
                successCriteria[i] = 1;
            }

            //selectionDomains

            IList<PrizeCategorySpecification> specsFFRKDetailedGuaranteedWithoutNames =
                GetFFRKPrizeCategorySpecifications_Detailed_Guaranteed_WithoutName();

            IList<PrizeSelectionRow> prizeSelectionRowsFFRKDetailedGuaranteedWithoutNames =
                _prizeSelectionTableHelper.GetPrizeSelectionTable(specsFFRKDetailedGuaranteedWithoutNames);

            IList<PrizeCategorySpecification> specsFFRKDetailedVariableWithoutNames =
                GetFFRKPrizeCategorySpecifications_Detailed_Variable_WithoutName();

            IList<PrizeSelectionRow> prizeSelectionRowsFFRKDetailedVariableWithoutNames =
                _prizeSelectionTableHelper.GetPrizeSelectionTable(specsFFRKDetailedVariableWithoutNames);

            IList<SelectionDomain> selectionDomains = new List<SelectionDomain>()
                                                      {
                                                          new SelectionDomain()
                                                          {
                                                              PrizesToSelectFromDomainCount = 1,
                                                              SelectionDomainName = "Guaranteed",
                                                              PrizeSelectionTable = prizeSelectionRowsFFRKDetailedGuaranteedWithoutNames
                                                          },

                                                          new SelectionDomain()
                                                          {
                                                              PrizesToSelectFromDomainCount = 10,
                                                              SelectionDomainName = "Variable",
                                                              PrizeSelectionTable = prizeSelectionRowsFFRKDetailedVariableWithoutNames
                                                          }

                                                      };

            PrizeSelectionsForSuccessInfo results = _selectionSuccessCalculator.GetResultsForPullsUntilSuccessSubset(successCriteria, selectionDomains, subsetSize);

            Assert.IsNotNull(results);

            Console.WriteLine(results.ToString());
        }
        #endregion

        #region FFRK Focused Helper Methods

        //PrizeCategorySpecifications Helpers
        public IList<PrizeCategorySpecification> GetFFRKPrizeCategorySpecifications_Simple_Guaranteed_WithName(int banner)
        {
            // 5 or 6* total
            //category 0 = on banner 5 or 6 ; 14.0% ; scaled = 60/60 * (14.0/14.0) = 1.0
            //total must be 60/60

            string onBannerFiveOrSixStarPrizeCategoryName = "5/6 *";

            double onBannerFiveOrSixStarRate = 1; // = 1.0

            IList<string> prizeNames = GetFestBannerRelicNamesSimple(banner);

            IList<PrizeCategorySpecification> specs = new List<PrizeCategorySpecification>()
                                                      {
                                                          _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
                                                              onBannerFiveOrSixStarPrizeCategoryName,
                                                              onBannerFiveOrSixStarRate,
                                                              prizeNames)
                                                      };
            return specs;
        }

        public IList<PrizeCategorySpecification> GetFFRKPrizeCategorySpecifications_Simple_Guaranteed_WithoutName()
        {
            // 5 or 6* total
            //category 0 = on banner 5 or 6 ; 14.0% ; scaled = 60/60 * (14.0/14.0) = 1.0
            //total must be 60/60

            string onBannerFiveOrSixStarPrizeCategoryName = "5/6 *";

            double onBannerFiveOrSixStarRate = 1; // = 1.0

            int onBannerFiveOrSixStarCategoryPrizeCountDefault = 14;

            IList<PrizeCategorySpecification> specs = new List<PrizeCategorySpecification>()
                                                      {
                                                          _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
                                                              onBannerFiveOrSixStarPrizeCategoryName,
                                                              onBannerFiveOrSixStarRate,
                                                              onBannerFiveOrSixStarCategoryPrizeCountDefault)
                                                      };
            return specs;
        }

        public IList<PrizeCategorySpecification> GetFFRKPrizeCategorySpecifications_Detailed_Guaranteed_WithName(int banner)
        {
            string onBannerFiveOrSixStaPrizeCategoryName = "5/6*";
            string offBannerSixStarPrizeCategoryName = "OffBan 6*";
            string offBannerFiveStarPrizeCategoryName = "OffBan 5*";

            double onBannerFiveOrSixStarRate = 1 * (14.0 / 14.04); // = 0.997150997 etc
            double offBannerSixStarRate = (0.02 / 14.04); // = .001424501 etc
            double offBannerFiveStarRate = (0.02 / 14.04); // = .001424501 etc

            IList<string> prizeNames = GetFestBannerRelicNamesSimple(banner);

            IList<PrizeCategorySpecification> specs = new List<PrizeCategorySpecification>()
                                                      {
                                                          _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
                                                              onBannerFiveOrSixStaPrizeCategoryName,
                                                              onBannerFiveOrSixStarRate,
                                                              prizeNames),

                                                          _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
                                                              offBannerSixStarPrizeCategoryName,
                                                              offBannerSixStarRate,
                                                              new List<string>(){offBannerSixStarPrizeCategoryName}),

                                                          _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
                                                              offBannerFiveStarPrizeCategoryName,
                                                              offBannerFiveStarRate,
                                                              new List<string>(){offBannerFiveStarPrizeCategoryName})
                                                      };
            return specs;
        }

        public IList<PrizeCategorySpecification> GetFFRKPrizeCategorySpecifications_Detailed_Guaranteed_WithoutName()
        {
            // 5 or 6* total
            //category 0 = on banner 5 or 6 ; 14.0% ; scaled = 60/60 * (14.0/14.04) = 0.997150997 etc
            //category 1 = off banner 6; .02 % ; scaled = 60/60 * (0.02/14.04) = .001424501 etc
            //category 2 = of banner 4; .02 % ;  scaled = 60/60 * (0.02/14.04) = .001424501 etc
            //total must be 60/60

            string onBannerFiveOrSixStaPrizeCategoryName = "5/6*";
            string offBannerSixStarPrizeCategoryName = "OffBan 6*";
            string offBannerFiveStarPrizeCategoryName = "OffBan 5*";

            double onBannerFiveOrSixStarRate = 1 * (14.0 / 14.04); // = 0.997150997 etc
            double offBannerSixStarRate = (0.02 / 14.04); // = .001424501 etc
            double offBannerFiveStarRate = (0.02 / 14.04); // = .001424501 etc

            int onBannerFiveOrSixStarCategoryPrizeCountDefault = 14;
            int offBannerSixStarCategoryPrizeCountDefault = 1;
            int offBannerFiveStarCategoryPrizeCountDefault = 1;

            IList<PrizeCategorySpecification> specs = new List<PrizeCategorySpecification>()
                        {
                            _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
                                onBannerFiveOrSixStaPrizeCategoryName,
                                onBannerFiveOrSixStarRate,
                                onBannerFiveOrSixStarCategoryPrizeCountDefault),

                            _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
                                offBannerSixStarPrizeCategoryName,
                                offBannerSixStarRate,
                                offBannerSixStarCategoryPrizeCountDefault),

                            _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
                                offBannerFiveStarPrizeCategoryName,
                                offBannerFiveStarRate,
                                offBannerFiveStarCategoryPrizeCountDefault)
                        };

            return specs;
        }
  
        public IList<PrizeCategorySpecification> GetFFRKPrizeCategorySpecifications_Simple_Variable_WithName(int banner)
        {
            string onBannerFiveOrSixStaPrizeCategoryName = "5/6 *";

            double onBannerFiveOrSixStarRate = 7.0 / 60.0; // = 0.11666666666666666666666666666667

            IList<string> prizeNames = GetFestBannerRelicNamesSimple(banner);

            IList<PrizeCategorySpecification> specs = new List<PrizeCategorySpecification>()
                                                      {
                                                          _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
                                                              onBannerFiveOrSixStaPrizeCategoryName,
                                                              onBannerFiveOrSixStarRate,
                                                              prizeNames)
                                                      };
            return specs;
        }

        public IList<PrizeCategorySpecification> GetFFRKPrizeCategorySpecifications_Simple_Variable_WithoutName()
        {
            string onBannerFiveOrSixStaPrizeCategoryName = "5/6 *";

            double onBannerFiveOrSixStarRate = 7.0 / 60.0; // = 0.11666666666666666666666666666667

            int onBannerFiveOrSixStarCategoryPrizeCountDefault = 14;

            IList<PrizeCategorySpecification> specs = new List<PrizeCategorySpecification>()
                                                      {
                                                          _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
                                                              onBannerFiveOrSixStaPrizeCategoryName,
                                                              onBannerFiveOrSixStarRate,
                                                              onBannerFiveOrSixStarCategoryPrizeCountDefault)
                                                      };
            return specs;
        }

        public IList<PrizeCategorySpecification> GetFFRKPrizeCategorySpecifications_Detailed_Variable_WithName(int banner)
        {
            string onBannerFiveOrSixStarPrizeCategoryName = "5/6 *";
            string offBannerSixStarPrizeCategoryName = "OffBan 6*";
            string offBannerFiveStarPrizeCategoryName = "OffBan 5*";

            double onBannerFiveOrSixStarRate = 7.0 / 60.0; // = 0.11666666666666666666666666666667
            double offBannerSixStarRate = (0.02 / 14.04); // = .001424501 etc
            double offBannerFiveStarRate = (0.02 / 14.04); // = .001424501 etc

            IList<string> prizeNames = GetFestBannerRelicNamesSimple(banner);

            IList<PrizeCategorySpecification> specs = new List<PrizeCategorySpecification>()
                                                      {
                                                          _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
                                                              onBannerFiveOrSixStarPrizeCategoryName,
                                                              onBannerFiveOrSixStarRate,
                                                              prizeNames),

                                                          _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
                                                              offBannerSixStarPrizeCategoryName,
                                                              offBannerSixStarRate,
                                                              new List<string>(){offBannerSixStarPrizeCategoryName}),

                                                          _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
                                                              offBannerFiveStarPrizeCategoryName,
                                                              offBannerFiveStarRate,
                                                              new List<string>(){offBannerFiveStarPrizeCategoryName})
                                                      };
            return specs;
        }

        public IList<PrizeCategorySpecification> GetFFRKPrizeCategorySpecifications_Detailed_Variable_WithoutName()
        {
            string onBannerFiveOrSixStaPrizeCategoryName = "5/6 *";

            double onBannerFiveOrSixStarRate = 7.0 / 60.0; // = 0.11666666666666666666666666666667

            int onBannerFiveOrSixStarCategoryPrizeCountDefault = 14;

            IList<PrizeCategorySpecification> specs = new List<PrizeCategorySpecification>()
                                                      {
                                                          _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
                                                              onBannerFiveOrSixStaPrizeCategoryName,
                                                              onBannerFiveOrSixStarRate,
                                                              onBannerFiveOrSixStarCategoryPrizeCountDefault)
                                                      };
            return specs;
        }

        public IList<string> GetFestBannerRelicNamesSimple(int bannerIndex)
        {
            if (bannerIndex < 1 || bannerIndex > 5)
            {
                throw new ArgumentOutOfRangeException("bannerIndex must be between 1 and 5");
            }

            IList<string> relicNames = null;

            switch(bannerIndex)
            {
                case 1:
                    relicNames = new List<string>() { "Diamond Shield (T)", "Hyperion Custom (8)", "Magic Album (C)", "Aegis Grimoire (C)", "Saintly Excalibur (T)", "Urara Institute Uniform (C)", "Doom Mace (12)", "Ragnarok (6)", "Yoshiyuki Shinuchi (7)", "Akademeia Uniform (B)", "Uraras Institute Hat (C)", "Blitz Armor (8)", "Institute Hat (C)", "Flame Shield (T)" };
                    break;
                case 2:
                    relicNames = new List<string>() { "Durandal (6)", "Gilgamesh Armor (5)", "Ame no Murakumo (5)", "Apocalypse (6)", "Chicken Knife (5)", "Terra Cloak (6)", "Furinkazan (5)", "Terra Armguard (6)", "Bartz Model (5)", "Tiger Fang (8)", "Rune Staff (5)", "Gold Armor (5)", "Asura Rod (6)", "Maximillian (5)" };
                    break;
                case 3:
                    relicNames = new List<string>() { "Ultima Weapon (7)", "Axe of the Conqueror (15)", "Cloud Gauntlets (7)", "Heal Rod (7)", "Enhancer (7)", "2nd Fusion Sword (7)", "Bow of the Clever (15)", "Ultima Blade (7)", "Rune Blade (7)", "Prince Fatigues (15)", "Moogle Doll (15)", "Neo Organics (7)", "Aerith Guise (7)", "Bronze Bangle (15)" };
                    break;
                case 4:
                    relicNames = new List<string>() { "Vigilante (10)", "Enkindler (8)", "Sorcery Targe (10)", "Unsetting Sun (13)", "Blitz Ace (10)", "Third F (8)", "Axis Blade (8)", "Twilight Steel (10)", "Onyx Targe (10)", "AMP Coat (13)", "Ninja Gear (IX)", "Lion Gloves (8)", "Tidus Armguard (10)", "Warrior Targe (10)" };
                    break;
                case 5:
                    relicNames = new List<string>() { "Duel Claws (7)", "Tournesol (12)", "Garnet Tights (9)", "Tifa Gloves (7)", "Ragnarok (12)", "Power Gloves (7)", "Aegis Shield (12)", "Oversoul (7)", "Staff of Ramuh (9)", "Aristocrat Crown (11)", "Nunchak (8)", "Princess Gloves (9)", "Tifa Guise (7)", "Ashe Gloves (12)" };
                    break;

            }

            return relicNames;
        }

        #endregion

        #region Console Output Methods
        public void WritePrizeResultTableJsonToConsole(IList<PrizeResultRow> prizeResultsTable)
        {
            string results = JsonConvert.SerializeObject(prizeResultsTable, Formatting.Indented);
            Console.WriteLine(results);
        }

        public void WritePrizePrizeCategorySpecificationTableTextToConsole(IList<PrizeCategorySpecification> prizeCategorySpecificationTable)
        {
            StringBuilder sb = new StringBuilder();
            //return $"{PrizeCategoryName,-25}{ProbabilityExtentForEntireCategory,-15}{PrizeCount,-8}{PrizeNames,-50}";
            sb.AppendLine($"{"Cat Name",-25}{"Prob Extent",-15}{"Prize Count",-8}{"Prize Name",-50}");
            sb.AppendLine("-------------------------------------------------------------");

            foreach (var prizeCategorySpecificationRow in prizeCategorySpecificationTable)
            {
                sb.AppendLine(prizeCategorySpecificationRow.ToString());
            }

            Console.WriteLine(sb.ToString());
        }

        public void WritePrizePrizeSelectionTableTextToConsole(IList<PrizeSelectionRow> prizeSelectionTable)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"{"Index",-8}{"Lower Bound",-25}{"Cat Name",-20}{"Prize Name",-50}");
            sb.AppendLine("-------------------------------------------------------------------------------------");

            foreach (var prizeSelectionRow in prizeSelectionTable)
            {
                sb.AppendLine(prizeSelectionRow.ToString());
            }

            Console.WriteLine(sb.ToString());
        }

        public void WritePrizeResultTableTextToConsole(IList<PrizeResultRow> prizeResultsTable)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"{"Index",-8}{"Count",-8}{"Cat Name",-15}{"Prize Name",-50}");
            sb.AppendLine("-------------------------------------------------------------");

            foreach (var prizeResultRow in prizeResultsTable)
            {
                sb.AppendLine(prizeResultRow.ToString());
            }
            sb.AppendLine("-------------------------------------------------------------");
            sb.AppendLine($"Total: {prizeResultsTable.Sum(r => r.PrizeSelectedCount)}");
            Console.WriteLine(sb.ToString());
        }
        #endregion

        #region Superseded Helper Methods
        //just on banner relics at 14%
        //public IList<PrizeSelectionRow> GetFFRKPrizeSelectionTable_Simple_Guaranteed(IList<string> prizeNames)
        //{
        //    // 5 or 6* total
        //    //category 0 = on banner 5 or 6 ; 14.0% ; scaled = 60/60 * (14.0/14.0) = 1.0
        //    //total must be 60/60

        //    string onBannerFiveOrSixStarPrizeCategoryName = "5/6 *";

        //    double onBannerFiveOrSixStarRate = 1; // = 1.0

        //    int onBannerFiveOrSixStarCategoryPrizeCountDefault = 14;

        //    IList<PrizeCategorySpecification> specs = null;

        //    if (prizeNames != null)
        //    {
        //        specs = new List<PrizeCategorySpecification>()
        //                {
        //                    _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
        //                        onBannerFiveOrSixStarPrizeCategoryName,
        //                        onBannerFiveOrSixStarRate,
        //                        prizeNames)
        //                };
        //    }
        //    else
        //    {
        //        specs = new List<PrizeCategorySpecification>()
        //                {
        //                    _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
        //                        onBannerFiveOrSixStarPrizeCategoryName,
        //                        onBannerFiveOrSixStarRate,
        //                        onBannerFiveOrSixStarCategoryPrizeCountDefault)
        //                };
        //    }


        //    IList<PrizeSelectionRow> result = _prizeSelectionTableHelper.GetPrizeSelectionTable(specs);

        //    return result;
        //}

        ////just on banner relics at 14%
        //public IList<PrizeSelectionRow> GetFFRKPrizeSelectionTable_Simple_Variable(IList<string> prizeNames)
        //{
        //    string onBannerFiveOrSixStaPrizeCategoryName = "5/6 *";

        //    double onBannerFiveOrSixStarRate = 7.0 / 60.0; // = 0.11666666666666666666666666666667

        //    int onBannerFiveOrSixStarCategoryPrizeCountDefault = 14;

        //    IList<PrizeCategorySpecification> specs = null;

        //    if (prizeNames != null)
        //    {
        //        specs = new List<PrizeCategorySpecification>()
        //                {
        //                    _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
        //                        onBannerFiveOrSixStaPrizeCategoryName,
        //                        onBannerFiveOrSixStarRate,
        //                        prizeNames)
        //                };
        //    }
        //    else
        //    {
        //        specs = new List<PrizeCategorySpecification>()
        //                {
        //                    _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
        //                        onBannerFiveOrSixStaPrizeCategoryName,
        //                        onBannerFiveOrSixStarRate,
        //                        onBannerFiveOrSixStarCategoryPrizeCountDefault)
        //                };
        //    }

        //    IList<PrizeSelectionRow> result = _prizeSelectionTableHelper.GetPrizeSelectionTable(specs);

        //    return result;
        //}

        ////on banner relics at 14% and off banner 5/6 at 0.4% total
        //public IList<PrizeSelectionRow> GetFFRKPrizeSelectionTable_Detailed_Guaranteed(IList<string> prizeNames)
        //{
        //    // 5 or 6* total
        //    //category 0 = on banner 5 or 6 ; 14.0% ; scaled = 60/60 * (14.0/14.04) = 0.997150997 etc
        //    //category 1 = off banner 6; .02 % ; scaled = 60/60 * (0.02/14.04) = .001424501 etc
        //    //category 2 = of banner 4; .02 % ;  scaled = 60/60 * (0.02/14.04) = .001424501 etc
        //    //total must be 60/60

        //    string onBannerFiveOrSixStaPrizeCategoryName = "5/6*";
        //    string offBannerSixStarPrizeCategoryName = "OffBan 6*";
        //    string offBannerFiveStarPrizeCategoryName = "OffBan 5*";

        //    double onBannerFiveOrSixStarRate = 1 * (14.0 / 14.04); // = 0.997150997 etc
        //    double offBannerSixStarRate = (0.02 / 14.04); // = .001424501 etc
        //    double offBannerFiveStarRate = (0.02 / 14.04); // = .001424501 etc

        //    int onBannerFiveOrSixStarCategoryPrizeCountDefault = 14;
        //    int offBannerSixStarCategoryPrizeCountDefault = 1;
        //    int offBannerFiveStarCategoryPrizeCountDefault = 1;

        //    IList<PrizeCategorySpecification> specs = null;

        //    if (prizeNames != null)
        //    {
        //        specs = new List<PrizeCategorySpecification>()
        //                {
        //                    _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
        //                        onBannerFiveOrSixStaPrizeCategoryName,
        //                        onBannerFiveOrSixStarRate,
        //                        prizeNames),

        //                    _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
        //                        offBannerSixStarPrizeCategoryName,
        //                        offBannerSixStarRate,
        //                        new List<string>(){offBannerSixStarPrizeCategoryName}),

        //                    _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
        //                        offBannerFiveStarPrizeCategoryName,
        //                        offBannerFiveStarRate,
        //                        new List<string>(){offBannerFiveStarPrizeCategoryName})
        //                };
        //    }
        //    else
        //    {
        //        specs = new List<PrizeCategorySpecification>()
        //                {
        //                    _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
        //                        onBannerFiveOrSixStaPrizeCategoryName,
        //                        onBannerFiveOrSixStarRate,
        //                        onBannerFiveOrSixStarCategoryPrizeCountDefault),

        //                    _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
        //                        offBannerSixStarPrizeCategoryName,
        //                        offBannerSixStarRate,
        //                        offBannerSixStarCategoryPrizeCountDefault),

        //                    _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
        //                        offBannerFiveStarPrizeCategoryName,
        //                        offBannerFiveStarRate,
        //                        offBannerFiveStarCategoryPrizeCountDefault)
        //                };
        //    }

        //    IList<PrizeSelectionRow> result = _prizeSelectionTableHelper.GetPrizeSelectionTable(specs);

        //    return result;
        //}

        ////on banner relics at 14% and off banner 5/6 at 0.4% total
        //public IList<PrizeSelectionRow> GetFFRKPrizeSelectionTable_Detailed_Variable(IList<string> prizeNames)
        //{
        //    string onBannerFiveOrSixStaPrizeCategoryName = "5/6 *";

        //    double onBannerFiveOrSixStarRate = 7.0 / 60.0; // = 0.11666666666666666666666666666667

        //    int onBannerFiveOrSixStarCategoryPrizeCountDefault = 14;

        //    IList<PrizeCategorySpecification> specs = null;

        //    if (prizeNames != null)
        //    {
        //        specs = new List<PrizeCategorySpecification>()
        //                {
        //                    _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
        //                        onBannerFiveOrSixStaPrizeCategoryName,
        //                        onBannerFiveOrSixStarRate,
        //                        prizeNames)
        //                };
        //    }
        //    else
        //    {
        //        specs = new List<PrizeCategorySpecification>()
        //                {
        //                    _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
        //                        onBannerFiveOrSixStaPrizeCategoryName,
        //                        onBannerFiveOrSixStarRate,
        //                        onBannerFiveOrSixStarCategoryPrizeCountDefault)
        //                };
        //    }

        //    IList<PrizeSelectionRow> result = _prizeSelectionTableHelper.GetPrizeSelectionTable(specs);

        //    return result;
        //}

        //public IList<PrizeSelectionRow> GetFFRKDetailedGuaranteedRelicSelectionTable()
        //{
        //    // 5 or 6* total
        //    //category 0 = on banner 5 or 6 ; 14.0% ; scaled = 60/60 * (14.0/14.04) = 0.997150997 etc
        //    //category 1 = off banner 6; .02 % ; scaled = 60/60 * (0.02/14.04) = .001424501 etc
        //    //category 2 = of banner 4; .02 % ;  scaled = 60/60 * (0.02/14.04) = .001424501 etc
        //    //total must be 60/60

        //    double onBannerFiveOrSixStarRate = 1 * (14.0 / 14.04); // = 0.997150997 etc
        //    double offBannerSixStarRate = (0.02 / 14.04); // = .001424501 etc
        //    double offBannerFiveStarRate = (0.02 / 14.04); // = .001424501 etc


        //    double fiveOrUp = onBannerFiveOrSixStarRate + offBannerSixStarRate + offBannerFiveStarRate; //should equal 1


        //    IList<PrizeCategorySpecification> specs = new List<PrizeCategorySpecification>()
        //                        {
        //                            _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
        //                                "On Banner 5/6* Relics", onBannerFiveOrSixStarRate, 14),

        //                            _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
        //                                "Off Banner 6* Relics", offBannerSixStarRate, 1),

        //                            _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
        //                                "Off Banner 5* Relics", offBannerFiveStarRate, 1),
        //                        };



        //    IList<PrizeSelectionRow> result = _prizeSelectionTableHelper.GetPrizeSelectionTable(specs);

        //    return result;
        //}

        //public IList<PrizeSelectionRow> GetFFRKDetailedVariableRelicSelectionTable()
        //{
        //    // 5 or 6* total
        //    //total is 7/60 (.116667)
        //    //category 0 = on banner 5 or 6 ; 14.0% ; scaled = 7/60 * (14.0/14.04) = 0.1163342830009497
        //    //category 1 = off banner 6; .02 % ; scaled = 7/60 * (0.02/14.04) = 1.661918328584995251661918328585e-4
        //    //category 2 = of banner 4; .02 % ;  scaled = 7/60 * (0.02/14.04) = 1.661918328584995251661918328585e-4
        //    //total must be 7/60

        //    double onBannerFiveOrSixStarRate = 7.0 / 60.0 * (14.0 / 14.04);// = 0.1163342830009497
        //    double offBannerSixStarRate = 7.0 / 60.0 * (0.02 / 14.04);// = 1.661918328584995251661918328585e-4
        //    double offBannerFiveStarRate = 7.0 / 60.0 * (0.02 / 14.04);// = 1.661918328584995251661918328585e-4
        //    double fourStarRate = 53.0 / 60.0 * (0.25 / .8596);// = .626431
        //    double threeStarRate = 1 - (onBannerFiveOrSixStarRate + offBannerSixStarRate + offBannerFiveStarRate + fourStarRate);// = .256902

        //    double fiveOrUp = onBannerFiveOrSixStarRate + offBannerSixStarRate + offBannerFiveStarRate;
        //    double total = onBannerFiveOrSixStarRate + offBannerSixStarRate + offBannerFiveStarRate + fourStarRate + threeStarRate;

        //    IList<PrizeCategorySpecification> specs = new List<PrizeCategorySpecification>()
        //            {
        //                _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
        //                    "On Banner 5/6* Relics", onBannerFiveOrSixStarRate, 14),

        //                _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
        //                    "Off Banner 6* Relics", offBannerSixStarRate, 1),

        //                _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
        //                    "Off Banner 5* Relics", offBannerFiveStarRate, 1),

        //                _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
        //                    "4* Relics", fourStarRate, 1),

        //                _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
        //                    "3 * Relics", threeStarRate, 1)
        //            };

        //    IList<PrizeSelectionRow> result = _prizeSelectionTableHelper.GetPrizeSelectionTable(specs);

        //    return result;
        //}
        #endregion

        #region Deprecated
        //[TestMethod]
        //public void SimulateBulkPullCurrentFFRK_Success()
        //{
        //    int guaranteedRelicCount = 1;
        //    double guaranteedRelicAcquisitionRate = 1.0;

        //    int variableRelicCount = 10;
        //    double variableRelicAcquisitionRate = 0.125;

        //    int bannerRelicCount = 18;


        //    IList<SelectionDomain> selectionDomains = new List<SelectionDomain>()
        //                                                                {
        //                                                                    new SelectionDomain()
        //                                                                    {
        //                                                                        PrizesToSelectFromDomainCount = guaranteedRelicCount,
        //                                                                        SelectionDomainName = "Guaranteed",
        //                                                                        ProbabilityTable = GetFFRKDetailedGuaranteedRelicProbabilityTable()
        //                                                                    },

        //                                                                    new SelectionDomain()
        //                                                                    {
        //                                                                        PrizesToSelectFromDomainCount = variableRelicCount,
        //                                                                        SelectionDomainName = "Variable",
        //                                                                        ProbabilityTable = GetFFRKDetailedVariableRelicProbabilityTable()
        //                                                                    }
        //                                                                };

        //    IDictionary<int, int> simulatedBulkPullResults = _selectionEngine.SimulateBulkPullGeneric(bannerRelicCount, selectionDomains, _random);

        //    Assert.IsNotNull(simulatedBulkPullResults);
        //}


        //public IDictionary<int, double> GetFFRKDetailedGuaranteedRelicProbabilityTable()
        //{
        //    // 5 0r 6* total
        //    //total is 7/60 (.116667)
        //    //category 0 = on banner 5 or 6 ; 14.0% ; scaled = 7/60 * (14.0/14.04) = 0.1163342830009497
        //    //category 1 = off banner 6; .02 % ; scaled = 7/60 * (0.02/14.04) = 1.661918328584995251661918328585e-4
        //    //category 2 = of banner 4; .02 % ;  scaled = 7/60 * (0.02/14.04) = 1.661918328584995251661918328585e-4
        //    //total must be 7/60

        //    double onBannerFiveOrSixStarRate = (14.0 / 14.04);// = 0.1163342830009497
        //    double offBannerSixStarRate = (0.02 / 14.04);// = 1.661918328584995251661918328585e-4
        //    double offBannerFiveStarRate = 1.0 - (onBannerFiveOrSixStarRate + offBannerSixStarRate);// = 1.661918328584995251661918328585e-4


        //    double fiveOrUp = onBannerFiveOrSixStarRate + offBannerSixStarRate + offBannerFiveStarRate;


        //    IList<PrizeCategorySpecification> specs = new List<PrizeCategorySpecification>()
        //                                              {
        //                                                  new PrizeCategorySpecification(){ProbabilityExtentForEntireCategory = onBannerFiveOrSixStarRate, PrizeCount = 14},
        //                                                  new PrizeCategorySpecification(){ProbabilityExtentForEntireCategory = offBannerSixStarRate, PrizeCount = 1},
        //                                                  new PrizeCategorySpecification(){ProbabilityExtentForEntireCategory = offBannerFiveStarRate, PrizeCount = 1}
        //                                              };

        //    IDictionary<int, double> result = _prizeSelectionTableHelper.GetPullProbabilityTable(specs, null);

        //    return result;
        //}

        //public IDictionary<int, double> GetFFRKDetailedVariableRelicProbabilityTable()
        //{
        //    // 5 0r 6* total
        //    //total is 7/60 (.116667)
        //    //category 0 = on banner 5 or 6 ; 14.0% ; scaled = 7/60 * (14.0/14.04) = 0.1163342830009497
        //    //category 1 = off banner 6; .02 % ; scaled = 7/60 * (0.02/14.04) = 1.661918328584995251661918328585e-4
        //    //category 2 = of banner 4; .02 % ;  scaled = 7/60 * (0.02/14.04) = 1.661918328584995251661918328585e-4
        //    //total must be 7/60

        //    double onBannerFiveOrSixStarRate = 7.0 / 60.0 * (14.0 / 14.04);// = 0.1163342830009497
        //    double offBannerSixStarRate = 7.0 / 60.0 * (0.02 / 14.04);// = 1.661918328584995251661918328585e-4
        //    double offBannerFiveStarRate = 7.0 / 60.0 * (0.02 / 14.04);// = 1.661918328584995251661918328585e-4
        //    double fourStarRate = 53.0 / 60.0 * (0.25 / .8596);// = .626431
        //    double threeStarRate = 1 - (onBannerFiveOrSixStarRate + offBannerSixStarRate + offBannerFiveStarRate + fourStarRate);// = .256902

        //    double fiveOrUp = onBannerFiveOrSixStarRate + offBannerSixStarRate + offBannerFiveStarRate;
        //    double total = onBannerFiveOrSixStarRate + offBannerSixStarRate + offBannerFiveStarRate + fourStarRate + threeStarRate;

        //    IList<PrizeCategorySpecification> specs = new List<PrizeCategorySpecification>()
        //        {
        //                        new PrizeCategorySpecification(){ProbabilityExtentForEntireCategory = onBannerFiveOrSixStarRate, PrizeCount = 14},
        //                        new PrizeCategorySpecification(){ProbabilityExtentForEntireCategory = offBannerSixStarRate, PrizeCount = 1},
        //                        new PrizeCategorySpecification(){ProbabilityExtentForEntireCategory = offBannerFiveStarRate, PrizeCount = 1},
        //                        new PrizeCategorySpecification(){ProbabilityExtentForEntireCategory = fourStarRate, PrizeCount = 1},
        //                        new PrizeCategorySpecification(){ProbabilityExtentForEntireCategory = threeStarRate, PrizeCount = 1}
        //        };

        //    IDictionary<int, double> result = _prizeSelectionTableHelper.GetPullProbabilityTable(specs, null);

        //    return result;
        //}
        #endregion
    }
}
