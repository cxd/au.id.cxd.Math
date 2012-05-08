using System;
using System.Drawing;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;
using au.id.cxd.MathOSXUI;

namespace au.id.cxd.Math.OSX.UI
{
	public partial class AppDelegate : NSApplicationDelegate
	{
		MainWindowController _mainWindowController;
		
		private Window.MathWindowAdapter _windowAdapter;
		
		public AppDelegate ()
		{
		}
		
		public override void WillFinishLaunching (NSNotification notification)
		{
			
		}
		
		public override void FinishedLaunching (NSObject notification)
		{
			
			_mainWindowController = new MainWindowController ();
			_mainWindowController.Window.MakeKeyAndOrderFront (this);
			_windowAdapter = new Window.MathWindowAdapter(_mainWindowController.Window);
			_windowAdapter.build();
			
		}
	}
}

