/**
 * Determine if the  
 */
define("mathui/dom/WhenReady",
function() {
	
	var loopTimer = function(test, callback) {
		var poll = function() {
			if (test()) {
				callback();
			} else {
				setTimeout(poll, 30);	
			}
		};
		poll();
	}
	
	/**
	 * 
 	 * @param {() -> Boolean} test
 	 * @param {() -> Unit} callback
	 */
	var readyTest = function(test, callback) {
		loopTimer(test, callback);
	};
	
	return readyTest;
	
});