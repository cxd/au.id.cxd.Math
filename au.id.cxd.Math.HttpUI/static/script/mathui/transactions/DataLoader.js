require(["dojo/request",
    "dojo/topic",
    "dojo/ready"],
    function(request, topic, ready) {
        ready(function() {


            /**
             * this function loads the raw data preview
             * associated with the project.
             */
            var loadDataPreview = function() {
                request("/preview/data", {
                    handleAs:"json",
                    method:"POST",
                    data:null
                }).then(function(response) {
                        console.log("Data Preview: ", response);
                        topic.publish("load/data/preview/success", response);

                    },
                    function(error) {
                        topic.publish("load/data/preview/error", error);
                        console.log("error:", error);
                    });


            };



            topic.subscribe("upload/file/success", function(e) {
                loadDataPreview();
            });

        });
    });