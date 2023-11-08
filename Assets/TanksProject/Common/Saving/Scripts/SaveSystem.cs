using UnityEngine;

namespace TanksProject.Common.Saving
{
    public class SaveLoadSystem
    {
        #region PUBLIC_METHODS
        public static void SaveConfig(SimData data, string name = "")
        {
            JsonReadWriteSystem.SaveToJson(data, name);
        }

        public static SimData LoadSimFile()
        {
            string file = FileExplorer.SelectFile();

            if (file == null)
            {
                Debug.Log("No file select");
                return null;
            }

            JsonReadWriteSystem.LoadFromJson(out SimData config, file);
            return config;
        }
        #endregion
    }
}