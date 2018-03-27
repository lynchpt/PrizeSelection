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
        }

        #region PrizeSelectionTableHelper

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
                _prizeSelectionTableHelper.CreatePrizeCategorySpecification("TestCat", 0.2, new List<string>(){"Sword", "Spear", "Dagger"});

            Assert.AreEqual(3, spec.PrizeNames.Count());
        }
        #endregion


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

        #region SelectionEngine

        [TestMethod]
        public void SelectPrizes_Single_FFRKSimple_WithNames_Success()
        {
            IList<SelectionDomain> selectionDomains = new List<SelectionDomain>()
                                                      {
                                                          new SelectionDomain()
                                                          {
                                                              PrizesToSelectFromDomainCount = 1,
                                                              SelectionDomainName = "Guaranteed",
                                                              PrizeSelectionTable = GetFFRKPrizeSelectionTable_Simple_Guaranteed(GetFestBannerRelicNamesSimple(1))
                                                          },

                                                          new SelectionDomain()
                                                          {
                                                              PrizesToSelectFromDomainCount = 10,
                                                              SelectionDomainName = "Variable",
                                                              PrizeSelectionTable = GetFFRKPrizeSelectionTable_Simple_Variable(GetFestBannerRelicNamesSimple(1))
                                                          }

                                                      };

            IList<PrizeResultRow> prizeResultsTable = _selectionEngine.SelectPrizes(selectionDomains);

            Assert.IsNotNull(prizeResultsTable);

            WritePrizeResultTableTextToConsole(prizeResultsTable);
        }

        [TestMethod]
        public void SelectPrizes_Single_FFRKSimple_WithoutNames_Success()
        {
            IList<SelectionDomain> selectionDomains = new List<SelectionDomain>()
                                                      {
                                                          new SelectionDomain()
                                                          {
                                                              PrizesToSelectFromDomainCount = 1,
                                                              SelectionDomainName = "Guaranteed",
                                                              PrizeSelectionTable = GetFFRKPrizeSelectionTable_Simple_Guaranteed(null)
                                                          },

                                                          new SelectionDomain()
                                                          {
                                                              PrizesToSelectFromDomainCount = 10,
                                                              SelectionDomainName = "Variable",
                                                              PrizeSelectionTable = GetFFRKPrizeSelectionTable_Simple_Variable(null)
                                                          }

                                                      };

            IList<PrizeResultRow> prizeResultsTable = _selectionEngine.SelectPrizes(selectionDomains);

            Assert.IsNotNull(prizeResultsTable);

            WritePrizeResultTableTextToConsole(prizeResultsTable);
        }



        [TestMethod]
        public void SelectPrizes_Single_FFRKDetailed_WithNames_Success()
        {
            IList<SelectionDomain> selectionDomains = new List<SelectionDomain>()
                                                      {
                                                          new SelectionDomain()
                                                          {
                                                              PrizesToSelectFromDomainCount = 1,
                                                              SelectionDomainName = "Guaranteed",
                                                              PrizeSelectionTable = GetFFRKPrizeSelectionTable_Detailed_Guaranteed(GetFestBannerRelicNamesSimple(2))
                                                          },

                                                          new SelectionDomain()
                                                          {
                                                              PrizesToSelectFromDomainCount = 10,
                                                              SelectionDomainName = "Variable",
                                                              PrizeSelectionTable = GetFFRKPrizeSelectionTable_Detailed_Variable(GetFestBannerRelicNamesSimple(2))
                                                          }

                                                      };

            IList<PrizeResultRow> prizeResultsTable = _selectionEngine.SelectPrizes(selectionDomains);

            Assert.IsNotNull(prizeResultsTable);

            WritePrizeResultTableTextToConsole(prizeResultsTable);
        }

        [TestMethod]
        public void SelectPrizes_Single_FFRKDetailed_WithoutNames_Success()
        {
            IList<SelectionDomain> selectionDomains = new List<SelectionDomain>()
                                                      {
                                                          new SelectionDomain()
                                                          {
                                                              PrizesToSelectFromDomainCount = 1,
                                                              SelectionDomainName = "Guaranteed",
                                                              PrizeSelectionTable = GetFFRKPrizeSelectionTable_Detailed_Guaranteed(null)
                                                          },

                                                          new SelectionDomain()
                                                          {
                                                              PrizesToSelectFromDomainCount = 10,
                                                              SelectionDomainName = "Variable",
                                                              PrizeSelectionTable = GetFFRKPrizeSelectionTable_Detailed_Variable(null)
                                                          }

                                                      };

            IList<PrizeResultRow> prizeResultsTable = _selectionEngine.SelectPrizes(selectionDomains);

            Assert.IsNotNull(prizeResultsTable);

            WritePrizeResultTableTextToConsole(prizeResultsTable);
        }

        [TestMethod]
        public void SelectPrizes_Multi_FFRKDetailed_WithNames_Success()
        {
            IList<SelectionDomain> selectionDomains = new List<SelectionDomain>()
                                                      {
                                                          new SelectionDomain()
                                                          {
                                                              PrizesToSelectFromDomainCount = 1,
                                                              SelectionDomainName = "Guaranteed",
                                                              PrizeSelectionTable = GetFFRKPrizeSelectionTable_Detailed_Guaranteed(GetFestBannerRelicNamesSimple(2))
                                                          },

                                                          new SelectionDomain()
                                                          {
                                                              PrizesToSelectFromDomainCount = 10,
                                                              SelectionDomainName = "Variable",
                                                              PrizeSelectionTable = GetFFRKPrizeSelectionTable_Detailed_Variable(GetFestBannerRelicNamesSimple(2))
                                                          }

                                                      };

            IList<PrizeResultRow> prizeResultsTable = _selectionEngine.SelectPrizes(selectionDomains, 6);

            Assert.IsNotNull(prizeResultsTable);

            WritePrizeResultTableTextToConsole(prizeResultsTable);
        }

        #endregion



        [TestMethod]
        public void DetailedFFRKProbabilityTableSetup_Success()
        {
            IList<PrizeSelectionRow> prizeSelectionTableGuaranteed = GetFFRKDetailedGuaranteedRelicSelectionTable();

            IList<PrizeSelectionRow> prizeSelectionTableVariable = GetFFRKDetailedVariableRelicSelectionTable();

            Assert.AreEqual(16, prizeSelectionTableGuaranteed.Count);
            Assert.AreEqual(18, prizeSelectionTableVariable.Count);
        }

        [TestMethod]
        public void SelectPrizesCurrentFFRK_Complete_Success()
        {
            IList<SelectionDomain> selectionDomains = new List<SelectionDomain>()
                                                      {
                                                          new SelectionDomain()
                                                          {
                                                              PrizesToSelectFromDomainCount = 1,
                                                              SelectionDomainName = "Guaranteed",
                                                              PrizeSelectionTable = GetFFRKDetailedGuaranteedRelicSelectionTable()
                                                          },

                                                          new SelectionDomain()
                                                          {
                                                              PrizesToSelectFromDomainCount = 10,
                                                              SelectionDomainName = "Variable",
                                                              PrizeSelectionTable = GetFFRKDetailedVariableRelicSelectionTable()
                                                          }
                                                          
                                                      };

            IList<PrizeResultRow> prizeResultsTable = _selectionEngine.SelectPrizes(selectionDomains);

            Assert.IsNotNull(prizeResultsTable);

            WritePrizeResultTableTextToConsole(prizeResultsTable);
        }

        [TestMethod]
        public void SelectPrizesCurrentFFRK_Simple_Success()
        {
            IList<SelectionDomain> selectionDomains = new List<SelectionDomain>()
                                                      {
                                                          new SelectionDomain()
                                                          {
                                                              PrizesToSelectFromDomainCount = 1,
                                                              SelectionDomainName = "Guaranteed",
                                                              PrizeSelectionTable = GetFFRKPrizeSelectionTable_Simple_Guaranteed(null)
                                                          },

                                                          new SelectionDomain()
                                                          {
                                                              PrizesToSelectFromDomainCount = 10,
                                                              SelectionDomainName = "Variable",
                                                              PrizeSelectionTable = GetFFRKPrizeSelectionTable_Simple_Variable(null)
                                                          }

                                                      };

            IList<PrizeResultRow> prizeResultsTable = _selectionEngine.SelectPrizes(selectionDomains);

            Assert.IsNotNull(prizeResultsTable);

            WritePrizeResultTableTextToConsole(prizeResultsTable);
        }

        #region FFRK Focused Helper Methods

        //just on banner relics at 14%
        public IList<PrizeSelectionRow> GetFFRKPrizeSelectionTable_Simple_Guaranteed(IList<string> prizeNames)
        {
            // 5 or 6* total
            //category 0 = on banner 5 or 6 ; 14.0% ; scaled = 60/60 * (14.0/14.0) = 1.0
            //total must be 60/60

            string onBannerFiveOrSixStaPrizeCategoryName = "5/6 *";

            double onBannerFiveOrSixStarRate = 1; // = 1.0

            int onBannerFiveOrSixStarCategoryPrizeCountDefault = 14;

            IList<PrizeCategorySpecification> specs = null;

            if (prizeNames != null)
            {
                 specs = new List<PrizeCategorySpecification>()
                        {
                            _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
                                onBannerFiveOrSixStaPrizeCategoryName,
                                onBannerFiveOrSixStarRate,
                                prizeNames)
                        };
            }
            else
            {
                specs = new List<PrizeCategorySpecification>()
                        {
                            _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
                                onBannerFiveOrSixStaPrizeCategoryName,
                                onBannerFiveOrSixStarRate,
                                onBannerFiveOrSixStarCategoryPrizeCountDefault)
                        };
            }


            IList<PrizeSelectionRow> result = _prizeSelectionTableHelper.GetPrizeSelectionTable(specs);

            return result;
        }

        //just on banner relics at 14%
        public IList<PrizeSelectionRow> GetFFRKPrizeSelectionTable_Simple_Variable(IList<string> prizeNames)
        {
            string onBannerFiveOrSixStaPrizeCategoryName = "5/6 *";

            double onBannerFiveOrSixStarRate = 7.0 / 60.0; // = 0.11666666666666666666666666666667

            int onBannerFiveOrSixStarCategoryPrizeCountDefault = 14;

            IList<PrizeCategorySpecification> specs = null;

            if (prizeNames != null)
            {
                specs = new List<PrizeCategorySpecification>()
                        {
                            _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
                                onBannerFiveOrSixStaPrizeCategoryName,
                                onBannerFiveOrSixStarRate,
                                prizeNames)
                        };
            }
            else
            {
                specs = new List<PrizeCategorySpecification>()
                        {
                            _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
                                onBannerFiveOrSixStaPrizeCategoryName,
                                onBannerFiveOrSixStarRate,
                                onBannerFiveOrSixStarCategoryPrizeCountDefault)
                        };
            }

            IList<PrizeSelectionRow> result = _prizeSelectionTableHelper.GetPrizeSelectionTable(specs);

            return result;
        }

        //on banner relics at 14% and off banner 5/6 at 0.4% total
        public IList<PrizeSelectionRow> GetFFRKPrizeSelectionTable_Detailed_Guaranteed(IList<string> prizeNames)
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

            IList<PrizeCategorySpecification> specs = null;

            if (prizeNames != null)
            {
                specs = new List<PrizeCategorySpecification>()
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
            }
            else
            {
                specs = new List<PrizeCategorySpecification>()
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
            }

            IList<PrizeSelectionRow> result = _prizeSelectionTableHelper.GetPrizeSelectionTable(specs);

            return result;
        }

        //on banner relics at 14% and off banner 5/6 at 0.4% total
        public IList<PrizeSelectionRow> GetFFRKPrizeSelectionTable_Detailed_Variable(IList<string> prizeNames)
        {
            string onBannerFiveOrSixStaPrizeCategoryName = "5/6 *";

            double onBannerFiveOrSixStarRate = 7.0 / 60.0; // = 0.11666666666666666666666666666667

            int onBannerFiveOrSixStarCategoryPrizeCountDefault = 14;

            IList<PrizeCategorySpecification> specs = null;

            if (prizeNames != null)
            {
                specs = new List<PrizeCategorySpecification>()
                        {
                            _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
                                onBannerFiveOrSixStaPrizeCategoryName,
                                onBannerFiveOrSixStarRate,
                                prizeNames)
                        };
            }
            else
            {
                specs = new List<PrizeCategorySpecification>()
                        {
                            _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
                                onBannerFiveOrSixStaPrizeCategoryName,
                                onBannerFiveOrSixStarRate,
                                onBannerFiveOrSixStarCategoryPrizeCountDefault)
                        };
            }

            IList<PrizeSelectionRow> result = _prizeSelectionTableHelper.GetPrizeSelectionTable(specs);

            return result;
        }





        public IList<PrizeSelectionRow> GetFFRKDetailedGuaranteedRelicSelectionTable()
        {
            // 5 or 6* total
            //category 0 = on banner 5 or 6 ; 14.0% ; scaled = 60/60 * (14.0/14.04) = 0.997150997 etc
            //category 1 = off banner 6; .02 % ; scaled = 60/60 * (0.02/14.04) = .001424501 etc
            //category 2 = of banner 4; .02 % ;  scaled = 60/60 * (0.02/14.04) = .001424501 etc
            //total must be 60/60

            double onBannerFiveOrSixStarRate = 1* (14.0 / 14.04); // = 0.997150997 etc
            double offBannerSixStarRate = (0.02 / 14.04); // = .001424501 etc
            double offBannerFiveStarRate = (0.02 / 14.04); // = .001424501 etc


            double fiveOrUp = onBannerFiveOrSixStarRate + offBannerSixStarRate + offBannerFiveStarRate; //should equal 1


            IList<PrizeCategorySpecification> specs = new List<PrizeCategorySpecification>()
                                {
                                    _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
                                        "On Banner 5/6* Relics", onBannerFiveOrSixStarRate, 14),

                                    _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
                                        "Off Banner 6* Relics", offBannerSixStarRate, 1),

                                    _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
                                        "Off Banner 5* Relics", offBannerFiveStarRate, 1),
                                };



            IList<PrizeSelectionRow> result = _prizeSelectionTableHelper.GetPrizeSelectionTable(specs);

            return result;
        }

        public IList<PrizeSelectionRow> GetFFRKDetailedVariableRelicSelectionTable()
        {
            // 5 or 6* total
            //total is 7/60 (.116667)
            //category 0 = on banner 5 or 6 ; 14.0% ; scaled = 7/60 * (14.0/14.04) = 0.1163342830009497
            //category 1 = off banner 6; .02 % ; scaled = 7/60 * (0.02/14.04) = 1.661918328584995251661918328585e-4
            //category 2 = of banner 4; .02 % ;  scaled = 7/60 * (0.02/14.04) = 1.661918328584995251661918328585e-4
            //total must be 7/60

            double onBannerFiveOrSixStarRate = 7.0 / 60.0 * (14.0 / 14.04);// = 0.1163342830009497
            double offBannerSixStarRate = 7.0 / 60.0 * (0.02 / 14.04);// = 1.661918328584995251661918328585e-4
            double offBannerFiveStarRate = 7.0 / 60.0 * (0.02 / 14.04);// = 1.661918328584995251661918328585e-4
            double fourStarRate = 53.0 / 60.0 * (0.25 / .8596);// = .626431
            double threeStarRate = 1 - (onBannerFiveOrSixStarRate + offBannerSixStarRate + offBannerFiveStarRate + fourStarRate);// = .256902

            double fiveOrUp = onBannerFiveOrSixStarRate + offBannerSixStarRate + offBannerFiveStarRate;
            double total = onBannerFiveOrSixStarRate + offBannerSixStarRate + offBannerFiveStarRate + fourStarRate + threeStarRate;

            IList<PrizeCategorySpecification> specs = new List<PrizeCategorySpecification>()
                    {
                        _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
                            "On Banner 5/6* Relics", onBannerFiveOrSixStarRate, 14),

                        _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
                            "Off Banner 6* Relics", offBannerSixStarRate, 1),

                        _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
                            "Off Banner 5* Relics", offBannerFiveStarRate, 1),

                        _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
                            "4* Relics", fourStarRate, 1),

                        _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
                            "3 * Relics", threeStarRate, 1)
                    };

            IList<PrizeSelectionRow> result = _prizeSelectionTableHelper.GetPrizeSelectionTable(specs);

            return result;
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
                    relicNames = new List<string>() { "Divine Veil Grimoire", "Zeus Mace (III)", "Burglar Sword (TA)", "Jadagna (XI)", "Fairy's Bow (IV)", "Kiyomori (T)", "Linen Cuirass (TA)", "Aristocrat's Crown (XI)", "Moogle Plushie (XV)", "Faerie Tail (VI)", "Keeper's Cap (Core)", "Onion Cape (III)", "Brigandine (T)", "Alkyoneus's Bracelet (XI)"};
                    break;
                case 2:
                    relicNames = new List<string>() { "Hauteclaire (XIII)", "Enkindler (VIII)", "Kain's Lance (IV)", "Conformer (VIII)", "Razor Carbine (XIII)", "Lifesaber (XIII)", "Squall's Contempt (VIII)", "Axis Blade (VIII)", "Enkindler (XIII)", "Crystal Cross (VIII)", "Abel Lance (IV)", "Lightning's Reprise (XIII)", "Lion Gloves (VIII)", "Dragoon Gauntlets (IV)"};
                    break;
                case 3:
                    relicNames = new List<string>() { "Rune Axe (III)", "Excalibur Trueblade (T)", "Onion Blade (III)", "Chicken Knife (V)", "Blade of Brennaere (XV)", "Crystal Shield (III)", "Great Sword (V)", "War Sword (XV)", "Kaiser Knuckle (VII)", "Onion Gauntlets (III)", "Genji Armor (III)", "Gladiolus' Fatigues (XV)", "Maximillian (V)", "Tifa's Guise (VII)" };
                    break;
                case 4:
                    relicNames = new List<string>() { "Masamune (III)", "Enhancer (VII)", "Force Stealer (VII-CC)", "Kiku-ichimonji (III)", "Force Stealer (VII)", "Mighty Hammer (III)", "Gladius (V)", "Ultima Blade (VII)", "Rune Blade (VII - CC)", "Sargatanas (IX)", "Ninja Gear (IX)", "Blessed Hammer (III)", "Steady Light (VII)", "Shinra Beta+ (VII-CC)" };
                    break;
                case 5:
                    relicNames = new List<string>() { "Durandal (VI)", "Conformer (VII)", "Red Scorpion (V)", "Double Edge (X)", "Oritsuru (VII)", "Terra's Cloak (VI)", "Mystile (VII)", "Terra's Armguard (VI)", "Spiral Shuriken (VII)", "Twilight Steel (X)", "Asura's Rod (VI)", "Yuffie's Guise (VII)", "Tidus' Armguard (X)", "Krile's Dress (V)" };
                    break;

            }

            return relicNames;
        }


        public void WritePrizeResultTableJsonToConsole(IList<PrizeResultRow> prizeResultsTable)
        {
            string results = JsonConvert.SerializeObject(prizeResultsTable, Formatting.Indented);
            Console.WriteLine(results);
        }

        public void WritePrizeResultTableTextToConsole(IList<PrizeResultRow> prizeResultsTable)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Index\tCount\tCat Name\tPrize Name");
            foreach (var prizeResultRow in prizeResultsTable)
            {
                sb.AppendLine(prizeResultRow.ToString());
            }
            sb.AppendLine($"Total: {prizeResultsTable.Sum(r => r.PrizeSelectedCount)}");
            Console.WriteLine(sb.ToString());
        }
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
