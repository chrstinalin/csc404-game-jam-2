using System;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class LockOnSelectable : MonoBehaviour
{
    [NonSerialized] public PlayerMarker PlayerMarker;
    [NonSerialized] public bool IsHover = false;

    public void Start()
    {
        PlayerMarker = PlayerMarker.Instance;
    }

    public void Update()
    {
        bool playerControllingMech = MovementManager.Instance != null && !MovementManager.Instance.IsMouseActive;
        if (!playerControllingMech) return;
        
        if (IsHover && Input.GetButtonDown("Interact") && PlayerMarker.Target != gameObject)
        {
            Debug.Log("Selected! Current target is " + gameObject);
            PlayerMarker.SetTarget(gameObject);
        }
    }

    public void OnHover(bool val)
    {
        IsHover = val;
        Outline enemyOutline = gameObject.GetComponent<Outline>();
        enemyOutline.OutlineWidth = IsHover ? Config.SELECTED_ENEMY_OUTLINE_WIDTH : Config.ENEMY_OUTLINE_WIDTH;
    }
}
