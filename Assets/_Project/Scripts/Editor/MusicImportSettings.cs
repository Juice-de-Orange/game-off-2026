using UnityEditor;
using UnityEngine;

namespace GameOff.EditorTools
{
    /// <summary>
    /// P154: the songs in Audio/Music are re-encoded for the web build at a lower quality. At the default 100 % each two-minute
    /// cassette song came out at about 2.8 MB, more than twice its source file, and the five of them were 40 % of the download.
    /// Applies on import, so deliveries get it too; the source files stay as delivered.
    /// </summary>
    public class MusicImportSettings : AssetPostprocessor
    {
        internal const string Folder = "Assets/_Project/Audio/Music/";
        internal const string Platform = "WebGL";
        internal const float WebQuality = 0.4f;

        // Raising this makes Unity re-import the music with the settings below.
        public override uint GetVersion() => 2;

        private void OnPreprocessAudio()
        {
            if (!assetPath.StartsWith(Folder)) return;
            var importer = (AudioImporter)assetImporter;
            var settings = importer.defaultSampleSettings; // the delivered settings, only the codec quality differs
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.quality = WebQuality;
            importer.SetOverrideSampleSettings(Platform, settings);
        }
    }
}
