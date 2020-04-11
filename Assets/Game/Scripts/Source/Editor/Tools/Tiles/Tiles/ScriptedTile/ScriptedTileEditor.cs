using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using System.Reflection;

namespace EllenExplorer.Tools.Tiles {
    #region @CLASSROOM: ScriptableObject e Inspector.
    /**
     * Todo ScriptableObject pode ter seu Inspector modificado para deixá-lo mais agradavel.
     * 
     * ScriptedTile estende de TileBase, que por sua vez, estende de ScriptableObject.
     * Logo TileBase e ScriptedTile possuem Inspectors customizáveis.
     * 
     * Então, todo ScriptableObject do tipo RuleTile terá seu Inspector customizado.
     */
    #endregion
    [CustomEditor(typeof(ScriptedTile), true)]
    [CanEditMultipleObjects]
    internal class ScriptedTileEditor : Editor {
        #region @CLASSROOM: `target`.
        /**
         * `target` vem do Editor.
         * É o ScriptableObject que esta sendo inspecionado no momento.
         * Neste caso o ScriptedTile.
         */
        #endregion
        private ScriptedTile scriptedTile => target as ScriptedTile;

        private PreviewRenderUtility previewRenderUtility;
        private Grid previewGrid;
        private List<Tilemap> previewTilemaps;
        private List<TilemapRenderer> previewTilemapRenderers;

        private ReorderableList reorderableListOfRules;

        // ReorderableList of Rules helper.
        private RLRulesHelper rlRules;

        #region @CLASSROOM: Sobre `OnEnable()`.
        /**
         * Quando o Asset é clicado, selecionado.
         */
        #endregion
        private void OnEnable() {
            rlRules = RLRulesHelper.Create();

            reorderableListOfRules = rlRules.CreateReorderableListOfRules(scriptedTile);
        }

        #region @CLASSROOM: Sobre `OnDisable()`.
        /**
         * Quando o Asset perde o foco. Quando outro Asset é clicado, selecionado.
         */
        #endregion
        private void OnDisable() {
            DestroyPreview();
        }

        #region Editor override methods implementation: The start execution.
        public override void OnInspectorGUI() {
            #region @CLASSROOM: `BeginChangeCheck()` e `EndChangeCheck()`.
            /**
             * Os métodos `BeginChangeCheck()` e `EndChangeCheck()` verificam se algum dos 'Fields' abaixo foi modificado.
             * Assim que qualquer 'Field' é alterado, o método `EndChangeCheck()` é executado.
             */
            #endregion
            EditorGUI.BeginChangeCheck();

            // Draw default fields.
            scriptedTile.defaultTileSprite = EditorGUILayout.ObjectField("Default Sprite", scriptedTile.defaultTileSprite, typeof(Sprite), false) as Sprite;
            scriptedTile.defaultTileGameObject = EditorGUILayout.ObjectField("Default Game Object", scriptedTile.defaultTileGameObject, typeof(GameObject), false) as GameObject;
            scriptedTile.defaultTileColliderType = (Tile.ColliderType)EditorGUILayout.EnumPopup("Default Collider Type", scriptedTile.defaultTileColliderType);

            EditorGUILayout.Space(20);

            // Draw ReorderableList of all Rules.
            if (reorderableListOfRules != null) {
                reorderableListOfRules.DoLayoutList();
            }

            if (EditorGUI.EndChangeCheck()) {
                rlRules.ForceRefreshTiles(scriptedTile);
            }
        }

        #region PreviewGUI.
        public override bool HasPreviewGUI() {
            return true;
        }

        public override void OnPreviewGUI(Rect previewRectangle, GUIStyle previewBackground) {
            if (previewRenderUtility == null) {
                CreatePreview();
            }

            if (Event.current.type != EventType.Repaint) {
                return;
            }

            previewRenderUtility.BeginPreview(previewRectangle, previewBackground);
            previewRenderUtility.camera.orthographicSize = 2;

            if (previewRectangle.height > previewRectangle.width) {
                previewRenderUtility.camera.orthographicSize *= previewRectangle.height / previewRectangle.width;
            }

            previewRenderUtility.camera.Render();
            previewRenderUtility.EndAndDrawPreview(previewRectangle);
        }

        #region PreviewGUI helpers.
        private void CreatePreview() {
            previewRenderUtility = new PreviewRenderUtility(true);
            previewRenderUtility.camera.orthographic = true;
            previewRenderUtility.camera.orthographicSize = 2;
            previewRenderUtility.camera.transform.position = new Vector3(0, 0, -10);

            GameObject previewGameObject = new GameObject();
            previewGrid = previewGameObject.AddComponent<Grid>();
            previewRenderUtility.AddSingleGO(previewGameObject);
            previewTilemaps = new List<Tilemap>();
            previewTilemapRenderers = new List<TilemapRenderer>();

            for (int tileIndex = 0; tileIndex < 4; tileIndex++) {
                GameObject previewTilemapGameObject = new GameObject();
                previewTilemaps.Add(previewTilemapGameObject.AddComponent<Tilemap>());
                previewTilemapRenderers.Add(previewTilemapGameObject.AddComponent<TilemapRenderer>());

                previewTilemapGameObject.transform.SetParent(previewGameObject.transform, false);
            }

            for (int x = -2; x <= 0; x++) {
                for (int y = -1; y <= 1; y++) {
                    previewTilemaps[0].SetTile(new Vector3Int(x, y, 0), scriptedTile);
                }
            }

            for (int y = -1; y <= 1; y++) {
                previewTilemaps[1].SetTile(new Vector3Int(1, y, 0), scriptedTile);
            }

            for (int x = -2; x <= 0; x++) {
                previewTilemaps[2].SetTile(new Vector3Int(x, -2, 0), scriptedTile);
            }

            previewTilemaps[3].SetTile(new Vector3Int(1, -2, 0), scriptedTile);
        }

        private void DestroyPreview() {
            if (previewRenderUtility != null) {
                previewRenderUtility.Cleanup();
                previewRenderUtility = null;
                previewGrid = null;
                previewTilemaps = null;
                previewTilemapRenderers = null;
            }
        }
        #endregion
        #endregion

        #region ScriptableObject icon.
        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height) {
            if (scriptedTile.defaultTileSprite != null) {
                return UseDefaultTileSpriteOnIconOfScriptableObject(width, height);
            }

            return base.RenderStaticPreview(assetPath, subAssets, width, height);
        }

        private Texture2D UseDefaultTileSpriteOnIconOfScriptableObject(int width, int height) {
            /*
             * Basicamente, o ícone do ScriptableObject (ScriptedTile) será modificado pelo `scriptedTile.defaultTileSprite`.
             */

            Type type = GetTypeByAssemblies("UnityEditor.SpriteUtility");

            if (type != null) {
                Type[] types = new Type[4] {
                    typeof(Sprite),
                    typeof(Color),
                    typeof(int),
                    typeof(int)
                };

                MethodInfo method = type.GetMethod("RenderStaticPreview", types);

                if (method != null) {
                    object[] parameters = new object[4] {
                        scriptedTile.defaultTileSprite,
                        Color.white,
                        width,
                        height
                    };

                    object icon = method.Invoke("RenderStaticPreview", parameters);

                    if (icon is Texture2D) {
                        return icon as Texture2D;
                    }
                }
            }

            return null;
        }
        #endregion
        #endregion

        private Type GetTypeByAssemblies(string typeName) {
            Type type = Type.GetType(typeName);

            if (type != null) {
                return type;
            }

            if (typeName.Contains(".")) {
                string assemblyName = typeName.Substring(0, typeName.IndexOf("."));
                Assembly assembly = Assembly.Load(assemblyName);

                if (assembly == null) {
                    return null;
                }

                type = assembly.GetType(typeName);

                if (type != null) {
                    return type;
                }
            }

            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            AssemblyName[] referencedAssemblies = currentAssembly.GetReferencedAssemblies();

            foreach (AssemblyName referencedAssemblyName in referencedAssemblies) {
                Assembly assembly = Assembly.Load(referencedAssemblyName);

                if (assembly != null) {
                    type = assembly.GetType(typeName);

                    if (type != null) {
                        return type;
                    }
                }
            }

            return null;
        }
    }
}