using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public static class DoomLevelGenerator
{
    private const string DOOM_SCENE_PATH = "Assets/Scenes/Doom_Level.unity";

    [MenuItem("Prototipo/Generar Nivel Secuencia DOOM en Nueva Escena")]
    public static void GenerarSecuenciaDoomNuevaEscena()
    {
        GenerarSecuenciaDoom(true);
    }

    [MenuItem("Prototipo/Generar Nivel Secuencia DOOM en Escena Actual")]
    public static void GenerarSecuenciaDoomEscenaActual()
    {
        GenerarSecuenciaDoom(false);
    }

    public static void GenerarSecuenciaDoom(bool enNuevaEscena)
    {
        if (enNuevaEscena)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            // Crear o abrir escena dedicada Doom_Level.unity
            Scene newScene;
            if (System.IO.File.Exists(DOOM_SCENE_PATH))
            {
                newScene = EditorSceneManager.OpenScene(DOOM_SCENE_PATH, OpenSceneMode.Single);
            }
            else
            {
                newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                EditorSceneManager.SaveScene(newScene, DOOM_SCENE_PATH);
                RegistrarEscenaEnBuildSettings(DOOM_SCENE_PATH);
            }
        }

        // 1. Asegurar Carpeta de Items y Crear ScriptableObjects según el GDD
        string itemsDir = "Assets/LG_Shooting/LGAssets/Items";
        if (!AssetDatabase.IsValidFolder("Assets/LG_Shooting/LGAssets"))
        {
            AssetDatabase.CreateFolder("Assets/LG_Shooting", "LGAssets");
        }
        if (!AssetDatabase.IsValidFolder(itemsDir))
        {
            AssetDatabase.CreateFolder("Assets/LG_Shooting/LGAssets", "Items");
        }

        ItemDataSO itemAmmo = CrearOCargarItemSO($"{itemsDir}/Item_Ammo.asset", "item_ammo", "Munición 9mm", ItemType.Municion, 50, false, 0f, 0f, new Color(0.2f, 0.8f, 0.3f));
        ItemDataSO itemBandage = CrearOCargarItemSO($"{itemsDir}/Item_Bandage.asset", "item_bandage", "Vendas Curativas", ItemType.Curacion, 5, false, 35f, 0f, new Color(0.2f, 0.9f, 0.9f));
        ItemDataSO itemResource = CrearOCargarItemSO($"{itemsDir}/Item_Resource.asset", "item_resource", "Recursos de Reparación", ItemType.Recurso, 10, false, 0f, 50f, new Color(0.9f, 0.6f, 0.2f));
        ItemDataSO itemKeyRed = CrearOCargarItemSO($"{itemsDir}/Item_Key_Red.asset", "item_key_red", "Llave Roja", ItemType.LlaveMaestra, 1, true, 0f, 0f, new Color(0.95f, 0.2f, 0.2f));
        ItemDataSO itemKeyBlue = CrearOCargarItemSO($"{itemsDir}/Item_Key_Blue.asset", "item_key_blue", "Llave Azul", ItemType.LlaveMaestra, 1, true, 0f, 0f, new Color(0.2f, 0.4f, 0.95f));

        AssetDatabase.SaveAssets();

        // 2. Cargar o crear Materiales URP
        string matDir = "Assets/LG_Shooting/LGAssets/Materials";
        Material matSuelo = AssetDatabase.LoadAssetAtPath<Material>($"{matDir}/Mat_Suelo.mat");
        Material matPared = AssetDatabase.LoadAssetAtPath<Material>($"{matDir}/Mat_Pared.mat");
        Material matPuerta = AssetDatabase.LoadAssetAtPath<Material>($"{matDir}/Mat_Puerta.mat");
        Material matMarco = AssetDatabase.LoadAssetAtPath<Material>($"{matDir}/Mat_Marco.mat");

        if (matSuelo == null) matSuelo = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (matPared == null) matPared = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (matPuerta == null) matPuerta = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (matMarco == null) matMarco = new Material(Shader.Find("Universal Render Pipeline/Lit"));

        // 3. Crear Raíz del Nivel DOOM
        GameObject levelRoot = GameObject.Find("DOOM_Level_Root");
        if (levelRoot != null) Undo.DestroyObjectImmediate(levelRoot);
        levelRoot = new GameObject("DOOM_Level_Root");

        // Luz ambiental y direccional
        GameObject lightGO = GameObject.Find("Directional Light");
        if (lightGO == null)
        {
            lightGO = new GameObject("Directional Light");
            Light l = lightGO.AddComponent<Light>();
            l.type = LightType.Directional;
            l.intensity = 0.5f;
            l.shadows = LightShadows.Soft;
            lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }
        else
        {
            Light l = lightGO.GetComponent<Light>();
            if (l != null)
            {
                l.intensity = 0.5f;
                l.shadows = LightShadows.Soft;
            }
        }

        // --- SECTOR 1: LAS AFUERAS (Z = -35 a -15) ---
        GameObject sector1 = new GameObject("Sector_1_Afueras");
        sector1.transform.SetParent(levelRoot.transform);
        CrearSuelo(sector1.transform, "Suelo_S1", new Vector3(0f, 0f, -25f), new Vector3(2.5f, 1f, 2f), matSuelo);
        CrearParedesSector(sector1.transform, "Muros_S1", new Vector3(0f, 0f, -25f), 25f, 20f, 4.5f, matPared, tieneParedSur: true, tieneParedNorteConPuerta: true);

        // Checkpoint Safe Room 1 (Inicio)
        CrearSafeRoom(sector1.transform, "SafeRoom_1", new Vector3(0f, 0.5f, -32f));

        // Coleccionables Sector 1
        CrearPickup(sector1.transform, "Pickup_Ammo_1", new Vector3(4f, 0.6f, -26f), itemAmmo, 20);
        CrearPickup(sector1.transform, "Pickup_Bandage_1", new Vector3(-4f, 0.6f, -26f), itemBandage, 2);
        CrearPickup(sector1.transform, "Pickup_Key_Red", new Vector3(6f, 0.6f, -18f), itemKeyRed, 1);

        // Enemigos Sector 1
        CrearEnemigo(sector1.transform, "Zombie_S1_A", new Vector3(-3f, 1f, -20f));
        CrearEnemigo(sector1.transform, "Zombie_S1_B", new Vector3(3f, 1f, -20f));

        // Compuerta Sector 1 a Sector 2 (Requiere Llave Roja)
        CrearDoomDoor(sector1.transform, "Puerta_Sector_1_2", new Vector3(0f, 0f, -15f), DoomDoorController.DoorLockType.RedKey, "Llave Roja", 2, matPuerta, matMarco, false);

        // --- SECTOR 2: CASCO URBANO Y ZONAS OSCURAS (Z = -15 a 10) ---
        GameObject sector2 = new GameObject("Sector_2_CascoUrbano");
        sector2.transform.SetParent(levelRoot.transform);
        CrearSuelo(sector2.transform, "Suelo_S2", new Vector3(0f, 0f, -2.5f), new Vector3(2.5f, 1f, 2.5f), matSuelo);
        // Sector 2 comparte el muro sur en z = -15f con Sector 1 (donde está Puerta_Sector_1_2), por lo que NO crea Muro_Sur sólido
        CrearParedesSector(sector2.transform, "Muros_S2", new Vector3(0f, 0f, -2.5f), 25f, 25f, 4.5f, matPared, tieneParedSur: false, tieneParedNorteConPuerta: true);

        // Checkpoint Safe Room 2
        CrearSafeRoom(sector2.transform, "SafeRoom_2", new Vector3(0f, 0.5f, -13f));

        // Coleccionables Sector 2
        CrearPickup(sector2.transform, "Pickup_Resource_1", new Vector3(-5f, 0.6f, -5f), itemResource, 3);
        CrearPickup(sector2.transform, "Pickup_Ammo_2", new Vector3(5f, 0.6f, -5f), itemAmmo, 25);
        CrearPickup(sector2.transform, "Pickup_Key_Blue", new Vector3(-6f, 0.6f, 5f), itemKeyBlue, 1);

        // Enemigos Sector 2 (Emboscada)
        CrearEnemigo(sector2.transform, "Zombie_S2_A", new Vector3(-4f, 1f, 0f));
        CrearEnemigo(sector2.transform, "Zombie_S2_B", new Vector3(4f, 1f, 0f));
        CrearEnemigo(sector2.transform, "Zombie_S2_C", new Vector3(0f, 1f, 6f));

        // Compuerta Sector 2 a Sector 3 (Requiere Llave Azul)
        CrearDoomDoor(sector2.transform, "Puerta_Sector_2_3", new Vector3(0f, 0f, 10f), DoomDoorController.DoorLockType.BlueKey, "Llave Azul", 3, matPuerta, matMarco, false);

        // --- SECTOR 3: ALTAR RITUAL & ARENA DEL JEFE (Z = 10 a 40) ---
        GameObject sector3 = new GameObject("Sector_3_ArenaJefe");
        sector3.transform.SetParent(levelRoot.transform);
        CrearSuelo(sector3.transform, "Suelo_S3", new Vector3(0f, 0f, 25f), new Vector3(3f, 1f, 3f), matSuelo);
        
        // Muros de Sector 3 (Ancho 30m, Largo 30m, Alto 6m)
        GameObject murosS3 = new GameObject("Muros_S3");
        murosS3.transform.SetParent(sector3.transform);
        
        // Pared Oeste y Pared Este
        CrearMuro(murosS3.transform, "Muro_Oeste", new Vector3(-15f, 3f, 25f), new Vector3(0.6f, 6f, 30f), matPared);
        CrearMuro(murosS3.transform, "Muro_Este", new Vector3(15f, 3f, 25f), new Vector3(0.6f, 6f, 30f), matPared);

        // Pared Norte con vano para Puerta_Escape_Final (z = 40f, alto = 6f)
        float vanoS3 = 3f;
        float segWidthS3 = (30f - vanoS3) / 2f; // 13.5m
        float segOffsetS3 = (vanoS3 / 2f) + (segWidthS3 / 2f); // 8.25m
        CrearMuro(murosS3.transform, "Muro_Norte_Izq", new Vector3(-segOffsetS3, 3f, 40f), new Vector3(segWidthS3, 6f, 0.6f), matPared);
        CrearMuro(murosS3.transform, "Muro_Norte_Der", new Vector3(segOffsetS3, 3f, 40f), new Vector3(segWidthS3, 6f, 0.6f), matPared);
        float dintelHeightS3 = 6f - 3.3f; // 2.7m
        float dintelYS3 = 3.3f + (dintelHeightS3 / 2f); // 4.65m
        CrearMuro(murosS3.transform, "Muro_Norte_Dintel", new Vector3(0f, dintelYS3, 40f), new Vector3(vanoS3, dintelHeightS3, 0.6f), matPared);

        // Pared Sur limítrofe con Sector 2 (z = 10f):
        // Conecta con el Muro Norte de Sector 2 (25m ancho x 4.5m alto) dejando despejado el paso central hacia Puerta_Sector_2_3
        CrearMuro(murosS3.transform, "Muro_Sur_AlaIzq", new Vector3(-13.75f, 3f, 10f), new Vector3(2.5f, 6f, 0.6f), matPared);
        CrearMuro(murosS3.transform, "Muro_Sur_AlaDer", new Vector3(13.75f, 3f, 10f), new Vector3(2.5f, 6f, 0.6f), matPared);
        CrearMuro(murosS3.transform, "Muro_Sur_DintelSuperior", new Vector3(0f, 5.25f, 10f), new Vector3(25f, 1.5f, 0.6f), matPared);

        // 4 Pilares de cobertura de piedra para esquivar embestidas de Fase 2
        CrearPilar(sector3.transform, "Pilar_SO", new Vector3(-7f, 3f, 18f));
        CrearPilar(sector3.transform, "Pilar_SE", new Vector3(7f, 3f, 18f));
        CrearPilar(sector3.transform, "Pilar_NO", new Vector3(-7f, 3f, 32f));
        CrearPilar(sector3.transform, "Pilar_NE", new Vector3(7f, 3f, 32f));

        // Checkpoint Safe Room 3 (Pre-jefe)
        CrearSafeRoom(sector3.transform, "SafeRoom_3", new Vector3(0f, 0.5f, 12f));
        CrearPickup(sector3.transform, "Pickup_Ammo_PreBoss", new Vector3(2f, 0.6f, 13f), itemAmmo, 30);
        CrearPickup(sector3.transform, "Pickup_Bandage_PreBoss", new Vector3(-2f, 0.6f, 13f), itemBandage, 2);

        // Puntos de Acecho en muros para Fase 1 del Jefe
        Transform[] perches = new Transform[4];
        perches[0] = CrearPuntoVantage(sector3.transform, "Perch_Oeste", new Vector3(-13.5f, 4.5f, 25f));
        perches[1] = CrearPuntoVantage(sector3.transform, "Perch_Este", new Vector3(13.5f, 4.5f, 25f));
        perches[2] = CrearPuntoVantage(sector3.transform, "Perch_Norte_Izq", new Vector3(-6f, 4.5f, 38.5f));
        perches[3] = CrearPuntoVantage(sector3.transform, "Perch_Norte_Der", new Vector3(6f, 4.5f, 38.5f));

        // Jefe Final: El Bebé Maldito
        CrearJefeFinal(sector3.transform, new Vector3(0f, 1f, 28f), perches);

        // Compuerta de Escape Final (Se desbloquea al derrotar al jefe) centrada en z = 40.0f
        CrearDoomDoor(sector3.transform, "Puerta_Escape_Final", new Vector3(0f, 0f, 40.0f), DoomDoorController.DoorLockType.BossDefeated, "Derrotar Jefe", 2, matPuerta, matMarco, true);

        // 4. Configurar Jugador
        ConfigurarJugador(new Vector3(0f, 1f, -32f));

        // 5. Configurar Canvas y HUD Completo
        ConfigurarHUDCompleto();

        // 6. Configurar DoomLevelManager
        GameObject lmGO = GameObject.Find("DoomLevelManager");
        if (lmGO == null) lmGO = new GameObject("DoomLevelManager");
        if (lmGO.GetComponent<DoomLevelManager>() == null) lmGO.AddComponent<DoomLevelManager>();

        // 7. Configurar Menú de Pausa y Ajustes (TimeManager + Canvas.prefab)
        PlayerSetupUtility.ConfigurarMenuPausaYTimeManager();

        if (enNuevaEscena)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), DOOM_SCENE_PATH);
        }

        EditorUtility.DisplayDialog("Secuencia DOOM Generada",
            "¡Nivel completo estilo DOOM generado con éxito!\n\n" +
            "• Sector 1 (Las Afueras): Safe Room 1, Munición directa, Vendas, Zombies y Llave Roja.\n" +
            "• Sector 2 (Casco Urbano): Zonas oscuras, Linterna [2 / F], Recursos, Zombies y Llave Azul.\n" +
            "• Sector 3 (Altar Ritual): Pilares de cobertura, Jefe Final 'El Bebé Maldito' (2 Fases) y Compuerta de Escape.\n" +
            "• HUD Táctico con Barra del Jefe y Pantalla de Intermisión estilo retro.\n\n" +
            (enNuevaEscena ? $"Guardado en escena segura: {DOOM_SCENE_PATH}" : "Generado en la escena activa actual."),
            "Entendido");
    }

    private static void RegistrarEscenaEnBuildSettings(string scenePath)
    {
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        foreach (var s in scenes)
        {
            if (s.path == scenePath) return;
        }

        EditorBuildSettingsScene[] newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
        System.Array.Copy(scenes, newScenes, scenes.Length);
        newScenes[newScenes.Length - 1] = new EditorBuildSettingsScene(scenePath, true);
        EditorBuildSettings.scenes = newScenes;
    }

    private static ItemDataSO CrearOCargarItemSO(string path, string id, string name, ItemType type, int stack, bool perm, float heal, float rep, Color theme)
    {
        ItemDataSO so = AssetDatabase.LoadAssetAtPath<ItemDataSO>(path);
        if (so == null)
        {
            so = ScriptableObject.CreateInstance<ItemDataSO>();
            so.itemID = id;
            so.itemName = name;
            so.itemType = type;
            so.maxStack = stack;
            so.isPermanent = perm;
            so.healAmount = heal;
            so.repairAmount = rep;
            so.themeColor = theme;

            AssetDatabase.CreateAsset(so, path);
        }
        return so;
    }

    private static void CrearSuelo(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
    {
        GameObject suelo = GameObject.CreatePrimitive(PrimitiveType.Plane);
        suelo.name = name;
        suelo.transform.SetParent(parent);
        suelo.transform.position = pos;
        suelo.transform.localScale = scale;
        if (mat != null) suelo.GetComponent<Renderer>().sharedMaterial = mat;
    }

    private static void CrearParedesSector(Transform parent, string groupName, Vector3 center, float widthX, float lengthZ, float height, Material mat, bool tieneParedSur = true, bool tieneParedNorteConPuerta = true)
    {
        GameObject group = new GameObject(groupName);
        group.transform.SetParent(parent);

        float halfX = widthX / 2f;
        float halfZ = lengthZ / 2f;
        float y = height / 2f;
        float thickness = 0.6f;

        // Pared Sur (omitida si el sector comparte frontera previa donde ya hay un muro/puerta)
        if (tieneParedSur)
        {
            CrearMuro(group.transform, "Muro_Sur", center + new Vector3(0f, y, -halfZ), new Vector3(widthX, height, thickness), mat);
        }

        // Pared Oeste
        CrearMuro(group.transform, "Muro_Oeste", center + new Vector3(-halfX, y, 0f), new Vector3(thickness, height, lengthZ), mat);
        // Pared Este
        CrearMuro(group.transform, "Muro_Este", center + new Vector3(halfX, y, 0f), new Vector3(thickness, height, lengthZ), mat);

        // Pared Norte con vano para compuerta en el centro
        if (tieneParedNorteConPuerta)
        {
            float vano = 3f;
            float segWidth = (widthX - vano) / 2f;
            float segOffset = (vano / 2f) + (segWidth / 2f);

            CrearMuro(group.transform, "Muro_Norte_Izq", center + new Vector3(-segOffset, y, halfZ), new Vector3(segWidth, height, thickness), mat);
            CrearMuro(group.transform, "Muro_Norte_Der", center + new Vector3(segOffset, y, halfZ), new Vector3(segWidth, height, thickness), mat);

            // Altura libre de 3.3m para albergar compuerta (3.2m de marco + 0.1m de margen superior)
            float dintelHeight = height - 3.3f;
            float dintelY = 3.3f + (dintelHeight / 2f);
            CrearMuro(group.transform, "Muro_Norte_Dintel", center + new Vector3(0f, dintelY, halfZ), new Vector3(vano, dintelHeight, thickness), mat);
        }
        else
        {
            CrearMuro(group.transform, "Muro_Norte", center + new Vector3(0f, y, halfZ), new Vector3(widthX, height, thickness), mat);
        }
    }

    private static void CrearMuro(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
    {
        GameObject muro = GameObject.CreatePrimitive(PrimitiveType.Cube);
        muro.name = name;
        muro.transform.SetParent(parent);
        muro.transform.position = pos;
        muro.transform.localScale = scale;
        if (mat != null) muro.GetComponent<Renderer>().sharedMaterial = mat;
    }

    private static void CrearPilar(Transform parent, string name, Vector3 pos)
    {
        GameObject pilar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pilar.name = name;
        pilar.transform.SetParent(parent);
        pilar.transform.position = pos;
        pilar.transform.localScale = new Vector3(2f, 3f, 2f);
    }

    private static void CrearSafeRoom(Transform parent, string name, Vector3 pos)
    {
        GameObject sr = new GameObject(name);
        sr.transform.SetParent(parent);
        sr.transform.position = pos;

        BoxCollider col = sr.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(6f, 3f, 6f);

        GameObject lightGO = new GameObject("SafeLight");
        lightGO.transform.SetParent(sr.transform);
        lightGO.transform.localPosition = new Vector3(0f, 2.5f, 0f);
        Light l = lightGO.AddComponent<Light>();
        l.type = LightType.Point;
        l.range = 8f;
        l.color = new Color(0.2f, 0.9f, 0.4f);

        SafeRoomCheckpoint cp = sr.AddComponent<SafeRoomCheckpoint>();
        SerializedObject cpSO = new SerializedObject(cp);
        SetPropertyRef(cpSO, "respawnPoint", sr.transform);
        SetPropertyRef(cpSO, "safeLight", l);
        cpSO.ApplyModifiedProperties();
    }

    private static Transform CrearPuntoVantage(Transform parent, string name, Vector3 pos)
    {
        GameObject vantage = new GameObject(name);
        vantage.transform.SetParent(parent);
        vantage.transform.position = pos;
        return vantage.transform;
    }

    private static void CrearPickup(Transform parent, string name, Vector3 pos, ItemDataSO data, int qty)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent);
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * 0.45f;
        go.transform.rotation = Quaternion.Euler(45f, 45f, 45f);

        var col = go.GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        if (data != null)
        {
            Renderer r = go.GetComponent<Renderer>();
            if (r != null) r.material.color = data.themeColor;
        }

        PickupInteractable pi = go.AddComponent<PickupInteractable>();
        pi.Initialize(data, qty, true);
    }

    private static void CrearEnemigo(Transform parent, string name, Vector3 pos)
    {
        GameObject z = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        z.name = name;
        z.transform.SetParent(parent);
        z.transform.position = pos;

        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head_Hitbox";
        head.transform.SetParent(z.transform);
        head.transform.localPosition = new Vector3(0f, 0.9f, 0.1f);
        head.transform.localScale = Vector3.one * 0.6f;

        Rigidbody rb = z.AddComponent<Rigidbody>();
        rb.mass = 1.5f;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        LG_Enemy enemy = z.AddComponent<LG_Enemy>();
        SerializedObject so = new SerializedObject(enemy);
        SerializedProperty hpProp = so.FindProperty("maxHealth");
        if (hpProp != null) hpProp.floatValue = 3f;
        SerializedProperty spdProp = so.FindProperty("speed");
        if (spdProp != null) spdProp.floatValue = 2.6f;
        SerializedProperty chaseProp = so.FindProperty("chaseRange");
        if (chaseProp != null) chaseProp.floatValue = 18f;
        so.ApplyModifiedProperties();
    }

    private static void CrearDoomDoor(Transform parent, string name, Vector3 pos, DoomDoorController.DoorLockType lockType, string keyName, int nextIdx, Material matDoor, Material matFrame, bool isExit)
    {
        GameObject doorRoot = new GameObject(name);
        doorRoot.transform.SetParent(parent);
        doorRoot.transform.position = pos;

        // Marcos laterales y travesaño superior ajustados a vano exacto de 3.0m
        GameObject frameIzq = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frameIzq.name = "Frame_Izq";
        frameIzq.transform.SetParent(doorRoot.transform);
        frameIzq.transform.localPosition = new Vector3(-1.35f, 1.6f, 0f);
        frameIzq.transform.localScale = new Vector3(0.3f, 3.2f, 0.5f);
        if (matFrame != null) frameIzq.GetComponent<Renderer>().sharedMaterial = matFrame;

        GameObject frameDer = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frameDer.name = "Frame_Der";
        frameDer.transform.SetParent(doorRoot.transform);
        frameDer.transform.localPosition = new Vector3(1.35f, 1.6f, 0f);
        frameDer.transform.localScale = new Vector3(0.3f, 3.2f, 0.5f);
        if (matFrame != null) frameDer.GetComponent<Renderer>().sharedMaterial = matFrame;

        GameObject frameSup = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frameSup.name = "Frame_Sup";
        frameSup.transform.SetParent(doorRoot.transform);
        frameSup.transform.localPosition = new Vector3(0f, 3.2f, 0f);
        frameSup.transform.localScale = new Vector3(3.0f, 0.2f, 0.5f);
        if (matFrame != null) frameSup.GetComponent<Renderer>().sharedMaterial = matFrame;

        // Luz indicadora de estado montada en el travesaño frontal
        GameObject statusLight = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        statusLight.name = "Status_Light";
        statusLight.transform.SetParent(doorRoot.transform);
        statusLight.transform.localPosition = new Vector3(0f, 3.2f, -0.26f);
        statusLight.transform.localScale = Vector3.one * 0.35f;

        GameObject hinge = new GameObject("Door_Hinge");
        hinge.transform.SetParent(doorRoot.transform);
        hinge.transform.localPosition = new Vector3(-1.2f, 0f, 0f);

        GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        panel.name = "Door_Panel";
        panel.transform.SetParent(hinge.transform);
        panel.transform.localPosition = new Vector3(1.2f, 1.6f, 0f);
        panel.transform.localScale = new Vector3(2.4f, 3.1f, 0.2f);
        if (matDoor != null) panel.GetComponent<Renderer>().sharedMaterial = matDoor;

        BoxCollider triggerCol = doorRoot.AddComponent<BoxCollider>();
        triggerCol.isTrigger = true;
        triggerCol.size = new Vector3(3.5f, 3.5f, 3.5f);
        triggerCol.center = new Vector3(0f, 1.6f, 0f);

        DoomDoorController dc = doorRoot.AddComponent<DoomDoorController>();
        SerializedObject dcSO = new SerializedObject(dc);
        SetPropertyRef(dcSO, "doorHinge", hinge.transform);
        SetPropertyRef(dcSO, "statusLightRenderer", statusLight.GetComponent<Renderer>());
        SerializedProperty lockProp = dcSO.FindProperty("lockType");
        if (lockProp != null) lockProp.enumValueIndex = (int)lockType;
        SerializedProperty keyProp = dcSO.FindProperty("customRequiredKey");
        if (keyProp != null) keyProp.stringValue = keyName;
        SerializedProperty nextProp = dcSO.FindProperty("nextLevelIndex");
        if (nextProp != null) nextProp.intValue = nextIdx;
        SerializedProperty exitProp = dcSO.FindProperty("isLevelExitDoor");
        if (exitProp != null) exitProp.boolValue = isExit;
        dcSO.ApplyModifiedProperties();
    }

    private static void CrearJefeFinal(Transform parent, Vector3 pos, Transform[] perches)
    {
        GameObject bossGO = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        bossGO.name = "Jefe_BebeMaldito";
        bossGO.transform.SetParent(parent);
        bossGO.transform.position = pos;
        bossGO.transform.localScale = new Vector3(1.8f, 2.2f, 1.8f);
        bossGO.GetComponent<Renderer>().material.color = new Color(0.7f, 0.1f, 0.8f);

        GameObject bossHead = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bossHead.name = "Head_Hitbox_Boss";
        bossHead.transform.SetParent(bossGO.transform);
        bossHead.transform.localPosition = new Vector3(0f, 0.95f, 0.2f);
        bossHead.transform.localScale = Vector3.one * 0.7f;
        bossHead.GetComponent<Renderer>().material.color = new Color(0.9f, 0.2f, 0.2f);

        CharacterController cc = bossGO.AddComponent<CharacterController>();
        cc.height = 2.2f;
        cc.radius = 0.9f;

        BossBabyController bossComp = bossGO.AddComponent<BossBabyController>();
        SerializedObject bSO = new SerializedObject(bossComp);
        SerializedProperty hpProp = bSO.FindProperty("maxHealth");
        if (hpProp != null) hpProp.floatValue = 35f;
        SetPropertyRef(bSO, "headTransform", bossHead.transform);

        SerializedProperty perchProp = bSO.FindProperty("wallPerchPoints");
        if (perchProp != null)
        {
            perchProp.arraySize = perches.Length;
            for (int i = 0; i < perches.Length; i++)
            {
                perchProp.GetArrayElementAtIndex(i).objectReferenceValue = perches[i];
            }
        }
        bSO.ApplyModifiedProperties();
    }

    private static void ConfigurarJugador(Vector3 spawnPos)
    {
        LG_ObjectPool pool = PlayerSetupUtility.ConfigurarBulletPool();
        PlayerSetupUtility.ConfigurarJugadorCompleto(spawnPos, pool);
    }

    private static void ConfigurarHUDCompleto()
    {
        GameObject playerObj = GameObject.Find("Player (RI + LG)");
        LG_ObjectPool pool = GameObject.Find("BulletObjectPool")?.GetComponent<LG_ObjectPool>();
        if (pool == null) pool = PlayerSetupUtility.ConfigurarBulletPool();

        GameObject canvasGO = PlayerSetupUtility.ConfigurarHUDCompleto(playerObj, pool);

        // Barra de Vida del Jefe DOOM
        Transform bossUI = canvasGO.transform.Find("BossHealthBarContainer");
        if (bossUI == null)
        {
            GameObject bossUIGO = new GameObject("BossHealthBarContainer");
            bossUIGO.transform.SetParent(canvasGO.transform, false);
            RectTransform r = bossUIGO.AddComponent<RectTransform>();
            r.anchorMin = new Vector2(0.5f, 1f);
            r.anchorMax = new Vector2(0.5f, 1f);
            r.pivot = new Vector2(0.5f, 1f);
            r.anchoredPosition = new Vector2(0f, -25f);
            r.sizeDelta = new Vector2(500f, 40f);

            GameObject textGO = new GameObject("BossName");
            textGO.transform.SetParent(bossUIGO.transform, false);
            TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = "EL BEBÉ MALDITO";
            tmp.fontSize = 20;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.95f, 0.2f, 0.2f);
            textGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 20f);

            GameObject sliderGO = new GameObject("BossSlider");
            sliderGO.transform.SetParent(bossUIGO.transform, false);
            sliderGO.AddComponent<Slider>();
            sliderGO.GetComponent<RectTransform>().sizeDelta = new Vector2(480f, 18f);

            bossUIGO.SetActive(false);
        }

        // UI de Intermisión Estilo DOOM
        Transform intermission = canvasGO.transform.Find("DoomIntermissionPanel");
        if (intermission == null)
        {
            GameObject interGO = new GameObject("DoomIntermissionPanel");
            interGO.transform.SetParent(canvasGO.transform, false);
            RectTransform r = interGO.AddComponent<RectTransform>();
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.sizeDelta = Vector2.zero;

            Image bg = interGO.AddComponent<Image>();
            bg.color = new Color(0.04f, 0.06f, 0.1f, 0.95f);

            GameObject title = new GameObject("TitleText");
            title.transform.SetParent(interGO.transform, false);
            TextMeshProUGUI tTmp = title.AddComponent<TextMeshProUGUI>();
            tTmp.text = "<b>SECTOR DESPEJADO</b>";
            tTmp.fontSize = 42;
            tTmp.alignment = TextAlignmentOptions.Center;
            tTmp.color = new Color(0.96f, 0.6f, 0.1f);
            title.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 160f);

            GameObject kills = new GameObject("KillsText");
            kills.transform.SetParent(interGO.transform, false);
            TextMeshProUGUI kTmp = kills.AddComponent<TextMeshProUGUI>();
            kTmp.fontSize = 26;
            kTmp.alignment = TextAlignmentOptions.Center;
            kills.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 60f);

            GameObject items = new GameObject("ItemsText");
            items.transform.SetParent(interGO.transform, false);
            TextMeshProUGUI iTmp = items.AddComponent<TextMeshProUGUI>();
            iTmp.fontSize = 26;
            iTmp.alignment = TextAlignmentOptions.Center;
            items.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);

            GameObject time = new GameObject("TimeText");
            time.transform.SetParent(interGO.transform, false);
            TextMeshProUGUI tmTmp = time.AddComponent<TextMeshProUGUI>();
            tmTmp.fontSize = 26;
            tmTmp.alignment = TextAlignmentOptions.Center;
            time.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -60f);

            GameObject btn = new GameObject("ContinueButton");
            btn.transform.SetParent(interGO.transform, false);
            Button b = btn.AddComponent<Button>();
            Image btnImg = btn.AddComponent<Image>();
            btnImg.color = new Color(0.85f, 0.2f, 0.2f);
            btn.GetComponent<RectTransform>().sizeDelta = new Vector2(300f, 50f);
            btn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -150f);

            GameObject btnText = new GameObject("BtnText");
            btnText.transform.SetParent(btn.transform, false);
            TextMeshProUGUI bTmp = btnText.AddComponent<TextMeshProUGUI>();
            bTmp.text = "CONTINUAR [ESPACIO]";
            bTmp.fontSize = 20;
            bTmp.alignment = TextAlignmentOptions.Center;

            DoomIntermissionUI dUI = interGO.AddComponent<DoomIntermissionUI>();
            SerializedObject dSO = new SerializedObject(dUI);
            SetPropertyRef(dSO, "intermissionPanel", interGO);
            SetPropertyRef(dSO, "titleText", tTmp);
            SetPropertyRef(dSO, "killsText", kTmp);
            SetPropertyRef(dSO, "itemsText", iTmp);
            SetPropertyRef(dSO, "timeText", tmTmp);
            SetPropertyRef(dSO, "continueButton", b);
            SetPropertyRef(dSO, "continueButtonText", bTmp);
            dSO.ApplyModifiedProperties();

            interGO.SetActive(false);
        }
    }

    private static void SetPropertyRef(SerializedObject so, string propName, Object value)
    {
        SerializedProperty prop = so.FindProperty(propName);
        if (prop != null)
        {
            prop.objectReferenceValue = value;
        }
    }
}
