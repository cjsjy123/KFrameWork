using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;


namespace KFrameWork
{
	public class ExpandMenuItem 
	{
		private const float  kWidth       = 160f;
		private const float  kThickHeight = 30f;
		private const float  kThinHeight  = 20f;
		private const string kStandardSpritePath           = "UI/Skin/UISprite.psd";
		private const string kBackgroundSpriteResourcePath = "UI/Skin/Background.psd";
		private const string kInputFieldBackgroundPath     = "UI/Skin/InputFieldBackground.psd";
		private const string kKnobPath                     = "UI/Skin/Knob.psd";
		private const string kCheckmarkPath                = "UI/Skin/Checkmark.psd";

		private static Vector2 s_ThickGUIElementSize    = new Vector2(kWidth, kThickHeight);
		private static Vector2 s_ThinGUIElementSize     = new Vector2(kWidth, kThinHeight);
		private static Vector2 s_ImageGUIElementSize    = new Vector2(100f, 100f);


		private static Color whiteColor =new Color(1f, 1f, 1f, 0.392f);

		public const string root ="GameObject/KUI/";

        private static void ClearRays(Component t)
        {
            Graphic[] rays = t.GetComponentsInChildren<Graphic>(true);
            foreach (var ray in rays)
            {
                ray.raycastTarget = false;
            }

            Graphic gray = t.GetComponent<Graphic>();
            if (gray != null)
                gray.raycastTarget = false;
        }

		[MenuItem(root+"Text",false,5)]
		public static void CreateText(MenuCommand commond)
		{
			GameObject go = CreateUIElementRoot("Text",commond,s_ThickGUIElementSize);
			TextExpand text = go.AddComponent<TextExpand>();
			text.text = "Test UI";
            text.supportRichText = false;
			text.fontSize = 20;
            ClearRays(text);

            SetParentAndAlign(go, Selection.activeGameObject);

			Selection.activeObject = go;
		}

		[MenuItem(root+"Image",false,5)]
		public static void CreateImage(MenuCommand commond)
		{
			GameObject go =  CreateUIElementRoot("Image", commond, s_ImageGUIElementSize);
            ImageExpand img =go.AddComponent<ImageExpand>();
            img.type = Image.Type.Simple;
            ClearRays(img);
            SetParentAndAlign(go, Selection.activeGameObject);

			Selection.activeObject = go;
		}

		[MenuItem(root+"RawImage",false,5)]
		public static void CreateRawImage(MenuCommand commond)
		{
			GameObject go = CreateUIElementRoot("RawImage", commond, s_ImageGUIElementSize);
            RawImageExpand raw =go.AddComponent<RawImageExpand>();
            ClearRays(raw);

            SetParentAndAlign(go, Selection.activeGameObject);

			Selection.activeObject = go;
		}

		[MenuItem(root+"Button",false,5)]
		public static void CreateButton(MenuCommand commond)
		{
			GameObject go = CreateUIElementRoot("Button", commond, s_ThickGUIElementSize);
            go.AddComponent<ImageExpand>();
            go.AddComponent<ButtonExpand>();

            GameObject textGo = new GameObject("Label");
			TextExpand text = textGo.AddComponent<TextExpand>();
			text.text="Kubility";
			text.fontSize =20;
			text.color = Color.black;
			text.alignment = TextAnchor.MiddleCenter;
            ClearRays(text);

            SetParentAndAlign(go,Selection.activeGameObject);
			SetParentAndAlign(textGo,go);

			RectTransform textRectTransform = textGo.GetComponent<RectTransform>();
			textRectTransform.anchorMin = Vector2.zero;
			textRectTransform.anchorMax = Vector2.one;
			textRectTransform.sizeDelta = Vector2.zero;

			Selection.activeObject = go;
		}

		[MenuItem(root+"Toggle",false,5)]
		public static void CreateToggle(MenuCommand command)
		{
			GameObject go =CreateUIElementRoot("Toggle", command, s_ThinGUIElementSize);
            ToggleExpand toggle = go.AddComponent<ToggleExpand>();
            ClearRays(toggle);

            GameObject Bg = CreateObj("BackGround",go);
			var bgImage= AddImage(Bg,whiteColor,kStandardSpritePath);

			GameObject Checkmark = CreateObj("Checkmark",Bg);
			var checkmarkImage= AddImage(Checkmark,whiteColor,kStandardSpritePath);

			GameObject textGo = CreateObj("Label",go);
			TextExpand text = textGo.AddComponent<TextExpand>();
			text.text="Kubility";
			text.color = Color.black;

            ClearRays(text);

            toggle.graphic = checkmarkImage;
			toggle.targetGraphic = bgImage;


			RectTransform bgRect = Bg.GetComponent<RectTransform>();
			bgRect.anchorMin        = new Vector2(0f, 1f);
			bgRect.anchorMax        = new Vector2(0f, 1f);
			bgRect.anchoredPosition = new Vector2(10f, -10f);
			bgRect.sizeDelta        = new Vector2(kThinHeight, kThinHeight);
			
			RectTransform checkmarkRect = Checkmark.GetComponent<RectTransform>();
			checkmarkRect.anchorMin = new Vector2(0.5f, 0.5f);
			checkmarkRect.anchorMax = new Vector2(0.5f, 0.5f);
			checkmarkRect.anchoredPosition = Vector2.zero;
			checkmarkRect.sizeDelta = new Vector2(20f, 20f);
			
			RectTransform labelRect = textGo.GetComponent<RectTransform>();
			labelRect.anchorMin        = new Vector2(0f, 0f);
			labelRect.anchorMax        = new Vector2(1f, 1f);
			labelRect.offsetMin        = new Vector2(23f, 1f);
			labelRect.offsetMax        = new Vector2(-5f, -2f);
			
			Selection.activeObject = go;
		}

        public static GameObject CreateScrollbar()
        {
            // Create GOs Hierarchy
            GameObject scrollbarRoot = CreateUIElementRoot("Scrollbar", s_ThickGUIElementSize);

            GameObject sliderArea = CreateObj("Sliding Area", scrollbarRoot);
            GameObject handle = CreateObj("Handle", sliderArea);

            ImageExpand bgImage = scrollbarRoot.AddComponent<ImageExpand>();
            bgImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(kBackgroundSpriteResourcePath);
            bgImage.type = Image.Type.Sliced;
            bgImage.color = whiteColor;

            ImageExpand handleImage = handle.AddComponent<ImageExpand>();
            handleImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(kStandardSpritePath);
            handleImage.type = Image.Type.Sliced;
            handleImage.color = whiteColor;

            RectTransform sliderAreaRect = sliderArea.GetComponent<RectTransform>();
            sliderAreaRect.sizeDelta = new Vector2(-20, -20);
            sliderAreaRect.anchorMin = Vector2.zero;
            sliderAreaRect.anchorMax = Vector2.one;

            RectTransform handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(20, 20);

            ScrollbarExpand scrollbar = scrollbarRoot.AddComponent<ScrollbarExpand>();
            scrollbar.handleRect = handleRect;
            scrollbar.targetGraphic = handleImage;
            ColorBlock colors = scrollbar.colors;
            colors.highlightedColor = new Color(0.882f, 0.882f, 0.882f);
            colors.pressedColor = new Color(0.698f, 0.698f, 0.698f);
            colors.disabledColor = new Color(0.521f, 0.521f, 0.521f);

            return scrollbarRoot;
        }

        [MenuItem(root + "DropDown", false, 5)]
        public static void CreateDropdown(MenuCommand command)
        {
            GameObject root = CreateUIElementRoot("Dropdown",command, s_ThickGUIElementSize);

            GameObject label = CreateObj("Label", root);
            GameObject arrow = CreateObj("Arrow", root);
            GameObject template = CreateObj("Template", root);
            GameObject viewport = CreateObj("Viewport", template);
            GameObject content = CreateObj("Content", viewport);
            GameObject item = CreateObj("Item", content);
            GameObject itemBackground = CreateObj("Item Background", item);
            GameObject itemCheckmark = CreateObj("Item Checkmark", item);
            GameObject itemLabel = CreateObj("Item Label", item);

            // Sub controls.

            GameObject scrollbar = CreateScrollbar();
            scrollbar.name = "Scrollbar";
            SetParentAndAlign(scrollbar, template);

            ScrollbarExpand scrollbarScrollbar = scrollbar.GetComponent<ScrollbarExpand>();
            scrollbarScrollbar.SetDirection(Scrollbar.Direction.BottomToTop, true);

            RectTransform vScrollbarRT = scrollbar.GetComponent<RectTransform>();
            vScrollbarRT.anchorMin = Vector2.right;
            vScrollbarRT.anchorMax = Vector2.one;
            vScrollbarRT.pivot = Vector2.one;
            vScrollbarRT.sizeDelta = new Vector2(vScrollbarRT.sizeDelta.x, 0);

            // Setup item UI components.

            TextExpand itemLabelText = itemLabel.AddComponent<TextExpand>();
            itemLabelText.alignment = TextAnchor.MiddleLeft;
            itemLabelText.color = Color.black;

            ImageExpand itemBackgroundImage = itemBackground.AddComponent<ImageExpand>();
            itemBackgroundImage.color = new Color32(245, 245, 245, 255);

            ImageExpand itemCheckmarkImage = itemCheckmark.AddComponent<ImageExpand>();
            itemCheckmarkImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(kCheckmarkPath);

            ToggleExpand itemToggle = item.AddComponent<ToggleExpand>();
            itemToggle.targetGraphic = itemBackgroundImage;
            itemToggle.graphic = itemCheckmarkImage;
            itemToggle.isOn = true;

            // Setup template UI components.

            ImageExpand templateImage = template.AddComponent<ImageExpand>();
            templateImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(kStandardSpritePath);
            templateImage.type = Image.Type.Sliced;

            ScrollRect templateScrollRect = template.AddComponent<ScrollRect>();
            templateScrollRect.content = (RectTransform)content.transform;
            templateScrollRect.viewport = (RectTransform)viewport.transform;
            templateScrollRect.horizontal = false;
            templateScrollRect.movementType = ScrollRect.MovementType.Clamped;
            templateScrollRect.verticalScrollbar = scrollbarScrollbar;
            templateScrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            templateScrollRect.verticalScrollbarSpacing = -3;

            Mask scrollRectMask = viewport.AddComponent<Mask>();
            scrollRectMask.showMaskGraphic = false;

            ImageExpand viewportImage = viewport.AddComponent<ImageExpand>();
            viewportImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UIMask.psd");
            viewportImage.type = Image.Type.Sliced;

            // Setup dropdown UI components.

            TextExpand labelText = label.AddComponent<TextExpand>();

            labelText.text = "Option A";
            labelText.alignment = TextAnchor.MiddleLeft;

            ImageExpand arrowImage = arrow.AddComponent<ImageExpand>();
            arrowImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/DropdownArrow.psd");//UI/Skin/DropdownArrow.psd

            ImageExpand backgroundImage = root.AddComponent<ImageExpand>();
            backgroundImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(kStandardSpritePath);
            backgroundImage.color = whiteColor;
            backgroundImage.type = Image.Type.Sliced;

            DropDownExpand dropdown = root.AddComponent<DropDownExpand>();
            dropdown.targetGraphic = backgroundImage;

            ColorBlock colors = dropdown.colors;
            colors.highlightedColor = new Color(0.882f, 0.882f, 0.882f);
            colors.pressedColor = new Color(0.698f, 0.698f, 0.698f);
            colors.disabledColor = new Color(0.521f, 0.521f, 0.521f);

            dropdown.template = template.GetComponent<RectTransform>();
            dropdown.captionText = labelText;
            dropdown.itemText = itemLabelText;

            // Setting default Item list.
            //itemLabelText.text = "Option A";

            // Set up RectTransforms.

            RectTransform labelRT = label.GetComponent<RectTransform>();
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = new Vector2(10, 6);
            labelRT.offsetMax = new Vector2(-25, -7);

            RectTransform arrowRT = arrow.GetComponent<RectTransform>();
            arrowRT.anchorMin = new Vector2(1, 0.5f);
            arrowRT.anchorMax = new Vector2(1, 0.5f);
            arrowRT.sizeDelta = new Vector2(20, 20);
            arrowRT.anchoredPosition = new Vector2(-15, 0);

            RectTransform templateRT = template.GetComponent<RectTransform>();
            templateRT.anchorMin = new Vector2(0, 0);
            templateRT.anchorMax = new Vector2(1, 0);
            templateRT.pivot = new Vector2(0.5f, 1);
            templateRT.anchoredPosition = new Vector2(0, 2);
            templateRT.sizeDelta = new Vector2(0, 150);

            RectTransform viewportRT = viewport.GetComponent<RectTransform>();
            viewportRT.anchorMin = new Vector2(0, 0);
            viewportRT.anchorMax = new Vector2(1, 1);
            viewportRT.sizeDelta = new Vector2(-18, 0);
            viewportRT.pivot = new Vector2(0, 1);

            RectTransform contentRT = content.GetComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0f, 1);
            contentRT.anchorMax = new Vector2(1f, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.anchoredPosition = new Vector2(0, 0);
            contentRT.sizeDelta = new Vector2(0, 28);

            RectTransform itemRT = item.GetComponent<RectTransform>();
            itemRT.anchorMin = new Vector2(0, 0.5f);
            itemRT.anchorMax = new Vector2(1, 0.5f);
            itemRT.sizeDelta = new Vector2(0, 20);

            RectTransform itemBackgroundRT = itemBackground.GetComponent<RectTransform>();
            itemBackgroundRT.anchorMin = Vector2.zero;
            itemBackgroundRT.anchorMax = Vector2.one;
            itemBackgroundRT.sizeDelta = Vector2.zero;

            RectTransform itemCheckmarkRT = itemCheckmark.GetComponent<RectTransform>();
            itemCheckmarkRT.anchorMin = new Vector2(0, 0.5f);
            itemCheckmarkRT.anchorMax = new Vector2(0, 0.5f);
            itemCheckmarkRT.sizeDelta = new Vector2(20, 20);
            itemCheckmarkRT.anchoredPosition = new Vector2(10, 0);

            RectTransform itemLabelRT = itemLabel.GetComponent<RectTransform>();
            itemLabelRT.anchorMin = Vector2.zero;
            itemLabelRT.anchorMax = Vector2.one;
            itemLabelRT.offsetMin = new Vector2(20, 1);
            itemLabelRT.offsetMax = new Vector2(-10, -2);

            template.SetActive(false);

            //ClearRays(dropdown);
        }

        [MenuItem(root+"Slider",false,5)]
		public static void CreateSlider(MenuCommand command )
		{
			GameObject go = CreateUIElementRoot("Slider", command, s_ThinGUIElementSize);

			GameObject background = CreateObj("Background", go);
			GameObject fillArea = CreateObj("Fill Area", go);
			GameObject fill = CreateObj("Fill", fillArea);
			GameObject handleArea = CreateObj("Handle Slide Area", go);
			GameObject handle = CreateObj("Handle", handleArea);
			
			// Background
            var backgroundimg = background.TryAddComponent<ImageExpand>();
            backgroundimg.sprite =  AssetDatabase.GetBuiltinExtraResource<Sprite>(kBackgroundSpriteResourcePath);
            backgroundimg.type = Image.Type.Sliced;
            backgroundimg.color = Color.white;

            RectTransform backgroundRect = background.GetComponent<RectTransform>();
			backgroundRect.anchorMin = new Vector2(0, 0.25f);
			backgroundRect.anchorMax = new Vector2(1, 0.75f);
			backgroundRect.sizeDelta = new Vector2(0, 0);
			
			// Fill Area
			RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
			fillAreaRect.anchorMin = new Vector2(0, 0.25f);
			fillAreaRect.anchorMax = new Vector2(1, 0.75f);
			fillAreaRect.anchoredPosition = new Vector2(-5, 0);
			fillAreaRect.sizeDelta = new Vector2(-20, 0);

            // Fill
            ImageExpand fillImage = fill.AddComponent<ImageExpand>();
            fillImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(kStandardSpritePath);
            fillImage.type = Image.Type.Sliced;
            fillImage.color = Color.white;

            RectTransform fillRect = fill.GetComponent<RectTransform>();
			fillRect.sizeDelta = new Vector2(10, 0);
			
			// Handle Area
			RectTransform handleAreaRect = handleArea.GetComponent<RectTransform>();
			handleAreaRect.sizeDelta = new Vector2(-20, 0);
			handleAreaRect.anchorMin = new Vector2(0, 0);
			handleAreaRect.anchorMax = new Vector2(1, 1);

            // Handle
            ImageExpand handleImage = handle.TryAddComponent<ImageExpand>();
            fillImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(kKnobPath);
            fillImage.type = Image.Type.Sliced;
            fillImage.color = Color.white;
            RectTransform handleRect = handle.GetComponent<RectTransform>();
			handleRect.sizeDelta = new Vector2(20, 0);

            // Setup slider component
            SliderExpand slider = go.AddComponent<SliderExpand>();
            ClearRays(slider);
            slider.fillRect = fill.GetComponent<RectTransform>();
			slider.handleRect = handle.GetComponent<RectTransform>();
			slider.targetGraphic = handleImage;
			slider.direction = Slider.Direction.LeftToRight;
			ColorBlock colors = slider.colors;
			colors.highlightedColor = new Color(0.882f, 0.882f, 0.882f);
			colors.pressedColor     = new Color(0.698f, 0.698f, 0.698f);
			colors.disabledColor    = new Color(0.521f, 0.521f, 0.521f);
			slider.colors = colors;

            //handleImage

            handleImage.raycastTarget = true;
        }

        [MenuItem(root+"Scrollbar",false,5)]
		public static void CreateScrollbar(MenuCommand command)
		{
			GameObject go = CreateUIElementRoot("Scrollbar",command, s_ThinGUIElementSize);

			GameObject sliderArea = CreateObj("Sliding Area", go);
			GameObject handle = CreateObj("Handle", sliderArea);
			
			ImageExpand bgImage = go.AddComponent<ImageExpand>();
			bgImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(kBackgroundSpriteResourcePath);
			bgImage.type = Image.Type.Sliced;
			bgImage.color =whiteColor;
            ClearRays(bgImage);

            ImageExpand handleImage = handle.AddComponent<ImageExpand>();
			handleImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(kStandardSpritePath);
			handleImage.type = Image.Type.Sliced;
			handleImage.color =  Color.white;
            ClearRays(handleImage);

            RectTransform sliderAreaRect = sliderArea.GetComponent<RectTransform>();
			sliderAreaRect.sizeDelta = new Vector2(-20, -20);
			sliderAreaRect.anchorMin = Vector2.zero;
			sliderAreaRect.anchorMax = Vector2.one;
			
			RectTransform handleRect = handle.GetComponent<RectTransform>();
			handleRect.sizeDelta = new Vector2(20, 20);

            ScrollbarExpand scrollbar = go.AddComponent<ScrollbarExpand>();
			scrollbar.handleRect = handleRect;
			scrollbar.targetGraphic = handleImage;
            ClearRays(scrollbar);

            ColorBlock colors = scrollbar.colors;
			colors.highlightedColor = new Color(0.882f, 0.882f, 0.882f);
			colors.pressedColor     = new Color(0.698f, 0.698f, 0.698f);
			colors.disabledColor    = new Color(0.521f, 0.521f, 0.521f);
			scrollbar.colors = colors;
		}

		[MenuItem(root+"InputField",false,5)]
		public static void CreateInputField(MenuCommand command)
		{
			GameObject go = CreateUIElementRoot("InputField",command,s_ThickGUIElementSize);
			GameObject childPlaceholder = CreateObj("Placeholder", go);
			GameObject childText = CreateObj("Text", go);
			
			ImageExpand image = go.AddComponent<ImageExpand>();
			image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(kInputFieldBackgroundPath);
			image.type = Image.Type.Sliced;
			image.color = Color.white;

            InputExpand inputField = go.AddComponent<InputExpand>();

            ColorBlock colors = inputField.colors;
			colors.highlightedColor = new Color(0.882f, 0.882f, 0.882f);
			colors.pressedColor     = new Color(0.698f, 0.698f, 0.698f);
			colors.disabledColor    = new Color(0.521f, 0.521f, 0.521f);
			inputField.colors = colors;
			
			TextExpand text = childText.AddComponent<TextExpand>();
			text.text = "";
			text.supportRichText = false;
			text.color =new Color(50f / 255f, 50f / 255f, 50f / 255f, 1f);

            TextExpand placeholder = childPlaceholder.AddComponent<TextExpand>();
			placeholder.text = "Kubility";
			placeholder.fontStyle = FontStyle.Italic;

            // Make placeholder color half as opaque as normal text color.
            Color placeholderColor = text.color;
			placeholderColor.a *= 0.5f;
			placeholder.color = placeholderColor;
			
			RectTransform textRectTransform = childText.GetComponent<RectTransform>();
			textRectTransform.anchorMin = Vector2.zero;
			textRectTransform.anchorMax = Vector2.one;
			textRectTransform.sizeDelta = Vector2.zero;
			textRectTransform.offsetMin = new Vector2(10, 6);
			textRectTransform.offsetMax = new Vector2(-10, -7);
			
			RectTransform placeholderRectTransform = childPlaceholder.GetComponent<RectTransform>();
			placeholderRectTransform.anchorMin = Vector2.zero;
			placeholderRectTransform.anchorMax = Vector2.one;
			placeholderRectTransform.sizeDelta = Vector2.zero;
			placeholderRectTransform.offsetMin = new Vector2(10, 6);
			placeholderRectTransform.offsetMax = new Vector2(-10, -7);
			
			inputField.textComponent = text;
			inputField.placeholder = placeholder;
		}

        [MenuItem(root+"Scroll View",false,5)]
		public static void CreateScrollView(MenuCommand command)
		{
			GameObject go =  CreateObj("Scroll View",Selection.activeObject);

			GameObject viewport = CreateObj("Viewport",go);
			GameObject content= CreateObj("Content",viewport);

			GameObject Hor_Scrollbar = CreateObj("Hor_Scrollbar",go);
			GameObject HS_sliderArea = CreateObj("Sliding Area",Hor_Scrollbar);
			GameObject HS_handle =CreateObj("Handle",HS_sliderArea);

			GameObject Ver_Scrollbar = CreateObj("Ver_Scrollbar",go);
			GameObject VS_sliderArea =CreateObj("Slideing Area",Ver_Scrollbar);
			GameObject VS_handle = CreateObj("Handle",VS_sliderArea);

			RectTransform TotalRect = go.GetComponent<RectTransform>();
			TotalRect.sizeDelta = new Vector2(200,200);

			RectTransform viewRect =  SetRectTransForm(viewport,new Vector2(0,0),new Vector2(1,1),new Vector2(0,1));
			viewRect.sizeDelta = new Vector2(-17,-17);
			//content

			RectTransform conRect = content.GetComponent<RectTransform>();
			conRect.sizeDelta = new Vector2(0,300);
			conRect.anchorMin = new Vector2(0,1);
			conRect.anchorMax = new Vector2(1,1);
			conRect.pivot = new Vector2(0,1);

            //Hor scorllbar
            ScrollbarExpand hbar = Hor_Scrollbar.AddComponent<ScrollbarExpand>();
            ClearRays(hbar);
            AddImage(Hor_Scrollbar,Color.white);

			ImageExpand hs_hanlder_image = AddImage(HS_handle,Color.white,kStandardSpritePath);
            ClearRays(hs_hanlder_image);
            hbar.targetGraphic = hs_hanlder_image;
			hbar.handleRect =HS_handle.GetComponent<RectTransform>();

			var hbarRect = SetRectTransForm(Hor_Scrollbar,new Vector2(0,0),new Vector2(1,0),new Vector2(0,0));
			var hbarArea = SetRectTransForm(HS_sliderArea,new Vector2(0,0),new Vector2(1,1),new Vector2(0.5f,0.5f));
			var hsRect = SetRectTransForm(HS_handle,new Vector2(0,0),new Vector2(1,1),new Vector2(0.5f,0.5f));
			hbarRect.sizeDelta = new Vector2(-17,20);
			hbarArea.sizeDelta = new Vector2(-20,-20);
			hsRect.sizeDelta = new Vector2(20,20);

            //Ver scorllbar
            ScrollbarExpand vbar =Ver_Scrollbar.AddComponent<ScrollbarExpand>();
			vbar.direction = Scrollbar.Direction.BottomToTop;
            ClearRays(vbar);

            AddImage(Ver_Scrollbar,Color.white);
			vbar.handleRect =VS_handle.GetComponent<RectTransform>();

			ImageExpand vs_hanlder_image = AddImage(VS_handle,Color.white,kStandardSpritePath);
			vbar.targetGraphic = vs_hanlder_image;
            ClearRays(vs_hanlder_image);

            var vsbarRect = SetRectTransForm(Ver_Scrollbar,new Vector2(1,0),new Vector2(1,1),new Vector2(1,1));
			var vsbarArea =SetRectTransForm(VS_sliderArea,new Vector2(0,0),new Vector2(1,1),new Vector2(0.5f,0.5f));
			var vsRect= SetRectTransForm(VS_handle,new Vector2(0,0),new Vector2(1,1),new Vector2(0.5f,0.5f));
			vsbarRect.sizeDelta = new Vector2(20,-17);
			vsbarArea.sizeDelta = new Vector2(-20,-20);
			vsRect.sizeDelta = new Vector2(20,20);
            //scroll
            ScrollViewExpand scroll = go.AddComponent<ScrollViewExpand>();
			scroll.horizontalScrollbar =hbar;
			scroll.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
			scroll.horizontalScrollbarSpacing =-3;

			scroll.verticalScrollbar = vbar;
			scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
			scroll.verticalScrollbarSpacing  =- 3;

			scroll.content =content.GetComponent<RectTransform>();
            ClearRays(scroll);

            ImageExpand image = go.AddComponent<ImageExpand>();
			image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(kBackgroundSpriteResourcePath);
			image.color = whiteColor;
			image.type = Image.Type.Sliced;
			image.fillCenter =true;
            ClearRays(image);

            viewport.AddComponent<Mask>().showMaskGraphic =false;
			ImageExpand vimage = viewport.AddComponent<ImageExpand>();
			vimage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(kBackgroundSpriteResourcePath);
			vimage.color = whiteColor;
			vimage.type = Image.Type.Sliced;
			vimage.fillCenter = true;
            ClearRays(vimage);

            scroll.viewport = viewRect;
            PlaceUIElementRoot(go, command);
        }

		[MenuItem(root+"Panel",false,5)]
		public static void CreatePanel(MenuCommand menuCommand)
		{
			GameObject go = CreateUIElementRoot("Panel", s_ThickGUIElementSize);

            ImageExpand image = go.AddComponent<ImageExpand>();
			image.type = Image.Type.Sliced;
			image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(kBackgroundSpriteResourcePath);
			image.fillCenter =true;
			image.color = Color.white;
            ClearRays(image);

            RectTransform rectTransform = go.GetComponent<RectTransform>();
			rectTransform.anchorMin = Vector2.zero;
			rectTransform.anchorMax = Vector2.one;
			rectTransform.anchoredPosition = Vector2.zero;
			rectTransform.sizeDelta = Vector2.zero;

            PlaceUIElementRoot(go, menuCommand);

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;

        }
        #region from ugui
        private static GameObject CreateUIElementRoot(string name, Vector2 size)
        {
            GameObject child = new GameObject(name);
            RectTransform rectTransform = child.AddComponent<RectTransform>();
            rectTransform.sizeDelta = size;
            return child;
        }

        static public GameObject GetOrCreateCanvasGameObject()
		{
			GameObject selectedGo = Selection.activeGameObject;
			
			// Try to find a gameobject that is the selected GO or one if its parents.
			Canvas canvas = (selectedGo != null) ? selectedGo.GetComponentInParent<Canvas>() : null;
			if (canvas != null && canvas.gameObject.activeInHierarchy)
				return canvas.gameObject;
			
			// No canvas in selection or its parents? Then use just any canvas..
			canvas = Object.FindObjectOfType(typeof(Canvas)) as Canvas;
			if (canvas != null && canvas.gameObject.activeInHierarchy)
				return canvas.gameObject;
			
			// No canvas in the scene at all? Then create a new one.
			var root = new GameObject("Canvas");
			root.layer = LayerMask.NameToLayer("UI");
			canvas = root.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			root.AddComponent<CanvasScaler>();
			root.AddComponent<GraphicRaycaster>();
			Undo.RegisterCreatedObjectUndo(root, "Create " + root.name);
			
			// if there is no event system add one...
			CreateEventSystem(false);
			return root;
		}

		private static void CreateEventSystem(bool select)
		{
			CreateEventSystem(select, null);
		}
		
		private static void CreateEventSystem(bool select, GameObject parent)
		{
			var esys = Object.FindObjectOfType<EventSystem>();
			if (esys == null)
			{
				var eventSystem = new GameObject("EventSystem");
				GameObjectUtility.SetParentAndAlign(eventSystem, parent);
				esys = eventSystem.AddComponent<EventSystem>();
				eventSystem.AddComponent<StandaloneInputModule>();
				//eventSystem.AddComponent<TouchInputModule>();
				
				Undo.RegisterCreatedObjectUndo(eventSystem, "Create " + eventSystem.name);
			}
			
			if (select && esys != null)
			{
				Selection.activeGameObject = esys.gameObject;
			}
		}

        private static void PlaceUIElementRoot(GameObject element, MenuCommand menuCommand)
        {
            GameObject parent = menuCommand.context as GameObject;
            if (parent == null || parent.GetComponentInParent<Canvas>() == null)
            {
                parent = GetOrCreateCanvasGameObject();
            }

            string uniqueName = GameObjectUtility.GetUniqueNameForSibling(parent.transform, element.name);
            element.name = uniqueName;
            Undo.RegisterCreatedObjectUndo(element, "Create " + element.name);
            Undo.SetTransformParent(element.transform, parent.transform, "Parent " + element.name);
            GameObjectUtility.SetParentAndAlign(element, parent);
            if (parent != menuCommand.context) // not a context click, so center in sceneview
                SetPositionVisibleinSceneView(parent.GetComponent<RectTransform>(), element.GetComponent<RectTransform>());

            Selection.activeGameObject = element;
        }

        static GameObject CreateUIElementRoot(string name, MenuCommand menuCommand, Vector2 size)
		{
			GameObject parent = menuCommand.context as GameObject;
			if (parent == null || parent.GetComponentInParent<Canvas>() == null)
			{
				parent = GetOrCreateCanvasGameObject();
			}
			GameObject child = new GameObject(name);
			
			Undo.RegisterCreatedObjectUndo(child, "Create " + name);
			Undo.SetTransformParent(child.transform, parent.transform, "Parent " + child.name);
			GameObjectUtility.SetParentAndAlign(child, parent);
			
			RectTransform rectTransform = child.AddComponent<RectTransform>();
			rectTransform.sizeDelta = size;
			if (parent != menuCommand.context) // not a context click, so center in sceneview
			{
				SetPositionVisibleinSceneView(parent.GetComponent<RectTransform>(), rectTransform);
			}
			Selection.activeGameObject = child;
			return child;
		}

		private static void SetPositionVisibleinSceneView(RectTransform canvasRTransform, RectTransform itemTransform)
		{
			// Find the best scene view
			SceneView sceneView = SceneView.lastActiveSceneView;
			if (sceneView == null && SceneView.sceneViews.Count > 0)
				sceneView = SceneView.sceneViews[0] as SceneView;
			
			// Couldn't find a SceneView. Don't set position.
			if (sceneView == null || sceneView.camera == null)
				return;
			
			// Create world space Plane from canvas position.
			Vector2 localPlanePosition;
			Camera camera = sceneView.camera;
			Vector3 position = Vector3.zero;
			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRTransform, new Vector2(camera.pixelWidth / 2, camera.pixelHeight / 2), camera, out localPlanePosition))
			{
				// Adjust for canvas pivot
				localPlanePosition.x = localPlanePosition.x + canvasRTransform.sizeDelta.x * canvasRTransform.pivot.x;
				localPlanePosition.y = localPlanePosition.y + canvasRTransform.sizeDelta.y * canvasRTransform.pivot.y;
				
				localPlanePosition.x = Mathf.Clamp(localPlanePosition.x, 0, canvasRTransform.sizeDelta.x);
				localPlanePosition.y = Mathf.Clamp(localPlanePosition.y, 0, canvasRTransform.sizeDelta.y);
				
				// Adjust for anchoring
				position.x = localPlanePosition.x - canvasRTransform.sizeDelta.x * itemTransform.anchorMin.x;
				position.y = localPlanePosition.y - canvasRTransform.sizeDelta.y * itemTransform.anchorMin.y;
				
				Vector3 minLocalPosition;
				minLocalPosition.x = canvasRTransform.sizeDelta.x * (0 - canvasRTransform.pivot.x) + itemTransform.sizeDelta.x * itemTransform.pivot.x;
				minLocalPosition.y = canvasRTransform.sizeDelta.y * (0 - canvasRTransform.pivot.y) + itemTransform.sizeDelta.y * itemTransform.pivot.y;
				
				Vector3 maxLocalPosition;
				maxLocalPosition.x = canvasRTransform.sizeDelta.x * (1 - canvasRTransform.pivot.x) - itemTransform.sizeDelta.x * itemTransform.pivot.x;
				maxLocalPosition.y = canvasRTransform.sizeDelta.y * (1 - canvasRTransform.pivot.y) - itemTransform.sizeDelta.y * itemTransform.pivot.y;
				
				position.x = Mathf.Clamp(position.x, minLocalPosition.x, maxLocalPosition.x);
				position.y = Mathf.Clamp(position.y, minLocalPosition.y, maxLocalPosition.y);
			}
			
			itemTransform.anchoredPosition = position;
			itemTransform.localRotation = Quaternion.identity;
			itemTransform.localScale = Vector3.one;
		}

		#endregion

		static RectTransform SetRectTransForm(GameObject go, Vector2 min ,Vector2 max,Vector2 pivot )
		{
			RectTransform rect = go.GetComponent<RectTransform>();
			rect.anchorMin = min;
			rect.anchorMax = max;
			rect.pivot = pivot;
			return rect;
		}

		static GameObject CreateObj(string name ,UnityEngine.Object parent)
		{
			GameObject obj = new GameObject(name);
			obj.AddComponent<RectTransform>();
			SetParentAndAlign(obj,parent as GameObject);
			return obj;
		}

		static ImageExpand AddImage(GameObject go,Color col ,string path =kBackgroundSpriteResourcePath)
		{
			ImageExpand image = go.AddComponent<ImageExpand>();
			image.type = Image.Type.Tiled;
			image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(path);
			image.fillCenter =true;
			image.color = col;
            ClearRays(image);
            return image;
		}

		static void SetParentAndAlign(GameObject child,GameObject parent)
		{
			GameObjectUtility.SetParentAndAlign(child, parent);
			
			Undo.RegisterCreatedObjectUndo(child, "Create " + child.name);
		}
	}
}


