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

            postCreate: function () {
                var self = this;
                topic.subscribe("project/load/result",
                    function (response) {

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
                menuSupport.attachToMenu("menu/upload", function (e) { self.onDisplay(e); }, function (e) { self.onHide(e); });
            },


            /**
             * prevalidate the input before attempting to post the form.
             */
            onUploadForm: function (evt) {

            }
        
		}));
		
		
	});
