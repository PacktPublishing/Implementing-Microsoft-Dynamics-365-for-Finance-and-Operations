using D365FO_ODataServiceClient.Microsoft.Dynamics.DataEntities;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.OData.Client;
using System;
using System.Linq;
namespace D365FO_ODataServiceClient
{
    class Program
    {
        static void Main(string[] args)
        {
            //Part 1 - Set Variables (Replace the variables values with your
            //environment URI)
            const string UriString = "https://XYZINCDEVENV.cloudax.dynamics.com";
            const string ADTenant = "https://login.windows.net/XYZINC.com";
            const string ADClntAppId = "YourAPPID";
            const string ADClntSecret = "AppSecret";
            // Part 2. Create context 
            Uri oDataUri = new Uri(UriString + "/data", UriKind.Absolute);
            Resources context = new Resources(oDataUri);
            //Part 3. Set Authorization Header
            context.SendingRequest2 += new EventHandler<SendingRequest2EventArgs>
            (delegate (object sender, SendingRequest2EventArgs e)
            {
                var authContext = new AuthenticationContext(ADTenant);
                var cred = new ClientCredential(ADClntAppId, ADClntSecret);
                var result = authContext.AcquireTokenAsync(UriString, cred).Result;
                e.RequestMessage.SetHeader("Authorization",
                                            result.CreateAuthorizationHeader());
            });
            // Part 4: Create data in Operations - create Customer group -create 
            //entity object
            CustomerGroup customerGroup = new CustomerGroup();
            //create collection and set context
            DataServiceCollection<CustomerGroup> custGroupCollection 
                                                = new DataServiceCollection<CustomerGroup>(context);
            // add to collection
            custGroupCollection.Add(customerGroup);
            // Set properties 
            customerGroup.CustomerGroupId = new Random().Next(1000).ToString("000");
            customerGroup.Description = "Brand new group";
            customerGroup.DataAreaId = "USMF";
            // save changes 
            context.SaveChanges(SaveChangesOptions.PostOnlySetProperties |
                                SaveChangesOptions.BatchWithSingleChangeset);
            //Part 5. Reading data from Operations
            DataServiceQuery<CustomerGroup> usmfCustomerGroup;
            usmfCustomerGroup = context.CustomerGroups.AddQueryOption("$filter", "dataAreaId eq 'USMF'")
                                                      .AddQueryOption("cross-company", "true");
            foreach (var custGroup in usmfCustomerGroup)
            {
                Console.WriteLine("Name:{0} {1}", custGroup.CustomerGroupId,
                                                  custGroup.DataAreaId);
            }
            Console.ReadLine();
        }
    }
}