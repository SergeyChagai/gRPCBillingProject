using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Billing
{
    public class BillingUtil
    {
        private const string DefaultUsersResourceName = "Billing.users.json";

        public static List<JsonUserProfile> LoadUsers()
        {
            var tmp = ReadUsersFromResource();
            var users = JsonConvert.DeserializeObject<List<JsonUserProfile>>(tmp);

            return users;
        }
        private static string ReadUsersFromResource()
        {
            var stream = typeof(BillingUtil).GetTypeInfo().Assembly.GetManifestResourceStream(DefaultUsersResourceName);
            if (stream == null)
            {
                throw new IOException(string.Format("Error loading the embedded resource \"{0}\"", DefaultUsersResourceName));
            }
            using (var streamReader = new StreamReader(stream))
            {
                return streamReader.ReadToEnd();
            }
        }
    }
}
