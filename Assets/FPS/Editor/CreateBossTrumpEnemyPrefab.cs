using System.IO;
using UnityEditor;
using UnityEngine;
using Unity.FPS.AI;

public static class CreateBossTrumpEnemyPrefab
{
    const string BasePrefabPath = "Assets/FPS/Prefabs/Enemies/Enemy_HoverBot.prefab";
    const string TexturePath = "Assets/FPS/Art/Textures/Boss_Trump_.jpg";
    const string MaterialPath = "Assets/FPS/Art/Materials/Enemies/BossTrump_Billboard.mat";
    const string OutputPrefabPath = "Assets/FPS/Prefabs/Enemies/Enemy_BossTrump.prefab";
    const float BillboardHeight = 5f;

    [MenuItem("Tools/FPS/Create Enemy Boss Trump Billboard")]
    static void Create()
    {
        EnsureFolder("Assets/FPS/Art/Materials/Enemies");
        Material material = CreateBillboardMaterial();
        GameObject root = PrefabUtility.LoadPrefabContents(BasePrefabPath);
        try
        {
            root.name = "Enemy_BossTrump";

            EnemyMobile mobile = root.GetComponent<EnemyMobile>();
            if (mobile == null)
                throw new System.InvalidOperationException("Enemy_HoverBot não contém EnemyMobile.");
            mobile.Animator = null; // Este inimigo usa uma imagem estática, sem rig de animação.
            mobile.RandomHitSparks = System.Array.Empty<ParticleSystem>();
            mobile.OnDetectVfx = System.Array.Empty<ParticleSystem>();

            Transform oldModel = FindChild(root.transform, "BasicRobot");
            if (oldModel != null)
                Object.DestroyImmediate(oldModel.gameObject);

            Transform weapon = FindChild(root.transform, "Weapon_EyeLazers");
            if (weapon != null)
            {
                foreach (Renderer renderer in weapon.GetComponentsInChildren<Renderer>(true))
                    renderer.enabled = false;
            }

            CreateBillboard(root.transform, material);

            Transform hitBox = FindChild(root.transform, "HitBox");
            SphereCollider hitCollider = hitBox != null ? hitBox.GetComponent<SphereCollider>() : null;
            if (hitCollider != null)
                hitCollider.radius = 0.8f;

            PrefabUtility.SaveAsPrefabAsset(root, OutputPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Prefab Enemy_BossTrump criado em " + OutputPrefabPath);
    }

    static Material CreateBillboardMaterial()
    {
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
        if (texture == null)
            throw new FileNotFoundException("Textura não encontrada.", TexturePath);

        Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
                shader = Shader.Find("Unlit/Texture");
            if (shader == null)
                throw new System.InvalidOperationException("Não foi encontrado um shader Unlit compatível.");

            material = new Material(shader);
            AssetDatabase.CreateAsset(material, MaterialPath);
        }

        if (material.HasProperty("_BaseMap"))
            material.SetTexture("_BaseMap", texture);
        if (material.HasProperty("_MainTex"))
            material.SetTexture("_MainTex", texture);
        EditorUtility.SetDirty(material);
        return material;
    }

    static void CreateBillboard(Transform parent, Material material)
    {
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "BossTrump_Billboard";
        quad.transform.SetParent(parent, false);
        quad.transform.localPosition = new Vector3(0f, BillboardHeight * 0.5f, 0f);
        quad.transform.localRotation = Quaternion.identity;

        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
        // Use the source JPG dimensions; Unity's current NPOT import resize changes Texture2D.width/height.
        const float sourceAspect = 330f / 413f;
        float aspect = sourceAspect;
        quad.transform.localScale = new Vector3(BillboardHeight * aspect, BillboardHeight, 1f);

        Collider primitiveCollider = quad.GetComponent<Collider>();
        if (primitiveCollider != null)
            Object.DestroyImmediate(primitiveCollider);

        MeshRenderer renderer = quad.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        quad.AddComponent<EnemyBillboard>();
    }

    static Transform FindChild(Transform root, string childName)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            if (child.name == childName)
                return child;
        return null;
    }

    static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string folder = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, folder);
        }
    }
}
