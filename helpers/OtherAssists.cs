using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBLXex_Rework.helpers {
    internal class OtherAssists {
        public static string GetSettingValue(string key) {
            string[] lines = File.ReadAllLines(MainWindow.settingsPath);

            for (int i = 0; i < lines.Length; i++) {
                string line = lines[i];
                string[] split = line.Split('=');

                string readKey = split[0];

                if (readKey == key) {
                    return split[1];
                }
            }

            return "";
        }

        public static void AssignSettingValue(string key, string value) {
            string recreatedFile = "";

            string[] lines = File.ReadAllLines(MainWindow.settingsPath);

            for (int i = 0; i < lines.Length; i++) {
                string line = lines[i];
                string[] split = line.Split('=');

                string readKey = split[0];
                string readValue = split[1];

                if (readKey == key) {
                    recreatedFile += $"{key}={value}\n";
                }
                else {
                    recreatedFile += $"{readKey}={readValue}\n";
                }
            }

            File.WriteAllText(MainWindow.settingsPath, recreatedFile);
        }
    }
}
