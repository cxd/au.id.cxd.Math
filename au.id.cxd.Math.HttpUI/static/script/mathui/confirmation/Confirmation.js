define([
    "dojo/_base/declare",
    "dijit/_WidgetBase",
    "dijit/_TemplatedMixin",
    // the widgets In Template Mixin adds nested template behaviour.
    "dijit/_WidgetsInTemplateMixin",
    // the widgets In Template Mixin adds nested template behaviour.
    "dojo/text!./templates/Confirmation.html",
    
    "dojo/on", 
    
    // the evented api
    "dojo/Evented",
    
    "dojo/mouse", 
    
    "dojo/query",
    
    "dojo/dom-class", 
    
    "dojo/dom",
    "dojo/dom-construct",
    "dojo/dom-style",
    "dojo/NodeList-dom",
    
    "dojo/topic",
    
	"dojo/_base/array",
    
    "dojo/_base/fx",
	
	"dojo/fx",

    "mathui/dom/WhenReady"
    
], function(declare, 
	_WidgetBase, 
	_TemplatedMixin, 
	_WidgetsInTemplateMixin, 
	template, 
	on, 
	Evented,
	mouse, 
	query,
	domClass,
	dom,
	domConstruct,
	domStyle,
	nodeList,
	topic,
	array,
	baseFx,
	fx,
	WhenReady) {

return declare([_WidgetBase, 
	_TemplatedMixin, 
	_WidgetsInTemplateMixin, 
	Evented],
	{
		 /**
         * the dojo template system needs widgetsInTemplate to be true for nested templates
         */
        templateString: template,
        
        widgetsInTemplate: true,
        
        _confirmAction: function() {},
        
        _cancelAction: function() {},
        
        _currentMessage: "",
        
        postCreate: function() {
        	this.inherited(arguments);
        	var _self = this;
        	topic.subscribe("confirmDialog/showDialog", 
        		function(params) {
        			_self.showDialog(params.message, params.confirmFn, params.cancelFn);
        		});
        },
        
         /**
        *display the dialog. 
        */
       display: function() {
       	domStyle.set(this.domNode, "opacity", "0.0");
       	dom.byId("confirmationMessage").innerHTML = this._currentMessage;

       	domClass.remove(this.domNode, "notdisplayed");
       	baseFx.animateProperty({
       		node:this.domNode,
       		properties:{
       			opacity:{
       				start:function() {return 0.0; },
       				end: function() { return 1.0; }
       			}
       		}
       	}).play();
       },
       
       fadeOut: function(endFn) {
	       	var _self = this;
	       	baseFx.animateProperty({
	       		node:this.domNode,
	       		properties:{
	       			opacity:{
	       				start:function() {return 1.0; },
	       				end: function() { return 0.0; }
	       			}
	       		},
	       		onEnd: function() {
	       			dom.byId("confirmationMessage").innerHTML = "";
	       			domClass.add(_self.domNode, "notdisplayed");		
	       			endFn();
	       		}
	       	}).play();
       },
        
        /**
         *show the dialog
         * display the text message
         * if the user clicks the confirm button invoke the callback confirmFn 
 * @param {Object} message
 * @param {Object} confirmFn
         */
        showDialog: function(message, confirmFn, cancelFn) {
        	this._currentMessage = message;
        	this._confirmAction = confirmFn;
        	this.display();
        },
        
        onConfirmDialog: function(evt) {
        	var _self = this;
        	this.fadeOut(function() {
        		if (_self._confirmAction != null) {
	        		_self._confirmAction();
	        	}
        	});
        },
        
        onCancelDialog: function(evt) {
            var _self = this;
        	this.fadeOut(function() {
        		if (_self._cancelAction != null) {
	        		_self._cancelAction();
	        	}
        	});
        }
 	});
});