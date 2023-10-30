using deVoid.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockObject : BlockElement
{
    

    public override void OnMouseDown()
    {
        print("Block " + transform.position);
        Signals.Get<GameManagerGetClickedBrickSignal>().Dispatch(gameObject.transform);
    }
}
