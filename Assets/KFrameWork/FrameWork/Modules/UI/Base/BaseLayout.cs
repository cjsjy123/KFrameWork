using UnityEngine;
using KFrameWork;

public abstract class BaseLayout:AbstractLayout
{
    protected override void DestroyUI(GameUI ui)
    {
        ui.DestorySelf();
    }
}