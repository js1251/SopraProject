using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceTrouble.GameObjects.Tiles;
using SpaceTrouble.GameObjects.Tiles.Interfaces;
using SpaceTrouble.util.DataStructures;
using SpaceTrouble.World;

// Created by Jakob Sailer

namespace SpaceTrouble.util.Tools {
    internal sealed class MeshQuad {
        internal Vector2[] Vertices { get; } // all vertices making up the quad
        internal Tile Entrance { get; } // connecting tiles that make up a entrance to this quad
        internal Dictionary<Vector2, MeshQuad> Nodes { get; } // nodes are exactly between two connecting quads
        internal List<Vector2[]> Lines { get; } // visual representation for connections between quads
        internal Color Color { get; } // the color of the visual representation of this quad

        internal MeshQuad(Vector2[] vertices, Tile entrance = null) {
            Vertices = vertices;
            Entrance = entrance;

            Lines = new List<Vector2[]>();
            Nodes = new Dictionary<Vector2, MeshQuad>();

            Color = DrawExtension.RandomColor();
        }
    }

    internal sealed class NavigationManager {
        public bool IsWorldCreation { get; set; }

        // NavMesh and NodeGraph
        public List<MeshQuad> ConvexHulls { get; } // a list of all nav-quads
        private Dictionary<Vector2, List<Vector2>> Graph { get; } // a nodegraph connecting all transitions between nav-quads
        private readonly ObjectManager mObjectManager; // the ObjectManager from the games GameState
        private Dictionary<Tile, List<MeshQuad>> mCheckedTiles;

        // Reachable Tiles for Ai
        private Dictionary<ObjectProperty, List<Tile>> ReachableTiles { get; }

        // AStar
        private Dictionary<Vector2[], Stack<Vector2>> mKnownPathsBetweenTiles = new Dictionary<Vector2[], Stack<Vector2>>();
        private Dictionary<Vector2[], Stack<Vector2>> mKnownPathsFromCreatures = new Dictionary<Vector2[], Stack<Vector2>>();

        public NavigationManager() {
            ConvexHulls = new List<MeshQuad>();
            Graph = new Dictionary<Vector2, List<Vector2>>();
            mObjectManager = WorldGameState.ObjectManager;
            mCheckedTiles = new Dictionary<Tile, List<MeshQuad>>();

            ReachableTiles = new Dictionary<ObjectProperty, List<Tile>> {
                {ObjectProperty.UnderConstruction, new List<Tile>()},
                {ObjectProperty.RequiresSpawnResources, new List<Tile>()},
                {ObjectProperty.ResourceContainer, new List<Tile>()},
                {ObjectProperty.RequiresAmmunition, new List<Tile>()}
            };
        }

        public void Update() {
            mKnownPathsFromCreatures = new Dictionary<Vector2[], Stack<Vector2>>();
        }

        public void DrawDebug(SpriteBatch spriteBatch, DebugMode mode) {
            if (mode is DebugMode.NavigationWalkingMesh) {
                foreach (var quad in ConvexHulls) {
                    // draw the quad
                    spriteBatch.DrawPolygon(quad.Vertices, quad.Vertices.Length, quad.Color, .5f);

                    // draw links between quads
                    foreach (var line in quad.Lines) {
                        spriteBatch.DrawLine(line[0], line[1], Color.Aqua, .5f);
                    }
                }

                return;
            }

            if (mode is DebugMode.NavigationWalkingNodes) {
                foreach (var (node, neighbors) in Graph) {
                    foreach (var neighbor in neighbors) {
                        spriteBatch.DrawLine(node, neighbor, Color.GreenYellow, .5f);
                    }
                }

                return;
            }

            if (mode is DebugMode.NavigationFlying) {
                // draw the vector bias
                foreach (var tile in WorldGameState.ObjectManager.DataStructure.ObjectData.GetAllTiles()) {
                    spriteBatch.DrawPoint(tile.WorldPosition, 2, Color.Red);
                    spriteBatch.DrawLine(tile.WorldPosition, tile.WorldPosition + tile.HeadingBias * 200f, Color.Red, .5f);
                }
            }
        }

        /// <summary>
        /// Returns the quad that contains the given tile
        /// </summary>
        /// <param name="tile">The tile to get the corresponding quad for</param>
        /// <returns>The MeshQuad the given tile is enclosed by.</returns>
        private List<MeshQuad> GetQuad(Tile tile) {
            if (tile != null && mCheckedTiles.ContainsKey(tile)) {
                return mCheckedTiles[tile];
            }

            return null;
        }

        /// <summary>
        /// Returns the quad that contains the given world-coordinates
        /// </summary>
        /// <param name="worldPos"></param>
        /// <returns>The MeshQuad the given world-coordinates are located in.</returns>
        public List<MeshQuad> GetQuad(Vector2 worldPos) {
            var tileAtPos = WorldGameState.ObjectManager.GetTile(CoordinateManager.WorldToTile(worldPos));
            return GetQuad(tileAtPos);
        }

        /// <summary>
        /// Updates the NavMesh for the tile-world. Should never be called each frame!!
        /// </summary>
        public void UpdateNavMesh() {
            if (!IsWorldCreation) {
                // methods for the tile-world navigation structure
                GenerateMesh(); // Generates Nav-quads on all walkable tiles
                GenerateLinks(); // Generates links between those quads
                GenerateGraph(); // Generates a node-graph from those links

                ResetReachableTiles();
                foreach (var (property, _) in ReachableTiles) {
                    GenerateReachable(property);
                }

                CheckPortalConnectivity();
                // once the navmesh has been updated the dynamic programming results from AStar are no longer valid
                mKnownPathsBetweenTiles = new Dictionary<Vector2[], Stack<Vector2>>();
            }
        }

        /// <summary>
        /// Calculates the vector bias field around all tiles of given type.
        /// </summary>
        /// <param name="type">The type of tile to do calculations for.</param>
        /// <param name="radius">The radius around the tile to create a field.</param>
        /// <param name="strength">A Force multiplier for the avoidance vector. Where 1 is 100% avoidance.</param>
        /// <param name="steerAway">If false, vectors will point towards the center tile instead.</param>
        public void UpdateAirBiasFieldForAll(GameObjectEnum type, float radius, float strength, bool steerAway = true) {
            foreach (var gameObject in mObjectManager.GetAllObjects(type)) {
                if (gameObject is Tile tile) {
                    UpdateAirBiasFieldFromTile(tile, radius, strength, steerAway);
                }
            }
        }

        /// <summary>
        /// Calculates the vector bias field around a given tile.
        /// </summary>
        /// <param name="tile">The tile around which the vector field should be calculated.</param>
        /// <param name="radius">The radius in world dimensions around the tile to create a field.</param>
        /// <param name="strength">A Force multiplier for the avoidance vector. Where 1 is 100% avoidance.</param>
        /// <param name="steerAway">If false, vectors will point towards the center tile instead.</param>
        public void UpdateAirBiasFieldFromTile(Tile tile, float radius, float strength, bool steerAway = true) {
            var tilePos = CoordinateManager.WorldToTile(tile.WorldPosition);

            var tileRadius = new Vector2(radius, radius);
            tileRadius.Y /= Global.TileHeight;
            tileRadius.X = tileRadius.Y;
            strength *= 50f;

            for (var x = -tileRadius.X; x <= tileRadius.X; x++) {
                for (var y = -tileRadius.Y; y <= tileRadius.Y; y++) {
                    var currentTile = mObjectManager.GetTile(new Vector2(tilePos.X + x, tilePos.Y + y));
                    if (currentTile == null) {
                        continue;
                    }

                    var heading = Vector2.Normalize(currentTile.WorldPosition - tile.WorldPosition);
                    var force = strength / Vector2.Distance(currentTile.WorldPosition, tile.WorldPosition);
                    heading *= force;
                    if (!steerAway) {
                        heading *= -1;
                    }

                    currentTile.HeadingBias = heading + currentTile.HeadingBias;

                    // clamp the length of the heading bias
                    const float maxTileHeadingForce = 0.1f;
                    if (currentTile.HeadingBias.Length() > maxTileHeadingForce) {
                        currentTile.HeadingBias = VectorMath.ExtendVectorFromTo(Vector2.Zero, currentTile.HeadingBias, maxTileHeadingForce);
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the total length of a given path.
        /// </summary>
        /// <param name="stack">The path as a stack.</param>
        /// <returns>Length of the path given</returns>
        public float PathLength(Stack<Vector2> stack) {
            if (stack.Count <= 0) {
                return float.PositiveInfinity;
            }

            var tempStack = new Stack<Vector2>(stack.ToArray().Reverse());
            var length = 0f;

            var lineStart = tempStack.Pop();
            while (tempStack.Count > 0) {
                var lineEnd = tempStack.Pop();
                length += Vector2.Distance(lineStart, lineEnd);
                lineStart = lineEnd;
            }

            return length;
        }

        /// <summary>
        /// Combines to given stacks into one. Second stack will be put on-top of the first.
        /// </summary>
        /// <param name="bottomStack">The stack to place on the bottom.</param>
        /// <param name="topStack">The stack to place on-top.</param>
        /// <returns>A new Stack that is a combination of the given stacks.</returns>
        public Stack<Vector2> CombineStack(Stack<Vector2> bottomStack, Stack<Vector2> topStack) {
            var tempTopStack = new Stack<Vector2>(topStack.ToList());

            var newStack = new Stack<Vector2>(bottomStack.ToList());

            while (tempTopStack.Count > 0) {
                newStack.Push(tempTopStack.Pop());
            }

            return newStack;
        }

        /// <summary>
        /// Finds a path between two points in the tile-world using AStar
        /// </summary>
        /// <param name="start">World-coordinates of the starting point.</param>
        /// <param name="end">World-coordinates of the target point.</param>
        /// <returns>A Stack of way-points that traverse the tile-world along walkable tiles</returns>
        public Stack<Vector2> FindPathOnTiles(Vector2 start, Vector2 end) {
            var path = new Stack<Vector2>();
            var startEnd = new[] {start, end};
            Dictionary<Vector2[], Stack<Vector2>> dynamicProgrammingDict;


            // figure out if path is in-between tiles
            var startIsTile = start == CoordinateManager.TileToWorld(CoordinateManager.WorldToTile(start));
            var endIsTile = end == CoordinateManager.TileToWorld(CoordinateManager.WorldToTile(end));
            if (startIsTile && endIsTile) {
                dynamicProgrammingDict = mKnownPathsBetweenTiles;
            } else {
                dynamicProgrammingDict = mKnownPathsFromCreatures;
            }

            // maybe this exact path has already been calculated
            if (dynamicProgrammingDict.ContainsKey(startEnd)) {
                return dynamicProgrammingDict[startEnd];
            }

            if (GetQuad(start) is null) {
                start = FindClosestValidTile(start);
            }

            if (GetQuad(start) != null && GetQuad(end) != null && start != end) {
                // generate a dictionary of node connections with A*
                var cameFrom = AStarSearch(start, end);

                // run down the node connections starting from the end until the next node is the start node
                var current = end;
                while (!current.Equals(start)) {
                    if (!cameFrom.ContainsKey(current)) {
                        return path;
                    }

                    path.Push(current);
                    current = cameFrom[current];
                }

                // add the start as the first way-point (could be left out but its nice to have for completion)
                path.Push(start);
            } else {
                // No valid path was found!
                path.Push(end);
            }

            // since a Stack is used there is no need to reverse the output
            // utilize dynamic programming
            dynamicProgrammingDict.Add(startEnd, path);

            return path;
        }

        private static Vector2 FindClosestValidTile(Vector2 worldPos) {
            var tilePos = CoordinateManager.WorldToTile(worldPos);
            var neighbors = new List<Tile> {
                WorldGameState.ObjectManager.GetTile(tilePos - Vector2.UnitX),
                WorldGameState.ObjectManager.GetTile(tilePos + Vector2.UnitX),
                WorldGameState.ObjectManager.GetTile(tilePos - Vector2.UnitY),
                WorldGameState.ObjectManager.GetTile(tilePos + Vector2.UnitY)
            };

            foreach (var tile in neighbors) {
                if (tile.IsWalkable) {
                    return tile.WorldPosition;
                }
            }

            // if still no valid tile was found bite the bullet and return the start
            return worldPos;
        }

        /// <summary>
        /// Generates a new Nav-mesh for all walkable and enterable tiles
        /// </summary>
        private void GenerateMesh() {
            mCheckedTiles = new Dictionary<Tile, List<MeshQuad>>();
            ConvexHulls.Clear();
            GenerateWalkable();
            GenerateEnterable();
        }

        private void GenerateWalkable() {
            foreach (var gameObject in mObjectManager.GetAllObjects(ObjectProperty.Walkable)) {
                if (gameObject is Tile tile) {
                    // check if tile is already part of a different Quad
                    if (mCheckedTiles.ContainsKey(tile)) {
                        continue;
                    }

                    var foundQuad = GenerateWalkableMeshQuad(tile);
                    ConvexHulls.Add(foundQuad);
                }
            }
        }

        private void GenerateEnterable() {
            foreach (var gameObject in mObjectManager.GetAllObjects(ObjectProperty.Enterable)) {
                if (gameObject is Tile tile) {
                    var foundQuads = GenerateEnterableMeshQuad(tile);
                    ConvexHulls.AddRange(foundQuads);
                }
            }
        }

        private MeshQuad GenerateWalkableMeshQuad(Tile tile) {
            // center tile-coordinates
            var tilePos = CoordinateManager.WorldToTile(tile.WorldPosition);

            //
            var span = RectangleFill(tilePos, 20);

            // generate vertex coordinates for the current quad
            var topLeft = CoordinateManager.TileToWorld(tilePos + new Vector2(-span[-Vector2.UnitX], -span[-Vector2.UnitY]));
            var topRight = CoordinateManager.TileToWorld(tilePos + new Vector2(span[Vector2.UnitX], -span[-Vector2.UnitY]));
            var bottomRight = CoordinateManager.TileToWorld(tilePos + new Vector2(span[Vector2.UnitX], span[Vector2.UnitY]));
            var bottomLeft = CoordinateManager.TileToWorld(tilePos + new Vector2(-span[-Vector2.UnitX], span[Vector2.UnitY]));

            // currently the vertices are on the center of a tile so offset them
            topLeft.Y -= Global.TileHeight / 3f;
            topRight.X += Global.TileWidth / 3f;
            bottomLeft.X -= Global.TileWidth / 3f;
            bottomRight.Y += Global.TileHeight / 3f;

            var vertices = new[] {
                topLeft,
                topRight,
                bottomRight,
                bottomLeft
            };

            var foundQuad = new MeshQuad(vertices);
            // mark all tiles enclosed in quad as checked
            for (var x = tilePos.X - span[-Vector2.UnitX]; x <= tilePos.X + span[Vector2.UnitX]; x++) {
                for (var y = tilePos.Y - span[-Vector2.UnitY]; y <= tilePos.Y + span[Vector2.UnitY]; y++) {
                    var enclosedTile = mObjectManager.GetTile(new Vector2(x, y));
                    if (!mCheckedTiles.ContainsKey(enclosedTile)) {
                        mCheckedTiles[enclosedTile] = new List<MeshQuad>();
                    }
                    mCheckedTiles[enclosedTile].Add(foundQuad);
                }
            }

            return foundQuad;
        }

        private Dictionary<Vector2, int> RectangleFill(Vector2 tilePos, int maxDepth = int.MaxValue) {
            var blockedDirections = new Dictionary<Vector2, bool> {
                {-Vector2.UnitX, false}, // left
                {-Vector2.UnitY, false}, // top
                {Vector2.UnitX, false}, // right
                {Vector2.UnitY, false} // bottom
            };

            var span = new Dictionary<Vector2, int> {
                {-Vector2.UnitX, 0}, // left
                {-Vector2.UnitY, 0}, // top
                {Vector2.UnitX, 0}, // right
                {Vector2.UnitY, 0} // bottom
            };

            var depth = 1;

            // there are four directions to expand towards. So if any of them aren't blocked keep going
            while (blockedDirections.Values.Contains(false) && depth < maxDepth) {
                var newBlockedDirections = new Dictionary<Vector2, bool>(blockedDirections);

                // check all four directions
                foreach (var (direction, isBlocked) in blockedDirections) {
                    // if the direction is blocked skip it
                    if (!isBlocked) {
                        /* The way this algorithm works is as such:
                         * Imagine four pyramids expanding out from the center tile but with their pointy bit towards the center.
                         * Each depth the pyramids food grows one layer (so the pyramid becomes taller but also wider). This happens for all directions.
                         * If there is something in the way the pyramid "foot" is blocked and the pyramid wont grow anymore. This directions is considered blocked.
                         * No when going to say the LEFT you have to keep in mind, that the foot of the pyramid cannot be wider than the pyramid next to it it tall.
                         * So the pyramid can become higher but no longer wider. It will look like a house of sorts. You'll have to consider this in clock- and anticlockwise direction.
                         */

                        // rotate the current direction by 90° clockwise
                        var clockwiseRotatedDirection = new Vector2 {
                            X = -direction.Y,
                            Y = direction.X
                        };

                        var antiClockwiseRotatedDirection = new Vector2 {
                            X = direction.Y,
                            Y = -direction.X
                        };

                        // the foot of the pyramid grows to both sides by one each time. so for depth 0 its 0 (in both directions); 1 its 1(in both directions); and so on...
                        // however consider what was mentioned before: the foot can never be wider than the direction its going in allows
                        for (var i = -Math.Clamp(depth, 0, span[antiClockwiseRotatedDirection]); i <= Math.Clamp(depth, 0, span[clockwiseRotatedDirection]); i++) {
                            // this is the position of the current tile of the new pyramid foot section (start at one side and iterate all the way to the other end)
                            var checkSpan = clockwiseRotatedDirection * i;

                            // here's the actual determining if a tile can be counted towards a quad or not:
                            var tileToCheck = mObjectManager.GetTile(tilePos + direction * depth + checkSpan);
                            if (tileToCheck == null || !tileToCheck.IsWalkable || mCheckedTiles.ContainsKey(tileToCheck)) {
                                newBlockedDirections[direction] = true;
                                break;
                            }
                        }

                        // if the current direction wasn't blocked in this iteration you can consider this direction to be safe to expand by one tile
                        if (!newBlockedDirections[direction]) {
                            span[direction]++;
                        }
                    }
                }

                blockedDirections = newBlockedDirections;
                depth++; // the depth is the distance from the center
            }

            return span;
        }

        private List<MeshQuad> GenerateEnterableMeshQuad(Tile tile) {
            // center tile-coordinates
            var tilePos = CoordinateManager.WorldToTile(tile.WorldPosition);

            // check for connections in all four directions for a tile
            var leftTile = mObjectManager.GetTile(tilePos - Vector2.UnitX);
            var topTile = mObjectManager.GetTile(tilePos - Vector2.UnitY);
            var rightTile = mObjectManager.GetTile(tilePos + Vector2.UnitX);
            var bottomTile = mObjectManager.GetTile(tilePos + Vector2.UnitY);

            // vertex coordinates
            Vector2 topLeft = tile.WorldPosition, topRight = tile.WorldPosition, bottomLeft = tile.WorldPosition, bottomRight = tile.WorldPosition, center = tile.WorldPosition;

            // currently the vertices are on the center of a tile so offset them
            topLeft.Y -= Global.TileHeight / 3f;
            topRight.X += Global.TileWidth / 3f;
            bottomLeft.X -= Global.TileWidth / 3f;
            bottomRight.Y += Global.TileHeight / 3f;

            var foundQuads = new List<MeshQuad>();

            if ((tilePos.X > 0) && leftTile.IsWalkable) {
                var vertices = new[] {
                    topLeft,
                    center,
                    bottomLeft
                };
                foundQuads.Add(new MeshQuad(vertices, leftTile));
            }

            if ((tilePos.Y > 0) && topTile.IsWalkable) {
                var vertices = new[] {
                    topLeft,
                    topRight,
                    center
                };
                foundQuads.Add(new MeshQuad(vertices, topTile));
            }

            if ((tilePos.X < (Global.WorldWidth - 1)) && rightTile.IsWalkable) {
                var vertices = new[] {
                    center,
                    topRight,
                    bottomRight
                };
                foundQuads.Add(new MeshQuad(vertices, rightTile));
            }

            if ((tilePos.Y < (Global.WorldWidth - 1)) && bottomTile.IsWalkable) {
                var vertices = new[] {
                    center,
                    bottomRight,
                    bottomLeft
                };
                foundQuads.Add(new MeshQuad(vertices, bottomTile));
            }

            if (foundQuads.Count > 0) {
                if (!mCheckedTiles.ContainsKey(tile)) {
                    mCheckedTiles[tile] = new List<MeshQuad>();
                }
                mCheckedTiles[tile].AddRange(foundQuads);
            }

            return foundQuads;
        }

        // TODO: generate links in intervals for big quads?
        private void GenerateLinks() {
            foreach (var quad in ConvexHulls) {
                // if its a regular quad check for all vertices
                if (quad.Vertices.Length == 4) {
                    for (var i = 0; i < quad.Vertices.Length; i++) {
                        // Generate Line to other MeshQuads on all corners. There can be a max of two links per corner.
                        var tilePos = CoordinateManager.WorldToTile(quad.Vertices[i]);
                        var endPoint1 = tilePos;
                        var endPoint2 = tilePos;

                        // TODO: make this work with polygons instead

                        if (i == 0 || i == 1) {
                            endPoint1 -= Vector2.UnitY;
                        } else {
                            endPoint1 += Vector2.UnitY;
                        }

                        if (i == 0 || i == 3) {
                            endPoint2 -= Vector2.UnitX;
                        } else {
                            endPoint2 += Vector2.UnitX;
                        }

                        var tile1 = mObjectManager.GetTile(endPoint1);
                        if (tile1 is IBuildable buildable1 && buildable1.BuildingFinished && tile1.IsWalkable) {
                            AddLink(quad, tilePos, endPoint1);
                        }

                        var tile2 = mObjectManager.GetTile(endPoint2);
                        if (tile2 != null && tile1 != null && !tile1.Equals(tile2)) {
                            // don't create a link for the same tile twice
                            if (tile2 is IBuildable buildable2 && buildable2.BuildingFinished && tile2.IsWalkable) {
                                AddLink(quad, tilePos, endPoint2);
                            }
                        }
                    }

                    // if its a enterable quad only check for the connecting side
                } else if (quad.Vertices.Length == 3) {
                    var tilePos = CoordinateManager.WorldToTile(quad.Vertices[0]);
                    AddLink(quad, tilePos, CoordinateManager.WorldToTile(quad.Entrance.WorldPosition));
                }
            }
        }

        private void AddLink(MeshQuad startQuad, Vector2 start, Vector2 end) {
            var endQuads = GetQuad(CoordinateManager.TileToWorld(end));
            if (endQuads == null) {
                return;
            }
            foreach (var endQuad in endQuads) {

                var linkStart = CoordinateManager.TileToWorld(start);
                var linkEnd = CoordinateManager.TileToWorld(end);

                var linkNode = new Vector2 {
                    X = (linkStart.X + linkEnd.X) / 2f,
                    Y = (linkStart.Y + linkEnd.Y) / 2f
                };

                // some visual representation of which quads are connected through this node
                var lines = new[] {
                    linkStart, linkEnd
                };

                if (!startQuad.Nodes.ContainsKey(linkNode)) {
                    startQuad.Lines.Add(lines); // only add lines here since they are being used for debug drawing only anyways
                    startQuad.Nodes.Add(linkNode, endQuad);
                }

                if (!endQuad.Nodes.ContainsKey(linkNode)) {
                    endQuad.Nodes.Add(linkNode, startQuad);
                }
            }
        }

        private void GenerateGraph() {
            Graph.Clear();
            foreach (var quad in ConvexHulls) {
                foreach (var node in quad.Nodes) {
                    // this could be avoided by understanding the problem better but if nodes aren't
                    // added to the connecting quad as well in AddLink some Quads don't have nodes at all!
                    if (Graph.ContainsKey(node.Key)) {
                        continue;
                    }

                    // graph has node, neighbor nodes pair
                    var neighbors = new List<Vector2>();

                    // add all nodes of the same quad as neighbors
                    neighbors.AddRange(quad.Nodes.Keys);
                    // add all nodes of the quad this node is connecting to neighbors
                    neighbors.AddRange(node.Value.Nodes.Keys);

                    Graph.Add(node.Key, neighbors);
                }
            }
        }


        private void ResetReachableTiles() {
            foreach (var entry in ReachableTiles.Keys) {
                ReachableTiles[entry].Clear();
            }
        }

        private void GenerateReachable(ObjectProperty property) {
            foreach (var gameObject in WorldGameState.ObjectManager.GetAllObjects(property)) {
                if (gameObject is Tile tile && WorldGameState.NavigationManager.GetQuad(tile) != null) {
                    ReachableTiles[property].Add(tile);
                }
            }
        }

        public List<Tile> GetReachable(ObjectProperty property) {
            return ReachableTiles[property];
        }

        private static void CheckPortalConnectivity() {
            foreach (var gameObject in WorldGameState.ObjectManager.GetAllObjects(GameObjectEnum.PortalTile)) {
                if (gameObject is PortalTile portal && WorldGameState.NavigationManager.GetQuad(portal) != null) {
                    portal.SpawnBoth = true;
                }
            }
        }

        /// <summary>
        /// Calculates a heuristic between two nodes in the tile-world. Takes objects blocking the path into account.
        /// </summary>
        /// <param name="start">The world-position of the start node.</param>
        /// <param name="end">The world-position of the end node.</param>
        /// <returns>A float representing the heuristic cost for that path.</returns>
        private static float Heuristic(Vector2 start, Vector2 end) {
            return Vector2.Distance(start, end);
        }

        private Dictionary<Vector2, Vector2> AStarSearch(Vector2 start, Vector2 end) {
            Graph.Add(end, new List<Vector2>());
            var meshQuads = GetQuad(end);
            if (meshQuads != null) {
                foreach (var meshQuad in meshQuads) {
                    Graph[end].AddRange(meshQuad.Nodes.Keys);
                }
            }

            foreach (var node in Graph[end]) {
                Graph[node].Add(end);
            }

            var costSoFar = new Dictionary<Vector2, float>();
            var cameFrom = new Dictionary<Vector2, Vector2>();

            costSoFar.Add(start, 0f);

            // frontier is a List of LinkNode-value pairs:
            // LinkNode, (float) priority, where LinkNode is:
            // link, quad the link is pointing towards
            var frontier = new PriorityQueue<Vector2>();

            // Add the starting location to the frontier with a priority of 0
            // current is the current node. Each node has a vector pointing at its exact location
            // and a second vector pointing into the quad that link is connecting
            frontier.Enqueue(start, 0f);

            //  if the end is in the same quad as the start the path is trivial
            if (GetQuad(start).Equals(GetQuad(end))) {
                cameFrom.Add(end, start);
                // simply dequeue the only queuing node (which is the start)
                frontier.Dequeue();
            }

            while (frontier.Count > 0) {
                // Get the Location from the frontier that has the lowest
                // priority, then remove that Location from the frontier
                var current = frontier.Dequeue();

                // if the current node is the same as the end node a path has been found
                if (current.Equals(end)) {
                    break;
                }

                // if current is start the neighbors are simply all nodes of the quad that encloses start
                // else its the neighbors of the current node
                var neighbors = new List<Vector2>();
                if (current == start) {
                    meshQuads = GetQuad(end);
                    if (meshQuads != null) {
                        foreach (var meshQuad in GetQuad(start)) {
                            neighbors.AddRange(meshQuad.Nodes.Keys);
                        }
                    }
                } else {
                    neighbors = Graph[current];
                }

                // all neighboring nodes
                foreach (var neighbor in neighbors) {
                    // the added cost is the euclidean distance to the neighbor link
                    var addedCost = costSoFar[current] + Heuristic(current, neighbor);

                    // If there's no cost assigned to the neighbor yet, or if the new
                    // cost is lower than the assigned one, add newCost for this neighbor
                    if (!costSoFar.ContainsKey(neighbor) || addedCost < costSoFar[neighbor]) {
                        // If we're replacing the previous cost, remove it
                        if (costSoFar.ContainsKey(neighbor)) {
                            costSoFar.Remove(neighbor);
                            cameFrom.Remove(neighbor);
                        }

                        cameFrom.Add(neighbor, current);
                        costSoFar.Add(neighbor, addedCost);

                        var heuristic = Heuristic(neighbor, end);

                        var priority = addedCost + heuristic;
                        // the next Node is the node where the current Link links to
                        frontier.Enqueue(neighbor, priority);
                    }
                }
            }

            foreach (var node in Graph[end]) {
                Graph[node].Remove(end);
            }

            Graph.Remove(end);
            return cameFrom;
        }
    }
}