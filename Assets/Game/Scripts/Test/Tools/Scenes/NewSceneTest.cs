using UnityEngine.SceneManagement;
using UnityEditor;
using NUnit.Framework;
using System.Collections.Generic;

namespace EllenExplorer.Tools.Scenes.Tests {
    public class NewSceneTest {
        // This test depends on `__DO_NOT_DELETE__` scene asset.
        private string defaultSceneAssetTemplatePath = "Assets/Game/Scenes/Templates/__DO_NOT_DELETE__.unity";

        private Scene scene;
        
        [TearDown]
        public void Cleanup() {
            // Remove generated scenes.
            if (!string.IsNullOrEmpty(scene.path)) {
                System.IO.File.Delete(scene.path);
                System.IO.File.Delete(scene.path + ".meta");
                AssetDatabase.Refresh();
            }

            // Close opened window.
            NewScene.Instance.CloseWindow();
        }

        [OneTimeTearDown]
        public void Cleanup_Deleted_Scenes_In_BuildSettings() {
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

        [Test]
        public void Should_Show_NewScene_MenuItemWindow() {
            bool isExecuted = EditorApplication.ExecuteMenuItem("Ellen Explorer/New Scene");

            Assert.IsTrue(isExecuted);
        }

        [Test]
        public void Should_Create_New_Scene_Asset() {
            scene = NewScene.Instance.CreateScene("__SceneForUnitTests__", defaultSceneAssetTemplatePath);
            
            Assert.IsTrue(scene.IsValid());
        }

        [Test]
        public void Should_Add_Scene_To_BuildSettings() {
            scene = NewScene.Instance.CreateScene("__SceneForUnitTests__", defaultSceneAssetTemplatePath);
            
            NewScene.Instance.AddSceneToBuildSettings(scene);
            scene = SceneManager.GetSceneByBuildIndex(scene.buildIndex);

            Assert.IsNotNull(scene.name);
        }
    }
}
