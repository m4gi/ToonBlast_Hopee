using deVoid.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockObject : BlockElement
{
    public enum BlockType
    {
        None,
        Orange,
        Yellow,
        Blue,
        Green,
        Red,
        Boom
    }

    public int idBlock;

    [SerializeField]
    private BlockType blockColor;

    public BlockType TypeBlock { get { return blockColor; } }
    public void SetTypeBlock(string name, int id)
    {
        idBlock = id;
        switch (name)
        {
            case "Orange":
                blockColor = BlockType.Orange;
                break;
            case "Yellow":
                blockColor = BlockType.Yellow;
                break;
            case "Blue":
                blockColor = BlockType.Blue;
                break;
            case "Green":
                blockColor = BlockType.Green;
                break;
            case "Red":
                blockColor = BlockType.Red;
                break;
            case "Boom":
                blockColor = BlockType.Boom; 
                break;
            default:
                blockColor = BlockType.None;
                break;

        }
    }

    public override void OnMouseDown()
    {
        print("Block " + transform.position);
        Signals.Get<GameManagerGetClickedBrickSignal>().Dispatch(gameObject.transform);
    }
}
