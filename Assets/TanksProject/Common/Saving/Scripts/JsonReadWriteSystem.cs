using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;

namespace TanksProject.Common.Saving
{
    public static class JsonReadWriteSystem
    {
        #region PRIVATE_FIELDS
        private static string path = Application.dataPath + "/" + "TanksProject/Common/Saving/Saves";
        #endregion

        #region PUBLIC_METHODS
        public static void LoadFromJson(out SimData dataToLoad, string path)
        {
            string json = File.ReadAllText(path);

            dataToLoad = JsonUtility.FromJson<SimData>(json);
        }

        public static void SaveToJson(SimData dataToSave, string fileName = "")
        {
            SimData config = dataToSave;

            SavetoJsonGeneric(config, fileName);
        }
        #endregion

        #region PRIVATE_METHODS
        private static void SavetoJsonGeneric(object data, string fileName = "")
        {
            string json = JsonUtility.ToJson(data, true);

            if (!Directory.Exists(path))
            {
                Debug.Log("Path not exist, lets create");
                Directory.CreateDirectory(path);
            }

            if (fileName == "")
            {
                List<string> files = Directory.GetFiles(path + "/", "*", SearchOption.TopDirectoryOnly).ToList();

                files.RemoveAll(f => f.Contains(".meta"));
                fileName = "file_" + files.Count;
            }

            File.WriteAllText(path + "/" + fileName + ".json", json);
        }
        #endregion
    }

}
