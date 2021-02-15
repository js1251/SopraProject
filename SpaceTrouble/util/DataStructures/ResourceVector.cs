using System;
using Newtonsoft.Json;
using SpaceTrouble.GameObjects.Tiles.Interfaces;

namespace SpaceTrouble.util.DataStructures {
    public struct ResourceVector {
        [JsonProperty] internal int Mass { get; private set; }
        [JsonProperty] internal int Energy { get; private set; }
        [JsonProperty] internal int Food { get; private set; }
        [JsonProperty] private int UnrefinedMass { get; set; }

        internal ResourceVector(int mass, int energy, int food, int unrefinedMass = 0) {
            Mass = mass;
            Energy = energy;
            Food = food;
            UnrefinedMass = unrefinedMass;
        }

        internal ResourceVector TransferResources(IResourceContainer container) {
            container.Resources += this;
            ResourceVector leftoverResources;
            (container.Resources, leftoverResources) = container.Resources.CapResources(container.ResourceCapacity);

            return leftoverResources;
        }

        private (ResourceVector, ResourceVector) CapResources(ResourceVector limit) {
            var leftoverResources = new ResourceVector(0, 0, 0);
            if (Mass > limit.Mass) {
                leftoverResources.Mass = Mass - limit.Mass;
                Mass = limit.Mass;
            }

            if (Energy > limit.Energy) {
                leftoverResources.Energy = Energy - limit.Energy;
                Energy = limit.Energy;
            }

            if (Food > limit.Food) {
                leftoverResources.Food = Food - limit.Food;
                Food = limit.Food;
            }

            if (UnrefinedMass > limit.UnrefinedMass) {
                leftoverResources.UnrefinedMass = UnrefinedMass - limit.UnrefinedMass;
                UnrefinedMass = limit.UnrefinedMass;
            }

            return (this, leftoverResources);
        }

        public (ResourceVector, ResourceVector) SubtractResources(ResourceVector resource) {
            Mass -= resource.Mass;
            Energy -= resource.Energy;
            Food -= resource.Food;
            UnrefinedMass -= resource.UnrefinedMass;

            var leftoverResources = new ResourceVector(0, 0, 0);
            if (Mass < 0) {
                leftoverResources.Mass = -Mass;
                Mass = 0;
            }

            if (Energy < 0) {
                leftoverResources.Energy = -Energy;
                Energy = 0;
            }

            if (Food < 0) {
                leftoverResources.Food = -Food;
                Food = 0;
            }

            if (UnrefinedMass < 0) {
                leftoverResources.UnrefinedMass = -UnrefinedMass;
                UnrefinedMass = 0;
            }

            return (this, leftoverResources);
        }

        internal static ResourceVector MassUnit { get; } = new ResourceVector(1, 0, 0);
        internal static ResourceVector EnergyUnit { get; } = new ResourceVector(0, 1, 0);
        internal static ResourceVector FoodUnit { get; } = new ResourceVector(0, 0, 1);
        internal static ResourceVector UnrefinedMassUnit { get; } = new ResourceVector(0, 0, 0, 1);
        internal static ResourceVector Empty { get; } = new ResourceVector(0, 0, 0);

        public static ResourceVector operator +(ResourceVector left, ResourceVector right) {
            return new ResourceVector(left.Mass + right.Mass, left.Energy + right.Energy, left.Food + right.Food, left.UnrefinedMass + right.UnrefinedMass);
        }

        public static ResourceVector operator -(ResourceVector left, ResourceVector right) {
            return new ResourceVector(left.Mass - right.Mass, left.Energy - right.Energy, left.Food - right.Food, left.UnrefinedMass - right.UnrefinedMass);
        }

        public static ResourceVector operator *(ResourceVector resources, int scalar) {
            return new ResourceVector(resources.Mass * scalar, resources.Energy * scalar, resources.Food * scalar, resources.UnrefinedMass * scalar);
        }

        internal bool IsEmpty(bool includeUnrefinedMass = false) {
            return Mass == 0 && Energy == 0 && Food == 0 && (!includeUnrefinedMass || UnrefinedMass == 0);
        }

        internal bool AllLessOrEqualThan(ResourceVector resources) {
            return Mass <= resources.Mass && Energy <= resources.Energy && Food <= resources.Food && UnrefinedMass <= resources.UnrefinedMass;
        }

        /// <summary>
        /// Returns a ResourceVector containing one resource for each resource that is contained in this ResourceVector;
        /// </summary>
        /// <param name="resources"></param>
        /// <returns></returns>
        internal ResourceVector HasAny(ResourceVector resources) {
            return new ResourceVector {
                Energy = Energy > 0 && resources.Energy > 0 ? 1 : 0,
                Food = Food > 0 && resources.Food > 0 ? 1 : 0,
                Mass = Mass > 0 && resources.Mass > 0 ? 1 : 0,
                UnrefinedMass = UnrefinedMass > 0 && resources.UnrefinedMass > 0 ? 1 : 0
            };
        }

        internal ResourceVector GetRandomEntry() {
            if (IsEmpty()) {
                return this;
            }

            // TODO: this can be done better Im sure

            var random = new Random().Next(0, 4);
            if (random == 0 && Mass > 0) {
                return new ResourceVector(Mass, 0, 0);
            }

            if (random == 1 && Energy > 0) {
                return new ResourceVector(0, Energy, 0);
            }

            if (random == 2 && Food > 0) {
                return new ResourceVector(0, 0, Food);
            }

            if (random == 3 && UnrefinedMass > 0) {
                return new ResourceVector(0, 0, 0, Energy);
            }

            return GetRandomEntry();
        }

        public override string ToString() {
            return Mass + ", " + Energy + ", " + Food;
        }
    }
}