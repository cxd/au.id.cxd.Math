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
        	topic.publish("menu/properties");
        },
        
        onUploadClick: function(evt) {
        	topic.publish("menu/upload");
        },
        
        onAttributesClick: function(evt) {
        	topic.publish("menu/attributes");
        },
        	
        onSingleValueClick: function(evt) {
        	topic.publish("menu/singleValue");
        },
        
        onDualValueClick: function(evt) {
        	topic.publish("menu/dualValue");
        },
        
        onMultiValueClick:function(evt) {
        	topic.publish("menu/multiValue");
        },
        
        onDefineClick: function(evt) {
        	topic.publish("menu/define");
        },
			
		onPartitionClick: function(evt) {
        	topic.publish("menu/partition");
        },
        	
		onTrainClick: function(evt) {
        	topic.publish("menu/train");
        },
        
        	
		onTestClick: function(evt) {
        	topic.publish("menu/test");
       	},
       
       	onCompareClick: function(evt) {
        	topic.publish("menu/compare");
        }
        
        
		});
		
		
	});
