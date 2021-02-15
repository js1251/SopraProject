using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using SpaceTrouble.GameObjects.Tiles;
using SpaceTrouble.GameObjects.Tiles.Interfaces;
using SpaceTrouble.util.DataStructures;
using SpaceTrouble.util.Tools;
using SpaceTrouble.World;

namespace SpaceTrouble.GameObjects.Creatures.friendly {
    internal abstract class TransportMinionAi : MinionAi {
        [JsonProperty] private ResourceVector ResourceToGet { get; set; } // the resource to get this time
        [JsonProperty] private IResourceContainer ContainerToGetResourceFrom { get; set; } // the container to get the resource from
        [JsonProperty] private IBuildable TargetToBringResourceTo { get; set; } // the target to bring the resource to
        [JsonProperty] public ObjectProperty TargetType { get; set; } // the type of the targets (eg. UnderConstruction, NeedsResourceForSpawn)
        [JsonProperty] public bool UseAStart { get; set; } // whether or not to use AStar to figure out the best path. If false algorithm will use euclidean distance
        [JsonIgnore] private ObjectManager ObjectManager { get; }
        [JsonIgnore] private NavigationManager NavigationManager { get; }
        [JsonIgnore] private float PriorityBoost { get; }

        protected TransportMinionAi(Minion minion) : base(minion) {
            Minion = minion;
            ResourceToGet = ResourceVector.Empty;
            ObjectManager = WorldGameState.ObjectManager;
            NavigationManager = WorldGameState.NavigationManager;
            PriorityBoost = 4f;
        }

        /// <summary>
        /// Called whenever the minion dies
        /// </summary>
        public override void OnCreatureDies() {
            base.OnCreatureDies();
            // Minion promised to pick up but will not be able to do so => remove promised resource from container again
            if (ContainerToGetResourceFrom != null) { // null check just in case the minions dies before ever finding a task
                ContainerToGetResourceFrom.PromisedResources -= ResourceToGet;
            }

            // if Minion promised to deliver resources but cannot do so anymore. Remove OnTheWayResources from target
            if (TargetToBringResourceTo != null) { // null check just in case the minions dies before ever finding a task
                TargetToBringResourceTo.OnTheWayResources -= ResourceToGet;
            }
        }

        /// <summary>
        /// Called whenever a transport Minion has reached a destination.
        /// </summary>
        public override void OnReachedTargetDestination() {
            var reachedTile = ObjectManager.GetTile(CoordinateManager.WorldToTile(Minion.WorldPosition));
            if (reachedTile.Equals(ContainerToGetResourceFrom) && !HasFear) {
                OnReachedContainer(reachedTile);
            } else if (reachedTile.Equals(TargetToBringResourceTo) && !reachedTile.Equals(Minion.EnteredBuilding)) {
                OnReachedTarget(reachedTile);
            } else {
                OnReachedOther(reachedTile);
            }

            base.OnReachedTargetDestination();
        }

        /// <summary>
        /// Called whenever a transport Minion has reached the resource-container to exchange resources with
        /// </summary>
        /// <param name="tile">The Tile the minion has reached.</param>
        private void OnReachedContainer(Tile tile) {
            if (Minion.CarryingResource.IsEmpty()) {
                if (tile is IResourceContainer container) {
                    Minion.CarryingResource = container.GetResources(ResourceToGet);
                } else {
                    System.Diagnostics.Debug.WriteLine("A non-ResourceContainer was targeted as a ResourceContainer! (" + tile + ")");
                }
            } else {
                System.Diagnostics.Debug.WriteLine("A Minion has reached a ResourceContainer even tho its already carrying a resource! Adding resource to Container.");
                // if the minion is already carrying a resource place it into the container
                if (tile is IResourceContainer container) {
                    container.AddResource(Minion.CarryingResource);
                    Minion.CarryingResource = ResourceVector.Empty;
                    ResourceToGet = ResourceVector.Empty;
                    Minion.Feeling = MinionFeeling.None;
                }
            }

            // the target could have been removed at this point
            if (TargetToBringResourceTo != null && WorldGameState.ObjectManager.GetTile(CoordinateManager.WorldToTile(TargetToBringResourceTo.WorldPosition)) is EmptyTile) {
                Minion.TargetDestinations = new Stack<Vector2>();
                ReturnResource();
            }
        }

        /// <summary>
        /// Called whenever a transport Minion has reached the target to carry resources to.
        /// </summary>
        /// <param name="tile">The Tile the minion has reached.</param>
        private void OnReachedTarget(Tile tile) {
            if (!Minion.CarryingResource.IsEmpty()) {
                if (tile is IBuildable buildable) {
                    if (!buildable.OnTheWayResources.HasAny(Minion.CarryingResource).IsEmpty()) {
                        // the construction was promised the resources carried by the minion
                        buildable.AddResource(Minion.CarryingResource);
                        Minion.CarryingResource = ResourceVector.Empty;
                        ResourceToGet = ResourceVector.Empty;
                    } else {
                        System.Diagnostics.Debug.WriteLine("A Minion has brought resources to a tile that it was not assigned to (force-build?)! Returning resource to container.");
                        
                        ReturnResource();
                        // display minion confusion
                        Minion.Feeling = MinionFeeling.Confused;
                    }
                } else {
                    System.Diagnostics.Debug.WriteLine("A non-Buildable was targeted by a minion as a Buildable! (" + tile + ")");
                }
            } else {
                System.Diagnostics.Debug.WriteLine("A Minion has reached a TargetTile even tho its not carrying a resource! (" + tile + ")");
            }
        }

        private void OnReachedOther(Tile tile) {
            if (HasFear || tile.Equals(Minion.EnteredBuilding)) {
                return;
            }

            System.Diagnostics.Debug.WriteLine("A Minion has brought resources to a tile that doesn't exist any more (deleted?)! Returning resource to container.");
            ReturnResource();
            Minion.Feeling = MinionFeeling.Confused;
        }

        private void ReturnResource() {
            TargetToBringResourceTo = null;
            Minion.TargetDestinations.Push(ContainerToGetResourceFrom.WorldPosition);
        }

        protected override Stack<Vector2> GetNewTargets() {
            IResourceContainer bestContainer = null;
            IBuildable bestTarget = null;
            var bestPath = new Stack<Vector2>();
            var bestDistance = float.PositiveInfinity;
            var resourceToGet = ResourceVector.Empty;

            // get all containers and constructions that are reachable
            var allContainers = NavigationManager.GetReachable(ObjectProperty.ResourceContainer);
            var allTargets = NavigationManager.GetReachable(TargetType);

            // first of all skip whole algorithm if there are no valid constructions or containers
            if (allContainers.Count <= 0 || allTargets.Count <= 0) {
                return bestPath;
            }

            // loop over all constructions
            foreach (var gameObject1 in allTargets) {
                if (!(gameObject1 is IBuildable target)) {
                    continue;
                }

                // get the required resources of the current construction
                var neededResources = target.NeededResources();

                // if the construction doesn't need any more resources, skip it
                if (neededResources.IsEmpty()) {
                    continue;
                }

                // try and find a suitable container that holds required resources for the current construction
                foreach (var gameObject2 in allContainers) {
                    if (!(gameObject2 is IResourceContainer container) || container.ContainerIsEmpty()) {
                        continue;
                    }

                    // get a ResourceVector for available resources (eg: {Food: 1, Mass: 1, ...} -> a 1 for every available).
                    // skip if that doesn't satisfy any neededResources 
                    var availableResources = container.HasAny(neededResources);
                    if (availableResources.IsEmpty()) {
                        continue;
                    }

                    // get the distance from the Minion to the container to the construction. Either by AStar or euclidean distance
                    float distance;
                    if (UseAStart) {
                        // try and find a path to that container from the current minion
                        var pathMinionToContainer = NavigationManager.FindPathOnTiles(Minion.WorldPosition, container.WorldPosition);
                        var pathContainerToConstruction = NavigationManager.FindPathOnTiles(container.WorldPosition, target.WorldPosition);
                        var totalPath = NavigationManager.CombineStack(pathContainerToConstruction, pathMinionToContainer);
                        distance = NavigationManager.PathLength(totalPath);

                        // if no path is possible the distance is infinity and this option can be skipped
                        if (float.IsPositiveInfinity(distance)) {
                            continue;
                        }
                    } else {
                        distance = Vector2.Distance(Minion.WorldPosition, container.WorldPosition);
                        distance += Vector2.Distance(container.WorldPosition, target.WorldPosition);
                    }

                    // if the current target is a prioritized target and it both needs resources and they are available somewhere
                    // set its distance to negative infinity. This guarantees that this is always the chosen target.
                    if (WorldGameState.PriorityManager.PrioritizedTiles[TargetType].Contains((Tile)target)) {
                        distance /= PriorityBoost;
                    }

                    // finally compare this new pathLength to the current bestPathLength
                    if (distance < bestDistance) {
                        bestPath = new Stack<Vector2>();
                        bestPath.Push(container.WorldPosition);
                        bestPath.Push(target.WorldPosition);

                        bestContainer = container;
                        resourceToGet = availableResources.GetRandomEntry();
                        bestTarget = target;

                        bestDistance = distance;
                    }
                }
            }

            if (!resourceToGet.IsEmpty() && bestContainer != null) {
                TargetToBringResourceTo = bestTarget;
                ContainerToGetResourceFrom = bestContainer;
                ResourceToGet = resourceToGet;

                TargetToBringResourceTo.OnTheWayResources += ResourceToGet;
                ContainerToGetResourceFrom.PromisedResources += ResourceToGet;
            }

            return bestPath;
        }
    }
}