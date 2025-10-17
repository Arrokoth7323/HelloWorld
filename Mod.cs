using MelonLoader;
using UnityEngine;
using BrokenArrow;
using static UnityEngine.GraphicsBuffer;

[assembly: MelonInfo(typeof(HelloWorld.Mod), "HelloWorld", "0.0.1", "Arrokoth7323")] // main mod class, name, version, author
[assembly: MelonGame("SteelBalalaikaStudio", "BrokenArrow")]

namespace HelloWorld
{
    public class Mod : MelonMod
    {
        private readonly string targetScene = "Hangar_Scene";

        public override void OnInitializeMelon()
        {
            MelonLogger.Msg($"Initializing '{Info.Name}'...");
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            // Only spawn in target scene
            if (sceneName.Equals(targetScene, System.StringComparison.OrdinalIgnoreCase))
            {
                MelonLogger.Msg($"Scene '{sceneName}' loaded — proceeding').");

                MelonCoroutines.Start(DuplicateObject());
            }
            else
            {
                MelonLogger.Msg($"Scene '{sceneName}' loaded — skipping spawn (target is '{targetScene}').");
            }
        }

        private System.Collections.IEnumerator DuplicateObject()
        {
            // Wait a frame to make sure everything is loaded
            yield return new WaitForSeconds(10f);

            // Find the prefab in the scene (by name)
            GameObject original = GameObject.Find("RQ4B_BASE");
            if (original == null)
            {
                MelonLogger.Warning("Original prefab not found in scene!");
                yield break;
            }

            GameObject.Destroy(original);

            MelonLogger.Msg($"Destroyed '{original.name}'");
        }
    }
}