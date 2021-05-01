using System;

namespace SpaceTrouble {
    public static class Program {
        [STAThread]
        private static void Main() {
            using var game = new SpaceTrouble();
            game.Run();
        }
    }
}