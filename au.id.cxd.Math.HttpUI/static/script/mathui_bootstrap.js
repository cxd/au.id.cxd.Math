
   
 // an example of loading the module.
require([
		"dojo/parser",
		"dojo/ready",
		"dojo/on",
        "dojo/Evented",
                
       

		// templates
		"mathui/upload_data/UploadData",
		"mathui/menu/MathUIMenu",
		"mathui/toolbar/MathUIToolbar"
		
        ], 
        function(
        	parser, 
        	ready,
        	on,
        	Evented) {
        	
        ready(function () {
        	// Invoke the dojo/parser
    		parser.parse();
	});
});