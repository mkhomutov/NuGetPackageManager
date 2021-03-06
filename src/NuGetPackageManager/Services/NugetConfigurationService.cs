namespace NuGetPackageManager.Services
{
    using Catel.Configuration;
    using Catel.Data;
    using Catel.Logging;
    using Catel.Runtime.Serialization;
    using Catel.Runtime.Serialization.Xml;
    using Catel.Services;
    using NuGetPackageManager.Configuration;
    using NuGetPackageManager.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Serialization;

    public class NugetConfigurationService : ConfigurationService
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private readonly IXmlSerializer _configSerializer;


        private readonly Dictionary<ConfigurationSections, string> _masterKeys = new Dictionary<ConfigurationSections, string>()
        {
            { ConfigurationSections.Feeds, $"NuGet_{ConfigurationSections.Feeds}" },
            { ConfigurationSections.ProjectExtensions, $"NuGet_{ConfigurationSections.ProjectExtensions}" }
        };


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

            if (serializedModel == null)
            {
                return null;
            }
            else
            {
                var serializedFeed = serializedModel as NuGetFeed;

                Guid guid = KeyFromString(key);

                if (serializedFeed != null)
                {
                    if (!ConfigurationIdGenerator.TryTakeUniqueIdentifier(guid, out Guid newGUid))
                    {
                        serializedFeed.SerializationIdentifier = newGUid;
                    }
                    else
                    {
                        serializedFeed.SerializationIdentifier = guid;
                    }

                    serializedFeed.Initialize();

                    return serializedFeed;
                }
                throw new InvalidCastException("Serialized model cannot be casted to type 'NuGetFeed'");
            }
        }

        public object GetRoamingValue(ConfigurationSections section)
        {
            var masterKey = _masterKeys[section];

            var obj = DeserializeXmlToListOfString(ConfigurationContainer.Roaming, masterKey);

            return obj;
        }

        public NuGetFeed GetRoamingValue(Guid key)
        {
            return GetValue(ConfigurationContainer.Roaming, KeyToString(key));
        }

        public IReadOnlyList<Guid> GetAllKeys(ConfigurationContainer container)
        {
            var feedKeys = GetValueFromStore(container, _masterKeys[ConfigurationSections.Feeds]);

            var keyList = feedKeys.Split(new string[] { Constants.ConfigKeySeparator }, StringSplitOptions.RemoveEmptyEntries);

            return keyList.Select(key => KeyFromString(key)).ToList();
        }

        public void SetValue(ConfigurationContainer container, string key, NuGetFeed value)
        {
            using (var memstream = new MemoryStream())
            {
                var rawxml = SerializeXml(memstream, () => value.Save(memstream, _configSerializer));

                bool shouldBeUpdated = !ValueExists(container, key);

                SetValueToStore(container, key, rawxml);

                if (shouldBeUpdated)
                {
                    UpdateSectionKeyList(container, ConfigurationSections.Feeds, key);
                }
            }
        }

        private string SerializeXml(Stream stream, Action putValueToStream)
        {
            putValueToStream();

            var streamReader = new StreamReader(stream);

            stream.Position = 0;

            string rawxml = streamReader.ReadToEnd();

            return rawxml;
        }

        private object DeserializeXmlToListOfString(ConfigurationContainer container, string key)
        {
            var storedValue = GetValueFromStore(container, key);

            if (string.IsNullOrEmpty(storedValue))
            {
                return new List<string>();
            }

            var ser = new System.Xml.Serialization.XmlSerializer(typeof(ListWrapper));

            using (StringReader sr = new StringReader(storedValue))
            {
                var obj = ser.Deserialize(sr);

                return (obj as ListWrapper)?.List;
            }
        }

        public void SetValueWithDefaultIdGenerator(ConfigurationContainer container, NuGetFeed value)
        {
            if (value.SerializationIdentifier.Equals(default(Guid)))
            {
                value.SerializationIdentifier = ConfigurationIdGenerator.GetUniqueIdentifier();
            }

            SetValue(container, KeyToString(value.SerializationIdentifier), value);
        }

        public void SetRoamingValueWithDefaultIdGenerator(NuGetFeed value)
        {
            SetValueWithDefaultIdGenerator(ConfigurationContainer.Roaming, value);
        }


        public void SetRoamingValueWithDefaultIdGenerator(List<string> extensibleProject)
        {
            SetValueInSection(ConfigurationContainer.Roaming, ConfigurationSections.ProjectExtensions, extensibleProject);
        }


        public void RemoveValues(ConfigurationContainer container, IReadOnlyList<NuGetFeed> feedList)
        {
            foreach (var feed in feedList)
            {
                var guid = feed.SerializationIdentifier;

                if (!guid.Equals(default(Guid)))
                {
                    guid = ConfigurationIdGenerator.IsCollision(guid) ? ConfigurationIdGenerator.GetOriginalIdentifier(guid) : guid;

                    if (IsValueAvailable(container, KeyToString(guid)))
                    {
                        SetValue(container, KeyToString(guid), String.Empty);

                        UpdateSectionKeyList(container, ConfigurationSections.Feeds, KeyToString(guid), true);
                    }
                }
            }
        }

        protected string KeyToString(Guid guid)
        {
            return $"_{guid}";
        }

        protected Guid KeyFromString(string key)
        {
            return Guid.Parse(key.Substring(1));
        }

        private void SetValueInSection(ConfigurationContainer container, ConfigurationSections section, object value)
        {
            using (var memStream = new MemoryStream())
            {
                var strValue = SerializeXml(memStream, () => _configSerializer.Serialize(value, memStream));

                SetValueToStore(container, _masterKeys[section], strValue);
            }
        }

        private void UpdateSectionKeyList(ConfigurationContainer container, ConfigurationSections confSection, string key, bool isRemove = false)
        {
            var keyList = GetValueFromStore(container, _masterKeys[confSection]);
            string updatedKeys = String.Empty;

            if (isRemove)
            {
                var persistedKeys = keyList.Split(new string[] { Constants.ConfigKeySeparator }, StringSplitOptions.RemoveEmptyEntries).ToList();

                //updatedKeys = String.Join(KeySeparator, persistedKeys.Except(keys));
                persistedKeys.Remove(key);
                updatedKeys = String.Join(Constants.ConfigKeySeparator, persistedKeys);
            }
            else
            {
                //updatedKeys = String.Join(keyList, keys);
                updatedKeys = String.Join(Constants.ConfigKeySeparator, key, keyList);
            }

            SetValueToStore(container, _masterKeys[confSection], updatedKeys);
        }

        [XmlRoot(ElementName = "Items")]
        public class ListWrapper
        {
            [XmlElement(ElementName = "string", Namespace = "http://schemas.microsoft.com/2003/10/Serialization/Arrays")]
            public List<string> List { get; set; }
        }
    }
}
