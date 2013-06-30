define([
	"dojo/_base/declare",
    "dijit/_WidgetBase",
    "dijit/_TemplatedMixin",
    
    // the widgets In Template Mixin adds nested template behaviour.
    "dijit/_WidgetsInTemplateMixin",
    
    "dojo/Evented",
    "dojo/topic",
    
    "dojo/text!./templates/MathUIMenu.html"
    
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
		return declare([
			widgetBase,
			templateMixin,
			embedTemplateMixin
		],{
		
		
		 /**
         * the dojo template system needs widgetsInTemplate to be true for nested templates
         */
        widgetsInTemplate: true,
        templateString: template,
        
		/**
         * prevalidate the input before attempting to post the form.
         */
        onPropertiesClick: function(evt) {
        	
        },
        
        onUploadClick: function(evt) {
        	
        },
        
        onAttributesClick: function(evt) {
        	
        },
        	
        onSingleValueClick: function(evt) {
        	
        },
        
        onDualValueClick: function(evt) {
        	
        },
        
        
        onDefineClick: function(evt) {
        	
        },
			
		onPartitionClick: function(evt) {
        	
        },
        	
		onTrainClick: function(evt) {
        	
        },
        
        	
		onTestClick: function(evt) {
        	
       },
       
       onCompareClick: function(evt) {
        	
        }
        
        
		});
		
		
	});
