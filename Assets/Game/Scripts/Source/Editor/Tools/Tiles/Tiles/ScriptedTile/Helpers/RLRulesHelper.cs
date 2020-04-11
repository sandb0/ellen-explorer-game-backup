using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace EllenExplorer.Tools.Tiles {
    public class RLRulesHelper {
        private static class ReorderableListGUIDefaults {
            public const float ComponentWidth = 48;
            public const float FieldWidth = 90f;
            public const float FieldHeight = 18f;
            public const float FieldPaddingTop = 1f;

            public const float ElementHeight = 48;
            public const float ElementPaddingHeight = 26;
        }

        private ScriptedTile scriptedTile;
        private List<int> neighborRuleCodes;

        private const string xIcon = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAABoSURBVDhPnY3BDcAgDAOZhS14dP1O0x2C/LBEgiNSHvfwyZabmV0jZRUpq2zi6f0DJwdcQOEdwwDLypF0zHLMa9+NQRxkQ+ACOT2STVw/q8eY1346ZlE54sYAhVhSDrjwFymrSFnD2gTZpls2OvFUHAAAAABJRU5ErkJggg==";
        private const string arrowIcon0 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAACYSURBVDhPzZExDoQwDATzE4oU4QXXcgUFj+YxtETwgpMwXuFcwMFSRMVKKwzZcWzhiMg91jtg34XIntkre5EaT7yjjhI9pOD5Mw5k2X/DdUwFr3cQ7Pu23E/BiwXyWSOxrNqx+ewnsayam5OLBtbOGPUM/r93YZL4/dhpR/amwByGFBz170gNChA6w5bQQMqramBTgJ+Z3A58WuWejPCaHQAAAABJRU5ErkJggg==";
        private const string arrowIcon1 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAABqSURBVDhPxYzBDYAgEATpxYcd+PVr0fZ2siZrjmMhFz6STIiDs8XMlpEyi5RkO/d66TcgJUB43JfNBqRkSEYDnYjhbKD5GIUkDqRDwoH3+NgTAw+bL/aoOP4DOgH+iwECEt+IlFmkzGHlAYKAWF9R8zUnAAAAAElFTkSuQmCC";
        private const string arrowIcon2 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAAC0SURBVDhPjVE5EsIwDMxPKFKYF9CagoJH8xhaMskLmEGsjOSRkBzYmU2s9a58TUQUmCH1BWEHweuKP+D8tphrWcAHuIGrjPnPNY8X2+DzEWE+FzrdrkNyg2YGNNfRGlyOaZDJOxBrDhgOowaYW8UW0Vau5ZkFmXbbDr+CzOHKmLinAXMEePyZ9dZkZR+s5QX2O8DY3zZ/sgYcdDqeEVp8516o0QQV1qeMwg6C91toYoLoo+kNt/tpKQEVvFQAAAAASUVORK5CYII=";
        private const string arrowIcon3 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAAB2SURBVDhPzY1LCoAwEEPnLi48gW5d6p31bH5SMhp0Cq0g+CCLxrzRPqMZ2pRqKG4IqzJc7JepTlbRZXYpWTg4RZE1XAso8VHFKNhQuTjKtZvHUNCEMogO4K3BhvMn9wP4EzoPZ3n0AGTW5fiBVzLAAYTP32C2Ay3agtu9V/9PAAAAAElFTkSuQmCC";
        private const string arrowIcon4 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAABqSURBVDhPnY3BCYBADASvFx924NevRdvbyoLBmNuDJQMDGjNxAFhK1DyUQ9fvobCdO+j7+sOKj/uSB+xYHZAxl7IR1wNTXJeVcaAVU+614uWfCT9mVUhknMlxDokd15BYsQrJFHeUQ0+MB5ErsPi/6hO1AAAAAElFTkSuQmCC";
        private const string arrowIcon5 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAACaSURBVDhPxZExEkAwEEVzE4UiTqClUDi0w2hlOIEZsV82xCZmQuPPfFn8t1mirLWf7S5flQOXjd64vCuEKWTKVt+6AayH3tIa7yLg6Qh2FcKFB72jBgJeziA1CMHzeaNHjkfwnAK86f3KUafU2ClHIJSzs/8HHLv09M3SaMCxS7ljw/IYJWzQABOQZ66x4h614ahTCL/WT7BSO51b5Z5hSx88AAAAAElFTkSuQmCC";
        private const string arrowIcon6 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAABQSURBVDhPYxh8QNle/T8U/4MKEQdAmsz2eICx6W530gygr2aQBmSMphkZYxqErAEXxusKfAYQ7XyyNMIAsgEkaYQBkAFkaYQBsjXSGDAwAAD193z4luKPrAAAAABJRU5ErkJggg==";
        private const string arrowIcon7 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAACYSURBVDhPxZE9DoAwCIW9iUOHegJXHRw8tIdx1egJTMSHAeMPaHSR5KVQ+KCkCRF91mdz4VDEWVzXTBgg5U1N5wahjHzXS3iFFVRxAygNVaZxJ6VHGIl2D6oUXP0ijlJuTp724FnID1Lq7uw2QM5+thoKth0N+GGyA7IA3+yM77Ag1e2zkey5gCdAg/h8csy+/89v7E+YkgUntOWeVt2SfAAAAABJRU5ErkJggg==";
        private const string xMirrorIcon = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAOwQAADsEBuJFr7QAAABh0RVh0U29mdHdhcmUAcGFpbnQubmV0IDQuMC41ZYUyZQAAAG1JREFUOE+lj9ENwCAIRB2IFdyRfRiuDSaXAF4MrR9P5eRhHGb2Gxp2oaEjIovTXSrAnPNx6hlgyCZ7o6omOdYOldGIZhAziEmOTSfigLV0RYAB9y9f/7kO8L3WUaQyhCgz0dmCL9CwCw172HgBeyG6oloC8fAAAAAASUVORK5CYII=";
        private const string yMirrorIcon = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAOwgAADsIBFShKgAAAABh0RVh0U29mdHdhcmUAcGFpbnQubmV0IDQuMC41ZYUyZQAAAG9JREFUOE+djckNACEMAykoLdAjHbPyw1IOJ0L7mAejjFlm9hspyd77Kk+kBAjPOXcakJIh6QaKyOE0EB5dSPJAiUmOiL8PMVGxugsP/0OOib8vsY8yYwy6gRyC8CB5QIWgCMKBLgRSkikEUr5h6wOPWfMoCYILdgAAAABJRU5ErkJggg==";
        private const string xyMirrorIcon = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAOwgAADsIBFShKgAAAABl0RVh0U29mdHdhcmUAcGFpbnQubmV0IDQuMC4yMfEgaZUAAAHkSURBVDhPrVJLSwJRFJ4cdXwjPlrVJly1kB62cpEguElXKgYKIpaC+EIEEfGxLqI/UES1KaJlEdGmRY9ltCsIWrUJatGm0eZO3xkHIsJdH3zce+ec75z5zr3cf2MMmLdYLA/BYFA2mUyPOPvwnR+GR4PXaDQLLpfrKpVKSb1eT6bV6XTeocAS4sIw7S804BzEZ4IgsGq1ykhcr9dlj8czwPdbxJdBMyX/As/zLiz74Ar2J9lsVulcKpUYut5DnEbsHFwEx8AhtFqtGViD6BOc1ul0B5lMRhGXy2Wm1+ufkBOE/2fsL1FsQpXCiCAcQiAlk0kJRZjf7+9TRxI3Gg0WCoW+IpGISHHERBS5UKUch8n2K5WK3O125VqtpqydTkdZie12W261WjIVo73b7RZVKccZDIZ1q9XaT6fTLB6PD9BFKhQKjITFYpGFw+FBNBpVOgcCARH516pUGZYZXk5R4B3efLBxDM9f1CkWi/WR3ICtGVh6Rd4NPE+p0iEgmkSRLRoMEjYhHpA4kUiIOO8iZRU8AmnadK2/QOOfhnjPZrO95fN5Zdq5XE5yOBwvuKoNxGfBkQ8FzXkPprnj9Xrfm82mDI8fsLON3x5H/Od+RwHdLfDds9vtn0aj8QoF6QH9JzjuG3acpxmu1RgPAAAAAElFTkSuQmCC";
        private const string rotatedIcon = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAOwQAADsEBuJFr7QAAABh0RVh0U29mdHdhcmUAcGFpbnQubmV0IDQuMC41ZYUyZQAAAHdJREFUOE+djssNwCAMQxmIFdgx+2S4Vj4YxWlQgcOT8nuG5u5C732Sd3lfLlmPMR4QhXgrTQaimUlA3EtD+CJlBuQ7aUAUMjEAv9gWCQNEPhHJUkYfZ1kEpcxDzioRzGIlr0Qwi0r+Q5rTgM+AAVcygHgt7+HtBZs/2QVWP8ahAAAAAElFTkSuQmCC";
        private const string fixedIcon = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAZdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuMjHxIGmVAAAA50lEQVQ4T51Ruw6CQBCkwBYKWkIgQAs9gfgCvgb4BML/qWBM9Bdo9QPIuVOQ3JIzosVkc7Mzty9NCPE3lORaKMm1YA/LsnTXdbdhGJ6iKHoVRTEi+r4/OI6zN01Tl/XM7HneLsuyW13XU9u2ous6gYh3kiR327YPsp6ZgyDom6aZYFqiqqqJ8mdZz8xoca64BHjkZT0zY0aVcQbysp6Z4zj+Vvkp65mZttxjOSozdkEzD7KemekcxzRNHxDOHSDiQ/DIy3pmpjtuSJBThStGKMtyRKSOLnSm3DCMz3f+FUpyLZTkOgjtDSWORSDbpbmNAAAAAElFTkSuQmCC";

        private Texture2D[] arrowIconsTexture = new Texture2D[9] {
            Base64ToTexture(arrowIcon0),
            Base64ToTexture(arrowIcon1),
            Base64ToTexture(arrowIcon2),
            Base64ToTexture(arrowIcon3),
            Base64ToTexture(arrowIcon4),
            Base64ToTexture(arrowIcon5),
            Base64ToTexture(arrowIcon6),
            Base64ToTexture(arrowIcon7),
            Base64ToTexture(xIcon)
        };

        private Texture2D[] rotationIconsTexture = new Texture2D[5] {
            Base64ToTexture(fixedIcon),
            Base64ToTexture(rotatedIcon),
            Base64ToTexture(xMirrorIcon),
            Base64ToTexture(yMirrorIcon),
            Base64ToTexture(xyMirrorIcon)
        };

        private RLRulesHelper() { }

        public static RLRulesHelper Create() {
            return new RLRulesHelper();
        }

        public ReorderableList CreateReorderableListOfRules(ScriptedTile scriptedTile) {
            this.scriptedTile = scriptedTile;

            bool draggable = true;
            bool displayHeader = true;
            bool displayAddButton = true;
            bool displayRemoveButton = true;

            if (this.scriptedTile != null) {
                ReorderableList reorderableListOfRules = new ReorderableList(
                    this.scriptedTile.rules,
                    typeof(ScriptedTile.Rule),
                    draggable,
                    displayHeader,
                    displayAddButton,
                    displayRemoveButton
                );

                reorderableListOfRules.drawHeaderCallback = OnDrawRLHeader;
                reorderableListOfRules.drawElementCallback = OnDrawRLElement;
                reorderableListOfRules.elementHeightCallback = GetRLElementHeight;
                reorderableListOfRules.onChangedCallback = OnRLUpdated;
                reorderableListOfRules.onAddCallback = OnRLAddElement;

                return reorderableListOfRules;
            }

            return null;
        }

        public void ForceRefreshTiles(ScriptedTile scriptedTileTarget) {
            /**
             * This method force the `TileBase.RefreshTile()`.
             * Then the `ScriptedTile.GetTileData()` will be executed for all Tiles.
             */
            EditorUtility.SetDirty(scriptedTileTarget);
            SceneView.RepaintAll();
        }

        private static Texture2D Base64ToTexture(string base64) {
            Texture2D texture = new Texture2D(1, 1);
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.LoadImage(System.Convert.FromBase64String(base64));

            return texture;
        }

        #region ReorderableList methods: The Rules.
        private void OnDrawRLHeader(Rect rlRectangle) {
            GUI.Label(rlRectangle, "Rules");
        }

        private void OnDrawRLElement(Rect elementRectangle, int ruleIndex, bool isActive, bool isFocused) {
            ScriptedTile.Rule rule = scriptedTile.rules[ruleIndex];

            float elementGUITop = elementRectangle.yMin;
            float elementGUIRight = elementRectangle.xMax;
            float elementGUILeft = elementRectangle.xMin;

            BoundsInt neighborsMatrixBounds = GetNeighborsMatrixBounds(rule.GetNeighborsMatrixBoundsBySelectedNeighbors());
            Vector2 neighborsMatrixGUI = TransformNeighborsMatrixBoundsToGUI(neighborsMatrixBounds);

            float neighborsMatrixGUIWidth = neighborsMatrixGUI.x;
            float neighborsMatrixGUIHeight = neighborsMatrixGUI.y;

            #region SpriteComponent Rectangle.
            float spriteComponentPositionX = elementGUIRight - ReorderableListGUIDefaults.ComponentWidth;
            float spriteComponentPositionY = elementGUITop + ReorderableListGUIDefaults.FieldPaddingTop * 2;
            float spriteComponentWidth = ReorderableListGUIDefaults.ComponentWidth;
            float spriteComponentHeight = ReorderableListGUIDefaults.ElementHeight;

            // Separar o espaço deste componente dentro do element do ReorderableList.
            Rect spriteComponentRectangle = new Rect(
                spriteComponentPositionX,
                spriteComponentPositionY,
                spriteComponentWidth,
                spriteComponentHeight
            );
            #endregion

            #region NeighborsMatrixComponent Rectangle.
            float neighborsMatrixComponentPositionX = elementGUIRight - neighborsMatrixGUIWidth - spriteComponentWidth - 10f;
            float neighborsMatrixComponentPositionY = elementGUITop + ReorderableListGUIDefaults.FieldPaddingTop * 2;
            float neighborsMatrixComponentWidth = neighborsMatrixGUIWidth;
            float neighborsMatrixComponentHeight = neighborsMatrixGUIHeight;

            // Separar o espaço deste componente dentro do element do ReorderableList.
            Rect neighborsMatrixComponentRectangle = new Rect(
                neighborsMatrixComponentPositionX,
                neighborsMatrixComponentPositionY,
                neighborsMatrixComponentWidth,
                neighborsMatrixComponentHeight
            );
            #endregion

            #region TileDataComponent Rectangle.
            float tileDataComponentPositionX = elementGUILeft;
            float tileDataComponentPositionY = elementGUITop + ReorderableListGUIDefaults.FieldPaddingTop;
            float tileDataComponentWidth = elementRectangle.width - neighborsMatrixGUIWidth - spriteComponentRectangle.width - 20f;
            float tileDataComponentHeight = elementRectangle.height - ReorderableListGUIDefaults.ElementPaddingHeight;

            // Separar o espaço deste componente dentro do element do ReorderableList.
            Rect tileDataComponentRectangle = new Rect(
                tileDataComponentPositionX,
                tileDataComponentPositionY,
                tileDataComponentWidth,
                tileDataComponentHeight
            );
            #endregion

            DrawTileDataComponent(tileDataComponentRectangle, rule);
            DrawNeighborsMatrixComponent(neighborsMatrixComponentRectangle, neighborsMatrixBounds, scriptedTile, rule);
            DrawSpriteComponent(spriteComponentRectangle, rule);
        }

        #region ReorderableList elements.
        #region Components.
        private void DrawTileDataComponent(Rect tileDataComponentRectangle, ScriptedTile.Rule rule) {
            float componentPositionX = tileDataComponentRectangle.x;
            float fieldPositionY = tileDataComponentRectangle.y;
            Rect fieldRectangle;

            /*
             * Inicia o `fieldOrderNumber`.
             * Ele controla a quantidade de padding que cada Field precisa.
             * A quantidade de padding é baseada na ordem (1º, 2º, ...) do Field.
             */
            int fieldOrderNumber = UpdateFieldOrderNumber(0);

            fieldRectangle = GetFieldRectangle("Game Object", componentPositionX, ref fieldPositionY, ref fieldOrderNumber, tileDataComponentRectangle, rule);
            rule.tileGameObject = EditorGUI.ObjectField(fieldRectangle, "", rule.tileGameObject, typeof(GameObject), false) as GameObject;

            fieldRectangle = GetFieldRectangle("Collider Type", componentPositionX, ref fieldPositionY, ref fieldOrderNumber, tileDataComponentRectangle, rule);
            rule.tileColliderType = (Tile.ColliderType)EditorGUI.EnumPopup(fieldRectangle, rule.tileColliderType);

            fieldRectangle = GetFieldRectangle("Sprite Output", componentPositionX, ref fieldPositionY, ref fieldOrderNumber, tileDataComponentRectangle, rule);
            rule.spriteOutputType = (ScriptedTile.RuleTileData.SpriteOutputType)EditorGUI.EnumPopup(fieldRectangle, rule.spriteOutputType);

            if (rule.spriteOutputType == ScriptedTile.RuleTileData.SpriteOutputType.Animation) {
                fieldRectangle = GetFieldRectangle("Speed", componentPositionX, ref fieldPositionY, ref fieldOrderNumber, tileDataComponentRectangle, rule);
                rule.animationSpeed = EditorGUI.FloatField(fieldRectangle, rule.animationSpeed);
            }

            if (rule.spriteOutputType == ScriptedTile.RuleTileData.SpriteOutputType.Random) {
                fieldRectangle = GetFieldRectangle("Noise", componentPositionX, ref fieldPositionY, ref fieldOrderNumber, tileDataComponentRectangle, rule);
                rule.perlinNoiseScale = EditorGUI.Slider(fieldRectangle, rule.perlinNoiseScale, 0.001f, 0.999f);

                fieldRectangle = GetFieldRectangle("Rotation", componentPositionX, ref fieldPositionY, ref fieldOrderNumber, tileDataComponentRectangle, rule);
                rule.randomSpriteRotationType = (ScriptedTile.Rule.RotationType)EditorGUI.EnumPopup(fieldRectangle, rule.randomSpriteRotationType);
            }

            // Lista de Sprites para a animação ou, para a randomização.
            if (rule.spriteOutputType != ScriptedTile.RuleTileData.SpriteOutputType.Single) {
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
                    tileDataComponentRectangle.width - ReorderableListGUIDefaults.FieldWidth,
                    ReorderableListGUIDefaults.FieldHeight
                );

                EditorGUI.BeginChangeCheck();

                GUI.Label(spritesNumberLabelRectangle, "Sprites");
                int spritesNumber = EditorGUI.DelayedIntField(spritesNumberFieldRectangle, rule.tileSprites.Length);

                /*
                 * Não há necessidade de ter mais que 500 Sprites por Rule.
                 * Um valor muito grande pode causar atrasos no Editor, por causa da quantidade de loops.
                 */
                spritesNumber = Mathf.Min(500, spritesNumber);

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
                        tileDataComponentRectangle.width - ReorderableListGUIDefaults.FieldWidth,
                        ReorderableListGUIDefaults.FieldHeight
                    );

                    rule.tileSprites[spriteIndex] = EditorGUI.ObjectField(spriteFieldRectangle, rule.tileSprites[spriteIndex], typeof(Sprite), false) as Sprite;

                    fieldPositionY = UpdateFieldPositionY(fieldPositionY);
                    fieldOrderNumber = UpdateFieldOrderNumber(fieldOrderNumber);
                }
                #endregion
            }
        }

        private void DrawNeighborsMatrixComponent(Rect neighborsMatrixComponentRectangle, BoundsInt neighborsMatrixBounds, ScriptedTile scriptedTile, ScriptedTile.Rule rule) {
            Handles.color = EditorGUIUtility.isProSkin ?
                new Color(1f, 1f, 1f, 0.2f) :
                new Color(0f, 0f, 0f, 0.2f);

            // Largura do Matrix GUI.
            float matrixWidth = neighborsMatrixComponentRectangle.width;
            // Altura do Matrix GUI.
            float matrixHeight = neighborsMatrixComponentRectangle.height;

            // Quantidade de colunas verticais do Matrix.
            float boundsWidth = neighborsMatrixBounds.size.x;
            // Quantidade de colunas horizontais do Matrix.
            float boundsHeight = neighborsMatrixBounds.size.y;

            int boundsMaxX = neighborsMatrixBounds.xMax;
            int boundsMaxY = neighborsMatrixBounds.yMax;
            int boundsMinX = neighborsMatrixBounds.xMin;
            int boundsMinY = neighborsMatrixBounds.yMin;

            // Largura de cada coluna do Matrix.
            float matrixColumnWidth = matrixWidth / boundsWidth;
            // Altura de cada coluna do Matrix.
            float matrixColumnHeight = matrixHeight / boundsHeight;

            float matrixGUITop = neighborsMatrixComponentRectangle.yMin;
            float matrixGUIRight = neighborsMatrixComponentRectangle.xMax;
            float matrixGUIBottom = neighborsMatrixComponentRectangle.yMax;
            float matrixGUILeft = neighborsMatrixComponentRectangle.xMin;

            // Draw horizontal columns.
            for (int horizontalColumnIndex = 0; horizontalColumnIndex <= boundsHeight; horizontalColumnIndex++) {
                float columnHeight = matrixGUITop + horizontalColumnIndex * matrixColumnHeight;

                Vector3 startPoint = new Vector3(matrixGUILeft, columnHeight);
                Vector3 endPoint = new Vector3(matrixGUIRight, columnHeight);

                Handles.DrawLine(startPoint, endPoint);
            }

            // Draw vertical columns.
            for (int verticalColumnIndex = 0; verticalColumnIndex <= boundsWidth; verticalColumnIndex++) {
                float columnWidth = matrixGUILeft + verticalColumnIndex * matrixColumnWidth;

                Vector3 startPoint = new Vector3(columnWidth, matrixGUITop);
                Vector3 endPoint = new Vector3(columnWidth, matrixGUIBottom);

                Handles.DrawLine(startPoint, endPoint);
            }

            Handles.color = Color.white;

            Dictionary<Vector3Int, int> neighbors = rule.GetNeighbors();

            // In a NeighborBox.
            for (int neighborY = boundsMinY; neighborY < boundsMaxX; neighborY++) {
                for (int neighborX = boundsMinX; neighborX < boundsMaxY; neighborX++) {
                    Vector3Int neighborBoxPositionOnMatrix = new Vector3Int(neighborX, neighborY, 0);

                    float neighborBoxPositionX = matrixGUILeft + (neighborX - boundsMinX) * matrixColumnWidth;
                    float neighborBoxPositionY = matrixGUITop + (-neighborY + boundsMaxY - 1) * matrixColumnHeight;
                    float width = matrixColumnWidth - 1;
                    float height = matrixColumnHeight - 1;

                    Rect neighborBoxRectangle = new Rect(
                        neighborBoxPositionX,
                        neighborBoxPositionY,
                        width,
                        height
                    );

                    InNeighborBox(neighborBoxPositionOnMatrix, neighborBoxRectangle, rule, neighbors);
                }
            }
        }

        private void DrawSpriteComponent(Rect spriteComponentRectangle, ScriptedTile.Rule rule) {
            rule.tileSprites[0] = EditorGUI.ObjectField(spriteComponentRectangle, rule.tileSprites[0], typeof(Sprite), false) as Sprite;
        }
        #endregion

        #region NeighborBox.
        private void InNeighborBox(Vector3Int neighborBoxPositionOnMatrix, Rect neighborBoxRectangle, ScriptedTile.Rule rule, Dictionary<Vector3Int, int> neighbors) {
            using (var changeCheckScope = new EditorGUI.ChangeCheckScope()) {
                // Todos os NeighborBox, exceto o do meio.
                if (neighborBoxPositionOnMatrix.x != 0 || neighborBoxPositionOnMatrix.y != 0) {
                    if (neighbors.ContainsKey(neighborBoxPositionOnMatrix)) {
                        DrawArrowIcons(neighborBoxRectangle, neighborBoxPositionOnMatrix, neighbors[neighborBoxPositionOnMatrix]);
                        OnMouseHoverPrintTooltipByNeighborBox(neighborBoxRectangle, neighbors[neighborBoxPositionOnMatrix]);
                    }

                    OnNeighborBoxClick(neighborBoxRectangle, rule, neighbors, neighborBoxPositionOnMatrix);
                } else {
                    OnCenteredNeighborBoxClick(neighborBoxRectangle, rule);
                    DrawRotationsIcons(neighborBoxRectangle, rule.nextTileSelectionType);
                }
            }
        }

        private void OnNeighborBoxClick(Rect neighborBoxRectangle, ScriptedTile.Rule rule, Dictionary<Vector3Int, int> neighbors, Vector3Int neighborBoxPositionOnMatrix) {
            if (Event.current.type == EventType.MouseDown && MouseOnPosition(neighborBoxRectangle)) {
                neighborRuleCodes = EnumRuleCodeToListRuleCode();

                // Se já houver um Neighbor associado a um NeighborBox.
                if (neighbors.ContainsKey(neighborBoxPositionOnMatrix)) {
                    int currentRuleCode = neighborRuleCodes.IndexOf(neighbors[neighborBoxPositionOnMatrix]);
                    int changedRuleCode = currentRuleCode + GetChangedClick();

                    if (changedRuleCode >= 0 && changedRuleCode < neighborRuleCodes.Count) {
                        changedRuleCode = (int)Mathf.Repeat(changedRuleCode, neighborRuleCodes.Count);
                        neighbors[neighborBoxPositionOnMatrix] = neighborRuleCodes[changedRuleCode];
                    } else {
                        neighbors.Remove(neighborBoxPositionOnMatrix);
                    }
                } else {
                    // Se não houver um Neighbor associado a um NeighborBox.
                    neighbors.Add(neighborBoxPositionOnMatrix, neighborRuleCodes[GetChangedClick() == 1 ? 0 : (neighborRuleCodes.Count - 1)]);
                }

                rule.SetNeighbors(neighbors);

                GUI.changed = true;
                Event.current.Use();
            }
        }

        private void OnCenteredNeighborBoxClick(Rect neighborBoxPositionOnMatrix, ScriptedTile.Rule rule) {
            if (Event.current.type == EventType.MouseDown && MouseOnPosition(neighborBoxPositionOnMatrix)) {
                rule.nextTileSelectionType = (ScriptedTile.RuleTileData.RotationType)(int)Mathf.Repeat(
                    (int)rule.nextTileSelectionType + GetChangedClick(),
                    Enum.GetValues(typeof(ScriptedTile.RuleTileData.RotationType)).Length
                );

                GUI.changed = true;
                Event.current.Use();
            }
        }

        private Rect GetFieldRectangle(string label, float componentPositionX, ref float fieldPositionY, ref int fieldOrderNumber, Rect componentRectangle, ScriptedTile.Rule rule) {
            Rect labelRectangle = new Rect(
                componentPositionX,
                fieldPositionY + GetFieldPaddingTopByFieldPositionY(fieldPositionY, fieldOrderNumber),
                ReorderableListGUIDefaults.FieldWidth,
                ReorderableListGUIDefaults.FieldHeight
            );

            Rect fieldRectangle = new Rect(
                componentPositionX + ReorderableListGUIDefaults.FieldWidth,
                fieldPositionY + GetFieldPaddingTopByFieldPositionY(fieldPositionY, fieldOrderNumber),
                /*
                 * `componentRectangle.width`: A largura do Field é proporcional a largura do element do ReorderableList,
                 * pois, `componentRectangle` é baseado no elementRectangle.
                 */
                componentRectangle.width - ReorderableListGUIDefaults.FieldWidth,
                ReorderableListGUIDefaults.FieldHeight
            );

            GUI.Label(labelRectangle, label);

            fieldPositionY = UpdateFieldPositionY(fieldPositionY);
            fieldOrderNumber = UpdateFieldOrderNumber(fieldOrderNumber);

            return fieldRectangle;
        }

        private void OnMouseHoverPrintTooltipByNeighborBox(Rect neighborBoxRectangle, int ruleCode) {
            FieldInfo[] enumRuleCodes = scriptedTile.neighborRuleCodes.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy);

            foreach (FieldInfo enumRuleCode in enumRuleCodes) {
                if ((int)enumRuleCode.GetValue(null) == ruleCode) {
                    // `GUIContent()`: On Mouse Hover in `neighborBoxRectangle` show Enum Rule Code Name.
                    GUI.Label(neighborBoxRectangle, new GUIContent("", enumRuleCode.Name));
                    break;
                }
            }
        }
        #endregion

        #region Icons.
        private int GetArrowIconIndex(Vector3Int neighborBoxPositionOnMatrix) {
            if (Mathf.Abs(neighborBoxPositionOnMatrix.x) == Mathf.Abs(neighborBoxPositionOnMatrix.y)) {
                if (neighborBoxPositionOnMatrix.x < 0 && neighborBoxPositionOnMatrix.y > 0) {
                    return 0;
                } else if (neighborBoxPositionOnMatrix.x > 0 && neighborBoxPositionOnMatrix.y > 0) {
                    return 2;
                } else if (neighborBoxPositionOnMatrix.x < 0 && neighborBoxPositionOnMatrix.y < 0) {
                    return 5;
                } else if (neighborBoxPositionOnMatrix.x > 0 && neighborBoxPositionOnMatrix.y < 0) {
                    return 7;
                }
            } else if (Mathf.Abs(neighborBoxPositionOnMatrix.x) > Mathf.Abs(neighborBoxPositionOnMatrix.y)) {
                if (neighborBoxPositionOnMatrix.x > 0) {
                    return 4;
                } else {
                    return 3;
                }
            } else {
                if (neighborBoxPositionOnMatrix.y > 0) {
                    return 1;
                } else {
                    return 6;
                }
            }

            return -1;
        }

        private void DrawArrowIcons(Rect neighborBoxRectangle, Vector3Int neighborBoxPositionOnMatrix, int ruleCode) {
            switch (ruleCode) {
                case ScriptedTile.RuleTileData.NeighborRuleCode.MustHave:
                    GUI.DrawTexture(neighborBoxRectangle, arrowIconsTexture[GetArrowIconIndex(neighborBoxPositionOnMatrix)]);
                    break;
                case ScriptedTile.RuleTileData.NeighborRuleCode.MustNotHave:
                    GUI.DrawTexture(neighborBoxRectangle, arrowIconsTexture[8]);
                    break;
                default:
                    GUIStyle style = new GUIStyle();
                    style.alignment = TextAnchor.MiddleCenter;
                    style.fontSize = 10;
                    GUI.Label(neighborBoxRectangle, ruleCode.ToString(), style);
                    break;
            }
        }

        private void DrawRotationsIcons(Rect neighborBoxRectangle, ScriptedTile.RuleTileData.RotationType nextTileSelectionType) {
            switch (nextTileSelectionType) {
                case ScriptedTile.RuleTileData.RotationType.Fixed:
                    GUI.DrawTexture(neighborBoxRectangle, rotationIconsTexture[0]);
                    break;
                case ScriptedTile.RuleTileData.RotationType.Rotated:
                    GUI.DrawTexture(neighborBoxRectangle, rotationIconsTexture[1]);
                    break;
                case ScriptedTile.RuleTileData.RotationType.MirrorX:
                    GUI.DrawTexture(neighborBoxRectangle, rotationIconsTexture[2]);
                    break;
                case ScriptedTile.RuleTileData.RotationType.MirrorY:
                    GUI.DrawTexture(neighborBoxRectangle, rotationIconsTexture[3]);
                    break;
                case ScriptedTile.RuleTileData.RotationType.MirrorXY:
                    GUI.DrawTexture(neighborBoxRectangle, rotationIconsTexture[4]);
                    break;
            }
        }
        #endregion

        private BoundsInt GetNeighborsMatrixBounds(BoundsInt bounds) {
            /*
             * BoundsInt, basicamente é um plano cartesiano.
             * Tem o menor ponto no eixo X: `xMin`.
             * Tem o maior ponto no eixo X: `xMax`.
             * Tem a largura total: `size.x` = |`xMin` + `xMax`|
             * 
             * Tem o menor ponto no eixo Y: `yMin`.
             * Tem o maior ponto no eixo Y: `yMax`.
             * Tem a altura total: `size.y` = |`yMin` + `yMax`|
             */

            bounds.xMin = Mathf.Min(bounds.xMin, -1);
            bounds.yMin = Mathf.Min(bounds.yMin, -1);
            bounds.xMax = Mathf.Max(bounds.xMax, 2);
            bounds.yMax = Mathf.Max(bounds.yMax, 2);

            return bounds;
        }

        private Vector2 TransformNeighborsMatrixBoundsToGUI(BoundsInt neighborsMatrixBounds) {
            int neighborsMatrixBoundsWidth = neighborsMatrixBounds.size.x;
            int neighborsMatrixBoundsHeight = neighborsMatrixBounds.size.y;

            return new Vector2(
                neighborsMatrixBoundsWidth * ReorderableListGUIDefaults.FieldHeight,
                neighborsMatrixBoundsHeight * ReorderableListGUIDefaults.FieldHeight
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

        private List<int> EnumRuleCodeToListRuleCode() {
            FieldInfo[] getNeighborRuleCodes = scriptedTile.neighborRuleCodes.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            Func<FieldInfo, int> selectorRuleCodes = ruleCode => (int)ruleCode.GetValue(null);
            List<int> neighborRuleCodes = getNeighborRuleCodes.Select(selectorRuleCodes).ToList();
            neighborRuleCodes.Sort();

            return neighborRuleCodes;
        }

        private bool MouseOnPosition(Rect positionRectangle) {
            return positionRectangle.Contains(Event.current.mousePosition);
        }

        private int GetChangedClick() {
            return Event.current.button == 1 ? -1 : 1;
        }
        #endregion

        private float GetRLElementHeight(int ruleIndex) {
            ScriptedTile.Rule rule = scriptedTile.rules[ruleIndex];

            BoundsInt neighborsMatrixBounds = GetNeighborsMatrixBounds(rule.GetNeighborsMatrixBoundsBySelectedNeighbors());

            float heightByTileDataComponent = 0f;
            float sumOfFieldPaddingTop;
            float sumOfheightOfAllSpritesFields;

            switch (rule.spriteOutputType) {
                case ScriptedTile.RuleTileData.SpriteOutputType.Single:
                    heightByTileDataComponent = ReorderableListGUIDefaults.ElementHeight + ReorderableListGUIDefaults.ElementPaddingHeight + ReorderableListGUIDefaults.FieldPaddingTop * 3;
                    break;

                case ScriptedTile.RuleTileData.SpriteOutputType.Random:
                    // Iniciar com 6 Fields.
                    sumOfFieldPaddingTop = ReorderableListGUIDefaults.FieldPaddingTop * 6;

                    for (int i = 0; i < rule.tileSprites.Length; i++) {
                        sumOfFieldPaddingTop += ReorderableListGUIDefaults.FieldPaddingTop;
                    }

                    sumOfheightOfAllSpritesFields = ReorderableListGUIDefaults.FieldHeight * (rule.tileSprites.Length + 3);

                    heightByTileDataComponent = sumOfFieldPaddingTop + sumOfheightOfAllSpritesFields + ReorderableListGUIDefaults.ElementHeight + ReorderableListGUIDefaults.ElementPaddingHeight;
                    break;

                case ScriptedTile.RuleTileData.SpriteOutputType.Animation:
                    // Iniciar com 5 Fields.
                    sumOfFieldPaddingTop = ReorderableListGUIDefaults.FieldPaddingTop * 5;

                    for (int i = 0; i < rule.tileSprites.Length; i++) {
                        sumOfFieldPaddingTop += ReorderableListGUIDefaults.FieldPaddingTop;
                    }

                    sumOfheightOfAllSpritesFields = ReorderableListGUIDefaults.FieldHeight * (rule.tileSprites.Length + 2);

                    heightByTileDataComponent = sumOfFieldPaddingTop + sumOfheightOfAllSpritesFields + ReorderableListGUIDefaults.ElementHeight + ReorderableListGUIDefaults.ElementPaddingHeight;
                    break;
            }

            float neighborsMatrixGUIHeight = TransformNeighborsMatrixBoundsToGUI(neighborsMatrixBounds).y + 10f;

            // Qual componente tem a maior altura? TileDataComponent ou NeighborMatrixBounds.
            return Mathf.Max(heightByTileDataComponent, neighborsMatrixGUIHeight);
        }

        private void OnRLUpdated(ReorderableList list) {
            // Os Tiles precisam ser atualizados caso, por exemplo, uma Rule é reordenada na lista.
            if (scriptedTile != null) {
                ForceRefreshTiles(scriptedTile);
            }
        }

        private void OnRLAddElement(ReorderableList list) {
            ScriptedTile.Rule rule = new ScriptedTile.Rule();

            // Use default values.
            rule.tileSprites[0] = scriptedTile.defaultTileSprite;
            rule.tileGameObject = scriptedTile.defaultTileGameObject;
            rule.tileColliderType = scriptedTile.defaultTileColliderType;
            rule.spriteOutputType = ScriptedTile.RuleTileData.SpriteOutputType.Single;

            scriptedTile.rules.Add(rule);
        }
        #endregion
    }
}