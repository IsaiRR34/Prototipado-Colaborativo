# Prototipado Colaborativo - FPS Survival MVP (URP)

Este repositorio contiene un prototipo jugable estilo **shooter en primera persona (FPS / Survival Horror)** configurado en **Universal Render Pipeline (URP)**. Incluye un recinto cerrado con texturas industriales, combate con armas de fuego y cuerpo a cuerpo, zombis articulados con animaciones procedimentales, sistema de inventario, llaves y condición de victoria.

---

## 🚀 Características del MVP Jugable

### 1. Entorno y Arena Cerrada con Texturas URP
* **Arena Perimetral (30x30m)**: Escenario delimitado por muros industriales para evitar caídas al vacío y crear una atmósfera inmersiva.
* **Texturas Dedicadas**:
  * **Suelo**: Baldosas de hormigón reforzado con rejillas de ventilación metálicas y desgaste (`Tex_Suelo.png`).
  * **Paredes**: Muros de búnker de hormigón y planchas de acero oscuro con remaches (`Tex_Pared.png`).
  * **Puerta Blindada**: Compuerta de seguridad pesada con franjas de advertencia de peligro amarillas y negras (`Tex_Puerta.png`).
* **Vano de Salida**: La pared norte integra de forma natural el marco y la puerta de escape.

### 2. Puerta Interactiva y Condición de Victoria ([Door.cs](file:///Assets/DoorKey/Door.cs))
* **Mecánica de Llave y Salida**: Para completar el nivel, el jugador debe explorar el recinto, sortear los obstáculos y recoger la **"Llave Roja"** (`Red_Key_Pink`).
* **Apertura de la Puerta**: Al acercarse a la puerta o presionar la tecla `E` con la llave en el inventario, la puerta rota suavemente en su bisagra (-95°) y abre el paso hacia la libertad.
* **Transición a Victoria**: Cruzar la puerta carga automáticamente la pantalla de victoria (`Victory.unity`).

### 3. Sistema de Combate Dual: Pistola y Bate Melee
* **Pistola (Tecla `1`)**: Disparo mediante Object Pool con 12 balas en cargador. Recarga con la tecla `R` consumiendo reservas del inventario.
* **Bate de Béisbol (Tecla `3`)**:
  * **Animación Procedimental de Swing**: Golpe angular rápido (70°) en C# sin depender de clips externos.
  * **Detección de Impacto (`SphereCast`)**: 2.5m de alcance frontal, aplica 2 de daño a zombis y propulsa físicamente objetos con Rigidbody.
* **Linterna (Tecla `2`)**: Iluminación para entornos oscuros.
* **Retícula (Crosshair)**: Punto de mira central en pantalla para apuntar con precisión en primera persona.

### 4. Enemigos Zombis 3D ([LG_Enemy.cs](file:///Assets/LG_Shooting/Scripts/LG_Enemy.cs))
* **Modelo 3D y Persecución**: Detección del jugador en radio de 15m con avance continuo.
* **Animaciones Procedimentales**: Oscilación senoidal de brazos y bamboleo de cuerpo al caminar; estiramiento frontal al atacar.
* **Parpadeo de Daño**: Al recibir impactos de bala o golpes del bate, todo el modelo del zombi destella en color rojo brillante.
* **Recompensas al Morir**: Al ser derrotado, genera un cubo brillante con munición o batería que se añade directamente al inventario.

### 5. Sistema de Inventario y HUD en Tiempo Real
* **Estandarización de "Munición"**: Normalización automática en [LG_Inventory.cs](file:///Assets/LG_Shooting/Scripts/LG_Inventory.cs) para sincronizar recolección y recarga.
* **HUD Reorganizado**:
  * **Barra de Vida**: Slider rojo en la esquina superior izquierda.
  * **Barra de Estamina**: Slider azul con recuperación gradual al dejar de correr.
  * **Contador de Munición**: Indicador numérico en tiempo real en la esquina superior derecha (`Cargador / Reserva`).
  * **Inventario Dinámico**: Desglose de ítems acumulados en pantalla.
  * **Mira Central (Crosshair)**: Retícula centrada para disparo y combate.

---

## 🛠️ Cómo Configurar y Probar

Todo el sistema está completamente automatizado a través de la herramienta de editor:

1. Abre el proyecto en **Unity Editor**.
2. En la barra superior, haz clic en **Prototipo > Configurar Escena Jugable**.
3. En la ventana emergente, haz clic en **Generar Prototipo Completo**.
4. ¡Listo! La herramienta generará en segundos:
   * Suelo y paredes perimetrales con texturas URP y colisiones.
   * La puerta blindada de salida en el vano norte.
   * El jugador con movimiento, cámara, armas (pistola y bate) y HUD completo.
   * Coleccionables de Munición, Batería y la Llave Roja.
   * La torre física de cubos y los zombis patrullando.
5. Presiona **Play** para comenzar a jugar el MVP.

---

## 📁 Archivos Principales del MVP

* [Door.cs](file:///Assets/DoorKey/Door.cs): Lógica interactiva de apertura con llave y carga de escena de victoria.
* [Hand.cs](file:///Assets/Melee/Hand.cs): Cambio de armas y ataque melee con animación procedimental de swing.
* [LG_Shoot.cs](file:///Assets/LG_Shooting/Scripts/LG_Shoot.cs): Disparo con cargador, recarga `R` y control de munición.
* [LG_Inventory.cs](file:///Assets/LG_Shooting/Scripts/LG_Inventory.cs): Almacén de ítems con normalización automática de nombres.
* [LG_Enemy.cs](file:///Assets/LG_Shooting/Scripts/LG_Enemy.cs): IA de zombi, daño, parpadeo visual, drops y animación senoidal.
* [LG_PlayerHealth.cs](file:///Assets/LG_Shooting/Scripts/LG_PlayerHealth.cs): Salud del jugador y reaparición.
* [LG_HUD.cs](file:///Assets/LG_Shooting/Scripts/LG_HUD.cs): Actualización de barras de vida, estamina e inventario.
* [PrototypeSetupWindow.cs](file:///Assets/LG_Shooting/Scripts/Editor/PrototypeSetupWindow.cs): Generador del escenario MVP, paredes, texturas URP y referencias.
