namespace NuGetPackageManager.Services
{
    using Catel;
    using Catel.Configuration;
    using Catel.Data;
    using Catel.Runtime.Serialization;
    using Catel.Runtime.Serialization.Xml;
    using Catel.Services;
    using NuGetPackageManager.Configuration;
    using NuGetPackageManager.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class NugetConfigurationService : ConfigurationService
    {
        private readonly IXmlSerializer _configSerializer;

        public NugetConfigurationService(ISerializationManager serializationManager,
            IObjectConverterService objectConverterService, IXmlSerializer serializer) : base(serializationManager, objectConverterService, serializer)
        {
            _configSerializer = serializer;
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

            if(serializedModel == null)
            {
                return null;
            }
            else
            {
                var serializedFeed = serializedModel as NuGetFeed;

                if (serializedFeed != null)
                {
                    if(!ConfigurationIdGenerator.TryTakeUniqueIdentifier(serializedFeed.SerializationIdentifier, out Guid newGUid))
                    {
                        serializedFeed.SerializationIdentifier = newGUid;
                    }

                    return serializedFeed;
                }
                throw new InvalidCastException("Serialized model cannot be casted to type 'NuGetFeed'");
            }
        }

        public NuGetFeed GetRoamingValue(Guid key)
        {
            return GetValue(ConfigurationContainer.Roaming, key.ToString());
        }

        public IReadOnlyList<Guid> GetAllKeys(ConfigurationContainer container)
        {
            var feedKeys = GetValueFromStore(container, masterKeys[ConfigurationSections.Feeds]);

            var keyList = feedKeys.Split(new string[] { KeySeparator }, StringSplitOptions.RemoveEmptyEntries);

            return keyList.Select(key => Guid.Parse(key)).ToList();
        }

        public void SetValue(ConfigurationContainer container, string key, NuGetFeed value)
        {
            using (var memstream = new MemoryStream())
            {
                value.Save(memstream, _configSerializer);

                var streamReader = new StreamReader(memstream);

                memstream.Position = 0;

                string rawxml = streamReader.ReadToEnd();

                bool shouldBeUpdated = !ValueExists(container, key);

                SetValueToStore(container, key, rawxml);

                if(shouldBeUpdated)
                {
                    UpdateSectionKeyList(container, ConfigurationSections.Feeds, key);
                }
            }
        }

        public void SetValueWithDefaultIdGenerator(ConfigurationContainer container, NuGetFeed value)
        {
            if(value.SerializationIdentifier.Equals(default(Guid)))
            {
                value.SerializationIdentifier = ConfigurationIdGenerator.GetUniqueIdentifier();
            }

            SetValue(container, value.SerializationIdentifier.ToString(), value);
        }

        public void SetRoamingValueWithDefaultIdGenerator(NuGetFeed value)
        {
            SetValueWithDefaultIdGenerator(ConfigurationContainer.Roaming, value);
        }

        public void RemoveValues(ConfigurationContainer container, IReadOnlyList<NuGetFeed> feedList)
        {
            foreach(var feed in feedList)
            {
                var guid = feed.SerializationIdentifier;

                if (!guid.Equals(default(Guid)))
                {
                    guid = ConfigurationIdGenerator.IsCollision(guid) ? ConfigurationIdGenerator.GetOriginalIdentifier(guid) : guid;

                    if (ValueExists(container, guid.ToString()))
                    {
                        SetValue(container, guid.ToString(), null);

                        UpdateSectionKeyList(container, ConfigurationSections.Feeds, guid.ToString(), true);
                    }
                }
            }
        }

        private Dictionary<ConfigurationSections, string> masterKeys = new Dictionary<ConfigurationSections, string>()
        {
            { ConfigurationSections.Feeds, $"NuGet_{ConfigurationSections.Feeds}" }
        };

        string KeySeparator = "|";

        private void UpdateSectionKeyList(ConfigurationContainer container, ConfigurationSections confSection, string key, bool isRemove = false)
        {
            var keyList = masterKeys[confSection];
            string updatedKeys = String.Empty;

            if(isRemove)
            {
                var persistedKeys = keyList.Split(new string[] { KeySeparator }, StringSplitOptions.RemoveEmptyEntries).ToList();

                //updatedKeys = String.Join(KeySeparator, persistedKeys.Except(keys));
                persistedKeys.Remove(key);
                updatedKeys = String.Join(KeySeparator, persistedKeys);
            }
            else
            {
                //updatedKeys = String.Join(keyList, keys);
                updatedKeys = String.Join(keyList, key);
            }

            SetValueToStore(container, masterKeys[confSection], updatedKeys);
        }
    }
}
