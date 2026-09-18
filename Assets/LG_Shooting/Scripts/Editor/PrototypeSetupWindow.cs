using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Audio;

public class PrototypeSetupWindow : EditorWindow
{
    [MenuItem("Prototipo/Configurar Escena Jugable")]
    public static void ShowWindow()
    {
        GetWindow<PrototypeSetupWindow>("Configurador de Prototipo");
    }

    private void OnGUI()
    {
        GUILayout.Label("Configurador de Prototipo MVP (URP)", EditorStyles.boldLabel);
        GUILayout.Space(10);
        GUILayout.Label("Esta herramienta preparara la escena como un MVP completo con:\n" +
                      "- Arena cerrada (suelo y paredes perimetrales con colisiones).\n" +
                      "- Texturas aplicadas en Suelo, Paredes y Puerta de salida.\n" +
                      "- Puerta blindada integrada en la pared norte conectada a la Llave Roja y Victoria.\n" +
                      "- Jugador con RIMovement, LG_Shoot y combate melee con bate (Hand).\n" +
                      "- HUD con barras de Vida, Estamina, Inventario, Balas y Reticula (Crosshair).\n" +
                      "- Coleccionables sincronizados ('Munición', 'Bateria', 'Llave Roja').\n" +
                      "- Zombis 3D animados y torre fisica interactiva.\n\n" +
                      "Asegurate de guardar la escena antes de proceder.", EditorStyles.wordWrappedLabel);
        
        GUILayout.Space(20);

        if (GUILayout.Button("Generar Prototipo Completo", GUILayout.Height(40)))
        {
            GenerarPrototipo();
        }
    }

    private static void GenerarPrototipo()
    {
        // 1. Crear Materiales URP con Texturas HD
        string texDir = "Assets/LG_Shooting/LGAssets/Textures";
        Material materialSuelo = ObtenerOCrearMaterialURPConTextura(
            "Assets/LG_Shooting/LGAssets/Materials/Mat_Suelo.mat",
            $"{texDir}/Tex_Suelo.png",
            $"{texDir}/Tex_Suelo_Normal.png",
            new Vector2(6f, 6f),
            Color.white
        );

        Material materialPared = ObtenerOCrearMaterialURPConTextura(
            "Assets/LG_Shooting/LGAssets/Materials/Mat_Pared.mat",
            $"{texDir}/Tex_Pared.png",
            $"{texDir}/Tex_Pared_Normal.png",
            new Vector2(3f, 1f),
            Color.white
        );

        Material materialPuerta = ObtenerOCrearMaterialURPConTextura(
            "Assets/LG_Shooting/LGAssets/Materials/Mat_Puerta.mat",
            $"{texDir}/Tex_Puerta.png",
            $"{texDir}/Tex_Puerta_Normal.png",
            new Vector2(1f, 1f),
            Color.white
        );

        Material materialMarco = ObtenerOCrearMaterialURP(
            "Assets/LG_Shooting/LGAssets/Materials/Mat_Marco.mat",
            new Color(0.18f, 0.18f, 0.2f)
        );

        Material materialObjetivos = ObtenerOCrearMaterialURP("Assets/LG_Shooting/LGAssets/Materials/Mat_Objetivos.mat", new Color(0.85f, 0.25f, 0.25f));
        Material materialBalas = ObtenerOCrearMaterialURP("Assets/LG_Shooting/LGAssets/Materials/Mat_Balas.mat", new Color(1f, 0.75f, 0f));
        
        // Materiales para Coleccionables
        Material materialMunicion = ObtenerOCrearMaterialURP("Assets/LG_Shooting/LGAssets/Materials/Mat_Collectible_Municion.mat", new Color(0.2f, 0.8f, 0.2f));
        Material materialBattery = ObtenerOCrearMaterialURP("Assets/LG_Shooting/LGAssets/Materials/Mat_Collectible_Battery.mat", new Color(0.2f, 0.6f, 0.9f));
        Material materialKey = ObtenerOCrearMaterialURP("Assets/LG_Shooting/LGAssets/Materials/Mat_Collectible_Key.mat", new Color(0.9f, 0.2f, 0.8f));

        // Clips de Audio SFX
        string audioDir = "Assets/LG_Shooting/LGAssets/Audio";
        AudioClip sfxShoot = AssetDatabase.LoadAssetAtPath<AudioClip>($"{audioDir}/SFX_Shoot.wav");
        AudioClip sfxReload = AssetDatabase.LoadAssetAtPath<AudioClip>($"{audioDir}/SFX_Reload.wav");
        AudioClip sfxEmpty = AssetDatabase.LoadAssetAtPath<AudioClip>($"{audioDir}/SFX_Empty.wav");
        AudioClip sfxSwing = AssetDatabase.LoadAssetAtPath<AudioClip>($"{audioDir}/SFX_Melee_Swing.wav");
        AudioClip sfxHit = AssetDatabase.LoadAssetAtPath<AudioClip>($"{audioDir}/SFX_Melee_Hit.wav");
        AudioClip sfxPickup = AssetDatabase.LoadAssetAtPath<AudioClip>($"{audioDir}/SFX_Pickup.wav");
        AudioClip sfxPickupAmmo = AssetDatabase.LoadAssetAtPath<AudioClip>($"{audioDir}/SFX_Pickup_Ammo.wav");
        AudioClip sfxPickupBattery = AssetDatabase.LoadAssetAtPath<AudioClip>($"{audioDir}/SFX_Pickup_Battery.wav");
        AudioClip sfxPickupKey = AssetDatabase.LoadAssetAtPath<AudioClip>($"{audioDir}/SFX_Pickup_Key.wav");
        AudioClip sfxZombieHit = AssetDatabase.LoadAssetAtPath<AudioClip>($"{audioDir}/SFX_Zombie_Hit.wav");
        AudioClip sfxPlayerHurt = AssetDatabase.LoadAssetAtPath<AudioClip>($"{audioDir}/SFX_Player_Hurt.wav");
        AudioClip sfxUnlock = AssetDatabase.LoadAssetAtPath<AudioClip>($"{audioDir}/SFX_Door_Unlock.wav");

        // Asegurar Luz Direccional para apreciar las texturas
        Light dirLight = Object.FindFirstObjectByType<Light>();
        if (dirLight == null)
        {
            GameObject lightGO = new GameObject("Directional Light");
            dirLight = lightGO.AddComponent<Light>();
            dirLight.type = LightType.Directional;
            lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            dirLight.intensity = 1.2f;
            dirLight.color = new Color(1f, 0.96f, 0.9f);
            Undo.RegisterCreatedObjectUndo(lightGO, "Crear Luz");
        }

        // 2. Configurar el Suelo de la Arena (30x30 metros)
        GameObject suelo = GameObject.Find("Suelo");
        if (suelo == null)
        {
            suelo = GameObject.CreatePrimitive(PrimitiveType.Plane);
            suelo.name = "Suelo";
        }
        suelo.transform.position = Vector3.zero;
        suelo.transform.localScale = new Vector3(3f, 1f, 3f); // 30x30 unidades
        suelo.GetComponent<Renderer>().sharedMaterial = materialSuelo;
        Undo.RegisterCreatedObjectUndo(suelo, "Crear Suelo");

        // 3. Crear Paredes Perimetrales de la Arena con hueco para la puerta
        CrearParedesArena(materialPared);

        // 3.1 Configurar el Techo de la Arena (roof)
        GameObject roof = GameObject.Find("roof");
        if (roof == null)
        {
            roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = "roof";
        }
        roof.transform.position = new Vector3(0.35f, 5.6f, -0.03f);
        roof.transform.localScale = new Vector3(10f, 1f, 30f);
        roof.transform.rotation = Quaternion.identity;
        roof.GetComponent<Renderer>().sharedMaterial = materialPared;
        Undo.RegisterCreatedObjectUndo(roof, "Crear Techo (roof)");

        // 3.2 Configurar Gestores de Escena (TimeManager y DialogueManager)
        GameObject timeMgrGO = GameObject.Find("TimeManager");
        if (timeMgrGO == null)
        {
            timeMgrGO = new GameObject("TimeManager");
        }
        timeMgrGO.transform.position = Vector3.zero;
        TimeManager timeMgr = timeMgrGO.GetComponent<TimeManager>();
        if (timeMgr == null) timeMgr = timeMgrGO.AddComponent<TimeManager>();
        Undo.RegisterCreatedObjectUndo(timeMgrGO, "Crear TimeManager");

        GameObject dialogueMgrGO = GameObject.Find("DialogueManager");
        if (dialogueMgrGO == null)
        {
            dialogueMgrGO = new GameObject("DialogueManager");
        }
        dialogueMgrGO.transform.position = Vector3.zero;
        LG_DialogueManager dialogueMgr = dialogueMgrGO.GetComponent<LG_DialogueManager>();
        if (dialogueMgr == null) dialogueMgr = dialogueMgrGO.AddComponent<LG_DialogueManager>();
        Undo.RegisterCreatedObjectUndo(dialogueMgrGO, "Crear DialogueManager");

        // 4. Crear el Prefab de la Bala si no existe
        string prefabPath = "Assets/LG_Shooting/LGAssets/BalaPrototipo.prefab";
        GameObject balaPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (balaPrefab == null)
        {
            GameObject tempBala = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tempBala.name = "BalaPrototipo";
            tempBala.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            
            LG_Bullet bulletComp = tempBala.AddComponent<LG_Bullet>();
            
            Rigidbody rb = tempBala.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            tempBala.GetComponent<Renderer>().sharedMaterial = materialBalas;

            Collider col = tempBala.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            if (!AssetDatabase.IsValidFolder("Assets/LG_Shooting/LGAssets"))
            {
                AssetDatabase.CreateFolder("Assets/LG_Shooting", "LGAssets");
            }

            balaPrefab = PrefabUtility.SaveAsPrefabAsset(tempBala, prefabPath);
            DestroyImmediate(tempBala);
        }
        else
        {
            Renderer r = balaPrefab.GetComponent<Renderer>();
            if (r != null && r.sharedMaterial != materialBalas)
            {
                r.sharedMaterial = materialBalas;
                EditorUtility.SetDirty(balaPrefab);
            }
        }

        // 5. Crear el Object Pool de Balas
        GameObject poolGO = GameObject.Find("BulletObjectPool");
        if (poolGO == null)
        {
            poolGO = new GameObject("BulletObjectPool");
        }
        poolGO.transform.position = Vector3.zero;
        LG_ObjectPool poolComp = poolGO.GetComponent<LG_ObjectPool>();
        if (poolComp == null)
        {
            poolComp = poolGO.AddComponent<LG_ObjectPool>();
        }

        SerializedObject poolSO = new SerializedObject(poolComp);
        poolSO.FindProperty("prefab").objectReferenceValue = balaPrefab;
        poolSO.FindProperty("initialSize").intValue = 30;
        poolSO.FindProperty("canGrow").boolValue = true;
        poolSO.ApplyModifiedProperties();
        Undo.RegisterCreatedObjectUndo(poolGO, "Crear Bullet Pool");

        // 6. Configurar el Player
        GameObject player = GameObject.Find("Player (RI + LG)");
        if (player == null)
        {
            player = new GameObject("Player (RI + LG)");
        }
        player.transform.position = new Vector3(0f, 1f, -11f); // Inicio al sur de la arena
        player.transform.rotation = Quaternion.identity;

        CharacterController charCtrl = player.GetComponent<CharacterController>();
        if (charCtrl == null) charCtrl = player.AddComponent<CharacterController>();
        charCtrl.height = 1.8f;
        charCtrl.center = new Vector3(0f, 0.9f, 0f);

        PlayerInput playerInput = player.GetComponent<PlayerInput>();
        if (playerInput == null) playerInput = player.AddComponent<PlayerInput>();
        InputActionAsset inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
        if (inputActions != null)
        {
            playerInput.actions = inputActions;
            playerInput.defaultControlScheme = "Keyboard&Mouse";
        }

        // Configurar Cabeza y Camara
        Transform cabeza = player.transform.Find("Cabeza");
        if (cabeza == null)
        {
            GameObject cabezaGO = new GameObject("Cabeza");
            cabeza = cabezaGO.transform;
            cabeza.SetParent(player.transform);
        }
        cabeza.localPosition = new Vector3(0f, 1.6f, 0f);
        cabeza.localRotation = Quaternion.identity;

        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.transform.SetParent(cabeza);
            mainCamera.transform.localPosition = Vector3.zero;
            mainCamera.transform.localRotation = Quaternion.identity;
        }

        // Añadir RIMovement
        RIMovement movement = player.GetComponent<RIMovement>();
        if (movement == null) movement = player.AddComponent<RIMovement>();
        movement.cabeza = cabeza;
        movement.caminar = 4f;
        movement.correr = 7f;
        movement.sensibilidadX = 0.15f;
        movement.sensibilidadY = 0.15f;

        // Añadir LG_Shoot
        LG_Shoot shoot = player.GetComponent<LG_Shoot>();
        if (shoot == null) shoot = player.AddComponent<LG_Shoot>();

        Transform firePoint = cabeza.Find("FirePoint");
        if (firePoint == null)
        {
            GameObject fpGO = new GameObject("FirePoint");
            firePoint = fpGO.transform;
            firePoint.SetParent(cabeza);
        }
        firePoint.localPosition = new Vector3(0.3f, -0.2f, 0.6f);
        firePoint.localRotation = Quaternion.identity;

        SerializedObject shootSO = new SerializedObject(shoot);
        shootSO.FindProperty("bulletPool").objectReferenceValue = poolComp;
        shootSO.FindProperty("firePoint").objectReferenceValue = firePoint;
        shootSO.FindProperty("fireRate").floatValue = 0.2f;
        if (sfxShoot != null) shootSO.FindProperty("shootSound").objectReferenceValue = sfxShoot;
        if (sfxReload != null) shootSO.FindProperty("reloadSound").objectReferenceValue = sfxReload;
        if (sfxEmpty != null) shootSO.FindProperty("emptySound").objectReferenceValue = sfxEmpty;
        shootSO.ApplyModifiedProperties();

        // Añadir LG_Inventory
        LG_Inventory inventory = player.GetComponent<LG_Inventory>();
        if (inventory == null) inventory = player.AddComponent<LG_Inventory>();

        // Añadir LG_PlayerHealth
        LG_PlayerHealth playerHealth = player.GetComponent<LG_PlayerHealth>();
        if (playerHealth == null) playerHealth = player.AddComponent<LG_PlayerHealth>();
        SerializedObject healthSO = new SerializedObject(playerHealth);
        if (sfxPlayerHurt != null) healthSO.FindProperty("hurtSound").objectReferenceValue = sfxPlayerHurt;
        healthSO.ApplyModifiedProperties();

        // Añadir Hand para armas y ataque melee
        Hand handComp = player.GetComponent<Hand>();
        if (handComp == null) handComp = player.AddComponent<Hand>();
        SerializedObject handSO = new SerializedObject(handComp);
        if (sfxSwing != null) handSO.FindProperty("swingSound").objectReferenceValue = sfxSwing;
        if (sfxHit != null) handSO.FindProperty("hitSound").objectReferenceValue = sfxHit;
        handSO.ApplyModifiedProperties();

        Undo.RegisterCreatedObjectUndo(player, "Configurar Player");

        // 7. Configurar el Canvas HUD
        GameObject canvasGO = GameObject.Find("HUD_Canvas");
        if (canvasGO == null)
        {
            canvasGO = new GameObject("HUD_Canvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasGO.AddComponent<GraphicRaycaster>();
        }

        // Limpiar elementos de HUD anteriores
        Transform oldHealthSlider = canvasGO.transform.Find("HealthSlider");
        if (oldHealthSlider != null) DestroyImmediate(oldHealthSlider.gameObject);
        Transform oldSlider = canvasGO.transform.Find("StaminaSlider");
        if (oldSlider != null) DestroyImmediate(oldSlider.gameObject);
        Transform oldText = canvasGO.transform.Find("InventoryText");
        if (oldText != null) DestroyImmediate(oldText.gameObject);
        Transform oldCrosshair = canvasGO.transform.Find("Crosshair");
        if (oldCrosshair != null) DestroyImmediate(oldCrosshair.gameObject);

        // Crear elementos UI
        Slider healthSlider = CrearSliderVida(canvasGO.transform);
        Slider staminaSlider = CrearSliderStamina(canvasGO.transform);
        Text inventoryText = CrearTextoInventario(canvasGO.transform);
        TMPro.TextMeshProUGUI ammoText = ConfigurarHUDMunicion(canvasGO.transform);
        CrearCrosshair(canvasGO.transform);

        // Conectar ammoText con LG_Shoot
        SerializedObject shootUpdateSO = new SerializedObject(shoot);
        shootUpdateSO.FindProperty("ammoText").objectReferenceValue = ammoText;
        shootUpdateSO.ApplyModifiedProperties();

        // Configurar LG_HUD
        LG_HUD hudComp = canvasGO.GetComponent<LG_HUD>();
        if (hudComp == null) hudComp = canvasGO.AddComponent<LG_HUD>();

        SerializedObject hudSO = new SerializedObject(hudComp);
        hudSO.FindProperty("playerMovement").objectReferenceValue = movement;
        hudSO.FindProperty("playerInventory").objectReferenceValue = inventory;
        hudSO.FindProperty("playerHealth").objectReferenceValue = playerHealth;
        hudSO.FindProperty("staminaSlider").objectReferenceValue = staminaSlider;
        hudSO.FindProperty("healthSlider").objectReferenceValue = healthSlider;
        hudSO.FindProperty("inventoryText").objectReferenceValue = inventoryText;
        hudSO.ApplyModifiedProperties();

        // Configurar SoundList en HUD_Canvas
        SoundList soundListComp = canvasGO.GetComponent<SoundList>();
        if (soundListComp == null) soundListComp = canvasGO.AddComponent<SoundList>();
        ConfigurarSoundList(soundListComp);

        // Configurar LG_TooltipManager y TooltipText en HUD_Canvas
        LG_TooltipManager tooltipMgr = canvasGO.GetComponent<LG_TooltipManager>();
        if (tooltipMgr == null) tooltipMgr = canvasGO.AddComponent<LG_TooltipManager>();
        TMPro.TextMeshProUGUI tooltipTextTMP = ConfigurarTooltipText(canvasGO.transform);
        SerializedObject tooltipSO = new SerializedObject(tooltipMgr);
        tooltipSO.FindProperty("tooltipText").objectReferenceValue = tooltipTextTMP;
        tooltipSO.ApplyModifiedProperties();

        // Configurar DialoguePanel en HUD_Canvas y conectarlo con DialogueManager
        ConfigurarDialogueUI(canvasGO.transform, dialogueMgr);

        Undo.RegisterCreatedObjectUndo(canvasGO, "Crear Canvas HUD");

        // Asegurar EventSystem
        if (UnityEngine.EventSystems.EventSystem.current == null)
        {
            GameObject esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            Undo.RegisterCreatedObjectUndo(esGO, "Crear EventSystem");
        }

        // 8. Crear coleccionables en la arena
        GameObject colRoot = GameObject.Find("Coleccionables");
        if (colRoot == null)
        {
            colRoot = new GameObject("Coleccionables");
        }

        for (int i = colRoot.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(colRoot.transform.GetChild(i).gameObject);
        }

        // Coleccionables posicionados en el recorrido con audios unicos
        CrearColeccionable(colRoot.transform, "Municion_Box_Green", new Vector3(4f, 0.6f, -6f), "Munición", 10, materialMunicion, sfxPickupAmmo);
        CrearColeccionable(colRoot.transform, "Battery_Pack_Blue", new Vector3(-4f, 0.6f, -6f), "Bateria", 1, materialBattery, sfxPickupBattery);
        CrearColeccionable(colRoot.transform, "Red_Key_Pink", new Vector3(0f, 0.6f, 9f), "Llave Roja", 1, materialKey, sfxPickupKey);
        Undo.RegisterCreatedObjectUndo(colRoot, "Crear Coleccionables Root");

        // 9. Crear objetivos fisicos interactivos (Torre de cubos al centro)
        GameObject obstaculosRoot = GameObject.Find("Obstaculos y Objetivos");
        if (obstaculosRoot == null)
        {
            obstaculosRoot = new GameObject("Obstaculos y Objetivos");
        }

        Vector3 spawnCenter = new Vector3(0f, 0.5f, 0f);
        for (int y = 0; y < 4; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                string targetName = $"Target_Cube_{x}_{y}";
                GameObject target = GameObject.Find(targetName);
                if (target == null)
                {
                    target = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    target.name = targetName;
                    target.transform.SetParent(obstaculosRoot.transform);
                }
                target.transform.position = spawnCenter + new Vector3(x * 1.1f, y * 1.1f, 0f);
                target.transform.rotation = Quaternion.identity;
                
                Rigidbody targetRb = target.GetComponent<Rigidbody>();
                if (targetRb == null) targetRb = target.AddComponent<Rigidbody>();
                targetRb.mass = 0.5f;

                target.GetComponent<Renderer>().sharedMaterial = materialObjetivos;
                Undo.RegisterCreatedObjectUndo(target, "Crear Objetivo Fisico");
            }
        }

        // 10. Crear Enemigos Zombis patrullando
        GameObject enemigosRoot = GameObject.Find("Enemigos");
        if (enemigosRoot == null)
        {
            enemigosRoot = new GameObject("Enemigos");
        }

        for (int i = enemigosRoot.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(enemigosRoot.transform.GetChild(i).gameObject);
        }

        Material materialEnemigo = ObtenerOCrearMaterialURP("Assets/LG_Shooting/LGAssets/Materials/Mat_Enemigo.mat", new Color(0.6f, 0.1f, 0.9f));
        Material materialBatteryDrop = AssetDatabase.LoadAssetAtPath<Material>("Assets/LG_Shooting/LGAssets/Materials/Mat_Collectible_Battery.mat");
        Material materialAmmoDrop = AssetDatabase.LoadAssetAtPath<Material>("Assets/LG_Shooting/LGAssets/Materials/Mat_Collectible_Municion.mat");

        CrearEnemigoPrototipo(enemigosRoot.transform, "Enemigo_Izquierda", new Vector3(-6f, 1f, 4f), materialEnemigo, materialBatteryDrop, "Bateria", 1, sfxZombieHit);
        CrearEnemigoPrototipo(enemigosRoot.transform, "Enemigo_Derecha", new Vector3(6f, 1f, 4f), materialEnemigo, materialAmmoDrop, "Munición", 5, sfxZombieHit);
        Undo.RegisterCreatedObjectUndo(enemigosRoot, "Crear Enemigos Root");

        // 11. Crear Puerta de Salida en la pared Norte
        CrearPuertaSalida(new Vector3(0f, 0f, 14.8f), materialPuerta, materialMarco, sfxUnlock, sfxEmpty);

        // 12. Crear Terminal interactiva de diálogo en la Arena
        CrearDialogueTerminal(new Vector3(-13.515f, 1f, 12.233f));

        EditorUtility.DisplayDialog("Éxito Prototipo", 
            "¡El prototipo se ha configurado con éxito y paridad total con LG_Scene!\n\n" +
            "✔ Arena cerrada con Suelo, Paredes y Techo (roof).\n" +
            "✔ Gestores TimeManager y DialogueManager inicializados.\n" +
            "✔ Canvas HUD con SoundList (12 SFX y Mixer), LG_TooltipManager, Sliders, Balas y Diálogos.\n" +
            "✔ Terminal interactiva de diálogo y Puerta blindada de salida (Llave Roja).\n" +
            "✔ Enemigos, Objetivos y Coleccionables sincronizados.\n\n" +
            "¡Haz clic en Play para probar!", 
            "OK");
    }

    private static void CrearParedesArena(Material matPared)
    {
        GameObject paredesRoot = GameObject.Find("Paredes_Arena");
        if (paredesRoot == null)
        {
            paredesRoot = new GameObject("Paredes_Arena");
        }
        paredesRoot.transform.position = Vector3.zero;
        paredesRoot.transform.rotation = Quaternion.identity;

        // Altura y grosor estándar
        float altura = 4.5f;
        float grosor = 0.6f;
        float yPos = altura / 2f;

        // Pared Sur (Espalda del jugador, z = -15)
        ConfigurarMuro(paredesRoot.transform, "Muro_Sur", new Vector3(0f, yPos, -15f), new Vector3(30f, altura, grosor), matPared);

        // Pared Oeste (Izquierda, x = -15)
        ConfigurarMuro(paredesRoot.transform, "Muro_Oeste", new Vector3(-15f, yPos, 0f), new Vector3(grosor, altura, 30f), matPared);

        // Pared Este (Derecha, x = 15)
        ConfigurarMuro(paredesRoot.transform, "Muro_Este", new Vector3(15f, yPos, 0f), new Vector3(grosor, altura, 30f), matPared);

        // Pared Norte: Dividida en Izquierda, Derecha y Dintel para dejar vano de puerta de 2.6m en el centro
        float anchoVano = 2.6f;
        float anchoSegmento = (30f - anchoVano) / 2f; // 13.7m
        float offsetCentro = (anchoVano / 2f) + (anchoSegmento / 2f); // 8.15m

        ConfigurarMuro(paredesRoot.transform, "Muro_Norte_Izq", new Vector3(-offsetCentro, yPos, 15f), new Vector3(anchoSegmento, altura, grosor), matPared);
        ConfigurarMuro(paredesRoot.transform, "Muro_Norte_Der", new Vector3(offsetCentro, yPos, 15f), new Vector3(anchoSegmento, altura, grosor), matPared);

        // Dintel superior sobre la puerta
        float alturaDintel = altura - 3.2f; // 1.3m sobre la puerta
        float yDintel = 3.2f + (alturaDintel / 2f);
        ConfigurarMuro(paredesRoot.transform, "Muro_Norte_Dintel", new Vector3(0f, yDintel, 15f), new Vector3(anchoVano, alturaDintel, grosor), matPared);

        Undo.RegisterCreatedObjectUndo(paredesRoot, "Crear Paredes Arena");
    }

    private static void ConfigurarMuro(Transform parent, string nombre, Vector3 pos, Vector3 escala, Material mat)
    {
        Transform oldT = parent.Find(nombre);
        GameObject muroGO = oldT != null ? oldT.gameObject : GameObject.CreatePrimitive(PrimitiveType.Cube);
        muroGO.name = nombre;
        muroGO.transform.SetParent(parent);
        muroGO.transform.position = pos;
        muroGO.transform.localScale = escala;
        muroGO.transform.rotation = Quaternion.identity;

        Renderer r = muroGO.GetComponent<Renderer>();
        if (r != null && mat != null)
        {
            r.sharedMaterial = mat;
        }
    }

    private static void CrearPuertaSalida(Vector3 position, Material matPuerta, Material matMarco, AudioClip sfxUnlock = null, AudioClip sfxDenied = null)
    {
        GameObject puertaRoot = GameObject.Find("Puerta_Salida");
        if (puertaRoot == null)
        {
            puertaRoot = new GameObject("Puerta_Salida");
        }
        puertaRoot.transform.position = position;
        puertaRoot.transform.rotation = Quaternion.identity;

        // Marco izquierdo
        Transform oldIzq = puertaRoot.transform.Find("Marco_Izq");
        GameObject marcoIzq = oldIzq != null ? oldIzq.gameObject : GameObject.CreatePrimitive(PrimitiveType.Cube);
        marcoIzq.name = "Marco_Izq";
        marcoIzq.transform.SetParent(puertaRoot.transform);
        marcoIzq.transform.localPosition = new Vector3(-1.2f, 1.6f, 0f);
        marcoIzq.transform.localScale = new Vector3(0.25f, 3.2f, 0.4f);
        if (matMarco != null) marcoIzq.GetComponent<Renderer>().sharedMaterial = matMarco;

        // Marco derecho
        Transform oldDer = puertaRoot.transform.Find("Marco_Der");
        GameObject marcoDer = oldDer != null ? oldDer.gameObject : GameObject.CreatePrimitive(PrimitiveType.Cube);
        marcoDer.name = "Marco_Der";
        marcoDer.transform.SetParent(puertaRoot.transform);
        marcoDer.transform.localPosition = new Vector3(1.2f, 1.6f, 0f);
        marcoDer.transform.localScale = new Vector3(0.25f, 3.2f, 0.4f);
        if (matMarco != null) marcoDer.GetComponent<Renderer>().sharedMaterial = matMarco;

        // Marco superior
        Transform oldSup = puertaRoot.transform.Find("Marco_Sup");
        GameObject marcoSup = oldSup != null ? oldSup.gameObject : GameObject.CreatePrimitive(PrimitiveType.Cube);
        marcoSup.name = "Marco_Sup";
        marcoSup.transform.SetParent(puertaRoot.transform);
        marcoSup.transform.localPosition = new Vector3(0f, 3.2f, 0f);
        marcoSup.transform.localScale = new Vector3(2.65f, 0.25f, 0.4f);
        if (matMarco != null) marcoSup.GetComponent<Renderer>().sharedMaterial = matMarco;

        // Bisagra y Hoja de la puerta
        Transform oldBisagra = puertaRoot.transform.Find("Door_Hinge");
        GameObject bisagra = oldBisagra != null ? oldBisagra.gameObject : new GameObject("Door_Hinge");
        bisagra.transform.SetParent(puertaRoot.transform);
        bisagra.transform.localPosition = new Vector3(-1.1f, 0f, 0f);
        bisagra.transform.localRotation = Quaternion.identity;

        Transform oldPanel = bisagra.transform.Find("Door_Mesh");
        GameObject doorPanel = oldPanel != null ? oldPanel.gameObject : GameObject.CreatePrimitive(PrimitiveType.Cube);
        doorPanel.name = "Door_Mesh";
        doorPanel.transform.SetParent(bisagra.transform);
        doorPanel.transform.localPosition = new Vector3(1.1f, 1.6f, 0f);
        doorPanel.transform.localScale = new Vector3(2.2f, 3.1f, 0.15f);
        if (matPuerta != null) doorPanel.GetComponent<Renderer>().sharedMaterial = matPuerta;

        // Componente Door
        Door doorComp = puertaRoot.GetComponent<Door>();
        if (doorComp == null) doorComp = puertaRoot.AddComponent<Door>();

        SerializedObject doorSO = new SerializedObject(doorComp);
        doorSO.FindProperty("doorHinge").objectReferenceValue = bisagra.transform;
        doorSO.FindProperty("requiredKeyName").stringValue = "Llave Roja";
        doorSO.FindProperty("openAngle").floatValue = -95f;
        doorSO.FindProperty("openDuration").floatValue = 1.2f;
        doorSO.ApplyModifiedProperties();

        // Collider Trigger para interacción
        BoxCollider triggerCol = puertaRoot.GetComponent<BoxCollider>();
        if (triggerCol == null) triggerCol = puertaRoot.AddComponent<BoxCollider>();
        triggerCol.isTrigger = true;
        triggerCol.center = new Vector3(0f, 1.6f, 0.5f);
        triggerCol.size = new Vector3(3f, 3.5f, 3f);

        Undo.RegisterCreatedObjectUndo(puertaRoot, "Crear Puerta Salida");
    }

    private static Material ObtenerOCrearMaterialURPConTextura(string matPath, string texPath, string normalPath, Vector2 tiling, Color tint)
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mat == null)
        {
            Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
            if (urpShader == null) urpShader = Shader.Find("Standard");
            
            mat = new Material(urpShader);
            string dir = System.IO.Path.GetDirectoryName(matPath);
            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }
            AssetDatabase.CreateAsset(mat, matPath);
        }

        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
        if (tex != null)
        {
            mat.SetTexture("_BaseMap", tex);
            mat.SetTexture("_MainTex", tex);
            mat.SetTextureScale("_BaseMap", tiling);
            mat.SetTextureScale("_MainTex", tiling);
        }

        if (!string.IsNullOrEmpty(normalPath))
        {
            Texture2D normTex = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
            if (normTex != null)
            {
                mat.SetTexture("_BumpMap", normTex);
                mat.SetTextureScale("_BumpMap", tiling);
                mat.EnableKeyword("_NORMALMAP");
                mat.SetFloat("_BumpScale", 1.0f);
            }
        }

        mat.color = tint;
        mat.SetFloat("_Smoothness", 0.35f);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static Material ObtenerOCrearMaterialURP(string path, Color color)
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
            if (urpShader == null) urpShader = Shader.Find("Standard");
            
            mat = new Material(urpShader);
            mat.color = color;
            
            string dir = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }

            AssetDatabase.CreateAsset(mat, path);
        }
        else
        {
            mat.color = color;
            EditorUtility.SetDirty(mat);
        }
        return mat;
    }

    private static Slider CrearSliderVida(Transform parent)
    {
        GameObject sliderGO = new GameObject("HealthSlider");
        sliderGO.transform.SetParent(parent, false);
        RectTransform sliderRect = sliderGO.AddComponent<RectTransform>();
        sliderRect.anchoredPosition = new Vector2(40f, -20f);
        sliderRect.sizeDelta = new Vector2(250f, 15f);
        sliderRect.anchorMin = new Vector2(0f, 1f);
        sliderRect.anchorMax = new Vector2(0f, 1f);
        sliderRect.pivot = new Vector2(0f, 1f);

        Slider slider = sliderGO.AddComponent<Slider>();

        GameObject bgGO = new GameObject("Background");
        bgGO.transform.SetParent(sliderRect, false);
        RectTransform bgRect = bgGO.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        Image bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.6f);

        GameObject fillAreaGO = new GameObject("Fill Area");
        fillAreaGO.transform.SetParent(sliderRect, false);
        RectTransform fillAreaRect = fillAreaGO.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = Vector2.zero;

        GameObject fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(fillAreaRect, false);
        RectTransform fillRect = fillGO.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        Image fillImg = fillGO.AddComponent<Image>();
        fillImg.color = new Color(0.85f, 0.15f, 0.15f, 0.8f);

        slider.targetGraphic = bgImg;
        slider.fillRect = fillRect;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;

        return slider;
    }

    private static Slider CrearSliderStamina(Transform parent)
    {
        GameObject sliderGO = new GameObject("StaminaSlider");
        sliderGO.transform.SetParent(parent, false);
        RectTransform sliderRect = sliderGO.AddComponent<RectTransform>();
        sliderRect.anchoredPosition = new Vector2(40f, -45f);
        sliderRect.sizeDelta = new Vector2(250f, 15f);
        sliderRect.anchorMin = new Vector2(0f, 1f);
        sliderRect.anchorMax = new Vector2(0f, 1f);
        sliderRect.pivot = new Vector2(0f, 1f);

        Slider slider = sliderGO.AddComponent<Slider>();

        GameObject bgGO = new GameObject("Background");
        bgGO.transform.SetParent(sliderRect, false);
        RectTransform bgRect = bgGO.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        Image bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.6f);

        GameObject fillAreaGO = new GameObject("Fill Area");
        fillAreaGO.transform.SetParent(sliderRect, false);
        RectTransform fillAreaRect = fillAreaGO.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = Vector2.zero;

        GameObject fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(fillAreaRect, false);
        RectTransform fillRect = fillGO.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        Image fillImg = fillGO.AddComponent<Image>();
        fillImg.color = new Color(0.2f, 0.6f, 1f, 0.8f);

        slider.targetGraphic = bgImg;
        slider.fillRect = fillRect;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;

        return slider;
    }

    private static Text CrearTextoInventario(Transform parent)
    {
        GameObject textGO = new GameObject("InventoryText");
        textGO.transform.SetParent(parent, false);
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchoredPosition = new Vector2(40f, -85f);
        textRect.sizeDelta = new Vector2(350f, 250f);
        textRect.anchorMin = new Vector2(0f, 1f);
        textRect.anchorMax = new Vector2(0f, 1f);
        textRect.pivot = new Vector2(0f, 1f);

        Text text = textGO.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 20;
        text.color = Color.white;
        text.supportRichText = true;
        text.alignment = TextAnchor.UpperLeft;

        Shadow shadow = textGO.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
        shadow.effectDistance = new Vector2(1.5f, -1.5f);

        return text;
    }

    private static TMPro.TextMeshProUGUI ConfigurarHUDMunicion(Transform canvasTransform)
    {
        // 1. Eliminar cualquier texto de munición suelto anterior en el Canvas principal (esquina superior derecha)
        for (int i = canvasTransform.childCount - 1; i >= 0; i--)
        {
            Transform child = canvasTransform.GetChild(i);
            if (child.name == "MunicionText" || child.name == "AmmoText")
            {
                DestroyImmediate(child.gameObject);
            }
        }

        // 2. Buscar o crear MunicionContainer en la esquina inferior derecha
        Transform containerT = canvasTransform.Find("MunicionContainer");
        GameObject containerGO;
        if (containerT == null)
        {
            GameObject sceneContainer = GameObject.Find("MunicionContainer");
            if (sceneContainer != null)
            {
                containerGO = sceneContainer;
                containerGO.transform.SetParent(canvasTransform, false);
            }
            else
            {
                containerGO = new GameObject("MunicionContainer");
                containerGO.transform.SetParent(canvasTransform, false);
            }
        }
        else
        {
            containerGO = containerT.gameObject;
        }

        RectTransform containerRect = containerGO.GetComponent<RectTransform>();
        if (containerRect == null) containerRect = containerGO.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(1f, 0f);
        containerRect.anchorMax = new Vector2(1f, 0f);
        containerRect.pivot = new Vector2(1f, 0f);
        containerRect.anchoredPosition = new Vector2(-35f, 25f);
        containerRect.sizeDelta = new Vector2(220f, 75f);

        // 3. Buscar o crear MunicionPannel con estilo táctico moderno (Dark Glassmorphism)
        Transform panelT = containerGO.transform.Find("MunicionPannel");
        GameObject panelGO;
        if (panelT == null)
        {
            panelGO = new GameObject("MunicionPannel");
            panelGO.transform.SetParent(containerGO.transform, false);
        }
        else
        {
            panelGO = panelT.gameObject;
        }

        RectTransform panelRect = panelGO.GetComponent<RectTransform>();
        if (panelRect == null) panelRect = panelGO.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = Vector2.zero;
        panelRect.pivot = new Vector2(0.5f, 0.5f);

        Image panelImg = panelGO.GetComponent<Image>();
        if (panelImg == null) panelImg = panelGO.AddComponent<Image>();
        panelImg.color = new Color(0.06f, 0.09f, 0.14f, 0.85f);

        // 4. Buscar o crear MunicionText con tipografía profesional
        Transform textT = panelGO.transform.Find("MunicionText");
        if (textT == null) textT = containerGO.transform.Find("MunicionText");
        GameObject textGO;
        if (textT == null)
        {
            textGO = new GameObject("MunicionText");
            textGO.transform.SetParent(panelGO.transform, false);
        }
        else
        {
            textGO = textT.gameObject;
            textGO.transform.SetParent(panelGO.transform, false);
        }

        RectTransform textRect = textGO.GetComponent<RectTransform>();
        if (textRect == null) textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = new Vector2(-16f, -8f);
        textRect.pivot = new Vector2(0.5f, 0.5f);

        TMPro.TextMeshProUGUI tmp = textGO.GetComponent<TMPro.TextMeshProUGUI>();
        if (tmp == null) tmp = textGO.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.fontSize = 36;
        tmp.richText = true;
        tmp.text = "<b><size=38><color=#FFFFFF>12</color></size></b><size=20><color=#64748B> / </color><color=#CBD5E1>10</color></size>\n<size=11><color=#38BDF8><b>PISTOLA 9MM</b></color></size>";

        return tmp;
    }

    private static void CrearCrosshair(Transform parent)
    {
        GameObject crosshairGO = new GameObject("Crosshair");
        crosshairGO.transform.SetParent(parent, false);
        RectTransform rect = crosshairGO.AddComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(6f, 6f);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        Image img = crosshairGO.AddComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.85f);
    }

    private static void CrearColeccionable(Transform parent, string goName, Vector3 position, string itemName, int amount, Material mat, AudioClip sfxPickup = null)
    {
        GameObject colGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        colGO.name = goName;
        colGO.transform.SetParent(parent);
        colGO.transform.position = position;
        colGO.transform.rotation = Quaternion.Euler(45f, 45f, 45f);
        colGO.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        LG_Collectible collectible = colGO.AddComponent<LG_Collectible>();
        
        SerializedObject colSO = new SerializedObject(collectible);
        colSO.FindProperty("itemName").stringValue = itemName;
        colSO.FindProperty("amount").intValue = amount;
        colSO.FindProperty("rotationSpeed").floatValue = 55f;
        colSO.FindProperty("bobFrequency").floatValue = 2f;
        colSO.FindProperty("bobAmplitude").floatValue = 0.15f;
        if (sfxPickup != null) colSO.FindProperty("pickupSound").objectReferenceValue = sfxPickup;
        colSO.ApplyModifiedProperties();

        colGO.GetComponent<Renderer>().sharedMaterial = mat;

        Collider c = colGO.GetComponent<Collider>();
        if (c != null) c.isTrigger = true;

        Undo.RegisterCreatedObjectUndo(colGO, $"Crear Coleccionable {goName}");
    }

    private static void CrearEnemigoPrototipo(Transform parent, string goName, Vector3 position, Material matEnemigo, Material matDrop, string dropItem, int dropQty, AudioClip sfxHurt = null)
    {
        string zombiePrefabPath = "Assets/ZombieMale_AAB/Prefabs/URP/ZombieMale_AAB_URP.prefab";
        GameObject zombiePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(zombiePrefabPath);

        GameObject enemigoGO;
        
        if (zombiePrefab != null)
        {
            enemigoGO = new GameObject(goName);
            enemigoGO.transform.SetParent(parent);
            enemigoGO.transform.position = position;
            enemigoGO.transform.localScale = Vector3.one;

            GameObject modelGO = (GameObject)PrefabUtility.InstantiatePrefab(zombiePrefab, enemigoGO.transform);
            modelGO.transform.localPosition = Vector3.zero;
            modelGO.transform.localRotation = Quaternion.identity;
            modelGO.transform.localScale = Vector3.one;

            CapsuleCollider col = enemigoGO.GetComponent<CapsuleCollider>();
            if (col == null) col = enemigoGO.AddComponent<CapsuleCollider>();
            col.height = 1.8f;
            col.center = new Vector3(0f, 0.9f, 0f);
            col.radius = 0.4f;
        }
        else
        {
            enemigoGO = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemigoGO.name = goName;
            enemigoGO.transform.SetParent(parent);
            enemigoGO.transform.position = position;
            enemigoGO.transform.localScale = Vector3.one;
            enemigoGO.GetComponent<Renderer>().sharedMaterial = matEnemigo;
        }

        Rigidbody rb = enemigoGO.GetComponent<Rigidbody>();
        if (rb == null) rb = enemigoGO.AddComponent<Rigidbody>();
        rb.mass = 1.5f;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        LG_Enemy enemyComp = enemigoGO.GetComponent<LG_Enemy>();
        if (enemyComp == null) enemyComp = enemigoGO.AddComponent<LG_Enemy>();

        SerializedObject enemySO = new SerializedObject(enemyComp);
        enemySO.FindProperty("maxHealth").floatValue = 3f;
        enemySO.FindProperty("speed").floatValue = 2.5f;
        enemySO.FindProperty("chaseRange").floatValue = 15f;
        enemySO.FindProperty("stopDistance").floatValue = 1.5f;
        enemySO.FindProperty("dropItemOnDeath").boolValue = true;
        enemySO.FindProperty("dropItemName").stringValue = dropItem;
        enemySO.FindProperty("dropAmount").intValue = dropQty;
        enemySO.FindProperty("dropMaterial").objectReferenceValue = matDrop;
        if (sfxHurt != null) enemySO.FindProperty("hurtSound").objectReferenceValue = sfxHurt;
        
        GameObject player = GameObject.Find("Player (RI + LG)");
        if (player != null)
        {
            enemySO.FindProperty("target").objectReferenceValue = player.transform;
        }

        enemySO.ApplyModifiedProperties();

        Undo.RegisterCreatedObjectUndo(enemigoGO, $"Crear Enemigo {goName}");
    }

    private static void ConfigurarSoundList(SoundList soundListComp)
    {
        string audioDir = "Assets/LG_Shooting/LGAssets/Audio";
        AudioMixer mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>("Assets/AExport/GameMixer.mixer");
        AudioMixerGroup sfxMixerGroup = null;
        if (mixer != null)
        {
            AudioMixerGroup[] groups = mixer.FindMatchingGroups("SFX");
            if (groups != null && groups.Length > 0) sfxMixerGroup = groups[0];
        }

        var soundDefs = new (string name, string file, float vol, float pitch)[]
        {
            ("SFX_Door_Unlock", "SFX_Door_Unlock.wav", 0.5f, 1f),
            ("SFX_Empty", "SFX_Empty.wav", 0.5f, 1f),
            ("SFX_Melee_Hit", "SFX_Melee_Hit.wav", 0.5f, 1f),
            ("SFX_Melee_Swing", "SFX_Melee_Swing.wav", 0.6f, 1f),
            ("SFX_Pickup", "SFX_Pickup.wav", 0.5f, 1f),
            ("SFX_Pickup_Ammo", "SFX_Pickup_Ammo.wav", 0.5f, 1f),
            ("SFX_Pickup_Battery", "SFX_Pickup_Battery.wav", 0.5f, 1f),
            ("SFX_Pickup_Key", "SFX_Pickup_Key.wav", 0.5f, 1f),
            ("SFX_Player_Hurt", "SFX_Player_Hurt.wav", 0.5f, 1f),
            ("SFX_Reload", "SFX_Reload.wav", 0.5f, 1f),
            ("SFX_Shoot", "SFX_Shoot.wav", 0.5f, 1f),
            ("SFX_Zombie_Hit", "SFX_Zombie_Hit.wav", 0.5f, 1f)
        };

        SerializedObject slSO = new SerializedObject(soundListComp);
        SerializedProperty listProp = slSO.FindProperty("soundList");
        listProp.arraySize = soundDefs.Length;

        for (int i = 0; i < soundDefs.Length; i++)
        {
            SerializedProperty elem = listProp.GetArrayElementAtIndex(i);
            elem.FindPropertyRelative("audioName").stringValue = soundDefs[i].name;
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{audioDir}/{soundDefs[i].file}");
            elem.FindPropertyRelative("clip").objectReferenceValue = clip;
            elem.FindPropertyRelative("volume").floatValue = soundDefs[i].vol;
            elem.FindPropertyRelative("pitch").floatValue = soundDefs[i].pitch;
            elem.FindPropertyRelative("loop").boolValue = false;
            elem.FindPropertyRelative("mixer").objectReferenceValue = sfxMixerGroup;
        }

        slSO.ApplyModifiedProperties();
    }

    private static TMPro.TextMeshProUGUI ConfigurarTooltipText(Transform canvasTransform)
    {
        Transform oldT = canvasTransform.Find("TooltipText");
        GameObject go = oldT != null ? oldT.gameObject : new GameObject("TooltipText");
        go.name = "TooltipText";
        go.transform.SetParent(canvasTransform, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        if (rect == null) rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, -341f);
        rect.sizeDelta = new Vector2(200f, 50f);

        TMPro.TextMeshProUGUI tmp = go.GetComponent<TMPro.TextMeshProUGUI>();
        if (tmp == null) tmp = go.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.fontSize = 24;
        tmp.color = Color.white;
        tmp.text = "";
        go.SetActive(false);

        return tmp;
    }

    private static void ConfigurarDialogueUI(Transform canvasTransform, LG_DialogueManager dialogueMgr)
    {
        Transform panelT = canvasTransform.Find("DialoguePanel");
        GameObject panelGO = panelT != null ? panelT.gameObject : new GameObject("DialoguePanel");
        panelGO.name = "DialoguePanel";
        panelGO.transform.SetParent(canvasTransform, false);

        RectTransform panelRect = panelGO.GetComponent<RectTransform>();
        if (panelRect == null) panelRect = panelGO.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(1920f, 1080f);

        Image panelImg = panelGO.GetComponent<Image>();
        if (panelImg == null) panelImg = panelGO.AddComponent<Image>();
        panelImg.color = new Color(0.08f, 0.08f, 0.12f, 0.5f);

        // DialogueText
        Transform textT = panelGO.transform.Find("DialogueText");
        GameObject textGO = textT != null ? textT.gameObject : new GameObject("DialogueText");
        textGO.name = "DialogueText";
        textGO.transform.SetParent(panelGO.transform, false);

        RectTransform textRect = textGO.GetComponent<RectTransform>();
        if (textRect == null) textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = new Vector2(0f, -286f);
        textRect.sizeDelta = new Vector2(1000f, 250f);

        TMPro.TextMeshProUGUI dtTMP = textGO.GetComponent<TMPro.TextMeshProUGUI>();
        if (dtTMP == null) dtTMP = textGO.AddComponent<TMPro.TextMeshProUGUI>();
        dtTMP.alignment = TMPro.TextAlignmentOptions.Center;
        dtTMP.fontSize = 32;
        dtTMP.color = Color.white;
        dtTMP.richText = true;

        // OptionsContainer
        Transform optT = panelGO.transform.Find("OptionsContainer");
        GameObject optGO = optT != null ? optT.gameObject : new GameObject("OptionsContainer");
        optGO.name = "OptionsContainer";
        optGO.transform.SetParent(panelGO.transform, false);

        RectTransform optRect = optGO.GetComponent<RectTransform>();
        if (optRect == null) optRect = optGO.AddComponent<RectTransform>();
        optRect.anchorMin = new Vector2(0.5f, 0.5f);
        optRect.anchorMax = new Vector2(0.5f, 0.5f);
        optRect.pivot = new Vector2(0.5f, 0.5f);
        optRect.anchoredPosition = new Vector2(0f, -448f);
        optRect.sizeDelta = new Vector2(400f, 80f);

        HorizontalLayoutGroup layout = optGO.GetComponent<HorizontalLayoutGroup>();
        if (layout == null) layout = optGO.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 30f;
        layout.childControlWidth = false;
        layout.childControlHeight = false;

        Button truthBtn = CrearBotonDialogo(optGO.transform, "TruthButton", "Verdad", new Color(0.2f, 0.7f, 0.3f));
        Button lieBtn = CrearBotonDialogo(optGO.transform, "LieButton", "Mentir", new Color(0.85f, 0.2f, 0.2f));

        panelGO.SetActive(false);
        optGO.SetActive(false);

        if (dialogueMgr != null)
        {
            SerializedObject dSO = new SerializedObject(dialogueMgr);
            dSO.FindProperty("dialoguePanel").objectReferenceValue = panelGO;
            dSO.FindProperty("dialogueText").objectReferenceValue = dtTMP;
            dSO.FindProperty("optionsContainer").objectReferenceValue = optGO;
            dSO.FindProperty("truthButton").objectReferenceValue = truthBtn;
            dSO.FindProperty("lieButton").objectReferenceValue = lieBtn;
            dSO.FindProperty("typingSpeed").floatValue = 0.02f;
            dSO.ApplyModifiedProperties();
        }
    }

    private static Button CrearBotonDialogo(Transform parent, string goName, string buttonLabel, Color normalColor)
    {
        Transform oldB = parent.Find(goName);
        GameObject btnGO = oldB != null ? oldB.gameObject : new GameObject(goName);
        btnGO.name = goName;
        btnGO.transform.SetParent(parent, false);

        RectTransform btnRect = btnGO.GetComponent<RectTransform>();
        if (btnRect == null) btnRect = btnGO.AddComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(160f, 45f);

        Image img = btnGO.GetComponent<Image>();
        if (img == null) img = btnGO.AddComponent<Image>();
        img.color = normalColor;

        Button btn = btnGO.GetComponent<Button>();
        if (btn == null) btn = btnGO.AddComponent<Button>();

        Transform lblT = btnGO.transform.Find("Text");
        GameObject lblGO = lblT != null ? lblT.gameObject : new GameObject("Text");
        lblGO.name = "Text";
        lblGO.transform.SetParent(btnGO.transform, false);

        RectTransform lblRect = lblGO.GetComponent<RectTransform>();
        if (lblRect == null) lblRect = lblGO.AddComponent<RectTransform>();
        lblRect.anchorMin = Vector2.zero;
        lblRect.anchorMax = Vector2.one;
        lblRect.sizeDelta = Vector2.zero;
        lblRect.anchoredPosition = Vector2.zero;

        TMPro.TextMeshProUGUI tmp = lblGO.GetComponent<TMPro.TextMeshProUGUI>();
        if (tmp == null) tmp = lblGO.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.fontSize = 20;
        tmp.text = buttonLabel;
        tmp.color = Color.white;

        return btn;
    }

    private static void CrearDialogueTerminal(Vector3 pos)
    {
        GameObject terminalGO = GameObject.Find("DialogueTerminal - Test");
        if (terminalGO == null)
        {
            terminalGO = new GameObject("DialogueTerminal - Test");
        }
        terminalGO.name = "DialogueTerminal - Test";
        terminalGO.transform.position = pos;
        terminalGO.transform.rotation = Quaternion.identity;

        BoxCollider col = terminalGO.GetComponent<BoxCollider>();
        if (col == null) col = terminalGO.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(4f, 4f, 4f);
        col.center = Vector3.zero;

        LG_DialogueTrigger trigger = terminalGO.GetComponent<LG_DialogueTrigger>();
        if (trigger == null) trigger = terminalGO.AddComponent<LG_DialogueTrigger>();
        trigger.dialogueLines = new string[]
        {
            "Diálogo 1",
            "Diálogo 2",
            "Diálogo 3",
            "Diálogo 4"
        };

        // Hijo visual Base (Cylinder)
        Transform baseT = terminalGO.transform.Find("Base");
        GameObject baseGO = baseT != null ? baseT.gameObject : GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        baseGO.name = "Base";
        baseGO.transform.SetParent(terminalGO.transform);
        baseGO.transform.localPosition = Vector3.zero;
        baseGO.transform.localScale = Vector3.one;
        baseGO.transform.localRotation = Quaternion.identity;

        // Hijo visual Cube (Cube)
        Transform cubeT = terminalGO.transform.Find("Cube");
        GameObject cubeGO = cubeT != null ? cubeT.gameObject : GameObject.CreatePrimitive(PrimitiveType.Cube);
        cubeGO.name = "Cube";
        cubeGO.transform.SetParent(terminalGO.transform);
        cubeGO.transform.localPosition = new Vector3(0f, 1.032f, 0f);
        cubeGO.transform.localScale = new Vector3(1f, 1f, 1.7681f);
        cubeGO.transform.localRotation = Quaternion.identity;

        Undo.RegisterCreatedObjectUndo(terminalGO, "Crear DialogueTerminal - Test");
    }
}
