using System;

using UIKit;
using System.Collections.Generic;
using DH.Custom.SegmentedControl;
using CoreGraphics;

namespace DHSegmentedControlSample
{
	public partial class ViewController : UIViewController
	{
		protected ViewController(IntPtr handle) : base(handle)
		{
			// Note: this .ctor should not contain any initialization logic.
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			CreateDynamicSegmentedControl();
			CreateFixedSegmentedControl();
		}

		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
			// Release any cached data, images, etc that aren't in use.
		}

		private void CreateDynamicSegmentedControl()
		{
			var screenWidth = UIScreen.MainScreen.Bounds.Width;

			var segmentedList = new List<string>();
			var numberOfOptions = 10;

			for (int i = 1; i <= numberOfOptions; i++)
			{
				segmentedList.Add("Dynamic " + i);
			}

			var SegmentedControl = new DHSegmentedControl();

			SegmentedControl.Font = UIFont.FromName("HelveticaNeue-Medium", 14f);

			var size = new CGSize(screenWidth, 40);

			var rect = new CGRect(DynamicSegmentedContainer.Bounds.Location, size);

			SegmentedControl.Frame = rect;
			SegmentedControl.SelectionStyle = DHSegmentedControlSelectionStyle.TextWidthStripe;
			SegmentedControl.SelectionIndicatorLocation = DHSegmentedControlLocation.Down;
			SegmentedControl.SelectionIndicatorColor = UIColor.Red;
			SegmentedControl.TextColor = UIColor.DarkGray;
			SegmentedControl.SelectedTextColor = UIColor.Red;
			SegmentedControl.BackgroundColor = UIColor.White;
			SegmentedControl.SegmentEdgeInset = new UIEdgeInsets(0, 10, 0, 10);
			SegmentedControl.LabelMargins = new UIEdgeInsets(0, 8, 0, 8);
			SegmentedControl.SelectionIndicatorHeight = 2.0f;
			SegmentedControl.UserDraggable = true;
			SegmentedControl.ShouldAnimateUserSelection = true;
			SegmentedControl.SelectedIndex = 1;
			SegmentedControl.SegmentWidthStyle = DHSegmentedControlWidthStyle.Dynamic;
			SegmentedControl.SectionTitles = segmentedList;
			DynamicSegmentedContainer.AddSubview(SegmentedControl);
			DynamicSegmentedContainer.BackgroundColor = UIColor.Clear;

		}

		private void CreateFixedSegmentedControl()
		{
			var screenWidth = UIScreen.MainScreen.Bounds.Width;

			var segmentedList = new List<string>();
			var numberOfOptions = 3;

			for (int i = 1; i <= numberOfOptions; i++)
			{
				segmentedList.Add("Fix " + i);
			}

			var SegmentedControl = new DHSegmentedControl(segmentedList);

			SegmentedControl.Font = UIFont.FromName("HelveticaNeue-Medium", 14f);

			var size = new CGSize(screenWidth, 40);

			var rect = new CGRect(FixedSegmentedContainer.Bounds.Location, size);

			SegmentedControl.Frame = rect;
			SegmentedControl.SelectionStyle = DHSegmentedControlSelectionStyle.TextWidthStripe;
			SegmentedControl.SelectionIndicatorLocation = DHSegmentedControlLocation.Down;
			SegmentedControl.SelectionIndicatorColor = UIColor.Red;
			SegmentedControl.TextColor = UIColor.DarkGray;
			SegmentedControl.SelectedTextColor = UIColor.Red;
			SegmentedControl.BackgroundColor = UIColor.White;
			SegmentedControl.SegmentEdgeInset = new UIEdgeInsets(0, 10, 0, 10);
			SegmentedControl.LabelMargins = new UIEdgeInsets(0, 8, 0, 8);
			SegmentedControl.SelectionIndicatorHeight = 2.0f;
			SegmentedControl.UserDraggable = false;
			SegmentedControl.ShouldAnimateUserSelection = true;
			SegmentedControl.SelectedIndex = 1;
			SegmentedControl.SegmentWidthStyle = DHSegmentedControlWidthStyle.Fixed;

			FixedSegmentedContainer.AddSubview(SegmentedControl);
			FixedSegmentedContainer.BackgroundColor = UIColor.Clear;
		}
	}
}
