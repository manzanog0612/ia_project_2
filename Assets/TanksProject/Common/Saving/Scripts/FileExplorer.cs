using UnityEngine;

using SFB;

namespace TanksProject.Common.Saving
{
    public class FileExplorer : MonoBehaviour
    {
        #region PUBLIC_METHODS
        public static string SelectFile()
        {
            ExtensionFilter[] extensions = new[] { new ExtensionFilter("json") };

            string[] paths = StandaloneFileBrowser.OpenFilePanel("Open File", Application.dataPath + "/" + "TanksProject/Common/Saving/Saves", extensions, false);
            return paths.Length == 0 ? null : paths[0];
        }
        #endregion
    }
}