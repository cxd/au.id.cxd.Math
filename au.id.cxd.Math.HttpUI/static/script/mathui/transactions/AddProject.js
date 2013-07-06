require(["dojo/request", 
"dojo/topic", 
"dojo/ready"],
	function(request, topic, ready) {
		ready(function() {


			/**
			 * this function loads the project list.
			 */
			var addProject = function(name) {
			
				request("/project/create", {
					handleAs:"json",
					method:"POST",
					data:"project="+name
				}).then(function(response) {
					console.log("Project Create response: ", response);
					topic.publish("project/create/result", response);
					topic.publish("load/projects/list",{});
				},
				function(error) {
					topic.publish("project/create/error", error);
					console.log("error:", error);
				});
				
			};
			
			
			console.log("Init request");
			
			topic.subscribe("project/create/request", function(e) {
				addProject(e);
			});
			
			});
	});