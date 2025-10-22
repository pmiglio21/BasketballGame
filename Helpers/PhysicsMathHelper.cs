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
    }
}
