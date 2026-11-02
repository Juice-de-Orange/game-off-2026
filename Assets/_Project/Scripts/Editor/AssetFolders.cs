using UnityEditor;

namespace GameOff.EditorTools
{
    /// <summary>Creates asset folders through the AssetDatabase, so CreateAsset works on a fresh checkout.</summary>
    internal static class AssetFolders
    {
        /// <summary>Ensures e.g. "Assets/_Project/Art/Materials/Hangar" exists (creates missing parents too).</summary>
        public static void Ensure(string folder)
        {
            folder = folder.Replace('\\', '/').TrimEnd('/');
            if (AssetDatabase.IsValidFolder(folder)) return;
            int slash = folder.LastIndexOf('/');
            string parent = folder.Substring(0, slash);
            Ensure(parent);
            AssetDatabase.CreateFolder(parent, folder.Substring(slash + 1));
        }

        public static void EnsureForAsset(string assetPath) => Ensure(System.IO.Path.GetDirectoryName(assetPath));
    }
}
