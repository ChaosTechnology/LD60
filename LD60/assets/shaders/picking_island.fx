import WORLD_VERTEX_SHADER, MATERIAL_VERTEX_SHADER from ChaosGraphics.NormalMap;
import ChaosGraphics.VertexShaders;
import computeSpecularSharpness, MAX_SPECULAR_SHARPNESS from ChaosGraphics.MaterialDefault;
import shaders/picking_base.cfx;

int pickingIndex;
float picked;
vec4 materialIndices[128];

sampler2DArray normalSampler : NORMAL_MAP;
sampler2DArray emissiveSampler : EMISSIVE_MAP;
sampler2DArray diffuseSampler : DIFFUSE_MAP;
sampler2DArray specularSampler : SPECULAR_MAP;

void fs_Picking(
    out vec4 col : COLOR0
) {
    col = vec4(pickingIndex, 0.0, 0.0, 1.0);
}

void fs_sampleMaterial(
    vec2 inTex : TEXCOORD0,
    vec4 materialIndex : LAYERS,
    out vec4 outEmissive : COLOR0,
    out vec4 outDiffuse : COLOR1,
    out vec4 outSpecular : COLOR2
) {
	outEmissive = texture(emissiveSampler, vec3(inTex, materialIndex.y + 0.1));
	outDiffuse  = texture(diffuseSampler, vec3(inTex, materialIndex.z + 0.1));
	outSpecular = texture(specularSampler, vec3(inTex, materialIndex.w + 0.1));
    highlight(picked, outEmissive, outDiffuse, outSpecular);
}


void fs_sampleWorldNormalMap(
	vec2 inTex : TEXCOORD0,
    vec4 materialIndex : LAYERS,
	vec3 inWorldPos : TEXCOORD1,
	vec3 inNormal : TEXCOORD2,
	vec4 inTangent : TEXCOORD3,
	out vec4 outPosition : COLOR0,
	out vec4 outNormal : COLOR1,
	out vec4 outSurface : COLOR2
) {
	vec4 normalSample = texture(normalSampler, vec3(inTex, materialIndex.x + 0.1));
	mat3 tangentSpace = mat3(inTangent.xyz, cross(inNormal, inTangent.xyz) * inTangent.w, inNormal);
	outNormal = vec4(normalize((tangentSpace * (2.0 * normalSample.xyz - vec3(1.0)))), normalSample.a);
	outPosition = vec4(inWorldPos, 1.0);
	outSurface = vec4(computeSpecularSharpness(texture(specularSampler, vec3(inTex, materialIndex.w + 0.1)).a), 0, 0, 0);
}

void vs_PassMaterialIndex(
    vec2 texCoord : TEXCOORD1,
    out vec4 layers : LAYERS
) {
    layers = materialIndices[int(texCoord.x)];
}

Pass Picking
{
    VertexShader = vs_transformPosition;
    FragmentShader = fs_Picking;
}

Pass World
{
	FragmentShader = fs_sampleWorldNormalMap;
	VertexShader = WORLD_VERTEX_SHADER, vs_PassMaterialIndex;
}

Pass Material
{
	FragmentShader = fs_sampleMaterial;
	VertexShader = MATERIAL_VERTEX_SHADER, vs_PassMaterialIndex;
}