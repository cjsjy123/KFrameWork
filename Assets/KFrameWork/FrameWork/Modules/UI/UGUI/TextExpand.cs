using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace KFrameWork
{
	public class TextExpand :Text 
	{

        private CanvasRenderer _canvasrender;

        public CanvasRenderer canvasrender
        {
            get
            {
                if (_canvasrender == null)
                    _canvasrender = GetComponent<CanvasRenderer>();
                return _canvasrender;
            }
        }

  //      public override bool IsActive ()
		//{
		//	return base.IsActive ();
		//}

  //      protected override void OnCanvasGroupChanged ()
		//{
		//	base.OnCanvasGroupChanged ();
		//}
		
		//protected override void OnCanvasHierarchyChanged ()
		//{
		//	base.OnCanvasHierarchyChanged ();
		//}
		
		//public override void SetAllDirty ()
		//{
		//	base.SetAllDirty ();
		//}
		
		//public override void SetLayoutDirty ()
		//{
		//	base.SetLayoutDirty ();
		//}
		
		//public override void SetVerticesDirty ()
		//{
		//	base.SetVerticesDirty ();
		//}
		
		//public override void SetMaterialDirty ()
		//{
		//	base.SetMaterialDirty ();
		//}
		
		//public override bool Raycast (Vector2 sp, Camera eventCamera)
		//{
		//	return base.Raycast (sp, eventCamera);
		//}

		
		//public override Material GetModifiedMaterial (Material baseMaterial)
		//{
		//	return base.GetModifiedMaterial (baseMaterial);
		//}
		
		//public override void Cull (Rect clipRect, bool validRect)
		//{
		//	base.Cull (clipRect, validRect);
		//}
		
		//public override void SetClipRect (Rect clipRect, bool validRect)
		//{
		//	base.SetClipRect (clipRect, validRect);
		//}
		
		//public override void RecalculateClipping ()
		//{
		//	base.RecalculateClipping ();
		//}
		
		//public override void RecalculateMasking ()
		//{
		//	base.RecalculateMasking ();
		//}
		
	}
}


