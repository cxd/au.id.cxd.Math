/**
 * Delay a callback for a set period of milliseconds
 */
define("mathui/dom/Delay",
function() {
	
	var _delay = function(millis, callback) {
		var poll = function() {
			callback();
		};
		setTimeout(poll, millis);
	}
	
	/**
	 * 
 	 * @param millis int milliseconds to delay
 	 * @param {() -> Unit} callback
	 */
	var delay = function(millis, callback) {
		_delay(millis, callback);
	};
	
	return delay;
	
});