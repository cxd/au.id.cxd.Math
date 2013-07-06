/**
 * this is an abstract widget
 * that can fade in or fade out. 
 */
define([
    "dojo/_base/declare",
    "dojo/dom-class", 
    "dojo/dom",
    "dojo/dom-construct",
    "dojo/dom-style",
    "dojo/_base/fx",
	"dojo/fx"
], function(declare,
	domClass,
	dom,
	domConstruct,
	domStyle,
	baseFx,
	fx) {

return declare([],
	{
		/**
        *display the dialog. 
        */
       showDialog: function() {
       	domStyle.set(this.domNode, "opacity", "0.0");
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
       
       /**
        * fade out the dialog 
        */
       fadeOut: function() {
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
       			domClass.add(_self.domNode, "notdisplayed");		
       		}
       	}).play();
       	
       }
       
	});
});