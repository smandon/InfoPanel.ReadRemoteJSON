# InfoPanel Read Remote JSON Plugin

[![Windows](https://badgen.net/badge/icon/windows?icon=windows&label)](https://microsoft.com/windows/)
[![.NET 8.0](https://img.shields.io/badge/.NET-8.0--windows-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Visual Studio](https://badgen.net/badge/icon/visualstudio?icon=visualstudio&label)](https://visualstudio.microsoft.com)
[![Open Source? Yes!](https://badgen.net/badge/Open%20Source%20%3F/Yes%21/blue?icon=github)](https://github.com/Naereen/badges/)
[![GPLv3 license](https://img.shields.io/badge/License-GPLv3-blue.svg)](http://perso.crans.org/besson/LICENSE.html)

A plugin for [InfoPanel](https://github.com/habibrehmansg/infopanel) that allows reading multiple values inside Json data over HTTP connection.

## Installation and setup

1. **Prerequisites**
- [InfoPanel](https://github.com/habibrehmansg/infopanel) application installed
- Windows operating system
- .NET 8.0 Runtime

2. **Download the plugin**
    - Download the latest release \*.zip file (`InfoPanel.ReadRemoteJSON-vX.X.X.zip`) from the [GitHub Releases page](https://github.com/smandon/InfoPanel.ReadRemoteJSON/releases).

3. **Import the plugin into InfoPanel**
   - Open the InfoPanel app.
   - Navigate to the **Plugins** page.
   - Click the **Import Plugin** button, then select the downloaded ZIP file.
   - InfoPanel will extract and install the plugin.
   - Quit and restart the InfoPanel app.

4. **Configure the plugin**
    - On the Plugins page, click the dropdown button at the right of the InfoPanel.JsonWeb item.
    - Click on the **Open Config File** button.
    - Change values in the config file:
        - ApiUrl: endpoint of the API your want to query
        - UpdateInterval: update interval in seconds
        - Token: optional authentication token (will add the token in the authentication HTTP header)
        - for each value to read, you must create a `[valueN]` section for each value to read, which must contain four keys:
            - Name: the name to display for the sensor
            - JsonPath: [Json Path](https://en.wikipedia.org/wiki/JSONPath) to identify the value you want to read
            - DataType: the type of data to read, either text or float (to be precise, anything else than text will be treated as float)
            - Unit: the sensor's unit (optional)
    - Click the **Reload** button to reload the plugins with your configuration
    - If there is an error and it cannot read values (due to a wrong configuration for example), it will always return zero as value for float data type and "no data" for text data type.

## Configuration example
I will take the Shelly Plug S as example, it is a Wi-Fi enabled smart plug with integrated power meter. It exposes status data (power, temperature,  relay status...) via a local endpoint as Json Data (cf. [API documentation](https://shelly-api-docs.shelly.cloud/gen1/#shelly-plug-plugs)).

The endpoint value is the URL used to access this api: e.g. http://192.168.1.2/status

The JSON data that it sends back looks like this:
```json
{
    "wifi_sta": {
        "connected": true,
        "ssid": "REDACTED",
        "ip": "192.168.1.2",
        "rssi": -50
    },
    "cloud": {
        "enabled": false,
        "connected": false
    },
    "mqtt": {
        "connected": false
    },
    "time": "01:45",
    "unixtime": 1762044312,
    "serial": 00000,
    "has_update": false,
    "mac": "REDACTED",
    "cfg_changed_cnt": 0,
    "actions_stats": {
        "skipped": 0
    },
    "relays": [{
            "ison": true,
            "has_timer": false,
            "timer_started": 0,
            "timer_duration": 0,
            "timer_remaining": 0,
            "overpower": false,
            "source": "http"
        }
    ],
    "meters": [{
            "power": 63.28,
            "overpower": 0.00,
            "is_valid": true,
            "timestamp": 1762047912,
            "counters": [67.700, 60.359, 59.919],
            "total": 387711
        }
    ],
    "temperature": 28.78,
    "overtemperature": false,
    "tmp": {
        "tC": 28.78,
        "tF": 83.80,
        "is_valid": true
    },
    "update": {
        "status": "idle",
        "has_update": false,
        "new_version": "20230913-113421/v1.14.0-gcb84623",
        "old_version": "20230913-113421/v1.14.0-gcb84623",
        "beta_version": "20231107-164219/v1.14.1-rc1-g0617c15"
    },
    "ram_total": 52056,
    "ram_free": 39364,
    "fs_size": 233681,
    "fs_free": 165911,
    "uptime": 252757
}
```

If we want to read both the power and temperature values as well as the new_version:
- for power, user JsonPath value : `$.meters[0].power` as float data tpye
- for temperature, use JsonPath value: `$.temperature` as float data type
- for new_version, use JsonPath value: `$.update.new_version` as text data type

You can use sites like [jsonpath.com](https://jsonpath.com/) to test and validate your JsonPath values.

The config file in that case would look like:
```ini
[config]
ApiUrl=http://192.168.1.2/status
UpdateInterval=1
Token=

[value1]
Name=Power
JsonPath=$.meters[0].power
DataType=float
Unit=W

[value2]
Name=Temperature
JsonPath=$.temperature
DataType=float
Unit=°C

[value3]
Name=New Version
JsonPath=$.update.new_version
DataType=text
Unit=
```

> [!WARNING]
> The ini file is expected to be encoded in UTF-8.

# Development

This plugin was developed using the official InfoPanel [plugin development guide](https://github.com/habibrehmansg/infopanel/blob/main/PLUGINS.md).

# Version history
- **1.0.0** Initial release
- **1.1.0** Added support for an authentication bearer token and data type setting

# External dependencies

### Newtonsoft Json.NET
- **License**: MIT License
- **Copyright**: © 2025 Newtonsoft
- **Project**: https://www.newtonsoft.com/json

The dll is included in the release and was downloaded [here](https://github.com/JamesNK/Newtonsoft.Json/releases/tag/13.0.4)



# License

This plugin is licensed under the GPL 3.0 - see the [license file][license] for details.