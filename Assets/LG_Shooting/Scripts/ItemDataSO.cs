using UnityEngine;

public enum ItemType
{
    Municion,
    Curacion,
    Recurso,
    LlaveMaestra
}

[CreateAssetMenu(fileName = "NewItemData", menuName = "SombraRural/Item Data SO")]
public class ItemDataSO : ScriptableObject
{
    [Header("Identificación")]
    public string itemID = "item_ammo";
    public string itemName = "Munición";
    [TextArea(2, 4)]
    public string description = "Balas estándar para pistola 9mm.";
    public ItemType itemType = ItemType.Municion;
    public Sprite icon;
    public Color themeColor = Color.white;

    [Header("Parámetros de Stack e Inventario")]
    public int maxStack = 50;
    [Tooltip("Si es true, este objeto no se puede tirar ni remover (ej. Llaves Maestras)")]
    public bool isPermanent = false;

    [Header("Efectos de Uso")]
    [Tooltip("Cantidad de vida que restaura al consumirse (para Curación)")]
    public float healAmount = 30f;
    [Tooltip("Cantidad de durabilidad que repara al usarse (para Recursos de Bate/Antorcha)")]
    public float repairAmount = 50f;

    [Header("Audio")]
    public AudioClip pickupSound;
}
