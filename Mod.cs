using MelonLoader;
using UnityEngine;
using BrokenArrow;
using static UnityEngine.GraphicsBuffer;
using Harmony;

[assembly: MelonInfo(typeof(HelloWorld.Mod), "HelloWorld", "0.0.1", "Arrokoth7323")] // main mod class, name, version, author
[assembly: MelonGame("SteelBalalaikaStudio", "BrokenArrow")]

namespace HelloWorld
{
    public class Mod : MelonMod
    {
        private readonly string embeddedBundle = "HelloWorld.AssetBundles.helloworld.bundle";
        private readonly HashSet<int> swappedObjects = new();

        private AssetBundle? replacementBundle;
        private GameObject? replacementPrefab;

        public override void OnInitializeMelon()
        {
            MelonLogger.Msg($"Initializing '{Info.Name}'...");

            // --- Load replacement bundle once on startup ---
            var stream = MelonAssembly.Assembly.GetManifestResourceStream(embeddedBundle); // Get the AssetBundle Stream
            if (stream == null)
            {
                MelonLogger.Msg($"AssetBundle {embeddedBundle} does not exist.");
                return;
            }

            // Convert System.IO.Stream to Il2CppSystem.IO.Stream
            Il2CppSystem.IO.Stream il2CppStream = new Il2CppSystem.IO.MemoryStream();
            using (var memoryStream = new System.IO.MemoryStream())
            {
                stream.CopyTo(memoryStream);
                il2CppStream = new Il2CppSystem.IO.MemoryStream(memoryStream.ToArray());
            }

            replacementBundle = AssetBundle.LoadFromStream(il2CppStream); // Load the AssetBundle from the Il2CppStream
        }

        public override void OnUpdate()
        {
            // Check periodically if the original has spawned
            foreach (GameObject obj in GameObject.FindObjectsOfType<GameObject>())
            {
                if (obj.name.Contains("57") && !swappedObjects.Contains(obj.GetInstanceID()))
                {
                    SwapPrefab(obj);
                }
            }
        }

        private void SwapPrefab(GameObject original)
        {
            // --- Prevent double-swap ---
            swappedObjects.Add(original.GetInstanceID());

            MelonLogger.Msg($"Swapping spawned prefab: {original.name}");

            // Store transform info
            Transform parent = original.transform.parent;
            Vector3 position = original.transform.position;
            Quaternion rotation = original.transform.rotation;
            Vector3 scale = original.transform.localScale;

            // Destroy the original
            GameObject.Destroy(original);

            // Load replacement
            replacementPrefab = replacementBundle.LoadAsset<GameObject>("X-Wing_Prefab");
            if (replacementPrefab == null)
            {
                MelonLogger.Error("Replacement prefab not found in bundle!");
                replacementBundle.Unload(false);
                return;
            }

            // Instantiate replacement
            GameObject newObj = GameObject.Instantiate(replacementPrefab, position, rotation, parent);
            newObj.transform.localScale = scale;

            // --- Prevent double-swap ---
            swappedObjects.Add(newObj.GetInstanceID());

            MelonLogger.Msg("Prefab swapped successfully.");
        }

        public override void OnApplicationQuit()
        {
            // Unload when game exits
            if (replacementBundle != null)
                replacementBundle.Unload(false);
        }
    }

    // Helper marker so we don’t re-swap the same prefab
    public class PrefabSwapped : MonoBehaviour { }
}