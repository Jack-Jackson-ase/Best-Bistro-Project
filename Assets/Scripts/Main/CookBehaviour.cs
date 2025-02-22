using UnityEngine;

public class CookBehaviour : MonoBehaviour
{
    [SerializeField] GameObject cookBody;
    [SerializeField] Animator cookAnimator;

    public void CookIsPeeking()
    {
        cookBody.SetActive(true);
        cookAnimator.SetTrigger("Looked at");
    }
    public void CookIsWatchingDeath()
    {
        cookBody.SetActive(true);
        cookAnimator.SetTrigger("Player Death");
    }

    public void CookIsJustStanding()
    {
        cookBody.SetActive(true);
    }
}
