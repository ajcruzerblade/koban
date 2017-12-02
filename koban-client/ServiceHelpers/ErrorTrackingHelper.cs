using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHelpers
{
    public static class ErrorTrackingHelper
    {
        public static Action<Exception, string> TrackException { get; set; }
            = (exception, message) => { };

        public static Func<Exception, string, Task> GenericApiCallExceptionHandler { get; set; }
            = (ex, errorTitle) => Task.FromResult(0);
    }
}
