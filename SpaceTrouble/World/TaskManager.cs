using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SpaceTrouble.GameObjects.Creatures;
using SpaceTrouble.GameObjects.Creatures.friendly;

namespace SpaceTrouble.World {
    internal enum MinionAiType {
        IdleMinionAi,
        ConstructionMinionAi,
        DefenceMinionAi,
        FoodMinionAi
    }

    internal sealed class TaskManager {
        [JsonProperty] private IDictionary<MinionAiType, List<MinionAiType>> PendingAiTransitions { get; }
        [JsonProperty] public Dictionary<MinionAiType, int> AssignedCounter { get; }
        [JsonIgnore] public Dictionary<MinionAiType, int> BusyCounter { get; private set; }
        [JsonIgnore] public Dictionary<MinionAiType, int> IdleCounter { get; private set; }
        [JsonProperty] public MinionAiType DefaultAi { get; set; }

        public TaskManager() {
            PendingAiTransitions = new Dictionary<MinionAiType, List<MinionAiType>> {
                {MinionAiType.ConstructionMinionAi, new List<MinionAiType>()},
                {MinionAiType.DefenceMinionAi, new List<MinionAiType>()},
                {MinionAiType.FoodMinionAi, new List<MinionAiType>()},
                {MinionAiType.IdleMinionAi, new List<MinionAiType>()}
            };

            AssignedCounter = new Dictionary<MinionAiType, int> {
                {MinionAiType.ConstructionMinionAi, 0},
                {MinionAiType.FoodMinionAi, 0},
                {MinionAiType.DefenceMinionAi, 0},
                {MinionAiType.IdleMinionAi, 0}
            };
            DefaultAi = MinionAiType.IdleMinionAi;
            EmptyCounters();
        }

        public void Update() {
            EmptyCounters();
            UpdateCounters();
        }

        private void EmptyCounters() {
            BusyCounter = new Dictionary<MinionAiType, int> {
                {MinionAiType.ConstructionMinionAi, 0},
                {MinionAiType.FoodMinionAi, 0},
                {MinionAiType.DefenceMinionAi, 0},
                {MinionAiType.IdleMinionAi, 0}
            };

            IdleCounter = new Dictionary<MinionAiType, int> {
                {MinionAiType.ConstructionMinionAi, 0},
                {MinionAiType.FoodMinionAi, 0},
                {MinionAiType.DefenceMinionAi, 0},
                {MinionAiType.IdleMinionAi, 0}
            };
        }

        private void UpdateCounters() {
            foreach (var gameObject in WorldGameState.ObjectManager.GetAllObjects(GameObjectEnum.Minion)) {
                if (!(gameObject is Minion minion)) {
                    continue;
                }

                var minionAiType = ParseAiToEnum(minion.AiImplementation);
                var minionIsIdle = minion.AiImplementation.IsIdle();

                if (minionIsIdle) {
                    IdleCounter[minionAiType]++;
                } else {
                    BusyCounter[minionAiType]++;
                }
            }
        }

        /// <summary>
        /// Inserts a new pending transition between two Ai's.
        /// </summary>
        /// <param name="from">The Ai to remove one assignment.</param>
        /// <param name="to">The Ai to add one assignment.</param>
        public void PushTaskFromTo(MinionAiType from, MinionAiType to) {
            if (AssignedCounter[from] <= 0) {
                return;
            }
            foreach (var (parent, toList) in PendingAiTransitions) {
                if (toList.Contains(from)) {
                    toList.Remove(from);
                    AssignedCounter[from]--;
                    AssignedCounter[parent]++;
                    PushTaskFromTo(parent, to);
                    return;
                }
            }

            // redundant entry (a -> a) can be ignored
            if (from.Equals(to)) {
                return;
            }

            AssignedCounter[from]--;
            AssignedCounter[to]++;
            PendingAiTransitions[from].Add(to);
        }

        /// <summary>
        /// Returns a new Ai depending on pending AiTransitions.
        /// </summary>
        /// <param name="minion">The minion that is asking for a new Ai.</param>
        /// <returns>A new Ai if it exists or the Ai the Minion is already implementing if not.</returns>
        public CreatureAi GetNewAi(Minion minion) {
            var minionAiType = ParseAiToEnum(minion.AiImplementation);
            if (PendingAiTransitions[minionAiType].Count > 0) {
                var newAi = CreateNewAi(PendingAiTransitions[minionAiType][0], minion);
                PendingAiTransitions[minionAiType].RemoveAt(0);
                return newAi;
            }

            // if no transition is found the minion should keep the current Ai
            return minion.AiImplementation;
        }

        /// <summary>
        /// Assigns a new Ai to the minion one last time to determine from where to safely decrement a value.
        /// </summary>
        /// <param name="minion">The minion that is dying.</param>
        public void MinionHasDied(Minion minion) {
            minion.AiImplementation = GetNewAi(minion);
            var minionAiType = ParseAiToEnum(minion.AiImplementation);
            if (PendingAiTransitions[minionAiType].Count > 0) {
                PendingAiTransitions[minionAiType].RemoveAt(0);
            }

            AssignedCounter[minionAiType]--;
        }

        /// <summary>
        /// Tells the TaskManager that a new minion has been created and increments the corresponding counter.
        /// </summary>
        /// <param name="minion">The freshly created Minion.</param>
        public void MinionHasBeenCreated(Minion minion) {
            var minionAiType = ParseAiToEnum(minion.AiImplementation);
            AssignedCounter[minionAiType]++;
        }

        private static MinionAiType ParseAiToEnum(CreatureAi ai) {
            return (MinionAiType) Enum.Parse(typeof(MinionAiType), ai.GetType().Name);
        }

        public MinionAi CreateNewAi(MinionAiType type, Minion minion) {
            return type switch {
                MinionAiType.ConstructionMinionAi => new ConstructionMinionAi(minion),
                MinionAiType.IdleMinionAi => new IdleMinionAi(minion),
                MinionAiType.DefenceMinionAi => new DefenceMinionAi(minion),
                MinionAiType.FoodMinionAi => new FoodMinionAi(minion),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}