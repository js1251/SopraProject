namespace SpaceTrouble.GameObjects.Creatures {
    interface IEnemy {
        public float AttackDamage { get; }
        public bool IsAttacking { get; set; }
    }
}
