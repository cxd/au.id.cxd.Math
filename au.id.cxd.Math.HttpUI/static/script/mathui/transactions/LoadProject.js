require(["dojo/request",
    "dojo/topic",
    "dojo/ready"],
    function(request, topic, ready) {
        ready(function() {


            /**
             * this function loads the project.
             */
            var loadProject = function(name) {

                request("/project/load", {
                    handleAs:"json",
                    method:"POST",
                    data:"project="+name
                }).then(function(response) {
                        console.log("Project Create response: ", response);
                        topic.publish("project/load/result", response);

                    },
                    function(error) {
                        topic.publish("project/load/error", error);
                        console.log("error:", error);
                    });

            };


            console.log("Init request");

            topic.subscribe("project/load/request", function(e) {
                loadProject(e);
            });

        });
    });