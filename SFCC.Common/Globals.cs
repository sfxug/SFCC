using System;
using System.Collections.Generic;
using System.Text;

namespace SFCC.Common
{
    public class Globals
    {
        public static string BaseUrl
        {
            get
            {
#if DEBUG
                return "https://someurl.azurewebsites.net/";
#else
                return "https://someurl.azurewebsites.net/";
#endif
            }
        }

        public static string ApiUrl { get { return BaseUrl + "api"; } }

        public static string UserIdToken { get { return "UserId"; } }
    }
}
