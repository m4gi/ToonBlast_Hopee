using deVoid.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombObject : BlockElement
{
    public override void OnMouseDown()
    {
        Signals.Get<GameManagerGetBombedBrickSignal>().Dispatch(gameObject.transform);
    }
}
