// an example of loading the module.
require([
    "dojo/parser",
    "dojo/ready",
    "dojo/on",
    "dojo/Evented",


    "mathui/transactions/ProjectList",
    "mathui/transactions/AddProject",
    "mathui/transactions/DeleteProject",
    "mathui/transactions/LoadProject",

    // templates
    "mathui/upload_data/UploadData",
    "mathui/menu/MathUIMenu",
    "mathui/menu/MenuSupport",

    "mathui/toolbar/MathUIToolbar",

    "mathui/confirmation/Confirmation",

    "mathui/project/Project"


],
    function (parser, ready, on, Evented) {

        ready(function () {
            // Invoke the dojo/parser
            parser.parse();
        });
    });