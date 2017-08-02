using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;  // Install-Package Microsoft.IdentityModel.Clients.ActiveDirectory
using Newtonsoft.Json;                                  // Install-Package Newtonsoft.Json
using Newtonsoft.Json.Linq;

namespace AdcPublishRelationships
{
    static class Program
    {
        private const string SampleAssetA = @"
{
  ""properties"" : {
    ""fromSourceSystem"" : false,
    ""name"": ""Customers"",
    ""dataSource"": {
      ""sourceType"": ""SQL Server"",
      ""objectType"": ""Table"",
    },
    ""dsl"": {
      ""protocol"": ""tds"",
      ""authentication"": ""windows"",
      ""address"": {
        ""server"": ""test.contoso.com"",
        ""database"": ""Northwind"",
        ""schema"": ""dbo"",
        ""object"": ""Customers""
      }
    },
    ""lastRegisteredBy"": {
      ""upn"": ""user1@contoso.com"",
      ""firstName"": ""User1FirstName"",
      ""lastName"": ""User1LastName""
    }
  },
  ""annotations"" : {
    ""schema"": {
      ""properties"" : {
        ""fromSourceSystem"" : false,
        ""columns"": [
          {
            ""name"": ""ID"",
            ""isNullable"": true,
            ""type"": ""int"",
            ""maxLength"": 4,
            ""precision"": 10
          },
          {
            ""name"": ""Name"",
            ""isNullable"": true,
            ""type"": ""nvarchar"",
            ""maxLength"": 100,
            ""precision"": 0
          },
          {
            ""name"": ""PostalCode"",
            ""isNullable"": true,
            ""type"": ""varchar"",
            ""maxLength"": 10,
            ""precision"": 0
          }
        ]
      }
    },
    ""tags"": [
      {
        ""properties"": {
          ""tag"": ""ADCSample"",
          ""fromSourceSystem"": false,
          ""key"": ""64f9f46341774e3bbf413fea50c98766--adcsample""
        }
      }
    ]
  }
}
";

        private const string SampleAssetB = @"
{
  ""properties"" : {
    ""fromSourceSystem"" : false,
    ""name"": ""Orders"",
    ""dataSource"": {
      ""sourceType"": ""SQL Server"",
      ""objectType"": ""Table"",
    },
    ""dsl"": {
      ""protocol"": ""tds"",
      ""authentication"": ""windows"",
      ""address"": {
        ""server"": ""test.contoso.com"",
        ""database"": ""Northwind"",
        ""schema"": ""dbo"",
        ""object"": ""Orders""
      }
    },
    ""lastRegisteredBy"": {
      ""upn"": ""user1@contoso.com"",
      ""firstName"": ""User1FirstName"",
      ""lastName"": ""User1LastName""
    }
  },
  ""annotations"" : {
    ""schema"": {
      ""properties"" : {
        ""fromSourceSystem"" : false,
        ""columns"": [
          {
            ""name"": ""ID"",
            ""isNullable"": false,
            ""type"": ""int"",
            ""maxLength"": 4,
            ""precision"": 10
          },
          {
            ""name"": ""Reference"",
            ""isNullable"": true,
            ""type"": ""varchar"",
            ""maxLength"": 10,
            ""precision"": 0
          },
          {
            ""name"": ""Amount"",
            ""isNullable"": true,
            ""type"": ""money"",
            ""maxLength"": 8,
            ""precision"": 15
          }
        ]
      }
    },
    ""tags"": [
      {
        ""properties"": {
          ""tag"": ""ADCSample"",
          ""fromSourceSystem"": false,
          ""key"": ""64f9f46341774e3bbf413fea50c98766--adcsample""
        }
      }
    ]
  }
}
";

        private const string SampleAssetC = @"
{
  ""properties"" : {
    ""fromSourceSystem"" : false,
    ""name"": ""Transactions"",
    ""dataSource"": {
      ""sourceType"": ""SQL Server"",
      ""objectType"": ""Table"",
    },
    ""dsl"": {
      ""protocol"": ""tds"",
      ""authentication"": ""windows"",
      ""address"": {
        ""server"": ""test.contoso.com"",
        ""database"": ""Northwind"",
        ""schema"": ""dbo"",
        ""object"": ""Transactions""
      }
    },
    ""lastRegisteredBy"": {
      ""upn"": ""user1@contoso.com"",
      ""firstName"": ""User1FirstName"",
      ""lastName"": ""User1LastName""
    }
  },
  ""annotations"" : {
    ""schema"": {
      ""properties"" : {
        ""fromSourceSystem"" : false,
        ""columns"": [
          {
            ""name"": ""OrderID"",
            ""isNullable"": false,
            ""type"": ""int"",
            ""maxLength"": 4,
            ""precision"": 10
          },
          {
            ""name"": ""CustomerID"",
            ""isNullable"": true,
            ""type"": ""int"",
            ""maxLength"": 4,
            ""precision"": 10
          },
          {
            ""name"": ""OrderDate"",
            ""isNullable"": true,
            ""type"": ""datetime"",
            ""maxLength"": 8,
            ""precision"": 23
          }
        ]
      }
    },
    ""tags"": [
      {
        ""properties"": {
          ""tag"": ""ADCSample"",
          ""fromSourceSystem"": false,
          ""key"": ""64f9f46341774e3bbf413fea50c98766--adcsample""
        }
      }
    ]
  }
}
";
        private const string SampleRelationshipAC = @"
{{
  ""properties"" : {{
    ""relationshipType"": ""Join"",
    ""fromAssetId"": ""{0}"",
    ""toAssetId"": ""{1}"",
    ""mappings"": [
      {{
        ""mapId"": ""Transactions__FK__Customers"",
        ""mapFrom"": [""ID""],
        ""mapTo"": [""CustomerID""]
      }}
    ],
    ""lastRegisteredBy"": {{
      ""upn"": ""user1@contoso.com"",
      ""firstName"": ""User1FirstName"",
      ""lastName"": ""User1LastName""
    }}
  }},
  ""annotations"" : {{
    ""tags"": [
      {{
        ""properties"": {{
          ""tag"": ""ADCSample"",
          ""fromSourceSystem"": false,
          ""key"": ""64f9f46341774e3bbf413fea50c98766--adcsample""
        }}
      }}
    ]
  }}
}}
";

        private const string SampleRelationshipBC = @"
{{
  ""properties"" : {{
    ""relationshipType"": ""Join"",
    ""fromAssetId"": ""{0}"",
    ""toAssetId"": ""{1}"",
    ""mappings"": [
      {{
        ""mapId"": ""Transactions__FK__Orders"",
        ""mapFrom"": [""ID""],
        ""mapTo"": [""OrderID""]
      }}
    ],
    ""lastRegisteredBy"": {{
      ""upn"": ""user1@contoso.com"",
      ""firstName"": ""User1FirstName"",
      ""lastName"": ""User1LastName""
    }}
  }},
  ""annotations"" : {{
    ""tags"": [
      {{
        ""properties"": {{
          ""tag"": ""ADCSample"",
          ""fromSourceSystem"": false,
          ""key"": ""64f9f46341774e3bbf413fea50c98766--adcsample""
        }}
      }}
    ]
  }}
}}
";

        private const string AdcApi_BaseUrl = "https://api.azuredatacatalog.com/catalogs/DefaultCatalog";
        private const string AdcApi_Resource_TableAsset = "views/tables";
        private const string AdcApi_Resource_ReportAsset = "views/reports";
        private const string AdcApi_Resource_JoinRelationship = "relationships/join";
        private const string AdcApi_Resource_FindRelationships = "relationships/find";

        private const string AdcApi_Version_PublicV1 = "2016-03-30";
        private const string AdcApi_Version_Relationships = "2017-06-30-Preview";

        private const string UriPathSeparator = "/";
        static AuthenticationResult _authResult;

        static void Main(string[] args)
        {
            // Publish assets
            Console.WriteLine("Publishing asset A");
            string assetALocation = PublishToCatalog(AdcApi_Resource_TableAsset, SampleAssetA);

            Console.WriteLine("Publishing asset B");
            string assetBLocation = PublishToCatalog(AdcApi_Resource_TableAsset, SampleAssetB);

            Console.WriteLine("Publishing asset C");
            string assetCLocation = PublishToCatalog(AdcApi_Resource_TableAsset, SampleAssetC);
            
            // Get assets
            string assetAPayload = GetFromCatalog(assetALocation);
            string assetBPayload = GetFromCatalog(assetBLocation);
            string assetCPayload = GetFromCatalog(assetCLocation);

            // Extract asset ID
            string assetAId = ExtractId(assetAPayload);
            string assetBId = ExtractId(assetBPayload);
            string assetCId = ExtractId(assetCPayload);

            // Publish relationships
            Console.WriteLine("Publishing relationship A--join->C");
            string relationshipAC = string.Format(SampleRelationshipAC, assetAId, assetCId);
            string relationshipACLocation = PublishToCatalog(AdcApi_Resource_JoinRelationship, relationshipAC,
                apiVersion: AdcApi_Version_Relationships);

            Console.WriteLine("Publishing relationship B--join->C");
            string relationshipBC = string.Format(SampleRelationshipBC, assetBId, assetCId);
            string relationshipBCLocation = PublishToCatalog(AdcApi_Resource_JoinRelationship, relationshipBC,
                apiVersion: AdcApi_Version_Relationships);

            // Find relationships
            string relationshipsForAssetC = FindRelationships(assetCId, "all");             // Return AC, BC, CD
            string joinRelationshipsForAssetC = FindRelationships(assetCId, "join");        // Return AC, BC

            string relationshipsFromAssetC = FindRelationships(assetCId, null, "all");      // Return CD
            string relationshipsToAssetC = FindRelationships(null, assetCId, "all");        // Return AC, BC

            string relationshipsFromAssetBToAssetC = FindRelationships(assetBId, assetCId, "all");  // Return BC
            string relationshipsFromAssetCToAssetB = FindRelationships(assetCId, assetBId, "all");  // Return empty list

            // Delete relationships
            DeleteFromCatalog(relationshipACLocation,
                apiVersion: AdcApi_Version_Relationships);
            DeleteFromCatalog(relationshipBCLocation,
                apiVersion: AdcApi_Version_Relationships);

            // Delete assets
            DeleteFromCatalog(assetALocation);
            DeleteFromCatalog(assetBLocation);
            DeleteFromCatalog(assetCLocation);
        }

        private static string PublishToCatalog(string resourceId, string payload, string apiVersion = AdcApi_Version_PublicV1)
        {
            string address = string.Join(UriPathSeparator, AdcApi_BaseUrl, resourceId);

            IDictionary<string, string> queryParameters = new Dictionary<string, string>();
            AddApiVersion(queryParameters, apiVersion);

            IDictionary<string, string> headers = new Dictionary<string, string>();
            AddAuthorizationHeader(headers);

            string publishedLocation;
            using (HttpWebResponse response = SendRequest("POST", address, queryParameters, headers, payload))
            {
                publishedLocation = response.Headers["Location"];
            }
            return publishedLocation;
        }

        private static string GetFromCatalog(string address, string apiVersion = AdcApi_Version_PublicV1, IDictionary<string, string> queryParameters = null)
        {
            if (queryParameters == null)
            {
                queryParameters = new Dictionary<string, string>();
            }
            AddApiVersion(queryParameters, apiVersion);

            IDictionary<string, string> headers = new Dictionary<string, string>();
            AddAuthorizationHeader(headers);

            string content;
            using (HttpWebResponse response = SendRequest("GET", address, queryParameters, headers))
            {
                using (Stream responseStream = response.GetResponseStream())
                using (TextReader reader = new StreamReader(responseStream))
                {
                    content = reader.ReadToEnd();
                }
            }
            return content;
        }

        private static void DeleteFromCatalog(string address, string apiVersion = AdcApi_Version_PublicV1)
        {
            IDictionary<string, string> queryParameters = new Dictionary<string, string>();
            AddApiVersion(queryParameters, apiVersion);

            IDictionary<string, string> headers = new Dictionary<string, string>();
            AddAuthorizationHeader(headers);

            using (SendRequest("DELETE", address, queryParameters, headers)) ;
        }

        private static string FindRelationships(string assetId, string relationshipType = "all")
        {
            string address = string.Join(UriPathSeparator, AdcApi_BaseUrl, AdcApi_Resource_FindRelationships, relationshipType);

            IDictionary<string, string> queryParameters = new Dictionary<string, string>
            {
                { "assetId",    assetId },
            };

            return GetFromCatalog(address,
                apiVersion: AdcApi_Version_Relationships,
                queryParameters: queryParameters);
        }

        private static string FindRelationships(string fromAssetId, string toAssetId, string relationshipType = "all")
        {
            string address = string.Join(UriPathSeparator, AdcApi_BaseUrl, AdcApi_Resource_FindRelationships, relationshipType);

            IDictionary<string, string> queryParameters = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(fromAssetId))
            {
                queryParameters["fromAssetId"] = fromAssetId;
            }
            if (!string.IsNullOrEmpty(toAssetId))
            {
                queryParameters["toAssetId"] = toAssetId;
            }

            return GetFromCatalog(address,
                apiVersion: AdcApi_Version_Relationships,
                queryParameters: queryParameters);
        }

        private static string ExtractId(string json)
        {
            JObject payload = JsonConvert.DeserializeObject<JObject>(json);
            return payload.Value<string>("id");
        }

        private static HttpWebResponse SendRequest(string verb, string address, IDictionary<string, string> queryParameters, IDictionary<string, string> headers, string requestBody = null)
        {
            UriBuilder uri = new UriBuilder(address);
            uri.Scheme = Uri.UriSchemeHttps;
            if (queryParameters != null)
            {
                uri.Query = string.Join("&",
                    queryParameters.Select(item => string.Format("{0}={1}", WebUtility.UrlEncode(item.Key), WebUtility.UrlEncode(item.Value))));
            }

            HttpWebRequest request = WebRequest.CreateHttp(uri.ToString());
            request.Method = verb;
            if (headers != null)
            {
                foreach (var entry in headers)
                {
                    request.Headers[entry.Key] = entry.Value;
                }
            }

            if (!string.IsNullOrEmpty(requestBody))
            {
                byte[] content = Encoding.UTF8.GetBytes(requestBody);
                request.ContentLength = content.Length;
                request.ContentType = "application/json";

                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(content, 0, content.Length);
                }
            }

            try
            {
                return (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                string content;
                using (Stream responseStream = ex.Response.GetResponseStream())
                using (TextReader reader = new StreamReader(responseStream))
                {
                    content = reader.ReadToEnd();
                }
                Console.Error.WriteLine(content);
                throw;
            }
        }

        private static void AddApiVersion(IDictionary<string, string> queryParameters, string apiVersion)
        {
            const string QueryParameter_ApiVersion = "api-version";

            queryParameters[QueryParameter_ApiVersion] = apiVersion;
        }

        private static void AddAuthorizationHeader(IDictionary<string, string> headers)
        {
            const string Header_Authorization = "Authorization";

            AuthenticationResult token = AccessToken().Result;
            headers[Header_Authorization] = token.CreateAuthorizationHeader();
        }

        // Get access token:
        // To call a Data Catalog REST operation, create an instance of AuthenticationContext and call AcquireToken
        // AuthenticationContext is part of the Active Directory Authentication Library NuGet package
        // To install the Active Directory Authentication Library NuGet package in Visual Studio, 
        // run "Install-Package Microsoft.IdentityModel.Clients.ActiveDirectory" from the NuGet Package Manager Console.
        static async Task<AuthenticationResult> AccessToken()
        {
            if (_authResult == null)
            {
                // Resource Uri for Data Catalog API
                string resourceUri = "https://api.azuredatacatalog.com";

                // To learn how to register a client app and get a Client ID, see https://msdn.microsoft.com/en-us/library/azure/mt403303.aspx#clientID   
                string clientId = "PLACEHOLDER";

                // A redirect uri gives AAD more details about the specific application that it will authenticate.
                // Since a client app does not have an external service to redirect to, this Uri is the standard placeholder for a client app.
                string redirectUri = "https://login.live.com/oauth20_desktop.srf";

                // Create an instance of AuthenticationContext to acquire an Azure access token
                // OAuth2 authority Uri
                string authorityUri = "https://login.windows.net/common/oauth2/authorize";
                AuthenticationContext authContext = new AuthenticationContext(authorityUri);

                // Call AcquireToken to get an Azure token from Azure Active Directory token issuance endpoint
                // AcquireToken takes a Client Id that Azure AD creates when you register your client app.
                _authResult = await authContext.AcquireTokenAsync(resourceUri, clientId, new Uri(redirectUri), new PlatformParameters(PromptBehavior.Always));
            }

            return _authResult;
        }
    }
}
