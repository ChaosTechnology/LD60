import ChaosGraphics.Instancing;
import ChaosGraphics.NormalMap;
import shaders/picking_base.cfx;

void fs_Picking(
    float instanceID : INSTANCE_ID,
    vec3 worldPos : TEXCOORD1,
    out vec4 col : COLOR0
) {
    col = vec4(worldPos, instanceID);
}

void fs_sampleMaterial(
    vec2 inTex : TEXCOORD0,
    vec2 picked : INSTANCE_ID,
    out vec4 outEmissive : COLOR0,
    out vec4 outDiffuse : COLOR1,
    out vec4 outSpecular : COLOR2
) {
    sampleMaterial(inTex, outEmissive, outDiffuse, outSpecular);
    highlight(picked.y, outEmissive, outDiffuse, outSpecular);
}

Pass Picking
{
    VertexShader = vs_transformPositionAndWorld, PASS(float INSTANCE_ID);
    FragmentShader = fs_Picking;
}

Pass Material
{
    VertexShader = MATERIAL_VERTEX_SHADER, PASS(vec2 INSTANCE_ID);
    FragmentShader = fs_sampleMaterial;
}
