using System;
using System.Collections.Generic;
using System.Text;

namespace Infra.Options.PrizeSelection
{
    public class LoggingOptions
    {
        public string LogFilePath { get; set; }

        public string ApplicationInsightsKey { get; set; }

        public string AppComponentName { get; set; }
    }
}
