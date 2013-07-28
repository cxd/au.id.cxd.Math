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
    "dgrid/Grid",
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
        Grid,

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
                topic.subscribe("load/data/preview/success", function(evt) { self.onDataPreview(evt); });
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

            // display a data grid for preview.
            onDataPreview: function(evt) {
                var self = this;
                var columnHeaders = {};
                var dataObjects = [];
                for(var i in evt.records.columns) {
                    var col = evt.records.columns[i];
                    for(var j in col) {
                        columnHeaders[j] = col[j];
                    }
                }
                for(var i in evt.records.data) {
                    var datum = {};
                    var row = evt.records.data[i];
                    for(var j in row) {
                        for(var k in row[j]) {
                            datum[k] = row[j][k];
                        }
                    }
                    dataObjects.push(datum);

                }
                // request the data to preview.
                var grid = new Grid({
                    columns: columnHeaders

                }, "data-preview-table");
                grid.renderArray(dataObjects);

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
                                topic.publish("upload/file/success", null);

                            } else {
                                // error.
                                // display an error.
                                console.log("Error: " + data.message + " could not upload file.");

                                topic.publish("upload/file/error", data);

                            }
                          },
                      function(error) {
                           // handle the error response.
                           console.log("Error: ", error);

                          topic.publish("upload/file/error", {error: error});
                      });
            }
        
		}));
		
		
	});