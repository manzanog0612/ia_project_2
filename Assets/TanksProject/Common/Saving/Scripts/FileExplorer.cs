using SFB;
using UnityEngine;

namespace Utilities
{
    public class FileExplorer : MonoBehaviour
    {
        private static ExtensionFilter[] extensions = new[]
        {
        new ExtensionFilter("json")
    };

        public static string SelectFile()
        {
            string[] paths = StandaloneFileBrowser.OpenFilePanel("Open File", Application.dataPath + "/" + "TanksProject/Common/Saving/Saves", extensions, false);
            return paths.Length == 0 ? null : paths[0];
        }
    }
}