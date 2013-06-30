define([
	"dojo/_base/declare",
    "dijit/_WidgetBase",
    "dijit/_TemplatedMixin",
    
    // the widgets In Template Mixin adds nested template behaviour.
    "dijit/_WidgetsInTemplateMixin",
    
    "dojo/Evented",
    "dojo/topic",
    
    "dojo/text!./templates/UploadData.html"
    
],
	function(
		declare,
		widgetBase,
		templateMixin,
		embedTemplateMixin,
		event,
		topic,
		template
	) {
		return(declare([
			declare,
			widgetBase,
			templateMixin,
			embedTemplateMixin
		], {
			
		/**
         * the dojo template system needs widgetsInTemplate to be true for nested templates
         */
        widgetsInTemplate: true,
        templateString: template,
		
		/**
         * prevalidate the input before attempting to post the form.
         */
        onUploadForm: function (evt) {
        	
        }
        
		}));
		
		
	});
