using System;

// Created by Jakob Sailer

namespace SpaceTrouble.util.Tools {
    internal static class MathExtension {
        public static float Lerp(this float value1, float value2, float amount) {
            return value1 + amount * (value2 - value1);
        }

        public static float Oscillation(float offset, float frequency, float min, float max) {
            var amplitude = Math.Abs(min - max) / 2f;
            return (float) Math.Sin(offset * frequency) * amplitude + (amplitude + min);
        }
    }
}