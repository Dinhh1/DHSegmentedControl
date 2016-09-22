using System;
using System.Collections.Generic;
using System.Linq;
using UIKit;
using Foundation;
using CoreAnimation;
using CoreGraphics;

namespace DH.Custom.SegmentedControl
{
	public class SegmentedScrollView : UIScrollView
	{
		public override void TouchesBegan(NSSet touches, UIEvent evt)
		{
			if (!Dragging)
				NextResponder.TouchesBegan(touches, evt);
			else
				base.TouchesBegan(touches, evt);
		}

		public override void TouchesMoved(NSSet touches, UIEvent evt)
		{
			if (!Dragging)
				NextResponder.TouchesMoved(touches, evt);
			else
				base.TouchesMoved(touches, evt);
		}

		public override void TouchesEnded(NSSet touches, UIEvent evt)
		{
			if (!Dragging)
				NextResponder.TouchesEnded(touches, evt);
			else
				base.TouchesEnded(touches, evt);
		}
	}

	public enum DHSegmentedControlType
	{
		Text
	}

	public enum DHSegmentedControlSelectionStyle
	{
		TextWidthStripe,
		FullWidthStripe,
		Box
	}

	public enum DHSegmentedControlLocation
	{
		Up,
		Down,
		None
	}

	public enum DHSegmentedControlWidthStyle
	{
		Fixed,
		Dynamic
	}

	public enum DHSegmentedControlBorderType
	{
		None,
		Top,
		Left,
		Bottom,
		Right
	}

	public class DHSegmentedControl : UIControl
	{
		private DHSegmentedControlType type;
		private SegmentedScrollView scrollView;

		private List<string> sectionTitles;
		private List<float> segmentWidths;
		private float segmentWidth;
		private float borderWidth;

		private int previousIndex;

		private List<UILabel> titleLabels;

		private UIEdgeInsets selectionIndicatorEdgeInsets;
		private DHSegmentedControlWidthStyle segmentWidthStyle;
		private DHSegmentedControlBorderType borderType;
		private DHSegmentedControlLocation selectionIndicatorLocation;
		private float selectionIndicatorBoxOpacity;

		public Func<DHSegmentedControl, string, int, bool, NSAttributedString> TitleFormatter { get; set; }
		public EventHandler<int> IndexChange;

		public UIColor BorderColor { get; set; }
		public bool TouchEnabled { get; set; }
		public bool UserDraggable { get; set; }
		public int SelectedIndex { get; set; }
		public UIFont Font { get; set; }
		public UIFont SelectedFont { get; set; }
		public UIColor TextColor { get; set; }
		public UIColor SelectedTextColor { get; set; }
		public UIColor SelectionIndicatorColor { get; set; }
		public float SelectionIndicatorHeight { get; set; }
		public UIColor VerticalDividerColor { get; set; }
		public bool VerticalDividerEnabled { get; set; }
		public float VerticalDividerWidth { get; set; }
		public DHSegmentedControlSelectionStyle SelectionStyle { get; set; }
		public UIEdgeInsets SegmentEdgeInset { get; set; }
		public UIEdgeInsets LabelMargins { get; set; }
		public CALayer SelectionIndicatorBoxLayer { get; set; }
		public CALayer SelectionIndicatorArrowLayer { get; set; }
		public CALayer SelectionIndicatorStripLayer { get; set; }
		public bool ShouldAnimateUserSelection { get; set; }

		public DHSegmentedControlBorderType BorderType
		{
			get { return borderType; }
			set
			{
				borderType = value;
				SetNeedsDisplay();
			}
		}

		public DHSegmentedControlLocation SelectionIndicatorLocation
		{
			get { return selectionIndicatorLocation; }
			set
			{
				selectionIndicatorLocation = value;
				if (value == DHSegmentedControlLocation.None)
					SelectionIndicatorHeight = 0.0f;
			}
		}

		public float SelectionIndicatorBoxOpacity
		{
			get { return selectionIndicatorBoxOpacity; }
			set
			{
				selectionIndicatorBoxOpacity = value;
				SelectionIndicatorBoxLayer.Opacity = value;
			}
		}

		public DHSegmentedControlWidthStyle SegmentWidthStyle
		{
			get { return segmentWidthStyle; }
			set
			{
				segmentWidthStyle = value;
			}
		}

		public DHSegmentedControl(IEnumerable<string> sectionTitles)
		{
			Initialize();
			this.sectionTitles = new List<string>(sectionTitles);
			this.titleLabels = new List<UILabel>();
			foreach (var title in sectionTitles)
			{
				var titleLabel = new UILabel(CGRect.Empty)
				{
					TextAlignment = UITextAlignment.Center,
					Lines = 0,
					LineBreakMode = UILineBreakMode.TailTruncation,
					AdjustsFontSizeToFitWidth = true,
					AdjustsLetterSpacingToFitWidth = true,
					MinimumScaleFactor = 0.5f
				};
				this.titleLabels.Add(titleLabel);

				//scrollView.AddSubview(titleLabel);
			}

			type = DHSegmentedControlType.Text;
		}


		private void Initialize()
		{
			previousIndex = -1;
			scrollView = new SegmentedScrollView { ScrollsToTop = false, ShowsVerticalScrollIndicator = false, ShowsHorizontalScrollIndicator = false };
			AddSubview(scrollView);

			Opaque = false;
			SelectionIndicatorColor = UIColor.FromRGBA(52.0f / 255.0f, 181.0f / 255.0f, 229.0f / 255.0f, 1.0f);

			SelectedIndex = 0;
			SegmentEdgeInset = new UIEdgeInsets(0, 0, 0, 0);

			LabelMargins = new UIEdgeInsets(4, 8, 4, 8);

			SelectionIndicatorHeight = 5.0f;
			selectionIndicatorEdgeInsets = new UIEdgeInsets(0, 0, 0, 0);
			SelectionStyle = DHSegmentedControlSelectionStyle.TextWidthStripe;
			SelectionIndicatorLocation = DHSegmentedControlLocation.Up;
			segmentWidthStyle = DHSegmentedControlWidthStyle.Fixed;
			UserDraggable = true;
			TouchEnabled = true;
			VerticalDividerEnabled = false;
			VerticalDividerColor = UIColor.Black;
			BorderColor = UIColor.Black;
			borderWidth = 1.0f;

			ShouldAnimateUserSelection = true;
			selectionIndicatorBoxOpacity = 0.2f;
			SelectionIndicatorArrowLayer = new DisposableCALayer();
			SelectionIndicatorStripLayer = new DisposableCALayer();
			SelectionIndicatorBoxLayer = new DisposableCALayer { Opacity = selectionIndicatorBoxOpacity, BorderWidth = 1.0f };

			ContentMode = UIViewContentMode.Redraw;
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			UpdateSegmentRects();
		}

		public override CGRect Frame
		{
			get { return base.Frame; }
			set
			{
				base.Frame = value;
				if (value != CGRect.Empty)
					UpdateSegmentRects();
			}
		}

		public void SetSelectionIndicatorLocation(DHSegmentedControlLocation value)
		{
			SelectionIndicatorLocation = value;
			if (value == DHSegmentedControlLocation.None)
				SelectionIndicatorHeight = 0.0f;
		}

		#region Drawing

		private CGSize MeasureTitle(int index)
		{
			var title = sectionTitles[index];
			var size = CGSize.Empty;
			var selected = index == SelectedIndex;

			if (TitleFormatter == null)
			{
				var nsTitle = new NSString(title);
				var stringAttributes = selected ? GetSelectedTitleTextAttributes() : GetTitleTextAttributes();
				size = new Version(UIDevice.CurrentDevice.SystemVersion).Major < 7 ? nsTitle.StringSize(stringAttributes.Font) : nsTitle.GetSizeUsingAttributes(stringAttributes);
			}
			else
			{
				size = TitleFormatter(this, title, index, selected).Size;
			}

			return size;
		}

		private NSAttributedString AttributedTitle(int index)
		{
			var title = sectionTitles[index];
			var selected = index == SelectedIndex;

			return TitleFormatter != null
					? TitleFormatter(this, title, index, selected)
					: new NSAttributedString(title, selected ? GetSelectedTitleTextAttributes() : GetTitleTextAttributes());
		}

		public override void Draw(CGRect rect)
		{
			if (BackgroundColor != null)
				BackgroundColor.SetFill();

			SelectionIndicatorArrowLayer.BackgroundColor = SelectionIndicatorColor.CGColor;
			SelectionIndicatorStripLayer.BackgroundColor = SelectionIndicatorColor.CGColor;

			SelectionIndicatorBoxLayer.BackgroundColor = SelectionIndicatorColor.CGColor;
			SelectionIndicatorBoxLayer.BorderColor = SelectionIndicatorColor.CGColor;

			scrollView.Layer.Sublayers = new DisposableCALayer[0];
			ClearScrollViewSubLayers();
			var oldRect = rect;

			if (type == DHSegmentedControlType.Text)
			{
				for (int idx = 0; idx < sectionTitles.Count; idx++)
				{
					var size = MeasureTitle(idx);
					var stringWidth = size.Width;
					var stringHeight = size.Height;
					CGRect newRect, rectDiv, rectFull;

					var locationUp = SelectionIndicatorLocation == DHSegmentedControlLocation.Up;
					var selectionStyleNotBox = SelectionStyle != DHSegmentedControlSelectionStyle.Box;

					var y = (float)Math.Round(((this.Frame.Height - (selectionStyleNotBox ? 1 : 0 * SelectionIndicatorHeight)) / 2) - (stringHeight / 2) + (SelectionIndicatorHeight * (locationUp ? 1 : 0)));

					if (segmentWidthStyle == DHSegmentedControlWidthStyle.Fixed && !UserDraggable)
					{
						stringWidth = segmentWidth - LabelMargins.Right - LabelMargins.Left;

						newRect = new CGRect((segmentWidth * idx) + (segmentWidth - stringWidth) / 2, 0, stringWidth, oldRect.Height - SelectionIndicatorHeight - LabelMargins.Top - LabelMargins.Bottom);
						rectDiv = new CGRect((segmentWidth * idx) + (VerticalDividerWidth / 2), SelectionIndicatorHeight * 2, VerticalDividerWidth, Frame.Size.Height - (SelectionIndicatorHeight * 4));
						rectFull = new CGRect(segmentWidth * idx, 0, segmentWidth, oldRect.Height - SelectionIndicatorHeight);
					}
					else if (segmentWidthStyle == DHSegmentedControlWidthStyle.Fixed && UserDraggable)
					{
						newRect = new CGRect((segmentWidth * idx) + (segmentWidth - stringWidth) / 2, y, stringWidth, stringHeight);
						rectDiv = new CGRect((segmentWidth * idx) + (VerticalDividerWidth / 2), SelectionIndicatorHeight * 2, VerticalDividerWidth, Frame.Size.Height - (SelectionIndicatorHeight * 4));
						rectFull = new CGRect(segmentWidth * idx, 0, segmentWidth, oldRect.Size.Height);
					}
					else
					{
						var xOffset = 0.0f;
						var i = 0;
						foreach (var width in segmentWidths)
						{
							if (idx == i)
								break;
							xOffset += width;
							i++;
						}

						var widthForIndex = segmentWidths[idx];
						newRect = new CGRect(xOffset, y, widthForIndex, stringHeight);
						rectFull = new CGRect(segmentWidth * idx, 0, widthForIndex, oldRect.Size.Height);
						rectDiv = new CGRect(xOffset - (VerticalDividerWidth / 2),
							SelectionIndicatorHeight * 2, VerticalDividerWidth, Frame.Size.Height - (SelectionIndicatorHeight * 4));
					}

					newRect = new CGRect((float)Math.Ceiling(newRect.X), (float)Math.Ceiling(newRect.Y), (float)Math.Ceiling(newRect.Size.Width), (float)Math.Ceiling(newRect.Size.Height));

					var titleLabel = titleLabels[idx];
					titleLabel.AttributedText = AttributedTitle(idx);
					titleLabel.Frame = newRect;
					titleLabel.SetNeedsLayout();
					scrollView.BringSubviewToFront(titleLabel);


					scrollView.AddSubview(titleLabel);

					if (VerticalDividerEnabled)
					{
						var verticalDividerLayer = new DisposableCALayer { Frame = rectDiv, BackgroundColor = VerticalDividerColor.CGColor };
						AddScrollViewSubLayer(verticalDividerLayer);
					}

					AddBackgroundAndBorderLayer(rectFull);
				}
			}


			if (SelectedIndex != -1)
			{

				if (SelectionIndicatorStripLayer.SuperLayer == null)
				{
					SelectionIndicatorStripLayer.Frame = FrameForSelectionIndicator();
					AddScrollViewSubLayer(SelectionIndicatorStripLayer);

					if (SelectionStyle == DHSegmentedControlSelectionStyle.Box && SelectionIndicatorBoxLayer.SuperLayer == null)
					{
						SelectionIndicatorBoxLayer.Frame = FrameForFillerSelectionIndicator();
						InsertScrollViewSubLayer(SelectionIndicatorBoxLayer, 0);
					}
				}
			}
		}

		private readonly List<CALayer> layers = new List<CALayer>();
		private void AddScrollViewSubLayer(CALayer layer)
		{
			scrollView.Layer.AddSublayer(layer);
			layers.Add(layer);
		}

		private void InsertScrollViewSubLayer(CALayer layer, int index)
		{
			scrollView.Layer.InsertSublayer(layer, index);
			layers.Add(layer);
		}

		private void ClearScrollViewSubLayers()
		{
			var diposables = layers.Where(c => c != SelectionIndicatorArrowLayer && c != SelectionIndicatorBoxLayer && c != SelectionIndicatorStripLayer);
			foreach (var layer in diposables)
			{
				layer.RemoveFromSuperLayer();
				layer.Dispose();
			}

			layers.Clear();
		}

		private void AddBackgroundAndBorderLayer(CGRect fullRect)
		{
			var backgroundLayer = new DisposableCALayer { Frame = fullRect };
			InsertScrollViewSubLayer(backgroundLayer, 0);

			var borderLayer = new DisposableCALayer { BackgroundColor = BorderColor.CGColor };
			switch (borderType)
			{
				case DHSegmentedControlBorderType.Top:
					borderLayer.Frame = new CGRect(0, 0, fullRect.Size.Width, borderWidth);
					break;
				case DHSegmentedControlBorderType.Left:
					borderLayer.Frame = new CGRect(0, 0, borderWidth, fullRect.Size.Height);
					break;
				case DHSegmentedControlBorderType.Bottom:
					borderLayer.Frame = new CGRect(0, fullRect.Size.Height - borderWidth, fullRect.Size.Width, borderWidth);
					break;
				case DHSegmentedControlBorderType.Right:
					borderLayer.Frame = new CGRect(fullRect.Size.Width - borderWidth, 0, borderWidth, fullRect.Size.Height);
					break;
			}
			AddScrollViewSubLayer(borderLayer);
		}

		private void SetArrowFrame()
		{
			SelectionIndicatorArrowLayer.Frame = FrameForSelectionIndicator();
			SelectionIndicatorArrowLayer.Mask = null;

			CGPoint p1, p2, p3;
			if (SelectionIndicatorLocation == DHSegmentedControlLocation.Down)
			{
				p1 = new CGPoint(SelectionIndicatorArrowLayer.Bounds.Size.Width / 2, 0);
				p2 = new CGPoint(0, SelectionIndicatorArrowLayer.Bounds.Size.Height);
				p3 = new CGPoint(SelectionIndicatorArrowLayer.Bounds.Size.Width, SelectionIndicatorArrowLayer.Bounds.Size.Height);
			}
			else if (SelectionIndicatorLocation == DHSegmentedControlLocation.Up)
			{
				p1 = new CGPoint(SelectionIndicatorArrowLayer.Bounds.Size.Width / 2, SelectionIndicatorArrowLayer.Bounds.Size.Height);
				p2 = new CGPoint(SelectionIndicatorArrowLayer.Bounds.Size.Width, 0);
				p3 = new CGPoint(0, 0);
			}
			else
			{
				p1 = p2 = p3 = CGPoint.Empty;
			}

			var arrowPath = new UIBezierPath();
			arrowPath.MoveTo(p1);
			arrowPath.AddLineTo(p2);
			arrowPath.AddLineTo(p3);
			arrowPath.ClosePath();

			SelectionIndicatorArrowLayer.Mask = new CAShapeLayer
			{
				Frame = SelectionIndicatorArrowLayer.Bounds,
				Path = arrowPath.CGPath
			};
		}

		private CGRect FrameForSelectionIndicator()
		{
			var indicatorYOffset = 0.0f;

			if (SelectionIndicatorLocation == DHSegmentedControlLocation.Down)
				indicatorYOffset = (float)(Bounds.Size.Height - SelectionIndicatorHeight + selectionIndicatorEdgeInsets.Bottom);
			else if (SelectionIndicatorLocation == DHSegmentedControlLocation.Up)
				indicatorYOffset = (float)selectionIndicatorEdgeInsets.Top;

			var sectionWidth = 0.0f;

			switch (type)
			{
				case DHSegmentedControlType.Text:
					sectionWidth = (float)MeasureTitle(SelectedIndex).Width;
					break;
			}

			if (SelectionStyle == DHSegmentedControlSelectionStyle.TextWidthStripe &&
				sectionWidth <= segmentWidth &&
				segmentWidthStyle != DHSegmentedControlWidthStyle.Dynamic)
			{
				var widthToEndOfSelectedSegment = (segmentWidth * SelectedIndex) + segmentWidth;
				var widthToStartOfSelectedIndex = segmentWidth * SelectedIndex;
				var x = ((widthToEndOfSelectedSegment - widthToStartOfSelectedIndex) / 2) + (widthToStartOfSelectedIndex - sectionWidth / 2);
				return new CGRect(x + selectionIndicatorEdgeInsets.Left, indicatorYOffset, sectionWidth - selectionIndicatorEdgeInsets.Right, SelectionIndicatorHeight);
			}
			else
			{
				if (segmentWidthStyle == DHSegmentedControlWidthStyle.Dynamic)
				{
					var selectedSegmentOffset = 0.0f;
					var i = 0;
					foreach (var width in segmentWidths)
					{
						if (SelectedIndex == i)
							break;
						selectedSegmentOffset += width;
						i++;
					}

					return new CGRect(selectedSegmentOffset + selectionIndicatorEdgeInsets.Left, indicatorYOffset, segmentWidths[SelectedIndex] - selectionIndicatorEdgeInsets.Right, SelectionIndicatorHeight + selectionIndicatorEdgeInsets.Bottom);
				}

				return new CGRect((segmentWidth + selectionIndicatorEdgeInsets.Left) * SelectedIndex, indicatorYOffset, segmentWidth - selectionIndicatorEdgeInsets.Right, SelectionIndicatorHeight);
			}
		}

		private CGRect FrameForFillerSelectionIndicator()
		{
			if (segmentWidthStyle == DHSegmentedControlWidthStyle.Dynamic)
			{
				var selectedSegmentOffset = 0.0f;
				int i = 0;

				foreach (var width in segmentWidths)
				{
					if (SelectedIndex == i)
						break;
					selectedSegmentOffset += width;
					i++;
				}

				return new CGRect(selectedSegmentOffset, 0, segmentWidths[SelectedIndex], Frame.Height);
			}

			return new CGRect(segmentWidth * SelectedIndex, 0, segmentWidth, Frame.Height);
		}

		public int SectionCount
		{
			get
			{
				return sectionTitles.Count;
			}
		}

		private void UpdateSegmentRects()
		{
			scrollView.ContentInset = UIEdgeInsets.Zero;
			scrollView.Frame = new CGRect(0, 0, Frame.Width, Frame.Height);

			if (SectionCount > 0)
			{
				segmentWidth = (float)(Frame.Size.Width / SectionCount);
			}

			if (type == DHSegmentedControlType.Text && segmentWidthStyle == DHSegmentedControlWidthStyle.Fixed && UserDraggable)
			{
				for (int i = 0; i < sectionTitles.Count; i++)
				{
					var stringWidth = MeasureTitle(i).Width + SegmentEdgeInset.Left + SegmentEdgeInset.Right;
					segmentWidth = (float)Math.Max(stringWidth, segmentWidth);
				}
			}
			else if (type == DHSegmentedControlType.Text && segmentWidthStyle == DHSegmentedControlWidthStyle.Dynamic)
			{
				segmentWidths = new List<float>();
				for (int i = 0; i < sectionTitles.Count; i++)
				{
					var stringWidth = MeasureTitle(i).Width + SegmentEdgeInset.Left + SegmentEdgeInset.Right;
					segmentWidths.Add((float)stringWidth);
				}
			}

			scrollView.ScrollEnabled = UserDraggable;
			scrollView.ContentSize = new CGSize(TotalSegmentControlWidth(), Frame.Size.Height);
		}

		private float TotalSegmentControlWidth()
		{
			if (type == DHSegmentedControlType.Text && segmentWidthStyle == DHSegmentedControlWidthStyle.Fixed)
				return sectionTitles.Count * segmentWidth;
			else //(segmentWidthStyle == AnimatedSegmentedControlWidthStyle.Dynamic)
				return segmentWidths.Sum();

		}

		#endregion

		#region Index Change

		private void SetSelectedSegmentIndex(int index, bool animated = false, bool notify = false)
		{
			previousIndex = SelectedIndex;
			SelectedIndex = index;
			SetNeedsDisplay();

			if (index == -1)
			{
				SelectionIndicatorArrowLayer.RemoveFromSuperLayer();
				SelectionIndicatorStripLayer.RemoveFromSuperLayer();
				SelectionIndicatorBoxLayer.RemoveFromSuperLayer();
			}
			else
			{
				ScrollToSelectedSegmentIndex(animated);

				if (animated)
				{
					if (SelectionIndicatorStripLayer.SuperLayer == null)
					{
						AddScrollViewSubLayer(SelectionIndicatorStripLayer);
						if (SelectionStyle == DHSegmentedControlSelectionStyle.Box && SelectionIndicatorBoxLayer.SuperLayer == null)
							InsertScrollViewSubLayer(SelectionIndicatorBoxLayer, 0);
						SetSelectedSegmentIndex(index, false, true);
					}

					if (notify)
					{
						NotifyForSegmentChange(index);
					}

					SelectionIndicatorArrowLayer.Actions = new NSDictionary();
					SelectionIndicatorStripLayer.Actions = new NSDictionary();
					SelectionIndicatorBoxLayer.Actions = new NSDictionary();

					CATransaction.Begin();
					CATransaction.AnimationDuration = 0.15f;
					CATransaction.AnimationTimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.Linear);
					SetArrowFrame();
					SelectionIndicatorBoxLayer.Frame = FrameForSelectionIndicator();
					SelectionIndicatorStripLayer.Frame = FrameForSelectionIndicator();
					SelectionIndicatorBoxLayer.Frame = FrameForFillerSelectionIndicator();
					CATransaction.Commit();
				}
				else
				{
					var newActions = new NSMutableDictionary();
					newActions.Add(new NSString("position"), NSNull.Null);
					newActions.Add(new NSString("bounds"), NSNull.Null);

					SelectionIndicatorArrowLayer.Actions = newActions;
					SelectionIndicatorStripLayer.Actions = newActions;
					SelectionIndicatorBoxLayer.Actions = newActions;

					SelectionIndicatorStripLayer.Frame = FrameForSelectionIndicator();
					SelectionIndicatorBoxLayer.Frame = FrameForFillerSelectionIndicator();

					if (notify)
						NotifyForSegmentChange(index);
				}
			}
		}

		private void NotifyForSegmentChange(int index)
		{
			if (Superview != null)
				SendActionForControlEvents(UIControlEvent.ValueChanged);

			if (IndexChange != null)
				IndexChange(this, index);
		}

		private void ScrollToSelectedSegmentIndex(bool animated)
		{
			CGRect rectForSelectedIndex;
			var selectedSegmentOffset = 0.0f;
			var localSegmentWidth = 0.0f;

			if (segmentWidthStyle == DHSegmentedControlWidthStyle.Fixed)
			{
				rectForSelectedIndex = new CGRect(segmentWidth * SelectedIndex, 0, segmentWidth, Frame.Size.Height);
				localSegmentWidth = segmentWidth;
			}
			else
			{
				var i = 0;
				var offsetter = 0.0f;
				foreach (var width in segmentWidths)
				{
					if (SelectedIndex == i)
						break;
					offsetter += width;
					i++;
				}

				rectForSelectedIndex = new CGRect(offsetter, 0, segmentWidths[SelectedIndex], Frame.Size.Height);
				localSegmentWidth = segmentWidths[SelectedIndex];
			}

			var multiplier = (SelectedIndex > previousIndex) ? 1 : -1;

			selectedSegmentOffset = (float)((Frame.Width / 2) + (multiplier * (localSegmentWidth / 2)));

			var rectToScrollTo = rectForSelectedIndex;

			rectToScrollTo.X -= selectedSegmentOffset;

			rectToScrollTo.Size = new CGSize(selectedSegmentOffset * 2, rectToScrollTo.Size.Height);
			scrollView.ScrollRectToVisible(rectToScrollTo, animated);
		}

		#endregion

		#region Touch

		public override void TouchesEnded(NSSet touches, UIEvent evt)
		{
			var touch = (UITouch)touches.AnyObject;
			var touchLocation = touch.LocationInView(this);

			if (!Bounds.Contains(touchLocation))
				return;

			var segment = 0;
			if (segmentWidthStyle == DHSegmentedControlWidthStyle.Fixed)
				segment = (int)Math.Truncate((touchLocation.X + scrollView.ContentOffset.X) / segmentWidth);
			else
			{
				var widthLeft = touchLocation.X + scrollView.ContentOffset.X;
				foreach (var width in segmentWidths)
				{
					widthLeft -= width;
					if (widthLeft <= 0)
						break;
					segment++;
				}
			}

			if (segment != SelectedIndex && segment < SectionCount)
			{
				if (TouchEnabled)
					SetSelectedSegmentIndex(segment, ShouldAnimateUserSelection, true);
			}
		}

		#endregion

		#region Styling

		public UIStringAttributes GetTitleTextAttributes()
		{

			var attributes = new UIStringAttributes();
			if (Font != null)
				attributes.Font = Font;
			if (TextColor != null)
				attributes.ForegroundColor = TextColor;
			return attributes;
		}

		public UIStringAttributes GetSelectedTitleTextAttributes()
		{
			var attributes = new UIStringAttributes();
			if (Font != null)
				attributes.Font = Font;
			if (SelectedTextColor != null)
				attributes.ForegroundColor = SelectedTextColor;
			return attributes;
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			ClearScrollViewSubLayers();

			SelectionIndicatorBoxLayer.Dispose();
			SelectionIndicatorStripLayer.Dispose();
			SelectionIndicatorArrowLayer.Dispose();

			foreach (var view in Subviews)
			{
				view.RemoveFromSuperview();
				view.Dispose();
			}

			base.Dispose(disposing);
		}

		#endregion
	}

	public class DisposableCALayer : CALayer
	{
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}
	}

	public class DisposableCATextLayer : CATextLayer
	{
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}
	}
}

