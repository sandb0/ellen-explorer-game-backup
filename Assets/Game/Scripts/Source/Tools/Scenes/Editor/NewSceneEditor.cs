using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EllenExplorer.Tools.Scenes {
    /**
     * NewScene Editor implemetation.
     */
    public class NewSceneEditor : EditorWindow {
        private static NewSceneEditor Instance { set; get; }

        private readonly string sceneAssetsTemplatesFilter = "Template t:Scene";
        private readonly string[] sceneAssetsTemplatesFolders = new string[] { "Assets/Game/Scenes/Templates" };
        private enum SaveDialogOptions { Save = 0, DontSave = 1, Cancel = 2 }

        private readonly GUIContent sceneNameLabel = new GUIContent("Name");
        private readonly string sceneTemplateLabel = "Template";
        private readonly string placeNewSceneInPath = "Assets/Game/Scenes";

        private string sceneName;
        private string[] sceneAssetsTemplatesOptions;
        private int[] sceneAssetsTemplatesOptionsValues;
        private string[] sceneAssetsTemplatesPaths;
        private int sceneAssetTemplateSelected;

        [MenuItem("Ellen Explorer/New Scene", priority = 100)]
        public static NewSceneEditor Initialize() {
            if (Instance == null) {
                Instance = GetWindow<NewSceneEditor>();
            }
            
            Instance.Show();
            Instance.sceneName = "NewScene";

            return Instance;
        }

        private void OnGUI() {
            // Find all Scenes assets that have 'Template' in their filename, and are placed in 'Assets/Game/Scenes/Templates' folder.
            FindScenesAssets();

            GUILayout.Label("Create New Scene", EditorStyles.boldLabel);

            GUILayout.Space(10);

            sceneAssetTemplateSelected = EditorGUILayout.IntPopup(
                sceneTemplateLabel,
                sceneAssetTemplateSelected,
                sceneAssetsTemplatesOptions,
                sceneAssetsTemplatesOptionsValues
            );
            sceneName = EditorGUILayout.TextField(sceneNameLabel, sceneName);

            GUILayout.Space(10);

            if (GUILayout.Button("Create")) {
                CheckBeforeCreateScene();
            }
        }

        public Scene CreateScene(string sceneName, string sceneAssetTemplatePath, string placeInPath = "") {
            // `sceneAssetTemplatePath` is a valid `SceneAsset`?
            if (AssetDatabase.GetMainAssetTypeAtPath(sceneAssetTemplatePath) != typeof(SceneAsset)) {
                throw new UnityException("It's not possible to create a scene without a template.");
            }

            if (string.IsNullOrEmpty(placeInPath)) {
                placeInPath = placeNewSceneInPath;
            }

            string newScenePath = placeInPath + "/" + sceneName + ".unity";

            newScenePath = GetAvailableAssetName(newScenePath);

            AssetDatabase.CopyAsset(sceneAssetTemplatePath, newScenePath);
            AssetDatabase.Refresh();

            Scene scene = EditorSceneManager.OpenScene(newScenePath, OpenSceneMode.Single);

            Close();

            return scene;
        }

        public void AddSceneToBuildSettings(Scene scene) {
            // Get all scenes in build.
            EditorBuildSettingsScene[] currentBuildScenes = EditorBuildSettings.scenes;

            // Make a copy of scenes in build and, add more one element for the new scene.
            EditorBuildSettingsScene[] newBuildScenes = new EditorBuildSettingsScene[currentBuildScenes.Length + 1];

            for (int i = 0; i < currentBuildScenes.Length; i++) {
                newBuildScenes[i] = currentBuildScenes[i];
            }

            // Add the new scene in copy of scenes.
            newBuildScenes[currentBuildScenes.Length] = new EditorBuildSettingsScene(scene.path, true);

            // Set the new scenes copy to build scenes.
            EditorBuildSettings.scenes = newBuildScenes;
        }

        public void CloseWindow() {
            Close();
        }

        private void FindScenesAssets() {
            string[] templateScenesAssets;

            templateScenesAssets = AssetDatabase.FindAssets(sceneAssetsTemplatesFilter, sceneAssetsTemplatesFolders);
            sceneAssetsTemplatesOptions = new string[templateScenesAssets.Length];
            sceneAssetsTemplatesOptionsValues = new int[templateScenesAssets.Length];
            sceneAssetsTemplatesPaths = new string[templateScenesAssets.Length];

            if (templateScenesAssets.Length > 0) {
                for (int i = 0; i < templateScenesAssets.Length; i++) {
                    string assetPath = AssetDatabase.GUIDToAssetPath(templateScenesAssets[i]);
                    string assetName = Path.GetFileNameWithoutExtension(assetPath);

                    sceneAssetsTemplatesOptions[i] = assetName; // Scene template file name.
                    sceneAssetsTemplatesOptionsValues[i] = i; // Index position to scene tempalte asset path.
                    sceneAssetsTemplatesPaths[i] = assetPath; // Scene template asset path.
                }
            }
        }

        /**
         * Private class `CreateScene`.
         * Used to create a Scene using the local variables of the `NewSceneEditor` class.
         */
        private Scene CreateScene() {
            string sceneAssetTemplatePath = sceneAssetsTemplatesPaths[sceneAssetTemplateSelected];
            return CreateScene(sceneName, sceneAssetTemplatePath);
        }

        private void CheckBeforeCreateScene() {
            // Cannot create in play mode.
            if (Application.isPlaying) {
                Debug.LogWarning("Cannot create scenes in play mode. Exit play mode first.");
                return;
            }

            // Cannot create without a scene asset template.
            if (sceneAssetsTemplatesPaths.Length <= 0) {
                Debug.LogWarning("It's not possible to create a scene without a template.");
                return;
            }

            // Cannot create with empty `sceneName`.
            if (string.IsNullOrEmpty(sceneName)) {
                Debug.LogWarning("Please, enter a scene name before creating a scene.");
                return;
            }

            // If the current Scene has been modified, display a dialog.
            Scene currentActiveScene = SceneManager.GetActiveScene();

            if (currentActiveScene.isDirty) {
                string dialogTitle = currentActiveScene.name + " Scene Has Been Modified.";
                string dialogMessage = "Do you want to save the changes you made to " + currentActiveScene.name + " Scene?\n"
                    + "Changes will be lost if you dont't save them.";

                int dialog = EditorUtility.DisplayDialogComplex(dialogTitle, dialogMessage, "Save", "Don't Save", "Cancel");

                switch (dialog) {
                    case (int)SaveDialogOptions.Save:
                        EditorSceneManager.SaveScene(currentActiveScene);
                        break;

                    case (int)SaveDialogOptions.DontSave:
                    default:
                        return;
                }
            }

            // It's all right, create scene.
            Scene scene = CreateScene();
            AddSceneToBuildSettings(scene);
        }

        private string GetAvailableAssetName(string path, int internalCounter = 0) {
            // If the file exists, rename it.
            if (!string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(path))) {
                // Rename.
                path = placeNewSceneInPath + "/" + sceneName + "_" + internalCounter + ".unity";

                // Checks whether the new name also exists.
                path = GetAvailableAssetName(path, ++internalCounter);
            }

            return path;
        }

        private string GetAvailableAssetName(string path) {
            int random = Random.Range(1000, 5000);

            // If the file exists, rename it.
            if (!string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(path))) {
                // Rename.
                path = placeNewSceneInPath + "/" + sceneName + "_" + random + ".unity";
            }

            return path;
        }
    }
}