using MonoMac.Foundation;
using MonoMac.AppKit;


namespace au.id.cxd.Math.OSX.UI
{
	
	// Should subclass MonoMac.AppKit.NSWindow
	[MonoMac.Foundation.Register("MainWindow")]
	public partial class MainWindow
	{
		
		public MainWindow() {
			//this.BackgroundColor = NSColor.Blue;
		}
		
	}
	
	// Should subclass MonoMac.AppKit.NSWindowController
	[MonoMac.Foundation.Register("MainWindowController")]
	public partial class MainWindowController
	{
	}
}

