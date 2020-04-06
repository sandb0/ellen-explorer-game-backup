using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

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
        public ScriptedTile scriptedTile => target as ScriptedTile;

        public ReorderableList reorderableListOfAllRules;

        private static class ReorderableListGUIDefaults {
            public const float ComponentWidth = 48;
            public const float FieldWidth = 80f;
            public const float FieldHeight = 18f;
            public const float FieldPaddingTop = 1f;

            public const float ElementHeight = 48;
            public const float ElementPaddingHeight = 26;
        }

        #region @CLASSROOM: Sobre `OnEnable()`.
        /**
         * Quando o Asset é clicado, selecionado.
         */
        #endregion
        private void OnEnable() {
            CreateReorderableListOfAllRules();
        }

        #region @CLASSROOM: Sobre `OnDisable()`.
        /**
         * Quando o Asset perde o foco. Quando outro Asset é clicado, selecionado.
         */
        #endregion
        private void OnDisable() {
        }

        #region Editor override methods implementation.
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
            if (reorderableListOfAllRules != null) {
                reorderableListOfAllRules.DoLayoutList();
            }

            if (EditorGUI.EndChangeCheck()) {
                ForceRefreshTileOfTileBase();
            }
        }

        public override bool HasPreviewGUI() {
            return base.HasPreviewGUI();
        }

        public override void OnPreviewGUI(Rect rect, GUIStyle background) {
        }

        //public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height) {
        //    return base.RenderStaticPreview(assetPath, subAssets, width, height);
        //}
        #endregion

        #region ReorderableList methods.
        private void CreateReorderableListOfAllRules() {
            bool draggable = true;
            bool displayHeader = true;
            bool displayAddButton = true;
            bool displayRemoveButton = true;

            reorderableListOfAllRules = new ReorderableList(
                scriptedTile.rules,
                typeof(ScriptedTile.Rule),
                draggable,
                displayHeader,
                displayAddButton,
                displayRemoveButton
            );

            reorderableListOfAllRules.drawHeaderCallback = OnDrawReorderableListHeader;
            reorderableListOfAllRules.drawElementCallback = OnDrawReorderableListElement;
            reorderableListOfAllRules.elementHeightCallback = GetReorderableListElementHeight;
            reorderableListOfAllRules.onChangedCallback = OnReorderableListUpdated;
            reorderableListOfAllRules.onAddCallback = OnAddElementInReorderableList;
        }

        private void OnDrawReorderableListHeader(Rect rectangle) {
            GUI.Label(rectangle, "Rules");
        }

        /**
         * Para cada Rule, este método será executado.
         * 
         * Em cada elemento tem 3 componentes: Tile data, Matrix e Sprite.
         */
        private void OnDrawReorderableListElement(Rect elementRectangle, int ruleIndex, bool isActive, bool isFocused) {
            ScriptedTile.Rule rule = scriptedTile.rules[ruleIndex];

            DrawTileDataComponent(elementRectangle, rule);
            DrawSpriteComponent(elementRectangle, rule);
        }

        #region ReorderableList element draws components.
        private void DrawSpriteComponent(Rect elementRectangle, ScriptedTile.Rule rule) {
            float elementStartPositionY = elementRectangle.yMin;
            float elementMaxWidth = elementRectangle.xMax;

            float spriteComponentPositionX = elementMaxWidth - ReorderableListGUIDefaults.ComponentWidth;
            float spriteComponentPositionY = elementStartPositionY + ReorderableListGUIDefaults.FieldPaddingTop;
            float spriteComponentWidth = ReorderableListGUIDefaults.ComponentWidth;
            float spriteComponentHeight = ReorderableListGUIDefaults.ElementHeight;

            Rect spriteComponentRectangle = new Rect(spriteComponentPositionX, spriteComponentPositionY, spriteComponentWidth, spriteComponentHeight);

            rule.tileSprites[0] = EditorGUI.ObjectField(spriteComponentRectangle, rule.tileSprites[0], typeof(Sprite), false) as Sprite;
        }

        private void DrawTileDataComponent(Rect elementRectangle, ScriptedTile.Rule rule) {
            float elementStartPositionX = elementRectangle.xMin;
            float elementStartPositionY = elementRectangle.yMin;

            // Separar o espaço deste componente dentro do element do ReorderableList.
            Rect componentRectangle = new Rect(
                elementStartPositionX,
                elementStartPositionY,
                elementRectangle.width - 120f, // TODO!!
                elementRectangle.height - ReorderableListGUIDefaults.ElementPaddingHeight
            );

            float componentPositionX = componentRectangle.x;
            float fieldPositionY = componentRectangle.y;

            /*
             * Inicia o `fieldOrderNumber`.
             * Ele controla a quantidade de padding que cada Field precisa.
             * A quantidade de padding é baseada na ordem (1º, 2º, ...) do Field.
             */
            int fieldOrderNumber = UpdateFieldOrderNumber(0);

            #region GameObject Field.
            Rect gameObjectLabelRectangle = new Rect(
                componentPositionX,
                fieldPositionY + GetFieldPaddingTopByFieldPositionY(fieldPositionY, fieldOrderNumber),
                ReorderableListGUIDefaults.FieldWidth,
                ReorderableListGUIDefaults.FieldHeight
            );

            Rect gameObjectFieldRectangle = new Rect(
                componentPositionX + ReorderableListGUIDefaults.FieldWidth,
                fieldPositionY + GetFieldPaddingTopByFieldPositionY(fieldPositionY, fieldOrderNumber),
                /*
                 * `componentRectangle.width`: A largura do Field é proporcional a largura do element do ReorderableList,
                 * pois, `componentRectangle` é baseado no elementRectangle.
                 */
                componentRectangle.width - ReorderableListGUIDefaults.FieldWidth,
                ReorderableListGUIDefaults.FieldHeight
            );

            GUI.Label(gameObjectLabelRectangle, "Game Object");
            rule.tileGameObject = EditorGUI.ObjectField(gameObjectFieldRectangle, "", rule.tileGameObject, typeof(GameObject), false) as GameObject;

            fieldPositionY = UpdateFieldPositionY(fieldPositionY);
            fieldOrderNumber = UpdateFieldOrderNumber(fieldOrderNumber);
            #endregion

            #region ColliderType Field.
            Rect colliderTypeLabelRectangle = new Rect(
                componentPositionX,
                fieldPositionY + GetFieldPaddingTopByFieldPositionY(fieldPositionY, fieldOrderNumber),
                ReorderableListGUIDefaults.FieldWidth,
                ReorderableListGUIDefaults.FieldHeight
            );

            Rect colliderTypeFieldRectangle = new Rect(
                componentPositionX + ReorderableListGUIDefaults.FieldWidth,
                fieldPositionY + GetFieldPaddingTopByFieldPositionY(fieldPositionY, fieldOrderNumber),
                /*
                 * `componentRectangle.width`: A largura do Field é proporcional a largura do element do ReorderableList,
                 * pois, `componentRectangle` é baseado no elementRectangle.
                 */
                componentRectangle.width - ReorderableListGUIDefaults.FieldWidth,
                ReorderableListGUIDefaults.FieldHeight
            );

            GUI.Label(colliderTypeLabelRectangle, "Collider Type");
            rule.tileColliderType = (Tile.ColliderType)EditorGUI.EnumPopup(colliderTypeFieldRectangle, rule.tileColliderType);

            fieldPositionY = UpdateFieldPositionY(fieldPositionY);
            fieldOrderNumber = UpdateFieldOrderNumber(fieldOrderNumber);
            #endregion

            #region SpriteOutputType Field.
            Rect spriteOutputTypeLabelRectangle = new Rect(
                componentPositionX,
                fieldPositionY + GetFieldPaddingTopByFieldPositionY(fieldPositionY, fieldOrderNumber),
                ReorderableListGUIDefaults.FieldWidth,
                ReorderableListGUIDefaults.FieldHeight
            );

            Rect spriteOutputTypeFieldRectangle = new Rect(
                componentPositionX + ReorderableListGUIDefaults.FieldWidth,
                fieldPositionY + GetFieldPaddingTopByFieldPositionY(fieldPositionY, fieldOrderNumber),
                /*
                 * `componentRectangle.width`: A largura do Field é proporcional a largura do element do ReorderableList,
                 * pois, `componentRectangle` é baseado no elementRectangle.
                 */
                componentRectangle.width - ReorderableListGUIDefaults.FieldWidth,
                ReorderableListGUIDefaults.FieldHeight
            );

            GUI.Label(spriteOutputTypeLabelRectangle, "Sprite Output");
            rule.spriteOutputType = (ScriptedTile.RuleOutput.SpriteOutputType)EditorGUI.EnumPopup(spriteOutputTypeFieldRectangle, rule.spriteOutputType);

            fieldPositionY = UpdateFieldPositionY(fieldPositionY);
            fieldOrderNumber = UpdateFieldOrderNumber(fieldOrderNumber);
            #endregion

            if (rule.spriteOutputType == ScriptedTile.RuleOutput.SpriteOutputType.Animation) {
                #region AnimationSpeed Field.
                Rect animationSpeedLabelRectangle = new Rect(
                    componentPositionX,
                    fieldPositionY + GetFieldPaddingTopByFieldPositionY(fieldPositionY, fieldOrderNumber),
                    ReorderableListGUIDefaults.FieldWidth,
                    ReorderableListGUIDefaults.FieldHeight
                );

                Rect animationSpeedFieldRectangle = new Rect(
                    componentPositionX + ReorderableListGUIDefaults.FieldWidth,
                    fieldPositionY + GetFieldPaddingTopByFieldPositionY(fieldPositionY, fieldOrderNumber),
                    /*
                     * `componentRectangle.width`: A largura do Field é proporcional a largura do element do ReorderableList,
                     * pois, `componentRectangle` é baseado no elementRectangle.
                     */
                    componentRectangle.width - ReorderableListGUIDefaults.FieldWidth,
                    ReorderableListGUIDefaults.FieldHeight
                );

                GUI.Label(animationSpeedLabelRectangle, "Speed");
                rule.animationSpeed = EditorGUI.FloatField(animationSpeedFieldRectangle, rule.animationSpeed);

                fieldPositionY = UpdateFieldPositionY(fieldPositionY);
                fieldOrderNumber = UpdateFieldOrderNumber(fieldOrderNumber);
                #endregion
            }

            if (rule.spriteOutputType == ScriptedTile.RuleOutput.SpriteOutputType.Random) {
                #region PerlinNoise Field.
                Rect animationSpeedLabelRectangle = new Rect(
                    componentPositionX,
                    fieldPositionY + GetFieldPaddingTopByFieldPositionY(fieldPositionY, fieldOrderNumber),
                    ReorderableListGUIDefaults.FieldWidth,
                    ReorderableListGUIDefaults.FieldHeight
                );

                Rect animationSpeedFieldRectangle = new Rect(
                    componentPositionX + ReorderableListGUIDefaults.FieldWidth,
                    fieldPositionY + GetFieldPaddingTopByFieldPositionY(fieldPositionY, fieldOrderNumber),
                    /*
                     * `componentRectangle.width`: A largura do Field é proporcional a largura do element do ReorderableList,
                     * pois, `componentRectangle` é baseado no elementRectangle.
                     */
                    componentRectangle.width - ReorderableListGUIDefaults.FieldWidth,
                    ReorderableListGUIDefaults.FieldHeight
                );

                GUI.Label(animationSpeedLabelRectangle, "Perlin Noise");
                rule.perlinNoise = EditorGUI.Slider(animationSpeedFieldRectangle, rule.perlinNoise, 0.001f, 0.999f);

                fieldPositionY = UpdateFieldPositionY(fieldPositionY);
                fieldOrderNumber = UpdateFieldOrderNumber(fieldOrderNumber);
                #endregion

                #region RandomRotationType Field.
                Rect randomRotationTypeLabelRectangle = new Rect(
                    componentPositionX,
                    fieldPositionY + GetFieldPaddingTopByFieldPositionY(fieldPositionY, fieldOrderNumber),
                    ReorderableListGUIDefaults.FieldWidth,
                    ReorderableListGUIDefaults.FieldHeight
                );

                Rect randomRotationTypeFieldRectangle = new Rect(
                    componentPositionX + ReorderableListGUIDefaults.FieldWidth,
                    fieldPositionY + GetFieldPaddingTopByFieldPositionY(fieldPositionY, fieldOrderNumber),
                    /*
                     * `componentRectangle.width`: A largura do Field é proporcional a largura do element do ReorderableList,
                     * pois, `componentRectangle` é baseado no elementRectangle.
                     */
                    componentRectangle.width - ReorderableListGUIDefaults.FieldWidth,
                    ReorderableListGUIDefaults.FieldHeight
                );

                GUI.Label(randomRotationTypeLabelRectangle, "Rotation");
                rule.randomRotationType = (ScriptedTile.Rule.RandomRotationType)EditorGUI.EnumPopup(randomRotationTypeFieldRectangle, rule.randomRotationType);

                fieldPositionY = UpdateFieldPositionY(fieldPositionY);
                fieldOrderNumber = UpdateFieldOrderNumber(fieldOrderNumber);
                #endregion
            }

            // Lista de Sprites para a animação ou, para a randomização.
            if (rule.spriteOutputType != ScriptedTile.RuleOutput.SpriteOutputType.Single) {
                #region Sprites Fields.
                Rect spritesNumberLabelRectangle = new Rect(
                    componentPositionX,
                    fieldPositionY + GetFieldPaddingTopByFieldPositionY(fieldPositionY, fieldOrderNumber),
                    ReorderableListGUIDefaults.FieldWidth,
                    ReorderableListGUIDefaults.FieldHeight
                );

                Rect spritesNumberFieldRectangle = new Rect(
                    componentPositionX + ReorderableListGUIDefaults.FieldWidth,
                    fieldPositionY + GetFieldPaddingTopByFieldPositionY(fieldPositionY, fieldOrderNumber),
                    /*
                     * `componentRectangle.width`: A largura do Field é proporcional a largura do element do ReorderableList,
                     * pois, `componentRectangle` é baseado no elementRectangle.
                     */
                    componentRectangle.width - ReorderableListGUIDefaults.FieldWidth,
                    ReorderableListGUIDefaults.FieldHeight
                );

                EditorGUI.BeginChangeCheck();

                GUI.Label(spritesNumberLabelRectangle, "Sprites");
                int spritesNumber = EditorGUI.DelayedIntField(spritesNumberFieldRectangle, rule.tileSprites.Length);

                // Modificar a lista de Sprites quando o Field acima (a quantidade de Sprites) for modificado.
                if (EditorGUI.EndChangeCheck()) {
                    // Deve ter pelo menos um campo.
                    Array.Resize(ref rule.tileSprites, Mathf.Max(spritesNumber, 1));
                }

                fieldPositionY = UpdateFieldPositionY(fieldPositionY);
                fieldOrderNumber = UpdateFieldOrderNumber(fieldOrderNumber);

                for (int spriteIndex = 0; spriteIndex < rule.tileSprites.Length; spriteIndex++) {
                    Rect spriteFieldRectangle = new Rect(
                        componentPositionX + ReorderableListGUIDefaults.FieldWidth,
                        fieldPositionY + GetFieldPaddingTopByFieldPositionY(fieldPositionY, fieldOrderNumber),
                        /*
                         * `componentRectangle.width`: A largura do Field é proporcional a largura do element do ReorderableList,
                         * pois, `componentRectangle` é baseado no elementRectangle.
                         */
                        componentRectangle.width - ReorderableListGUIDefaults.FieldWidth,
                        ReorderableListGUIDefaults.FieldHeight
                    );

                    rule.tileSprites[spriteIndex] = EditorGUI.ObjectField(spriteFieldRectangle, rule.tileSprites[spriteIndex], typeof(Sprite), false) as Sprite;

                    fieldPositionY = UpdateFieldPositionY(fieldPositionY);
                    fieldOrderNumber = UpdateFieldOrderNumber(fieldOrderNumber);
                }
                #endregion
            }
        }
        #endregion

        private float GetReorderableListElementHeight(int ruleIndex) {
            ScriptedTile.Rule rule = scriptedTile.rules[ruleIndex];

            BoundsInt bounds = GetMatrixBoundsForGUI(rule.GetMatrixBounds());

            float inspectorHeight = ReorderableListGUIDefaults.ElementHeight + ReorderableListGUIDefaults.ElementPaddingHeight;
            float matrixHeight = GetMatrixSize(bounds).y + 10f;

            // Quem é maior o Matrix ou o element Inspector?
            return Mathf.Max(inspectorHeight, matrixHeight) + 300f;
        }

        private void OnReorderableListUpdated(ReorderableList list) { }

        /**
         * Ao adicionar uma nova Rule na lista de Rule: `ScriptedTile.rules`.
         * A `reorderableListOfAllRules` usa esta lista para renderizar cada Rule no Inspector.
         */
        private void OnAddElementInReorderableList(ReorderableList list) {
            ScriptedTile.Rule rule = new ScriptedTile.Rule();

            // Use default values.
            rule.tileSprites[0] = scriptedTile.defaultTileSprite;
            rule.tileGameObject = scriptedTile.defaultTileGameObject;
            rule.tileColliderType = scriptedTile.defaultTileColliderType;
            rule.spriteOutputType = ScriptedTile.RuleOutput.SpriteOutputType.Single;

            scriptedTile.rules.Add(rule);
        }
        #endregion

        private void ForceRefreshTileOfTileBase() {
            /**
             * This method force the `TileBase.RefreshTile()`.
             * Then the `ScriptedTile.GetTileData()` will be executed for all Tiles.
             */
            EditorUtility.SetDirty(target);
            SceneView.RepaintAll();
        }

        private BoundsInt GetMatrixBoundsForGUI(BoundsInt bounds) {
            // Criar o GUI padrão, 3x3; de -1 até 1 (-1, 0, 1).
            bounds.xMin = Mathf.Min(bounds.xMin, -1);
            bounds.yMin = Mathf.Min(bounds.yMin, -1);
            bounds.xMax = Mathf.Max(bounds.xMax, 2); // Por que 2 e não 3?
            bounds.xMax = Mathf.Max(bounds.xMax, 2); // Por que 2 e não 3?

            return bounds;
        }

        private Vector2 GetMatrixSize(BoundsInt bounds) {
            return new Vector2(
                bounds.size.x * ReorderableListGUIDefaults.FieldHeight,
                bounds.size.y * ReorderableListGUIDefaults.FieldHeight
            );
        }

        private float GetFieldPaddingTopByFieldPositionY(float fieldPositionY, int fieldOrderNumber) {
            if (fieldOrderNumber == 1 || fieldOrderNumber == 2) {
                return ReorderableListGUIDefaults.FieldPaddingTop * fieldOrderNumber;
            }

            int fieldOrderNumberMultiplier = 2;

            for (int i = 3; i <= fieldOrderNumber; i++) {
                fieldOrderNumberMultiplier += 1;
            }

            return ReorderableListGUIDefaults.FieldPaddingTop * fieldOrderNumberMultiplier;
        }

        private int UpdateFieldOrderNumber(int fieldOrderNumber) {
            return fieldOrderNumber += 1;
        }

        private float UpdateFieldPositionY(float fieldPositionY) {
            return fieldPositionY += ReorderableListGUIDefaults.FieldHeight;
        }
    }
}