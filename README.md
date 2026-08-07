# Prototipado Colaborativo - Sistema de Enemigos, Vida y UI

Este repositorio contiene un prototipo de juego estilo shooter en primera persona configurado en Universal Render Pipeline (URP). Recientemente se han añadido sistemas de combate avanzados, inteligencia de enemigos con animaciones procedimentales y retroalimentación de interfaz de usuario.

---

## 🚀 Características Añadidas

### 1. Sistema de Enemigos (Zombis 3D)
* **Integración de Modelos**: Se reemplazaron las cápsulas de prueba por el modelo 3D articulado de zombi (`ZombieMale_AAB_URP.prefab`) dentro de la escena de pruebas.
* **Animaciones Procedimentales**: Como el pack de recursos original no incluía clips de animación, se implementaron movimientos realistas directamente en C# a través de transformaciones locales en los huesos de los hombros (`upperarm_l` / `upperarm_r`):
  * **Caminar/Persecución**: Brazos levantados hacia adelante oscilando con una función senoidal, acompañados de un bamboleo de lado a lado del cuerpo entero.
  * **Reposo (Idle)**: Brazos relajados a los lados con un movimiento lento que simula la respiración.
  * **Embestida de Ataque**: Estiramiento rápido de los brazos hacia el frente cuando el zombi hiere al jugador.

### 2. Combate e Interacción
* **Daño al Zombi**: Las balas infligen daño y el cuerpo del zombi parpadea en color rojo brillante (afectando a todas sus mallas: cabeza, camisa, pantalones, etc.) por una fracción de segundo.
* **Ataque al Jugador**: El zombi inflige `10` de daño al jugador en intervalos de `1.5` segundos si se encuentra a distancia de ataque.
* **Recompensa por Derrota**: Al morir, el zombi genera un cubo físico brillante con física de diamante flotante que contiene munición o baterías. Al pasar sobre él, se añade directamente al inventario del jugador.

### 3. Sistema de Salud del Jugador
* **Salud del Player (`LG_PlayerHealth.cs`)**: Administra la cantidad de puntos de vida del personaje.
* **Reaparición (Soft-Respawn)**: Si los puntos de vida llegan a `0`, la vida se recarga por completo y el jugador es teletransportado de vuelta al punto de inicio de la escena para poder seguir jugando en modo sandbox sin interrumpir el flujo.

### 4. Interfaz de Usuario HUD Reajustada
* **Barra de Vida**: Añadida una nueva barra deslizante (Slider) de color rojo rubí en la esquina superior izquierda.
* **Barra de Estamina**: La barra de estamina original se movió hacia abajo y se mantuvo en color azul brillante.
* **Inventario**: La caja de texto informativa del inventario ahora se coloca justo debajo de ambas barras de manera ordenada y legible.

---

## 🛠️ Cómo Configurar y Probar

Todo el sistema está completamente integrado con la ventana editora del configurador del proyecto para una generación automatizada.

1. Abre el proyecto en **Unity Editor**.
2. En la barra superior, haz clic en **Prototipo > Configurar Escena Jugable**.
3. En la ventana emergente, haz clic en **Generar Prototipo Completo**.
4. ¡Listo! La escena se limpiará y configurará en segundos con el suelo, el jugador, el canvas del HUD con las nuevas barras, los coleccionables iniciales, la torre física de cubos y los nuevos zombis programados para perseguirte.
5. Presiona **Play** para comenzar a testear.

---

## 📁 Archivos Principales del Sistema

* [LG_Enemy.cs](Assets/LG_Shooting/Scripts/LG_Enemy.cs): Lógica de IA, ataque, daño multi-renderer, drops y animación de brazos.
* [LG_PlayerHealth.cs](Assets/LG_Shooting/Scripts/LG_PlayerHealth.cs): Control de la vida del jugador, curación y reaparición en el origen.
* [LG_HUD.cs](Assets/LG_Shooting/Scripts/LG_HUD.cs): Actualización de los valores de UI en pantalla.
* [LG_Collectible.cs](Assets/LG_Shooting/Scripts/LG_Collectible.cs): Añadida la inicialización dinámica de ítems y cantidades.
* [LG_Bullet.cs](Assets/LG_Shooting/Scripts/LG_Bullet.cs): Lógica de impacto y envío de señales de daño al script de enemigo.
* [PrototypeSetupWindow.cs](Assets/LG_Shooting/Scripts/Editor/PrototypeSetupWindow.cs): Generación de la escena de zombis, asignación de referencias y acomodo visual de los sliders de la interfaz.

