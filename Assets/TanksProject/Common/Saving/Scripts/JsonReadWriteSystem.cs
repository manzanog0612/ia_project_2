using System.IO;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

namespace Utilities
{
    public static class JsonReadWriteSystem
    {
        private static string path = Application.dataPath + "/" + folder_name + "/";

        private static string folder_name = "saves";
        public static void SaveToJson(data.ConfigurationData dataToSave, string fileName = "")
        {

            data.ConfigurationData config = dataToSave;

            SavetoJsonGeneric(config, fileName);
        }
        public static void LoadFromJson(out data.ConfigurationData dataToLoad, string path)
        {
            string json = File.ReadAllText(path);

            dataToLoad = JsonUtility.FromJson<data.ConfigurationData>(json);
        }

        public static void LoadFromJson(out data.SimData dataToLoad, string path)
        {
            string json = File.ReadAllText(path);

            dataToLoad = JsonUtility.FromJson<data.SimData>(json);
        }

        public static void SaveToJson(data.SimData dataToSave, string fileName = "")
        {
            data.SimData config = dataToSave;

            SavetoJsonGeneric(config, fileName);
        }

        private static void SavetoJsonGeneric(object data,string fileName = "")
        {
            string json = JsonUtility.ToJson(data, true);

            if (!Directory.Exists(path)) { Debug.Log("Path not exist, lets create"); Directory.CreateDirectory(path); }

            if (fileName == "") { fileName = "file_" + Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly).Length; }

            File.WriteAllText(Application.dataPath + "/" + folder_name + "/" + fileName + ".json", json);
        }
    }

}
