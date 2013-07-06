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

    "dojo/text!./templates/Project.html",

    "mathui/menu/MenuSupport"

],
    function (declare, widgetBase, templateMixin, embedTemplateMixin, event, topic, dom, domConstruct, domClass, domAttr, domGeom, domStyle, query, domNode, template, menuSupport) {
        return declare([
            widgetBase,
            templateMixin,
            embedTemplateMixin
        ], {


            /**
             * the dojo template system needs widgetsInTemplate to be true for nested templates
             */
            widgetsInTemplate: true,
            templateString: template,

            projectList: [],

            postCreate: function () {
                var self = this;
                topic.subscribe("projects/list",
                    function (list) {
                        if (data == null || data == undefined) {
                            return;
                        }
                        if (!data.success) {
                            return;
                        }
                        self.projectList = data.projects;
                    });

                this.subscribeToMenu();
                topic.subscribe("project/add/click", function (evt) { self.onDisplayNew(evt) ; });
                topic.subscribe("project/selected", function(evt) { self.onSelected(evt); });
                //topic.subscribe("", onHide);
            },

            /// handle the project selected event.
            onSelected: function (name) {
                this.onDisplay();
                var node = query("#project-name-field")[0];
                node.value = name;
                // load the project data
                topic.publish("project/load/request", name);
            },

            onDisplayNew: function (evt) {
                var node = query(".project-add");
                node.removeClass("hidden");
                node.addClass("block");
                var node = query("#project-name-field")[0];
                node.value = "";
            },

            onDisplay: function (evt) {
                var node = query(".project-add");
                node.removeClass("hidden");
                node.addClass("block");
            },

            onHide: function (evt) {
                var node = query(".project-add");
                node.removeClass("block");
                node.addClass("hidden");

            },


            subscribeToMenu: function () {
                var self = this;
                console.log(menuSupport);
                menuSupport.attachToMenu("menu/properties", function (e) { self.onDisplay(e); }, function (e) { self.onHide(e); });
            },


            onChange: function (evt) {

            },

            onAddClick: function (evt) {
                var node = query("#project-name-field")[0];
                var name = node.value;
                if (name != null && name != undefined && name != "") {
                    topic.publish("project/create/request", name);
                }
            },

            onCancelClick: function (evt) {
                this.onHide(evt);

            },

            onDeleteClick: function (evt) {
                var self = this;
                var node = query("#project-name-field")[0];
                var name = node.value;
                if (name != null && name != undefined && name != "") {
                    // general prompt to confirm deletion.
                    topic.publish("confirmDialog/showDialog",
                        {
                            message: "Do you really want to delete this project?",
                            confirmFn: function () {
                                topic.publish("project/delete/request", name);
                                self.onHide(null);
                            },
                            cancelFn: function () {

                            }
                        });
                }
            }



        });
    });