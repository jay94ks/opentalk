using System;
using System.IO;
using System.Reflection;
using static System.Environment;

namespace OpenTalk
{
    public abstract partial class Application
    {
        public class Environments
        {
            static Environments()
            {
                ExecFile = Assembly.GetEntryAssembly().Location;
                ExecPath = Path.GetDirectoryName(ExecFile);

                SettingPath = DetermineWritablePath("settings");
                LoggingPath = DetermineWritablePath("logs");

                UserDataPath = DetermineWritablePath("datas", true);
                CachePath = DetermineWritablePath("caches", true);
                TempPath = DetermineWritablePath("temps", true);

                ModulePath = Path.Combine(ExecPath, "modules");

                if (!Directory.Exists(ModulePath))
                {
                    try
                    {
                        // 억지로 만들지는 안습니다.
                        // 실패하면 그뿐.
                        Directory.CreateDirectory(ModulePath);
                    }
                    catch { }
                }

                RemoveAllTempFiles();
            }

            /// <summary>
            /// 임시파일 경로 내에 존재하는 모든 파일들과 디렉터리들을 삭제합니다.
            /// </summary>
            /// <param name="PathName"></param>
            private static void RemoveAllTempFiles()
            {
                try
                {
                    Directory.Delete(TempPath, true);
                    Directory.CreateDirectory(TempPath);
                }
                catch { }
            }

            /// <summary>
            /// 캐쉬 파일들을 모두 삭제합니다.
            /// </summary>
            public static void CleanCacheFiles()
            {
                // 디렉토리는 남겨놓고 파일만 모두 지웁니다.
                foreach(FileInfo EachFile in (new DirectoryInfo(CachePath))
                    .GetFiles("*", SearchOption.AllDirectories))
                {
                    try { EachFile.Delete(); }
                    catch { }
                }
            }

            /// <summary>
            /// 실행 파일 경로를 획득합니다.
            /// </summary>
            public static string ExecFile { get; private set; }

            /// <summary>
            /// 실행 경로를 획득합니다.
            /// </summary>
            public static string ExecPath { get; private set; }

            /// <summary>
            /// 모듈 경로를 획득합니다.
            /// </summary>
            public static string ModulePath { get; private set; }

            /// <summary>
            /// 설정 파일이 저장되는 경로를 획득합니다.
            /// </summary>
            public static string SettingPath { get; private set; }

            /// <summary>
            /// 로그 파일들이 저장되는 경로를 획득합니다.
            /// </summary>
            public static string LoggingPath { get; private set; }

            /// <summary>
            /// 캐쉬 파일들이 저장되는 경로를 획득합니다.
            /// (윈도우 계정별로 다른 경로가 선정됩니다)
            /// </summary>
            public static string CachePath { get; private set; }

            /// <summary>
            /// 사용자 데이터 파일들이 저장되는 경로를 획득합니다.
            /// (윈도우 계정별로 다른 경로가 선정됩니다)
            /// </summary>
            public static string UserDataPath { get; private set; }

            /// <summary>
            /// 런타임 중에 생성되는 임시 파일들이 저장되는 경로를 획득합니다.
            /// (윈도우 계정별로 다른 경로가 선정됩니다)
            /// </summary>
            public static string TempPath { get; private set; }

            /// <summary>
            /// 쓰기 가능한 경로를 결정합니다.
            /// </summary>
            /// <param name="TargetName"></param>
            /// <returns></returns>
            private static string DetermineWritablePath(string TargetName, bool PerAccount = false)
            {
                string ChoosenPath = Path.Combine(ExecPath, TargetName);
                int Counter = 0;

                while (Counter <= 1)
                {
                    if (!PerAccount)
                    {
                        if (!Directory.Exists(ChoosenPath))
                        {
                            try
                            {
                                Directory.CreateDirectory(ChoosenPath);
                                continue;
                            }
                            catch { }
                        }

                        else
                        {
                            string tempFile = Path.Combine(ChoosenPath, "write-test.tmp");

                            try
                            {
                                File.Delete(tempFile);
                                File.WriteAllText(tempFile, "Test Write");
                                File.Delete(tempFile);
                                break;
                            }
                            catch { }
                        }
                    }

                    ChoosenPath = Path.Combine(
                        GetFolderPath(SpecialFolder.ApplicationData),
                        Path.GetFileNameWithoutExtension(ExecFile), 
                        TargetName);

                    Counter++;
                    PerAccount = true;
                }

                return ChoosenPath;
            }
        }
    }
}
