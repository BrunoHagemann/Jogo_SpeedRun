using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Unity.FPS.AI;
using Unity.FPS.Game;

public static class CreateAttackBotEnemyPrefab
{
    const string HoverBotPath = "Assets/FPS/Prefabs/Enemies/Enemy_HoverBot.prefab";
    const string AttackBotModelPath = "Assets/AttackBot/attackBot.prefab";
    const string OutputPrefabPath = "Assets/FPS/Prefabs/Enemies/Enemy_attackBot.prefab";
    const string ControllerPath = "Assets/AttackBot/AttackBot_Enemy.controller";
    const string BreathingClipPath = "Assets/AttackBot/breathing.anim";

    [MenuItem("Tools/FPS/Create Enemy AttackBot")]
    static void Create()
    {
        EnsureEnemyAnimatorController();

        GameObject root = PrefabUtility.LoadPrefabContents(HoverBotPath);
        try
        {
            root.name = "Enemy_attackBot";
            EnemyMobile mobile = root.GetComponent<EnemyMobile>();
            DetectionModule detection = root.GetComponentInChildren<DetectionModule>(true);
            WeaponController weapon = root.GetComponentInChildren<WeaponController>(true);
            if (mobile == null || detection == null || weapon == null)
                throw new System.Exception("Enemy_HoverBot está sem EnemyMobile, DetectionModule ou WeaponController.");
            mobile.RandomHitSparks = System.Array.Empty<ParticleSystem>();
            mobile.OnDetectVfx = System.Array.Empty<ParticleSystem>();

            Transform oldModel = root.transform.Find("BasicRobot");
            if (oldModel == null)
                oldModel = FindChildNamed(root.transform, "BasicRobot");
            if (oldModel != null)
                Object.DestroyImmediate(oldModel.gameObject);

            GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AttackBotModelPath);
            if (modelPrefab == null)
                throw new System.Exception("Modelo não encontrado: " + AttackBotModelPath);

            GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab, root.transform);
            model.name = "AttackBotModel";
            model.transform.localPosition = Vector3.zero;
            // The imported vehicle's forward axis is local +X; align it with
            // the FPS enemy's +Z forward axis so it faces the player.
            model.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);

            Renderer[] renderers = model.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
                throw new System.Exception("O prefab attackBot não contém Renderers.");
            Bounds modelBounds = renderers[0].bounds;
            foreach (Renderer renderer in renderers)
                modelBounds.Encapsulate(renderer.bounds);
            model.transform.position += Vector3.up * -modelBounds.min.y;

            Animator animator = model.GetComponentInChildren<Animator>(true);
            if (animator == null)
                throw new System.Exception("O modelo attackBot não contém Animator.");
            animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);
            mobile.Animator = animator;
            detection.Animator = animator;

            Transform weaponTransform = weapon.transform;
            Transform primary = CreateMuzzle(weaponTransform, "PontoDeDisparo_Esquerdo",
                new Vector3(-0.85f, 0f, 0.32f));
            Transform secondary = CreateMuzzle(weaponTransform, "PontoDeDisparo_Direito",
                new Vector3(0.85f, 0f, 0.32f));
            weapon.WeaponMuzzle = primary;

            EnemyDualMuzzleFire dualFire = weapon.GetComponent<EnemyDualMuzzleFire>();
            if (dualFire == null)
                dualFire = weapon.gameObject.AddComponent<EnemyDualMuzzleFire>();
            dualFire.SecondaryMuzzle = secondary;

            PrefabUtility.SaveAsPrefabAsset(root, OutputPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Prefab Enemy_attackBot criado em " + OutputPrefabPath);
    }

    static Transform CreateMuzzle(Transform parent, string name, Vector3 localPosition)
    {
        Transform muzzle = parent.Find(name);
        if (muzzle == null)
        {
            GameObject point = new GameObject(name);
            muzzle = point.transform;
            muzzle.SetParent(parent, false);
        }
        muzzle.localPosition = localPosition;
        muzzle.localRotation = Quaternion.identity;
        muzzle.localScale = Vector3.one;
        return muzzle;
    }

    static Transform FindChildNamed(Transform parent, string name)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
            if (child.name == name)
                return child;
        return null;
    }

    static void EnsureEnemyAnimatorController()
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ControllerPath));
            controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        }

        AddParameterIfMissing(controller, "MoveSpeed", AnimatorControllerParameterType.Float);
        AddParameterIfMissing(controller, "Attack", AnimatorControllerParameterType.Trigger);
        AddParameterIfMissing(controller, "Alerted", AnimatorControllerParameterType.Bool);
        AddParameterIfMissing(controller, "OnDamaged", AnimatorControllerParameterType.Trigger);

        AnimationClip breathing = AssetDatabase.LoadAssetAtPath<AnimationClip>(BreathingClipPath);
        if (breathing != null && controller.layers.Length > 0)
        {
            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            AnimatorState state = stateMachine.defaultState;
            if (state != null)
                state.motion = breathing;
        }
        EditorUtility.SetDirty(controller);
    }

    static void AddParameterIfMissing(AnimatorController controller, string name,
        AnimatorControllerParameterType type)
    {
        foreach (AnimatorControllerParameter parameter in controller.parameters)
            if (parameter.name == name)
                return;
        controller.AddParameter(name, type);
    }
}
