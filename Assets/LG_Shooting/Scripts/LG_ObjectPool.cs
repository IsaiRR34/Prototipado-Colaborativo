using System.Collections.Generic;
using UnityEngine;

public class LG_ObjectPool : MonoBehaviour
{
    [Header("Pool Configuration")]
    [Tooltip("The prefab to be pooled.")]
    [SerializeField] private GameObject prefab;

    [Tooltip("The initial number of instances to create in the pool.")]
    [SerializeField] private int initialSize = 20;

    [Tooltip("Whether the pool can instantiate more objects if all pooled objects are active.")]
    [SerializeField] private bool canGrow = true;

    private Queue<GameObject> poolQueue = new Queue<GameObject>();

    private void Start()
    {
        InitializePool();
    }

    /// <summary>
    /// Instantiates the initial size of the pool and keeps them inactive.
    /// </summary>
    private void InitializePool()
    {
        if (prefab == null)
        {
            Debug.LogError($"[LG_ObjectPool] Prefab is not assigned on GameObject '{gameObject.name}'!", this);
            return;
        }

        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            poolQueue.Enqueue(obj);
        }
    }

    /// <summary>
    /// Gets an active GameObject from the pool.
    /// </summary>
    /// <returns>An active GameObject or null if no object is available and the pool cannot grow.</returns>
    public GameObject Get()
    {
        // Search for an inactive object in the queue
        while (poolQueue.Count > 0)
        {
            GameObject obj = poolQueue.Dequeue();
            
            // Check if the object still exists (in case it was destroyed externally)
            if (obj != null)
            {
                obj.SetActive(true);
                return obj;
            }
        }

        // If no object is available and the pool is allowed to grow
        if (canGrow && prefab != null)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(true);
            return obj;
        }

        Debug.LogWarning($"[LG_ObjectPool] No objects available in the pool and CanGrow is disabled on '{gameObject.name}'.", this);
        return null;
    }

    /// <summary>
    /// Deactivates a GameObject and returns it to the pool.
    /// </summary>
    /// <param name="obj">The GameObject to return.</param>
    public void ReturnToPool(GameObject obj)
    {
        if (obj == null) return;

        obj.SetActive(false);
        poolQueue.Enqueue(obj);
    }
}
