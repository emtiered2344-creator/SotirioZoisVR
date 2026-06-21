using UnityEngine;

public class WoundManager : MonoBehaviour
{
    // --- Singleton Pattern ---
    public static WoundManager Instance { get; private set; }

    [Header("Active Wound")]
    [Tooltip("The wound the player is currently tasked to treat.")]
    [SerializeField] private Wound activeWound;

    public Wound ActiveWound
    {
        get => activeWound;
        set
        {
            // Hide the old active wound's points
            if (activeWound != null && !activeWound.IsHealed)
                activeWound.SetPointsVisibility(false);

            activeWound = value;

            // Show the new active wound's points (if it exists and isn't healed)
            if (activeWound != null && !activeWound.IsHealed)
            {
                activeWound.SetPointsVisibility(true);
                Debug.Log($"🎯 New Focus Wound: {activeWound.name}");
            }
        }
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }       

    /// <summary>
    /// Call this when a wound fails (contact timeout) to reset focus.
    /// </summary>
    public void WoundFailed(Wound failedWound)
    {
        if (activeWound == failedWound)
        {
            failedWound.SetPointsVisibility(false);
            activeWound = null;
        }
    }

    /// <summary>
    /// Call this when a wound is fully healed to clear the focus.
    /// </summary>
    public void WoundHealed(Wound healedWound)
    {
        if (activeWound == healedWound)
        {
            // Hide the points before clearing
            healedWound.SetPointsVisibility(false);
            activeWound = null;
            Debug.Log("✅ Active wound healed and cleared!");
        }
    }

}
