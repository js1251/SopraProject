using System;

namespace SpaceTrouble {
    public static class Program {
        [STAThread]
        static void Main()
        {
            using var game = new SpaceTrouble();
            game.Run();
        }
    }
}