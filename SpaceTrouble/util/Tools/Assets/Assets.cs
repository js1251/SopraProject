using Microsoft.Xna.Framework.Content;

// Created by Jakob Sailer

namespace SpaceTrouble.util.Tools.Assets {
    internal static class Assets {

        public static Textures Textures { get; } = new Textures();
        public static Fonts Fonts { get; } = new Fonts();
        public static Sounds Sounds { get; } = new Sounds();

        public static void LoadContent(ContentManager content) {
            Textures.LoadContent(content);
            Fonts.LoadContent(content);
            Sounds.LoadContent(content);
        }
    }
}
