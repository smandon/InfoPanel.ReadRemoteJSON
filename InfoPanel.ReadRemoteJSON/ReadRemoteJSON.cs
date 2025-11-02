using InfoPanel.Plugins;
using IniParser;
using IniParser.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace InfoPanel.ReadRemoteJSON
{
    public class ReadRemoteJSON : BasePlugin
    {
        private string _apiUrl = "";
        private double _updateInterval = 1;
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly List<PluginContainer> _containers = new();
        private readonly List<string> _jsonpathValues = new();
        private IniData? IniData { get; set; }

        public bool TryGetValue(string section, string key, out string value)
        {
            value = string.Empty;
            if (IniData != null && IniData[section].ContainsKey(key))
            {
                value = IniData[section][key];
                return true;
            }
            value = "";
            return false;
        }

        public ReadRemoteJSON() : base("readremotejson-plugin", "Read Remote JSON", "Reads values from JSON data over HTTP")
        {
        }

        public override string? ConfigFilePath => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "config.ini");

        public override void Initialize()
        {
            PluginContainer defaultContainer = new("Read remote JSON", "Json Data");

            if (File.Exists(ConfigFilePath))
            {
                var parser = new FileIniDataParser();
                IniData = parser.ReadFile(ConfigFilePath, Encoding.UTF8);

                if (TryGetValue("config", "ApiUrl", out var apiUrlValue))
                {
                    _apiUrl = apiUrlValue;
                    if (TryGetValue("config", "UpdateInterval", out var updateIntervalValue))
                    {
                        _updateInterval = Convert.ToDouble(updateIntervalValue);
                    }

                    CultureInfo ci = new CultureInfo("en-US");
                    foreach (SectionData section in IniData.Sections)
                    {
                        if (section.SectionName.StartsWith("value", true, ci)) {
                            if (TryGetValue(section.SectionName, "Name", out var _nameValue) &&
                            TryGetValue(section.SectionName, "JsonPath", out var _jsonpathValue) &&
                            TryGetValue(section.SectionName, "Unit", out var _unitValue))
                            {
                                defaultContainer.Entries.Add(new PluginSensor(section.SectionName, _nameValue, 0, _unitValue));
                                _jsonpathValues.Add(_jsonpathValue);
                            }
                        }
                    }
                    _containers.Add(defaultContainer);
                }
            }
        }

        public override TimeSpan UpdateInterval => TimeSpan.FromSeconds(_updateInterval); // Not sure if using the config file's value

        public override void Load(List<IPluginContainer> containers)
        {
            containers.AddRange(_containers.Cast<IPluginContainer>());
        }

        public override async Task UpdateAsync(CancellationToken cancellationToken)
        {
            try
            {
                string jsonData = await _httpClient.GetStringAsync(_apiUrl);

                foreach(var container in _containers)
                {
                    for (int i=0; i<container.Entries.Count;i++)
                    {
                        string _jsonpath = _jsonpathValues[i];
                        float value = GetValueFromJson(jsonData, _jsonpath);
                        if (container.Entries[i] is PluginSensor sensor)
                        {
                            sensor.Value = value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                foreach (var container in _containers)
                {
                    foreach (var entry in container.Entries.OfType<PluginSensor>())
                    {
                        entry.Value = 0;
                    }
                }
            }
        }

        public override void Update() => throw new NotImplementedException();
        public override void Close() => _httpClient.Dispose();

        private float GetValueFromJson(string json, string jsonPath)
        {
            JObject jo = JObject.Parse(json);
            var value = jo.SelectToken(jsonPath);
            if (value != null)
            {
                return (float)value;
            }
            else
            {
                return 0;
            }

        }
    }
}
