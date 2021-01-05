using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static RayMarchingPrimitive;

[BurstCompile]
public struct World1GemRMJob : IJobParallelFor
{
    public TickBlock tickBlock;

    public CameraTransform camera;
    public DiamondSettings diamond;
    public RenderSettings render;
    public PillarSettings pillar;
    public AstroSettings astro;
    public LightSettings light;
    public NativeArray<Color32> outputColor;

    public NativeArray<float3> normals;
    public NativeArray<Color32> edgeColor;

    //[ReadOnly] public NativeArray<Color32> astroTexture;
    //public int2 astroTextureSizes;

    const float derivativeDelta = 0.0001f;
    [System.Serializable]
    public struct CameraTransform
    {
        public float3 pitchYawRoll;
        public float3 lookAtPosition;
        public float distance;
    }

    [System.Serializable]
    public struct RenderSettings
    {
        public float cameraDepth;
        public float2 scales;
        public float distanceThreshold;
        public float speed;

        public int maxStep;
        public float stopThreshold;

        public float shineIntensity;
        public int resolution;

        public Color colorForDepthMin;
        public Color colorForDepthMax;
        public float blendDepthRatio;
        public BlendingMode blendDepth;
        public int depthMaxStep;
        public float depthStepSize;
        public float depthMinSize;
        public float depthMaxSize;

        public int reflectionCount;
        public float maxReflectionDist;
    }

    [System.Serializable]
    public struct DiamondSettings
    {
        [Header("Transform")]
        public float3 diamondPosition;
        public float3 axis;
        public float3 pivot;
        public float height;
        public float boxAngle;
        public float3 cubeCutoutSizes;
        public float cubeCutoutY;
        public float sinHeightAmp;
        public float sinHeightFreq;

        [Header("Colors")]
        public Color32 edgeColor;
        public Color4Dither color4Dither;
        public const int materialID = 0;
        public Color shineColor;
        public Reflection reflection;
    }

    [System.Serializable]
    public struct PillarSettings
    {
        [Header("Transform")]
        public float3 boxTopSize;
        public float3 boxTopPosition;

        public float3 boxPillarSize;
        public float3 boxPillarPosition;

        public float3 boxBottom1Size;
        public float3 boxBottom1Position;

        public float3 boxBottom2Size;
        public float3 boxBottom2Position;

        [Header("Colors")]
        public Color4Dither color4Dither;
        public const int materialID = 1;
        public Reflection reflection;
    }

    [System.Serializable]
    public struct AstroSettings
    {
        public float3 position;

        //Box projection
        public float3 headPosition;
        public float headSize;

        public float3 bodyPos;
        public float bodySize;

        public float3 armStartPos;
        public float3 armEndStartPos;
        public float armThicc;

        public float3 legStartPos;
        public float3 legEndStartPos;
        public float legThicc;

        public float smooth;

        public float headSinAmp;
        public float headSinFreq;
        public float upperBodySinAmp;
        public float upperBodySinFreq;
        public float upperBodyOffsynch;

        [Header("Colors")]
        public Color4Dither color4Dither;
        public const int materialID = 2;
        public Reflection reflection;

        [Header("Glass")]
        public float3 glassPosition;
        public float glassSize;
        public float3 glassBoxCutoffPosition;
        public float3 glassBoxCutoffSize;
        public Color4Dither glassColor4Dither;
        public const int glassMaterialID = 3;
        public Reflection glassReflection;
    }



    [System.Serializable]
    public struct LightSettings
    {
        public float3 position;
        public float intensityMin;
        public float intensityMax;
        public float lightSin;
        public Color color;
        public bool normalize;
        public bool useSquare;
    }

    [System.Serializable]
    public struct Reflection
    {
        public float reflectNormalAjust;
        public float reflectionRatio;
        public float reflectionDistMax;
        public BlendingMode blending;
    }

    struct RayInfo
    {
        public float3 ro;
        public float3 rd;
        public int2 gridPos;
    }

    struct Result
    {
        public float3 pos;
        public float distance;
        public int numberSteps;
        public float3 normal;
        public float depth;
        public int materialID;
        public bool hasHit;
    }

    float DistanceFunction(float3 position, float t)
    {
        float diamond = DiamondDistance(position, t);
        float pillar = PillarDistance(position, t);
        float astro = AstroDistance(position, t);
        float astroGlass = AstroGlassDistance(position, t);
        return math.min(diamond, math.min(pillar, math.min(astro, astroGlass)));
    }

    int GetMaterialID(float3 position, float t)
    {
        FixedListFloat32 dists = new FixedListFloat32();
        dists.Add(DiamondDistance(position, t));
        dists.Add(PillarDistance(position, t));
        dists.Add(AstroDistance(position, t));
        dists.Add(AstroGlassDistance(position, t));

        int minID = -1;
        float minDist = float.MaxValue;
        for (int i = 0; i < 4; i++)
        {
            if (dists[i] < minDist)
            {
                minID = i;
                minDist = dists[i];
            }
        }
        return minID;
    }

    float DiamondDistance(float3 position, float t)
    {
        position = Translate(position, diamondPos);
        position = RotateAroundAxisUnsafe(position - diamond.pivot, diamond.axis, t);
        position += diamond.pivot;

        float3 pyramidPos = position;
        //Mirror pyramid on the xz plane
        pyramidPos.y = math.abs(pyramidPos.y);
        float pyramidBottom1 = sdPyramid(pyramidPos, diamond.height);
        float pyramidBottom2 = sdPyramid(RotateYQuater(pyramidPos), diamond.height);

        float3 boxPos = position;
        float cutoutCube = sdBox(boxPos + math.up() * diamond.cubeCutoutY, diamond.cubeCutoutSizes);
        return math.max(cutoutCube, math.max(pyramidBottom1, pyramidBottom2));
    }

    float PillarDistance(float3 position, float t)
    {
        float boxTop = sdBox(position - pillar.boxTopPosition, pillar.boxTopSize);
        float boxPillar = sdBox(position - pillar.boxPillarPosition, pillar.boxPillarSize);
        float boxBottom1 = sdBox(position - pillar.boxBottom1Position, pillar.boxBottom1Size);
        float boxBottom2 = sdBox(position - pillar.boxBottom2Position, pillar.boxBottom2Size);

        return math.min(boxTop, math.min(boxPillar, math.min(boxBottom1, boxBottom2)));
    }

    float AstroDistance(float3 position, float t)
    {
        position = position - astro.position;
        float head = sdSphere(position - astro.headPosition - astroHeadSinOffset, astro.headSize);
        float body = sdSphere(position - astro.bodyPos - astroUpperBodySinOffset, astro.bodySize);

        //Create X mirror
        position.x = math.abs(position.x);
        float arm = sdCapsule(position - astroUpperBodySinOffset, astro.armStartPos, astro.armEndStartPos, astro.armThicc);
        float leg = sdCapsule(position, astro.legStartPos, astro.legEndStartPos, astro.legThicc);
   
        float d = polysmin(head, body, astro.smooth);
        d = polysmin(d, arm, astro.smooth);
        d = polysmin(d, leg, astro.smooth);
        //smooth
        return d;
    }
    float AstroGlassDistance(float3 position, float t)
    {
        position = position - astro.position - astroHeadSinOffset;
        float glass = sdSphere(position - astro.glassPosition, astro.glassSize);
        float boxCutoff = sdBox(position - astro.glassBoxCutoffPosition, astro.glassBoxCutoffSize);
        return math.max(glass, boxCutoff);
    }


    float3 astroHeadSinOffset;
    float3 astroUpperBodySinOffset;

    float3 diamondPos;
    quaternion cameraRotation;
    quaternion cameraRotationInv;
    public void Execute(int index)
    {
        //precompute animation trigo
        astroHeadSinOffset = math.up() * math.sin(astro.headSinFreq * tickBlock.tick) * astro.headSinAmp;
        astroUpperBodySinOffset = math.up() * math.sin(astro.upperBodySinFreq * tickBlock.tick + astro.upperBodyOffsynch) * astro.upperBodySinAmp;
        diamond.axis = math.normalize(diamond.axis);
        diamondPos = diamond.diamondPosition + math.up() * diamond.sinHeightAmp * math.sin(diamond.sinHeightFreq * tickBlock.tick);


        int2 gridPosition = ArrayHelper.IndexToPos(index, GameManager.GridSizes);

        float2 uv = ((float2)gridPosition / GameManager.GridSizes - 0.5f) / render.scales;
        quaternion fromLookAt = quaternion.Euler(math.radians(camera.pitchYawRoll));

        float3 dir = math.mul(fromLookAt, new float3(0, 0, 1));

        float3 position = camera.lookAtPosition + dir * camera.distance;
        cameraRotation = quaternion.LookRotation(-dir, math.up());
        cameraRotationInv = math.inverse(cameraRotation);

        RayInfo rayInfo = new RayInfo();
        rayInfo.ro = position +math.mul(cameraRotation, new float3(uv.x, uv.y, 0));
        rayInfo.rd = math.mul(cameraRotation, new float3(0, 0, 1));
        rayInfo.gridPos = gridPosition;

        Result result = RayMarch(rayInfo.ro, rayInfo.rd);

        Color colorFront = CalculateColor(in rayInfo, in result, render.reflectionCount).ReduceResolution(render.resolution);

        normals[index] = result.normal;
        edgeColor[index] = GetEdgeColor(ref result);

        outputColor[index] = colorFront;
    }

    float3 GetNormal(float3 position)
    {
        float dt = derivativeDelta;

        float t = tickBlock.tick * render.speed;
        float d = DistanceFunction(position, t);
        float dx = DistanceFunction(position + new float3(dt, 0, 0), t);
        float dy = DistanceFunction(position + new float3(0, dt, 0), t);
        float dz = DistanceFunction(position + new float3(0, 0, dt), t);

        return math.normalize(new float3(dx, dy, dz) - d);
    }

    Result RayMarch(float3 ro, float3 rd)
    {
        Result result = new Result();

        float3 currentPosition = ro;
        float currentDistance = 0;
        float t = tickBlock.tick * render.speed;

        int i;
        for (i = 0; i < render.maxStep; i++)
        {
            float distance = DistanceFunction(currentPosition, t);
            currentDistance += distance;
            currentPosition += rd * distance;

            if(distance < render.stopThreshold)
            {
                result.hasHit = true;
                break;
            }
        }

        result.numberSteps = i;
        result.pos = currentPosition;
        result.distance = currentDistance;
        result.normal = GetNormal(currentPosition);
        result.depth = CalculateDepth(currentPosition, rd, t);
        result.materialID = GetMaterialID(currentPosition, t);
        return result;
    }

    float CalculateDepth(float3 currentPos, float3 rd, float t)
    {
        float depth = 0;
        for (int i = 0; i < render.depthMaxStep; i++)
        {
            currentPos += rd * render.depthStepSize;

            float distance = DistanceFunction(currentPos, t);
            if(distance >= 0)
            {
                return depth;
            }
            depth += render.depthStepSize;
        }
        return depth;
    }

    Color CalculateColor(in RayInfo rayInfo, in Result result, int reflectionCount)
    {
        //will mess with shine
        if(!result.hasHit || result.distance > render.maxReflectionDist)
            return Color.clear;

        switch (result.materialID)
        {
            case DiamondSettings.materialID: return CalculateDiamondColor(in rayInfo, in result, reflectionCount);
            case PillarSettings.materialID: return CalculatePillarColor(in rayInfo, in result, reflectionCount);
            case AstroSettings.materialID: return CalculateAstroColor(in rayInfo, in result, reflectionCount);
            case AstroSettings.glassMaterialID: return CalculateAstroGlassColor(in rayInfo, in result, reflectionCount);
        }
        return Color.clear;
    }

    Color CalculateDiamondColor(in RayInfo rayInfo, in Result result, int reflectionCount)
    {
        if (result.distance < render.distanceThreshold)
        {
            float lightT = CalculateLight(result.pos, result.normal);
            float depthRatio = result.depth / render.depthMaxSize;

            Color32 lightColor = diamond.color4Dither.GetColorWitLightValue(lightT, rayInfo.gridPos);
            return ReflectColor(lightColor, in rayInfo, in result, in diamond.reflection, reflectionCount);
        }
        else if (result.numberSteps > render.maxStep * render.shineIntensity)
        {
            float t = math.remap(render.maxStep * render.shineIntensity, render.maxStep, 0, 1, result.numberSteps);
            return t * diamond.shineColor;
        }
        return Color.clear;
    }

    Color CalculatePillarColor(in RayInfo rayInfo, in Result result, int reflectionCount)
    {
        if (result.distance < render.distanceThreshold)
        {
            float lightT = CalculateLight(result.pos, result.normal);
            Color lightColor = pillar.color4Dither.GetColorWitLightValue(lightT, rayInfo.gridPos);
            return ReflectColor(lightColor, in rayInfo, in result, in pillar.reflection, reflectionCount);
        }
        return Color.clear;
    }

    Color CalculateAstroColor(in RayInfo rayInfo, in Result result, int reflectionCount)
    {
        if (result.distance < render.distanceThreshold)
        {
            float lightT = CalculateLight(result.pos, result.normal);
            Color lightColor = astro.color4Dither.GetColorWitLightValue(lightT, rayInfo.gridPos);
            return ReflectColor(lightColor, in rayInfo, in result, in astro.reflection, reflectionCount);

            ////float lightT = CalculateLight(result.pos, result.normal);
            ////Color lightColor = pillar.color4Dither.GetColorWitLightValue(lightT, rayInfo.gridPos);
            //float3 localPos = (result.pos - astro.position) / astro.size;
            //float3 n = math.abs(result.normal);
            //Color xz = SampleTexture(MathUtils.unorm(localPos.xz), astroTexture, astroTextureSizes);
            //Color xy = SampleTexture(MathUtils.unorm(localPos.xy), astroTexture, astroTextureSizes);
            //Color yz = SampleTexture(MathUtils.unorm(localPos.yz), astroTexture, astroTextureSizes);

            //Color sampleColor = n.z * xy + n.y * xz + n.x * yz;
            ////return sampleColor;
            //Color normal = new Color(n.x, n.y, n.z);
            //Color lightColor = Color.Lerp(sampleColor, normal, 0.5f);
            ////change for astro
            //return ReflectColor(lightColor, in rayInfo, in result, in diamond.reflection, reflectionCount);
        }
        return Color.clear;
    }
    Color CalculateAstroGlassColor(in RayInfo rayInfo, in Result result, int reflectionCount)
    {
        float lightT = CalculateLight(result.pos, result.normal);
        Color lightColor = astro.glassColor4Dither.GetColorWitLightValue(lightT, rayInfo.gridPos);
        return ReflectColor(lightColor, in rayInfo, in result, in astro.glassReflection, reflectionCount);
    }


    Color ReflectColor(Color lightColor, in RayInfo rayInfo, in Result result, in Reflection reflection, int reflectionCount)
    {
        if(reflectionCount > 0)
        {
            //reflect once
            float3 reflectNormal = math.reflect(rayInfo.rd, result.normal);
            float3 ro = result.pos + reflectNormal * reflection.reflectNormalAjust;
            float3 rd = reflectNormal;

            Result reflResult = RayMarch(result.pos + reflectNormal * reflection.reflectNormalAjust, reflectNormal);
            if (reflResult.hasHit)
            {
                RayInfo newRayInfo = rayInfo;
                newRayInfo.ro = ro;
                newRayInfo.rd = rd;

                Color reflectionColor = CalculateColor(in rayInfo, in reflResult, reflectionCount - 1);
                float distFadeoff = 1 - math.saturate(reflResult.distance / reflection.reflectionDistMax);
                reflectionColor.a = reflection.reflectionRatio * distFadeoff;
                return RenderingUtils.Blend(lightColor, reflectionColor, reflection.blending);
            }
        }
        return lightColor;
    }

    float CalculateLight(float3 point, float3 normal)
    {
        float3 lightDiff = light.position - point;
        float dist = math.length(lightDiff);
        float3 lightDir = lightDiff / dist;
        
        if(light.useSquare)
            dist = dist * dist;

        float intensity = math.dot(lightDir, normal);
        if (light.normalize)
            intensity = MathUtils.unorm(intensity);

        intensity = math.saturate(intensity);

        //add view vector

        //add color??
        float lIntensity = math.remap(-1, 1, light.intensityMin, light.intensityMax, math.sin(tickBlock.tick * light.lightSin));
        return math.saturate(intensity * lIntensity / dist);
        //return/* math.saturate(MathUtils.unorm(math.dot(lightDir, normal)) + render.baseIntensity)*/;
    }

    Color32 GetEdgeColor(ref Result result)
    {
        switch (result.materialID)
        {
            case DiamondSettings.materialID: return diamond.edgeColor;
        }

        return Color.clear;
    }

    Color32 SampleTexture(float2 uv, NativeArray<Color32> texture, int2 textureSize)
    {
        int2 pos = (int2)math.floor(uv * textureSize);
        pos = math.clamp(pos, 0, textureSize - 1);
        int index = ArrayHelper.PosToIndex(pos, textureSize);
        return texture[index];
    }
}
