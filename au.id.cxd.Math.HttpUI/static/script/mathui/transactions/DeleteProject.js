require(["dojo/request", 
"dojo/topic", 
"dojo/ready"],
	function(request, topic, ready) {
		ready(function() {


			/**
			 * this function loads the project list.
			 */
			var deleteProject = function(name) {
			
				request("/project/delete", {
					handleAs:"json",
					method:"POST",
					data:"project="+name
				}).then(function(response) {
					console.log("Project Create response: ", response);
					topic.publish("project/delete/result", response);
					topic.publish("load/projects/list",{});
				},
				function(error) {
					topic.publish("project/delete/error", error);
					console.log("error:", error);
				});
				
			};
			
			
			console.log("Init request");
			
			topic.subscribe("project/delete/request", function(e) {
				deleteProject(e);
			});
			
			});
	});