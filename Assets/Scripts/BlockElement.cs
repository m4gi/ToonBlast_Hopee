using deVoid.Utils;
using UnityEngine;
public enum BlockType
{
    None,
    Orange,
    Yellow,
    Blue,
    Green,
    Red,
    Bomb
}
public abstract class BlockElement : MonoBehaviour
{
    public abstract void OnMouseDown();

    private int _x = -1;

    private int _y = -1;

    private static readonly int Destroy1 = Animator.StringToHash("destroy");

    

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
            case "Bomb":
                blockColor = BlockType.Bomb;
                break;
            default:
                blockColor = BlockType.None;
                break;

        }
    }

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
