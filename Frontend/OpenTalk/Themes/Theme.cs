using Newtonsoft.Json;
using OpenTalk.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.Themes
{
    public class Theme
    {
        private static Theme m_Current;
        private static List<Theme> m_InstalledThemes = new List<Theme>();

        private class JsonImported
        {
            [JsonProperty("title")]
            public string Title { get; set; } = "UNTITLED";

            [JsonProperty("author")]
            public string Author { get; set; } = "";

            [JsonProperty("description")]
            public string Description { get; set; } = "";

            [JsonProperty("copyright")]
            public string Copyright { get; set; } = "";
        }

        /// <summary>
        /// 현재 OpenTalk 테마입니다.
        /// </summary>
        public static Theme CurrentTheme {
            get => m_Current;
            set {
                if (m_Current != value)
                {
                    if (m_Current != null && value != null)
                    {
                        if (m_Current.Directory.FullName ==
                            value.Directory.FullName)
                            return;
                    }

                    m_Current = value;

                    File.WriteAllText(
                        Path.Combine(Application.Environments.UserDataPath, "theme.dat"),
                        value.Name);

                    Future.RunForUI(() => ThemeChanged?.Invoke(m_Current));
                }
            }
        }

        /// <summary>
        /// 설치된 OpenTalk 테마들입니다.
        /// </summary>
        public static Theme[] InstalledThemes => m_InstalledThemes.Locked((X) => X.ToArray());

        /// <summary>
        /// 현재 테마가 변경되면 실행됩니다.
        /// </summary>
        public static event Action<Theme> ThemeChanged;

        /// <summary>
        /// Static Initializer!
        /// </summary>
        static Theme()
        {
            string ThemeSettingFile
                = Path.Combine(Application.Environments.UserDataPath, "theme.dat");

            string ThemeDirectory 
                = Path.Combine(Application.Environments.ExecPath, "Themes");

            if (!System.IO.Directory.Exists(ThemeDirectory))
                throw new ApplicationException();

            DirectoryInfo[] Directories = (new DirectoryInfo(ThemeDirectory)).GetDirectories();

            foreach(DirectoryInfo EachDirectory in Directories)
                m_InstalledThemes.Add(new Theme(EachDirectory));

            string ThemeName = "default";

            if (File.Exists(ThemeSettingFile))
                ThemeName = File.ReadAllText(ThemeSettingFile).ToLower();

            foreach (Theme Theme in m_InstalledThemes)
            {
                if (ThemeName == Theme.Name)
                    m_Current = Theme;
            }

            if (m_Current == null &&
                m_InstalledThemes.Count > 0)
                m_Current = m_InstalledThemes[0];
        }

        private JsonImported m_Imported;

        /// <summary>
        /// 실제 경로로부터 테마 객체를 생성합니다.
        /// </summary>
        /// <param name="Directory"></param>
        public Theme(DirectoryInfo Directory)
        {
            try
            {
                m_Imported = JsonConvert.DeserializeObject<JsonImported>(
                    Path.Combine(Directory.FullName, "Theme.json"));
            }
            catch
            {
                m_Imported = new JsonImported() {
                    Title = Directory.Name
                };
            }

            this.Directory = Directory;
        }

        /// <summary>
        /// 이 테마의 식별명을 가져옵니다.
        /// </summary>
        public string Name => Directory.Name.ToLower();

        /// <summary>
        /// 이 테마의 이름을 가져옵니다.
        /// </summary>
        public string Title => m_Imported.Title;

        /// <summary>
        /// 이 테마를 만든 사람을 데려(?)옵니다.
        /// </summary>
        public string Author => m_Imported.Author;

        /// <summary>
        /// 이 테마의 설명을 가져옵니다.
        /// </summary>
        public string Description => m_Imported.Description;

        /// <summary>
        /// 이 테마의 저작권 정보를 가져옵니다.
        /// </summary>
        public string Copyright => m_Imported.Copyright;

        /// <summary>
        /// 이 테마가 설치된 경로를 가져옵니다.
        /// </summary>
        public DirectoryInfo Directory { get; }
    }
}
