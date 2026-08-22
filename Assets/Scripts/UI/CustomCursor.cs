using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Drop this on the root of a menu (MainMenu canvas, PauseScreen prefab, etc.).
// It swaps the OS cursor to the sprites in Assets/Sprites/UI/Cursor while this
// object is enabled, and restores the system cursor as soon as it's disabled -
// so gameplay scenes are never left with the menu cursor stuck on.
public class CustomCursor : MonoBehaviour
{
    [Header("Cursor Textures")]
    [SerializeField] private Texture2D defaultCursor;
    [SerializeField] private Texture2D hoverCursor;
    [SerializeField] private Texture2D clickCursor;

    [Header("Settings")]
    [SerializeField] private Vector2 hotspot = Vector2.zero;
    [SerializeField] private CursorMode cursorMode = CursorMode.Auto;

    private enum CursorState { Default, Hover, Click }
    private CursorState currentState;
    private bool stateInitialized;
    private CursorLockMode previousLockState;

    private PointerEventData pointerEventData;
    private readonly List<RaycastResult> raycastResults = new List<RaycastResult>();

    private void OnEnable()
    {
        // Gameplay (CameraManager) locks the cursor to the screen center for
        // camera look, which pins/hides the OS cursor regardless of Cursor.visible
        // or Cursor.SetCursor. Unlock it while this menu is up, and restore
        // whatever lock mode was active beforehand once it closes again.
        previousLockState = Cursor.lockState;
        Cursor.lockState = CursorLockMode.None;

        Cursor.visible = true;
        stateInitialized = false;
        ApplyCursor(CursorState.Default);
    }

    private void OnDisable()
    {
        // Only restore the prior lock state within a still-live scene (e.g. Resume
        // un-pausing gameplay). During a scene unload, gameObject.scene.isLoaded is
        // already false here - the incoming scene establishes its own cursor state,
        // and restoring stale state (e.g. a lock inherited from a level scene, still
        // remembered by an EndScreen that never wanted it) would just leak it forward.
        if (gameObject.scene.isLoaded)
            Cursor.lockState = previousLockState;

        // Hand control back to the OS/game default cursor so this menu's
        // cursor doesn't leak into whatever scene/state comes next.
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    private void Update()
    {
        bool overInteractable = IsPointerOverInteractable();
        bool pressed = Input.GetMouseButton(0);

        CursorState desired =
            pressed && overInteractable ? CursorState.Click :
            overInteractable ? CursorState.Hover :
            CursorState.Default;

        if (!stateInitialized || desired != currentState)
            ApplyCursor(desired);
    }

    private bool IsPointerOverInteractable()
    {
        if (EventSystem.current == null) return false;

        pointerEventData ??= new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;

        raycastResults.Clear();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);

        if (raycastResults.Count == 0) return false;

        // Only the topmost hit counts, mirroring how the input module picks a click
        // target (first raycast hit, then walk up for a handler). Scanning the whole
        // list instead would report "clickable" for a button sitting underneath some
        // other raycast-blocking graphic - showing a hand cursor on a click that can
        // never land.
        Selectable selectable = raycastResults[0].gameObject.GetComponentInParent<Selectable>();
        return selectable != null && selectable.interactable;
    }

    private void ApplyCursor(CursorState state)
    {
        currentState = state;
        stateInitialized = true;

        Texture2D texture = state switch
        {
            CursorState.Hover => hoverCursor != null ? hoverCursor : defaultCursor,
            CursorState.Click => clickCursor != null ? clickCursor : (hoverCursor != null ? hoverCursor : defaultCursor),
            _ => defaultCursor
        };

        Cursor.SetCursor(texture, hotspot, cursorMode);
    }
}
