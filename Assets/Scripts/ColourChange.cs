using System.Collections;
using UnityEngine;

public class ColourChange : MonoBehaviour
{
    public Material win;
    public Material defMat;

    private Renderer rend;
    private Collider targetCollider;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        defMat = rend.material;
        targetCollider = GetComponent<Collider>();
    }

    private void OnEnable()
    {
        Box.OnBoxReachedGoal += ChangeMaterialTemporary;
    }

    private void OnDisable()
    {
        Box.OnBoxReachedGoal -= ChangeMaterialTemporary;
    }

    private void ChangeMaterialTemporary(Collider target)
    {
        if (target != targetCollider)
        {
            return;
        }

        StopAllCoroutines(); // In case it's already running
        StartCoroutine(ChangeRoutine());
    }

    private IEnumerator ChangeRoutine()
    {
        rend.material = win;
        yield return new WaitForSeconds(1f);
        rend.material = defMat;
    }
}
