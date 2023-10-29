using UnityEngine;

namespace Utilities
{
    public class SaveLoadSystem
    {
        public static void SaveConfig(data.SimData data, string name = "")
        {
            JsonReadWriteSystem.SaveToJson(data, name);
        }

        public static data.SimData LoadSimFile()
        {
            string file = FileExplorer.SelectFile();

            if (file == null) 
            { 
                Debug.Log("No file select"); 
                return null; 
            }

            JsonReadWriteSystem.LoadFromJson(out data.SimData config, file);
            return config;
        }
    }
}