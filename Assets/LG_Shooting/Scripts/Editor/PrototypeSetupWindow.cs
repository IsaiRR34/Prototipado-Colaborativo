using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;

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
                      "- Rotación de cámara (Mouse Look) configurada.\n" +
                      "- Un pool de balas (LG_ObjectPool) y prefab de bala creado automáticamente con materiales URP.\n" +
                      "- Un escenario básico de pruebas (suelo y objetivos físicos con materiales URP).\n\n" +
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
        Undo.RegisterCreatedObjectUndo(player, "Configurar Player");

        // 6. Crear algunos objetivos interactivos (cubos con físicas)
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
            "- Se configuró el Player: 'Player (RI + LG)'\n" +
            "- Se configuró el Object Pool de Balas en la escena.\n" +
            "- Se creó un prefab de bala básico en LGAssets.\n" +
            "- Se crearon objetivos destructibles apilados enfrente.\n\n" +
            "¡Haz clic en Play para probar!", 
            "OK");
    }

    private static Material ObtenerOCrearMaterialURP(string path, Color color)
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            // Intentar usar el Shader Lit de URP
            Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
            if (urpShader == null)
            {
                // Fallback a Standard por si acaso no estuviera inicializado URP aún
                urpShader = Shader.Find("Standard");
            }
            
            mat = new Material(urpShader);
            mat.color = color;
            
            // Crear carpetas si no existen
            string dir = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }

            AssetDatabase.CreateAsset(mat, path);
        }
        else
        {
            // Asegurarse de que use el shader correcto si ya existía pero era Standard
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
