using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PrototypeSetupWindow : EditorWindow
{
    [MenuItem("Prototipo/Configurar Escena Jugable")]
    public static void ShowWindow()
    {
        GetWindow<PrototypeSetupWindow>("Configurador de Prototipo");
    }

    private void OnGUI()
    {
        GUILayout.Label("Configurador de Prototipo Jugable (URP)", EditorStyles.boldLabel);
        GUILayout.Space(10);
        GUILayout.Label("Esta herramienta preparará la escena actual con:\n" +
                      "- Un jugador controlado con RIMovement y LG_Shoot.\n" +
                      "- Sistema de Inventario (LG_Inventory) y HUD (Canvas UI) con materiales URP.\n" +
                      "- Rotación de cámara (Mouse Look) configurada.\n" +
                      "- Objetos coleccionables flotantes interactivos (\"Municion\", \"Bateria\", \"Llave Roja\").\n" +
                      "- Un pool de balas (LG_ObjectPool) y balas con físicas de impacto real.\n" +
                      "- Un escenario básico de pruebas (suelo y objetivos físicos de URP).\n\n" +
                      "Asegúrate de tener guardada tu escena actual antes de proceder.", EditorStyles.wordWrappedLabel);
        
        GUILayout.Space(20);

        if (GUILayout.Button("Generar Prototipo Completo", GUILayout.Height(40)))
        {
            GenerarPrototipo();
        }
    }

    private static void GenerarPrototipo()
    {
        // 1. Crear Materiales URP Básicos para una mejor estética
        Material materialSuelo = ObtenerOCrearMaterialURP("Assets/LG_Shooting/LGAssets/Materials/Mat_Suelo.mat", new Color(0.15f, 0.15f, 0.18f));
        Material materialObjetivos = ObtenerOCrearMaterialURP("Assets/LG_Shooting/LGAssets/Materials/Mat_Objetivos.mat", new Color(0.85f, 0.25f, 0.25f));
        Material materialBalas = ObtenerOCrearMaterialURP("Assets/LG_Shooting/LGAssets/Materials/Mat_Balas.mat", new Color(1f, 0.75f, 0f));
        
        // Materiales para Coleccionables
        Material materialAmmo = ObtenerOCrearMaterialURP("Assets/LG_Shooting/LGAssets/Materials/Mat_Collectible_Ammo.mat", new Color(0.2f, 0.8f, 0.2f));
        Material materialBattery = ObtenerOCrearMaterialURP("Assets/LG_Shooting/LGAssets/Materials/Mat_Collectible_Battery.mat", new Color(0.2f, 0.6f, 0.9f));
        Material materialKey = ObtenerOCrearMaterialURP("Assets/LG_Shooting/LGAssets/Materials/Mat_Collectible_Key.mat", new Color(0.9f, 0.2f, 0.8f));

        // 2. Configurar el Suelo de la escena
        GameObject suelo = GameObject.Find("Suelo");
        if (suelo == null)
        {
            suelo = GameObject.CreatePrimitive(PrimitiveType.Plane);
            suelo.name = "Suelo";
        }
        suelo.transform.position = Vector3.zero;
        suelo.transform.localScale = new Vector3(5f, 1f, 5f); // 50x50 unidades
        suelo.GetComponent<Renderer>().sharedMaterial = materialSuelo;
        Undo.RegisterCreatedObjectUndo(suelo, "Crear Suelo");

        // 3. Crear el Prefab de la Bala si no existe
        string prefabPath = "Assets/LG_Shooting/LGAssets/BalaPrototipo.prefab";
        GameObject balaPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (balaPrefab == null)
        {
            GameObject tempBala = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tempBala.name = "BalaPrototipo";
            tempBala.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            
            // Añadir el componente LG_Bullet
            LG_Bullet bulletComp = tempBala.AddComponent<LG_Bullet>();
            
            // Añadir un Rigidbody para interacciones físicas
            Rigidbody rb = tempBala.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            // Ponerle el material brillante
            tempBala.GetComponent<Renderer>().sharedMaterial = materialBalas;

            // Asegurarnos de que el colisionador esté configurado como Trigger para evitar empujar al jugador
            Collider col = tempBala.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            // Crear la carpeta LGAssets si no existe
            if (!AssetDatabase.IsValidFolder("Assets/LG_Shooting/LGAssets"))
            {
                AssetDatabase.CreateFolder("Assets/LG_Shooting", "LGAssets");
            }

            // Guardar prefab
            balaPrefab = PrefabUtility.SaveAsPrefabAsset(tempBala, prefabPath);
            DestroyImmediate(tempBala);
        }
        else
        {
            // Actualizar material por si acaso
            Renderer r = balaPrefab.GetComponent<Renderer>();
            if (r != null && r.sharedMaterial != materialBalas)
            {
                r.sharedMaterial = materialBalas;
                EditorUtility.SetDirty(balaPrefab);
            }
        }

        // 4. Crear el Object Pool de Balas en la escena
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

        // Asignar el prefab al Object Pool usando SerializedObject
        SerializedObject poolSO = new SerializedObject(poolComp);
        poolSO.FindProperty("prefab").objectReferenceValue = balaPrefab;
        poolSO.FindProperty("initialSize").intValue = 30;
        poolSO.FindProperty("canGrow").boolValue = true;
        poolSO.ApplyModifiedProperties();
        Undo.RegisterCreatedObjectUndo(poolGO, "Crear Bullet Pool");

        // 5. Configurar el Player
        GameObject player = GameObject.Find("Player (RI + LG)");
        if (player == null)
        {
            player = new GameObject("Player (RI + LG)");
        }
        player.transform.position = new Vector3(0f, 1f, 0f);
        player.transform.rotation = Quaternion.identity;

        // Añadir Character Controller
        CharacterController charCtrl = player.GetComponent<CharacterController>();
        if (charCtrl == null) charCtrl = player.AddComponent<CharacterController>();
        charCtrl.height = 1.8f;
        charCtrl.center = new Vector3(0f, 0.9f, 0f);

        // Añadir Player Input
        PlayerInput playerInput = player.GetComponent<PlayerInput>();
        if (playerInput == null) playerInput = player.AddComponent<PlayerInput>();
        InputActionAsset inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
        if (inputActions != null)
        {
            playerInput.actions = inputActions;
            playerInput.defaultControlScheme = "Keyboard&Mouse";
        }
        else
        {
            Debug.LogWarning("[Configurador] No se encontró el asset Assets/InputSystem_Actions.inputactions.");
        }

        // Configurar Cabeza y Cámara
        Transform cabeza = player.transform.Find("Cabeza");
        if (cabeza == null)
        {
            GameObject cabezaGO = new GameObject("Cabeza");
            cabeza = cabezaGO.transform;
            cabeza.SetParent(player.transform);
        }
        cabeza.localPosition = new Vector3(0f, 1.6f, 0f);
        cabeza.localRotation = Quaternion.identity;

        // Buscar cámara principal en la escena y emparentarla a Cabeza
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.transform.SetParent(cabeza);
            mainCamera.transform.localPosition = Vector3.zero;
            mainCamera.transform.localRotation = Quaternion.identity;
        }
        else
        {
            GameObject camGO = new GameObject("Main Camera");
            camGO.AddComponent<Camera>();
            camGO.AddComponent<AudioListener>();
            camGO.tag = "MainCamera";
            camGO.transform.SetParent(cabeza);
            camGO.transform.localPosition = Vector3.zero;
            camGO.transform.localRotation = Quaternion.identity;
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

        // Configurar punto de disparo (FirePoint)
        Transform firePoint = cabeza.Find("FirePoint");
        if (firePoint == null)
        {
            GameObject fpGO = new GameObject("FirePoint");
            firePoint = fpGO.transform;
            firePoint.SetParent(cabeza);
        }
        firePoint.localPosition = new Vector3(0.3f, -0.2f, 0.6f); // un poco a la derecha y adelante de la cámara
        firePoint.localRotation = Quaternion.identity;

        // Asignar referencias en LG_Shoot usando SerializedObject
        SerializedObject shootSO = new SerializedObject(shoot);
        shootSO.FindProperty("bulletPool").objectReferenceValue = poolComp;
        shootSO.FindProperty("firePoint").objectReferenceValue = firePoint;
        shootSO.FindProperty("fireRate").floatValue = 0.15f;
        shootSO.ApplyModifiedProperties();

        // Añadir LG_Inventory
        LG_Inventory inventory = player.GetComponent<LG_Inventory>();
        if (inventory == null) inventory = player.AddComponent<LG_Inventory>();

        Undo.RegisterCreatedObjectUndo(player, "Configurar Player");

        // 6. Configurar el Canvas HUD y Componente LG_HUD
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

        // Limpiar elementos de HUD anteriores si existieran
        Transform oldSlider = canvasGO.transform.Find("StaminaSlider");
        if (oldSlider != null) DestroyImmediate(oldSlider.gameObject);
        Transform oldText = canvasGO.transform.Find("InventoryText");
        if (oldText != null) DestroyImmediate(oldText.gameObject);

        // Crear elementos UI
        Slider staminaSlider = CrearSliderStamina(canvasGO.transform);
        Text inventoryText = CrearTextoInventario(canvasGO.transform);

        // Configurar LG_HUD
        LG_HUD hudComp = canvasGO.GetComponent<LG_HUD>();
        if (hudComp == null) hudComp = canvasGO.AddComponent<LG_HUD>();

        SerializedObject hudSO = new SerializedObject(hudComp);
        hudSO.FindProperty("playerMovement").objectReferenceValue = movement;
        hudSO.FindProperty("playerInventory").objectReferenceValue = inventory;
        hudSO.FindProperty("staminaSlider").objectReferenceValue = staminaSlider;
        hudSO.FindProperty("inventoryText").objectReferenceValue = inventoryText;
        hudSO.ApplyModifiedProperties();
        Undo.RegisterCreatedObjectUndo(canvasGO, "Crear Canvas HUD");

        // 7. Crear coleccionables en la escena
        GameObject colRoot = GameObject.Find("Coleccionables");
        if (colRoot == null)
        {
            colRoot = new GameObject("Coleccionables");
        }

        // Limpiar coleccionables anteriores
        for (int i = colRoot.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(colRoot.transform.GetChild(i).gameObject);
        }

        // Crear Coleccionables flotantes interactivos
        CrearColeccionable(colRoot.transform, "Ammo_Box_Green", new Vector3(4f, 0.6f, 5f), "Municion", 10, materialAmmo);
        CrearColeccionable(colRoot.transform, "Battery_Pack_Blue", new Vector3(-4f, 0.6f, 5f), "Bateria", 1, materialBattery);
        CrearColeccionable(colRoot.transform, "Red_Key_Pink", new Vector3(0f, 0.6f, 13f), "Llave Roja", 1, materialKey);
        Undo.RegisterCreatedObjectUndo(colRoot, "Crear Coleccionables Root");

        // 8. Crear algunos objetivos interactivos (cubos con físicas)
        GameObject obstaculosRoot = GameObject.Find("Obstaculos y Objetivos");
        if (obstaculosRoot == null)
        {
            obstaculosRoot = new GameObject("Obstaculos y Objetivos");
        }

        // Crear una pila o torre de cubos para derribar disparando
        Vector3 spawnCenter = new Vector3(0f, 0.5f, 8f);
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
                Undo.RegisterCreatedObjectUndo(target, "Crear Objetivo Físico");
            }
        }

        // Mostrar diálogo de confirmación
        EditorUtility.DisplayDialog("Éxito", 
            "¡El prototipo se ha configurado con éxito usando materiales URP!\n\n" +
            "- Se configuró el Player y el Inventario.\n" +
            "- Se generó el Canvas UI para Estamina e Inventario en pantalla.\n" +
            "- Se colocaron coleccionables flotantes interactivos de colores.\n" +
            "- Se configuró el Object Pool de Balas en la escena (las balas empujan bloques).\n" +
            "- Se crearon objetivos físicos destruibles (torre de cubos).\n\n" +
            "¡Haz clic en Play para probar!", 
            "OK");
    }

    private static Slider CrearSliderStamina(Transform parent)
    {
        // 1. Root Slider GO
        GameObject sliderGO = new GameObject("StaminaSlider");
        sliderGO.transform.SetParent(parent, false);
        RectTransform sliderRect = sliderGO.AddComponent<RectTransform>();
        sliderRect.anchoredPosition = new Vector2(40f, -40f);
        sliderRect.sizeDelta = new Vector2(250f, 15f);
        sliderRect.anchorMin = new Vector2(0f, 1f); // Top Left
        sliderRect.anchorMax = new Vector2(0f, 1f);
        sliderRect.pivot = new Vector2(0f, 1f);

        Slider slider = sliderGO.AddComponent<Slider>();

        // 2. Background
        GameObject bgGO = new GameObject("Background");
        bgGO.transform.SetParent(sliderRect, false);
        RectTransform bgRect = bgGO.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        Image bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.6f);

        // 3. Fill Area
        GameObject fillAreaGO = new GameObject("Fill Area");
        fillAreaGO.transform.SetParent(sliderRect, false);
        RectTransform fillAreaRect = fillAreaGO.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = Vector2.zero;

        // 4. Fill
        GameObject fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(fillAreaRect, false);
        RectTransform fillRect = fillGO.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        Image fillImg = fillGO.AddComponent<Image>();
        fillImg.color = new Color(0.2f, 0.6f, 1f, 0.8f); // Color azul estamina

        // Link references
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
        textRect.anchoredPosition = new Vector2(40f, -80f);
        textRect.sizeDelta = new Vector2(350f, 250f);
        textRect.anchorMin = new Vector2(0f, 1f); // Top Left
        textRect.anchorMax = new Vector2(0f, 1f);
        textRect.pivot = new Vector2(0f, 1f);

        Text text = textGO.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); // Fuente estándar
        text.fontSize = 20;
        text.color = Color.white;
        text.supportRichText = true;
        text.alignment = TextAnchor.UpperLeft;

        // Sombra de lectura
        Shadow shadow = textGO.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
        shadow.effectDistance = new Vector2(1.5f, -1.5f);

        return text;
    }

    private static void CrearColeccionable(Transform parent, string goName, Vector3 position, string itemName, int amount, Material mat)
    {
        // Crear un objeto en forma de Cubo Girado o Diamante para el coleccionable
        GameObject colGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        colGO.name = goName;
        colGO.transform.SetParent(parent);
        colGO.transform.position = position;
        colGO.transform.rotation = Quaternion.Euler(45f, 45f, 45f); // Diamante
        colGO.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        // Añadir componente de coleccionable
        LG_Collectible collectible = colGO.AddComponent<LG_Collectible>();
        
        SerializedObject colSO = new SerializedObject(collectible);
        colSO.FindProperty("itemName").stringValue = itemName;
        colSO.FindProperty("amount").intValue = amount;
        colSO.FindProperty("rotationSpeed").floatValue = 55f;
        colSO.FindProperty("bobFrequency").floatValue = 2f;
        colSO.FindProperty("bobAmplitude").floatValue = 0.15f;
        colSO.ApplyModifiedProperties();

        // Aplicar material URP
        colGO.GetComponent<Renderer>().sharedMaterial = mat;

        // Asegurar colisionador como Trigger
        Collider c = colGO.GetComponent<Collider>();
        if (c != null) c.isTrigger = true;

        Undo.RegisterCreatedObjectUndo(colGO, $"Crear Coleccionable {goName}");
    }

    private static Material ObtenerOCrearMaterialURP(string path, Color color)
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
            if (urpShader == null)
            {
                urpShader = Shader.Find("Standard");
            }
            
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
            if (mat.shader.name == "Standard")
            {
                Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
                if (urpShader != null)
                {
                    mat.shader = urpShader;
                }
            }
        }
        return mat;
    }
}
