# Vegas Language Changer
[![Platform](https://img.shields.io/badge/platform-windows--xp+-blue.svg)](https://www.microsoft.com/windows)
[![Framework](https://img.shields.io/badge/.NET%20Framework-4.0-blueviolet.svg)](https://dotnet.microsoft.com/download/dotnet-framework)
[![License](https://img.shields.io/badge/license-GPLv3-green.svg)](https://www.gnu.org/licenses/gpl-3.0.html)
[![Developer](https://img.shields.io/badge/developer-IZH318-lightgrey.svg)](https://github.com/IZH318) <br> <br>

<img width="586" height="510" alt="캡처_2025_09_24_03_47_13_354" src="https://github.com/user-attachments/assets/83ed5bb6-e6ba-4bc3-a58a-441d88dc46f3" />

A utility to easily change the UI language of VEGAS Pro and VEGAS Movie Studio. <br> <br>
Instead of the complex process of manually editing the registry, you can instantly switch to your desired language with just a few clicks through an intuitive graphical interface.

<br>

## 🌟 Key Features
-   **Automatic Version Detection**: Automatically scans for and lists installed versions of **VEGAS Pro (v9.0 ~ v23.0)** and **Movie Studio (v8.0 ~ v17.0)**.
-   **Batch Language Change**: Select multiple VEGAS versions to change their language all at once.
-   **Intelligent Language Filtering**: The dropdown menu only shows languages commonly supported by all selected versions, preventing incompatible language settings.
-   **Safe Registry Backup**: Before changing the language, your current registry settings are automatically backed up to a `.reg` file on your desktop, allowing for easy restoration.
-   **Automatic Plugin Cache Clearing**: Deletes relevant cache files to prevent language mixing issues that can occur after a language change.
-   **Automatic Language File Restoration**: If a specific language file (`.cfg`) is missing in VEGAS Pro 14 or later, the program automatically copies a built-in file to the VEGAS installation path to ensure a complete language change.
-   **Multilingual UI Support**: The program's UI is automatically localized by detecting the system language. (Supports 10 languages, including English, Korean, Japanese, Chinese, and German)
-   **Automatic Administrator Privilege Request**: Displays a UAC (User Account Control) prompt upon execution to request the necessary administrator privileges for registry modification.

<br>

## 🔄 Changelog

### v1.1.0 (2025-10-28)

-   #### **Changes**
    -   **Use of official cfg files**: Switched from custom-generated cfg files to official ones when processing Vegas Pro 20 ~ 23.
    -   **Updated About dialog**: Added a 'Special Thanks' section.
    -   **Improved localization**: Refined translations for more natural phrasing.

<br>

<details>
<summary>📜 Previous Updates - Click to expand</summary>
<br>
<details>
<summary>v1.0.0 (2025-09-24)</summary>
  
  -   **Misc**
      -   Initial release of `Vegas Language Changer`.

</details>
</details>

<br>

## 📋 Requirements
- **Operating System**: **Windows XP SP3** or later
- **Framework**: **.NET Framework 4.0** or later
  - *Comes pre-installed on Windows 8, 10, and 11. Users on Windows XP or 7 may need to install it.* ➜ .NET Framework 4.0 [**Download**](https://www.microsoft.com/en-us/download/details.aspx?id=17851)

<br>

## 🚀 How to Use
1.  Go to the GitHub **[Releases](https://github.com/IZH318/Vegas-Language-Changer/releases)** page.
2.  Download the latest `.exe` file.
3.  Run the downloaded `.exe` file.
    -   *Note: The program will request administrator privileges (UAC) to modify the registry. A Windows SmartScreen warning may appear. If so, click 'More info' > 'Run anyway' to proceed.*
4.  In the **Available Versions** list, double-click a VEGAS version you want to change, or select it and click the `>` button to move it to the **Versions to Change** list.
5.  Select the desired language from the dropdown menu at the bottom.
6.  Click the **Change Language** button. A confirmation dialog will appear. Click `OK` to complete the process.

<br>

## 🛠️ Development Environment
This project was developed and built based on the following environment.
-   **Operating System (OS):** Windows 10 Pro (64-bit)
-   **IDE:** Microsoft Visual Studio 2019 (v142)
-   **Required Workload:** .NET desktop development (Windows Forms)
-   **Target Framework:** .NET Framework 4.0
-   **Language:** C#

<br>

## 🛠️ Technical Deep Dive
This program uses C# and Windows Forms (.NET Framework 4.0) to safely and reliably change the language settings of the VEGAS product family in a Windows environment, utilizing the following core technologies.

### 1. Registry Scanning and Control
-   **Scan Target**: Scans are centered around the `HKEY_LOCAL_MACHINE\SOFTWARE` key. To detect 32-bit VEGAS versions on 64-bit Windows, it queries both `RegistryView.Registry64` and `RegistryView.Registry32`, which covers the 32-bit registry path (`Wow6432Node`).
-   **Core Key**: The language setting for each VEGAS version is determined by the `ULangID` (REG_DWORD) value in the following path. The program uses the `Microsoft.Win32.RegistryKey` class to change this value to the LCID (Locale ID) of the target language.
    ```
    HKLM\SOFTWARE\{Vendor}\{Product}\{Version}\Lang
    ```
    -   `{Vendor}`: `Sony Creative Software`, `MAGIX`, `VEGAS Creative Software`, etc.
    -   `{Product}`: `Vegas Pro`, `Movie Studio Platinum`, etc.

### 2. Safety Features
-   **Registry Backup**: Uses `System.Text.StringBuilder` to format the target `ULangID` values into the standard `.reg` file format, then saves it to the desktop as `Vegas_RegBackup_{timestamp}.reg` using `File.WriteAllText`. The file is saved with `Encoding.Unicode` for compatibility with the Windows Registry Editor.
-   **Plugin Cache Cleanup**: To resolve issues where some plugin windows might still appear in the previous language, the program gets the `%LOCALAPPDATA%` path using `Environment.GetFolderPath` and automatically deletes the cache files for each version (`plugin_manager_cache.bin`, `svfx_plugin_cache.bin`) using `File.Delete`. When VEGAS restarts, these files are cleanly regenerated according to the new language settings.

### 3. Language File (`.cfg`) Restoration Mechanism
Starting with VEGAS Pro 14.0, a complete language switch requires not only the `ULangID` registry value but also the corresponding `.cfg` file in the `Language` directory within the installation folder. This program solves this by embedding the necessary `.cfg` files as **Embedded Resources**.

1.  **Resource Access**: The `System.Reflection.Assembly.GetManifestResourceStream()` method is used to get the data stream of the `.cfg` file embedded within the `.exe` executable.
2.  **Path Discovery and Copying**: It checks for the existence of a `Language` or `language` folder based on the `InstallPath` read from the registry.
3.  **Conditional Copy**: Only if the target language's `.cfg` file (e.g., `local_ko_KR.cfg`) does not exist in the destination folder, the embedded resource is copied to a physical file using `Stream.CopyTo` and a `FileStream`.

<br>

## 📋 Supported Languages by Version
This program supports a wide range of VEGAS products. <br>

The table below lists the officially supported languages for each version, all of which can be switched using the Vegas Language Changer.

> **⚠️ Important Note on Language Changes**
>
> Even when using this tool, some VEGAS versions may not apply certain languages (especially **Polish** and **Russian**) perfectly. This issue is caused by missing or incomplete language resource files within the VEGAS program itself.
>
> Therefore, while this tool correctly performs all necessary steps to change the language, the final display result may vary depending on your VEGAS installation environment.

### VEGAS Pro
| Version | Tested Build | Supported Languages |
| :--- | :--- | :--- |
| **1.0** | 134 | English |
| **2.0** | 302 | English |
| **3.0** | 76 | English |
| **4.0** | 115 | English |
| **5.0** | 122 | English |
| **6.0** | 84 | English |
| **7.0** | 115 | English |
| **8.0** | 144 | English |
| **8.1** | 171 | English |
| **9.0** | 562 | German, English, Spanish, French, Japanese |
| **10.0** | 388 | German, English, Spanish, French, Japanese |
| **11.0** | 371 | German, English, Spanish, French, Japanese, Russian |
| **12.0** | 367 | German, English, Spanish, French, Japanese, Russian |
| **13.0** | 290 | German, English, Spanish, French, Japanese, Polish, Russian, Chinese (Simplified) |
| **14.0** | 161 | German, English, Spanish, French, Japanese, Polish, Russian, Chinese (Simplified) |
| **15.0** | 177, 384, 416 | German, English, Spanish, French, Japanese, Polish, Russian, Chinese (Simplified) <br> **Note:** Korean support added from build 384. |
| **16.0** | 248 | German, English, Spanish, French, Japanese, Korean, Polish, Portuguese, Russian, Chinese (Simplified) |
| **17.0** | 284 | German, English, Spanish, French, Japanese, Korean, Polish, Portuguese, Chinese (Simplified) |
| **18.0** | 284 | German, English, Spanish, French, Japanese, Korean, Polish, Portuguese, Chinese (Simplified) |
| **19.0** | 341 | German, English, Spanish, French, Japanese, Korean, Polish, Portuguese, Chinese (Simplified) |
| **20.0** | 411 | German, English, Spanish, French, Japanese, Korean, Polish, Portuguese, Chinese (Simplified) |
| **21.0** | 108 | German, English, Spanish, French, Japanese, Korean, Polish, Portuguese, Chinese (Simplified) |
| **22.0** | 194 | German, English, Spanish, French, Japanese, Korean, Polish, Portuguese, Chinese (Simplified) |
| **23.0** | 278 | German, English, Spanish, French, Japanese, Korean, Polish, Portuguese, Chinese (Simplified) |

### VEGAS Movie Studio
| Version | Tested Build | Supported Languages |
| :--- | :--- | :--- |
| **8.0** | 142 | German, English, Spanish, French, Japanese |
| **Platinum 8.0** | 139 | German, English, Spanish, French, Japanese |
| **9.0** | 33 | English |
| **Platinum 9.0** | 92 | German, English, Spanish, French, Japanese |
| **HD Platinum 10.0** | 179 | German, English, Spanish, French, Japanese |
| **HD Platinum 11.0** | 295 | German, English, Spanish, French, Japanese |
| **Platinum 12.0** | 576 | German, English, Spanish, French, Japanese, Polish, Russian, Chinese (Simplified) |
| **Platinum 13.0** | 943 | German, English, Spanish, French, Japanese, Polish, Russian, Chinese (Simplified) |
| **Platinum 14.0** | 148 | German, English, Spanish, French, Japanese, Polish, Portuguese, Russian, Chinese (Simplified) |
| **Platinum 15.0** | 157 | German, English, Spanish, French, Japanese, Polish, Portuguese, Russian, Chinese (Simplified) |
| **Platinum 16.0** | 109 | German, English, Spanish, French, Japanese, Korean, Polish, Portuguese, Russian, Chinese (Simplified) |
| **Platinum 17.0** | 143 | German, English, Spanish, French, Japanese, Korean, Polish, Portuguese, Russian, Chinese (Simplified) |

<br>

## 👏 Special Thanks

A huge thank you to the following people for their contributions that made this project more complete.

*   **[zzzzzz9125](https://github.com/zzzzzz9125)**
    *   For providing crucial official language configuration (`.cfg`) files needed for the program.
        *   **Vegas Pro 20:** `local_zh_CN.cfg`
        *   **Vegas Pro 21 ~ 23:** All official `.cfg` files

<br>

## 📜 License
This program is licensed under the **[GNU General Public License v3.0](LICENSE)**.
