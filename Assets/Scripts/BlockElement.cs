using deVoid.Utils;
using UnityEngine;

public abstract class BlockElement : MonoBehaviour
{
    public abstract void OnMouseDown();
     
    private int _x = -1;

    private int _y = -1;

    private static readonly int Destroy1 = Animator.StringToHash("destroy");

    public void Trigger(int x, int y)
    {
        var anim = GetComponent<Animator>();
        anim.SetTrigger(Destroy1);
        _x = x;
        _y = y;
    }
    private void OnDestroy()
    {
        Signals.Get<GameManagerDeleteFromGridSignal>().Dispatch(_x, _y);
    }
}
