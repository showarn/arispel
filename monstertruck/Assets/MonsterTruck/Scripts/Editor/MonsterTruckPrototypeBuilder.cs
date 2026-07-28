#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace MonsterTruckGame.Vehicle.Editor;

public static class MonsterTruckPrototypeBuilder
{
    private const string RootFolder = "Assets/MonsterTruck";
    private const string SpriteFolder = RootFolder + "/Sprites";
    private const string PrefabFolder = RootFolder + "/Prefabs";
    private const float PixelsPerUnit = 100.0f;

    private static readonly Vector3 ChassisPosition =
        new(-0.02500f, -1.82500f, 0f);

    private static readonly Vector3 RearWheelPosition =
        new(-1.64032f, -1.93071f, 0f);

    private static readonly Vector3 FrontWheelPosition =
        new(1.54217f, -1.92934f, 0f);

    [MenuItem("Tools/Monster Truck/Create Corrected Prototype Prefab")]
    public static void CreatePrototypePrefab()
    {
        EnsureFolder(PrefabFolder);
        ConfigureSpriteImports();

        Sprite bodySprite = LoadSprite("body.png");
        Sprite chassisSprite =
            LoadSprite("chassis_with_mount_markers.png");
        Sprite wheelSprite =
            LoadSprite("wheel_standard_centered_256.png");

        GameObject root = new("MonsterTruckPrototypeV2");

        try
        {
            GameObject bodyObject = new("BodyPhysics");
            bodyObject.transform.SetParent(root.transform, false);

            Rigidbody2D bodyRigidbody =
                bodyObject.AddComponent<Rigidbody2D>();
            bodyRigidbody.mass = 2.2f;
            bodyRigidbody.gravityScale = 3.2f;
            bodyRigidbody.linearDamping = 0.08f;
            bodyRigidbody.angularDamping = 0.25f;
            bodyRigidbody.collisionDetectionMode =
                CollisionDetectionMode2D.Continuous;

            BoxCollider2D bodyCollider =
                bodyObject.AddComponent<BoxCollider2D>();
            bodyCollider.size = new Vector2(4.45f, 1.15f);
            bodyCollider.offset = new Vector2(0f, -0.1f);

            SpriteRenderer bodyRenderer = CreateVisual(
                "Body",
                bodyObject.transform,
                bodySprite,
                Vector3.zero,
                20
            );

            SpriteRenderer chassisRenderer = CreateVisual(
                "Chassis",
                bodyObject.transform,
                chassisSprite,
                ChassisPosition,
                10
            );

            CreateWheel(
                "RearWheel",
                root.transform,
                bodyRigidbody,
                wheelSprite,
                RearWheelPosition,
                out WheelJoint2D rearJoint,
                out SpriteRenderer rearWheelRenderer
            );

            CreateWheel(
                "FrontWheel",
                root.transform,
                bodyRigidbody,
                wheelSprite,
                FrontWheelPosition,
                out WheelJoint2D frontJoint,
                out SpriteRenderer frontWheelRenderer
            );

            MonsterTruckController2D controller =
                root.AddComponent<MonsterTruckController2D>();

            SerializedObject controllerObject =
                new(controller);
            controllerObject.FindProperty(
                "vehicleBody"
            ).objectReferenceValue = bodyRigidbody;
            controllerObject.FindProperty(
                "rearWheelJoint"
            ).objectReferenceValue = rearJoint;
            controllerObject.FindProperty(
                "frontWheelJoint"
            ).objectReferenceValue = frontJoint;
            controllerObject.ApplyModifiedPropertiesWithoutUndo();

            MonsterTruckVisualCustomizer customizer =
                root.AddComponent<MonsterTruckVisualCustomizer>();

            SerializedObject customizerObject =
                new(customizer);
            customizerObject.FindProperty(
                "bodyRenderer"
            ).objectReferenceValue = bodyRenderer;
            customizerObject.FindProperty(
                "chassisRenderer"
            ).objectReferenceValue = chassisRenderer;
            customizerObject.FindProperty(
                "rearWheelRenderer"
            ).objectReferenceValue = rearWheelRenderer;
            customizerObject.FindProperty(
                "frontWheelRenderer"
            ).objectReferenceValue = frontWheelRenderer;
            customizerObject.ApplyModifiedPropertiesWithoutUndo();

            string prefabPath =
                PrefabFolder + "/MonsterTruckPrototypeV2.prefab";

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(
                root,
                prefabPath
            );

            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);

            Debug.Log(
                $"Korrigerad monstertruck-prefab skapad: {prefabPath}",
                prefab
            );
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
        }
    }

    private static SpriteRenderer CreateVisual(
        string objectName,
        Transform parent,
        Sprite sprite,
        Vector3 localPosition,
        int sortingOrder
    )
    {
        GameObject visualObject = new(objectName);
        visualObject.transform.SetParent(parent, false);
        visualObject.transform.localPosition = localPosition;

        SpriteRenderer renderer =
            visualObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = sortingOrder;

        return renderer;
    }

    private static void CreateWheel(
        string objectName,
        Transform parent,
        Rigidbody2D connectedBody,
        Sprite sprite,
        Vector3 localPosition,
        out WheelJoint2D joint,
        out SpriteRenderer renderer
    )
    {
        GameObject wheelObject = new(objectName);
        wheelObject.transform.SetParent(parent, false);
        wheelObject.transform.localPosition = localPosition;

        renderer = wheelObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 30;

        Rigidbody2D wheelBody =
            wheelObject.AddComponent<Rigidbody2D>();
        wheelBody.mass = 0.4f;
        wheelBody.gravityScale = 3.2f;
        wheelBody.collisionDetectionMode =
            CollisionDetectionMode2D.Continuous;

        CircleCollider2D wheelCollider =
            wheelObject.AddComponent<CircleCollider2D>();
        wheelCollider.radius = 0.82f;

        joint = wheelObject.AddComponent<WheelJoint2D>();
        joint.connectedBody = connectedBody;
        joint.autoConfigureConnectedAnchor = false;
        joint.anchor = Vector2.zero;
        joint.connectedAnchor = localPosition;

        JointSuspension2D suspension = joint.suspension;
        suspension.angle = 90f;
        suspension.frequency = 5.5f;
        suspension.dampingRatio = 0.85f;
        joint.suspension = suspension;
    }

    private static void ConfigureSpriteImports()
    {
        string[] guids = AssetDatabase.FindAssets(
            "t:Texture2D",
            new[] { SpriteFolder }
        );

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (
                AssetImporter.GetAtPath(path)
                is not TextureImporter importer
            )
            {
                continue;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression =
                TextureImporterCompression.Uncompressed;
            importer.spriteAlignment =
                (int)SpriteAlignment.Center;

            importer.SaveAndReimport();
        }
    }

    private static Sprite LoadSprite(string fileName)
    {
        string path = SpriteFolder + "/" + fileName;
        Sprite? sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

        if (sprite == null)
        {
            throw new InvalidOperationException(
                $"Kunde inte läsa sprite: {path}"
            );
        }

        return sprite;
    }

    private static void EnsureFolder(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath))
        {
            return;
        }

        string[] segments = folderPath.Split('/');
        string currentPath = segments[0];

        for (int index = 1; index < segments.Length; index++)
        {
            string nextPath =
                currentPath + "/" + segments[index];

            if (!AssetDatabase.IsValidFolder(nextPath))
            {
                AssetDatabase.CreateFolder(
                    currentPath,
                    segments[index]
                );
            }

            currentPath = nextPath;
        }
    }
}
#endif
