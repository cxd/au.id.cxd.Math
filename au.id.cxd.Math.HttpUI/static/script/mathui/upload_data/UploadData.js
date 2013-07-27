define([
	"dojo/_base/declare",
    "dijit/_WidgetBase",
    "dijit/_TemplatedMixin",
    
    // the widgets In Template Mixin adds nested template behaviour.
    "dijit/_WidgetsInTemplateMixin",
    
    "dojo/Evented",
    "dojo/topic",

    "dojo/dom",
    "dojo/dom-construct",
    "dojo/dom-class",
    "dojo/dom-attr",
    "dojo/dom-geometry",
    "dojo/dom-style",
    "dojo/query",
    "dojo/NodeList-dom",

    "dojo/request/iframe",
    "dojox/form/Uploader",

    "dojo/text!./templates/UploadData.html" ,

    "mathui/menu/MenuSupport"
    
],
	function(
		declare,
		widgetBase,
		templatedMixin,
		widgetsInTemplateMixin,
		evented,
		topic,

        dom,
        domConstruct,
        domClass,
        domAttr,
        domGeometry,
        domStyle,
        query,
        nodeListDom,

        iframe,
        uploader,

		template,
        menuSupport
	) {
		return(declare([
			widgetBase,
            templatedMixin,
            widgetsInTemplateMixin
		], {
			
		/**
         * the dojo template system needs widgetsInTemplate to be true for nested templates
         */
        widgetsInTemplate: true,
        templateString: template,

        project:{},

            postCreate: function () {
                var self = this;
                topic.subscribe("project/load/result",
                    function (response) {
                          // TODO: load data to display preview.

                    });

                this.subscribeToMenu();

                topic.subscribe("project/add/click", function (evt) { self.onHide(evt) ; });
                topic.subscribe("project/selected", function(evt) { self.onHide(evt); });

            },

            onDisplay: function (evt) {
                query(".upload-data")
                    .removeClass("hidden")
                    .addClass("block");
            },

            onHide: function (evt) {
                query(".upload-data")
                    .removeClass("block")
                    .addClass("hidden");

            },

            subscribeToMenu: function () {
                var self = this;
                console.log(menuSupport);
                menuSupport.attachToMenu("menu/upload", function (project) {
                    self.onDisplay(project);
                    self.project = project; },
                    function (e) { self.onHide(e); });
            },


            onHeadingCheckChange: function(evt) {
                var hidden = dom.byId("containsHeader");
                if (dom.byId("headingCheckbox").checked) {
                    domAttr.set(hidden, "value", "True");

                } else {
                    domAttr.set(hidden, "value", "False");

                }
            },

            /**
             * prevalidate the input before attempting to post the form.
             */
            onUploadForm: function (evt) {
                // store the current project name
                domAttr.set(dom.byId("uploadProject"), "value", this.project.project);

                // submit the form.
                iframe("/upload/file",
                      {
                        form:"upload_data_form",
                        handleAs:"json",
                        method:"POST"
                      }
                  ).then(
                          function(data) {
                            // read the response and determine whether
                            // to display an error.
                            if (data.status == true) {
                                // success.
                                // TODO: display a preview.
                                topic.publish("project/file/preview/request", null);

                            } else {
                                // error.
                                // display an error.
                                console.log("Error: " + data.message + " could not upload file.");

                            }
                          },
                      function(error) {
                           // handle the error response.
                           console.log("Error: ", error);
                      });
            }
        
		}));
		
		
	});