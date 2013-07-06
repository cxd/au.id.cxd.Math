define([
	
    "dojo/Evented",
    "dojo/topic",
    
],
	function(
		event,
		topic
	) {
		return {
		
		actions: [
			"menu/properties",
			"menu/upload",
			"menu/attributes",
			"menu/singleValue",
			"menu/dualValue",
			"menu/multiValue",
			"menu/define",
			"menu/partition",
			"menu/train",
			"menu/test",
			"menu/compare"	
			],
		
		/**
		 * attach an item to the menu.
		 * provide the following
 * @param {Object} displayEventName - the name of the event which causes the object to become active.
 * @param {Object} displayFn - the activation function
 * @param {Object} dismissFn - the dismiss function
		 */
		attachToMenu: function(displayEventName, displayFn, dismissFn) {
			
			for(var i in this.actions) {
				var action = this.actions[i];
				if (action == displayEventName) {
					topic.subscribe(action, function(evt) { displayFn(evt); });
				} else {
					topic.subscribe(action, function(evt) { dismissFn(evt); });
				}
			}
			
		}	
		
	} 
});