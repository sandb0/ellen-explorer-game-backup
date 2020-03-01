using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

namespace EllenExplorer {
    namespace Editor {
        public class NewScene : EditorWindow {
            private string sceneName;

            private enum SaveDialogOptions { Save = 0, DontSave = 1, Cancel = 2 }

            private readonly GUIContent sceneNameLabel = new GUIContent("Name");
            private readonly string sceneTemplateLabel = "Template";

            private readonly string sceneAssetsTemplatesFilter = "Template t:Scene";
            private readonly string[] sceneAssetsTemplatesFolders = new string[] { "Assets/Game/Scenes/Templates" };

            private string[] sceneAssetsTemplatesOptions;
            private int[] sceneAssetsTemplatesOptionsValues;
            private string[] sceneAssetsTemplatesPaths;
            private int sceneAssetTemplateSelected;

            [MenuItem("Ellen Explorer/New Scene", priority = 100)]
            public static void MenuItem() {
                NewScene window = GetWindow<NewScene>();
                window.Show();
                
                window.sceneName = "NewScene";
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
                CreateScene();
            }

            private void CreateScene() {
                string sceneTemplatePath = sceneAssetsTemplatesPaths[sceneAssetTemplateSelected];
                string newScenePath = "Assets/Game/Scenes/" + sceneName + ".unity";

                newScenePath = NoReplaceFileByPath(newScenePath);

                AssetDatabase.CopyAsset(sceneTemplatePath, newScenePath);
                AssetDatabase.Refresh();

                Scene scene = EditorSceneManager.OpenScene(newScenePath, OpenSceneMode.Single);
                AddSceneToBuildSettings(scene);

                Close();
            }

            private void AddSceneToBuildSettings(Scene scene) {
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

            private string NoReplaceFileByPath(string path, int internalCounter = 0) {                
                // If the file exists, rename it.
                if (!string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(path))) {
                    // Rename.
                    path = "Assets/Game/Scenes/" + sceneName + "_" + internalCounter + ".unity";

                    // Checks whether the new name also exists.
                    path = NoReplaceFileByPath(path, ++internalCounter);
                }

                return path;
            }

            private string NoReplaceFileByPath(string path) {
                int random = Random.Range(1000, 5000);

                // If the file exists, rename it.
                if (!string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(path))) {
                    // Rename.
                    path = "Assets/Game/Scenes/" + sceneName + "_" + random + ".unity";
                }

                return path;
            }
        }
    }
}
