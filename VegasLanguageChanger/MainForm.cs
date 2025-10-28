// 네임스페이스 및 라이브러리 임포트
// ==============================================================================
// .NET 프레임워크 표준 라이브러리 (Standard Library)
// ==============================================================================
using Microsoft.Win32;          // Windows 레지스트리에 접근하기 위한 클래스 (VEGAS 설치 정보 조회 및 수정용)
using System;                   // 기본 클래스 및 기본 데이터 형식을 정의하는 핵심 네임스페이스
using System.Collections.Generic; // 제네릭 컬렉션(List, Dictionary 등)을 사용하기 위한 네임스페이스
using System.Data;                // ADO.NET 아키텍처를 구성하는 클래스를 포함 (여기서는 직접 사용되지 않음)
using System.Diagnostics;         // 디버깅 및 성능 추적, 프로세스 상호작용을 위한 클래스 (레지스트리 스캔 오류 로깅용)
using System.Globalization;     // 문화권별 정보(언어, 날짜/숫자 형식 등)를 정의하는 클래스 (버전 번호 파싱용)
using System.IO;                  // 파일 및 디렉토리 스트림에 대한 동기/비동기 읽기/쓰기를 허용하는 형식 (레지스트리 백업, 파일 복사용)
using System.Linq;                // LINQ(Language-Integrated Query)를 지원하는 클래스 및 인터페이스 (컬렉션 데이터 처리용)
using System.Security;            // 코드 접근 보안 시스템의 기본 구조를 제공 (레지스트리 접근 예외 처리용)
using System.Text;                // 문자 인코딩 및 문자열 조작을 위한 클래스 (결과 로그 생성용)
using System.Windows.Forms;       // Windows Forms 애플리케이션 생성을 위한 클래스 (GUI 구성용)

namespace VegasLanguageChanger
{
    /// <summary>
    /// VEGAS Pro 및 Movie Studio의 언어 설정을 변경하는 메인 애플리케이션 폼 클래스.
    /// 
    /// 이 클래스는 전체 애플리케이션의 핵심 기능을 담당하며, GUI 인터페이스,
    /// 설치된 VEGAS 버전 탐색, 레지스트리 수정, 언어 파일 복사, 플러그인 캐시 삭제 등을 통합하여 제공함.
    /// </summary>
    public partial class MainForm : Form
    {
        // ==============================================================================
        // 1. 데이터 및 상태 변수 (Data & State Variables)
        // ==============================================================================

        /// <summary>
        /// 시스템에서 탐지된 VEGAS 설치 버전 목록을 저장하는 리스트.
        /// 프로그램 시작 시 `FindInstalledVegasVersions` 메서드에 의해 채워짐.
        /// </summary>
        private List<InstalledVegasVersion> _installedVersions = new List<InstalledVegasVersion>();

        /// <summary>
        /// 애플리케이션 UI에 현재 적용된 언어의 코드 (예: "en", "ko").
        /// 시스템 언어 감지 또는 사용자 선택에 의해 설정됨.
        /// </summary>
        private string _currentLangCode = "en";

        /// <summary>
        /// 사용자가 언어 콤보박스에서 최종적으로 선택한 언어의 키 (예: "korean").
        /// 이 값은 '언어 변경' 버튼 클릭 시 어떤 언어로 변경할지 결정하는 데 사용됨.
        /// </summary>
        private string _chosenLanguageKey = null;

        /// <summary>
        /// MainForm 클래스의 생성자.
        /// Windows Forms 디자이너에서 생성된 컴포넌트들을 초기화.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 폼이 처음 로드될 때 호출되는 이벤트 핸들러.
        /// 애플리케이션 시작 시 필요한 모든 초기화 작업을 순차적으로 수행.
        /// </summary>
        /// <param name="sender">이벤트를 발생시킨 객체.</param>
        /// <param name="e">이벤트 관련 데이터를 포함하는 객체.</param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            // [디버깅 전용] .exe 파일에 포함된 실제 리소스 이름 목록을 텍스트 파일로 출력.
            // 리소스 경로 문제 발생 시 이 코드의 주석을 해제하여 정확한 경로를 확인할 수 있음.
            /*
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                string[] resourceNames = assembly.GetManifestResourceNames();
                string allNames = string.Join("\n", resourceNames);

                System.IO.File.WriteAllText("ActualResourceNames.txt", allNames);

                MessageBox.Show("프로그램 폴더에 ActualResourceNames.txt 파일이 생성되었습니다. 내용을 확인해주세요.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("리소스 이름 확인 중 오류 발생: " + ex.Message);
            }
            */

            DetectAndSetSystemLanguage(); // 1. 시스템 언어를 감지하고 UI 텍스트를 설정.
            FindInstalledVegasVersions(); // 2. PC에 설치된 모든 VEGAS 버전을 탐색.
            PopulateAvailableList();      // 3. 탐색된 버전 목록을 UI 리스트 박스에 채움.
            UpdateLanguageComboBox();     // 4. 선택된 버전에 따라 언어 콤보박스를 업데이트.
            this.CenterToScreen();        // 5. 폼을 화면 중앙에 위치시킴.
        }


        // ==============================================================================
        // 2. 핵심 로직: 버전 탐색 및 언어 변경 (Core Logic)
        // ==============================================================================

        /// <summary>
        /// Windows 레지스트리를 스캔하여 지원되는 VEGAS 버전들의 설치 정보를 찾음.
        /// 
        /// 64비트 및 32비트 레지스트리 뷰를 모두 탐색하여 `VegasData`에 정의된
        /// 가능한 모든 경로에서 VEGAS의 언어 설정 키(`ULangID`)를 찾음.
        /// 성공적으로 찾은 버전은 설치 경로 정보와 함께 `_installedVersions` 리스트에 추가됨.
        /// </summary>
        private void FindInstalledVegasVersions()
        {
            _installedVersions.Clear(); // 이전 탐색 결과를 초기화.
            var registryViews = new[] { RegistryView.Registry64, RegistryView.Registry32 }; // 64비트와 32비트 레지스트리를 모두 탐색.

            // 지원되는 모든 VEGAS 버전에 대해 반복.
            foreach (var version in VegasData.AllSupportedVersions)
            {
                bool versionFound = false; // 해당 버전이 발견되었는지 추적하는 플래그.
                // 각 버전은 여러 개의 가능한 레지스트리 경로를 가질 수 있음 (예: Sony -> Magix -> VEGAS).
                foreach (var basePath in version.RegistryPaths)
                {
                    if (versionFound) break; // 이미 찾았다면 더 이상 탐색할 필요 없음.
                    foreach (var view in registryViews)
                    {
                        try
                        {
                            // 최종 언어 설정 레지스트리 키의 전체 경로를 구성.
                            string fullKeyPath = Path.Combine(basePath, version.RegistryKey);
                            fullKeyPath = Path.Combine(fullKeyPath, "Lang").Replace("/", "\\");

                            using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
                            using (var langKey = baseKey.OpenSubKey(fullKeyPath))
                            {
                                // 언어 설정 키가 존재하면, 해당 VEGAS 버전이 설치된 것으로 간주.
                                if (langKey != null)
                                {
                                    string installPath = null;
                                    // 부모 키로 이동하여 설치 경로("InstallPath") 값을 조회.
                                    string parentKeyPath = Path.Combine(basePath, version.RegistryKey).Replace("/", "\\");
                                    using (var parentKey = baseKey.OpenSubKey(parentKeyPath))
                                    {
                                        if (parentKey != null)
                                        {
                                            installPath = parentKey.GetValue("InstallPath") as string;
                                        }
                                    }

                                    // 탐색된 버전 정보를 객체로 생성.
                                    var installedVersion = new InstalledVegasVersion(version)
                                    {
                                        FullPath = fullKeyPath,
                                        InstallPath = installPath
                                    };

                                    // 중복 추가를 방지하며 리스트에 추가.
                                    if (!_installedVersions.Any(iv => iv.Name == installedVersion.Name))
                                    {
                                        _installedVersions.Add(installedVersion);
                                    }
                                    versionFound = true;
                                    break; // 이 버전에 대한 탐색 완료.
                                }
                            }
                        }
                        // 권한 문제로 레지스트리 접근에 실패하는 경우는 정상적인 상황일 수 있으므로 무시.
                        catch (SecurityException) { }
                        // 그 외 예상치 못한 오류는 디버그 출력으로 로깅.
                        catch (Exception ex) { Debug.WriteLine("Error scanning registry: " + ex.Message); }
                    }
                }
            }
        }

        /// <summary>
        /// '언어 변경' 버튼 클릭 시 실행되는 메인 이벤트 핸들러.
        /// 
        /// 사용자 입력 유효성 검사, 실행 확인, 레지스트리 백업, 레지스트리 수정,
        /// 언어 파일 복사, 플러그인 캐시 삭제 등 언어 변경에 필요한 모든 작업을 순차적으로 오케스트레이션함.
        /// </summary>
        /// <param name="sender">이벤트를 발생시킨 객체.</param>
        /// <param name="e">이벤트 관련 데이터를 포함하는 객체.</param>
        private void btnChangeLanguage_Click(object sender, EventArgs e)
        {
            // --- 1. 사전 조건 및 사용자 입력 유효성 검사 ---
            var selectedVersionsToChange = lstSelected.Items.Cast<InstalledVegasVersion>().ToList();
            if (selectedVersionsToChange.Count == 0) // 변경할 버전을 선택했는지 확인.
            {
                MessageBox.Show(GetText("error_no_versions_selected"), GetText("error_title"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(_chosenLanguageKey) || cmbLanguage.SelectedIndex < 0 || !(cmbLanguage.SelectedItem is LanguageItem)) // 변경할 언어를 선택했는지 확인.
            {
                MessageBox.Show(GetText("error_no_valid_lang"), GetText("error_title"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 선택된 언어 키로부터 레지스트리에 기록할 숫자 ID를 조회.
            var langIdEntry = VegasData.LangIdToKey.FirstOrDefault(kvp => kvp.Value == _chosenLanguageKey);
            int targetLangId = langIdEntry.Key;
            string targetLangName = GetText(_chosenLanguageKey);

            // Vegas Pro 15.0의 한국어 지원 버전에 대한 특수 경고 처리.
            if (selectedVersionsToChange.Any(v => v.Name == "Vegas Pro 15.0") && targetLangId == 1042)
            {
                if (MessageBox.Show(GetText("vegas_pro_15_korean_warning"), GetText("confirm_title"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                {
                    return; // 사용자가 '아니요'를 선택하면 작업을 중단.
                }
            }

            // --- 2. 최종 실행 확인 ---
            string confirmMsg = GetText("confirm_message")
                                .Replace("{num_versions}", selectedVersionsToChange.Count.ToString())
                                .Replace("{lang_name}", targetLangName);
            if (MessageBox.Show(confirmMsg, GetText("confirm_title"), MessageBoxButtons.OKCancel, MessageBoxIcon.Information) != DialogResult.OK)
            {
                return; // 사용자가 '취소'를 선택하면 작업을 중단.
            }

            // --- 3. 핵심 작업 순차 실행 ---
            var results = new StringBuilder(); // 모든 작업 결과를 누적할 StringBuilder.

            // 3-1. 레지스트리 백업.
            string backupPath = BackupRegistry(selectedVersionsToChange, results);
            if (string.IsNullOrEmpty(backupPath)) // 백업이 실패하면 안전을 위해 전체 작업을 중단.
            {
                MessageBox.Show(GetText("backup_fail_message"), GetText("backup_fail_title"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            results.AppendLine(GetText("task_complete_message").Replace("{backup_filename}", Path.GetFileName(backupPath)));
            results.AppendLine();

            // 3-2. 레지스트리 수정.
            List<InstalledVegasVersion> succeededVersions = ModifyRegistry(selectedVersionsToChange, targetLangId, results);

            // 3-3. 언어 파일(.cfg) 복사 (성공한 버전에 대해서만 수행).
            CopyLanguageFiles(succeededVersions, _chosenLanguageKey, results);

            // 3-4. 플러그인 캐시 삭제 (성공한 버전에 대해서만 수행).
            ClearPluginCache(succeededVersions, results);

            // --- 4. 최종 결과 표시 ---
            MessageBox.Show(results.ToString().Trim(), GetText("task_complete_title"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 선택된 VEGAS 버전들의 현재 언어 설정(ULangID)을 .reg 파일로 백업.
        /// 
        /// 레지스트리 변경 작업 전, 만일의 사태에 대비하여 현재 설정 값을 바탕화면에 .reg 파일로 저장함.
        /// 사용자가 이 파일을 실행하면 원래 언어 설정으로 쉽게 복원할 수 있음.
        /// </summary>
        /// <param name="versions">백업할 VEGAS 버전 목록.</param>
        /// <param name="log">작업 결과를 기록할 StringBuilder 객체.</param>
        /// <returns>성공 시 백업 파일의 전체 경로, 실패 시 null.</returns>
        private string BackupRegistry(List<InstalledVegasVersion> versions, StringBuilder log)
        {
            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupFilePath = Path.Combine(desktopPath, "Vegas_RegBackup_" + timestamp + ".reg");

                var regContent = new StringBuilder();
                regContent.AppendLine("Windows Registry Editor Version 5.00");
                regContent.AppendLine();

                var registryViews = new[] { RegistryView.Registry64, RegistryView.Registry32 };
                foreach (var version in versions)
                {
                    foreach (var view in registryViews)
                    {
                        try
                        {
                            // 레지스트리를 열어 현재 'ULangID' 값을 읽어옴.
                            using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
                            using (var hkey = baseKey.OpenSubKey(version.FullPath))
                            {
                                if (hkey != null)
                                {
                                    object value = hkey.GetValue("ULangID");
                                    if (value is int)
                                    {
                                        int langId = (int)value;
                                        // .reg 파일 형식에 맞게 문자열을 구성.
                                        regContent.AppendLine("[" + "HKEY_LOCAL_MACHINE\\" + version.FullPath + "]");
                                        regContent.AppendLine("\"ULangID\"=dword:" + langId.ToString("x8"));
                                        regContent.AppendLine();
                                        break; // 해당 버전에 대한 백업 완료.
                                    }
                                }
                            }
                        }
                        catch { /* 키가 없는 경우 등은 무시하고 다음 뷰를 시도 */ }
                    }
                }

                // 생성된 .reg 파일 내용을 유니코드 인코딩으로 저장 (다국어 레지스트리 호환성).
                File.WriteAllText(backupFilePath, regContent.ToString(), Encoding.Unicode);
                return backupFilePath;
            }
            catch (Exception ex)
            {
                log.AppendLine("Backup failed: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 선택된 VEGAS 버전들의 레지스트리에서 'ULangID' 값을 목표 언어 ID로 변경.
        /// </summary>
        /// <param name="versions">언어를 변경할 VEGAS 버전 목록.</param>
        /// <param name="langId">설정할 목표 언어의 숫자 ID.</param>
        /// <param name="log">작업 결과를 기록할 StringBuilder 객체.</param>
        /// <returns>성공적으로 레지스트리가 수정된 VEGAS 버전 목록.</returns>
        private List<InstalledVegasVersion> ModifyRegistry(List<InstalledVegasVersion> versions, int langId, StringBuilder log)
        {
            var successList = new List<string>();
            var failureList = new List<string>();
            var succeededVersions = new List<InstalledVegasVersion>();

            var registryViews = new[] { RegistryView.Registry64, RegistryView.Registry32 };
            foreach (var version in versions)
            {
                bool modified = false;
                foreach (var view in registryViews)
                {
                    try
                    {
                        // 쓰기 권한(true)으로 레지스트리 키를 열어 값을 설정.
                        using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
                        using (var hkey = baseKey.OpenSubKey(version.FullPath, true))
                        {
                            if (hkey != null)
                            {
                                hkey.SetValue("ULangID", langId, RegistryValueKind.DWord);
                                successList.Add(version.Name);
                                succeededVersions.Add(version);
                                modified = true;
                                break; // 수정 성공.
                            }
                        }
                    }
                    catch (Exception ex) // 권한 부족 등의 예외 처리.
                    {
                        failureList.Add(version.Name + ": " + ex.Message);
                        modified = true; // 오류가 발생했더라도 시도는 했으므로 더 이상 반복하지 않음.
                        break;
                    }
                }
                if (!modified) // 모든 뷰에서 키를 찾지 못한 경우.
                {
                    failureList.Add(version.Name + ": " + GetText("reg_path_not_found"));
                }
            }

            // 성공/실패 목록을 로그에 추가.
            if (successList.Any())
                log.AppendLine(GetText("success") + "\n" + string.Join("\n", successList.ToArray()) + "\n");
            if (failureList.Any())
                log.AppendLine(GetText("failure") + "\n" + string.Join("\n", failureList.ToArray()) + "\n");

            return succeededVersions;
        }

        /// <summary>
        /// 특정 언어(주로 한국어)에 필요한 추가 언어 파일(.cfg)을 VEGAS 설치 폴더에 복사.
        /// 
        /// VEGAS 14.0 이상 버전에서 한국어 등을 제대로 표시하기 위해 필요한 `local_ko_KR.cfg`와 같은
        /// 파일을 프로그램에 내장된 리소스로부터 VEGAS의 language 폴더로 복사함.
        /// 파일이 이미 존재하면 복사를 건너뜀.
        /// </summary>
        /// <param name="versions">언어 파일을 복사할 대상 VEGAS 버전 목록.</param>
        /// <param name="langKey">복사할 언어의 키 (예: "korean").</param>
        /// <param name="log">작업 결과를 기록할 StringBuilder 객체.</param>
        private void CopyLanguageFiles(List<InstalledVegasVersion> versions, string langKey, StringBuilder log)
        {
            var successes = new List<string>();
            var failures = new List<string>();

            // 선택된 언어에 해당하는 .cfg 파일 접미사(예: ko_KR)를 조회.
            string cfgSuffix;
            if (!VegasData.LangKeyToCfgSuffix.TryGetValue(langKey, out cfgSuffix)) return; // .cfg 파일이 필요 없는 언어는 종료.
            string cfgFilename = "local_" + cfgSuffix + ".cfg";

            var assembly = System.Reflection.Assembly.GetExecutingAssembly(); // 내장 리소스에 접근하기 위함.

            // .cfg 파일 복사를 지원하는 버전만 필터링하여 처리.
            foreach (var version in versions.Where(v => v.CfgCopySupported))
            {
                if (string.IsNullOrEmpty(version.InstallPath)) // 설치 경로를 모르면 복사 불가.
                {
                    failures.Add(GetText("cfg_copy_failed_no_dest").Replace("{version_name}", version.Name));
                    continue;
                }

                // VEGAS 버전마다 'language' 또는 'Language' 폴더 이름을 사용하므로, 실제 존재하는 폴더 이름을 확인.
                string langFolderNameOnDisk = Directory.Exists(Path.Combine(version.InstallPath, "language")) ? "language" : "Language";
                string destFolder = Path.Combine(version.InstallPath, langFolderNameOnDisk);
                string destFile = Path.Combine(destFolder, cfgFilename);

                if (!File.Exists(destFile)) // 파일이 아직 존재하지 않을 때만 복사 수행.
                {
                    // .exe에 내장된 리소스의 전체 경로를 구성. (예: VegasLanguageChanger.VLC_Source.VEGAS_Pro_23_0.language.local_ko_KR.cfg)
                    string folderNamePart = ("VEGAS Pro " + version.RegistryKey).Replace(" ", "_").Replace(".", "._");

                    string[] possibleLangFolderNames = { "Language", "language" }; // 리소스 경로도 대소문자를 구분하므로 둘 다 시도.
                    Stream resourceStream = null;
                    string finalResourceName = "";

                    foreach (var nameCase in possibleLangFolderNames)
                    {
                        string resourceName = string.Format("VegasLanguageChanger.VLC_Source.{0}.{1}.{2}",
                                                            folderNamePart,
                                                            nameCase,
                                                            cfgFilename);

                        resourceStream = assembly.GetManifestResourceStream(resourceName);
                        if (resourceStream != null)
                        {
                            finalResourceName = resourceName;
                            break; // 리소스를 찾으면 반복 중단.
                        }
                    }

                    try
                    {
                        if (resourceStream == null) // 내장된 리소스를 찾지 못한 경우.
                        {
                            failures.Add(GetText("cfg_copy_failed_no_source").Replace("{version_name}", version.Name));
                            continue;
                        }

                        Directory.CreateDirectory(destFolder); // 대상 폴더가 없으면 생성.

                        // 리소스 스트림을 파일로 저장.
                        using (FileStream fileStream = new FileStream(destFile, FileMode.Create))
                        {
                            resourceStream.CopyTo(fileStream);
                        }

                        successes.Add(GetText("cfg_copied").Replace("{version_name}", version.Name).Replace("{filename}", cfgFilename));
                    }
                    catch (Exception ex) // 권한 부족 등의 파일 I/O 예외 처리.
                    {
                        failures.Add(GetText("cfg_copy_failed_permission").Replace("{version_name}", version.Name).Replace("{error}", ex.Message));
                    }
                    finally
                    {
                        // 리소스 스트림을 안전하게 닫음.
                        if (resourceStream != null)
                        {
                            resourceStream.Dispose();
                        }
                    }
                }
            }

            // 성공/실패 결과를 로그에 추가.
            if (successes.Any() || failures.Any())
            {
                var allMessages = successes.Concat(failures).ToArray();
                log.AppendLine(GetText("cfg_copy_results") + "\n" + string.Join("\n", allMessages) + "\n");
            }
        }

        /// <summary>
        /// VEGAS의 플러그인 캐시 파일을 삭제하여 언어 변경 후 발생할 수 있는 UI 깨짐 현상을 방지.
        /// 
        /// VEGAS는 플러그인 정보를 캐시 파일에 저장하는데, 언어 변경 시 이 캐시가
        /// 이전 언어 정보와 충돌하여 문제가 발생할 수 있음. 이 함수는 해당 캐시 파일들을
        /// `%localappdata%` 폴더에서 찾아 삭제하며, VEGAS가 다음 실행 시 캐시를 자동으로 재생성하도록 유도함.
        /// </summary>
        /// <param name="versions">캐시를 삭제할 대상 VEGAS 버전 목록.</param>
        /// <param name="log">작업 결과를 기록할 StringBuilder 객체.</param>
        private void ClearPluginCache(List<InstalledVegasVersion> versions, StringBuilder log)
        {
            var successes = new List<string>();
            var failures = new List<string>();
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrEmpty(localAppData)) return; // AppData 경로를 찾을 수 없으면 종료.

            foreach (var version in versions)
            {
                // 제품 이름(Pro/Studio)에 따라 캐시 폴더 경로를 구성.
                string productFolder = version.Name.Contains("Pro") ? "VEGAS Pro" :
                                       version.Name.Contains("Studio") ? "Movie Studio Platinum" : null;
                if (productFolder == null) continue; // 지원되지 않는 제품군은 건너뜀.

                string cachePath = Path.Combine(localAppData, productFolder);
                cachePath = Path.Combine(cachePath, version.RegistryKey);

                bool deletedOnce = false;
                bool errorOccurred = false;

                // 삭제할 대상 캐시 파일 목록.
                foreach (string fileName in new[] { "plugin_manager_cache.bin", "svfx_plugin_cache.bin" })
                {
                    string filePath = Path.Combine(cachePath, fileName);
                    if (File.Exists(filePath))
                    {
                        try
                        {
                            File.Delete(filePath);
                            deletedOnce = true;
                        }
                        catch (Exception ex) // 권한 부족 등의 파일 I/O 예외 처리.
                        {
                            failures.Add(GetText("cache_clear_failed").Replace("{version_name}", version.Name).Replace("{error}", ex.Message));
                            errorOccurred = true;
                            break; // 한 버전에서 오류 발생 시 추가 파일 삭제 시도를 중단.
                        }
                    }
                }

                if (deletedOnce && !errorOccurred)
                {
                    successes.Add(GetText("cache_cleared").Replace("{version_name}", version.Name));
                }
            }

            // 성공/실패 결과를 로그에 추가.
            if (successes.Any() || failures.Any())
            {
                var allMessages = successes.Concat(failures).ToArray();
                log.AppendLine(GetText("cache_clear_results") + "\n" + string.Join("\n", allMessages) + "\n");
            }
        }


        // ==============================================================================
        // 3. UI 컨트롤 및 이벤트 핸들러 (UI Controls & Event Handlers)
        // ==============================================================================

        /// <summary>
        /// 탐색된 VEGAS 버전 목록을 UI의 '사용 가능 버전' 리스트 박스에 채움.
        /// 
        /// `_installedVersions` 리스트에 저장된 데이터를 UI에 표시하고, 만약 설치된 버전이
        /// 하나도 없다면 사용자에게 안내 메시지를 보여주고 관련 컨트롤들을 비활성화함.
        /// </summary>
        private void PopulateAvailableList()
        {
            lstAvailable.Items.Clear();
            lstSelected.Items.Clear();

            if (_installedVersions.Count == 0) // 설치된 버전이 없을 경우.
            {
                lblInfo.Text = GetText("no_versions_found");
                grpAvailable.Enabled = false;
                grpSelected.Enabled = false;
                btnChangeLanguage.Enabled = false;
                cmbLanguage.Enabled = false;
                return;
            }

            // ListBox에 InstalledVegasVersion 객체를 직접 추가하고,
            // 화면에는 Name 속성만 표시되도록 설정.
            lstAvailable.DisplayMember = "Name";
            lstSelected.DisplayMember = "Name";

            // 버전 번호를 기준으로 오름차순 정렬하여 목록에 추가.
            foreach (var version in _installedVersions.OrderBy(v => float.Parse(v.RegistryKey, CultureInfo.InvariantCulture)))
            {
                lstAvailable.Items.Add(version);
            }
        }

        /// <summary>
        /// '>' 버튼 클릭 시, '사용 가능' 목록에서 선택된 항목을 '변경할' 목록으로 이동.
        /// </summary>
        private void btnMoveRight_Click(object sender, EventArgs e)
        {
            MoveSelectedItems(lstAvailable, lstSelected);
        }

        /// <summary>
        /// '>>' 버튼 클릭 시, '사용 가능' 목록의 모든 항목을 '변경할' 목록으로 이동.
        /// </summary>
        private void btnMoveAllRight_Click(object sender, EventArgs e)
        {
            MoveAllItems(lstAvailable, lstSelected);
        }

        /// <summary>
        /// '<' 버튼 클릭 시, '변경할' 목록에서 선택된 항목을 '사용 가능' 목록으로 이동.
        /// </summary>
        private void btnMoveLeft_Click(object sender, EventArgs e)
        {
            MoveSelectedItems(lstSelected, lstAvailable);
        }

        /// <summary>
        /// '<<' 버튼 클릭 시, '변경할' 목록의 모든 항목을 '사용 가능' 목록으로 이동.
        /// </summary>
        private void btnMoveAllLeft_Click(object sender, EventArgs e)
        {
            MoveAllItems(lstSelected, lstAvailable);
        }

        /// <summary>
        /// '사용 가능' 목록의 항목을 더블 클릭했을 때 '변경할' 목록으로 이동.
        /// </summary>
        private void lstAvailable_DoubleClick(object sender, EventArgs e)
        {
            MoveSelectedItems(lstAvailable, lstSelected);
        }

        /// <summary>
        /// '변경할' 목록의 항목을 더블 클릭했을 때 '사용 가능' 목록으로 이동.
        /// </summary>
        private void lstSelected_DoubleClick(object sender, EventArgs e)
        {
            MoveSelectedItems(lstSelected, lstAvailable);
        }

        /// <summary>
        /// 사용자가 언어 콤보박스에서 항목을 선택하고 선택이 확정되었을 때 호출.
        /// 선택된 언어의 키 값을 `_chosenLanguageKey` 변수에 저장.
        /// </summary>
        private void cmbLanguage_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (cmbLanguage.SelectedItem is LanguageItem)
            {
                _chosenLanguageKey = ((LanguageItem)cmbLanguage.SelectedItem).Key;
            }
        }

        /// <summary>
        /// '변경할 버전' 목록이 변경될 때마다 언어 콤보박스의 내용을 동적으로 업데이트.
        /// 
        /// '변경할 버전'으로 선택된 모든 VEGAS 버전들이 공통적으로 지원하는 언어만
        /// 콤보박스에 표시함. 공통 언어가 없으면 사용자에게 안내 메시지를 보여줌.
        /// </summary>
        private void UpdateLanguageComboBox()
        {
            var selectedVersions = lstSelected.Items.Cast<InstalledVegasVersion>().ToList();
            cmbLanguage.DataSource = null; // 데이터 소스를 먼저 초기화해야 Items.Clear()가 제대로 동작.
            cmbLanguage.Items.Clear();

            if (selectedVersions.Count == 0) // 선택된 버전이 없으면 콤보박스를 비활성화.
            {
                cmbLanguage.Items.Add(new LanguageItem { Key = "", Name = GetText("select_version") });
                cmbLanguage.SelectedIndex = 0;
                cmbLanguage.Enabled = false;
                btnChangeLanguage.Enabled = false;
                return;
            }

            // 첫 번째 버전의 지원 언어 목록으로 시작하여, 나머지 버전들과의 교집합을 구함.
            var commonLangIds = new HashSet<int>(selectedVersions.First().SupportedLangIds);
            foreach (var version in selectedVersions.Skip(1))
            {
                commonLangIds.IntersectWith(version.SupportedLangIds);
            }

            // 계산된 공통 언어 ID들을 UI에 표시할 LanguageItem 객체 리스트로 변환.
            var availableLanguages = commonLangIds
                .Where(id => VegasData.LangIdToKey.ContainsKey(id))
                .Select(id => new LanguageItem { Key = VegasData.LangIdToKey[id], Name = GetText(VegasData.LangIdToKey[id]) })
                .OrderBy(lang => lang.Name)
                .ToList();

            if (availableLanguages.Count == 0) // 공통 지원 언어가 없는 경우.
            {
                cmbLanguage.Items.Add(new LanguageItem { Key = "", Name = GetText("no_common_lang") });
                cmbLanguage.SelectedIndex = 0;
                cmbLanguage.Enabled = false;
                btnChangeLanguage.Enabled = false;
                return;
            }

            // 콤보박스에 데이터 소스를 바인딩하고, 이전에 선택했던 언어가 있다면 유지.
            cmbLanguage.DataSource = availableLanguages;
            cmbLanguage.DisplayMember = "Name";
            cmbLanguage.ValueMember = "Key";
            var previouslySelected = availableLanguages.FirstOrDefault(l => l.Key == _chosenLanguageKey);
            cmbLanguage.SelectedItem = previouslySelected ?? availableLanguages.First();
            if (cmbLanguage.SelectedItem is LanguageItem) // 현재 선택된 언어 키를 갱신.
            {
                _chosenLanguageKey = ((LanguageItem)cmbLanguage.SelectedItem).Key;
            }
            cmbLanguage.Enabled = true;
            btnChangeLanguage.Enabled = true;
        }


        // ==============================================================================
        // 4. 다국어 및 UI 헬퍼 (Localization & UI Helpers)
        // ==============================================================================

        /// <summary>
        /// 사용자의 시스템 UI 언어를 감지하여 프로그램의 기본 언어를 설정.
        /// 지원되는 언어일 경우 해당 언어로, 아니면 영어(en)를 기본값으로 사용.
        /// </summary>
        private void DetectAndSetSystemLanguage()
        {
            string cultureName = CultureInfo.CurrentUICulture.Name; // 예: "ko-KR", "en-US"
            string langCode = cultureName.Split('-')[0]; // 예: "ko", "en"

            if (VegasData.Texts.ContainsKey(langCode))
            {
                _currentLangCode = langCode;
            }
            else
            {
                _currentLangCode = "en"; // 지원하지 않는 언어일 경우 영어로 대체.
            }
            ApplyLocalization(); // 결정된 언어로 모든 UI 텍스트를 업데이트.
        }

        /// <summary>
        /// 사용자가 메뉴에서 선택한 언어로 프로그램 UI를 변경.
        /// </summary>
        /// <param name="langCode">변경할 언어의 두 글자 코드 (예: "ko").</param>
        private void SetLanguage(string langCode)
        {
            if (VegasData.Texts.ContainsKey(langCode))
            {
                _currentLangCode = langCode;
                ApplyLocalization();      // UI 텍스트를 새로운 언어로 업데이트.
                UpdateLanguageComboBox(); // 언어 콤보박스의 언어 이름들도 새로운 언어로 업데이트.
            }
        }

        /// <summary>
        /// 현재 설정된 언어에 맞는 텍스트를 반환하는 중앙 헬퍼 함수.
        /// 현재 언어에 해당 키가 없으면, 영어(en)에서 찾아보고, 거기도 없으면 "MISSING:키"를 반환.
        /// </summary>
        /// <param name="key">가져올 텍스트의 키.</param>
        /// <returns>다국어 처리된 텍스트 문자열.</returns>
        private string GetText(string key)
        {
            Dictionary<string, string> langDict;
            if (VegasData.Texts.TryGetValue(_currentLangCode, out langDict) && langDict.ContainsKey(key))
            {
                return langDict[key];
            }
            // 현재 언어에 번역이 없는 경우, 영어로 대체(fallback).
            if (VegasData.Texts["en"].ContainsKey(key))
            {
                return VegasData.Texts["en"][key];
            }
            return "MISSING:" + key; // 모든 언어에 키가 없는 경우, 디버깅을 위해 키 이름을 반환.
        }

        /// <summary>
        /// 현재 설정된 언어(`_currentLangCode`)를 기반으로 모든 UI 컨트롤의 텍스트를 업데이트.
        /// </summary>
        private void ApplyLocalization()
        {
            // 폼 제목, 라벨, 그룹박스, 버튼 등 정적인 텍스트를 업데이트.
            this.Text = GetText("title");
            lblInfo.Text = GetText("info_text");
            grpAvailable.Text = GetText("available_versions");
            grpSelected.Text = GetText("versions_to_change");
            btnChangeLanguage.Text = GetText("change_lang_button");

            // 메뉴 아이템은 동적으로 재생성하여 언어를 적용.
            menuStripMain.Items.Clear();
            var programLangMenu = new ToolStripMenuItem(GetText("program_language"));

            var langOptions = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("german", "de"), new KeyValuePair<string, string>("english", "en"),
                new KeyValuePair<string, string>("spanish", "es"), new KeyValuePair<string, string>("french", "fr"),
                new KeyValuePair<string, string>("japanese", "ja"), new KeyValuePair<string, string>("korean", "ko"),
                new KeyValuePair<string, string>("polish", "pl"), new KeyValuePair<string, string>("portuguese", "pt"),
                new KeyValuePair<string, string>("russian", "ru"), new KeyValuePair<string, string>("chinese_simplified", "zh")
            };

            foreach (var lang in langOptions)
            {
                var item = new ToolStripMenuItem(GetText(lang.Key));
                item.Tag = lang.Value; // Tag 속성에 언어 코드를 저장.
                item.Click += (s, e) => SetLanguage(((ToolStripMenuItem)s).Tag.ToString()); // 클릭 시 SetLanguage 호출.
                programLangMenu.DropDownItems.Add(item);
            }

            var aboutMenu = new ToolStripMenuItem(GetText("about_menu"));
            aboutMenu.Click += (s, e) => ShowAboutDialog();

            menuStripMain.Items.Add(programLangMenu);
            menuStripMain.Items.Add(aboutMenu);
        }

        /// <summary>
        /// '정보' 메뉴 클릭 시 프로그램 정보를 담은 메시지 박스를 표시.
        /// </summary>
        private void ShowAboutDialog()
        {
            string message = GetText("title") + "\n" +
                             GetText("about_label_version") + ": " + VegasData.AboutInfo["version"] + " (" + GetText("about_label_updated") + ": " + VegasData.AboutInfo["updated"] + ")\n\n" +
                             GetText("about_label_developer") + ": " + VegasData.AboutInfo["developer"] + "\n" +
                             GetText("about_label_website") + ": " + VegasData.AboutInfo["website"] + "\n\n" +
                             GetText("about_label_license") + ": " + VegasData.AboutInfo["license"] + "\n\n" +
                             GetText("about_label_special_thanks") + "\n" +
                             GetText("about_special_thanks_content");
            MessageBox.Show(message, GetText("about_menu"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 소스 리스트 박스에서 선택된 항목들을 대상 리스트 박스로 이동시키는 헬퍼 함수.
        /// </summary>
        /// <param name="source">항목을 가져올 소스 ListBox.</param>
        /// <param name="destination">항목을 추가할 대상 ListBox.</param>
        private void MoveSelectedItems(ListBox source, ListBox destination)
        {
            if (source.SelectedItems.Count > 0)
            {
                // 반복 중 컬렉션이 변경되므로, 이동할 항목을 미리 별도 리스트에 복사.
                var itemsToMove = source.SelectedItems.Cast<InstalledVegasVersion>().ToList();
                foreach (var item in itemsToMove)
                {
                    source.Items.Remove(item);
                    destination.Items.Add(item);
                }
                SortListBox(source);
                SortListBox(destination);
                UpdateLanguageComboBox(); // 목록이 변경되었으므로 언어 콤보박스 갱신.
            }
        }

        /// <summary>
        /// 소스 리스트 박스의 모든 항목을 대상 리스트 박스로 이동시키는 헬퍼 함수.
        /// </summary>
        /// <param name="source">항목을 가져올 소스 ListBox.</param>
        /// <param name="destination">항목을 추가할 대상 ListBox.</param>
        private void MoveAllItems(ListBox source, ListBox destination)
        {
            if (source.Items.Count > 0)
            {
                var itemsToMove = source.Items.Cast<InstalledVegasVersion>().ToList();
                source.Items.Clear();
                foreach (var item in itemsToMove)
                {
                    destination.Items.Add(item);
                }
                SortListBox(destination);
                UpdateLanguageComboBox();
            }
        }

        /// <summary>
        /// 리스트 박스의 항목들을 VEGAS 버전 번호 순으로 정렬하는 헬퍼 함수.
        /// </summary>
        /// <param name="listBox">정렬할 ListBox.</param>
        private void SortListBox(ListBox listBox)
        {
            var sortedItems = listBox.Items.Cast<InstalledVegasVersion>()
                                     .OrderBy(v => float.Parse(v.RegistryKey, CultureInfo.InvariantCulture)).ToList();
            listBox.Items.Clear();
            foreach (var item in sortedItems)
            {
                listBox.Items.Add(item);
            }
        }

        /// <summary>
        /// 언어 콤보박스에 데이터 바인딩하기 위한 내부 데이터 클래스.
        /// </summary>
        private class LanguageItem
        {
            public string Key { get; set; }  // 내부적으로 사용할 언어 키 (예: "korean")
            public string Name { get; set; } // UI에 표시될 언어 이름 (예: "한국어")

            /// <summary>
            /// 콤보박스가 이 객체를 화면에 표시할 때 호출하는 메서드를 오버라이드.
            /// Name 속성 값을 반환하여 사용자에게 친숙한 언어 이름이 보이도록 함.
            /// </summary>
            /// <returns>UI에 표시될 언어 이름.</returns>
            public override string ToString()
            {
                return this.Name;
            }
        }
    }

    // ==============================================================================
    // 5. 정적 데이터 클래스 (Static Data Classes)
    // ==============================================================================

    /// <summary>
    /// 시스템에 실제 설치된 VEGAS 버전의 동적 정보를 담는 클래스.
    /// `VegasVersion`의 정적 데이터를 상속받고, 탐색된 레지스트리 경로와 설치 경로를 추가로 가짐.
    /// </summary>
    public class InstalledVegasVersion : VegasVersion
    {
        public string FullPath { get; set; }    // 언어 설정(ULangID)이 있는 레지스트리의 전체 경로
        public string InstallPath { get; set; } // VEGAS가 설치된 실제 디스크 경로

        /// <summary>
        /// `VegasVersion` 객체를 기반으로 `InstalledVegasVersion` 인스턴스를 생성.
        /// </summary>
        /// <param name="baseVersion">복사할 정적 데이터가 담긴 기본 `VegasVersion` 객체.</param>
        public InstalledVegasVersion(VegasVersion baseVersion)
        {
            this.Name = baseVersion.Name;
            this.RegistryPaths = baseVersion.RegistryPaths;
            this.RegistryKey = baseVersion.RegistryKey;
            this.SupportedLangIds = baseVersion.SupportedLangIds;
            this.CfgCopySupported = baseVersion.CfgCopySupported;
        }
    }

    /// <summary>
    /// 각 VEGAS 버전에 대한 정적인 정보(이름, 레지스트리 경로, 지원 언어 등)를 정의하는 기본 클래스.
    /// </summary>
    public class VegasVersion
    {
        public string Name { get; set; }                  // UI에 표시될 버전 이름 (예: "Vegas Pro 23.0")
        public string[] RegistryPaths { get; set; }       // 설치 정보를 찾기 위한 가능한 레지스트리 기본 경로들
        public string RegistryKey { get; set; }           // 버전별 고유 키 (예: "23.0")
        public int[] SupportedLangIds { get; set; }      // 해당 버전이 공식적으로 지원하는 언어의 숫자 ID 배열
        public bool CfgCopySupported { get; set; }        // 추가 언어 파일(.cfg) 복사가 필요한 버전인지 여부
    }

    /// <summary>
    /// 애플리케이션 전반에서 사용되는 모든 정적 데이터(지원 버전, 언어 코드, 다국어 텍스트 등)를
    /// 중앙에서 관리하는 정적 클래스.
    /// </summary>
    public static class VegasData
    {
        // 각 데이터는 `static readonly`로 선언하여 프로그램 실행 중 변경되지 않음을 보장.

        public static readonly Dictionary<int, string> LangIdToKey;           // 레지스트리 언어 ID(1042)를 내부 언어 키("korean")로 매핑.
        public static readonly Dictionary<string, string> LangKeyToCfgSuffix; // 내부 언어 키("korean")를 .cfg 파일 접미사("ko_KR")로 매핑.
        public static readonly Dictionary<string, string> AboutInfo;          // '정보' 대화상자에 표시될 프로그램 정보.
        public static readonly List<VegasVersion> AllSupportedVersions;       // 이 프로그램이 지원하는 모든 VEGAS 버전의 정적 정보 목록.
        public static readonly Dictionary<string, Dictionary<string, string>> Texts; // 다국어 UI 텍스트 저장소.

        /// <summary>
        /// 정적 생성자. 프로그램에서 `VegasData` 클래스를 처음 참조할 때 단 한 번 실행되어 모든 정적 데이터를 초기화.
        /// </summary>
        static VegasData()
        {
            // --- 언어 ID 및 파일명 매핑 데이터 ---
            LangIdToKey = new Dictionary<int, string>
            {
                { 1031, "german" }, { 1033, "english" }, { 1034, "spanish" }, { 1036, "french" },
                { 1041, "japanese" }, { 1042, "korean" }, { 1045, "polish" }, { 1046, "portuguese" },
                { 1049, "russian" }, { 2052, "chinese_simplified" }
            };
            LangKeyToCfgSuffix = new Dictionary<string, string>
            {
                { "german", "de_DE" }, { "english", "en_US" }, { "spanish", "es_ES" }, { "french", "fr_FR" },
                { "japanese", "ja_JP" }, { "korean", "ko_KR" }, { "portuguese", "pt_BR" }, { "chinese_simplified", "zh_CN" }
            };

            // --- 프로그램 정보 데이터 ---
            AboutInfo = new Dictionary<string, string>
            {
                { "version", "1.1.0" }, { "updated", "2025-10-28" },
                { "license", "GNU General Public License v3.0" }, { "developer", "(Github) IZH318" },
                { "website", "https://github.com/IZH318" }
            };

            // --- 지원 VEGAS 버전 목록 데이터 ---
            AllSupportedVersions = new List<VegasVersion>
            {
                new VegasVersion { Name = "Vegas Pro 9.0", RegistryPaths = new[] { @"SOFTWARE\Sony Creative Software\Vegas Pro" }, RegistryKey = "9.0", SupportedLangIds = new[] { 1031, 1033, 1034, 1036, 1041 }, CfgCopySupported = false },
                new VegasVersion { Name = "Vegas Pro 10.0", RegistryPaths = new[] { @"SOFTWARE\Sony Creative Software\Vegas Pro" }, RegistryKey = "10.0", SupportedLangIds = new[] { 1031, 1033, 1034, 1036, 1041 }, CfgCopySupported = false },
                new VegasVersion { Name = "Vegas Pro 11.0", RegistryPaths = new[] { @"SOFTWARE\Sony Creative Software\Vegas Pro" }, RegistryKey = "11.0", SupportedLangIds = new[] { 1031, 1033, 1034, 1036, 1041, 1049 }, CfgCopySupported = false },
                new VegasVersion { Name = "Vegas Pro 12.0", RegistryPaths = new[] { @"SOFTWARE\Sony Creative Software\Vegas Pro" }, RegistryKey = "12.0", SupportedLangIds = new[] { 1031, 1033, 1034, 1036, 1041, 1049 }, CfgCopySupported = false },
                new VegasVersion { Name = "Vegas Pro 13.0", RegistryPaths = new[] { @"SOFTWARE\Sony Creative Software\Vegas Pro" }, RegistryKey = "13.0", SupportedLangIds = new[] { 1031, 1033, 1034, 1036, 1041, 1045, 1049, 2052 }, CfgCopySupported = false },
                new VegasVersion { Name = "Vegas Pro 14.0", RegistryPaths = new[] { @"SOFTWARE\VEGAS Creative Software\VEGAS Pro", @"SOFTWARE\MAGIX\VEGAS Pro", @"SOFTWARE\Sony Creative Software\Vegas Pro" }, RegistryKey = "14.0", SupportedLangIds = new[] { 1031, 1033, 1034, 1036, 1041, 1045, 1049, 2052 }, CfgCopySupported = true },
                new VegasVersion { Name = "Vegas Pro 15.0", RegistryPaths = new[] { @"SOFTWARE\VEGAS Creative Software\VEGAS Pro", @"SOFTWARE\MAGIX\VEGAS Pro", @"SOFTWARE\Sony Creative Software\Vegas Pro" }, RegistryKey = "15.0", SupportedLangIds = new[] { 1031, 1033, 1034, 1036, 1041, 1042, 1045, 1049, 2052 }, CfgCopySupported = true },
                new VegasVersion { Name = "Vegas Pro 16.0", RegistryPaths = new[] { @"SOFTWARE\VEGAS Creative Software\VEGAS Pro", @"SOFTWARE\MAGIX\VEGAS Pro", @"SOFTWARE\Sony Creative Software\Vegas Pro" }, RegistryKey = "16.0", SupportedLangIds = new[] { 1031, 1033, 1034, 1036, 1041, 1042, 1045, 1046, 1049, 2052 }, CfgCopySupported = true },
                new VegasVersion { Name = "Vegas Pro 17.0", RegistryPaths = new[] { @"SOFTWARE\VEGAS Creative Software\VEGAS Pro", @"SOFTWARE\MAGIX\VEGAS Pro", @"SOFTWARE\Sony Creative Software\Vegas Pro" }, RegistryKey = "17.0", SupportedLangIds = new[] { 1031, 1033, 1034, 1036, 1041, 1042, 1045, 1046, 2052 }, CfgCopySupported = true },
                new VegasVersion { Name = "Vegas Pro 18.0", RegistryPaths = new[] { @"SOFTWARE\VEGAS Creative Software\VEGAS Pro", @"SOFTWARE\MAGIX\VEGAS Pro", @"SOFTWARE\Sony Creative Software\Vegas Pro" }, RegistryKey = "18.0", SupportedLangIds = new[] { 1031, 1033, 1034, 1036, 1041, 1042, 1045, 1046, 2052 }, CfgCopySupported = true },
                new VegasVersion { Name = "Vegas Pro 19.0", RegistryPaths = new[] { @"SOFTWARE\VEGAS Creative Software\VEGAS Pro", @"SOFTWARE\MAGIX\VEGAS Pro", @"SOFTWARE\Sony Creative Software\Vegas Pro" }, RegistryKey = "19.0", SupportedLangIds = new[] { 1031, 1033, 1034, 1036, 1041, 1042, 1045, 1046, 2052 }, CfgCopySupported = true },
                new VegasVersion { Name = "Vegas Pro 20.0", RegistryPaths = new[] { @"SOFTWARE\VEGAS Creative Software\VEGAS Pro", @"SOFTWARE\MAGIX\VEGAS Pro", @"SOFTWARE\Sony Creative Software\Vegas Pro" }, RegistryKey = "20.0", SupportedLangIds = new[] { 1031, 1033, 1034, 1036, 1041, 1042, 1045, 1046, 2052 }, CfgCopySupported = true },
                new VegasVersion { Name = "Vegas Pro 21.0", RegistryPaths = new[] { @"SOFTWARE\VEGAS Creative Software\VEGAS Pro", @"SOFTWARE\MAGIX\VEGAS Pro", @"SOFTWARE\Sony Creative Software\Vegas Pro" }, RegistryKey = "21.0", SupportedLangIds = new[] { 1031, 1033, 1034, 1036, 1041, 1042, 1045, 1046, 2052 }, CfgCopySupported = true },
                new VegasVersion { Name = "Vegas Pro 22.0", RegistryPaths = new[] { @"SOFTWARE\VEGAS Creative Software\VEGAS Pro", @"SOFTWARE\MAGIX\VEGAS Pro", @"SOFTWARE\Sony Creative Software\Vegas Pro" }, RegistryKey = "22.0", SupportedLangIds = new[] { 1031, 1033, 1034, 1036, 1041, 1042, 1045, 1046, 2052 }, CfgCopySupported = true },
                new VegasVersion { Name = "Vegas Pro 23.0", RegistryPaths = new[] { @"SOFTWARE\VEGAS Creative Software\VEGAS Pro", @"SOFTWARE\MAGIX\VEGAS Pro", @"SOFTWARE\Sony Creative Software\Vegas Pro" }, RegistryKey = "23.0", SupportedLangIds = new[] { 1031, 1033, 1034, 1036, 1041, 1042, 1045, 1046, 2052 }, CfgCopySupported = true },
                new VegasVersion { Name = "Vegas Movie Studio 8.0", RegistryPaths = new[] { @"SOFTWARE\Sony Media Software\Vegas Movie Studio" }, RegistryKey = "8.0", SupportedLangIds = new[] { 1031, 1033, 1034, 1036, 1041 }, CfgCopySupported = false },
                new VegasVersion { Name = "Vegas Movie Studio Platinum 8.0", RegistryPaths = new[] { @"SOFTWARE\Sony Media Software\Vegas Movie Studio Platinum" }, RegistryKey = "8.0", SupportedLangIds = new[] { 1031, 1033, 1034, 1036, 1041 }, CfgCopySupported = false },
                new VegasVersion { Name = "Vegas Movie Studio 9.0", RegistryPaths = new[] { @"SOFTWARE\Sony Creative Software\Vegas Movie Studio" }, RegistryKey = "9.0", SupportedLangIds = new[] { 1033 }, CfgCopySupported = false },
                new VegasVersion { Name = "Vegas Movie Studio Platinum 9.0", RegistryPaths = new[] { @"SOFTWARE\Sony Creative Software\Vegas Movie Studio Platinum" }, RegistryKey = "9.0", SupportedLangIds = new[] { 1031, 1033, 1034, 1036, 1041 }, CfgCopySupported = false },
                new VegasVersion { Name = "Vegas Movie Studio HD Platinum 10.0", RegistryPaths = new[] { @"SOFTWARE\Sony Creative Software\Vegas Movie Studio HD Platinum" }, RegistryKey = "10.0", SupportedLangIds = new[] { 1031, 1033, 1034, 1036, 1041 }, CfgCopySupported = false },
                new VegasVersion { Name = "Vegas Movie Studio HD Platinum 11.0", RegistryPaths = new[] { @"SOFTWARE\Sony Creative Software\Vegas Movie Studio HD Platinum" }, RegistryKey = "11.0", SupportedLangIds = new[] { 1031, 1033, 1034, 1036, 1041 }, CfgCopySupported = false },
                new VegasVersion { Name = "Vegas Movie Studio Platinum 12.0", RegistryPaths = new[] { @"SOFTWARE\Sony Creative Software\Movie Studio Platinum" }, RegistryKey = "12.0", SupportedLangIds = new[] { 1031, 1033, 1034, 1036, 1041, 1045, 1049, 2052 }, CfgCopySupported = false },
                new VegasVersion { Name = "Vegas Movie Studio Platinum 13.0", RegistryPaths = new[] { @"SOFTWARE\Sony Creative Software\Movie Studio Platinum" }, RegistryKey = "13.0", SupportedLangIds = new[] { 1031, 1033, 1034, 1036, 1041, 1045, 1049, 2052 }, CfgCopySupported = false },
                new VegasVersion { Name = "Vegas Movie Studio Platinum 14.0", RegistryPaths = new[] { @"SOFTWARE\VEGAS\Movie Studio Platinum", @"SOFTWARE\MAGIX\Movie Studio Platinum", @"SOFTWARE\Sony Creative Software\Movie Studio Platinum" }, RegistryKey = "14.0", SupportedLangIds = new[] { 1031, 1033, 1034, 1036, 1041, 1045, 1046, 1049, 2052 }, CfgCopySupported = false },
                new VegasVersion { Name = "Vegas Movie Studio Platinum 15.0", RegistryPaths = new[] { @"SOFTWARE\VEGAS\Movie Studio Platinum", @"SOFTWARE\MAGIX\Movie Studio Platinum", @"SOFTWARE\Sony Creative Software\Movie Studio Platinum" }, RegistryKey = "15.0", SupportedLangIds = new[] { 1031, 1033, 1034, 1036, 1041, 1045, 1046, 1049, 2052 }, CfgCopySupported = false },
                new VegasVersion { Name = "Vegas Movie Studio Platinum 16.0", RegistryPaths = new[] { @"SOFTWARE\VEGAS\Movie Studio Platinum", @"SOFTWARE\MAGIX\Movie Studio Platinum", @"SOFTWARE\Sony Creative Software\Movie Studio Platinum" }, RegistryKey = "16.0", SupportedLangIds = new[] { 1031, 1033, 1034, 1036, 1041, 1042, 1045, 1046, 1049, 2052 }, CfgCopySupported = false },
                new VegasVersion { Name = "Vegas Movie Studio Platinum 17.0", RegistryPaths = new[] { @"SOFTWARE\VEGAS\Movie Studio Platinum", @"SOFTWARE\MAGIX\Movie Studio Platinum", @"SOFTWARE\Sony Creative Software\Movie Studio Platinum" }, RegistryKey = "17.0", SupportedLangIds = new[] { 1031, 1033, 1034, 1036, 1041, 1042, 1045, 1046, 1049, 2052 }, CfgCopySupported = false },
            };

            // --- 다국어 UI 텍스트 데이터 ---
            Texts = new Dictionary<string, Dictionary<string, string>>();

            // 독일어 (de)
            var de = new Dictionary<string, string> {
                { "title", "Vegas Sprachwechsler" }, { "info_text", "Wählen Sie die Vegas-Versionen auf Ihrem PC aus, um deren Sprache zu ändern." },
                { "no_versions_found", "Keine installierten Vegas Pro oder Vegas Movie Studio Versionen gefunden." }, { "select_version", "Version auswählen" }, { "no_common_lang", "Keine gemeinsame Sprache" }, { "change_lang_button", "Sprache ändern" },
                { "error_title", "Fehler" }, { "error_no_valid_lang", "Keine gültige Sprache ausgewählt." }, { "confirm_title", "Ausführung bestätigen" },
                { "confirm_message", "Die Sprache für {num_versions} ausgewählte(n) Version(en) wird auf '{lang_name}' geändert.\n\nSicherung, Kopieren von Sprachdateien und Cache-Löschung werden gemeinsam durchgeführt." },
                { "backup_fail_title", "Sicherung fehlgeschlagen" }, { "backup_fail_message", "Die Registrierungssicherung ist fehlgeschlagen." }, { "task_complete_title", "Aufgabe abgeschlossen" },
                { "task_complete_message", "Sicherungsdatei wurde auf Ihrem Desktop unter '{backup_filename}' gespeichert." }, { "success", "Registrierungsänderung erfolgreich:" }, { "failure", "Registrierungsänderung fehlgeschlagen:" },
                { "reg_path_not_found", "Registrierungspfad nicht gefunden." }, { "program_language", "Programmsprache" }, { "vegas_pro_15_korean_warning", "Vegas Pro 15.0 unterstützt Koreanisch ab Build 384. Fortfahren?" },
                { "german", "Deutsch" }, { "english", "Englisch" }, { "spanish", "Spanisch" }, { "french", "Französisch" }, { "japanese", "Japanisch" }, { "korean", "Koreanisch" }, { "polish", "Polnisch" }, { "portuguese", "Portugiesisch" }, { "russian", "Russisch" }, { "chinese_simplified", "Chinesisch (vereinfacht)" },
                { "cache_clear_results", "Ergebnisse der Plugin-Cache-Löschung:" }, { "cache_cleared", "{version_name}: Cache-Dateien gelöscht" }, { "cache_clear_failed", "{version_name}: Cache-Löschung fehlgeschlagen ({error})" },
                { "available_versions", "Verfügbare Versionen" }, { "versions_to_change", "Zu ändernde Versionen" }, { "error_no_versions_selected", "Bitte wählen Sie mindestens eine Version zum Ändern aus." },
                { "cfg_copy_results", "Ergebnisse des Kopierens der Sprachdatei (.cfg):" }, { "cfg_copied", "{version_name}: '{filename}' erfolgreich kopiert" }, { "cfg_copy_failed_no_source", "{version_name}: Quelldatei nicht gefunden" },
                { "cfg_copy_failed_no_dest", "{version_name}: Installationspfad nicht gefunden" }, { "cfg_copy_failed_permission", "{version_name}: Kopierberechtigung verweigert ({error})" }, { "cfg_copy_unsupported", "{version_name}: Nicht unterstützte Version" },
                { "about_menu", "Über" }, { "about_label_version", "Version" }, { "about_label_updated", "Letzte Aktualisierung" }, { "about_label_developer", "Entwickler" }, { "about_label_website", "Webseite" }, { "about_label_license", "Lizenz" },
                { "about_label_special_thanks", "===== Besonderer Dank =====" },
                { "about_special_thanks_content", "(Github) zzzzzz9125:\n     Bereitstellung der Dateien für Vegas Pro 20 (offizielle local_zh_CN.cfg)\n     und Vegas Pro 21 ~ 23 (alle offiziellen cfg)" }
            };
            Texts.Add("de", de);

            // 영어 (en) - 기본 대체 언어
            var en = new Dictionary<string, string> {
                { "title", "Vegas Language Changer" }, { "info_text", "Select the Vegas versions on your PC to change their language." },
                { "no_versions_found", "No installed Vegas Pro or Vegas Movie Studio versions found." }, { "select_version", "Select a Version" }, { "no_common_lang", "No Common Language" }, { "change_lang_button", "Change Language" },
                { "error_title", "Error" }, { "error_no_valid_lang", "No valid language selected." }, { "confirm_title", "Execution Confirmation" },
                { "confirm_message", "The language for {num_versions} selected version(s) will be changed to '{lang_name}'.\n\nSettings backup, language file copy, and cache clearing will be performed together." },
                { "backup_fail_title", "Backup Failed" }, { "backup_fail_message", "Registry backup failed." }, { "task_complete_title", "Task Completed" },
                { "task_complete_message", "Backup file has been saved to your desktop at '{backup_filename}'." }, { "success", "Registry Change Success:" }, { "failure", "Registry Change Failure:" },
                { "reg_path_not_found", "Registry path not found." }, { "program_language", "Program Language" }, { "vegas_pro_15_korean_warning", "Vegas Pro 15.0 supports Korean from build 384 or later. Continue?" },
                { "german", "German" }, { "english", "English" }, { "spanish", "Spanish" }, { "french", "French" }, { "japanese", "Japanese" }, { "korean", "Korean" }, { "polish", "Polish" }, { "portuguese", "Portuguese" }, { "russian", "Russian" }, { "chinese_simplified", "Chinese Simplified" },
                { "cache_clear_results", "Plugin Cache Deletion Results:" }, { "cache_cleared", "{version_name}: Cache files deleted" }, { "cache_clear_failed", "{version_name}: Cache deletion failed ({error})" },
                { "available_versions", "Available Versions" }, { "versions_to_change", "Versions to Change" }, { "error_no_versions_selected", "Please select at least one version to change." },
                { "cfg_copy_results", "Language File (.cfg) Copy Results:" }, { "cfg_copied", "{version_name}: Copied '{filename}' successfully" }, { "cfg_copy_failed_no_source", "{version_name}: Source file not found" },
                { "cfg_copy_failed_no_dest", "{version_name}: Installation path not found" }, { "cfg_copy_failed_permission", "{version_name}: Copy permission denied ({error})" }, { "cfg_copy_unsupported", "{version_name}: Unsupported version" },
                { "about_menu", "About" }, { "about_label_version", "Version" }, { "about_label_updated", "Last updated" }, { "about_label_developer", "Developer" }, { "about_label_website", "Website" }, { "about_label_license", "License" },
                { "about_label_special_thanks", "===== Special Thanks =====" },
                { "about_special_thanks_content", "(Github) zzzzzz9125:\n     Provided Vegas Pro 20 (official local_zh_CN.cfg)\n     and Vegas Pro 21 ~ 23 (all official cfg) files" }
            };
            Texts.Add("en", en);

            // 스페인어 (es)
            var es = new Dictionary<string, string> {
                { "title", "Cambiador de Idioma de Vegas" }, { "info_text", "Seleccione las versiones de Vegas en su PC para cambiar su idioma." },
                { "no_versions_found", "No se encontraron versiones de Vegas Pro o Vegas Movie Studio instaladas." }, { "select_version", "Seleccionar una versión" }, { "no_common_lang", "Ningún idioma común" }, { "change_lang_button", "Cambiar idioma" },
                { "error_title", "Error" }, { "error_no_valid_lang", "No se ha seleccionado un idioma válido." }, { "confirm_title", "Confirmación de ejecución" },
                { "confirm_message", "El idioma para {num_versions} versión(es) seleccionada(s) se cambiará a '{lang_name}'.\n\nLa copia de seguridad, la copia de archivos de idioma y la limpieza de la caché se realizarán conjuntamente." },
                { "backup_fail_title", "Copia de seguridad fallida" }, { "backup_fail_message", "La copia de seguridad del registro falló." }, { "task_complete_title", "Tarea completada" },
                { "task_complete_message", "El archivo de copia de seguridad se ha guardado en su escritorio en '{backup_filename}'." }, { "success", "Éxito al cambiar el registro:" }, { "failure", "Fallo al cambiar el registro:" },
                { "reg_path_not_found", "Ruta del registro no encontrada." }, { "program_language", "Idioma del programa" }, { "vegas_pro_15_korean_warning", "Vegas Pro 15.0 es compatible con el coreano a partir de la compilación 384. ¿Desea continuar?" },
                { "german", "Alemán" }, { "english", "Inglés" }, { "spanish", "Español" }, { "french", "Francés" }, { "japanese", "Japonés" }, { "korean", "Coreano" }, { "polish", "Polaco" }, { "portuguese", "Portugués" }, { "russian", "Ruso" }, { "chinese_simplified", "Chino Simplificado" },
                { "cache_clear_results", "Resultados de eliminación de caché de complementos:" }, { "cache_cleared", "{version_name}: Archivos de caché eliminados" }, { "cache_clear_failed", "{version_name}: Fallo al eliminar caché ({error})" },
                { "available_versions", "Versiones disponibles" }, { "versions_to_change", "Versiones a cambiar" }, { "error_no_versions_selected", "Seleccione al menos una versión para cambiar." },
                { "cfg_copy_results", "Resultados de la copia del archivo de idioma (.cfg):" }, { "cfg_copied", "{version_name}: '{filename}' copiado correctamente" }, { "cfg_copy_failed_no_source", "{version_name}: Archivo de origen no encontrado" },
                { "cfg_copy_failed_no_dest", "{version_name}: Ruta de instalación no encontrada" }, { "cfg_copy_failed_permission", "{version_name}: Permiso de copia denegado ({error})" }, { "cfg_copy_unsupported", "{version_name}: Versión no compatible" },
                { "about_menu", "Acerca de" }, { "about_label_version", "Versión" }, { "about_label_updated", "Última actualización" }, { "about_label_developer", "Desarrollador" }, { "about_label_website", "Sitio web" }, { "about_label_license", "Licencia" },
                { "about_label_special_thanks", "===== Agradecimientos Especiales =====" },
                { "about_special_thanks_content", "(Github) zzzzzz9125:\n     Proporcionó archivos para Vegas Pro 20 (local_zh_CN.cfg oficial)\n     y Vegas Pro 21 ~ 23 (todos los cfg oficiales)" }
            };
            Texts.Add("es", es);

            // 프랑스어 (fr)
            var fr = new Dictionary<string, string> {
                { "title", "Changeur de Langue Vegas" }, { "info_text", "Sélectionnez les versions de Vegas sur votre PC pour changer leur langue." },
                { "no_versions_found", "Aucune version de Vegas Pro ou Vegas Movie Studio installée trouvée." }, { "select_version", "Sélectionner une version" }, { "no_common_lang", "Aucune langue commune" }, { "change_lang_button", "Changer la langue" },
                { "error_title", "Erreur" }, { "error_no_valid_lang", "Aucune langue valide sélectionnée." }, { "confirm_title", "Confirmation d'exécution" },
                { "confirm_message", "La langue pour {num_versions} version(s) sélectionnée(s) sera changée en '{lang_name}'.\n\nLa sauvegarde, la copie des fichiers de langue et le nettoyage du cache seront effectués ensemble." },
                { "backup_fail_title", "Sauvegarde échouée" }, { "backup_fail_message", "La sauvegarde du registre a échoué." }, { "task_complete_title", "Tâche terminée" },
                { "task_complete_message", "Le fichier de sauvegarde a été enregistré sur votre bureau sous '{backup_filename}'." }, { "success", "Succès de la modification du registre :" }, { "failure", "Échec de la modification du registre :" },
                { "reg_path_not_found", "Chemin du registre introuvable." }, { "program_language", "Langue du programme" }, { "vegas_pro_15_korean_warning", "Vegas Pro 15.0 prend en charge le coréen à partir de la version 384. Continuer ?" },
                { "german", "Allemand" }, { "english", "Anglais" }, { "spanish", "Espagnol" }, { "french", "Français" }, { "japanese", "Japonais" }, { "korean", "Coréen" }, { "polish", "Polonais" }, { "portuguese", "Portugais" }, { "russian", "Russe" }, { "chinese_simplified", "Chinois simplifié" },
                { "cache_clear_results", "Résultats de la suppression du cache des plugins :" }, { "cache_cleared", "{version_name} : Fichiers de cache supprimés" }, { "cache_clear_failed", "{version_name} : Échec de la suppression du cache ({error})" },
                { "available_versions", "Versions disponibles" }, { "versions_to_change", "Versions à modifier" }, { "error_no_versions_selected", "Veuillez sélectionner au moins une version à modifier." },
                { "cfg_copy_results", "Résultats de la copie du fichier de langue (.cfg) :" }, { "cfg_copied", "{version_name} : '{filename}' copié avec succès" }, { "cfg_copy_failed_no_source", "{version_name} : Fichier source introuvable" },
                { "cfg_copy_failed_no_dest", "{version_name} : Chemin d'installation introuvable" }, { "cfg_copy_failed_permission", "{version_name} : Autorisation de copie refusée ({error})" }, { "cfg_copy_unsupported", "{version_name} : Version non supportée" },
                { "about_menu", "À propos" }, { "about_label_version", "Version" }, { "about_label_updated", "Dernière mise à jour" }, { "about_label_developer", "Développeur" }, { "about_label_website", "Site web" }, { "about_label_license", "Licence" },
                { "about_label_special_thanks", "===== Remerciements Particuliers =====" },
                { "about_special_thanks_content", "(Github) zzzzzz9125:\n     A fourni les fichiers pour Vegas Pro 20 (local_zh_CN.cfg officiel)\n     et Vegas Pro 21 ~ 23 (tous les cfg officiels)" }
            };
            Texts.Add("fr", fr);

            // 일본어 (ja)
            var ja = new Dictionary<string, string> {
                { "title", "Vegas 言語変更ツール" }, { "info_text", "PCにインストールされているVegasのバージョンを選択して言語を変更します。" },
                { "no_versions_found", "インストールされているVegas ProまたはVegas Movie Studioのバージョンが見つかりません。" }, { "select_version", "バージョンを選択" }, { "no_common_lang", "共通言語なし" }, { "change_lang_button", "言語を変更" },
                { "error_title", "エラー" }, { "error_no_valid_lang", "有効な言語が選択されていません。" }, { "confirm_title", "実行確認" },
                { "confirm_message", "選択された{num_versions}つのバージョンの言語を「{lang_name}」に変更します。\n\n設定のバックアップ、言語ファイルのコピー、キャッシュのクリアが同時に実行されます。" },
                { "backup_fail_title", "バックアップ失敗" }, { "backup_fail_message", "レジストリのバックアップに失敗しました。" }, { "task_complete_title", "タスク完了" },
                { "task_complete_message", "バックアップファイルはデスクトップの「{backup_filename}」に保存されました。" }, { "success", "レジストリ変更成功:" }, { "failure", "レジストリ変更失敗:" },
                { "reg_path_not_found", "レジストリパスが見つかりません。" }, { "program_language", "プログラム言語" }, { "vegas_pro_15_korean_warning", "Vegas Pro 15.0はビルド384以降で韓国語をサポートしています。続行しますか？" },
                { "german", "ドイツ語" }, { "english", "英語" }, { "spanish", "スペイン語" }, { "french", "フランス語" }, { "japanese", "日本語" }, { "korean", "韓国語" }, { "polish", "ポーランド語" }, { "portuguese", "ポルトガル語" }, { "russian", "ロシア語" }, { "chinese_simplified", "中国語 (簡体字)" },
                { "cache_clear_results", "プラグインキャッシュの削除結果：" }, { "cache_cleared", "{version_name}：キャッシュファイルを削除しました" }, { "cache_clear_failed", "{version_name}：キャッシュの削除に失敗しました ({error})" },
                { "available_versions", "利用可能なバージョン" }, { "versions_to_change", "変更するバージョン" }, { "error_no_versions_selected", "変更するバージョンを少なくとも1つ選択してください。" },
                { "cfg_copy_results", "言語ファイル（.cfg）のコピー結果：" }, { "cfg_copied", "{version_name}：「{filename}」を正常にコピーしました" }, { "cfg_copy_failed_no_source", "{version_name}：ソースファイルが見つかりません" },
                { "cfg_copy_failed_no_dest", "{version_name}：インストールパスが見つかりません" }, { "cfg_copy_failed_permission", "{version_name}：コピー権限がありません ({error})" }, { "cfg_copy_unsupported", "{version_name}：サポートされていないバージョン" },
                { "about_menu", "情報" }, { "about_label_version", "バージョン" }, { "about_label_updated", "最終更新日" }, { "about_label_developer", "開発者" }, { "about_label_website", "ウェブサイト" }, { "about_label_license", "ライセンス" },
                { "about_label_special_thanks", "===== 協力してくださった方々 =====" },
                { "about_special_thanks_content", "(Github) zzzzzz9125様:\n     Vegas Pro 20 (公式 local_zh_CN.cfg)、\n     Vegas Pro 21 ~ 23 (全ての公式 cfg) ファイルを提供" }
            };
            Texts.Add("ja", ja);

            // 한국어 (ko)
            var ko = new Dictionary<string, string> {
                { "title", "Vegas 언어 변경 프로그램" }, { "info_text", "PC에 설치된 Vegas 버전을 선택하여 언어를 변경하세요." },
                { "no_versions_found", "설치된 Vegas Pro 또는 Vegas Movie Studio 버전을 찾을 수 없습니다." }, { "select_version", "버전을 선택하세요" }, { "no_common_lang", "공통 지원 언어 없음" }, { "change_lang_button", "언어 변경" },
                { "error_title", "오류" }, { "error_no_valid_lang", "유효한 언어가 선택되지 않았습니다." }, { "confirm_title", "실행 확인" },
                { "confirm_message", "선택된 {num_versions}개 버전의 언어를 '{lang_name}'(으)로 변경합니다.\n\n현재 설정 백업, 언어 파일 복사, 캐시 삭제가 함께 진행됩니다。" },
                { "backup_fail_title", "백업 실패" }, { "backup_fail_message", "레지스트리 백업에 실패했습니다." }, { "task_complete_title", "작업 완료" },
                { "task_complete_message", "백업 파일이 바탕화면의 '{backup_filename}'에 저장되었습니다." }, { "success", "레지스트리 변경 성공:" }, { "failure", "레지스트리 변경 실패:" },
                { "reg_path_not_found", "레지스트리 경로를 찾을 수 없습니다." }, { "program_language", "프로그램 언어" }, { "vegas_pro_15_korean_warning", "Vegas Pro 15.0 버전은 Build 384 이상부터 한국어를 지원합니다. 계속 진행하시겠습니까?" },
                { "german", "독일어" }, { "english", "영어" }, { "spanish", "스페인어" }, { "french", "프랑스어" }, { "japanese", "일본어" }, { "korean", "한국어" }, { "polish", "폴란드어" }, { "portuguese", "포르투갈어" }, { "russian", "러시아어" }, { "chinese_simplified", "중국어 간체" },
                { "cache_clear_results", "플러그인 캐시 삭제 결과:" }, { "cache_cleared", "{version_name}: 캐시 파일 삭제됨" }, { "cache_clear_failed", "{version_name}: 캐시 삭제 실패 ({error})" },
                { "available_versions", "사용 가능 버전" }, { "versions_to_change", "변경할 버전" }, { "error_no_versions_selected", "언어를 변경할 버전을 하나 이상 선택하세요." },
                { "cfg_copy_results", "언어 파일(.cfg) 복사 결과:" }, { "cfg_copied", "{version_name}: '{filename}' 복사 성공" }, { "cfg_copy_failed_no_source", "{version_name}: 원본 파일을 찾을 수 없음" },
                { "cfg_copy_failed_no_dest", "{version_name}: 설치 경로를 찾을 수 없음" }, { "cfg_copy_failed_permission", "{version_name}: 복사 권한 없음 ({error})" }, { "cfg_copy_unsupported", "{version_name}: 지원하지 않는 버전" },
                { "about_menu", "정보" }, { "about_label_version", "버전" }, { "about_label_updated", "최종 업데이트" }, { "about_label_developer", "개발자" }, { "about_label_website", "웹사이트" }, { "about_label_license", "라이선스" },
                { "about_label_special_thanks", "===== 도움을 주신 분 =====" },
                { "about_special_thanks_content", "(Github) zzzzzz9125:\n     Vegas Pro 20 (공식 local_zh_CN.cfg),\n     Vegas Pro 21 ~ 23 (모든 공식 cfg) 파일 제공" }
            };
            Texts.Add("ko", ko);

            // 폴란드어 (pl)
            var pl = new Dictionary<string, string> {
                { "title", "Zmieniacz Języka Vegas" }, { "info_text", "Wybierz wersje Vegas na swoim komputerze, aby zmienić ich język." },
                { "no_versions_found", "Nie znaleziono zainstalowanych wersji Vegas Pro lub Vegas Movie Studio." }, { "select_version", "Wybierz wersję" }, { "no_common_lang", "Brak wspólnego języka" }, { "change_lang_button", "Zmień język" },
                { "error_title", "Błąd" }, { "error_no_valid_lang", "Nie wybrano prawidłowego języka." }, { "confirm_title", "Potwierdzenie wykonania" },
                { "confirm_message", "Język dla {num_versions} wybranej(ych) wersji zostanie zmieniony na '{lang_name}'.\n\nKopia zapasowa, kopiowanie plików językowych i czyszczenie pamięci podręcznej zostaną wykonane jednocześnie." },
                { "backup_fail_title", "Niepowodzenie kopii zapasowej" }, { "backup_fail_message", "Kopia zapasowa rejestru nie powiodła się." }, { "task_complete_title", "Zadanie zakończone" },
                { "task_complete_message", "Plik kopii zapasowej został zapisany na pulpicie jako '{backup_filename}'." }, { "success", "Sukces zmiany rejestru:" }, { "failure", "Niepowodzenie zmiany rejestru:" },
                { "reg_path_not_found", "Nie znaleziono ścieżki rejestru." }, { "program_language", "Język programu" }, { "vegas_pro_15_korean_warning", "Vegas Pro 15.0 obsługuje język koreański od kompilacji 384. Kontynuować?" },
                { "german", "Niemiecki" }, { "english", "Angielski" }, { "spanish", "Hiszpański" }, { "french", "Francuski" }, { "japanese", "Japoński" }, { "korean", "Koreański" }, { "polish", "Polski" }, { "portuguese", "Portugalski" }, { "russian", "Rosyjski" }, { "chinese_simplified", "Chiński uproszczony" },
                { "cache_clear_results", "Wyniki usuwania pamięci podręcznej wtyczek:" }, { "cache_cleared", "{version_name}: Pliki pamięci podręcznej usunięte" }, { "cache_clear_failed", "{version_name}: Nie udało się usunąć pamięci podręcznej ({error})" },
                { "available_versions", "Dostępne wersje" }, { "versions_to_change", "Wersje do zmiany" }, { "error_no_versions_selected", "Wybierz co najmniej jedną wersję do zmiany." },
                { "cfg_copy_results", "Wyniki kopiowania pliku językowego (.cfg):" }, { "cfg_copied", "{version_name}: Pomyślnie skopiowano '{filename}'" }, { "cfg_copy_failed_no_source", "{version_name}: Nie znaleziono pliku źródłowego" },
                { "cfg_copy_failed_no_dest", "{version_name}: Nie znaleziono ścieżki instalacji" }, { "cfg_copy_failed_permission", "{version_name}: Odmowa uprawnień do kopiowania ({error})" }, { "cfg_copy_unsupported", "{version_name}: Nieobsługiwana wersja" },
                { "about_menu", "O programie" }, { "about_label_version", "Wersja" }, { "about_label_updated", "Ostatnia aktualizacja" }, { "about_label_developer", "Deweloper" }, { "about_label_website", "Strona internetowa" }, { "about_label_license", "Licencja" },
                { "about_label_special_thanks", "===== Specjalne Podziękowania =====" },
                { "about_special_thanks_content", "(Github) zzzzzz9125:\n     Dostarczył pliki dla Vegas Pro 20 (oficjalny local_zh_CN.cfg)\n     oraz Vegas Pro 21 ~ 23 (wszystkie oficjalne cfg)" }
            };
            Texts.Add("pl", pl);

            // 포르투갈어 (pt)
            var pt = new Dictionary<string, string> {
                { "title", "Vegas Trocador de Idioma" }, { "info_text", "Selecione as versões do Vegas no seu PC para alterar o idioma." },
                { "no_versions_found", "Nenhuma versão do Vegas Pro ou Vegas Movie Studio instalada encontrada." }, { "select_version", "Selecione uma versão" }, { "no_common_lang", "Nenhum idioma comum" }, { "change_lang_button", "Alterar idioma" },
                { "error_title", "Erro" }, { "error_no_valid_lang", "Nenhum idioma válido selecionado." }, { "confirm_title", "Confirmação de Execução" },
                { "confirm_message", "O idioma para {num_versions} versão(ões) selecionada(s) será alterado para '{lang_name}'.\n\nO backup, a cópia de arquivos de idioma e a limpeza de cache serão realizados em conjunto." },
                { "backup_fail_title", "Falha no backup" }, { "backup_fail_message", "O backup do registro falhou." }, { "task_complete_title", "Tarefa Concluída" },
                { "task_complete_message", "O arquivo de backup foi salvo na sua área de trabalho em '{backup_filename}'." }, { "success", "Sucesso na alteração do registro:" }, { "failure", "Falha na alteração do registro:" },
                { "reg_path_not_found", "Caminho do registro não encontrado." }, { "program_language", "Idioma do Programa" }, { "vegas_pro_15_korean_warning", "O Vegas Pro 15.0 suporta coreano a partir da compilação 384. Continuar?" },
                { "german", "Alemão" }, { "english", "Inglês" }, { "spanish", "Espanhol" }, { "french", "Francês" }, { "japanese", "Japonês" }, { "korean", "Coreano" }, { "polish", "Polonês" }, { "portuguese", "Português" }, { "russian", "Russo" }, { "chinese_simplified", "Chinês Simplificado" },
                { "cache_clear_results", "Resultados da exclusão de cache de plugins:" }, { "cache_cleared", "{version_name}: Arquivos de cache excluídos" }, { "cache_clear_failed", "{version_name}: Falha na exclusão de cache ({error})" },
                { "available_versions", "Versões disponíveis" }, { "versions_to_change", "Versões para alterar" }, { "error_no_versions_selected", "Selecione pelo menos uma versão para alterar." },
                { "cfg_copy_results", "Resultados da cópia do arquivo de idioma (.cfg):" }, { "cfg_copied", "{version_name}: '{filename}' copiado com sucesso" }, { "cfg_copy_failed_no_source", "{version_name}: Arquivo de origen não encontrado" },
                { "cfg_copy_failed_no_dest", "{version_name}: Caminho de instalação não encontrado" }, { "cfg_copy_failed_permission", "{version_name}: Permissão de cópia negada ({error})" }, { "cfg_copy_unsupported", "{version_name}: Versão não suportada" },
                { "about_menu", "Sobre" }, { "about_label_version", "Versão" }, { "about_label_updated", "Última atualização" }, { "about_label_developer", "Desenvolvedor" }, { "about_label_website", "Website" }, { "about_label_license", "Licença" },
                { "about_label_special_thanks", "===== Agradecimentos Especiais =====" },
                { "about_special_thanks_content", "(Github) zzzzzz9125:\n     Forneceu os arquivos do Vegas Pro 20 (local_zh_CN.cfg oficial)\n     e do Vegas Pro 21 ~ 23 (todos os cfg oficiais)" }
            };
            Texts.Add("pt", pt);

            // 러시아어 (ru)
            var ru = new Dictionary<string, string> {
                { "title", "Смена языка Vegas" }, { "info_text", "Выберите версии Vegas на вашем ПК, чтобы изменить их язык." },
                { "no_versions_found", "Установленные версии Vegas Pro или Vegas Movie Studio не найдены." }, { "select_version", "Выберите версию" }, { "no_common_lang", "Нет общего языка" }, { "change_lang_button", "Изменить язык" },
                { "error_title", "Ошибка" }, { "error_no_valid_lang", "Не выбран действительный язык." }, { "confirm_title", "Подтверждение выполнения" },
                { "confirm_message", "Язык для {num_versions} выбранной(ых) версии(й) будет изменен на «{lang_name}».\n\nРезервное копирование, копирование языковых файлов и очистка кэша будут выполнены вместе." },
                { "backup_fail_title", "Сбой резервного копирования" }, { "backup_fail_message", "Резервное копирование реестра не удалось." }, { "task_complete_title", "Задача завершена" },
                { "task_complete_message", "Файл резервной копии сохранен на рабочем столе как «{backup_filename}»." }, { "success", "Реестр успешно изменен:" }, { "failure", "Ошибка изменения реестра:" },
                { "reg_path_not_found", "Путь реестра не найден." }, { "program_language", "Язык программы" }, { "vegas_pro_15_korean_warning", "Vegas Pro 15.0 поддерживает корейский язык начиная со сборки 384. Продолжить?" },
                { "german", "Немецкий" }, { "english", "Английский" }, { "spanish", "Испанский" }, { "french", "Французский" }, { "japanese", "Японский" }, { "korean", "Корейский" }, { "polish", "Польский" }, { "portuguese", "Португальский" }, { "russian", "Русский" }, { "chinese_simplified", "Упрощенный китайский" },
                { "cache_clear_results", "Результаты удаления кэша плагинов:" }, { "cache_cleared", "{version_name}: Файлы кэша удалены" }, { "cache_clear_failed", "{version_name}: Не удалось удалить кэш ({error})" },
                { "available_versions", "Доступные версии" }, { "versions_to_change", "Версии для изменения" }, { "error_no_versions_selected", "Пожалуйста, выберите хотя бы одну версию для изменения." },
                { "cfg_copy_results", "Результаты копирования языкового файла (.cfg):" }, { "cfg_copied", "{version_name}: Файл «{filename}» успешно скопирован" }, { "cfg_copy_failed_no_source", "{version_name}: Исходный файл не найден" },
                { "cfg_copy_failed_no_dest", "{version_name}: Путь установки не найден" }, { "cfg_copy_failed_permission", "{version_name}: В копировании отказано ({error})" }, { "cfg_copy_unsupported", "{version_name}: Неподдерживаемая версия" },
                { "about_menu", "О программе" }, { "about_label_version", "Версия" }, { "about_label_updated", "Последнее обновление" }, { "about_label_developer", "Разработчик" }, { "about_label_website", "Веб-сайт" }, { "about_label_license", "Лицензия" },
                { "about_label_special_thanks", "===== Особая благодарность =====" },
                { "about_special_thanks_content", "(Github) zzzzzz9125:\n     Предоставил файлы для Vegas Pro 20 (официальный local_zh_CN.cfg)\n     и Vegas Pro 21 ~ 23 (все официальные cfg)" }
            };
            Texts.Add("ru", ru);

            // 중국어 간체 (zh)
            var zh = new Dictionary<string, string> {
                { "title", "Vegas 语言切换器" }, { "info_text", "在您的电脑上选择 Vegas 版本以更改其语言。" },
                { "no_versions_found", "未找到已安装的 Vegas Pro 或 Vegas Movie Studio 版本。" }, { "select_version", "选择版本" }, { "no_common_lang", "无通用语言" }, { "change_lang_button", "更改语言" },
                { "error_title", "错误" }, { "error_no_valid_lang", "未选择有效的语言。" }, { "confirm_title", "执行确认" },
                { "confirm_message", "选定的 {num_versions} 个版本的语言将更改为 '{lang_name}'。\n\n设置备份、语言文件复制和缓存清理将一并执行。" },
                { "backup_fail_title", "备份失败" }, { "backup_fail_message", "注册表备份失败。" }, { "task_complete_title", "任务完成" },
                { "task_complete_message", "备份文件已保存到您的桌面，文件名为 '{backup_filename}'。" }, { "success", "注册表更改成功：" }, { "failure", "注册表更改失败：" },
                { "reg_path_not_found", "未找到注册表路径。" }, { "program_language", "程序语言" }, { "vegas_pro_15_korean_warning", "Vegas Pro 15.0 从版本号 384 开始支持韩语。要继续吗？" },
                { "german", "德语" }, { "english", "英语" }, { "spanish", "西班牙语" }, { "french", "法语" }, { "japanese", "日语" }, { "korean", "韩语" }, { "polish", "波兰语" }, { "portuguese", "葡萄牙语" }, { "russian", "俄语" }, { "chinese_simplified", "简体中文" },
                { "cache_clear_results", "插件缓存删除结果：" }, { "cache_cleared", "{version_name}：缓存文件已删除" }, { "cache_clear_failed", "{version_name}：缓存删除失败 ({error})" },
                { "available_versions", "可用版本" }, { "versions_to_change", "要更改的版本" }, { "error_no_versions_selected", "请至少选择一个要更改的版本。" },
                { "cfg_copy_results", "语言文件 (.cfg) 复制结果：" }, { "cfg_copied", "{version_name}：成功复制 '{filename}'" }, { "cfg_copy_failed_no_source", "{version_name}：未找到源文件" },
                { "cfg_copy_failed_no_dest", "{version_name}：未找到安装路径" }, { "cfg_copy_failed_permission", "{version_name}：复制权限被拒绝 ({error})" }, { "cfg_copy_unsupported", "{version_name}：不支持的版本" },
                { "about_menu", "关于" }, { "about_label_version", "版本" }, { "about_label_updated", "最后更新" }, { "about_label_developer", "开发者" }, { "about_label_website", "网站" }, { "about_label_license", "许可证" },
                { "about_label_special_thanks", "===== 特别感谢 =====" },
                { "about_special_thanks_content", "(Github) zzzzzz9125:\n     提供了 Vegas Pro 20 (官方 local_zh_CN.cfg) 文件\n     以及 Vegas Pro 21 ~ 23 (所有官方 cfg) 文件" }
            };
            Texts.Add("zh", zh);
        }
    }
}
