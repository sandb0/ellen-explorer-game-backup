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

        private readonly List<Vector3Int> neighborsBoxesPositionOnMatrix = new List<Vector3Int>() {
            new Vector3Int(-1, 1, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(1, 1, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(1, 0, 0),
            new Vector3Int(-1, -1, 0),
            new Vector3Int(0, -1, 0),
            new Vector3Int(1, -1, 0)
        };

        [Serializable]
        public class RuleTileData {
            public Sprite[] tileSprites = new Sprite[1];
            public GameObject tileGameObject;
            public Tile.ColliderType tileColliderType = Tile.ColliderType.Sprite;

            public float animationSpeed = 1f;
            public float perlinNoiseScale = 0.5f;

            // Tipo da rotação randômica do Sprite do Tile.
            public RotationType randomSpriteRotationType;
            /*
             * Tipo de seleção do nextTile.
             * 
             * Os RuleCodes dos Neighbors continuarão os mesmos, na mesma posição.
             * Mas os Tiles selecionados para a verificação (nextTile), não serão respectivos a posição
             * do NeighborBox.
             * O Sprite do Tile tambem será rotacionado.
             * 
             * - Fixed: A seleção será normal, apenas um Tile e respectivo a posição do NeighborBox.
             * - Rotated: A seleção acontece em 4 Tiles ao redor do Tile atual (a cada 90 graus).
             * - MirrorX: A seleção acontece no Tile oposto ao NeighborBox no eixo X
             * (se o NeighBox está na esquerda, o Tile selecionado será o da direita).
             * - MirrorY: A seleção acontece no Tile oposto ao NeighborBox no eixo Y.
             * (se o NeighBox está em cima, o Tile selecionado será o de baixo).
             * - MirrorXY: A seleção acontece no Tile oposto ao NeighborBox no eixo X ou Y.
             */
            public RotationType nextTileSelectionType;

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

            public enum RotationType {
                Fixed, // Não há rotação. O Sprite e o nextTile não serão modificados.
                Rotated, // Rotação 0°, 90°, 180° e 270°.
                MirrorX, // Rotação X * -1. Oposto no eixo X.
                MirrorY, // Rotação Y * -1. Oposto no eixo Y.
                MirrorXY // Rotação X e/ou Y * -1. Oposto de X e/ou Y. No eixo X e/ou Y.
            }
        }

        [Serializable]
        public class Rule : RuleTileData {
            public List<int> neighborsRuleCode = new List<int>();
            public List<Vector3Int> neighborsBoxesPositionOnMatrix = new List<Vector3Int>() {
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

                for (int neighborIndex = 0; neighborIndex < neighborsRuleCode.Count && neighborIndex < neighborsBoxesPositionOnMatrix.Count; neighborIndex++) {
                    neighbors.Add(neighborsBoxesPositionOnMatrix[neighborIndex], neighborsRuleCode[neighborIndex]);
                }

                return neighbors;
            }

            public void SetNeighbors(Dictionary<Vector3Int, int> neighbors) {
                neighborsBoxesPositionOnMatrix = neighbors.Keys.ToList();
                neighborsRuleCode = neighbors.Values.ToList();
            }

            public BoundsInt GetNeighborsMatrixBoundsBySelectedNeighbors() {
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
        public override bool StartUp(Vector3Int tilePosition, ITilemap iTilemap, GameObject gameObject) {
            if (gameObject == null) {
                return true;
            }

            // Se o Rule tiver um gameObject definido.

            Tilemap tilemap = iTilemap.GetComponent<Tilemap>();
            Matrix4x4 tileOrientationMatrix = tilemap.orientationMatrix;

            Matrix4x4 matrixTransform = Matrix4x4.identity;

            bool neighborsMatched = false;

            Vector3 gameObjectPosition = new Vector3();
            Quaternion gameObjectRotation = new Quaternion();
            Vector3 gameObjectScale = new Vector3();

            foreach (Rule rule in rules) {
                neighborsMatched = IsNeighborsMatched(rule, tilePosition, iTilemap, ref matrixTransform);

                // Se alguma Rule substituir o Tile.
                if (neighborsMatched) {
                    // Calcular a position, rotation e scale do GameObject do Tile.

                    Matrix4x4 transform = matrixTransform * tileOrientationMatrix;

                    gameObjectPosition = new Vector3(transform.m03, transform.m13, transform.m23);

                    Vector3 forward = new Vector3(transform.m02, transform.m12, transform.m22);
                    Vector3 upwards = new Vector3(transform.m01, transform.m11, transform.m21);
                    gameObjectRotation = Quaternion.LookRotation(forward, upwards);

                    gameObjectScale = tileOrientationMatrix.lossyScale;

                    break;
                }
            }

            // Se nenhuma Rule substituir o Tile.
            if (!neighborsMatched) {
                // Calcular a position, rotation e scale do GameObject do Tile.

                gameObjectPosition = new Vector3(tileOrientationMatrix.m03, tileOrientationMatrix.m13, tileOrientationMatrix.m23);

                Vector3 forward = new Vector3(tileOrientationMatrix.m02, tileOrientationMatrix.m12, tileOrientationMatrix.m22);
                Vector3 upwards = new Vector3(tileOrientationMatrix.m01, tileOrientationMatrix.m11, tileOrientationMatrix.m21);
                gameObjectRotation = Quaternion.LookRotation(forward, upwards);

                gameObjectScale = tileOrientationMatrix.lossyScale;
            }

            // Definir a position, rotation e scale do GameObject do Tile.
            gameObject.transform.localPosition = gameObjectPosition;
            gameObject.transform.localRotation = gameObjectRotation;
            gameObject.transform.localScale = gameObjectScale;

            return true;
        }

        public override void GetTileData(Vector3Int tilePosition, ITilemap iTilemap, ref TileData tileData) {
            Matrix4x4 matrixTransform = Matrix4x4.identity;

            // Dados padrão do Tile, se, nenhuma regra corresponder ao Tile.

            tileData.sprite = defaultTileSprite;
            tileData.gameObject = defaultTileGameObject;
            tileData.colliderType = defaultTileColliderType;
            tileData.transform = matrixTransform;
            tileData.flags = TileFlags.LockTransform;

            CheckRuleCanReplaceTileData(tilePosition, iTilemap, ref tileData, matrixTransform);
        }

        public override bool GetTileAnimationData(Vector3Int tilePosition, ITilemap iTilemap, ref TileAnimationData tileAnimationData) {
            Matrix4x4 matrixTransform = Matrix4x4.identity;

            /*
             * Se uma das Rules for um Tile com animação, é preciso definir o `tileAnimationData`
             * com os dados da animação.
             * 
             * Mas, antes é necessário verificar se o Rule é correspondido.
             */
            foreach (Rule rule in rules) {
                if (rule.spriteOutputType == Rule.SpriteOutputType.Animation) {
                    bool neighborsMatched = IsNeighborsMatched(rule, tilePosition, iTilemap, ref matrixTransform);

                    if (neighborsMatched) {
                        // Replace Tile data.
                        tileAnimationData.animatedSprites = rule.tileSprites;
                        tileAnimationData.animationSpeed = rule.animationSpeed;

                        return true;
                    }
                }
            }

            return false;
        }

        public override void RefreshTile(Vector3Int tilePosition, ITilemap iTilemap) {
            OnEnterOrLeaveWithAnyTilePaintToolInTilemap(tilePosition, iTilemap);
        }
        #endregion

        private void OnEnterOrLeaveWithAnyTilePaintToolInTilemap(Vector3Int tilePosition, ITilemap iTilemap) {
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
             * 
             * Refresh apenas no Tile atual.
             */
            base.RefreshTile(tilePosition, iTilemap);

            Tilemap tilemap = iTilemap.GetComponent<Tilemap>();

            // Refresh nos Tiles que estão ao redor do Tile atual.
            foreach (Vector3Int tileAroundOffsetPosition in neighborsBoxesPositionOnMatrix) {
                Vector3Int tileAroundPosition = GetNextTilePosition(tilePosition, tileAroundOffsetPosition);

                // O Refresh só acontecerá se o `aroundTile` for um ScriptedTile.

                TileBase aTile = tilemap.GetTile(tileAroundPosition);
                ScriptedTile scriptedTile = null;

                if (aTile is ScriptedTile) {
                    scriptedTile = aTile as ScriptedTile;

                    if (scriptedTile) {
                        base.RefreshTile(tileAroundPosition, iTilemap);
                    }
                }
            }
        }

        private void CheckRuleCanReplaceTileData(Vector3Int tilePosition, ITilemap iTilemap, ref TileData tileData, Matrix4x4 matrixTransform) {
            /**
             * Cada Rule contém uma sua regra de substituição, os Neighbors.
             * Por exemplo, esta Rule só irá substituir um Tile se, tiver um Tile aqui, não tiver ali e, tiver aqui.
             * 
             * Para que uma Rule possa substituir os dados de um Tile, os Neighbors devem ser
             * correspondidos.
             * 
             * As Rules serão verificadas uma a uma, até que os Neighbors de uma delas sejam correspondidos.
             * Se os Neighbors de nenhuma Rule for correspondido, o Tile usurá seus dados padrão.
             */
            foreach (Rule rule in rules) {
                Matrix4x4 transform = matrixTransform;

                bool neighborsMatched = IsNeighborsMatched(rule, tilePosition, iTilemap, ref transform);

                if (neighborsMatched) {
                    ReplaceTileData(rule, tilePosition, ref tileData, transform);

                    /*
                     * O restante das Rules não precisam ser verificadas.
                     * 
                     * É por causa disso que a ordem das Rules na lista é importante.
                     * É a primeira Rule correspondida que irá substituir os dados do Tile.
                     */
                    break;
                }
            }
        }

        #region Neighbors matches checking.
        private bool IsNeighborsMatched(Rule rule, Vector3Int tilePosition, ITilemap iTilemap, ref Matrix4x4 transform) {
            #region @CLASSROOM A lógica do NextTile Selection Type.
            /**
             * A regra de substituição (Neighbors) possui 8 NeighborsBox.
             *   0 1 2
             *   3 - 5
             *   6 7 8
             * O - é o Tile atual. O que, talvez, será substituído.
             * 
             * Cada NeighborBox possui uma regra: MustHave, MustNotHave and, Whatever.
             * Cada NeighborBox será verificado se, o Tile respectivo ao NeighborBox corresponde a regra.
             * Essa é a funcionalidade padrão: Fixed.
             * 
             * Mas, existem 4 outros tipos de verificação: Rotated, MirrorX, MirrorY and, MirrorXY.
             * 
             * - Rotated:
             * Se no tipo Fixed, o Tile 3, seria verificado apenas com NeighborBox 3, no tipo Rotated,
             * os Tile 3, 1, 5 e 7 serão verificados com o NeighborBox 3. Percebeu? Os nextTiles foram
             * rotacionados em 90°, e verificados um a um se, algum corresponderia a regra no NeighborBox.
             * - MirrorX:
             * Se no tipo Fixed, o Tile 3, seria verificado apenas com NeighborBox 3, no tipo MirrorX,
             * o Tile 5, será verificado com o NeighborBox 3. Percebeu? O nextTile foi invertido no eixo X e, 
             * verificado se, corresponderia a regra no NeighborBox.
             * - MirrorY:
             * Se no tipo Fixed, o Tile 2, seria verificado apenas com NeighborBox 2, no tipo MirrorY,
             * o Tile 8, será verificado com o NeighborBox 2. Percebeu? O nextTile foi invertido no eixo Y e, 
             * verificado se, corresponderia a regra no NeighborBox.
             * - MirrorXY:
             * Os tipos MirrorX e/ou MirrorY entrarão em ação. Depende de qual corresponder ao NeighborBox.
             */
            #endregion

            if (IsFixedNeighborsMatched(rule, tilePosition, iTilemap)) {
                transform = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, 0f), Vector3.one);

                return true;
            }

            if (rule.nextTileSelectionType == ScriptedTile.RuleTileData.RotationType.Rotated) {
                // 4 nextTiles 90° de distância um do outro serão verificados no mesmo NeighborBox.
                for (int rotation = 90; rotation < 360; rotation += 90) {
                    if (IsRotatedNeighborsMatched(rule, tilePosition, iTilemap, rotation)) {
                        transform = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, -rotation), Vector3.one);

                        return true;
                    }
                }
            } else if (rule.nextTileSelectionType == ScriptedTile.RuleTileData.RotationType.MirrorX) {
                // Verifica invertendo o nextTile no eixo X.
                if (IsMirroredNeighborsMatched(rule, tilePosition, iTilemap, true, false)) {
                    transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1f, 1f, 1f));

                    return true;
                }
            } else if (rule.nextTileSelectionType == ScriptedTile.RuleTileData.RotationType.MirrorY) {
                // Verifica invertendo o nextTile no eixo Y.
                if (IsMirroredNeighborsMatched(rule, tilePosition, iTilemap, false, true)) {
                    transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1f, -1f, 1f));

                    return true;
                }
            } else if (rule.nextTileSelectionType == ScriptedTile.RuleTileData.RotationType.MirrorXY) {
                // Verifica invertendo o nextTile nos eixos X e Y.
                if (IsMirroredNeighborsMatched(rule, tilePosition, iTilemap, true, true)) {
                    transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1f, -1f, 1f));

                    return true;
                }

                // Verifica invertendo o nextTile no eixo X.
                if (IsMirroredNeighborsMatched(rule, tilePosition, iTilemap, true, false)) {
                    transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1f, 1f, 1f));

                    return true;
                }

                // Verifica invertendo o nextTile no eixo Y.
                if (IsMirroredNeighborsMatched(rule, tilePosition, iTilemap, false, true)) {
                    transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1f, -1f, 1f));

                    return true;
                }
            }

            return false;
        }

        private bool IsFixedNeighborsMatched(Rule rule, Vector3Int tilePosition, ITilemap iTilemap) {
            return IsRotatedNeighborsMatched(rule, tilePosition, iTilemap, 0);
        }

        private bool IsRotatedNeighborsMatched(Rule rule, Vector3Int tilePosition, ITilemap iTilemap, int rotation) {
            // Cada NeighborBox e sua respectiva RuleCode será verificada.
            for (int neighborIndex = 0; neighborIndex < rule.neighborsRuleCode.Count && neighborIndex < rule.neighborsBoxesPositionOnMatrix.Count; neighborIndex++) {
                int neighborRuleCode = rule.neighborsRuleCode[neighborIndex];

                Vector3Int neighborBoxPositionOnMatrix = rule.neighborsBoxesPositionOnMatrix[neighborIndex];

                // Através da rotação, calcular o offset do nextTile.
                Vector3Int nextTilePositionOffsetByRotation = GetNextTilePositionOffsetByRotation(neighborBoxPositionOnMatrix, rotation);
                // Com o offset do nextTile, calcular a posição do nextTile.
                Vector3Int nextTilePosition = GetNextTilePosition(tilePosition, nextTilePositionOffsetByRotation);

                TileBase nextTile = iTilemap.GetTile(nextTilePosition);

                // Se o NeighborBox não é correspondido, a Rule não poderá substituir os dados do Tile.
                if (!IsNeighborBoxMatched(neighborRuleCode, nextTile)) {
                    return false;
                }
            }

            // Todos os NeighborBoxes foram correspondidos, logo, a Rule poderá substituir os dados do Tile.
            return true;
        }

        private bool IsMirroredNeighborsMatched(Rule rule, Vector3Int tilePosition, ITilemap iTilemap, bool isMirrorX, bool isMirrorY) {
            // Cada NeighborBox e sua respectiva RuleCode será verificada.
            for (int neighborIndex = 0; neighborIndex < rule.neighborsRuleCode.Count && neighborIndex < rule.neighborsBoxesPositionOnMatrix.Count; neighborIndex++) {
                int neighborRuleCode = rule.neighborsRuleCode[neighborIndex];

                Vector3Int neighborBoxPositionOnMatrix = rule.neighborsBoxesPositionOnMatrix[neighborIndex];

                // O offset do nextTile é o oposto do NeighborBox.
                Vector3Int nextTilePositionOffsetByMirrorAxis = GetNextTilePositionOffsetByMirrorAxis(neighborBoxPositionOnMatrix, isMirrorX, isMirrorY);
                // Com o offset do nextTile, calcular a posição do nextTile.
                Vector3Int nextTilePosition = GetNextTilePosition(tilePosition, nextTilePositionOffsetByMirrorAxis);

                TileBase nextTile = iTilemap.GetTile(nextTilePosition);

                // Se o NeighborBox não é correspondido, a Rule não poderá substituir os dados do Tile.
                if (!IsNeighborBoxMatched(neighborRuleCode, nextTile)) {
                    return false;
                }
            }

            // Todos os NeighborBoxes foram correspondidos, logo, a Rule poderá substituir os dados do Tile.
            return true;
        }

        private bool IsNeighborBoxMatched(int neighborRuleCode, TileBase nextTile) {
            ScriptedTile thisScriptedTile = this;

            switch (neighborRuleCode) {
                /*
                    * Se o NeighborBox pede um MustHave, e o Tile (`nextTile`) respectivo ao NeighborBoxPositionOnMatrix
                    * pertencer ao mesmo ScriptedTile, os outros NeighborBox devem continuar sendo verificados, pois, talvez,
                    * todos os Neighbors serão correspondidos e, o Rule poderá substituir os dados do Tile.
                    * 
                    * Se não for do mesmo ScriptedTile, o NeighborBox não é correspondido e a Rule não poderá substituir os dados do Tile.
                    */
                case RuleTileData.NeighborRuleCode.MustHave:
                    return nextTile == thisScriptedTile;
                /*
                    * Se o NeighborBox pede um MustNotHave, e o Tile (`nextTile`) respectivo ao NeighborBoxPositionOnMatrix
                    * pertencer a outro ScriptedTile (outro Tile), os outros NeighborBox devem continuar sendo verificados, pois, talvez,
                    * todos os Neighbors serão correspondidos e, o Rule poderá substituir os dados Tile.
                    * 
                    * Se for do mesmo ScriptedTile, o NeighborBox não é correspondido e a Rule não poderá substituir os dados do Tile.
                    */
                case RuleTileData.NeighborRuleCode.MustNotHave:
                    return nextTile != thisScriptedTile;
            }

            return true;
        }

        #region Helpers.
        private Vector3Int GetNextTilePositionOffsetByRotation(Vector3Int neighborBoxPositionOnMatrix, int rotation) {
            switch (rotation) {
                case 0:
                    return neighborBoxPositionOnMatrix;
                case 90:
                    return new Vector3Int(neighborBoxPositionOnMatrix.y, -neighborBoxPositionOnMatrix.x, 0);
                case 180:
                    return new Vector3Int(-neighborBoxPositionOnMatrix.x, -neighborBoxPositionOnMatrix.y, 0);
                case 270:
                    return new Vector3Int(-neighborBoxPositionOnMatrix.y, neighborBoxPositionOnMatrix.x, 0);
            }

            return neighborBoxPositionOnMatrix;
        }

        private Vector3Int GetNextTilePositionOffsetByMirrorAxis(Vector3Int neighborBoxPositionOnMatrix, bool isMirrorX, bool isMirrorY) {
            if (isMirrorX) {
                neighborBoxPositionOnMatrix.x *= -1;
            }

            if (isMirrorY) {
                neighborBoxPositionOnMatrix.y *= -1;
            }

            return neighborBoxPositionOnMatrix;
        }

        private Vector3Int GetNextTilePosition(Vector3Int currentTilePosition, Vector3Int nextPosition) {
            return currentTilePosition + nextPosition;
        }
        #endregion
        #endregion

        private float GetPerlinNoiseValue(Vector3Int tilePosition, float perlinNoiseScale, float offset) {
            float x = (tilePosition.x + offset) * perlinNoiseScale;
            float y = (tilePosition.y + offset) * perlinNoiseScale;

            return Mathf.PerlinNoise(x, y);
        }

        private void ReplaceTileData(Rule rule, Vector3Int tilePosition, ref TileData tileData, Matrix4x4 matrixTransform) {
            switch (rule.spriteOutputType) {
                case Rule.SpriteOutputType.Single:
                case Rule.SpriteOutputType.Animation:
                    tileData.sprite = rule.tileSprites[0];
                    break;
                case Rule.SpriteOutputType.Random:
                    int firstSpriteIndex = 0;
                    int lastSpriteIndex = rule.tileSprites.Length;
                    int randomSpriteIndex = Mathf.FloorToInt(GetPerlinNoiseValue(tilePosition, rule.perlinNoiseScale, 100000f) * lastSpriteIndex);

                    int spriteIndex = Mathf.Clamp(randomSpriteIndex, firstSpriteIndex, lastSpriteIndex - 1);

                    tileData.sprite = rule.tileSprites[spriteIndex];

                    // Random Sprite rotation.
                    if (rule.randomSpriteRotationType != Rule.RotationType.Fixed) {
                        matrixTransform = GetSpriteRandomRotation(rule.randomSpriteRotationType, matrixTransform, rule.perlinNoiseScale, tilePosition);
                    }
                    break;
            }

            tileData.transform = matrixTransform;
            tileData.gameObject = rule.tileGameObject;
            tileData.colliderType = rule.tileColliderType;
        }

        private Matrix4x4 GetSpriteRandomRotation(Rule.RotationType randomSpriteRotationType, Matrix4x4 transform, float perlinNoiseScale, Vector3Int tilePosition) {
            Vector3 position = Vector3.zero;
            Quaternion rotation = Quaternion.identity;
            Vector3 scale = Vector3.one;

            float perlinNoiseScaleValue = GetPerlinNoiseValue(tilePosition, perlinNoiseScale, 200000f);

            switch (randomSpriteRotationType) {
                // Rotaciona o Sprite.
                case RuleTileData.RotationType.Rotated:
                    int angle = Mathf.Clamp(
                        Mathf.FloorToInt(perlinNoiseScaleValue * 4),
                        0,
                        4 - 1
                    );

                    rotation = Quaternion.Euler(0f, 0f, -(angle * 90));

                    return transform * Matrix4x4.TRS(position, rotation, scale);

                // Inverter ou não o Sprite no eixo X.
                case RuleTileData.RotationType.MirrorX:
                    scale = new Vector3(
                        perlinNoiseScaleValue < 0.5 ? 1f : -1f,
                        1f,
                        1f
                    );

                    return transform * Matrix4x4.TRS(position, rotation, scale);

                // Inverter ou não o Sprite no eixo Y.
                case RuleTileData.RotationType.MirrorY:
                    scale = new Vector3(
                        1f,
                        // Inverter ou não o Sprite no eixo Y.
                        perlinNoiseScaleValue < 0.5 ? 1f : -1f,
                        1f
                    );

                    return transform * Matrix4x4.TRS(position, rotation, scale);

                // Inverter ou não o Sprite nos eixos X e/ou Y.
                case RuleTileData.RotationType.MirrorXY:
                    scale = new Vector3(
                        Math.Abs(perlinNoiseScaleValue - 0.5) > 0.25 ? 1f : -1f,
                        perlinNoiseScaleValue < 0.5 ? 1f : -1f,
                        1f
                    );

                    return transform * Matrix4x4.TRS(position, rotation, scale);
            }

            return transform;
        }
    }
}