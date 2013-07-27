require(["dojo/request",
    "dojo/topic",
    "dojo/ready"],
    function(request, topic, ready) {
        ready(function() {


            /**
             * this function loads the raw data preview
             * associated with the project.
             */
            var loadDataPreview = function(name) {



            };



            topic.subscribe("project/file/preview/request", function(e) {
                loadDataPreview(e);
            });

        });
    });