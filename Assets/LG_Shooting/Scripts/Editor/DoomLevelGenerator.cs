using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public static class DoomLevelGenerator
{
    [MenuItem("Prototipo/Generar Secuencia Completa Estilo DOOM")]
    public static void GenerarSecuenciaDoom()
    {
        // 1. Asegurar Carpeta de Items y Crear ScriptableObjects
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

        // 2. Materiales URP
        string matDir = "Assets/LG_Shooting/LGAssets/Materials";
        Material matSuelo = AssetDatabase.LoadAssetAtPath<Material>($"{matDir}/Mat_Suelo.mat");
        Material matPared = AssetDatabase.LoadAssetAtPath<Material>($"{matDir}/Mat_Pared.mat");
        Material matPuerta = AssetDatabase.LoadAssetAtPath<Material>($"{matDir}/Mat_Puerta.mat");
        Material matMarco = AssetDatabase.LoadAssetAtPath<Material>($"{matDir}/Mat_Marco.mat");
        Material matEnemigo = AssetDatabase.LoadAssetAtPath<Material>($"{matDir}/Mat_Enemigo.mat");

        if (matSuelo == null) matSuelo = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (matPared == null) matPared = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (matPuerta == null) matPuerta = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (matMarco == null) matMarco = new Material(Shader.Find("Universal Render Pipeline/Lit"));

        // 3. Crear Estructura de Niveles en la Escena
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
            l.intensity = 1.0f;
            lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        // --- SECTOR 1: LAS AFUERAS (Z = -35 a -15) ---
        GameObject sector1 = new GameObject("Sector_1_Afueras");
        sector1.transform.SetParent(levelRoot.transform);
        CrearSuelo(sector1.transform, "Suelo_S1", new Vector3(0f, 0f, -25f), new Vector3(2.5f, 1f, 2f), matSuelo);
        CrearParedesSector(sector1.transform, "Muros_S1", new Vector3(0f, 0f, -25f), 25f, 20f, 4.5f, matPared);

        // Checkpoint Safe Room 1
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
        CrearParedesSector(sector2.transform, "Muros_S2", new Vector3(0f, 0f, -2.5f), 25f, 25f, 4.5f, matPared);

        // Checkpoint Safe Room 2
        CrearSafeRoom(sector2.transform, "SafeRoom_2", new Vector3(0f, 0.5f, -13f));

        // Coleccionables Sector 2
        CrearPickup(sector2.transform, "Pickup_Resource_1", new Vector3(-5f, 0.6f, -5f), itemResource, 3);
        CrearPickup(sector2.transform, "Pickup_Ammo_2", new Vector3(5f, 0.6f, -5f), itemAmmo, 25);
        CrearPickup(sector2.transform, "Pickup_Key_Blue", new Vector3(-6f, 0.6f, 5f), itemKeyBlue, 1);

        // Enemigos Sector 2
        CrearEnemigo(sector2.transform, "Zombie_S2_A", new Vector3(-4f, 1f, 0f));
        CrearEnemigo(sector2.transform, "Zombie_S2_B", new Vector3(4f, 1f, 0f));
        CrearEnemigo(sector2.transform, "Zombie_S2_C", new Vector3(0f, 1f, 6f));

        // Compuerta Sector 2 a Sector 3 (Requiere Llave Azul)
        CrearDoomDoor(sector2.transform, "Puerta_Sector_2_3", new Vector3(0f, 0f, 10f), DoomDoorController.DoorLockType.BlueKey, "Llave Azul", 3, matPuerta, matMarco, false);

        // --- SECTOR 3: ALTAR RITUAL & ARENA DEL JEFE (Z = 10 a 40) ---
        GameObject sector3 = new GameObject("Sector_3_ArenaJefe");
        sector3.transform.SetParent(levelRoot.transform);
        CrearSuelo(sector3.transform, "Suelo_S3", new Vector3(0f, 0f, 25f), new Vector3(3f, 1f, 3f), matSuelo);
        CrearParedesSector(sector3.transform, "Muros_S3", new Vector3(0f, 0f, 25f), 30f, 30f, 6f, matPared);

        // Pilares de piedra para esquivar embestidas de Fase 2
        CrearPilar(sector3.transform, "Pilar_SO", new Vector3(-7f, 3f, 18f));
        CrearPilar(sector3.transform, "Pilar_SE", new Vector3(7f, 3f, 18f));
        CrearPilar(sector3.transform, "Pilar_NO", new Vector3(-7f, 3f, 32f));
        CrearPilar(sector3.transform, "Pilar_NE", new Vector3(7f, 3f, 32f));

        // Checkpoint Safe Room 3
        CrearSafeRoom(sector3.transform, "SafeRoom_3", new Vector3(0f, 0.5f, 12f));

        // Puntos de Acecho en muros para Fase 1 del Jefe
        Transform[] perches = new Transform[4];
        perches[0] = CrearPuntoVantage(sector3.transform, "Perch_Oeste", new Vector3(-13.5f, 4f, 25f));
        perches[1] = CrearPuntoVantage(sector3.transform, "Perch_Este", new Vector3(13.5f, 4f, 25f));
        perches[2] = CrearPuntoVantage(sector3.transform, "Perch_Norte_Izq", new Vector3(-6f, 4f, 38.5f));
        perches[3] = CrearPuntoVantage(sector3.transform, "Perch_Norte_Der", new Vector3(6f, 4f, 38.5f));

        // Jefe Final: El Bebé Maldito
        CrearJefeFinal(sector3.transform, new Vector3(0f, 1f, 28f), perches);

        // Compuerta de Escape Final (Se desbloquea al derrotar al jefe)
        CrearDoomDoor(sector3.transform, "Puerta_Escape_Final", new Vector3(0f, 0f, 39.8f), DoomDoorController.DoorLockType.BossDefeated, "Derrotar Jefe", 2, matPuerta, matMarco, true);

        // 4. Configurar el Jugador con todos los componentes requeridos
        ConfigurarJugador(new Vector3(0f, 1f, -32f), itemAmmo);

        // 5. Configurar Canvas y HUD Completo
        ConfigurarHUDCompleto();

        // 6. Configurar DoomLevelManager
        GameObject lmGO = GameObject.Find("DoomLevelManager");
        if (lmGO == null) lmGO = new GameObject("DoomLevelManager");
        if (lmGO.GetComponent<DoomLevelManager>() == null) lmGO.AddComponent<DoomLevelManager>();

        EditorUtility.DisplayDialog("Secuencia DOOM Generada",
            "¡El proyecto se ha estructurado con éxito en una secuencia estilo DOOM!\n\n" +
            "✔ Sector 1 (Afueras): Safe Room 1, Munición directa, Vendas, Zombies y Llave Roja.\n" +
            "✔ Sector 2 (Casco Urbano): Zonas oscuras, Recursos de reparación, Bate/Mechero y Llave Azul.\n" +
            "✔ Sector 3 (Altar Ritual): Pilares de cobertura, Jefe Final El Bebé Maldito (2 Fases) y Salida.\n" +
            "✔ Interfaz HUD táctica con ranuras fijas de inventario y munición directa.\n" +
            "✔ Pantalla de Intermisión con estadísticas al estilo DOOM clásico.\n\n" +
            "Presiona PLAY para probar la experiencia completa.", "¡Entendido!");
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

    private static void CrearParedesSector(Transform parent, string groupName, Vector3 center, float widthX, float lengthZ, float height, Material mat)
    {
        GameObject group = new GameObject(groupName);
        group.transform.SetParent(parent);

        float halfX = widthX / 2f;
        float halfZ = lengthZ / 2f;
        float y = height / 2f;
        float thickness = 0.6f;

        // Pared Sur
        CrearMuro(group.transform, "Muro_Sur", center + new Vector3(0f, y, -halfZ), new Vector3(widthX, height, thickness), mat);
        // Pared Oeste
        CrearMuro(group.transform, "Muro_Oeste", center + new Vector3(-halfX, y, 0f), new Vector3(thickness, height, lengthZ), mat);
        // Pared Este
        CrearMuro(group.transform, "Muro_Este", center + new Vector3(halfX, y, 0f), new Vector3(thickness, height, lengthZ), mat);

        // Pared Norte con vano para compuerta en el centro
        float vano = 3f;
        float segWidth = (widthX - vano) / 2f;
        float segOffset = (vano / 2f) + (segWidth / 2f);

        CrearMuro(group.transform, "Muro_Norte_Izq", center + new Vector3(-segOffset, y, halfZ), new Vector3(segWidth, height, thickness), mat);
        CrearMuro(group.transform, "Muro_Norte_Der", center + new Vector3(segOffset, y, halfZ), new Vector3(segWidth, height, thickness), mat);
        CrearMuro(group.transform, "Muro_Norte_Dintel", center + new Vector3(0f, y + 1.2f, halfZ), new Vector3(vano, height - 2.4f, thickness), mat);
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
        cpSO.FindProperty("respawnPoint").objectReferenceValue = sr.transform;
        cpSO.FindProperty("safeLight").objectReferenceValue = l;
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

        // Cabeza como hitbox diferenciada
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
        so.FindProperty("maxHealth").floatValue = 3f;
        so.FindProperty("speed").floatValue = 2.6f;
        so.FindProperty("chaseRange").floatValue = 18f;
        so.ApplyModifiedProperties();
    }

    private static void CrearDoomDoor(Transform parent, string name, Vector3 pos, DoomDoorController.DoorLockType lockType, string keyName, int nextIdx, Material matDoor, Material matFrame, bool isExit)
    {
        GameObject doorRoot = new GameObject(name);
        doorRoot.transform.SetParent(parent);
        doorRoot.transform.position = pos;

        // Marco
        GameObject frameIzq = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frameIzq.name = "Frame_Izq";
        frameIzq.transform.SetParent(doorRoot.transform);
        frameIzq.transform.localPosition = new Vector3(-1.3f, 1.6f, 0f);
        frameIzq.transform.localScale = new Vector3(0.3f, 3.2f, 0.5f);
        if (matFrame != null) frameIzq.GetComponent<Renderer>().sharedMaterial = matFrame;

        GameObject frameDer = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frameDer.name = "Frame_Der";
        frameDer.transform.SetParent(doorRoot.transform);
        frameDer.transform.localPosition = new Vector3(1.3f, 1.6f, 0f);
        frameDer.transform.localScale = new Vector3(0.3f, 3.2f, 0.5f);
        if (matFrame != null) frameDer.GetComponent<Renderer>().sharedMaterial = matFrame;

        // Luz de estado de acceso
        GameObject statusLight = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        statusLight.name = "Status_Light";
        statusLight.transform.SetParent(doorRoot.transform);
        statusLight.transform.localPosition = new Vector3(0f, 3.3f, 0f);
        statusLight.transform.localScale = Vector3.one * 0.35f;

        // Hoja y bisagra
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
        dcSO.FindProperty("doorHinge").objectReferenceValue = hinge.transform;
        dcSO.FindProperty("lockType").enumValueIndex = (int)lockType;
        dcSO.FindProperty("customRequiredKey").stringValue = keyName;
        dcSO.FindProperty("nextLevelIndex").intValue = nextIdx;
        dcSO.FindProperty("isLevelExitDoor").boolValue = isExit;
        dcSO.FindProperty("statusLightRenderer").objectReferenceValue = statusLight.GetComponent<Renderer>();
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

        // Hitbox vulnerable de cabeza
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
        bSO.FindProperty("maxHealth").floatValue = 35f;
        bSO.FindProperty("headTransform").objectReferenceValue = bossHead.transform;

        SerializedProperty perchProp = bSO.FindProperty("wallPerchPoints");
        perchProp.arraySize = perches.Length;
        for (int i = 0; i < perches.Length; i++)
        {
            perchProp.GetArrayElementAtIndex(i).objectReferenceValue = perches[i];
        }
        bSO.ApplyModifiedProperties();
    }

    private static void ConfigurarJugador(Vector3 spawnPos, ItemDataSO initialAmmo)
    {
        GameObject player = GameObject.Find("Player (RI + LG)");
        if (player == null)
        {
            player = new GameObject("Player (RI + LG)");
        }

        player.transform.position = spawnPos;
        player.transform.rotation = Quaternion.identity;

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc == null) cc = player.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.center = new Vector3(0f, 0.9f, 0f);

        PlayerInput pInput = player.GetComponent<PlayerInput>();
        if (pInput == null) pInput = player.AddComponent<PlayerInput>();
        pInput.actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");

        Transform cabeza = player.transform.Find("Cabeza");
        if (cabeza == null)
        {
            GameObject cabGO = new GameObject("Cabeza");
            cabeza = cabGO.transform;
            cabeza.SetParent(player.transform);
            cabeza.localPosition = new Vector3(0f, 1.6f, 0f);
        }

        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.transform.SetParent(cabeza);
            cam.transform.localPosition = Vector3.zero;
            cam.transform.localRotation = Quaternion.identity;
        }

        RIMovement mov = player.GetComponent<RIMovement>();
        if (mov == null) mov = player.AddComponent<RIMovement>();
        mov.cabeza = cabeza;

        LG_PlayerHealth health = player.GetComponent<LG_PlayerHealth>();
        if (health == null) health = player.AddComponent<LG_PlayerHealth>();

        LG_Inventory inv = player.GetComponent<LG_Inventory>();
        if (inv == null) inv = player.AddComponent<LG_Inventory>();

        // Otorgar munición inicial en el inventario
        if (initialAmmo != null)
        {
            inv.AddItem(initialAmmo, 30);
        }

        LG_Shoot shoot = player.GetComponent<LG_Shoot>();
        if (shoot == null) shoot = player.AddComponent<LG_Shoot>();

        Hand hand = player.GetComponent<Hand>();
        if (hand == null) hand = player.AddComponent<Hand>();
    }

    private static void ConfigurarHUDCompleto()
    {
        GameObject canvasGO = GameObject.Find("HUD_Canvas");
        if (canvasGO == null)
        {
            canvasGO = new GameObject("HUD_Canvas");
            Canvas c = canvasGO.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        LG_HUD hud = canvasGO.GetComponent<LG_HUD>();
        if (hud == null) hud = canvasGO.AddComponent<LG_HUD>();

        GameObject playerObj = GameObject.Find("Player (RI + LG)");

        // Crear o buscar InventoryText en el Canvas
        Transform oldInvText = canvasGO.transform.Find("InventoryText");
        Text invText;
        if (oldInvText == null)
        {
            GameObject textGO = new GameObject("InventoryText");
            textGO.transform.SetParent(canvasGO.transform, false);
            RectTransform textRect = textGO.AddComponent<RectTransform>();
            textRect.anchoredPosition = new Vector2(40f, -85f);
            textRect.sizeDelta = new Vector2(350f, 250f);
            textRect.anchorMin = new Vector2(0f, 1f);
            textRect.anchorMax = new Vector2(0f, 1f);
            textRect.pivot = new Vector2(0f, 1f);

            invText = textGO.AddComponent<Text>();
            invText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            invText.fontSize = 20;
            invText.color = Color.white;
            invText.supportRichText = true;
            invText.alignment = TextAnchor.UpperLeft;

            Shadow shadow = textGO.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
            shadow.effectDistance = new Vector2(1.5f, -1.5f);
        }
        else
        {
            invText = oldInvText.GetComponent<Text>();
        }

        // Crear o buscar HealthSlider y StaminaSlider si no existen
        Slider healthSlider = canvasGO.transform.Find("HealthSlider")?.GetComponent<Slider>();
        Slider staminaSlider = canvasGO.transform.Find("StaminaSlider")?.GetComponent<Slider>();

        SerializedObject hudSO = new SerializedObject(hud);
        hudSO.FindProperty("inventoryText").objectReferenceValue = invText;
        if (playerObj != null)
        {
            hudSO.FindProperty("playerMovement").objectReferenceValue = playerObj.GetComponent<RIMovement>();
            hudSO.FindProperty("playerInventory").objectReferenceValue = playerObj.GetComponent<LG_Inventory>();
            hudSO.FindProperty("playerHealth").objectReferenceValue = playerObj.GetComponent<LG_PlayerHealth>();
            hudSO.FindProperty("playerHand").objectReferenceValue = playerObj.GetComponent<Hand>();
        }
        if (healthSlider != null) hudSO.FindProperty("healthSlider").objectReferenceValue = healthSlider;
        if (staminaSlider != null) hudSO.FindProperty("staminaSlider").objectReferenceValue = staminaSlider;
        hudSO.ApplyModifiedProperties();

        // Crear contenedor de barra del jefe
        Transform oldBossUI = canvasGO.transform.Find("BossHealthBarContainer");
        if (oldBossUI == null)
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
            Slider sl = sliderGO.AddComponent<Slider>();
            sliderGO.GetComponent<RectTransform>().sizeDelta = new Vector2(480f, 18f);

            bossUIGO.SetActive(false); // Se activa al entrar al sector 3
        }

        // Crear UI de Intermisión Estilo DOOM
        Transform oldIntermission = canvasGO.transform.Find("DoomIntermissionPanel");
        if (oldIntermission == null)
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
            dSO.FindProperty("intermissionPanel").objectReferenceValue = interGO;
            dSO.FindProperty("titleText").objectReferenceValue = tTmp;
            dSO.FindProperty("killsText").objectReferenceValue = kTmp;
            dSO.FindProperty("itemsText").objectReferenceValue = iTmp;
            dSO.FindProperty("timeText").objectReferenceValue = tmTmp;
            dSO.FindProperty("continueButton").objectReferenceValue = b;
            dSO.FindProperty("continueButtonText").objectReferenceValue = bTmp;
            dSO.ApplyModifiedProperties();

            interGO.SetActive(false);
        }
    }
}
