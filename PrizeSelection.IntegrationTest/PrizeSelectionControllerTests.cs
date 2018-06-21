using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using PrizeSelection.Api;
using PrizeSelection.Dto.Api;
using PrizeSelection.Logic;
using PrizeCategorySpecification = PrizeSelection.Model.PrizeCategorySpecification;
using PrizeResultRow = PrizeSelection.Model.PrizeResultRow;
using PrizeSelectionRow = PrizeSelection.Model.PrizeSelectionRow;
using SelectionDomain = PrizeSelection.Model.SelectionDomain;
using D = PrizeSelection.Dto.Api;

namespace PrizeSelection.IntegrationTest
{
    [TestClass]
    public class PrizeSelectionControllerTests
    {
        #region Class Variables
        private static TestServerFixture _testServerFixture;
        private const string BasePath = "api/v1.0/PrizeSelection/";

        //for use in helper methdods - logic stolen from unit tests
        private static IResultsFormatter _resultsFormatter;
        private static IPrizeSelectionTableHelper _prizeSelectionTableHelper;
        private static IPrizeResultsTableHelper _prizeResultsTableHelper;
        private static ISelectionEngine _selectionEngine;
        private static ISelectionSuccessCalculator _selectionSuccessCalculator;
        private static IMapper _mapper;
        private static Random _random;
        #endregion

        [ClassInitialize]
        public static void InitializeTestClass(TestContext testContext)
        {
            _testServerFixture = new TestServerFixture();

            //for use in helper methdods - logic stolen from unit tests
            _resultsFormatter = new ResultsFormatter();
            _prizeSelectionTableHelper = new PrizeSelectionTableHelper(_resultsFormatter);
            _prizeResultsTableHelper = new PrizeResultsTableHelper();
            _selectionEngine = new SelectionEngine(_prizeSelectionTableHelper, _prizeResultsTableHelper, new NullLogger<ISelectionEngine>());
            _selectionSuccessCalculator = new SelectionSuccessCalculator(_prizeResultsTableHelper, _selectionEngine);

            _mapper = ConfigureMappings();
        }

        #region Test methods
        [TestMethod]
        public void CreatePrizeCategorySpecification_FFRK_Detailed_Guaranteed_WithName_Success()
        {
            string onBannerFiveOrSixStarPrizeCategoryName = "FiveOrSixStar";

            double onBannerFiveOrSixStarRate = 1 * (14.0 / 14.04); // = 0.997150997150997150997150997151


            // get json for any needed post data
            IList<string> prizeNames = GetFestBannerRelicNamesSimple(1);
            string serializedPostBody = JsonConvert.SerializeObject(prizeNames);

            //set up post content
            HttpContent content = new StringContent(serializedPostBody);
            content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");

            //call actual api
            var httpResponse = _testServerFixture.Client.PostAsync($"{BasePath}PrizeCategorySpecification/{onBannerFiveOrSixStarPrizeCategoryName}/{onBannerFiveOrSixStarRate}", content).Result;

            //retrieve and deserialize api response data to object.
            string resultString = httpResponse.Content.ReadAsStringAsync().Result;
            IList<PrizeCategorySpecification> specs = JsonConvert.DeserializeObject<IList<PrizeCategorySpecification>>(resultString);

            //Assertions
            Assert.IsNotNull(specs);
            Assert.AreEqual(prizeNames.Count, specs.First().PrizeNames.Count);

            //write data to console for spot checking
            WritePrizeCategorySpecificationTableTextToConsole(specs);
        }

        [TestMethod]
        public void CreatePrizeCategorySpecification_FFRK_Detailed_Guaranteed_NoNames_Success()
        {
            string onBannerFiveOrSixStarPrizeCategoryName = "FiveOrSixStar";

            double onBannerFiveOrSixStarRate = 1 * (14.0 / 14.04); // = 0.997150997150997150997150997151

            // get json for any needed post data
            int prizeCount = 14;


            //call actual api
            var httpResponse = _testServerFixture.Client.GetAsync($"{BasePath}PrizeCategorySpecificationNoNames/{onBannerFiveOrSixStarPrizeCategoryName}/{onBannerFiveOrSixStarRate}/{prizeCount}").Result;

            //retrieve and deserialize api response data to object.
            string resultString = httpResponse.Content.ReadAsStringAsync().Result;
            IList<PrizeCategorySpecification> specs = JsonConvert.DeserializeObject<IList<PrizeCategorySpecification>>(resultString);

            //Assertions
            Assert.IsNotNull(specs);
            Assert.AreEqual(prizeCount, specs.First().PrizeNames.Count);

            //write data to console for spot checking
            WritePrizeCategorySpecificationTableTextToConsole(specs);
        }

        [TestMethod]
        public void CreatePrizeSelectionTable_Guaranteed_WithName_Success()
        {
            // get json for any needed post data
            IList<PrizeCategorySpecification> specs = GetPrizeCategorySpecifications_FFRK_Detailed_Guaranteed_WithName();
            string serializedSpecs = JsonConvert.SerializeObject(specs);

            //set up post content
            HttpContent content = new StringContent(serializedSpecs);
            content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");

            //call actual api
            var httpResponse = _testServerFixture.Client.PostAsync($"{BasePath}PrizeSelectionTable", content).Result;

            //retrieve and deserialize api response data to object.
            string resultString = httpResponse.Content.ReadAsStringAsync().Result;
            IList<PrizeSelectionRow> prizeSelectionRows = JsonConvert.DeserializeObject<IList<PrizeSelectionRow>>(resultString);

            //Assertions
            Assert.IsNotNull(prizeSelectionRows);

            //write data to console for spot checking
            WritePrizeSelectionTableTextToConsole(prizeSelectionRows);

        }

        [TestMethod]
        public void ExecutePrizeSelectionSingle_Guaranteed_WithName_Success()
        {
            //public IActionResult GetPrizeSelectionSingle([FromBody]IList<D.SelectionDomain> selectionDomains)

            // get json for any needed post data
            IList<SelectionDomain> selectionDomains = GetSelectionDomains_FFRK_Detailed_WithNames();
            string serializedSelectionDomains = JsonConvert.SerializeObject(selectionDomains);

            //set up post content
            HttpContent content = new StringContent(serializedSelectionDomains);
            content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");

            //call actual api
            var httpResponse = _testServerFixture.Client.PostAsync($"{BasePath}PrizeResults", content).Result;

            //retrieve and deserialize api response data to object.
            string resultString = httpResponse.Content.ReadAsStringAsync().Result;
            IList<PrizeResultRow> prizeResultRows = JsonConvert.DeserializeObject<IList<PrizeResultRow>>(resultString);

            //Assertions
            Assert.IsNotNull(prizeResultRows);

            //write data to console for spot checking
            WritePrizeResultTableTextToConsole(prizeResultRows);

        }

        [TestMethod]
        public void ExecutePrizeSelectionMulti_Guaranteed_WithName_Success()
        {
            int selectionCount = 4;

            // get json for any needed post data
            IList<SelectionDomain> selectionDomains = GetSelectionDomains_FFRK_Detailed_WithNames();
            string serializedSelectionDomains = JsonConvert.SerializeObject(selectionDomains);

            //set up post content
            HttpContent content = new StringContent(serializedSelectionDomains);
            content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");

            //call actual api
            var httpResponse = _testServerFixture.Client.PostAsync($"{BasePath}PrizeResults/{selectionCount}", content).Result;

            //retrieve and deserialize api response data to object.
            string resultString = httpResponse.Content.ReadAsStringAsync().Result;
            IList<PrizeResultRow> prizeResultRows = JsonConvert.DeserializeObject<IList<PrizeResultRow>>(resultString);

            //Assertions
            Assert.IsNotNull(prizeResultRows);

            //write data to console for spot checking
            WritePrizeResultTableTextToConsole(prizeResultRows);

        }

        [TestMethod]
        public void ExecuteSuccessChance_Guaranteed_WithName_Success()
        {
            int selectionCount = 8;
            int prizesSought = 3;

            // get json for any needed post data
            //success criteria
            int prizeCountOnBanner = 16; //16 prizes on banner,including of banner entries
            IDictionary<int, int> successCriteria = _prizeResultsTableHelper.GetEmptyPrizeResultsSummary(prizeCountOnBanner);      
            for (int i = 1; i <= prizesSought; i++)
            {
                successCriteria[i] = 1;
            }
            //selection domains
            IList<SelectionDomain> selectionDomainModels = GetSelectionDomains_FFRK_Detailed_WithNames();
            IList<D.SelectionDomain> selectionDomains = _mapper.Map<IList<D.SelectionDomain>>(selectionDomainModels);

            SuccessCalculationInput input = new SuccessCalculationInput()
                { SelectionCount = selectionCount, SelectionDomains = selectionDomains, SuccessCriteria = successCriteria };
            string serializedInput = JsonConvert.SerializeObject(input);

            //set up post content
            HttpContent content = new StringContent(serializedInput);
            content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");

            //call actual api
            var httpResponse = _testServerFixture.Client.PostAsync($"{BasePath}SuccessChance/", content).Result;

            //retrieve and deserialize api response data to object.
            string resultString = httpResponse.Content.ReadAsStringAsync().Result;
            double successChance = JsonConvert.DeserializeObject<double>(resultString);

            //Assertions
            Assert.AreNotEqual(0, successChance);

            //write data to console for spot checking
            Console.WriteLine($"Success Chance: {successChance}");

        }

        [TestMethod]
        public void ExecuteSuccessChanceSubset_Guaranteed_WithName_Success()
        {
            int selectionCount = 8;
            int prizesSought = 3;
            int subsetSize = 2;

            // get json for any needed post data
            //success criteria
            int prizeCountOnBanner = 16; //16 prizes on banner,including of banner entries
            IDictionary<int, int> successCriteria = _prizeResultsTableHelper.GetEmptyPrizeResultsSummary(prizeCountOnBanner);
            for (int i = 1; i <= prizesSought; i++)
            {
                successCriteria[i] = 1;
            }
            //selection domains
            IList<SelectionDomain> selectionDomainModels = GetSelectionDomains_FFRK_Detailed_WithNames();
            IList<D.SelectionDomain> selectionDomains = _mapper.Map<IList<D.SelectionDomain>>(selectionDomainModels);

            SuccessCalculationInput input = new SuccessCalculationInput()
                { SelectionCount = selectionCount, SelectionDomains = selectionDomains, SuccessCriteria = successCriteria, SubsetSize = subsetSize };
            string serializedInput = JsonConvert.SerializeObject(input);

            //set up post content
            HttpContent content = new StringContent(serializedInput);
            content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");

            //call actual api
            var httpResponse = _testServerFixture.Client.PostAsync($"{BasePath}SuccessChanceSubset/", content).Result;

            //retrieve and deserialize api response data to object.
            string resultString = httpResponse.Content.ReadAsStringAsync().Result;
            double successChance = JsonConvert.DeserializeObject<double>(resultString);

            //Assertions
            Assert.AreNotEqual(0, successChance);

            //write data to console for spot checking
            Console.WriteLine($"Success Chance: {successChance}");

        }

        [TestMethod]
        public void ExecutePullsUntilSuccess_Guaranteed_WithName_Success()
        {
            int prizesSought = 3;

            // get json for any needed post data
            //success criteria
            int prizeCountOnBanner = 16; //16 prizes on banner,including of banner entries
            IDictionary<int, int> successCriteria = _prizeResultsTableHelper.GetEmptyPrizeResultsSummary(prizeCountOnBanner);
            for (int i = 1; i <= prizesSought; i++)
            {
                successCriteria[i] = 1;
            }
            //selection domains
            IList<SelectionDomain> selectionDomainModels = GetSelectionDomains_FFRK_Detailed_WithNames();
            IList<D.SelectionDomain> selectionDomains = _mapper.Map<IList<D.SelectionDomain>>(selectionDomainModels);

            SuccessCalculationInput input = new SuccessCalculationInput(){ SelectionDomains = selectionDomains, SuccessCriteria = successCriteria };
            string serializedInput = JsonConvert.SerializeObject(input);

            //set up post content
            HttpContent content = new StringContent(serializedInput);
            content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");

            //call actual api
            var httpResponse = _testServerFixture.Client.PostAsync($"{BasePath}SelectionsUntilSuccess/", content).Result;

            //retrieve and deserialize api response data to object.
            string resultString = httpResponse.Content.ReadAsStringAsync().Result;
            D.PrizeSelectionsForSuccessInfo successInfo = JsonConvert.DeserializeObject<D.PrizeSelectionsForSuccessInfo>(resultString);

            //Assertions
            Assert.IsNotNull(successInfo);

            //write data to console for spot checking
            Console.WriteLine(successInfo.ToString());

        }

        [TestMethod]
        public void ExecutePullsUntilSuccessSubset_Guaranteed_WithName_Success()
        {
            int prizesSought = 3;
            int subsetSize = 2;

            // get json for any needed post data
            //success criteria
            int prizeCountOnBanner = 16; //16 prizes on banner,including of banner entries
            IDictionary<int, int> successCriteria = _prizeResultsTableHelper.GetEmptyPrizeResultsSummary(prizeCountOnBanner);
            for (int i = 1; i <= prizesSought; i++)
            {
                successCriteria[i] = 1;
            }
            //selection domains
            IList<SelectionDomain> selectionDomainModels = GetSelectionDomains_FFRK_Detailed_WithNames();
            IList<D.SelectionDomain> selectionDomains = _mapper.Map<IList<D.SelectionDomain>>(selectionDomainModels);

            SuccessCalculationInput input = new SuccessCalculationInput() { SelectionDomains = selectionDomains, SuccessCriteria = successCriteria, SubsetSize = subsetSize };
            string serializedInput = JsonConvert.SerializeObject(input);

            //set up post content
            HttpContent content = new StringContent(serializedInput);
            content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");

            //call actual api
            var httpResponse = _testServerFixture.Client.PostAsync($"{BasePath}SelectionsUntilSuccessSubset/", content).Result;

            //retrieve and deserialize api response data to object.
            string resultString = httpResponse.Content.ReadAsStringAsync().Result;
            D.PrizeSelectionsForSuccessInfo successInfo = JsonConvert.DeserializeObject<D.PrizeSelectionsForSuccessInfo>(resultString);

            //Assertions
            Assert.IsNotNull(successInfo);

            //write data to console for spot checking
            Console.WriteLine(successInfo.ToString());

        }
        #endregion


        #region Helper Methods

        public IList<string> GetFestBannerRelicNamesSimple(int bannerIndex)
        {
            if (bannerIndex < 1 || bannerIndex > 5)
            {
                throw new ArgumentOutOfRangeException("bannerIndex must be between 1 and 5");
            }

            IList<string> relicNames = null;

            switch (bannerIndex)
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

        public static IMapper ConfigureMappings()
        {
            MapperConfiguration mapperConfiguration =
                new MapperConfiguration(
                    mce =>
                    {
                        mce.AddProfile<PrizeSelectionModelMappingProfile>();
                        //mce.ConstructServicesUsing(t => ActivatorUtilities.CreateInstance(provider, t));
                    });

            mapperConfiguration.AssertConfigurationIsValid();

            IMapper mapper = mapperConfiguration.CreateMapper();

            return mapper;
        }

        public IList<PrizeCategorySpecification> GetPrizeCategorySpecifications_FFRK_Detailed_Guaranteed_WithName()
        {
            string onBannerFiveOrSixStarPrizeCategoryName = "FiveStarSixStar";
            string offBannerSixStarPrizeCategoryName = "OffBan 6 Star";
            string offBannerFiveStarPrizeCategoryName = "OffBan 5 Star";
            double onBannerFiveOrSixStarRate = 1 * (14.0 / 14.04); // = 0.997150997150997150997150997151     
            double offBannerSixStarRate = (0.02 / 14.04); // = 0.0014245014245014245014245014245
            double offBannerFiveStarRate = (0.02 / 14.04); // = 0.0014245014245014245014245014245

            IList<string> prizeNames = GetFestBannerRelicNamesSimple(1);

            //TEST
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

        public IList<PrizeCategorySpecification> GetPrizeCategorySpecifications_FFRK_Detailed_Variable_WithName()
        {
            string onBannerFiveOrSixStarPrizeCategoryName = "FiveStarSixStar";
            string offBannerSixStarPrizeCategoryName = "OffBan 6 Star";
            string offBannerFiveStarPrizeCategoryName = "OffBan 5 Star";

            double onBannerFiveOrSixStarRate = 7.0 / 60.0; // = 0.11666666666666666666666666666667
            double offBannerSixStarRate = (0.02 / 14.04); // = 0.0014245014245014245014245014245
            double offBannerFiveStarRate = (0.02 / 14.04); // = 0.0014245014245014245014245014245

            IList<string> prizeNames = GetFestBannerRelicNamesSimple(1);

            //TEST
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

        public IList<SelectionDomain> GetSelectionDomains_FFRK_Detailed_WithNames()
        {
            IList<PrizeCategorySpecification> specsFFRKDetailedGuaranteedWithNames =
                GetPrizeCategorySpecifications_FFRK_Detailed_Guaranteed_WithName();

            IList<PrizeSelectionRow> prizeSelectionRowsFFRKDetailedGuaranteedWithNames =
                _prizeSelectionTableHelper.GetPrizeSelectionTable(specsFFRKDetailedGuaranteedWithNames);

            IList<PrizeCategorySpecification> specsFFRKDetailedVariableWithNames =
                GetPrizeCategorySpecifications_FFRK_Detailed_Variable_WithName();

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

            return selectionDomains;
        }
        #endregion

        #region Console Output Methods
        public void WritePrizeResultTableJsonToConsole(IList<PrizeResultRow> prizeResultsTable)
        {
            string results = JsonConvert.SerializeObject(prizeResultsTable, Formatting.Indented);
            Console.WriteLine(results);
        }

        public void WritePrizeCategorySpecificationTableTextToConsole(IList<PrizeCategorySpecification> prizeCategorySpecificationTable)
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

        public void WritePrizeSelectionTableTextToConsole(IList<PrizeSelectionRow> prizeSelectionTable)
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

            sb.AppendLine($"{"Index",-8}{"Count",-8}{"Cat Name",-25}{"Prize Name",-50}");
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

    }
}
