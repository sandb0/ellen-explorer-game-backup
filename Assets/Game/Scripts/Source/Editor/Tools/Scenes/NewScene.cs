using UnityEngine.SceneManagement;

namespace EllenExplorer.Tools.Scenes {
    /**
     * NewScene: Expose class API.
     */
    public class NewScene {
        private static NewScene _instance = null;

        private NewScene() { }

        public static NewScene Instance {
            get {
                if (_instance == null) {
                    _instance = new NewScene();
                }

                return _instance;
            }
        }

        public Scene CreateScene(string sceneName, string sceneAssetTemplatePath, string placeInPath = "") {
            return NewSceneWindow.Initialize().CreateScene(sceneName, sceneAssetTemplatePath, placeInPath);
        }

        public void AddSceneToBuildSettings(Scene scene) {
            NewSceneWindow.Initialize().AddSceneToBuildSettings(scene);
        }

        public void CloseWindow() {
            NewSceneWindow.Initialize().CloseWindow();
        }
    }
}