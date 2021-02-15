namespace SpaceTrouble.GameObjects.Creatures.friendly {
    internal sealed class IdleMinionAi : MinionAi {

        /*
         *  This is how assigning Minions to different types could work:
         *
         *
         *  This should work. else if, else if, ... can probably be avoided.
         *  The Idle state doesn't have to be a button since you wont want to assign minion to or from the idle type.
         *  Only take Minions from other types and they will automatically be pushed to the idle type.
         *  Idk how bad it is to always take the first minion available in the category but I don't think you'll notice in-game.
         *  Technically this way the "oldest" minion switches roles most often but I doubt you can keep track of who's who.
         *
         *
         *if (pressedButton == constructionButton) {
         *    if (input == rmb) {
         *          AssignAMinionToIdle(new ConstructionMinionAi(minion);
         *    else if (input == lmb) {
         *          AssignAMinionToAi(new ConstructionMinionAi(minion));
         *    }
         * } else if (pressedButton == foodButton) {
         *      ...
         * }
         *
         * private void AssignAMinionToIdle(Ai ai) {
         *      foreach (minion in AllMinion) {
         *          if (minion.AiImplementation is ofType(ai)) {
         *              var distance = Vector2.Distance(minion.WorldPosition, minion.TargetDestination[^1]);
         *              if (bestDistance < distance) {
         *                  bestMinion = minion;
         *              }
         *          }
         *      }
         *      bestMinion.PendingAi = new IdleMinionAi(minion);
         * }
         *
         * private void AssignAMinionToAi(Ai ai) {
         *      foreach (minion in AllMinion) {
         *          if (minion.PendingAi is IdleMinionAi || minion.AiImplementation is IdleMinionAi) {
         *              minion.PendingAi = ai;
         *              return;
         *          }
         *      }
         * }
         *
         */

        public IdleMinionAi(Minion minion) : base(minion) {
        }
    }
}
