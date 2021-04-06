using System.IO;

namespace Entitas.Unity {
    public static class EntitasPreferences {

        // modify by chiuan
        // editor the properties file in path.
        const string CONFIG_PATH = "Assets/Lib/Entitas/Entitas.properties";

        public static EntitasPreferencesConfig LoadConfig() {
            return new EntitasPreferencesConfig(File.Exists(CONFIG_PATH) ? File.ReadAllText(CONFIG_PATH) : string.Empty);
        }

        public static void SaveConfig(EntitasPreferencesConfig config) {
            File.WriteAllText(CONFIG_PATH, config.ToString());
        }
    }
}