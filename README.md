<p align="center">
  <img src="docs/logo.png" alt="TRON" width="660">
</p>

<p align="center">
  IIS-2024 Algoritmos y Estructuras de Datos I, Proyecto 1
</p>

---

## Descripción

Juego de motos de luz en 2D desarrollado en Unity. El jugador controla una moto que deja una estela a su paso y compite contra cuatro bots en una cuadrícula. Quien choca contra una estela, contra el borde o se queda sin combustible, pierde. El objetivo es ser el último en pie.

El proyecto pone en práctica estructuras de datos implementadas a mano, sin usar las colecciones de .NET para la lógica principal del juego:

| Estructura | Uso en el juego |
| --- | --- |
| Lista enlazada | La moto: la cabeza y cada segmento de la estela son nodos enlazados. |
| Grafo en malla | La cuadrícula: cada celda (`Node`) apunta a sus vecinas arriba, abajo, izquierda y derecha. |
| Cola de prioridad | Los objetos recogidos. El combustible tiene prioridad sobre el resto. |
| Pila | Los poderes recogidos. Se usa siempre el del tope. |

## Descarga

La versión compilada está disponible en la sección de releases: [TRON 1.0](https://github.com/Mandamientos/Tron/releases/tag/game).

1. Descargar `TRON_V1.rar`.
2. Descomprimirlo con WinRAR o 7-Zip.
3. Ejecutar el juego desde la carpeta extraída.

## Ejecutar desde el código fuente

Requisitos:

- Unity 2022.3.41f1 (LTS)
- TextMeshPro (incluido en el proyecto)

Pasos:

1. Clonar el repositorio.
2. Abrir la carpeta `Tron/` desde Unity Hub con la versión 2022.3.41f1.
3. Abrir la escena `Assets/Scenes/mainMenu.unity` y presionar Play.

## Controles

| Tecla | Acción |
| --- | --- |
| Flechas | Cambiar de dirección |
| 1 | Activar el poder en el tope de la pila |
| 2 a 7 | Mover el poder de esa posición al tope de la pila |
| Esc | Pausar y reanudar |

## Mecánicas

### Movimiento y combustible

La moto avanza una celda por paso en la dirección actual; no puede girar 180 grados. El combustible baja una unidad cada cinco movimientos y, al llegar a cero, la moto se destruye.

### Objetos (cola)

Aparecen en el mapa cada cinco segundos. Al recogerlos entran en una cola de prioridad de hasta ocho espacios, y cada ocho segundos se aplica el objeto del frente.

| Objeto | Efecto |
| --- | --- |
| Combustible | Recupera entre 15 y 24 unidades, hasta un máximo de 100. Tiene prioridad. |
| Estela | Agrega siete segmentos a la estela. |
| Bomba | Destruye la moto. |

### Poderes (pila)

Aparecen en el mapa cada diez segundos y se guardan en una pila de hasta siete espacios. El jugador decide cuando usarlos.

| Poder | Efecto |
| --- | --- |
| Hipervelocidad | Aumenta la velocidad durante cinco segundos sin consumir combustible. |
| Escudo | Da inmunidad ante los choques durante cinco segundos. |

### Bots

Hay cuatro bots (verde, celeste, morado y amarillo). Se mueven de forma aleatoria, evitan los bordes y tienen su propia cola de objetos y pila de poderes, que usan de forma automática. Si dos cabezas chocan, ambos bots se destruyen.

### Muerte y victoria

Cuando una moto se destruye, su estela desaparece y los objetos y poderes que llevaba vuelven a aparecer en el mapa. Si el jugador muere, regresa al menú principal; si elimina a los cuatro bots, pasa a la pantalla de victoria.

## Escenas

| Índice | Escena | Descripción |
| --- | --- | --- |
| 0 | `mainMenu` | Menú principal |
| 1 | `settingsMenu` | Volumen de música y efectos (se guardan con `PlayerPrefs`) |
| 2 | `pressAnyButton` | Pantalla de espera antes de la partida |
| 3 | `game` | Partida |
| 4 | `youWon` | Pantalla de victoria |

## Estructura del código

Todos los scripts están en `Tron/Assets/Scripts/`.

| Archivo | Responsabilidad |
| --- | --- |
| `gridCreator.cs` | Crea la cuadrícula y define la clase `Node`. |
| `playerController.cs` | Entrada del jugador, movimiento y muerte. Contiene la lista enlazada `motorBike`. |
| `itemsHandler.cs` | Generación de objetos y cola de prioridad del jugador (`itemQueue`). |
| `powerHandler.cs` | Generación de poderes, pila del jugador (`powerStack`) y reordenamiento con una cola auxiliar. |
| `botController.cs` | Movimiento y muerte de los bots. Contiene la lista enlazada `botBike`. |
| `botQueue.cs` | Cola de objetos de los bots. |
| `botStack.cs` | Pila de poderes de los bots. |
| `countdownAnim.cs` | Cuenta regresiva que da inicio a la partida. |
| `pauseController.cs` | Menú de pausa. |
| `settingsManager.cs` | Ajustes de volumen. |
| `musicHandler.cs` | Música del menú, persistente entre escenas. |
| `transitionHandler.cs` | Transiciones animadas entre escenas. |
| `anyKey.cs` | Pasa a la partida al presionar cualquier tecla. |
| `quitGame.cs` | Cierra la aplicación. |

## Documentación adicional

El documento completo del proyecto está en [`Proyecto_TRON_merged.pdf`](Proyecto_TRON_merged.pdf).
