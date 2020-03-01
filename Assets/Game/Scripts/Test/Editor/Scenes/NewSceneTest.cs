using UnityEngine.SceneManagement;
using UnityEditor;
using NUnit.Framework;
using System.Collections.Generic;

namespace EllenExplorer.Editor.Tests {
    public class NewSceneTest {
        // This test depends on `DefaultTemplate` asset scene.
        string defaultSceneAssetTemplatePath = "Assets/Game/Scenes/Templates/__SceneForUnitTests__.unity";

        Scene scene;

        [TearDown]
        public void Cleanup() {
            // Remove generated scenes.
            if (!string.IsNullOrEmpty(scene.path)) {
                System.IO.File.Delete(scene.path);
                System.IO.File.Delete(scene.path + ".meta");
                AssetDatabase.Refresh();
            }
            
            // Close opened window.
            NewScene.Initialize().Close();

        }

        [OneTimeTearDown]
        public void CleanupInactiveScenesInBuildSettings() {
            // Some tests create temporary scenes and add it on build settings. Need be to remove.

            // Remove inactive scenes from build settings: Update Build Settings.
            EditorBuildSettingsScene[] currentBuildScenes = EditorBuildSettings.scenes;
            List<EditorBuildSettingsScene> enabledBuildScenes = new List<EditorBuildSettingsScene>();

            for (int i = 0; i < currentBuildScenes.Length; i++) {
                if (currentBuildScenes[i].enabled) {
                    // Fixed! The last scene of build settings don't add on enabled list.
                    if (i == currentBuildScenes.Length - 1) {
                        continue;
                    }

                    enabledBuildScenes.Add(currentBuildScenes[i]);
                }
            }

            EditorBuildSettingsScene[] newScenes = enabledBuildScenes.ToArray();
            EditorBuildSettings.scenes = newScenes;
        }

        [Test, Order(0)]
        public void ShouldShowNewSceneMenuItemWindow() {
            bool isExecuted = EditorApplication.ExecuteMenuItem("Ellen Explorer/New Scene");

            Assert.IsTrue(isExecuted);
        }

        [Test, Order(1)]
        public void ShouldCreateNewSceneAsset() {
            scene = NewScene.Initialize().CreateScene("__SceneForUnitTests__", defaultSceneAssetTemplatePath);
            
            Assert.IsTrue(scene.IsValid());
        }

        [Test, Order(2)]
        public void ShouldAddNewSceneToBuildSettings() {
            scene = NewScene.Initialize().CreateScene("__SceneForUnitTests__", defaultSceneAssetTemplatePath);
            
            NewScene.Initialize().AddSceneToBuildSettings(scene);
            scene = SceneManager.GetSceneByBuildIndex(scene.buildIndex);

            Assert.IsNotNull(scene.name);
        }
    }
}
