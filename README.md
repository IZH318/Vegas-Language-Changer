# Vegas Language Changer
[![Platform](https://img.shields.io/badge/platform-windows--xp+-blue.svg)](https://www.microsoft.com/windows)
[![Framework](https://img.shields.io/badge/.NET%20Framework-4.0-blueviolet.svg)](https://dotnet.microsoft.com/download/dotnet-framework)
[![License](https://img.shields.io/badge/license-GPLv3-green.svg)](https://www.gnu.org/licenses/gpl-3.0.html)
[![Developer](https://img.shields.io/badge/developer-IZH318-lightgrey.svg)](https://github.com/IZH318) <br> <br>

<img width="586" height="510" alt="캡처_2025_09_24_03_47_13_354" src="https://github.com/user-attachments/assets/c549fbcc-62ab-41bb-bde1-0c937c2eb033" />

VEGAS Pro 및 VEGAS Movie Studio의 UI 언어를 손쉽게 변경할 수 있는 유틸리티입니다. <br> <br>
레지스트리를 직접 수정하는 복잡한 과정 없이, 직관적인 그래픽 인터페이스를 통해 클릭 몇 번으로 원하는 언어로 즉시 전환할 수 있습니다. <br> <br>

**영문 버전의 README는 [README_EN.md](./README_EN.md)에서 확인하실 수 있습니다.** <br>
**You can find the English version of the README in [README_EN.md](./README_EN.md).**

<br>

## 🌟 주요 기능 (Key Features)
-   **자동 버전 탐지**: PC에 설치된 **VEGAS Pro(v9.0 ~ v23.0)** 및 **Movie Studio(v8.0 ~ v17.0)** 버전을 자동으로 스캔하고 목록에 표시합니다.
-   **일괄 언어 변경**: 여러 버전의 VEGAS를 선택하여 한 번에 언어를 변경할 수 있습니다.
-   **지능형 언어 필터링**: 선택된 모든 버전에서 공통으로 지원하는 언어만 드롭다운 메뉴에 표시하여, 호환되지 않는 언어 설정을 원천 차단합니다.
-   **안전한 레지스트리 백업**: 언어 변경 실행 전, 현재 레지스트리 설정을 바탕화면에 `.reg` 파일로 자동 백업하여 언제든 원상 복구가 가능합니다.
-   **플러그인 캐시 자동 삭제**: 언어 변경 후 발생할 수 있는 언어 섞임 문제를 예방하기 위해 관련 캐시 파일을 자동으로 삭제합니다.
-   **언어 파일 자동 복원**: VEGAS Pro 14 이상 버전에서 특정 언어 파일(`.cfg`)이 누락된 경우, 프로그램에 내장된 파일을 VEGAS 설치 경로에 자동으로 복사하여 완벽한 언어 변경을 지원합니다.
-   **다국어 UI 지원**: 시스템 언어를 감지하여 프로그램 UI가 자동으로 현지화됩니다. (영어, 한국어, 일본어, 중국어, 독일어 등 10개 언어 지원)
-   **관리자 권한 자동 요청**: 레지스트리 수정에 필요한 관리자 권한을 위해 프로그램 실행 시 UAC(사용자 계정 컨트롤) 프롬프트를 표시합니다.

<br>

## 📋 요구 사항 (Requirements)
- **운영 체제**: **Windows XP SP3** 이상
- **프레임워크**: **.NET Framework 4.0** 이상
  - *Windows 8, 10, 11에는 기본적으로 설치되어 있으며, Windows XP, 7 사용자는 설치가 필요할 수 있습니다.* ➜ .NET Framework 4.0 [**다운로드**](https://www.microsoft.com/en-us/download/details.aspx?id=17851)

<br>

## 🚀 사용 방법 (How to Use)
1.  GitHub **[Releases](https://github.com/IZH318/Vegas-Language-Changer/releases)** 페이지로 이동합니다.
2.  최신 버전의 `.exe` 파일을 다운로드합니다.
3.  다운로드한 `.exe` 파일을 실행합니다.
    -   *참고: 프로그램 실행 시 레지스트리 수정을 위해 관리자 권한(UAC)을 요청하며, Windows SmartScreen 경고가 표시될 수 있습니다. 이 경우 '추가 정보' > '실행'을 클릭하여 진행하세요.*
4.  **사용 가능 버전(Available Versions)** 목록에서 언어를 변경하고 싶은 VEGAS 버전을 더블클릭, 또는 선택한 후 `>` 버튼을 눌러 **변경할 버전(Versions to Change)** 목록으로 이동시킵니다.
5.  하단의 드롭다운 메뉴에서 변경하고 싶은 언어를 선택합니다.
6.  `언어 변경(Change Language)` 버튼을 클릭하면 확인 창이 나타나고, `OK`를 누르면 작업이 완료됩니다.

<br>

## 🛠️ 개발 환경 (Development Environment)
이 프로젝트는 다음 환경을 기준으로 개발 및 빌드되었습니다.
-   **운영 체제 (OS):** Windows 10 Pro (64-bit)
-   **개발 도구 (IDE):** Microsoft Visual Studio 2019 (v142)
-   **필수 워크로드 (Workload):** .NET 데스크톱 개발 (Windows Forms)
-   **대상 프레임워크 (Target Framework):** .NET Framework 4.0
-   **언어 (Language):** C#

<br>

## 🛠️ 기술적 분석 (Technical Deep Dive)
이 프로그램은 C#과 Windows Forms(.NET Framework 4.0)를 사용하여 Windows 환경에서 VEGAS 제품군의 언어 설정을 안정적으로 변경하기 위해 다음과 같은 핵심 기술을 사용합니다.

### 1. 레지스트리 스캔 및 제어
-   **탐색 대상**: `HKEY_LOCAL_MACHINE` 하이브의 `SOFTWARE` 키를 중심으로 스캔합니다. 64비트 Windows 환경에서 32비트 VEGAS 버전도 탐지하기 위해 `RegistryView.Registry64`와 `RegistryView.Registry32` 뷰를 모두 사용하여 32비트 레지스트리 경로(`Wow6432Node`)까지 함께 조회합니다.
-   **핵심 키**: 각 VEGAS 버전의 언어 설정은 다음 경로의 `ULangID` (REG_DWORD) 값에 의해 결정됩니다. 이 프로그램은 `Microsoft.Win32.RegistryKey` 클래스를 사용하여 해당 값을 목표 언어의 LCID(언어 코드 식별자)로 변경합니다.
    ```
    HKLM\SOFTWARE\{Vendor}\{Product}\{Version}\Lang
    ```
    -   `{Vendor}`: `Sony Creative Software`, `MAGIX`, `VEGAS Creative Software` 등
    -   `{Product}`: `Vegas Pro`, `Movie Studio Platinum` 등

### 2. 안전 장치 (Safety Features)
-   **레지스트리 백업**: `System.Text.StringBuilder`를 사용하여 변경 대상 `ULangID` 값들을 표준 `.reg` 파일 형식으로 구성한 후, `File.WriteAllText`를 통해 바탕화면에 `Vegas_RegBackup_{timestamp}.reg` 파일로 저장합니다. 이 파일은 Windows 레지스트리 편집기와의 호환성을 위해 `Encoding.Unicode`로 저장됩니다.
-   **플러그인 캐시 정리**: 언어 변경 후, 일부 플러그인 창이나 메뉴가 이전 언어로 표시되는 문제를 해결하기 위해, `Environment.GetFolderPath`로 `%LOCALAPPDATA%` 경로를 가져온 뒤 각 버전별 캐시 파일(`plugin_manager_cache.bin`, `svfx_plugin_cache.bin`)을 `File.Delete`를 통해 자동으로 삭제합니다. VEGAS를 다시 시작하면 이 파일들은 새로운 언어 설정에 맞춰 깨끗하게 재생성됩니다.

### 3. 언어 파일(`.cfg`) 복원 메커니즘
VEGAS Pro 14.0 이상 버전부터는 `ULangID` 레지스트리 값뿐만 아니라, 설치 폴더 내 `Language` 디렉터리에 해당 언어의 `.cfg` 파일이 존재해야 완벽하게 언어가 적용됩니다. 이 프로그램은 필수 `.cfg` 파일들을 **임베디드 리소스(Embedded Resources)**로 내장하여 이 문제를 해결합니다.

1.  **리소스 접근**: `System.Reflection.Assembly.GetManifestResourceStream()` 메서드를 사용하여 실행 파일(`.exe`) 내에 포함된 `.cfg` 파일의 데이터 스트림을 가져옵니다.
2.  **경로 탐색 및 복사**: 레지스트리에서 읽어온 VEGAS 설치 경로(`InstallPath`)를 기준으로 `Language` 또는 `language` 폴더의 존재를 확인합니다.
3.  **조건부 복사**: 만약 대상 폴더에 목표 언어의 `.cfg` 파일(예: `local_ko_KR.cfg`)이 존재하지 않을 경우에만, `Stream.CopyTo` 메서드와 `FileStream`을 사용하여 임베디드 리소스를 실제 파일로 복사합니다.

<br>

## 📋 버전별 지원 언어 (Supported Languages by Version)
이 프로그램은 다양한 VEGAS 제품군을 지원합니다. <br>

아래 표는 각 버전별 공식 지원 언어 목록이며, `Vegas Language Changer`를 통해 이 언어들 간의 전환이 가능합니다.

> **⚠️ 언어 변경 관련 중요 안내**
>
> 이 툴을 사용하여 언어를 변경하더라도, 일부 VEGAS 버전에서는 언어(특히 **폴란드어** 및 **러시아어**)가 완벽하게 적용되지 않을 수 있습니다.
> 
> 이는 VEGAS 프로그램 자체에 해당 언어 리소스 파일이 누락되었거나 불완전하기 때문에 발생하는 문제입니다.
>
> 따라서 이 툴은 언어 변경에 필요한 모든 작업을 정상적으로 수행하지만, 최종적인 표시 결과는 사용자의 VEGAS 설치 환경에 따라 달라질 수 있습니다.

### VEGAS Pro
| 버전 | 테스트 기준 빌드 (Tested Build) | 지원 언어 |
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
| **15.0** | 177, 384, 416 | German, English, Spanish, French, Japanese, Polish, Russian, Chinese (Simplified) <br> **참고:** 빌드 384부터 한국어 지원 |
| **16.0** | 248 | German, English, Spanish, French, Japanese, Korean, Polish, Portuguese, Russian, Chinese (Simplified) |
| **17.0** | 284 | German, English, Spanish, French, Japanese, Korean, Polish, Portuguese, Chinese (Simplified) |
| **18.0** | 284 | German, English, Spanish, French, Japanese, Korean, Polish, Portuguese, Chinese (Simplified) |
| **19.0** | 341 | German, English, Spanish, French, Japanese, Korean, Polish, Portuguese, Chinese (Simplified) |
| **20.0** | 411 | German, English, Spanish, French, Japanese, Korean, Polish, Portuguese, Chinese (Simplified) |
| **21.0** | 108 | German, English, Spanish, French, Japanese, Korean, Polish, Portuguese, Chinese (Simplified) |
| **22.0** | 194 | German, English, Spanish, French, Japanese, Korean, Polish, Portuguese, Chinese (Simplified) |
| **23.0** | 278 | German, English, Spanish, French, Japanese, Korean, Polish, Portuguese, Chinese (Simplified) |

### VEGAS Movie Studio
| 버전 | 테스트 기준 빌드 (Tested Build) | 지원 언어 |
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

## 📄 내장 `.cfg` 파일 생성 정책
일부 최신 VEGAS 버전은 특정 언어의 `.cfg` 파일을 공식적으로 제공하지 않습니다. 이 프로그램에 내장된 파일들은 사용자의 편의를 위해 다음과 같은 엄격하고 일관된 원칙에 따라 제작되었습니다.

### **공통 작업 원칙 (Standard Operating Procedure)**
1.  **기반(Base) 설정:** 생성하려는 **최신 버전의 영어(`en_US`) 설정 파일**을 **`뼈대(Base)`** 로 사용했습니다. 이를 통해 최신 버전의 모든 항목과 구조를 그대로 가져올 수 있었습니다.
2.  **참조(Reference) 설정:** **과거 버전의 대상 언어 설정 파일**을 **`참조(Reference)`** 파일로 사용하여, 해당 언어에 맞는 고유한 링크나 정보를 가져왔습니다.
3.  **선별적 병합(Selective Merge):** `뼈대`를 기반으로 `참조` 파일의 값을 덮어쓰되, 다음과 같은 **핵심 예외 규칙**을 엄격하게 적용했습니다.
    *   **`phash` 값은 반드시 `뼈대(Base)` 파일의 최신 버전 값을 유지**했습니다.
    *   `참조(Reference)` 파일에 없는 새로운 항목(예: `[GettingStarted]` 섹션)은 `뼈대(Base)`의 값을 그대로 보존했습니다.

### 각 버전별 언어 파일 생성 내역
| VEGAS 버전 | 대상 언어 파일 | 뼈대 (Base) | 참조 (Reference) | 주요 작업 내용 |
| :--- | :--- | :--- | :--- | :--- |
| **Pro 20** | `local_ja_JP.cfg` (일본어) | v20 English (US) | v19 Japanese (JP) | v20 구조에 v19 일본어 링크/정보 적용 및 v20 `phash` 유지 |
| | `local_zh_CN.cfg` (중국어) | v20 English (US) | v19 Chinese (CN) | v20 구조에 v19 중국어 링크/문서명 적용 및 v20 `phash` 유지 |
| **Pro 21** | `local_ja_JP.cfg` (일본어) | v21 English (US) | v19 Japanese (JP) | v21 구조에 v19 일본어 값 적용 및 v21 `phash` 유지 |
| | `local_zh_CN.cfg` (중국어) | v21 English (US) | v19 Chinese (CN) | v21 구조에 v19 중국어 값 적용 및 v21 `phash` 유지 |
| **Pro 22** | `local_ja_JP.cfg` (일본어) | v22 English (US) | v19 Japanese (JP) | v22의 신규 `GettingStarted` 섹션 보존하며 v19 일본어 값 적용 |
| | `local_ko_KR.cfg` (한국어) | v22 English (US) | **v21** Korean (KR) | v22 구조에 **최신 v21 한국어 정보**를 반영하고 v22 `phash` 유지 |
| | `local_zh_CN.cfg` (중국어) | v22 English (US) | v19 Chinese (CN) | v22 구조에 v19 중국어 값 적용 및 v22 `phash` 유지 |
| **Pro 23** | `local_ja_JP.cfg` (일본어) | v23 English (US) | v19 Japanese (JP) | v23의 신규 `VEGASAV1_` 항목 보존하며 v19 일본어 값 적용 |
| | `local_ko_KR.cfg` (한국어) | v23 English (US) | **v21** Korean (KR) | v23 구조에 **최신 v21 한국어 정보**를 반영하고 v23 `phash` 유지 |
| | `local_pt_BR.cfg` (포르투갈어) | v23 English (US) | v19 Portuguese (BR) | v23 구조에 v19 포르투갈어 값 적용 및 v23 `phash` 유지 |
| | `local_zh_CN.cfg` (중국어) | v23 English (US) | v19 Chinese (CN) | v23 구조에 v19 중국어 값 적용 및 v23 `phash` 유지 |

<br>

## 📜 라이선스 (License)
이 프로그램은 **[GNU General Public License v3.0](LICENSE)** 에 따라 라이선스가 부여됩니다.
