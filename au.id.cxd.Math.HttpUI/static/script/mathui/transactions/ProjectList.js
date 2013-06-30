require(["dojo/request", 
"dojo/topic", 
"dojo/ready"],
	function(request, topic, ready) {
		ready(function() {
			/**
			 * this function loads the project list.
			 */
			var loadProjectList = function() {
			
				request.get("/projects/list", {
					handleAs:"json"
				}).then(function(response) {
					console.log("Project List response: ", response);
					topic.publish("projects/list", response);
					
				},
				function(error) {
					topic.publish("projects/list/error", error);
					console.log("error:", error);
				});
				
			};
			
			
			console.log("Init request");
			
			topic.subscribe("load/projects/list", function(e) {
				loadProjectList();
			});
			
			});
	})