
using UnityEngine;
using UnityEngine.UIElements;
using ActionTree.Data;



namespace ActionTree.Editor
{
	public class BaseNodeView : VisualElement
	{
		public BaseNodeData Node { get; }

		protected VisualElement Header;
		protected VisualElement Body;
		protected Label TitleLabel;
		protected Label SubtitleLabel;

		protected const float HeaderHeight = 50f;


		public event System.Action<BaseNodeView> Clicked;


		public bool IsHoverParent { get; set; }
		public bool IsConnectionAvailable { get; set; } = true;
		public BaseNodeView(BaseNodeData node)
		{
			Node = node;

			style.position = Position.Absolute;
			style.flexDirection = FlexDirection.Column;

			style.backgroundColor =
				new Color(0.23f, 0.23f, 0.23f);

			style.borderTopWidth = 1;
			style.borderBottomWidth = 1;
			style.borderLeftWidth = 1;
			style.borderRightWidth = 1;

			style.borderTopColor = Color.black;
			style.borderBottomColor = Color.black;
			style.borderLeftColor = Color.black;
			style.borderRightColor = Color.black;

			CreateHeader();
			CreateBody();

			Add(Header);
			Add(Body);
		}

		private void CreateHeader()
		{
			Header = new VisualElement();

			Header.style.height = 24;

			Header.style.flexDirection = FlexDirection.Row;

			Header.style.alignItems = Align.Center;

			Header.style.backgroundColor =
				new Color(0.18f, 0.18f, 0.18f);

			Header.style.paddingLeft = 6;

			TitleLabel = new Label(Node.Title);

			TitleLabel.style.unityFontStyleAndWeight =
				FontStyle.Bold;

			Header.Add(TitleLabel);
		}

		private void CreateBody()
		{
			Body = new VisualElement();

			Body.style.flexGrow = 1;

			Body.style.justifyContent = Justify.Center;
			Body.style.alignItems = Align.Center;

			SubtitleLabel = new Label(Node.Subtitle);

			SubtitleLabel.style.fontSize = 14;
			SubtitleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;

			Body.Add(SubtitleLabel);
		}

		private void OnHeaderClicked(MouseDownEvent evt)
		{
			if (evt.button != 0)
				return;

			Node.IsExpanded = !Node.IsExpanded;

			Clicked?.Invoke(this);
			
			

		}



		//private void RefreshCommentIcon()
		//{
		//	bool hasComment =
		//		!string.IsNullOrWhiteSpace(Node.Comment);

		//	//CommentIcon.style.display =
		//	//	hasComment
		//	//	? DisplayStyle.Flex
		//	//	: DisplayStyle.None;

		//	//CommentIcon.tooltip = Node.Comment;
		//}
		public virtual Rect GetDropZone(GraphViewport viewport)
		{
			const float Height = 40f;

			Vector2 screen = viewport.WorldToScreen(Node.Position);

			return new Rect(
				screen.x,
				screen.y + Node.Height * viewport.Zoom,
				Node.Width * viewport.Zoom,
				Height);
		}
		public virtual void Refresh(GraphViewport viewport)
		{
			float zoom = viewport.Zoom;

			Vector2 screen = viewport.WorldToScreen(Node.Position);

			style.left = screen.x;
			style.top = screen.y;

			style.width = Node.Width * zoom;
			style.height = Node.Height * zoom;

			RefreshZoom(zoom);

			TitleLabel.text = Node.Title;
			SubtitleLabel.text = Node.Subtitle;

			style.opacity = IsConnectionAvailable ? 1f : 0.3f;

			Color border = Color.black;

			if (Node.IsSelected)
				border = new Color(0.25f, 0.55f, 1f);
			else if (IsHoverParent)
				border = Color.green;

			style.borderTopColor = border;
			style.borderBottomColor = border;
			style.borderLeftColor = border;
			style.borderRightColor = border;
		}
		private void RefreshZoom(float zoom)
		{
			// Header
			Header.style.height = 24 * zoom;

			Header.style.paddingLeft = 6 * zoom;
			Header.style.paddingTop = 2 * zoom;
			Header.style.paddingBottom = 2 * zoom;

			// Body
			Body.style.height = (Node.Height - 24) * zoom;

			// 字體
			TitleLabel.style.fontSize = 14 * zoom;

			SubtitleLabel.style.fontSize = 12 * zoom;

			// Body 文字置中
			Body.style.justifyContent = Justify.Center;
			Body.style.alignItems = Align.Center;
		}
		public void SetSelected(bool selected)
		{
			Node.IsSelected = selected;

			Color color = selected
				? new Color(0.25f, 0.55f, 1f)
				: Color.black;

			style.borderTopColor = color;
			style.borderBottomColor = color;
			style.borderLeftColor = color;
			style.borderRightColor = color;
		}
		
	}
}