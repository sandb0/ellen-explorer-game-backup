using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Linq;
using System.Collections.Generic;

namespace EllenExplorer.Tools.Tiles {
    #region @CLASSROOM: Palettes, Tile e ScriptableObjects.
    /**
     * Um Palette pode conter diversos Tiles.
     * Um Tile, por padrão, é apenas uma imagem.
     * 
     * Mas, o Unity permite sobrescrever o comportamento padrão de um Tile.
     * Por exemplo, com base nos Tiles ao redor e em uma lógica de neighbors (onde deve ter um Tile e onde não deve ter um Tile),
     * o sprite de um Tile e dos Tiles ao redor, podem ser modificados.
     * 
     * Tudo começa com o método `RefreshTile()`, em seguida o método `GetTileData()` irá buscar os
     * dados de um Tile. A partir deste ponto, o Tile e os Tiles ao redor podem ser modificados.
     * 
     * 
     * === Observação importante ===
     * 
     * Este ScriptedTile irá criar um único Tile. A diferença será que este Tile
     * não será apenas uma imagem, mas, várias imagens e, com uma determinada lógica para que cada uma delas
     * apareçam.
     * 
     * Um ScriptableObject é um Asset que, basicamente, armazena dados e, pode ter seu Inspector customizado.
     * Por TileBase estender de ScriptableObject e, ScriptedTile estender de TileBase, todo ScriptedTile armazenará todas
     * as imagens e terá seu Inspector customizado.
     * 
     * 
     * === Sobre o ScriptedTile ===
     * 
     * É um Tile.
     * Contém regras (Rule).
     * Cada Rule contém as dados de um Tile normal: Sprite, GameObject, ColliderType e, Transform.
     * E cada Rule tentará substituir os dados padrão do ScriptedTile.
     * Mas, para isso acontecer, as regras de substituição devem ser satisfeitas (Neighbors).
     * Onde deve ter um Tile, deve ter; Onde não deve ter um Tile, não deve ter.
     */
    #endregion
    [Serializable]
    [CreateAssetMenu(fileName = "New Scripted Tile", menuName = "Tiles/Scripted Tile")]
    public class ScriptedTile : TileBase {
        public Sprite defaultTileSprite;
        public GameObject defaultTileGameObject;
        public Tile.ColliderType defaultTileColliderType = Tile.ColliderType.Sprite;

        public Type neighborRuleCodes => typeof(Rule.NeighborRuleCode);

        [Serializable]
        public class RuleTileData {
            public Sprite[] tileSprites = new Sprite[1];
            public GameObject tileGameObject;
            public Tile.ColliderType tileColliderType = Tile.ColliderType.Sprite;

            public float animationSpeed = 1f;

            public float perlinNoise = 0.5f;

            public RandomRotationType randomRotationType;

            public SpriteOutputType spriteOutputType = SpriteOutputType.Single;

            public class NeighborRuleCode {
                public const int MustHave = 1;
                public const int MustNotHave = 2;
            }

            public enum SpriteOutputType {
                Single,
                Random,
                Animation
            }

            public enum RandomRotationType {
                Fixed,
                Rotated, // Rotacionar o Sprite do Tile em 0, 90, 180 ou 270 graus.
                MirrorX, // Rotacionar o Sprite do Tile em 180° no eixo X.
                MirrorY, // Rotacionar o Sprite do Tile em 180° no eixo Y.
                MirrorXY // Rotacionar o Sprite do Tile em 180° nos eixo X e/ou Y.
            }
        }

        [Serializable]
        public class Rule : RuleTileData {
            public List<int> neighborsRuleCode = new List<int>();
            public List<Vector3Int> neighborsMatrixPosition = new List<Vector3Int>() {
                new Vector3Int(-1, 1, 0),
                new Vector3Int(0, 1, 0),
                new Vector3Int(1, 1, 0),
                new Vector3Int(-1, 0, 0),
                new Vector3Int(1, 0, 0),
                new Vector3Int(-1, -1, 0),
                new Vector3Int(0, -1, 0),
                new Vector3Int(1, -1, 0)
            };

            public Dictionary<Vector3Int, int> GetNeighbors() {
                Dictionary<Vector3Int, int> neighbors = new Dictionary<Vector3Int, int>();

                for (int neighborIndex = 0; neighborIndex < neighborsRuleCode.Count && neighborIndex < neighborsMatrixPosition.Count; neighborIndex++) {
                    neighbors.Add(neighborsMatrixPosition[neighborIndex], neighborsRuleCode[neighborIndex]);
                }

                return neighbors;
            }

            public void SetNeighbors(Dictionary<Vector3Int, int> neighbors) {
                neighborsMatrixPosition = neighbors.Keys.ToList();
                neighborsRuleCode = neighbors.Values.ToList();
            }

            public BoundsInt GetNeighborsMatrixBoundsBySelectedNeighbors() {
                /**
                 * BoundsInt é uma caixa usada para delimitações.
                 * 
                 * Será usado para delimitar os limites do Neighbor Matrix.
                 */

                Vector3Int boundsMinSize = Vector3Int.zero;
                Vector3Int boundsMaxSize = Vector3Int.zero;
                BoundsInt bounds = new BoundsInt(boundsMinSize, boundsMaxSize);

                foreach (var neighbors in GetNeighbors()) {
                    // Delimitar os limites mínimos nos eixos X e Y.
                    // O mínimo já esta no `bounds` ou é a menor posição do Neighbor Matrix respectivo ao seu eixo.
                    bounds.xMin = Mathf.Min(bounds.xMin, neighbors.Key.x);
                    bounds.yMin = Mathf.Min(bounds.yMin, neighbors.Key.y);

                    // Delimitar os limites máximo nos eixos X e Y.
                    // O máximo já esta no `bounds` ou é a posição do Neighbor Matrix respectivo ao seu eixo + 1.
                    bounds.xMax = Mathf.Max(bounds.xMax, neighbors.Key.x + 1);
                    bounds.yMax = Mathf.Max(bounds.yMax, neighbors.Key.y + 1);
                }

                return bounds;
            }
        }

        [HideInInspector]
        public List<Rule> rules = new List<Rule>();

        #region TileBase override methods implementation.
        /// <summary>
        /// </summary>
        /// <param name="tilePosition">Posição do Tile no Tilemap.</param>
        /// <param name="tilemap">O Tilemap em que o Tile está.</param>
        /// <param name="gameObject">Um GameObject para o Tile.</param>
        /// <returns>True.</returns>
        public override bool StartUp(Vector3Int tilePosition, ITilemap tilemap, GameObject gameObject) {
            return true;
        }

        /// <summary>
        /// </summary>
        /// <param name="tilePosition">Posição do Tile no Tilemap.</param>
        /// <param name="tilemap">O Tilemap em que o Tile está.</param>
        /// <param name="tileData">Dados do Tile que será renderizado (painted).</param>
        public override void GetTileData(Vector3Int tilePosition, ITilemap tilemap, ref TileData tileData) {
            Matrix4x4 matrixTransform = Matrix4x4.identity;

            // Dados padrão do Tile, se, nenhuma regra corresponder ao Tile.

            tileData.sprite = defaultTileSprite;
            tileData.gameObject = defaultTileGameObject;
            tileData.colliderType = defaultTileColliderType;
            tileData.transform = matrixTransform;
            tileData.flags = TileFlags.LockTransform;

            CheckRulesToTryMatchOnTile(tilePosition, tilemap, ref tileData, matrixTransform);
        }

        /// <summary>
        /// </summary>
        /// <param name="tilePosition">Posição do Tile no Tilemap.</param>
        /// <param name="tilemap">O Tilemap em que o Tile está.</param>
        /// <param name="tileAnimationData">Dados para executar uma animação no Tile que será renderizado (painted).</param>
        /// <returns>True.</returns>
        public override bool GetTileAnimationData(Vector3Int tilePosition, ITilemap tilemap, ref TileAnimationData tileAnimationData) {
            return true;
        }

        /// <summary>
        /// </summary>
        /// <param name="tilePosition">Posição do Tile no Tilemap.</param>
        /// <param name="tilemap">O Tilemap em que o Tile está.</param>
        public override void RefreshTile(Vector3Int tilePosition, ITilemap tilemap) {
            OnEnterOrLeaveWithAnyTilePaintToolInTilemap(tilePosition, tilemap);
        }
        #endregion

        private void OnEnterOrLeaveWithAnyTilePaintToolInTilemap(Vector3Int tilePosition, ITilemap tilemap) {
            /**
             * === Enter or leave on Tile empty or paited. ===
             * 
             * O método `base.RefreshTile()` irá executar o `GetTileData()` se,
             * qualquer tile paint tool entrar ou sair de um Tile (empty or paited).
             * 
             * === Enter on Tile painted. Or Paint a Tile. ===
             * 
             * O método `base.RefreshTile()` irá executar os métodos `GetTileData()`, `StartUp()`, `GetTileAnimationData()` se,
             * qualquer tile paint tool entrar em um Tile (painted).
             */
            //base.RefreshTile(tilePosition, tilemap);
        }

        private void CheckRulesToTryMatchOnTile(Vector3Int tilePosition, ITilemap tilemap, ref TileData tileData, Matrix4x4 matrixTransform) {
            /**
             * As Rules são varificadas uma a uma, até que uma delas corresponda ao Tile.
             * Se nenhuma Rule corresponder o Tile, os dados padrão do Tile serão usados.
             * 
             * As Rules devem modificar os dados do Tile: Sprite, Transform, GameObject e ColliderType.
             * 
             * Uma Rule corresponde a um Tile de acordo com os `Neighbors` e os Tiles ao redor do Tile.
             */
            foreach (Rule rule in rules) {
                Matrix4x4 transform = matrixTransform;

                bool ruleMatch = RuleMatches(rule, tilePosition, tilemap, ref transform);

                if (ruleMatch) {
                    ChangeTileData(rule, ref tileData, transform);

                    // O restante das Rules não precisam ser verificadas.
                    break;
                }
            }
        }

        private bool RuleMatches(Rule rule, Vector3Int tilePosition, ITilemap tilemap, ref Matrix4x4 transform) {
            /**
             * Dentro de um Rule, todos os Neighbors serão verificados.
             * Se todos os Neighbors forem válidos, o Tile atual será moficado pelo Rule.
             */
            /*for (int neighborIndex = 0; neighborIndex < rule.neighbors.Count; neighborIndex++) {
                int neighborRuleCode = rule.neighborsRuleCode[neighborIndex];

                Vector3Int positionOffset = rule.neighborsMatrixPosition[neighborIndex];

                TileBase tileByNeighborPosition = tilemap.GetTile(tilePosition + positionOffset);

                if (!PigMethod(neighborRuleCode, tileByNeighborPosition)) {
                    return false;
                }
            }*/

            return true;
        }

        private bool PigMethod(int neighborRuleCode, TileBase tileByNeighborPosition) {
            ScriptedTile thisRuleTile = this;

            switch (neighborRuleCode) {
                case RuleTileData.NeighborRuleCode.MustHave:
                    return tileByNeighborPosition == thisRuleTile;
                case RuleTileData.NeighborRuleCode.MustNotHave:
                    return tileByNeighborPosition != thisRuleTile;
            }

            return true;
        }

        private void ChangeTileData(Rule rule, ref TileData tileData, Matrix4x4 matrixTransform) {
            switch (rule.spriteOutputType) {
                case Rule.SpriteOutputType.Single:
                case Rule.SpriteOutputType.Animation:
                    tileData.sprite = rule.tileSprites[0];
                    break;
            }

            tileData.transform = matrixTransform;
            tileData.gameObject = rule.tileGameObject;
            tileData.colliderType = rule.tileColliderType;
        }
    }
}