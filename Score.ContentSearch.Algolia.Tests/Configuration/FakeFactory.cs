using Sitecore;
using Sitecore.Collections;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Comparers;
using Sitecore.Data.DataProviders;
using Sitecore.Data.IDTables;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Diagnostics.PerformanceCounters;
using Sitecore.Exceptions;
using Sitecore.IO;
using Sitecore.Reflection;
using Sitecore.Security.Domains;
using Sitecore.SecurityModel;
using Sitecore.Sites;
using Sitecore.Tasks;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.Xml;
using Sitecore.Xml.XPath;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Linq;
using System.Reflection;
using System.Xml;


namespace Score.ContentSearch.Algolia.Tests.Configuration
{
    // Type: Sitecore.Configuration.Factory
// Assembly: Sitecore.Kernel, Version=7.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1D84F8DA-C991-492C-BDB5-0B45ECF0566C
// Assembly location: C:\prog\git\score-contentsearch-algolia\libs\Sitecore\Sitecore.Kernel.dll
    public class FakeFactory
    {

        private static readonly char[] ForbiddenChars = "[\\\"*^';&></=]".ToCharArray();
        private static readonly object InitializationLock = new object();
        private static readonly object NullObject = new object();
        private Hashtable cache = new Hashtable();
        private Hashtable nodeCache = new Hashtable();
        private XmlNode _configuration;
        private ItemNavigator itemNavigatorPrototype;
        private MasterVariablesReplacer masterVariablesReplacer;

        public FakeFactory(XmlDocument configuration)
        {
            Assert.IsNotNull(configuration, "Configuration Document not provided");
            _configuration = configuration.DocumentElement.FirstChild;
            Assert.IsNotNull(_configuration, "sitecore section not found");
        }

        public ItemNavigator CreateItemNavigator(Item item)
        {
            Assert.ArgumentNotNull((object) item, "item");
            ItemNavigator itemNavigator = GetItemNavigatorPrototype().CloneInstance();
            if (itemNavigator == null)
                return (ItemNavigator) null;
            itemNavigator.Initialize(item);
            return itemNavigator;
        }

        public T CreateObject<T>(XmlNode configNode) where T : class
        {
            Assert.ArgumentNotNull((object) configNode, "configNode");
            if (XmlUtil.GetAttribute("type", configNode).Length == 0)
            {
                string typeString = ReflectionUtil.GetTypeString(typeof (T));
                configNode = configNode.CloneNode(true);
                XmlUtil.SetAttribute("type", typeString, configNode);
            }
            object @object = CreateObject(configNode, true);
            T obj = @object as T;
            if ((object) obj != null)
                return obj;
            throw new ConfigurationException("Unexpected object type created from configuration node. Expected: " +
                                             typeof (T).FullName + ". Got: " + @object.GetType().FullName + ". XML: " +
                                             configNode.OuterXml);
        }

        public ConfigStore GetConfigStore(string configStoreName)
        {
            Assert.ArgumentNotNullOrEmpty(configStoreName, "configStoreName");
            string configPath = "configStores/add[@name='" + configStoreName + "']";
            object @object = CreateObject(configPath, true);
            if (@object == null)
                throw new ConfigurationException(
                    "Could not instantiate configuration store. Path to configuration store information: " + configPath);
            ConfigStore configStore = @object as ConfigStore;
            if (configStore != null)
                return configStore;
            throw new ConfigurationException("Configuration store object is of wrong type: " +
                                             @object.GetType().FullName + " (expected: " + typeof (ConfigStore).FullName +
                                             "). Path to configuration store information: " + configPath);
        }

        public List<CustomHandler> GetCustomHandlers()
        {
            string str = "customHandlers/handler";
            List<CustomHandler> list1;
            if (TryGetCacheValue<List<CustomHandler>>(cache, str, false, out list1))
                return list1;
            List<CustomHandler> list2 = new List<CustomHandler>();
            foreach (XmlNode node in GetConfigNodes(str))
            {
                string attribute1 = XmlUtil.GetAttribute("trigger", node);
                string handler = XmlUtil.GetAttribute("handler", node);
                string attribute2 = XmlUtil.GetAttribute("type", node);
                if (attribute1.Length == 0)
                    throw new InvalidOperationException("Invalid request handler entry: " + node.OuterXml);
                Type type = (Type) null;
                if (attribute2.Length > 0)
                {
                    type = ReflectionUtil.GetTypeInfo(attribute2);
                    if (type == (Type) null)
                        throw new InvalidOperationException("Type " + attribute2 + " not found.");
                    if (handler.Length == 0)
                        handler = "sitecore_handlers.ashx";
                }
                CustomHandler customHandler = new CustomHandler(attribute1, handler, type);
                list2.Add(customHandler);
            }
            return AddToCache<List<CustomHandler>>(cache, str, list2);
        }

        public Database GetDatabase(string name)
        {
            Assert.ArgumentNotNull((object) name, "name");
            return Assert.ResultNotNull<Database>(GetDatabase(name, true));
        }

        public Database GetDatabase(string name, bool assert)
        {
            Assert.ArgumentNotNull((object) name, "name");
            if (name.IndexOfAny(ForbiddenChars) < 0)
                return CreateObject("databases/database[@id='" + name + "']", assert) as Database;
            Assert.IsFalse(assert, "assert");
            return (Database) null;
        }

        public string[] GetDatabaseNames()
        {
            return GetNames("databases/database", "id");
        }

        public List<Database> GetDatabases()
        {
            List<Database> list = new List<Database>();
            foreach (string name in GetDatabaseNames())
                list.Add(GetDatabase(name));
            return list;
        }

        public Domain GetDomain(string name)
        {
            Assert.ArgumentNotNull((object) name, "name");
            return DomainManager.GetDomain(name);
        }

        public StringDictionary GetDomainMap(string path)
        {
            Assert.ArgumentNotNullOrEmpty(path, "path");
            StringDictionary stringDictionary =
                new StringDictionary((IEqualityComparer) StringComparer.OrdinalIgnoreCase);
            foreach (XmlNode node in GetConfigNodes(FileUtil.MakePath(path, "map", '/')))
            {
                string attribute1 = XmlUtil.GetAttribute("domain", node);
                string attribute2 = XmlUtil.GetAttribute("provider", node);
                if (!string.IsNullOrEmpty(attribute1) && !string.IsNullOrEmpty(attribute2))
                    stringDictionary[attribute1] = attribute2;
            }
            return stringDictionary;
        }

        public string[] GetDomainNames()
        {
            return Enumerable.ToArray<string>(DomainManager.GetDomainNames());
        }

        public Hashtable GetHashtable(string path, HashKeyType keyType,
            HashValueType valueType, HashValueFormat format, Type dataType)
        {
            Assert.ArgumentNotNull((object) path, "path");
            Assert.ArgumentNotNull((object) dataType, "dataType");
            Hashtable hashtable = new Hashtable();
            XmlNode configNode1 = GetConfigNode(path);
            if (configNode1 != null)
            {
                XmlNodeList childNodes1 = configNode1.ChildNodes;
                for (int index1 = 0; index1 < childNodes1.Count; ++index1)
                {
                    XmlNode xmlNode = childNodes1[index1];
                    if (xmlNode.NodeType == XmlNodeType.Element)
                    {
                        ArrayList arrayList = new ArrayList();
                        XmlNodeList childNodes2 = xmlNode.ChildNodes;
                        for (int index2 = 0; index2 < childNodes2.Count; ++index2)
                        {
                            XmlNode configNode2 = childNodes2[index2];
                            if (configNode2.NodeType == XmlNodeType.Element)
                            {
                                object obj = (object) null;
                                switch (valueType)
                                {
                                    case HashValueType.NodeName:
                                        obj = (object) configNode2.LocalName;
                                        break;
                                    case HashValueType.Object:
                                        obj = CreateObject(configNode2, false);
                                        break;
                                }
                                if (obj != null)
                                    arrayList.Add(obj);
                            }
                        }
                        if (arrayList.Count > 0)
                        {
                            string str = (string) null;
                            if (keyType == HashKeyType.NodeName)
                                str = xmlNode.LocalName;
                            if (str != null)
                            {
                                switch (format)
                                {
                                    case HashValueFormat.SingleObject:
                                        hashtable[(object) str] = arrayList[0];
                                        continue;
                                    case HashValueFormat.Array:
                                        hashtable[(object) str] = (object) arrayList.ToArray(dataType);
                                        continue;
                                    default:
                                        continue;
                                }
                            }
                        }
                    }
                }
            }
            return hashtable;
        }

        public IDTableProvider GetIDTable()
        {
            return CreateObject("IDTable", true) as IDTableProvider;
        }


        public IComparer<Item> GetItemComparer(Item item)
        {
            Assert.ArgumentNotNull((object) item, "item");
            return ComparerFactory.GetComparer(item);
        }

        public Sitecore.Links.LinkDatabase GetLinkDatabase()
        {
            object @object = CreateObject("LinkDatabase", true);
            Assert.IsNotNull(@object, "Object is null");
            Sitecore.Links.LinkDatabase linkDatabase = @object as Sitecore.Links.LinkDatabase;
            if (linkDatabase != null)
                return linkDatabase;
            else
                throw new InvalidTypeException("LinkDatabase object is of wrong type: " + @object.GetType().FullName);
        }

        public MasterVariablesReplacer GetMasterVariablesReplacer()
        {
            MasterVariablesReplacer variablesReplacer1 = masterVariablesReplacer;
            if (variablesReplacer1 != null)
                return variablesReplacer1;
            MasterVariablesReplacer variablesReplacer2 =
                ReflectionUtil.CreateObject(Settings.MasterVariablesReplacer, new object[0]) as MasterVariablesReplacer;
            if (variablesReplacer2 == null)
                return (MasterVariablesReplacer) null;
            masterVariablesReplacer = variablesReplacer2;
            return variablesReplacer2;
        }

        public PerformanceCounterCollection GetPerformanceCounters()
        {
            PerformanceCounterCollection counterCollection = new PerformanceCounterCollection();
            Type ancestorType = typeof (PerformanceCounter);
            foreach (Type type in Assembly.GetExecutingAssembly().GetExportedTypes())
            {
                if (type.Name.EndsWith("Counters", StringComparison.InvariantCulture))
                {
                    foreach (PropertyInfo propertyInfo in type.GetProperties(BindingFlags.Static | BindingFlags.Public))
                    {
                        if (MainUtil.IsType(propertyInfo.PropertyType, ancestorType))
                            counterCollection.Add(
                                propertyInfo.GetValue((object) null, (object[]) null) as PerformanceCounter);
                    }
                }
            }
            return counterCollection;
        }

        public TCollection GetProviders<TProvider, TCollection>(string rootPath, out TProvider defaultProvider)
            where TProvider : ProviderBase where TCollection : ProviderCollection, new()
        {
            Assert.ArgumentNotNullOrEmpty(rootPath, "rootPath");
            string xpath = FileUtil.MakePath(rootPath, "providers", '/');
            XmlNode configNode = GetConfigNode(rootPath, true);
            Assert.IsNotNull((object) configNode, "Could not find any node by the following path: {0}", new object[1]
            {
                (object) rootPath
            });
            List<XmlNode> childNodes = XmlUtil.GetChildNodes(GetConfigNode(xpath, true), true);
            Assert.IsTrue((childNodes.Count > 0 ? 1 : 0) != 0, "Could not find any provider nodes below: {0}",
                new object[1]
                {
                    (object) xpath
                });
            TCollection providers = GetProviders<TProvider, TCollection>(childNodes);
            Assert.IsTrue((providers.Count > 0 ? 1 : 0) != 0, "No providers found below: {0}", new object[1]
            {
                (object) xpath
            });
            string attribute = GetAttribute("defaultProvider", configNode, (string[]) null);
            TProvider provider1 = default (TProvider);
            IEnumerator enumerator = providers.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    TProvider provider2 = (TProvider) enumerator.Current;
                    if (attribute.Length == 0 || provider2.Name == attribute)
                    {
                        provider1 = provider2;
                        break;
                    }
                }
            }
            finally
            {
                IDisposable disposable = enumerator as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }
            Assert.IsNotNull((object) provider1, "Could not find default provider: {0}. Root path: {1}", new object[2]
            {
                (object) attribute,
                (object) rootPath
            });
            defaultProvider = provider1;
            return providers;
        }

        public IRetryable GetRetryer()
        {
            XmlNode configNode = GetConfigNode("retryer");
            if (configNode == null || GetAttribute("disabled", configNode, (string[]) null) == "true")
                return (IRetryable) new NullRetryer();
            else
                return CreateObject("retryer", false) as IRetryable ?? (IRetryable) new NullRetryer();
        }

        public Replacer GetReplacer(string name)
        {
            Assert.ArgumentNotNullOrEmpty(name, "name");
            return CreateObject("replacers/replacer[@id='" + name + "']", false) as Replacer;
        }

        public SiteContext GetSite(string siteName)
        {
            Assert.ArgumentNotNullOrEmpty(siteName, "siteName");
            return SiteContextFactory.GetSiteContext(siteName);
        }

        public SiteInfo GetSiteInfo(string siteName)
        {
            Assert.ArgumentNotNullOrEmpty(siteName, "siteName");
            return SiteContextFactory.GetSiteInfo(siteName);
        }

        public List<SiteInfo> GetSiteInfoList()
        {
            List<SiteInfo> list = new List<SiteInfo>();
            foreach (string siteName in GetSiteNames())
            {
                SiteInfo siteInfo = GetSiteInfo(siteName);
                if (siteInfo != null)
                    list.Add(siteInfo);
            }
            return list;
        }

        public string[] GetSiteNames()
        {
            return SiteContextFactory.GetSiteNames();
        }

        [Obsolete("Use GetSitesInfo() method instead")]
        public List<SiteContext> GetSites()
        {
            List<SiteContext> list = new List<SiteContext>();
            foreach (string siteName in GetSiteNames())
            {
                SiteContext site = GetSite(siteName);
                if (site != null)
                    list.Add(site);
            }
            return list;
        }

        [Obsolete("Deprecated - Use GetSiteInfoList instead.")]
        public List<SiteInfo> GetSitesInfo()
        {
            return GetSiteInfoList();
        }

        public string GetString(string configPath, bool assert)
        {
            Assert.ArgumentNotNullOrEmpty(configPath, "configPath");
            return CreateObject(configPath, assert) as string ?? string.Empty;
        }

        public Set<string> GetStringSet(string configPath)
        {
            Assert.ArgumentNotNullOrEmpty(configPath, "configPath");
            Set<string> set = new Set<string>();
            XmlNodeList configNodes = GetConfigNodes(configPath);
            if (configNodes.Count == 0)
                return set;
            foreach (XmlNode xmlNode in configNodes)
                set.Add(xmlNode.InnerText);
            return set;
        }

        public TaskDatabase GetArchiveTaskDatabase()
        {
            object @object = CreateObject("ArchiveTaskDatabase", true);
            Assert.IsNotNull(@object, "Object not found");
            TaskDatabase taskDatabase = @object as TaskDatabase;
            if (taskDatabase != null)
                return taskDatabase;
            else
                throw new InvalidTypeException("TaskDatabase object is of wrong type: " + @object.GetType().FullName);
        }

        public TaskDatabase GetTaskDatabase()
        {
            object @object = CreateObject("TaskDatabase", true);
            Assert.IsNotNull(@object, "Object not found");
            TaskDatabase taskDatabase = @object as TaskDatabase;
            if (taskDatabase != null)
                return taskDatabase;
            else
                throw new InvalidTypeException("TaskDatabase object is of wrong type: " + @object.GetType().FullName);
        }

        public object CreateObject(string configPath, bool assert)
        {
            Assert.ArgumentNotNullOrEmpty(configPath, "configPath");
            return CreateObject(configPath, (string[]) null, assert);
        }

        public object CreateObject(string configPath, string[] parameters, bool assert)
        {
            Assert.ArgumentNotNull((object) configPath, "configPath");
            object obj;
            if (TryGetCacheValue<object>(cache, configPath, !assert, out obj))
                return obj;
            lock (string.Intern(configPath))
            {
                if (TryGetCacheValue<object>(cache, configPath, !assert, out obj))
                    return obj;
                XmlNode local_1 = GetConfigNode(configPath, assert);
                if (local_1 == null)
                {
                    AddToCache<object>(cache, configPath, NullObject);
                    return (object) null;
                }
                else if (XmlUtil.GetAttribute("mode", local_1) == "off")
                {
                    Assert.IsTrue((!assert ? 1 : 0) != 0, "The requested object has mode=off. Configuration path: {0}",
                        new object[1]
                        {
                            (object) configPath
                        });
                    AddToCache<object>(cache, configPath, NullObject);
                    return (object) null;
                }
                else
                {
                    object local_0_1 = CreateObject(local_1, parameters, assert);
                    if (local_0_1 == null)
                    {
                        AddToCache<object>(cache, configPath, NullObject);
                        return (object) null;
                    }
                    else
                    {
                        AssignProperties(local_1, parameters, local_0_1, assert, true, (IFactoryHelper) null);
                        if (GetAttribute("singleInstance", local_1, (string[]) null) != "true")
                            return local_0_1;
                        else
                            return AddToCache<object>(cache, configPath, local_0_1);
                    }
                }
            }
        }

        public object CreateObject(XmlNode configNode, bool assert)
        {
            Assert.ArgumentNotNull((object) configNode, "configNode");
            return CreateObject(configNode, (string[]) null, assert, (IFactoryHelper) null);
        }

        public object CreateObject(XmlNode configNode, string[] parameters, bool assert)
        {
            Assert.ArgumentNotNull((object) configNode, "configNode");
            return CreateObject(configNode, parameters, assert, (IFactoryHelper) null);
        }

        public object CreateObject(XmlNode configNode, string[] parameters, bool assert, IFactoryHelper helper)
        {
            Assert.ArgumentNotNull((object) configNode, "configNode");
            object obj = (((CreateFromFactory(configNode, parameters, assert)
                            ?? CreateFromFactoryMethod(configNode, parameters, assert))
                           ?? CreateFromReference(configNode, parameters, assert))
                          ?? CreateFromTypeName(configNode, parameters, assert))
                         ?? CreateFromConnectionStringName(configNode, parameters, assert);
            if (obj == null)
                return (object) GetStringValue(configNode, parameters);
            bool flag = true;
            if (obj is IInitializable)
            {
                IInitializable initializable = obj as IInitializable;
                initializable.Initialize(configNode);
                flag = initializable.AssignProperties;
            }
            if (flag)
                AssignProperties(configNode, parameters, obj, assert, false, helper);
            IConstructable constructable = obj as IConstructable;
            if (constructable != null)
                constructable.Constructed(configNode);
            return obj;
        }

        public Type CreateType(XmlNode configNode, bool assert)
        {
            Assert.ArgumentNotNull((object) configNode, "configNode");
            return CreateType(configNode, (string[]) null, assert);
        }

        public Type CreateType(XmlNode configNode, string[] parameters, bool assert)
        {
            Assert.ArgumentNotNull((object) configNode, "configNode");
            string attribute1 = GetAttribute("type", configNode, parameters);
            if (attribute1.Length > 0)
            {
                Type type = Type.GetType(attribute1, false, true);
                if (type == (Type) null)
                {
                    foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        type = assembly.GetType(attribute1, false, true);
                        if (type != (Type) null)
                            break;
                    }
                }
                Error.Assert(!assert || type != (Type) null, "Could not resolve type name: " + attribute1);
                if (type != (Type) null)
                    ReflectionCounters.TypesResolved.Increment();
                else
                    ReflectionCounters.TypesNotResolved.Increment();
                return type;
            }
            else
            {
                string attribute2 = GetAttribute("assembly", configNode, parameters);
                string attribute3 = GetAttribute("class", configNode, parameters);
                string className = FileUtil.MakePath(GetAttribute("namespace", configNode, parameters),
                    attribute3, '.');
                if (className.Length > 0 && attribute2.Length > 0)
                {
                    string assemblyFile = FileUtil.MapPath(attribute2);
                    Assembly assembly = Assembly.LoadFrom(assemblyFile);
                    Error.Assert(!assert || assembly != (Assembly) null, "Could load assembly: '" + assemblyFile + "'");
                    if (assembly != (Assembly) null)
                    {
                        Type type = FindType(className, assembly);
                        Error.Assert((!assert ? 1 : (type != (Type) null ? 1 : 0)) != 0,
                            "Could not find type: '" + className + "' in assembly: '" + assemblyFile + "'");
                        if (type != (Type) null)
                            ReflectionCounters.TypesResolved.Increment();
                        else
                            ReflectionCounters.TypesNotResolved.Increment();
                        return type;
                    }
                }
                return (Type) null;
            }
        }

        public Type FindType(string className, Assembly assembly)
        {
            Assert.ArgumentNotNullOrEmpty(className, "className");
            Assert.ArgumentNotNull((object) assembly, "assembly");
            foreach (Type type in assembly.GetExportedTypes())
            {
                if (type.FullName == className)
                    return type;
            }
            return (Type) null;
        }

        public string GetAttribute(string name, XmlNode node, string[] parameters)
        {
            Assert.ArgumentNotNullOrEmpty(name, "name");
            Assert.ArgumentNotNull((object) node, "node");
            return ReplaceVariables(XmlUtil.GetAttribute(name, node), node, parameters);
        }

        public XmlNode GetConfigNode(string xpath)
        {
            Assert.ArgumentNotNullOrEmpty(xpath, "xpath");
            return GetConfigNode(xpath, false);
        }

        public XmlNode GetConfigNode(string xpath, bool assert)
        {
            Assert.ArgumentNotNullOrEmpty(xpath, "xpath");
            XmlNode xmlNode1;
            if (TryGetCacheValue<XmlNode>(nodeCache, xpath, !assert, out xmlNode1))
                return xmlNode1;
            var documentElement = GetConfiguration();
            Assert.IsNotNull((object) documentElement, "Could not find DocumentElement in configuration.");
            XmlNode xmlNode2 = documentElement.SelectSingleNode(xpath);
            if (xmlNode2 != null)
                return AddToCache<XmlNode>(nodeCache, xpath, xmlNode2);
            Assert.IsTrue((!assert ? 1 : 0) != 0, "Could not find configuration node: {0}", new object[1]
            {
                (object) xpath
            });
            AddToCache<object>(nodeCache, xpath, NullObject);
            return (XmlNode) null;
        }

        public XmlNodeList GetConfigNodes(string xpath)
        {
            Assert.ArgumentNotNullOrEmpty(xpath, "xpath");
            return GetConfiguration().SelectNodes(xpath);
        }

        public void Reset()
        {
            _configuration = (XmlDocument) null;
            cache = new Hashtable();
            nodeCache = new Hashtable();
            itemNavigatorPrototype = (ItemNavigator) null;
            masterVariablesReplacer = (MasterVariablesReplacer) null;
        }

        public XmlNode GetConfiguration()
        {
            if (_configuration != null)
                return _configuration;
            lock (InitializationLock)
            {
                if (_configuration != null)
                    return _configuration;
                XmlNode local_0 = ConfigReader.GetConfigNode();
                Assert.IsNotNull((object) local_0, "Could not read Sitecore configuration.");
                XmlDocument local_1 = new XmlDocument();
                local_1.AppendChild(local_1.ImportNode(local_0, true));
                ExpandIncludeFiles((XmlNode) local_1.DocumentElement, new Hashtable());
                //LoadAutoIncludeFiles((XmlNode) local_1.DocumentElement);
                ReplaceGlobalVariables((XmlNode) local_1.DocumentElement);
                _configuration = local_1;
                return _configuration;
            }
        }

        private object CreateFromConnectionStringName(XmlNode configNode, string[] parameters, bool assert)
        {
            Assert.ArgumentNotNull((object) configNode, "configNode");
            string attribute = GetAttribute("connectionStringName", configNode, parameters);
            if (string.IsNullOrEmpty(attribute))
                return (object) null;
            string connectionString = Settings.GetConnectionString(attribute);
            Assert.IsTrue(!assert || !string.IsNullOrEmpty(connectionString),
                "Could not create object. Unknown connection string name: " + attribute);
            return (object) connectionString;
        }

        private object CreateFromFactoryMethod(XmlNode configNode, string[] parameters, bool assert)
        {
            Assert.ArgumentNotNull((object) configNode, "configNode");
            string attribute1 = XmlUtil.GetAttribute("factoryMethod", configNode);
            if (string.IsNullOrEmpty(attribute1))
                return (object) null;
            string attribute2 = XmlUtil.GetAttribute("type", configNode);
            if (string.IsNullOrEmpty(attribute2))
                return (object) null;
            List<object> list = new List<object>();
            int num = 0;
            while (true)
            {
                XmlAttribute xmlAttribute = configNode.Attributes["arg" + (object) num];
                if (xmlAttribute != null)
                {
                    list.Add((object) xmlAttribute.Value);
                    ++num;
                }
                else
                    break;
            }
            object obj = ReflectionUtil.CallStaticMethod(attribute2, attribute1, list.ToArray());
            if (obj == null && assert)
                throw new ConfigurationException("Could not create object from factory method. Config XML: " +
                                                 configNode.OuterXml);
            else
                return obj;
        }

        private bool TryGetCacheValue<T>(Hashtable cacheInstance, string key, bool allowNull, out T value)
            where T : class
        {
            Assert.ArgumentNotNull((object) cacheInstance, "cacheInstance");
            value = default (T);
            object objA = cacheInstance[(object) key];
            if (objA == null)
                return false;
            if (object.ReferenceEquals(objA, NullObject))
                return allowNull;
            value = objA as T;
            return (object) value != null;
        }

        private void ExpandIncludeFile(XmlNode xmlNode, Hashtable cycleDetector)
        {
            Assert.ArgumentNotNull((object) xmlNode, "xmlNode");
            Assert.ArgumentNotNull((object) cycleDetector, "cycleDetector");
            string filename = GetAttribute("file", xmlNode, (string[]) null).ToLowerInvariant();
            if (filename.Length == 0)
                return;
            Assert.IsTrue((!cycleDetector.ContainsKey((object) filename) ? 1 : 0) != 0,
                "Cycle detected in configuration include files. The file '{0}' is being included directly or indirectly in a way that causes a cycle to form.",
                new object[1]
                {
                    (object) filename
                });
            XmlDocument xmlDocument = XmlUtil.LoadXmlFile(filename);
            if (xmlDocument.DocumentElement == null)
                return;
            XmlNode parentNode = xmlNode.ParentNode;
            XmlNode xmlNode1 = xmlNode.OwnerDocument.ImportNode((XmlNode) xmlDocument.DocumentElement, true);
            parentNode.ReplaceChild(xmlNode1, xmlNode);
            cycleDetector.Add((object) filename, (object) string.Empty);
            ExpandIncludeFiles(xmlNode1, cycleDetector);
            cycleDetector.Remove((object) filename);
            while (xmlNode1.FirstChild != null)
                parentNode.AppendChild(xmlNode1.FirstChild);
            foreach (XmlNode newChild in xmlNode1.ChildNodes)
                parentNode.AppendChild(newChild);
            XmlUtil.TransferAttributes(xmlNode1, parentNode);
            parentNode.RemoveChild(xmlNode1);
        }

        private void ExpandIncludeFiles(XmlNode rootNode, Hashtable cycleDetector)
        {
            Assert.ArgumentNotNull((object) rootNode, "rootNode");
            Assert.ArgumentNotNull((object) cycleDetector, "cycleDetector");
            if (rootNode.LocalName == "sc.include")
            {
                ExpandIncludeFile(rootNode, cycleDetector);
            }
            else
            {
                XmlNodeList xmlNodeList = rootNode.SelectNodes(".//sc.include");
                for (int index = 0; index < xmlNodeList.Count; ++index)
                    ExpandIncludeFile(xmlNodeList[index], cycleDetector);
            }
        }

        //private void LoadAutoIncludeFiles(XmlNode element)
        //{
        //    Assert.ArgumentNotNull((object) element, "element");
        //    ConfigPatcher patcher = new ConfigPatcher(element);
        //    LoadAutoIncludeFiles(patcher, MainUtil.MapPath("/App_Config/Sitecore/Components"));
        //    LoadAutoIncludeFiles(patcher, MainUtil.MapPath("/App_Config/Include"));
        //}

        //private void LoadAutoIncludeFiles(ConfigPatcher patcher, string folder)
        //{
        //    Assert.ArgumentNotNull((object) patcher, "patcher");
        //    Assert.ArgumentNotNull((object) folder, "folder");
        //    try
        //    {
        //        if (!Directory.Exists(folder))
        //            return;
        //        foreach (string str in Directory.GetFiles(folder, "*.config"))
        //        {
        //            try
        //            {
        //                if ((File.GetAttributes(str) & FileAttributes.Hidden) == (FileAttributes) 0)
        //                    patcher.ApplyPatch(str);
        //            }
        //            catch (Exception ex)
        //            {
        //                Log.Error(string.Concat(new object[4]
        //                {
        //                    (object) "Could not load configuration file: ",
        //                    (object) str,
        //                    (object) ": ",
        //                    (object) ex
        //                }), typeof (Factory));
        //            }
        //        }
        //        foreach (string str in Directory.GetDirectories(folder))
        //        {
        //            try
        //            {
        //                if ((File.GetAttributes(str) & FileAttributes.Hidden) == (FileAttributes) 0)
        //                    LoadAutoIncludeFiles(patcher, str);
        //            }
        //            catch (Exception ex)
        //            {
        //                Log.Error(string.Concat(new object[4]
        //                {
        //                    (object) "Could not scan configuration folder ",
        //                    (object) str,
        //                    (object) " for files: ",
        //                    (object) ex
        //                }), typeof (Factory));
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error(string.Concat(new object[4]
        //        {
        //            (object) "Could not scan configuration folder ",
        //            (object) folder,
        //            (object) " for files: ",
        //            (object) ex
        //        }), typeof (Factory));
        //    }
        //}

        private void ReplaceGlobalVariables(XmlNode rootNode)
        {
            Assert.ArgumentNotNull((object) rootNode, "rootNode");
            XmlNodeList xmlNodeList = rootNode.SelectNodes(".//sc.variable");
            StringDictionary variables = new StringDictionary();
            foreach (XmlAttribute xmlAttribute in (XmlNamedNodeMap) rootNode.Attributes)
            {
                string name = xmlAttribute.Name;
                string @string = StringUtil.GetString(new string[1]
                {
                    xmlAttribute.Value
                });
                if (name.Length > 0)
                {
                    string index = "$(" + name + ")";
                    variables[index] = @string;
                }
            }
            for (int index1 = 0; index1 < xmlNodeList.Count; ++index1)
            {
                string attribute1 = XmlUtil.GetAttribute("name", xmlNodeList[index1]);
                string attribute2 = XmlUtil.GetAttribute("value", xmlNodeList[index1]);
                if (attribute1.Length > 0)
                {
                    string index2 = "$(" + attribute1 + ")";
                    variables[index2] = attribute2;
                }
            }
            if (variables.Count == 0)
                return;
            ReplaceGlobalVariables(rootNode, variables);
        }

        private void ReplaceGlobalVariables(XmlNode node, StringDictionary variables)
        {
            Assert.ArgumentNotNull((object) node, "node");
            Assert.ArgumentNotNull((object) variables, "variables");
            foreach (XmlAttribute xmlAttribute in (XmlNamedNodeMap) node.Attributes)
            {
                string str = xmlAttribute.Value;
                if (str.IndexOf('$') >= 0)
                {
                    foreach (string oldValue in variables.Keys)
                        str = str.Replace(oldValue, variables[oldValue]);
                    xmlAttribute.Value = str;
                }
            }
            foreach (XmlNode node1 in node.ChildNodes)
            {
                if (node1.NodeType == XmlNodeType.Element)
                    ReplaceGlobalVariables(node1, variables);
            }
        }

        private T AddToCache<T>(Hashtable cacheInstance, string key, T value) where T : class
        {
            Assert.IsNotNull((object) cacheInstance, "cacheInstance");
            lock (cacheInstance.SyncRoot)
            {
                T local_0 = cacheInstance[(object) key] as T;
                if ((object) local_0 != null)
                    return local_0;
                cacheInstance[(object) key] = (object) value;
                return value;
            }
        }

        private void AssignProperties(XmlNode configNode, string[] parameters, object obj, bool assert,
            bool deferred, IFactoryHelper helper)
        {
            Assert.ArgumentNotNull((object) configNode, "configNode");
            XmlNodeList childNodes = configNode.ChildNodes;
            ArrayList arrayList = new ArrayList();
            foreach (XmlNode node in childNodes)
            {
                if (IsPropertyNode(node, parameters, deferred))
                    arrayList.Add((object) node);
            }
            if (arrayList.Count <= 0)
                return;
            List<object> list = new List<object>(arrayList.Count*2);
            foreach (XmlNode xmlNode in arrayList)
            {
                if (helper == null || !helper.SetProperty(xmlNode, obj))
                {
                    string name = xmlNode.Name;
                    object innerObject = GetInnerObject(xmlNode, parameters, assert);
                    if (innerObject != null)
                    {
                        list.Add((object) name);
                        list.Add(innerObject);
                    }
                }
            }
            AssignProperties(obj, list.ToArray());
        }

        private void AssignProperties(object obj, object[] properties)
        {
            if (properties == null)
                return;
            int index1 = 0;
            while (index1 < properties.Length - 1)
            {
                string name = properties[index1] as string;
                object obj1 = properties[index1 + 1];
                Error.AssertString(name, "propertyName", false);
                if (obj1 is ObjectList)
                {
                    ObjectList objectList = obj1 as ObjectList;
                    ArrayList list1 = objectList.List;
                    int index2 = 0;
                    while (index2 < list1.Count - 1)
                    {
                        string str = list1[index2] as string;
                        object obj2 = list1[index2 + 1];
                        if (objectList.AddMethod.Length > 0)
                        {
                            object[] parameters;
                            if (!string.IsNullOrEmpty(str))
                                parameters = new object[2]
                                {
                                    (object) str,
                                    obj2
                                };
                            else
                                parameters = new object[1]
                                {
                                    obj2
                                };
                            Assert.IsNotNull(obj, "object");
                            MethodInfo method = ReflectionUtil.GetMethod(obj.GetType(), objectList.AddMethod, true, true,
                                true, parameters);
                            Error.AssertNotNull((object) method,
                                "Could not find add method: " + objectList.AddMethod + " (type: " +
                                obj.GetType().FullName + ")");
                            ReflectionUtil.InvokeMethod(method, parameters, obj);
                        }
                        else
                        {
                            PropertyInfo propertyInfo = ReflectionUtil.GetPropertyInfo(obj, name);
                            Assert.IsNotNull(obj, "object");
                            Error.AssertNotNull((object) propertyInfo,
                                "Could not get property info of: " + name + " (type: " + obj.GetType().FullName + ")");
                            if (!string.IsNullOrEmpty(str))
                            {
                                if (MainUtil.IsType(propertyInfo.PropertyType, typeof (IDictionary)))
                                {
                                    IDictionary dictionary = propertyInfo.GetValue(obj, new object[0]) as IDictionary;
                                    Assert.IsNotNull((object) dictionary, "dictionary");
                                    dictionary[(object) str] = obj2;
                                }
                                else
                                    Error.Raise("Could not assign values to the property: " + name +
                                                " (it is not an IDictionary)");
                            }
                            else if (MainUtil.IsType(propertyInfo.PropertyType, typeof (IList)))
                            {
                                IList list2 = propertyInfo.GetValue(obj, new object[0]) as IList;
                                Assert.IsNotNull((object) list2, "collection");
                                list2.Add(obj2);
                            }
                            else
                                Error.Raise("Could not assign values to the property: " + name + " (it is not an IList)");
                        }
                        index2 += 2;
                    }
                }
                else
                {
                    bool flag = ReflectionUtil.SetProperty(obj, name, obj1);
                    Assert.IsNotNull(obj, "object");
                    Assert.IsTrue((flag ? 1 : 0) != 0, "Could not find property '{0}' on object of type: {1}",
                        (object) name, (object) obj.GetType().FullName);
                }
                index1 += 2;
            }
        }

        private object CreateFromFactory(XmlNode configNode, string[] parameters, bool assert)
        {
            Assert.ArgumentNotNull((object) configNode, "configNode");
            string attribute1 = GetAttribute("factory", configNode, parameters);
            string attribute2 = GetAttribute("ref", configNode, parameters);
            if (attribute1.Length > 0 && attribute2.Length > 0)
            {
                IFactory factory = GetFactory(attribute1, assert);
                Error.Assert(!assert || factory != null, "Could not create factory: " + attribute1);
                if (factory != null)
                {
                    object @object = factory.GetObject(attribute2);
                    Error.Assert(!assert || @object != null,
                        "Could not get object from factory: " + attribute1 + ". Object id: " + attribute2);
                    return @object;
                }
            }
            return (object) null;
        }

        private object CreateFromReference(XmlNode configNode, string[] parameters, bool assert)
        {
            Assert.ArgumentNotNull((object) configNode, "configNode");
            string attribute = GetAttribute("ref", configNode, parameters);
            if (attribute.Length == 0)
                attribute = GetAttribute("path", configNode, parameters);
            if (attribute.Length <= 0)
                return (object) null;
            string[] callParameters = GetCallParameters(configNode, parameters);
            object @object = CreateObject(attribute, callParameters, assert);
            Error.Assert(!assert || @object != null, "Could not create object from reference path: " + attribute);
            return @object;
        }

        private object CreateFromTypeName(XmlNode configNode, string[] parameters, bool assert)
        {
            Assert.ArgumentNotNull((object) configNode, "configNode");
            Type type = CreateType(configNode, parameters, assert);
            if (!(type != (Type) null))
                return (object) null;
            object[] constructorParameters = GetConstructorParameters(configNode, parameters, assert);
            object @object = ReflectionUtil.CreateObject(type, constructorParameters);
            if (!assert || @object != null)
                return @object;
            string message = "Could not create instance of type: " + type.FullName + ".";
            if (ReflectionUtil.GetConstructorInfo(type, constructorParameters) == (ConstructorInfo) null)
                message = message + " No matching constructor was found.";
            throw new ConfigurationException(message);
        }

        private string[] GetCallParameters(XmlNode configNode, string[] parameters)
        {
            Assert.ArgumentNotNull((object) configNode, "configNode");
            ArrayList arrayList = (ArrayList) null;
            int num = 0;
            for (string name = "param" + (object) num;
                XmlUtil.HasAttribute(name, configNode) || num == 0;
                name = "param" + (object) num)
            {
                if (arrayList == null)
                    arrayList = new ArrayList();
                arrayList.Add((object) GetAttribute(name, configNode, parameters));
                ++num;
            }
            if (arrayList != null)
                return arrayList.ToArray(typeof (string)) as string[];
            else
                return (string[]) null;
        }

        private object[] GetConstructorParameters(XmlNode configNode, string[] parameters, bool assert)
        {
            Assert.ArgumentNotNull((object) configNode, "configNode");
            XmlNodeList xmlNodeList = configNode.SelectNodes("param");
            object[] objArray = new object[xmlNodeList.Count];
            for (int index = 0; index < xmlNodeList.Count; ++index)
                objArray[index] = GetInnerObject(xmlNodeList[index], parameters, assert);
            return objArray;
        }

        private IFactory GetFactory(string xpath)
        {
            Assert.ArgumentNotNull((object) xpath, "xpath");
            XmlNode configNode = GetConfigNode(xpath);
            if (configNode == null)
                return (IFactory) null;
            string attribute = XmlUtil.GetAttribute("factory", configNode);
            if (string.IsNullOrEmpty(attribute))
                return (IFactory) null;
            if (attribute.IndexOf(',') >= 0)
                return ReflectionUtil.CreateObject(attribute, new object[0]) as IFactory;
            else
                return GetFactory(attribute, false);
        }

        private IFactory GetFactory(string name, bool assert)
        {
            Assert.ArgumentNotNull((object) name, "name");
            XmlNode configNode = GetConfigNode("factories/factory[@id='" + name + "']", assert);
            Assert.IsNotNull((object) configNode, "configNode");
            return CreateObject(configNode, assert) as IFactory;
        }

        private object GetInnerObject(XmlNode paramNode, string[] parameters, bool assert)
        {
            Assert.ArgumentNotNull((object) paramNode, "paramNode");
            if (IsObjectNode(paramNode))
                return CreateObject(paramNode, parameters, assert);
            string attribute1 = GetAttribute("hint", paramNode, parameters);
            if (attribute1.StartsWith("call:", StringComparison.InvariantCulture))
            {
                ObjectList objectList = new ObjectList(attribute1.Substring(5));
                objectList.Add(string.Empty, (object) paramNode);
                return (object) objectList;
            }
            else
            {
                bool flag1 = attribute1.StartsWith("list:", StringComparison.InvariantCulture) || attribute1 == "list" ||
                             attribute1 == "dictionary";
                bool flag2 = attribute1.StartsWith("raw:", StringComparison.InvariantCulture);
                string addMethod = StringUtil.Mid(attribute1, flag2 ? 4 : 5);
                XmlNodeList xmlNodeList = paramNode.SelectNodes("node()");
                if (xmlNodeList.Count > 0)
                {
                    if (attribute1 == "setting")
                        return (object) null;
                    bool flag3 = attribute1.StartsWith("version:", StringComparison.InvariantCulture) ||
                                 attribute1 == "version";
                    if (!flag1 && !flag2 && !flag3)
                        return CreateObject(xmlNodeList[0], parameters, assert);
                    if (flag3)
                    {
                        //Lucene.Net.Util.Version result;
                        //Enum.TryParse<Lucene.Net.Util.Version>(paramNode.InnerText, true, out result);
                        //return (object) result;
                        return null;
                    }
                    else
                    {
                        ObjectList objectList = new ObjectList(addMethod);
                        foreach (XmlNode xmlNode in xmlNodeList)
                        {
                            if (xmlNode.NodeType == XmlNodeType.Element)
                            {
                                if (flag2)
                                    ReplaceVariables(xmlNode, parameters);
                                string attribute2 = GetAttribute("key", xmlNode, parameters);
                                object obj = flag2
                                    ? (object) xmlNode
                                    : CreateObject(xmlNode, parameters, assert);
                                if (obj != null)
                                    objectList.Add(attribute2, obj);
                            }
                        }
                        return (object) objectList;
                    }
                }
                else if (flag1 || flag2)
                    return (object) new ObjectList(addMethod);
                else
                    return (object) GetStringValue(paramNode, parameters);
            }
        }

        private string[] GetNames(string configPath, string nameAttribute)
        {
            Assert.ArgumentNotNull((object) configPath, "configPath");
            Assert.ArgumentNotNull((object) nameAttribute, "nameAttribute");
            ArrayList arrayList = new ArrayList();
            foreach (XmlNode node in GetConfigNodes(configPath))
            {
                string attribute = XmlUtil.GetAttribute(nameAttribute, node);
                if (attribute.Length > 0)
                    arrayList.Add((object) attribute);
            }
            return arrayList.ToArray(typeof (string)) as string[] ?? new string[0];
        }

        private TCollection GetProviders<TProvider, TCollection>(List<XmlNode> nodes)
            where TProvider : ProviderBase where TCollection : ProviderCollection, new()
        {
            Assert.ArgumentNotNull((object) nodes, "nodes");
            TCollection instance = Activator.CreateInstance<TCollection>();
            foreach (XmlNode xmlNode in nodes)
            {
                if (xmlNode.LocalName == "clear")
                {
                    instance.Clear();
                }
                else
                {
                    Assert.IsTrue((xmlNode.LocalName == "add" ? 1 : 0) != 0, "Unknown node name in provider list: {0}",
                        new object[1]
                        {
                            (object) xmlNode.LocalName
                        });
                    object @object = CreateObject(xmlNode, true);
                    Type type1 = @object.GetType();
                    Type type2 = typeof (TProvider);
                    Assert.IsTrue((type2.IsAssignableFrom(type1) ? 1 : 0) != 0,
                        "Unexpected provider type: {0}. Expected: {1}", (object) type1.FullName, (object) type2.FullName);
                    TProvider provider = @object as TProvider;
                    if ((object) provider == null)
                        throw new InvalidOperationException("Provider definition must have a 'name' attribute. Xml: " +
                                                            xmlNode.OuterXml);
                    string attribute = XmlUtil.GetAttribute("name", xmlNode);
                    if (attribute.Length == 0)
                        throw new InvalidOperationException("Provider definition must have a 'name' attribute. Xml: " +
                                                            xmlNode.OuterXml);
                    provider.Initialize(attribute, XmlUtil.GetAttributes(xmlNode));
                    instance.Add((ProviderBase) provider);
                }
            }
            return instance;
        }

        private string GetStringValue(XmlNode node, string[] parameters)
        {
            Assert.ArgumentNotNull((object) node, "node");
            XmlNode node1 = node;
            if (node1.NodeType == XmlNodeType.Attribute)
            {
                XmlAttribute xmlAttribute = node1 as XmlAttribute;
                if (xmlAttribute != null)
                    node1 = (XmlNode) xmlAttribute.OwnerElement;
            }
            if (node1.NodeType != XmlNodeType.Element)
                node1 = node1.ParentNode;
            return ReplaceVariables(node.InnerText, node1, parameters);
        }

        private bool IsObjectNode(XmlNode node)
        {
            Assert.ArgumentNotNull((object) node, "node");
            if (node.NodeType != XmlNodeType.Element)
                return false;
            if (!XmlUtil.HasAttribute("ref", node) && !XmlUtil.HasAttribute("type", node) &&
                !XmlUtil.HasAttribute("path", node))
                return XmlUtil.HasAttribute("connectionStringName", node);
            else
                return true;
        }

        private bool IsPropertyNode(XmlNode node, string[] parameters, bool deferred)
        {
            Assert.ArgumentNotNull((object) node, "node");
            if (node.NodeType != XmlNodeType.Element || node.Name == "param" ||
                GetAttribute("hint", node, parameters) == "skip")
                return false;
            bool flag = GetAttribute("hint", node, parameters) == "defer";
            return deferred == flag;
        }

        private void ReplaceVariables(XmlNode node, string[] parameters)
        {
            Assert.ArgumentNotNull((object) node, "node");
            foreach (XmlAttribute xmlAttribute in (XmlNamedNodeMap) node.Attributes)
                xmlAttribute.Value = ReplaceVariables(xmlAttribute.Value, node, parameters);
        }

        private string ReplaceVariables(string value, XmlNode node, string[] parameters)
        {
            Assert.ArgumentNotNull((object) value, "value");
            Assert.ArgumentNotNull((object) node, "node");
            for (node = node.ParentNode;
                node != null && node.NodeType == XmlNodeType.Element &&
                value.IndexOf("$(", StringComparison.InvariantCulture) >= 0;
                node = node.ParentNode)
            {
                foreach (XmlAttribute xmlAttribute in (XmlNamedNodeMap) node.Attributes)
                {
                    string oldValue = "$(" + xmlAttribute.LocalName + ")";
                    value = value.Replace(oldValue, xmlAttribute.Value);
                }
                value = value.Replace("$(name)", node.LocalName);
            }
            if (parameters != null)
            {
                for (int index = 0; index < parameters.Length; ++index)
                    value = value.Replace("$(" + (object) index + ")", parameters[index]);
            }
            return value;
        }

        private ItemNavigator GetItemNavigatorPrototype()
        {
            ItemNavigator itemNavigator1 = itemNavigatorPrototype;
            if (itemNavigator1 != null)
                return itemNavigator1;
            ItemNavigator itemNavigator2 = CreateObject("prototypes/ItemNavigator", true) as ItemNavigator;
            Assert.IsNotNull((object) itemNavigator2, "Could not create prototype: ItemNavigator");
            itemNavigatorPrototype = itemNavigator2;
            return itemNavigator2;
        }

        public enum HashKeyType
        {
            NodeName,
        }

        public enum HashValueFormat
        {
            SingleObject,
            Array,
        }

        public enum HashValueType
        {
            NodeName,
            Object,
        }
    }

    internal class NullRetryer : IRetryable
    {
        //public TimeSpan Interval
        //{
        //    get
        //    {
        //        return new TimeSpan();
        //    }
        //}

        //public int RepeatNumber
        //{
        //    get
        //    {
        //        return 0;
        //    }
        //}

        //public T Execute<T>(Func<T> action)
        //{
        //    Assert.ArgumentNotNull((object)action, "action");
        //    return this.Execute<T>(action, (Action)null);
        //}

        //public T Execute<T>(Func<T> action, Action recover)
        //{
        //    Assert.IsNotNull((object)action, "action");
        //    try
        //    {
        //        using (new RetryerDisabler((IRetryable)this))
        //            return action();
        //    }
        //    catch
        //    {
        //        if (recover != null)
        //            recover();
        //        throw;
        //    }
        //}

        //public void ExecuteNoResult(Action action)
        //{
        //    Assert.IsNotNull((object)action, "action");
        //    this.ExecuteNoResult(action, (Action)null);
        //}

        //public void ExecuteNoResult(Action action, Action recover)
        //{
        //    Assert.ArgumentNotNull((object)action, "action");
        //    try
        //    {
        //        using (new RetryerDisabler((IRetryable)this))
        //            action();
        //    }
        //    catch
        //    {
        //        if (recover != null)
        //            recover();
        //        throw;
        //    }
        //}
        public T Execute<T>(Func<T> action)
        {
            throw new NotImplementedException();
        }

        public T Execute<T>(Func<T> action, Action recover)
        {
            throw new NotImplementedException();
        }

        public void ExecuteNoResult(Action action)
        {
            throw new NotImplementedException();
        }

        public void ExecuteNoResult(Action action, Action recover)
        {
            throw new NotImplementedException();
        }

        public TimeSpan Interval { get; private set; }
        public int RepeatNumber { get; private set; }
    }
}


