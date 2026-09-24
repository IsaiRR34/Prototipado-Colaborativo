using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.Rendering.Universal;
using TMPro;

/// <summary>
/// Utilidad centralizada para generar y configurar al 100% el Jugador (Player RI + LG),
/// sus armas 3D, cámara, sistemas de disparo, cuerpo, HUD completo y referencias de audio.
/// Asegura paridad total entre el prototipo base, la secuencia de niveles DOOM y escenas de pruebas.
/// </summary>
public static class PlayerSetupUtility
{
    private const string INPUT_ACTIONS_PATH = "Assets/InputSystem_Actions.inputactions";
    private const string GUN_PATH = "Assets/Low Poly Guns/Models/Guns/assault1/assault1.fbx";
    private const string GUN_MAT_PATH = "Assets/Low Poly Guns/Models/Guns/assault1/assault1.mat";
    private const string FLASHLIGHT_PREFAB_PATH = "Assets/Free-FlashLight/Prefabs/FlashLight.prefab";
    private const string BAT_PREFAB_PATH = "Assets/Melee/Bat/Prefabs/Baseball_Bat_03.prefab";
    private const string BULLET_PREFAB_PATH = "Assets/LG_Shooting/LGAssets/BalaPrototipo.prefab";
    private const string AUDIO_DIR = "Assets/LG_Shooting/LGAssets/Audio";
    private const string MIXER_PATH = "Assets/AExport/GameMixer.mixer";

    [MenuItem("Prototipo/Configurar Solo Jugador Completo")]
    public static void MenuConfigurarSoloJugador()
    {
        GameObject existingPlayer = GameObject.Find("Player (RI + LG)");
        Vector3 pos = existingPlayer != null ? existingPlayer.transform.position : new Vector3(0f, 1f, -11f);

        LG_ObjectPool pool = ConfigurarBulletPool();
        var (player, _) = ConfigurarJugadorCompleto(pos, pool);
        ConfigurarHUDCompleto(player, pool);
        ConfigurarMenuPausaYTimeManager();

        EditorUtility.DisplayDialog("Jugador Configurado al 100%",
            "¡El Jugador 'Player (RI + LG)' ha sido generado / reparado con éxito!\n\n" +
            "• Armas 3D: Rifle/Pistola assault1, Bate de béisbol, Linterna 3D [2/F].\n" +
            "• Cámara y Cabeza sincronizados con RIMovement (giro vertical y horizontal activo).\n" +
            "• Sistema de Disparo (LG_Shoot) conectado con BulletObjectPool y munición inicial.\n" +
            "• Combate Melee (Hand) configurado con SFX y detección por SphereCast.\n" +
            "• HUD Completo: Barras de Salud y Estamina, Munición táctica, Inventario, Retícula y 12 SFX.\n" +
            "• Tag 'Player' asignado correctamente.",
            "Aceptar");
    }

    [MenuItem("Prototipo/Corregir Paredes y Puertas de Escena Activa")]
    public static void CorregirParedesYPuertasEscenaActiva()
    {
        int cambios = 0;

        // 1. Corregir Muros_S2 (Eliminar Muro_Sur sólido duplicado sobre Puerta_Sector_1_2)
        GameObject murosS2 = GameObject.Find("Muros_S2");
        if (murosS2 != null)
        {
            Transform muroSurS2 = murosS2.transform.Find("Muro_Sur");
            if (muroSurS2 != null)
            {
                Undo.DestroyObjectImmediate(muroSurS2.gameObject);
                cambios++;
            }

            Transform dintelS2 = murosS2.transform.Find("Muro_Norte_Dintel");
            if (dintelS2 != null)
            {
                Undo.RecordObject(dintelS2, "Corregir Dintel S2");
                dintelS2.localPosition = new Vector3(0f, 3.9f, 10f);
                dintelS2.localScale = new Vector3(3f, 1.2f, 0.6f);
                cambios++;
            }
        }

        // 2. Corregir Muros_S1 (Dintel)
        GameObject murosS1 = GameObject.Find("Muros_S1");
        if (murosS1 != null)
        {
            Transform dintelS1 = murosS1.transform.Find("Muro_Norte_Dintel");
            if (dintelS1 != null)
            {
                Undo.RecordObject(dintelS1, "Corregir Dintel S1");
                dintelS1.localPosition = new Vector3(0f, 3.9f, -15f);
                dintelS1.localScale = new Vector3(3f, 1.2f, 0.6f);
                cambios++;
            }
        }

        // 3. Corregir Muros_S3 (Eliminar Muro_Sur sólido que tapa Puerta_Sector_2_3, crear aletas y dintel superior)
        GameObject murosS3 = GameObject.Find("Muros_S3");
        if (murosS3 != null)
        {
            Transform muroSurS3 = murosS3.transform.Find("Muro_Sur");
            Material matPared = null;
            if (muroSurS3 != null)
            {
                Renderer mr = muroSurS3.GetComponent<Renderer>();
                if (mr != null) matPared = mr.sharedMaterial;
                Undo.DestroyObjectImmediate(muroSurS3.gameObject);
                cambios++;
            }
            if (matPared == null)
            {
                matPared = AssetDatabase.LoadAssetAtPath<Material>("Assets/LG_Shooting/LGAssets/Materials/Mat_Doom_Wall.mat");
            }

            // Asegurar Muro_Sur_AlaIzq
            Transform alaIzq = murosS3.transform.Find("Muro_Sur_AlaIzq");
            if (alaIzq == null)
            {
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "Muro_Sur_AlaIzq";
                go.transform.SetParent(murosS3.transform);
                go.transform.localPosition = new Vector3(-13.75f, 3f, 10f);
                go.transform.localScale = new Vector3(2.5f, 6f, 0.6f);
                if (matPared != null) go.GetComponent<Renderer>().sharedMaterial = matPared;
                Undo.RegisterCreatedObjectUndo(go, "Crear Ala Izq Muro Sur S3");
                cambios++;
            }

            // Asegurar Muro_Sur_AlaDer
            Transform alaDer = murosS3.transform.Find("Muro_Sur_AlaDer");
            if (alaDer == null)
            {
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "Muro_Sur_AlaDer";
                go.transform.SetParent(murosS3.transform);
                go.transform.localPosition = new Vector3(13.75f, 3f, 10f);
                go.transform.localScale = new Vector3(2.5f, 6f, 0.6f);
                if (matPared != null) go.GetComponent<Renderer>().sharedMaterial = matPared;
                Undo.RegisterCreatedObjectUndo(go, "Crear Ala Der Muro Sur S3");
                cambios++;
            }

            // Asegurar Muro_Sur_DintelSuperior
            Transform dintelSup = murosS3.transform.Find("Muro_Sur_DintelSuperior");
            if (dintelSup == null)
            {
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "Muro_Sur_DintelSuperior";
                go.transform.SetParent(murosS3.transform);
                go.transform.localPosition = new Vector3(0f, 5.25f, 10f);
                go.transform.localScale = new Vector3(25f, 1.5f, 0.6f);
                if (matPared != null) go.GetComponent<Renderer>().sharedMaterial = matPared;
                Undo.RegisterCreatedObjectUndo(go, "Crear Dintel Superior Muro Sur S3");
                cambios++;
            }

            // Corregir Dintel Norte S3
            Transform dintelS3 = murosS3.transform.Find("Muro_Norte_Dintel");
            if (dintelS3 != null)
            {
                Undo.RecordObject(dintelS3, "Corregir Dintel S3");
                dintelS3.localPosition = new Vector3(0f, 4.65f, 40f);
                dintelS3.localScale = new Vector3(3f, 2.7f, 0.6f);
                cambios++;
            }
        }

        // 4. Corregir Puerta_Escape_Final (Centrar en z = 40.0f)
        GameObject escapeDoor = GameObject.Find("Puerta_Escape_Final");
        if (escapeDoor != null)
        {
            Undo.RecordObject(escapeDoor.transform, "Alinear Puerta_Escape_Final");
            escapeDoor.transform.position = new Vector3(0f, 0f, 40.0f);
            cambios++;
        }

        // 5. Corregir Paredes_Arena (Prototipo base / LG_Scene)
        GameObject paredesArena = GameObject.Find("Paredes_Arena");
        if (paredesArena != null)
        {
            float altura = 4.5f;
            float grosor = 0.6f;
            float yPos = altura / 2f;
            float anchoVano = 2.65f;
            float anchoSegmento = (30f - anchoVano) / 2f;
            float offsetCentro = (anchoVano / 2f) + (anchoSegmento / 2f);
            float alturaDintel = altura - 3.325f;
            float yDintel = 3.325f + (alturaDintel / 2f);

            Transform izq = paredesArena.transform.Find("Muro_Norte_Izq");
            if (izq != null)
            {
                Undo.RecordObject(izq, "Ajustar Muro_Norte_Izq");
                izq.localPosition = new Vector3(-offsetCentro, yPos, 15f);
                izq.localScale = new Vector3(anchoSegmento, altura, grosor);
                cambios++;
            }

            Transform der = paredesArena.transform.Find("Muro_Norte_Der");
            if (der != null)
            {
                Undo.RecordObject(der, "Ajustar Muro_Norte_Der");
                der.localPosition = new Vector3(offsetCentro, yPos, 15f);
                der.localScale = new Vector3(anchoSegmento, altura, grosor);
                cambios++;
            }

            Transform dintel = paredesArena.transform.Find("Muro_Norte_Dintel");
            if (dintel != null)
            {
                Undo.RecordObject(dintel, "Ajustar Muro_Norte_Dintel");
                dintel.localPosition = new Vector3(0f, yDintel, 15f);
                dintel.localScale = new Vector3(anchoVano, alturaDintel, grosor);
                cambios++;
            }
        }

        // 6. Corregir Puerta_Salida (Centrar en z = 15.0f)
        GameObject puertaSalida = GameObject.Find("Puerta_Salida");
        if (puertaSalida != null)
        {
            Undo.RecordObject(puertaSalida.transform, "Alinear Puerta_Salida");
            puertaSalida.transform.position = new Vector3(0f, 0f, 15.0f);
            BoxCollider col = puertaSalida.GetComponent<BoxCollider>();
            if (col != null)
            {
                Undo.RecordObject(col, "Ajustar Trigger Puerta_Salida");
                col.center = new Vector3(0f, 1.6f, 0f);
                col.size = new Vector3(3.2f, 3.5f, 3f);
            }
            cambios++;
        }

        // 7. Asegurar travesaño superior (Frame_Sup) en compuertas DOOM
        string[] doomDoors = new string[] { "Puerta_Sector_1_2", "Puerta_Sector_2_3", "Puerta_Escape_Final" };
        Material matMarco = AssetDatabase.LoadAssetAtPath<Material>("Assets/LG_Shooting/LGAssets/Materials/Mat_Doom_DoorFrame.mat");
        foreach (string dName in doomDoors)
        {
            GameObject dGO = GameObject.Find(dName);
            if (dGO != null)
            {
                Transform frameIzq = dGO.transform.Find("Frame_Izq");
                if (frameIzq != null)
                {
                    Undo.RecordObject(frameIzq, "Ajustar Frame_Izq");
                    frameIzq.localPosition = new Vector3(-1.35f, 1.6f, 0f);
                    frameIzq.localScale = new Vector3(0.3f, 3.2f, 0.5f);
                }

                Transform frameDer = dGO.transform.Find("Frame_Der");
                if (frameDer != null)
                {
                    Undo.RecordObject(frameDer, "Ajustar Frame_Der");
                    frameDer.localPosition = new Vector3(1.35f, 1.6f, 0f);
                    frameDer.localScale = new Vector3(0.3f, 3.2f, 0.5f);
                }

                Transform frameSup = dGO.transform.Find("Frame_Sup");
                if (frameSup == null)
                {
                    GameObject supGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    supGO.name = "Frame_Sup";
                    supGO.transform.SetParent(dGO.transform);
                    supGO.transform.localPosition = new Vector3(0f, 3.2f, 0f);
                    supGO.transform.localScale = new Vector3(3.0f, 0.2f, 0.5f);
                    if (matMarco != null) supGO.GetComponent<Renderer>().sharedMaterial = matMarco;
                    Undo.RegisterCreatedObjectUndo(supGO, "Crear Frame_Sup");
                    cambios++;
                }
                else
                {
                    Undo.RecordObject(frameSup, "Ajustar Frame_Sup");
                    frameSup.localPosition = new Vector3(0f, 3.2f, 0f);
                    frameSup.localScale = new Vector3(3.0f, 0.2f, 0.5f);
                }

                Transform statusLight = dGO.transform.Find("Status_Light");
                if (statusLight != null)
                {
                    Undo.RecordObject(statusLight, "Ajustar Status_Light");
                    statusLight.localPosition = new Vector3(0f, 3.2f, -0.26f);
                    statusLight.localScale = Vector3.one * 0.35f;
                }
            }
        }

        // 8. Asegurar TimeManager y Menú de Pausa (Canvas.prefab con VolumeSliders y Reanudar)
        ConfigurarMenuPausaYTimeManager();
        cambios++;

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Paredes y Puertas Corregidas",
            $"¡Geometría de paredes y puertas corregida con éxito!\n\n" +
            $"• Muros sur sólidos duplicados eliminados (paso despejado entre sectores).\n" +
            $"• Dinteles superiores elevados a 3.3m (sin cortes ni solapamientos).\n" +
            $"• Puertas centradas en el eje Z (Puerta_Escape_Final a 40.0m, Puerta_Salida a 15.0m).\n" +
            $"• Marcos y vanos alineados.\n\n" +
            $"Total de modificaciones registradas: {cambios}.",
            "Aceptar");
    }

    /// <summary>
    /// Crea y asegura el Prefab y el Object Pool de Balas en la escena.
    /// </summary>
    public static LG_ObjectPool ConfigurarBulletPool()
    {
        GameObject balaPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BULLET_PREFAB_PATH);
        Material matBalas = AssetDatabase.LoadAssetAtPath<Material>("Assets/LG_Shooting/LGAssets/Materials/Mat_Balas.mat");

        if (balaPrefab == null)
        {
            GameObject tempBala = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tempBala.name = "BalaPrototipo";
            tempBala.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

            LG_Bullet bulletComp = tempBala.AddComponent<LG_Bullet>();
            Rigidbody rb = tempBala.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            if (matBalas != null)
            {
                tempBala.GetComponent<Renderer>().sharedMaterial = matBalas;
            }

            Collider col = tempBala.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            if (!AssetDatabase.IsValidFolder("Assets/LG_Shooting/LGAssets"))
            {
                AssetDatabase.CreateFolder("Assets/LG_Shooting", "LGAssets");
            }

            balaPrefab = PrefabUtility.SaveAsPrefabAsset(tempBala, BULLET_PREFAB_PATH);
            Object.DestroyImmediate(tempBala);
        }

        GameObject poolGO = GameObject.Find("BulletObjectPool");
        if (poolGO == null)
        {
            poolGO = new GameObject("BulletObjectPool");
        }
        poolGO.transform.position = Vector3.zero;

        LG_ObjectPool poolComp = poolGO.GetComponent<LG_ObjectPool>();
        if (poolComp == null) poolComp = poolGO.AddComponent<LG_ObjectPool>();

        SerializedObject poolSO = new SerializedObject(poolComp);
        poolSO.FindProperty("prefab").objectReferenceValue = balaPrefab;
        poolSO.FindProperty("initialSize").intValue = 30;
        poolSO.FindProperty("canGrow").boolValue = true;
        poolSO.ApplyModifiedProperties();

        Undo.RegisterCreatedObjectUndo(poolGO, "Configurar BulletObjectPool");
        return poolComp;
    }

    /// <summary>
    /// Configura completamente al Jugador (Player RI + LG) con todas sus armas 3D, componentes y referencias.
    /// </summary>
    public static (GameObject player, LG_ObjectPool pool) ConfigurarJugadorCompleto(Vector3 spawnPos, LG_ObjectPool poolComp = null)
    {
        if (poolComp == null)
        {
            poolComp = ConfigurarBulletPool();
        }

        // Clips de Audio SFX
        AudioClip sfxShoot = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AUDIO_DIR}/SFX_Shoot.wav");
        AudioClip sfxReload = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AUDIO_DIR}/SFX_Reload.wav");
        AudioClip sfxEmpty = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AUDIO_DIR}/SFX_Empty.wav");
        AudioClip sfxSwing = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AUDIO_DIR}/SFX_Melee_Swing.wav");
        AudioClip sfxHit = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AUDIO_DIR}/SFX_Melee_Hit.wav");
        AudioClip sfxPlayerHurt = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AUDIO_DIR}/SFX_Player_Hurt.wav");

        // 1. GameObject Raíz del Jugador
        GameObject player = GameObject.Find("Player (RI + LG)");
        if (player == null)
        {
            player = new GameObject("Player (RI + LG)");
        }
        player.name = "Player (RI + LG)";
        player.tag = "Player";
        player.transform.position = spawnPos;
        player.transform.rotation = Quaternion.identity;

        // 2. CharacterController
        CharacterController charCtrl = player.GetComponent<CharacterController>();
        if (charCtrl == null) charCtrl = player.AddComponent<CharacterController>();
        charCtrl.height = 1.8f;
        charCtrl.radius = 0.5f;
        charCtrl.center = new Vector3(0f, 0.9f, 0f);
        charCtrl.stepOffset = 0.3f;
        charCtrl.slopeLimit = 45f;
        charCtrl.minMoveDistance = 0f;

        // 3. PlayerInput
        PlayerInput playerInput = player.GetComponent<PlayerInput>();
        if (playerInput == null) playerInput = player.AddComponent<PlayerInput>();
        InputActionAsset inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(INPUT_ACTIONS_PATH);
        if (inputActions != null)
        {
            playerInput.actions = inputActions;
            playerInput.defaultControlScheme = "Keyboard&Mouse";
        }

        // 4. Cabeza
        Transform cabeza = player.transform.Find("Cabeza");
        if (cabeza == null)
        {
            GameObject cabezaGO = new GameObject("Cabeza");
            cabeza = cabezaGO.transform;
            cabeza.SetParent(player.transform);
        }
        cabeza.localPosition = new Vector3(0f, 1.6f, 0f);
        cabeza.localRotation = Quaternion.identity;

        // 5. Cámara Principal
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject camGO = GameObject.Find("Main Camera");
            if (camGO != null) mainCamera = camGO.GetComponent<Camera>();
            if (mainCamera == null) mainCamera = Object.FindFirstObjectByType<Camera>();
            if (mainCamera == null)
            {
                camGO = new GameObject("Main Camera");
                mainCamera = camGO.AddComponent<Camera>();
            }
        }

        mainCamera.tag = "MainCamera";
        mainCamera.transform.SetParent(cabeza);
        mainCamera.transform.localPosition = Vector3.zero;
        mainCamera.transform.localRotation = Quaternion.identity;

        if (mainCamera.GetComponent<AudioListener>() == null)
        {
            mainCamera.gameObject.AddComponent<AudioListener>();
        }

        UniversalAdditionalCameraData camData = mainCamera.GetComponent<UniversalAdditionalCameraData>();
        if (camData == null) camData = mainCamera.gameObject.AddComponent<UniversalAdditionalCameraData>();

        playerInput.camera = mainCamera;

        // 6. FirePoint para el cañón del arma
        Transform firePoint = cabeza.Find("FirePoint");
        if (firePoint == null)
        {
            GameObject fpGO = new GameObject("FirePoint");
            firePoint = fpGO.transform;
            firePoint.SetParent(cabeza);
        }
        firePoint.localPosition = new Vector3(0.197f, -0.2f, 1.284f);
        firePoint.localRotation = Quaternion.identity;

        // 7. Armas 3D bajo Cabeza (assault1, FlashLight, Bat)
        GameObject gunAsset = AssetDatabase.LoadAssetAtPath<GameObject>(GUN_PATH);
        GameObject flashLightAsset = AssetDatabase.LoadAssetAtPath<GameObject>(FLASHLIGHT_PREFAB_PATH);
        GameObject batAsset = AssetDatabase.LoadAssetAtPath<GameObject>(BAT_PREFAB_PATH);

        // --- Arma 1: assault1 (Rifle / Pistola) ---
        Transform gunT = cabeza.Find("assault1");
        GameObject gunGO = null;
        if (gunT == null && gunAsset != null)
        {
            gunGO = (GameObject)PrefabUtility.InstantiatePrefab(gunAsset, cabeza);
            gunGO.name = "assault1";
        }
        else if (gunT != null)
        {
            gunGO = gunT.gameObject;
        }

        if (gunGO != null)
        {
            gunGO.transform.localPosition = new Vector3(0.428f, -0.54f, 0.46f);
            gunGO.transform.localRotation = Quaternion.identity;
            gunGO.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            Material gunMat = AssetDatabase.LoadAssetAtPath<Material>(GUN_MAT_PATH);
            if (gunMat != null)
            {
                Renderer[] rends = gunGO.GetComponentsInChildren<Renderer>(true);
                foreach (var r in rends)
                {
                    r.sharedMaterial = gunMat;
                }
            }
            gunGO.SetActive(true);
        }

        // --- Arma 2: FlashLight (Linterna 3D) ---
        Transform flashT = cabeza.Find("FlashLight");
        GameObject flashGO = null;
        if (flashT == null && flashLightAsset != null)
        {
            flashGO = (GameObject)PrefabUtility.InstantiatePrefab(flashLightAsset, cabeza);
            flashGO.name = "FlashLight";
        }
        else if (flashT != null)
        {
            // Si existía un objeto simple sin mesh, reemplazarlo con el prefab 3D
            if (flashT.GetComponentInChildren<MeshFilter>() == null && flashLightAsset != null)
            {
                Object.DestroyImmediate(flashT.gameObject);
                flashGO = (GameObject)PrefabUtility.InstantiatePrefab(flashLightAsset, cabeza);
                flashGO.name = "FlashLight";
            }
            else
            {
                flashGO = flashT.gameObject;
            }
        }

        if (flashGO != null)
        {
            flashGO.transform.localPosition = new Vector3(0.41f, -0.27f, 0.79f);
            flashGO.transform.localRotation = Quaternion.identity;
            flashGO.transform.localScale = new Vector3(4f, 4f, 4f);

            Light spot = flashGO.GetComponentInChildren<Light>(true);
            if (spot != null)
            {
                spot.type = LightType.Spot;
                spot.range = 25f;
                spot.intensity = 2.2f;
                spot.spotAngle = 62.36f;
                spot.innerSpotAngle = 54.16f;
                UniversalAdditionalLightData spotData = spot.GetComponent<UniversalAdditionalLightData>();
                if (spotData == null) spot.gameObject.AddComponent<UniversalAdditionalLightData>();
            }
            flashGO.SetActive(false);
        }

        // --- Arma 3: Bat (Bate de béisbol) ---
        Transform batT = cabeza.Find("Bat");
        GameObject batGO = null;
        if (batT == null && batAsset != null)
        {
            batGO = (GameObject)PrefabUtility.InstantiatePrefab(batAsset, cabeza);
            batGO.name = "Bat";
        }
        else if (batT != null)
        {
            batGO = batT.gameObject;
        }

        if (batGO != null)
        {
            batGO.transform.localPosition = new Vector3(0.44f, -1.18f, 0.87f);
            batGO.transform.localRotation = Quaternion.Euler(27.63f, 0f, 0f);
            batGO.transform.localScale = new Vector3(2f, 2f, 2f);
            batGO.SetActive(false);
        }

        // 8. RIMovement (Movimiento y Mouse Look)
        RIMovement movement = player.GetComponent<RIMovement>();
        if (movement == null) movement = player.AddComponent<RIMovement>();
        movement.cabeza = cabeza; // CRÍTICO: Sin esto el giro vertical del ratón no funciona
        movement.caminar = 4f;
        movement.correr = 7f;
        movement.agachado = 2f;
        movement.aceleracion = 14f;
        movement.gravedad = -22f;
        movement.estaminaMax = 5f;
        movement.recuperacion = 1.5f;
        movement.sensibilidadX = 0.15f;
        movement.sensibilidadY = 0.15f;
        movement.limiteVertical = 80f;

        // 9. LG_Inventory (Inventario de Ítems)
        LG_Inventory inventory = player.GetComponent<LG_Inventory>();
        if (inventory == null) inventory = player.AddComponent<LG_Inventory>();
        if (inventory.GetItemCount("Munición") == 0)
        {
            inventory.AddItem("Munición", 30);
        }
        if (inventory.GetItemCount("Vendas") == 0)
        {
            inventory.AddItem("Vendas", 1);
        }

        // 10. LG_Shoot (Sistema de Disparo)
        LG_Shoot shoot = player.GetComponent<LG_Shoot>();
        if (shoot == null) shoot = player.AddComponent<LG_Shoot>();

        SerializedObject shootSO = new SerializedObject(shoot);
        shootSO.FindProperty("bulletPool").objectReferenceValue = poolComp; // Nombre de campo correcto
        shootSO.FindProperty("firePoint").objectReferenceValue = firePoint;
        shootSO.FindProperty("playerInventory").objectReferenceValue = inventory;
        shootSO.FindProperty("fireRate").floatValue = 0.2f;
        shootSO.FindProperty("maxClipSize").intValue = 12;
        shootSO.FindProperty("currentClip").intValue = 12;
        shootSO.FindProperty("ammoItemName").stringValue = "Munición";
        if (inputActions != null)
        {
            var shootAct = inputActions.FindAction("Player/Shoot") ?? inputActions.FindAction("Shoot") ?? inputActions.FindAction("Attack");
            if (shootAct != null)
            {
                shootSO.FindProperty("shootAction").objectReferenceValue = InputActionReference.Create(shootAct);
            }
        }
        SetPropertyRef(shootSO, "shootSound", sfxShoot);
        SetPropertyRef(shootSO, "reloadSound", sfxReload);
        SetPropertyRef(shootSO, "emptySound", sfxEmpty);
        shootSO.ApplyModifiedProperties();

        // 11. LG_PlayerHealth (Salud del Jugador)
        LG_PlayerHealth playerHealth = player.GetComponent<LG_PlayerHealth>();
        if (playerHealth == null) playerHealth = player.AddComponent<LG_PlayerHealth>();
        SerializedObject healthSO = new SerializedObject(playerHealth);
        healthSO.FindProperty("maxHealth").floatValue = 100f;
        SetPropertyRef(healthSO, "hurtSound", sfxPlayerHurt);
        healthSO.ApplyModifiedProperties();

        // 12. Hand (Gestor de Armas y Ataque Melee)
        Hand handComp = player.GetComponent<Hand>();
        if (handComp == null) handComp = player.AddComponent<Hand>();
        SerializedObject handSO = new SerializedObject(handComp);
        handSO.FindProperty("gun").objectReferenceValue = gunGO;
        handSO.FindProperty("flashLight").objectReferenceValue = flashGO;
        handSO.FindProperty("sword").objectReferenceValue = batGO;
        handSO.FindProperty("shootScript").objectReferenceValue = shoot;
        handSO.FindProperty("attackDamage").floatValue = 2f;
        handSO.FindProperty("attackRange").floatValue = 2.5f;
        handSO.FindProperty("attackRate").floatValue = 0.45f;
        handSO.FindProperty("hitForce").floatValue = 12f;
        SetPropertyRef(handSO, "playerCamera", mainCamera);
        SetPropertyRef(handSO, "swingSound", sfxSwing);
        SetPropertyRef(handSO, "hitSound", sfxHit);
        handSO.ApplyModifiedProperties();

        Undo.RegisterCreatedObjectUndo(player, "Configurar Player Completo");
        return (player, poolComp);
    }

    /// <summary>
    /// Configura completamente el HUD del Jugador (Vida, Estamina, Inventario, Munición, Retícula, Tooltips, SoundList).
    /// </summary>
    public static GameObject ConfigurarHUDCompleto(GameObject playerGO, LG_ObjectPool poolComp, LG_DialogueManager dialogueMgr = null)
    {
        if (playerGO == null) playerGO = GameObject.Find("Player (RI + LG)");

        RIMovement movement = playerGO != null ? playerGO.GetComponent<RIMovement>() : null;
        LG_Inventory inventory = playerGO != null ? playerGO.GetComponent<LG_Inventory>() : null;
        LG_PlayerHealth playerHealth = playerGO != null ? playerGO.GetComponent<LG_PlayerHealth>() : null;
        LG_Shoot shoot = playerGO != null ? playerGO.GetComponent<LG_Shoot>() : null;

        // 1. Canvas HUD
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

        // Limpiar elementos de HUD antiguos si se recrean
        Transform oldHealthSlider = canvasGO.transform.Find("HealthSlider");
        if (oldHealthSlider != null) Object.DestroyImmediate(oldHealthSlider.gameObject);
        Transform oldSlider = canvasGO.transform.Find("StaminaSlider");
        if (oldSlider != null) Object.DestroyImmediate(oldSlider.gameObject);
        Transform oldText = canvasGO.transform.Find("InventoryText");
        if (oldText != null) Object.DestroyImmediate(oldText.gameObject);
        Transform oldCrosshair = canvasGO.transform.Find("Crosshair");
        if (oldCrosshair != null) Object.DestroyImmediate(oldCrosshair.gameObject);

        // 2. Elementos UI del Jugador
        Slider healthSlider = CrearSliderVida(canvasGO.transform);
        Slider staminaSlider = CrearSliderStamina(canvasGO.transform);
        Text inventoryText = CrearTextoInventario(canvasGO.transform);
        TextMeshProUGUI ammoText = ConfigurarHUDMunicion(canvasGO.transform);
        CrearCrosshair(canvasGO.transform);

        // 3. Conectar ammoText con LG_Shoot
        if (shoot != null && ammoText != null)
        {
            SerializedObject shootSO = new SerializedObject(shoot);
            shootSO.FindProperty("ammoText").objectReferenceValue = ammoText;
            shootSO.ApplyModifiedProperties();
        }

        // 4. LG_HUD
        LG_HUD hudComp = canvasGO.GetComponent<LG_HUD>();
        if (hudComp == null) hudComp = canvasGO.AddComponent<LG_HUD>();

        SerializedObject hudSO = new SerializedObject(hudComp);
        if (movement != null) hudSO.FindProperty("playerMovement").objectReferenceValue = movement;
        if (inventory != null) hudSO.FindProperty("playerInventory").objectReferenceValue = inventory;
        if (playerHealth != null) hudSO.FindProperty("playerHealth").objectReferenceValue = playerHealth;
        hudSO.FindProperty("staminaSlider").objectReferenceValue = staminaSlider;
        hudSO.FindProperty("healthSlider").objectReferenceValue = healthSlider;
        hudSO.FindProperty("inventoryText").objectReferenceValue = inventoryText;
        hudSO.ApplyModifiedProperties();

        // 5. SoundList
        SoundList soundListComp = canvasGO.GetComponent<SoundList>();
        if (soundListComp == null) soundListComp = canvasGO.AddComponent<SoundList>();
        ConfigurarSoundList(soundListComp);

        // 6. LG_TooltipManager
        LG_TooltipManager tooltipMgr = canvasGO.GetComponent<LG_TooltipManager>();
        if (tooltipMgr == null) tooltipMgr = canvasGO.AddComponent<LG_TooltipManager>();
        TextMeshProUGUI tooltipTextTMP = ConfigurarTooltipText(canvasGO.transform);
        SerializedObject tooltipSO = new SerializedObject(tooltipMgr);
        tooltipSO.FindProperty("tooltipText").objectReferenceValue = tooltipTextTMP;
        tooltipSO.ApplyModifiedProperties();

        // 7. DialogueUI (si existe DialogueManager)
        if (dialogueMgr == null) dialogueMgr = Object.FindFirstObjectByType<LG_DialogueManager>();
        if (dialogueMgr != null)
        {
            ConfigurarDialogueUI(canvasGO.transform, dialogueMgr);
        }

        // 8. EventSystem
        if (UnityEngine.EventSystems.EventSystem.current == null)
        {
            GameObject esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            Undo.RegisterCreatedObjectUndo(esGO, "Crear EventSystem");
        }

        // 9. Configurar Menú de Pausa (TimeManager + Canvas.prefab con PauseScreen)
        ConfigurarMenuPausaYTimeManager();

        Undo.RegisterCreatedObjectUndo(canvasGO, "Configurar HUD_Canvas Completo");
        return canvasGO;
    }

    public static Slider CrearSliderVida(Transform parent)
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

    public static Slider CrearSliderStamina(Transform parent)
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

    public static Text CrearTextoInventario(Transform parent)
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

    public static TextMeshProUGUI ConfigurarHUDMunicion(Transform canvasTransform)
    {
        for (int i = canvasTransform.childCount - 1; i >= 0; i--)
        {
            Transform child = canvasTransform.GetChild(i);
            if (child.name == "MunicionText" || child.name == "AmmoText")
            {
                Object.DestroyImmediate(child.gameObject);
            }
        }

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

        TextMeshProUGUI tmp = textGO.GetComponent<TextMeshProUGUI>();
        if (tmp == null) tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 36;
        tmp.richText = true;
        tmp.text = "<b><size=38><color=#FFFFFF>12</color></size></b><size=20><color=#64748B> / </color><color=#CBD5E1>30</color></size>\n<size=11><color=#38BDF8><b>PISTOLA 9MM</b></color></size>";

        return tmp;
    }

    public static void CrearCrosshair(Transform parent)
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

    public static void ConfigurarSoundList(SoundList soundListComp)
    {
        AudioMixer mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MIXER_PATH);
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
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AUDIO_DIR}/{soundDefs[i].file}");
            elem.FindPropertyRelative("clip").objectReferenceValue = clip;
            elem.FindPropertyRelative("volume").floatValue = soundDefs[i].vol;
            elem.FindPropertyRelative("pitch").floatValue = soundDefs[i].pitch;
            elem.FindPropertyRelative("loop").boolValue = false;
            elem.FindPropertyRelative("mixer").objectReferenceValue = sfxMixerGroup;
        }

        slSO.ApplyModifiedProperties();
    }

    public static TextMeshProUGUI ConfigurarTooltipText(Transform canvasTransform)
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

        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        if (tmp == null) tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 24;
        tmp.color = Color.white;
        tmp.text = "";
        go.SetActive(false);

        return tmp;
    }

    public static void ConfigurarDialogueUI(Transform canvasTransform, LG_DialogueManager dialogueMgr)
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

        Transform textT = panelGO.transform.Find("DialogueText");
        GameObject textGO = textT != null ? textT.gameObject : new GameObject("DialogueText");
        textGO.name = "DialogueText";
        textGO.transform.SetParent(panelGO.transform, false);

        RectTransform textRect = textGO.GetComponent<RectTransform>();
        if (textRect == null) textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = new Vector2(0f, 60f);
        textRect.sizeDelta = new Vector2(900f, 140f);

        TextMeshProUGUI dialogueTMP = textGO.GetComponent<TextMeshProUGUI>();
        if (dialogueTMP == null) dialogueTMP = textGO.AddComponent<TextMeshProUGUI>();
        dialogueTMP.alignment = TextAlignmentOptions.Center;
        dialogueTMP.fontSize = 28;
        dialogueTMP.color = Color.white;
        dialogueTMP.richText = true;
        dialogueTMP.text = "...";

        Transform optT = panelGO.transform.Find("OptionsContainer");
        GameObject optGO = optT != null ? optT.gameObject : new GameObject("OptionsContainer");
        optGO.name = "OptionsContainer";
        optGO.transform.SetParent(panelGO.transform, false);

        RectTransform optRect = optGO.GetComponent<RectTransform>();
        if (optRect == null) optRect = optGO.AddComponent<RectTransform>();
        optRect.anchorMin = new Vector2(0.5f, 0.5f);
        optRect.anchorMax = new Vector2(0.5f, 0.5f);
        optRect.pivot = new Vector2(0.5f, 0.5f);
        optRect.anchoredPosition = new Vector2(0f, -80f);
        optRect.sizeDelta = new Vector2(500f, 100f);

        Button truthBtn = CrearBotonDialogo(optGO.transform, "TruthButton", "Decir la Verdad", new Color(0.2f, 0.7f, 0.3f));
        Button lieBtn = CrearBotonDialogo(optGO.transform, "LieButton", "Mentir", new Color(0.8f, 0.25f, 0.25f));

        RectTransform tbRect = truthBtn.GetComponent<RectTransform>();
        tbRect.anchoredPosition = new Vector2(-120f, 0f);
        tbRect.sizeDelta = new Vector2(200f, 50f);

        RectTransform lbRect = lieBtn.GetComponent<RectTransform>();
        lbRect.anchoredPosition = new Vector2(120f, 0f);
        lbRect.sizeDelta = new Vector2(200f, 50f);

        panelGO.SetActive(false);
        optGO.SetActive(false);

        if (dialogueMgr != null)
        {
            SerializedObject dmSO = new SerializedObject(dialogueMgr);
            dmSO.FindProperty("dialoguePanel").objectReferenceValue = panelGO;
            dmSO.FindProperty("dialogueText").objectReferenceValue = dialogueTMP;
            dmSO.FindProperty("optionsContainer").objectReferenceValue = optGO;
            dmSO.FindProperty("truthButton").objectReferenceValue = truthBtn;
            dmSO.FindProperty("lieButton").objectReferenceValue = lieBtn;
            dmSO.ApplyModifiedProperties();
        }
    }

    private static Button CrearBotonDialogo(Transform parent, string goName, string buttonLabel, Color normalColor)
    {
        Transform oldT = parent.Find(goName);
        GameObject btnGO = oldT != null ? oldT.gameObject : new GameObject(goName);
        btnGO.name = goName;
        btnGO.transform.SetParent(parent, false);

        RectTransform rect = btnGO.GetComponent<RectTransform>();
        if (rect == null) rect = btnGO.AddComponent<RectTransform>();

        Image img = btnGO.GetComponent<Image>();
        if (img == null) img = btnGO.AddComponent<Image>();
        img.color = normalColor;

        Button btn = btnGO.GetComponent<Button>();
        if (btn == null) btn = btnGO.AddComponent<Button>();

        ColorBlock cb = btn.colors;
        cb.normalColor = normalColor;
        cb.highlightedColor = normalColor * 1.25f;
        cb.pressedColor = normalColor * 0.75f;
        btn.colors = cb;

        Transform oldText = btnGO.transform.Find("Text");
        GameObject tGO = oldText != null ? oldText.gameObject : new GameObject("Text");
        tGO.name = "Text";
        tGO.transform.SetParent(btnGO.transform, false);

        RectTransform tRect = tGO.GetComponent<RectTransform>();
        if (tRect == null) tRect = tGO.AddComponent<RectTransform>();
        tRect.anchorMin = Vector2.zero;
        tRect.anchorMax = Vector2.one;
        tRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = tGO.GetComponent<TextMeshProUGUI>();
        if (tmp == null) tmp = tGO.AddComponent<TextMeshProUGUI>();
        tmp.text = buttonLabel;
        tmp.fontSize = 20;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        return btn;
    }

    private static void SetPropertyRef(SerializedObject so, string propName, Object value)
    {
        SerializedProperty prop = so.FindProperty(propName);
        if (prop != null)
        {
            prop.objectReferenceValue = value;
        }
    }

    [MenuItem("Prototipo/Configurar Solo Menu de Pausa y Ajustes")]
    public static void MenuConfigurarPausa()
    {
        ConfigurarMenuPausaYTimeManager();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("Menú de Pausa y Ajustes",
            "¡El Menú de Pausa y Ajustes de Audio (TimeManager + Canvas.prefab) ha sido configurado con éxito en la escena activa!\n\n" +
            "• Presiona [Escape] para pausar/despausar en cualquier momento.\n" +
            "• Controles de volumen: Master, Música y SFX vinculados al mezclador.\n" +
            "• Botón de Reanudar funcional.\n" +
            "• Bloqueo de cámara y disparo mientras está en pausa.",
            "Aceptar");
    }

    /// <summary>
    /// Configura TimeManager y el Canvas de Pausa con controles de audio (Canvas.prefab)
    /// </summary>
    public static GameObject ConfigurarMenuPausaYTimeManager()
    {
        // 0. EventSystem para poder interactuar con botones y sliders
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            var inputModule = esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            inputModule.AssignDefaultActions();
            Undo.RegisterCreatedObjectUndo(esGO, "Crear EventSystem");
        }

        // 1. TimeManager
        GameObject timeMgrGO = GameObject.Find("TimeManager");
        if (timeMgrGO == null) timeMgrGO = new GameObject("TimeManager");
        timeMgrGO.transform.position = Vector3.zero;
        TimeManager timeMgr = timeMgrGO.GetComponent<TimeManager>();
        if (timeMgr == null) timeMgr = timeMgrGO.AddComponent<TimeManager>();
        Undo.RegisterCreatedObjectUndo(timeMgrGO, "Crear TimeManager");

        // 2. Canvas.prefab (PauseScreen con VolumeSliders y botón Reanudar)
        string canvasPrefabPath = "Assets/AExport/Canvas.prefab";
        GameObject canvasPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(canvasPrefabPath);
        GameObject settingsCanvasGO = GameObject.Find("Canvas");
        if (settingsCanvasGO == null && canvasPrefab != null)
        {
            settingsCanvasGO = (GameObject)PrefabUtility.InstantiatePrefab(canvasPrefab);
            settingsCanvasGO.name = "Canvas";
            Undo.RegisterCreatedObjectUndo(settingsCanvasGO, "Instanciar Canvas Settings & Pause");
        }

        if (settingsCanvasGO != null)
        {
            // Eliminar Fader y elementos residuales del 2D Platformer que oscurecen la pantalla
            string[] residuales = new string[] { "Fader", "CanvasGroup_LifePoints", "CoinText", "IconImage" };
            foreach (string n in residuales)
            {
                Transform t = settingsCanvasGO.transform.Find(n);
                if (t != null) Object.DestroyImmediate(t.gameObject);
            }

            Transform pauseScreenT = settingsCanvasGO.transform.Find("PauseScreen");
            CanvasGroup pauseScreenCG = pauseScreenT != null ? pauseScreenT.GetComponent<CanvasGroup>() : settingsCanvasGO.GetComponentInChildren<CanvasGroup>(true);
            if (pauseScreenCG != null && timeMgr != null)
            {
                SerializedObject tmSO = new SerializedObject(timeMgr);
                tmSO.FindProperty("pauseScreen").objectReferenceValue = pauseScreenCG;
                tmSO.FindProperty("pauseKey").intValue = (int)KeyCode.Escape;
                var altKeyProp = tmSO.FindProperty("alternatePauseKey");
                if (altKeyProp != null) altKeyProp.intValue = (int)KeyCode.P;
                tmSO.FindProperty("pauseTweenTime").floatValue = 0.25f;
                tmSO.ApplyModifiedProperties();
            }

            Button resumeBtn = pauseScreenT != null ? pauseScreenT.GetComponentInChildren<Button>(true) : settingsCanvasGO.GetComponentInChildren<Button>(true);
            if (resumeBtn != null && timeMgr != null)
            {
                UnityEditor.Events.UnityEventTools.RemovePersistentListener(resumeBtn.onClick, 0);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(resumeBtn.onClick, timeMgr.ResumeGame);
            }
        }

        return settingsCanvasGO;
    }
}
