using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.AI;
using Unity.FPS.AI;
using Unity.FPS.Game;

public static class CreateSpaceManEnemyPrefab
{
    const string HoverBotPath = "Assets/FPS/Prefabs/Enemies/Enemy_HoverBot.prefab";
    const string SpaceManModelPath = "Assets/FreeAnimatedSpaceMan/Prefab/space_man_model.prefab";
    const string IdleAnimationPath = "Assets/FreeAnimatedSpaceMan/animations_mecanim/Idle.FBX";
    const string RunAnimationPath = "Assets/FreeAnimatedSpaceMan/animations_mecanim/Run.FBX";
    const string ControllerPath = "Assets/FreeAnimatedSpaceMan/animations_mecanim/SpaceMan_EnemyAI.controller";
    const string OutputPrefabPath = "Assets/FPS/Prefabs/Enemies/Enemy_SpaceMan.prefab";

    [MenuItem("Tools/FPS/Create Enemy SpaceMan")]
    static void Create()
    {
        AnimatorController controller = CreateAnimatorController();

        GameObject root = PrefabUtility.LoadPrefabContents(HoverBotPath);
        try
        {
            root.name = "Enemy_SpaceMan";

            EnemyController enemyController = root.GetComponent<EnemyController>();
            EnemyMobile mobile = root.GetComponent<EnemyMobile>();
            Health health = root.GetComponent<Health>();
            Actor actor = root.GetComponent<Actor>();
            NavMeshAgent agent = root.GetComponent<NavMeshAgent>();
            DetectionModule detection = root.GetComponentInChildren<DetectionModule>(true);
            WeaponController weapon = root.GetComponentInChildren<WeaponController>(true);
            if (enemyController == null || mobile == null || health == null || actor == null || agent == null ||
                detection == null || weapon == null)
            {
                throw new Exception("Enemy_HoverBot está sem os componentes base necessários para o inimigo SpaceMan.");
            }

            Transform oldModel = FindChildNamed(root.transform, "BasicRobot");
            if (oldModel != null)
                UnityEngine.Object.DestroyImmediate(oldModel.gameObject);

            Transform shadowProjector = FindChildNamed(root.transform, "ShadowProjector");
            if (shadowProjector != null)
                UnityEngine.Object.DestroyImmediate(shadowProjector.gameObject);

            GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SpaceManModelPath);
            if (modelPrefab == null)
                throw new Exception("Modelo SpaceMan não encontrado: " + SpaceManModelPath);

            GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab, root.transform);
            model.name = "SpaceManModel";
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one;

            Renderer[] renderers = model.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
                throw new Exception("O prefab SpaceMan não contém Renderers.");

            Bounds bounds = renderers[0].bounds;
            foreach (Renderer renderer in renderers)
                bounds.Encapsulate(renderer.bounds);
            if (bounds.size.y <= 0.01f)
                throw new Exception("Não foi possível medir a altura do modelo SpaceMan.");

            model.transform.localScale = Vector3.one * (1.8f / bounds.size.y);
            bounds = GetCombinedBounds(model.GetComponentsInChildren<Renderer>(true));
            model.transform.position += Vector3.up * -bounds.min.y;

            Animator animator = model.GetComponentInChildren<Animator>(true);
            if (animator == null)
                throw new Exception("O prefab SpaceMan não contém Animator.");

            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            mobile.Animator = animator;
            mobile.RandomHitSparks = Array.Empty<ParticleSystem>();
            mobile.OnDetectVfx = Array.Empty<ParticleSystem>();
            mobile.MovementSound = null;
            mobile.AttackStopDistanceRatio = 0.08f;
            detection.Animator = animator;
            enemyController.EyeColorMaterial = null;
            enemyController.BodyMaterial = null;

            agent.radius = 0.4f;
            agent.height = 1.8f;
            agent.baseOffset = 0f;

            Transform hitBox = FindChildNamed(root.transform, "HitBox");
            if (hitBox != null)
            {
                hitBox.localPosition = new Vector3(0f, 0.95f, 0f);
                hitBox.localRotation = Quaternion.identity;
                SphereCollider collider = hitBox.GetComponent<SphereCollider>();
                if (collider != null)
                {
                    collider.radius = 0.38f;
                    collider.center = Vector3.zero;
                }
            }

            Transform healthBarPivot = FindChildNamed(root.transform, "HealthBarPivot");
            if (healthBarPivot != null)
                healthBarPivot.localPosition = new Vector3(0f, 2.0f, 0f);

            AudioSource movementAudio = root.GetComponent<AudioSource>();
            if (movementAudio != null)
            {
                movementAudio.Stop();
                movementAudio.clip = null;
                movementAudio.loop = false;
                movementAudio.playOnAwake = false;
            }

            PrefabUtility.SaveAsPrefabAsset(root, OutputPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Prefab Enemy_SpaceMan criado em " + OutputPrefabPath);
    }

    static AnimatorController CreateAnimatorController()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(ControllerPath));
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller != null)
            throw new Exception("O Animator Controller já existe; ele não será sobrescrito: " + ControllerPath);

        controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        controller.AddParameter("MoveSpeed", AnimatorControllerParameterType.Float);
        controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Alerted", AnimatorControllerParameterType.Bool);
        controller.AddParameter("OnDamaged", AnimatorControllerParameterType.Trigger);

        AnimationClip idle = LoadClip(IdleAnimationPath);
        AnimationClip run = LoadClip(RunAnimationPath);
        AnimatorStateMachine machine = controller.layers[0].stateMachine;

        AnimatorState idleState = machine.AddState("Idle");
        idleState.motion = idle;
        AnimatorState runState = machine.AddState("Run");
        runState.motion = run;
        machine.defaultState = idleState;

        AnimatorState attackState = machine.AddState("Attack");
        attackState.motion = idle;
        AnimatorState damagedState = machine.AddState("Damaged");
        damagedState.motion = idle;

        AnimatorStateTransition idleToRun = idleState.AddTransition(runState);
        idleToRun.hasExitTime = false;
        idleToRun.duration = 0.15f;
        idleToRun.AddCondition(AnimatorConditionMode.Greater, 0.1f, "MoveSpeed");

        AnimatorStateTransition runToIdle = runState.AddTransition(idleState);
        runToIdle.hasExitTime = false;
        runToIdle.duration = 0.15f;
        runToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "MoveSpeed");

        AnimatorStateTransition anyToAttack = machine.AddAnyStateTransition(attackState);
        anyToAttack.hasExitTime = false;
        anyToAttack.duration = 0.05f;
        anyToAttack.canTransitionToSelf = false;
        anyToAttack.AddCondition(AnimatorConditionMode.If, 0f, "Attack");
        AddLocomotionExit(attackState, idleState, runState);

        AnimatorStateTransition anyToDamaged = machine.AddAnyStateTransition(damagedState);
        anyToDamaged.hasExitTime = false;
        anyToDamaged.duration = 0.05f;
        anyToDamaged.canTransitionToSelf = false;
        anyToDamaged.AddCondition(AnimatorConditionMode.If, 0f, "OnDamaged");
        AddLocomotionExit(damagedState, idleState, runState);

        EditorUtility.SetDirty(controller);
        return controller;
    }

    static void AddLocomotionExit(AnimatorState from, AnimatorState idle, AnimatorState run)
    {
        AnimatorStateTransition toIdle = from.AddTransition(idle);
        toIdle.hasExitTime = true;
        toIdle.exitTime = 0.95f;
        toIdle.duration = 0.1f;
        toIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "MoveSpeed");

        AnimatorStateTransition toRun = from.AddTransition(run);
        toRun.hasExitTime = true;
        toRun.exitTime = 0.95f;
        toRun.duration = 0.1f;
        toRun.AddCondition(AnimatorConditionMode.Greater, 0.1f, "MoveSpeed");
    }

    static AnimationClip LoadClip(string path)
    {
        AnimationClip clip = AssetDatabase.LoadAllAssetsAtPath(path)
            .OfType<AnimationClip>()
            .FirstOrDefault(candidate => !candidate.name.StartsWith("__preview__"));
        if (clip == null)
            throw new Exception("Animação não encontrada: " + path);
        return clip;
    }

    static Bounds GetCombinedBounds(Renderer[] renderers)
    {
        Bounds bounds = renderers[0].bounds;
        foreach (Renderer renderer in renderers)
            bounds.Encapsulate(renderer.bounds);
        return bounds;
    }

    static Transform FindChildNamed(Transform parent, string name)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
            if (child.name == name)
                return child;
        return null;
    }
}
