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
            /// 쓰기 가능한 경로를 결정합니다.
            /// </summary>
            /// <param name="TargetName"></param>
            /// <returns></returns>
            private static string DetermineWritablePath(string TargetName)
            {
                string ChoosenPath = Path.Combine(ExecPath, TargetName);
                int Counter = 0;

                while (Counter <= 1)
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

                    ChoosenPath = Path.Combine(
                        GetFolderPath(SpecialFolder.ApplicationData),
                        Path.GetFileNameWithoutExtension(ExecFile));

                    Counter++;
                }

                return ChoosenPath;
            }
        }
    }
}
