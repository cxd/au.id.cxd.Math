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
    
    "dojo/text!./templates/MathUIToolbar.html"
    
],
	function(
		declare,
		widgetBase,
		templateMixin,
		embedTemplateMixin,
		event,
		topic,
		dom,
		domConstruct,
		domClass,
		domAttr,
		domGeom,
		domStyle,
		query,
		domNode,
		
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
       	 * initialise and subscribe 
       	 */
       	postCreate: function() {
       		var self = this;
       		topic.subscribe("projects/list", function (data) { self.onProjectListUpdated(data); });
       		topic.publish("load/projects/list", {});
       	},
       	
       	clearSelectOptions: function(selectNode) {
       		selectNode.empty();
       	},
       	
       	addSelectOption: function(selectNode, optionText, optionValue) {
       		var option = domConstruct.toDom("<option value=\""+optionValue+"\">"+optionText+"</option>");
       		selectNode.addContent(option);
       	},
       	
       	setEnabled: function(selectNode, enabled) {
       		selectNode.attr("disabled", !enabled);
       	},
       	
       	onProjectListUpdated: function(data) {
       		console.log("Data: " + data);
       		if (data == null || data == undefined) {
       			return;
       		}
       		if (!data.success) {
       			return;
       		}
       		var projects = data.projects;
       		var select = query(".project-list-select");
       		this.clearSelectOptions(select);
       		if (projects.length == 0) {
       			this.setEnabled(select, false);
       			this.addSelectOption(select, "No Projects Created", "none");
       			return;	
       		}
       		this.setEnabled(select, true);
       		for(var i in projects) {
       			this.addSelectOption(select, projects[i], projects[i]);
       		}
       	}
        
		});
		
		
	});
