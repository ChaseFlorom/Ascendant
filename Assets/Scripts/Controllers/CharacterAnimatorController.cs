using UnityEngine;

public class CharacterAnimatorController : MonoBehaviour
{
    public Animator downAnimator;
    public Animator upAnimator;
    public Animator sideAnimator;

    public void SetBool(string parameterName, bool value)
    {
        if (downAnimator) downAnimator.SetBool(parameterName, value);
        if (upAnimator) upAnimator.SetBool(parameterName, value);
        if (sideAnimator) sideAnimator.SetBool(parameterName, value);
    }

    public void ActivateDown()
    {
        if (downAnimator) downAnimator.gameObject.SetActive(true);
        if (upAnimator) upAnimator.gameObject.SetActive(false);
        if (sideAnimator) sideAnimator.gameObject.SetActive(false);
    }

    public void ActivateUp()
    {
        if (downAnimator) downAnimator.gameObject.SetActive(false);
        if (upAnimator) upAnimator.gameObject.SetActive(true);
        if (sideAnimator) sideAnimator.gameObject.SetActive(false);
    }

    public void ActivateSide(bool faceLeft)
    {
        if (downAnimator) downAnimator.gameObject.SetActive(false);
        if (upAnimator) upAnimator.gameObject.SetActive(false);
        if (sideAnimator)
        {
            sideAnimator.gameObject.SetActive(true);
            Vector3 s = sideAnimator.transform.localScale;
            s.x = faceLeft ? -Mathf.Abs(s.x) : Mathf.Abs(s.x);
            sideAnimator.transform.localScale = s;
        }
    }
}
