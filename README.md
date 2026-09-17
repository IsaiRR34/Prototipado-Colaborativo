# Prototipado Colaborativo - FPS Survival MVP (URP)

Este repositorio contiene un prototipo jugable estilo **shooter en primera persona (FPS / Survival Horror)** configurado en **Universal Render Pipeline (URP)**. Incluye un recinto cerrado con texturas industriales PBR y mapas de normales, combate con armas de fuego y cuerpo a cuerpo, efectos de sonido inmersivos (SFX), HUD táctico profesional, zombis articulados con animaciones procedimentales, sistema de inventario sincronizado, llaves y condición de victoria.

---

## 🚀 Características del MVP Jugable

### 1. Entorno y Arena Cerrada con Texturas URP y Mapas de Normales
* **Arena Perimetral (30x30m)**: Escenario delimitado por muros industriales para evitar caídas al vacío y crear una atmósfera de búnker inmersiva.
* **Materiales PBR con Relieve 3D (Normal Maps)**:
  * **Suelo**: Baldosas de hormigón reforzado con rejillas metálicas de ventilación (`Tex_Suelo.png` + `Tex_Suelo_Normal.png`).
  * **Paredes**: Muros de hormigón y planchas de acero oscuro con remaches tridimensionales (`Tex_Pared.png` + `Tex_Pared_Normal.png`).
  * **Puerta Blindada**: Compuerta de seguridad con volante, bisagras y franjas de advertencia (`Tex_Puerta.png` + `Tex_Puerta_Normal.png`).
* **Vano de Salida**: La pared norte integra de forma natural el marco y la compuerta de escape.

---

### 2. Puerta Interactiva y Condición de Victoria ([Door.cs](file:///Assets/DoorKey/Door.cs))
* **Mecánica de Llave y Salida**: Para completar el nivel, el jugador debe explorar el recinto, sortear los obstáculos y recoger la **"Llave Roja"** (`Red_Key_Pink`).
* **Apertura de la Puerta**: Al acercarse a la compuerta o presionar `E` / `F` con la llave en el inventario:
  * Reproduce el sonido de desbloqueo pesado y rechinido metálico (`SFX_Door_Unlock.wav`).
  * Rota suavemente en su bisagra (-95°) a lo largo de 1.2 segundos.
  * Si el jugador intenta abrirla sin la llave, emite un aviso sonoro de bloqueo (`SFX_Empty.wav`).
* **Transición a Victoria**: Cruzar la puerta carga automáticamente la pantalla de victoria (`Victory.unity`).

---

### 3. Sistema de Combate Dual con Efectos de Sonido (SFX)
* **Pistola 9mm (Tecla `1`)**:
  * Disparo mediante Object Pool con 12 balas en el cargador.
  * **Sonido de Disparo (`SFX_Shoot.wav`)**: Impacto sonoro contundente con decaimiento natural.
  * **Gatillazo Seco (`SFX_Empty.wav`)**: Sonido de percutor metálico al disparar con el cargador en 0.
  * **Recarga Sonora (`SFX_Reload.wav`)**: Secuencia mecánica completa de 1.5s (expulsión, inserción de cargador y amartillado de corredera con la tecla `R`).
* **Bate de Béisbol Melee (Tecla `3`)**:
  * **Animación Procedimental de Swing**: Golpe angular rápido (70°) en C#.
  * **Sonido de Swing (`SFX_Melee_Swing.wav`)**: Silbido aerodinámico en cada ataque.
  * **Sonido de Impacto (`SFX_Melee_Hit.wav`)**: Golpe sordo al conectar con enemigos u obstáculos físicos.
  * **Detección (`SphereCast`)**: 2.5m de alcance frontal, aplica 2 de daño a zombis y propulsa objetos con Rigidbody.
* **Linterna (Tecla `2`)**: Iluminación táctica para entornos oscuros.
* **Retícula (Crosshair)**: Punto de mira central calibrado en pantalla para máxima precisión.

---

### 4. HUD Táctico Profesional y Dinámica de Munición
* **Widget de Munición en Esquina Inferior Derecha (`MunicionContainer` / `MunicionPannel`)**:
  * Diseño Glassmorphism oscuro translúcido (`rgba(15, 23, 36, 0.85)`).
  * **Estado Normal**: Tipografía nítida en blanco (`38pt`) mostrando balas en cargador, separador `/`, reserva en inventario (`20pt`) y etiqueta `PISTOLA 9MM`.
  * **Alerta de Munición Baja ($\le 3$)**: El número cambia a tono rojizo de precaución con la etiqueta `MUNICIÓN BAJA`.
  * **Cargador Vacío ($0$)**: Resalta en color rojo con indicación destacada en amarillo: `[ R ] RECARGAR`.
  * **Durante Recarga**: Muestra dinámicamente `RECARGANDO...`.
  * **Sin Munición Total ($0/0$)**: Alerta crítica `SIN MUNICIÓN`.
  * **Ocultamiento Contextual**: El widget se oculta limpiamente al cambiar al bate de béisbol y se restaura al reequipar la pistola.
* **Barras de Estado**:
  * **Salud**: Slider rojo en la esquina superior izquierda.
  * **Estamina**: Slider azul con recuperación gradual al correr o caminar.
  * **Inventario Dinámico**: Desglose en tiempo real de ítems acumulados en pantalla (`• Munición`, `• Bateria`, `• Llave Roja`).

---

### 5. Coleccionables Interactivos con Audio Único ([LG_Collectible.cs](file:///Assets/LG_Shooting/Scripts/LG_Collectible.cs))
* **Estandarización Total**: Normalización de nombres a `"Munición"` para sincronizar cajas, inventario y arma.
* **Audio Único para Cada Objeto**:
  * **Munición (`SFX_Pickup_Ammo.wav`)**: Tintineo metálico nítido de cartuchos de balas cayendo en el estuche.
  * **Batería (`SFX_Pickup_Battery.wav`)**: Zumbido eléctrico ascendente / sobrecarga de energía sci-fi.
  * **Llave Roja (`SFX_Pickup_Key.wav`)**: Tintineo de llaves de latón acompañado de un acorde brillante de recompensa.
* **Animación Flotante**: Rotación y oscilación senoidal continua en el espacio 3D.

---

### 6. Enemigos Zombis 3D ([LG_Enemy.cs](file:///Assets/LG_Shooting/Scripts/LG_Enemy.cs)) y Salud del Jugador ([LG_PlayerHealth.cs](file:///Assets/LG_Shooting/Scripts/LG_PlayerHealth.cs))
* **Audio de Daño al Zombi (`SFX_Zombie_Hit.wav`)**: Gruñido gutural de dolor e impacto de carne viva al recibir disparos o batazos.
* **Audio de Daño al Jugador (`SFX_Player_Hurt.wav`)**: Jadeo / quejido visceral de dolor e impacto contundente al ser golpeado por los zombis.
* **Modelo 3D y Persecución**: Detección del jugador en radio de 15m con avance continuo.
* **Animaciones Procedimentales**: Oscilación senoidal de brazos y bamboleo de cuerpo al caminar; estiramiento frontal al atacar.
* **Parpadeo de Daño**: Al recibir impactos de bala o golpes del bate, todo el modelo del zombi destella en color rojo brillante.
* **Recompensas al Morir**: Al ser derrotado, genera un cubo flotante con munición o batería con su correspondiente sonido al recogerlo.

---

## 🛠️ Cómo Configurar y Probar

Todo el sistema está completamente automatizado a través de la herramienta de editor:

1. Abre el proyecto en **Unity Editor**.
2. En la barra superior, haz clic en **Prototipo > Configurar Escena Jugable**.
3. En la ventana emergente, haz clic en **Generar Prototipo Completo**.
4. La herramienta generará y sincronizará en segundos:
   * Suelo y paredes perimetrales con texturas URP, mapas de normales y colisiones.
   * La puerta blindada de salida en el vano norte con audio de desbloqueo y escena de victoria vinculada.
   * El jugador con movimiento, cámara, armas (pistola y bate), AudioSource y efectos de sonido enlazados (incluyendo dolor al recibir daño).
   * Coleccionables con audio único diferenciado para Munición, Batería y Llave Roja.
   * Enemigos zombis con retroalimentación sonora al ser heridos.
   * El widget inferior derecho de munición y HUD completo.
   * La torre física de cubos y los zombis patrullando.
5. Presiona **Play** para jugar el prototipo.

---

## 📁 Estructura de Archivos Principales

* [Door.cs](file:///Assets/DoorKey/Door.cs): Lógica interactiva de apertura con llave, audio 3D de compuerta y transición a escena de victoria.
* [Hand.cs](file:///Assets/Melee/Hand.cs): Cambio de armas, ataque melee con animación procedimental y SFX de swing e impacto.
* [LG_Shoot.cs](file:///Assets/LG_Shooting/Scripts/LG_Shoot.cs): Disparo con cargador, SFX de disparo, recarga, gatillazo seco y control del HUD inferior derecho.
* [LG_Collectible.cs](file:///Assets/LG_Shooting/Scripts/LG_Collectible.cs): Coleccionables con rotación, bobbing y SFX único por ítem (`SFX_Pickup_Ammo`, `SFX_Pickup_Battery`, `SFX_Pickup_Key`).
* [LG_Inventory.cs](file:///Assets/LG_Shooting/Scripts/LG_Inventory.cs): Almacén de ítems con normalización automática de nombres ("Munición").
* [LG_Enemy.cs](file:///Assets/LG_Shooting/Scripts/LG_Enemy.cs): IA de zombi, daño, gruñido de dolor (`SFX_Zombie_Hit`), parpadeo visual, drops y animación senoidal.
* [LG_PlayerHealth.cs](file:///Assets/LG_Shooting/Scripts/LG_PlayerHealth.cs): Salud del jugador y quejido de dolor (`SFX_Player_Hurt`).
* [LG_HUD.cs](file:///Assets/LG_Shooting/Scripts/LG_HUD.cs): Actualización de barras de vida, estamina e inventario.
* [PrototypeSetupWindow.cs](file:///Assets/LG_Shooting/Scripts/Editor/PrototypeSetupWindow.cs): Generador del escenario MVP, paredes, texturas URP, mapas de normales, audio SFX y referencias.
* **Directorio de Audio**: `Assets/LG_Shooting/LGAssets/Audio/` (`SFX_Shoot.wav`, `SFX_Reload.wav`, `SFX_Empty.wav`, `SFX_Melee_Swing.wav`, `SFX_Melee_Hit.wav`, `SFX_Pickup_Ammo.wav`, `SFX_Pickup_Battery.wav`, `SFX_Pickup_Key.wav`, `SFX_Door_Unlock.wav`, `SFX_Zombie_Hit.wav`, `SFX_Player_Hurt.wav`).
