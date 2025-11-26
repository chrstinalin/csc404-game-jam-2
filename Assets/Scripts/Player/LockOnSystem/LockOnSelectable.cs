using System;
using UnityEngine;
 

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
        if (MovementManager.Instance == null) return;
        
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
