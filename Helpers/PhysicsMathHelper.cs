using Godot;

namespace Helpers
{
    public static class PhysicsMathHelper
    {
        public static float GetHorizontalDistance(Vector3 pointA, Vector3 pointB)
        {
            float distanceX = Mathf.Abs(pointA.X - pointB.X);
            float distanceZ = Mathf.Abs(pointA.Z - pointB.Z);

            float hypotenuseDistance = Mathf.Sqrt(Mathf.Pow(distanceX, 2) + Mathf.Pow(distanceZ, 2));

            return hypotenuseDistance;
        }

        //Not used right now, but I don't want to do the math for this again
        public static Vector3 GetCartesianCoordinatesFromPolarCoordinates(float radius, float angleBetweenXAndZ, float angleBetweenXandY)
        {
            float x = radius * Mathf.Sin(angleBetweenXandY) * Mathf.Cos(angleBetweenXAndZ);
            float y = radius * Mathf.Cos(angleBetweenXandY);
            float z = radius * Mathf.Sin(angleBetweenXandY) * Mathf.Sin(angleBetweenXAndZ);

            return new Vector3(x, y, z);
        }
    }
}
