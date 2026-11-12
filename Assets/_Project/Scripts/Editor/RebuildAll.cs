using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GameOff.EditorTools
{
    /// <summary>
    /// Runs every builder in the right order after asset deliveries (P42), from the menu or with -executeMethod:
    /// textures and materials, station, exterior, documents and counter, then what sits on top of the station in Main, then the
    /// menu scenes that show it. The light bake is a separate step because it takes long: MainLightingSetup.SetupAndBakeNow.
    /// </summary>
    public static class RebuildAll
    {
        private const string MainScene = "Assets/_Project/Scenes/Main.unity";

        [MenuItem("GameOff/Rebuild All After Deliveries")]
        public static void FromMenu()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) Rebuild();
        }

        public static void Rebuild()
        {
            Debug.Log(DeliveryIntegrator.Run());
            var scene = EditorSceneManager.OpenScene(MainScene, OpenSceneMode.Single);
            StationGrayboxBuilder.Build();
            StationHabBuilder.Build();
            SkyExteriorBuilder.Build();
            EditorSceneManager.SaveScene(scene);
            Debug.Log(DocumentPrefabBuilder.Build());
            DayFlowBuilder.BuildIntoMainFromMenu();
            EconomyBuilder.BuildIntoMainFromMenu();
            DepartureBuilder.BuildIntoMainFromMenu();
            RestSpotBuilder.BuildIntoMainFromMenu(); // P28: needs the station, sky, day flow and economy
            TrafficBuilder.BuildIntoMainFromMenu();
            AmbienceBuilder.BuildIntoMainFromMenu();
            GuardRoundsBuilder.BuildIntoMainFromMenu(); // P148: needs the day flow and the player
            MenuSceneBuilder.BuildFromMenu();
            HelpOverlayBuilder.BuildIntoMainFromMenu(); // P140: last, it holds references to what the builders above made
            Debug.Log("[RebuildAll] Done. Bake the light next (GameOff/Setup Main Lighting + Bake).");
        }
    }
}
