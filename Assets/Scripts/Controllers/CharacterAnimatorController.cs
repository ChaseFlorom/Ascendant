using UnityEngine;

public class CharacterAnimatorController : MonoBehaviour
{
    public Animator downAnimator;
    public Animator upAnimator;
    public Animator sideAnimator;

    public void SetBool(string parameterName, bool value)
    {
        if (IsValidAnimator(downAnimator) && downAnimator.gameObject.activeSelf)
            downAnimator.SetBool(parameterName, value);
        if (IsValidAnimator(upAnimator) && upAnimator.gameObject.activeSelf)
            upAnimator.SetBool(parameterName, value);
        if (IsValidAnimator(sideAnimator) && sideAnimator.gameObject.activeSelf)
            sideAnimator.SetBool(parameterName, value);
    }

    private bool IsValidAnimator(Animator animator)
    {
        return animator != null && animator.runtimeAnimatorController != null;
    }

    public void ActivateDown(bool isWalking)
    {
        if (downAnimator) {
            downAnimator.gameObject.SetActive(true);
            downAnimator.SetBool("isWalking", isWalking);
        }
        if (upAnimator) upAnimator.gameObject.SetActive(false);
        if (sideAnimator) sideAnimator.gameObject.SetActive(false);
    }

    public void ActivateUp(bool isWalking)
    {
        if (downAnimator) downAnimator.gameObject.SetActive(false);
        if (upAnimator) {
            upAnimator.gameObject.SetActive(true);
            upAnimator.SetBool("isWalking", isWalking);
        }
        if (sideAnimator) sideAnimator.gameObject.SetActive(false);
    }

    public void ActivateSide(bool faceLeft, bool isWalking)
    {
        if (downAnimator) downAnimator.gameObject.SetActive(false);
        if (upAnimator) upAnimator.gameObject.SetActive(false);
        if (sideAnimator)
        {
            sideAnimator.gameObject.SetActive(true);
            Vector3 s = sideAnimator.transform.localScale;
            s.x = faceLeft ? -Mathf.Abs(s.x) : Mathf.Abs(s.x);
            sideAnimator.transform.localScale = s;
            sideAnimator.SetBool("isWalking", isWalking);
        }
    }

    // Keep old overloads for compatibility
    public void ActivateDown() => ActivateDown(false);
    public void ActivateUp() => ActivateUp(false);
    public void ActivateSide(bool faceLeft) => ActivateSide(faceLeft, false);
}
