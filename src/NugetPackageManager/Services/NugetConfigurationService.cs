using Catel.Configuration;
using Catel.Data;
using Catel.Runtime.Serialization;
using Catel.Runtime.Serialization.Xml;
using Catel.Services;
using NuGetPackageManager.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NuGetPackageManager.Services
{
    public class NugetConfigurationService : ConfigurationService
    {
        public NugetConfigurationService(ISerializationManager serializationManager,
            IObjectConverterService objectConverterService, IXmlSerializer serializer) : base(serializationManager, objectConverterService, serializer)
        {
            configSerializer = serializer;
        }

        protected override string GetValueFromStore(ConfigurationContainer container, string key)
        {
            return base.GetValueFromStore(container, key);
        }

        public NuGetFeed GetValue(ConfigurationContainer container, string key)
        {
            var rawXml = GetValueFromStore(container, key);

            var ser = new System.Xml.Serialization.XmlSerializer(typeof(NuGetFeed));

            object serializedModel = null;

            using (StringReader sr = new StringReader(rawXml))
            {
                serializedModel = ser.Deserialize(sr);
            }

            //using(var memstream = new MemoryStream())
            //{
            //    var streamWriter = new StreamWriter(memstream);
            //    streamWriter.Write(rawXml);

            //    memstream.Position = 0;

            //    var obj = configSerializer.Deserialize(new NugetFeed(), memstream);

            //    feed = configSerializer.Deserialize<NugetFeed>(memstream);
            //}

            return serializedModel == null ? null : serializedModel as NuGetFeed;
        }

        public void SetValue(ConfigurationContainer container, string key, NuGetFeed value)
        {
            using (var memstream = new MemoryStream())
            {
                value.Save(memstream, configSerializer);

                var streamReader = new StreamReader(memstream);

                memstream.Position = 0;

                string rawxml = streamReader.ReadToEnd();

                SetValueToStore(container, key, rawxml);
            }    
        }


        protected override void SetValueToStore(ConfigurationContainer container, string key, string value)
        {
            //serialize catel model to string

            base.SetValueToStore(container, key, value);
        }

        private IXmlSerializer configSerializer;
    }
}
