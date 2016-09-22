// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace DHSegmentedControlSample
{
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		UIKit.UIView DynamicSegmentedContainer { get; set; }

		[Outlet]
		UIKit.UIView FixedSegmentedContainer { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (DynamicSegmentedContainer != null) {
				DynamicSegmentedContainer.Dispose ();
				DynamicSegmentedContainer = null;
			}

			if (FixedSegmentedContainer != null) {
				FixedSegmentedContainer.Dispose ();
				FixedSegmentedContainer = null;
			}
		}
	}
}
