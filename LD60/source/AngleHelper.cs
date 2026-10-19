using ChaosFramework.Math.Vectors;
using static ChaosFramework.Math.Trigonometry;
using static ChaosFramework.Math.Clamping;

public static class AngleHelper
{
    public static float TurnTowards(float facingAngle, Vector3f t, float deltaLimit)
    {
        float target = (float)System.Math.Atan2(t.x, t.z); // physics.state.velocity.x, physics.state.velocity.z);
        float difference = (float)System.Math.Atan2(Sin(target - facingAngle), Cos(target - facingAngle));
        float step = Clamp(-deltaLimit, deltaLimit, difference);
        return facingAngle + step;
    }
}
