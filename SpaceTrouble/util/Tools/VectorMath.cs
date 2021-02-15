using System;
using Microsoft.Xna.Framework;

namespace SpaceTrouble.util.Tools {
    internal static class VectorMath {

        /// <summary>
        /// Calculates the angle of a given Vector2 relative to east being 0°.
        /// </summary>
        /// <param name="vector">The vector to get an angle from</param>
        /// <returns>An angle between 0° and 360°</returns>
        public static float VectorToAngle(Vector2 vector) {
            if (vector == Vector2.Zero) {
                return 0;
            }

            var angle = (float) (VectorToRadians(vector) * 180 / Math.PI);
            angle += 360;
            angle %= 360;

            return angle;
        }

        public static float VectorToRadians(Vector2 vector) {
            if (vector == Vector2.Zero) {
                return 0;
            }

            var normalizedHeading = Vector2.Normalize(vector);
            return (float) Math.Atan2(normalizedHeading.Y, normalizedHeading.X);
        }

        /// <summary>
        /// Linear interpolation between two angles. Includes wrapping.
        /// </summary>
        /// <param name="start">The angle to append to.</param>
        /// <param name="end">The angle to move towards.</param>
        /// <param name="amount">A factor between 0 and 1.</param>
        /// <returns>A new angle that's closer to the end angle.</returns>
        public static float LerpDegrees(float start, float end, float amount) {
            // thanks to "Rob" from StackOverflow for this code
            // https://stackoverflow.com/questions/2708476/rotation-interpolation
            // I implemented float lerp myself but it didn't work for wrapping of values

            var difference = Math.Abs(end - start);
            if (difference > 180) {
                // We need to add on to one of the values.
                if (end > start) {
                    // We'll add it on to start...
                    start += 360;
                } else {
                    // Add it on to end.
                    end += 360;
                }
            }

            // Interpolate it.
            var value = (start + ((end - start) * amount));

            // Wrap it..
            const int rangeZero = 360;

            if (value >= 0 && value <= 360) {
                return value;
            }

            return value % rangeZero;
        }

        /// <summary>
        /// Creates a vector pointing towards a target but can be extended by a given amount.
        /// </summary>
        /// <param name="origin">The origin of the desired Vector.</param>
        /// <param name="target">The point the Vector should point towards (or intersect).</param>
        /// <param name="desiredLength">The length of the output vector.</param>
        /// <returns>A new Vector2 of desired length, pointing from the origin towards the target.</returns>
        public static Vector2 ExtendVectorFromTo(Vector2 origin, Vector2 target, float desiredLength) {
            return origin + Vector2.Normalize(target - origin) * desiredLength;
        }

        /// <summary>
        /// Calculates a Vector2 at which two points traveling along both vectors with a given speed will intersect perfectly. Assumes constant speed and direction. Approximation! only works for high speeds!
        /// </summary>
        /// <param name="source">The source of the first traveling object.</param>
        /// <param name="projectileSpeed">The speed of the first object.</param>
        /// <param name="target">The position of the target to hit.</param>
        /// <param name="targetDirection">The direction the target is traveling.</param>
        /// <param name="targetSpeed">The speed of the target</param>
        /// <returns>A Vector describing a point at which both points will intersect.</returns>
        public static Vector2 PredictIntersection(Vector2 source, float projectileSpeed, Vector2 target, Vector2 targetDirection, float targetSpeed) {
            var distanceToTarget = Vector2.Distance(source, target);
            var travelTime = distanceToTarget / projectileSpeed;

            return target + targetDirection * targetSpeed * travelTime;
        }

        /// <summary>
        /// Calculates the smallest angle between two given angles.
        /// </summary>
        /// <param name="angle1">The first angle.</param>
        /// <param name="angle2">The second angle.</param>
        /// <returns>The smallest angle between the given angles.</returns>
        public static float MinAngleBetweenAngles(float angle1, float angle2) {
            var angle = Math.Abs(angle1 - angle2);
            return angle > 180 ? Math.Abs(180 - Math.Abs(angle - 180)) : angle;
        }

        public static Vector2 Rotate90ClockWise(Vector2 input) {
            return new Vector2 {
                X = -input.Y,
                Y = input.X
            };
        }
    }
}