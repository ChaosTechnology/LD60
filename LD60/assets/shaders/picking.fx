import ChaosGraphics.SkinnedInstancing;
import ChaosGraphics.NormalMap;

void fs_Picking(
    float instanceID : INSTANCE_ID,
    out vec4 col : COLOR0
) {
    col = vec4(instanceID, 0.0, 0.0, 1.0);
}

Pass Picking
{
    VertexShader = vs_transformPosition, PASS(float INSTANCE_ID);
    FragmentShader = fs_Picking;
}
