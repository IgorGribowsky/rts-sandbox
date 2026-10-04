using UnityEngine;

public class GameController : MonoBehaviour
{
    public bool FriendlyFire = false;

    [Tooltip("How much of its price an unfinished building gives back when cancelled with Esc, in percent (M-010).")]
    [Range(0, 100)]
    public int BuildingCancelRefundPercent = 70;
}
