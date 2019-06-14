using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BCC.Core.Model.CheckRunSubmission;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace BCC.Core.Serialization
{
    public static class CreateCheckRunSerializer
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = new List<JsonConverter>
            {
                new StringEnumConverter { NamingStrategy = new CamelCaseNamingStrategy() } 
            },
            MissingMemberHandling = MissingMemberHandling.Error
        };

        public static string Serialize(CreateCheckRun createCheckRun) => JsonConvert.SerializeObject(createCheckRun, JsonSerializerSettings);

        public static CreateCheckRun DeSerialize(string json) => JsonConvert.DeserializeObject<CreateCheckRun>(json, JsonSerializerSettings);
    }
}
