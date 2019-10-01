class Socket {
    name = "";
    constructor(name) {
        this.name = name;
    }
}

class Input {
    controls = [];
    connection = null;
    constructor(name, title, type) {
        this.name = name;
        this.title = title;
        this.type = type;
    }
    addControl(control) {
        this.controls.push(control);
        return this;
    }
    build() {
        for (var c in this.controls)
            return this.controls[c].build(); //TODO: fix multiple controls?
    }
    set value(value) {
        for (var c in this.controls)
            this.controls[c].value = value;
    }
}

class Output {
    controls = [];
    connections = [];
    constructor(name, title, type) {
        this.name = name;
        this.title = title;
        this.type = type;
    }
    addControl(control) {
        this.controls.push(control);
        return this;
    }
    build() {
        for (var c in this.controls)
            return this.controls[c].build(); //TODO: multiple controls
    }
    set value(value) {
        for (var c in this.controls)
            this.controls[c].value = value;
    }
}

class Component {
    name = "";
    constructor(name) {
        this.name = name;
    }
}

class Control {

}



class Node {
    el = {};
    name = "";
    inputs = [];
    outputs = [];
    data = {}

    constructor(data, editor) {
        this.name = data.name;
        this.id = data.id;
        this.editor = editor;

        if (editor.components[this.name] == null)
            console.log("Could not find component " + this.name);
        this.component = editor.components[this.name];

        this.el.card = $(`<div class="node card bg-dark text-white">`);
        this.el.card.css({ left: data.position[0], top: data.position[1] });

        this.el.card.append($(`<div class="card-header">` + data.name + `</div>`));
        this.el.connections = $(`<ul class="list-group list-group-flush"/>`);
        this.el.card.append(this.el.connections);
        this.component.build(this, data);

        //interactivity
        this.el.card.mousedown(this, this.handle_dragnode);

        editor.el.append(this.el.card);
    }

    toJSON() {
        var outs = {};
        for (var o in this.outputs) {
            for (var c in this.outputs[o].controls) {
                this.data[this.outputs[o].name] = this.outputs[o].controls[c].value;
            }
            var out = { connections: [] };
            for (var c in this.outputs[o].connections)
                out.connections.push({
                    node: this.outputs[o].connections[c].node,
                    input: this.outputs[o].connections[c].input,
                    data: {}
                });
            outs[this.outputs[o].name] = out;
        }
        var ins = {};
        for (var i in this.inputs) {
            for (var c in this.inputs[i].controls) {
                this.data[this.inputs[i].name] = this.inputs[i].controls[c].value;
            }
            var in_ = { connections: [] };
            if(this.inputs[i].connection)
                in_.connections.push({
                    node: this.inputs[i].connection.node,
                    output: this.inputs[i].connection.output,
                    data: {}
                });
            ins[this.inputs[i].name] = in_;
        }


        return {
            id: this.id,
            data: this.data,
            inputs: ins,
            outputs: outs,
            position: [parseInt(this.el.card.css("left")), parseInt(this.el.card.css("top"))],
            name: this.component.name,
        }
    }

    handle_dragnode(e) {
        if (e.preventDefault)
            e.preventDefault();

        var node = e.data;
        if (editor.selectedNode != null) {
            editor.selectedNode.el.card.removeClass("bg-primary");
            editor.selectedNode.el.card.addClass("bg-dark");
            editor.selectedNode.el.card.find(".bg-primary").addClass("bg-dark");
            editor.selectedNode.el.card.find(".bg-primary").removeClass("bg-primary");
        }
        editor.selectedNode = node;
        editor.selectedNode.el.card.addClass("bg-primary");
        editor.selectedNode.el.card.removeClass("bg-dark");
        editor.selectedNode.el.card.find(".bg-dark").addClass("bg-primary");
        editor.selectedNode.el.card.find(".bg-dark").removeClass("bg-dark");

        window.my_dragging = {};

        my_dragging.pageP0 = editor.mousePos(e);

        my_dragging.pageX0 = e.pageX;
        my_dragging.pageY0 = e.pageY;
        my_dragging.elem = this;
        my_dragging.offset0 = $(this).offset();
        function handle_dragging(e) {
            var left = my_dragging.offset0.left + (e.pageX - my_dragging.pageX0);
            var top = my_dragging.offset0.top + (e.pageY - my_dragging.pageY0);
            $(my_dragging.elem)
                .offset({ top: top, left: left });

            node.updateConnectionPositions();
        }
        function handle_mouseup(e) {
            $('body')
                .off('mousemove', handle_dragging)
                .off('mouseup', handle_mouseup);
        }
        $('body')
            .on('mouseup', handle_mouseup)
            .on('mousemove', handle_dragging);
        return false;
    }

    addOutput(output, data) {
        this.outputs.push(output);
        output.el = $(`<li class="list-group-item bg-dark">` + output.title + `</li>`);
        output.control = output.build();
        if (output.control) {
            output.el.append($("<br />"));
            output.el.append(output.control);
        }

        if (data && data.data && data.data[output.name] != null) {
            output.value = data.data[output.name];
            this.data[output.name] = data.data[output.name];
        }

        output.el.append($(`<div class="outputsocket socket-` + output.type.name.toLowerCase() +`"></div>`));
        this.el.connections.append(output.el);

        var node = this;
        output.el.find(".outputsocket").mousedown(function (e) {
            var output = null;
            for (var o in node.outputs)
                if (node.outputs[o].el.find(".outputsocket").is($(this)))
                    output = node.outputs[o];

            if (e.preventDefault)
                e.preventDefault();

            var position = editor.findSocketPos($(this));

            var mousePos = editor.mousePos(e);
            mousePos = { left: mousePos[0], top: mousePos[1] };

            window.my_dragging = {};
            my_dragging.svg = $('<svg class="connection"></svg>');
            my_dragging.position = position;

            my_dragging.path = document.createElementNS("http://www.w3.org/2000/svg", "path");
            my_dragging.svg.append(my_dragging.path);
            my_dragging.path.setAttributeNS(null, "class", "main-path");
            my_dragging.path.setAttributeNS(null, "d", editor.createPathString(position, mousePos));
            editor.el.append(my_dragging.svg);

            function handle_dragging(e) {
                var mousePos = editor.mousePos(e);
                mousePos = { left: mousePos[0], top: mousePos[1] };

                my_dragging.path.setAttributeNS(null, "d", 'M ' +
                    my_dragging.position.left + ' ' + my_dragging.position.top + ' C ' +
                    mousePos.left + ' ' + my_dragging.position.top + ' ' +
                    my_dragging.position.left + ' ' + mousePos.top + ' ' +
                    mousePos.left + ' ' + mousePos.top);
            }
            function handle_mouseup(e) {
                var mousePos = { left: editor.el.offset().left, top: editor.el.offset().top };
                mousePos.left = e.pageX - mousePos.left;
                mousePos.top = e.pageY - mousePos.top;

                var foundInput = null;
                var foundNode = null;
                $.each(editor.nodes, (i, node) => {
                    $.each(node.inputs, (i, input) => {
                        var position = editor.findSocketPos(input.el.find(".inputsocket"));
                        if (Math.abs(mousePos.left - position.left) < 32 &&
                            Math.abs(mousePos.top - position.top) < 32) {
                            foundInput = input;
                            foundNode = node;
                        }
                    });
                });

                $('body')
                    .off('mousemove', handle_dragging)
                    .off('mouseup', handle_mouseup);

                my_dragging.svg.remove();
                if (!foundInput)
                    return;

                //if the input we found has a connection, break it
                if (foundInput.connection) {
                    foundInput.connection.out.connections =
                        foundInput.connection.out.connections.filter(
                            con => con.node != foundNode.id || con.input != foundInput.name);
                    foundInput.connection.el.remove();
                    foundInput.connection = null;
                }

                if (output.type.name != foundInput.type.name) {
                    alert("Incompatible types");
                    return;
                }

                editor.buildConnection(foundNode, foundInput, node, output);
            }
            $('body')
                .on('mouseup', handle_mouseup)
                .on('mousemove', handle_dragging);
            return false;
        });



        return this;
    }
    addInput(input, data) {
        this.inputs.push(input);
        input.el = $(`<li class="list-group-item bg-dark"><div class="inputsocket socket-` + input.type.name.toLowerCase()+`"></div>` + input.title + `</li>`);
        input.control = input.build();
        if (input.control) {
            input.el.append($("<br />"));
            input.el.append(input.control);
        }
        if (data && data.data && data.data[input.name]) {
            input.value = data.data[input.name];
            this.data[input.name] = data.data[input.name];
        }
        this.el.connections.append(input.el);
        return this;
    }

    removeInput(index) {
        //TODO: remove connections
        this.inputs[index].el.remove();
        this.inputs.splice(index,1);
    }


    updateConnectionPositions() {
        for (var i in this.inputs) {
            var input = this.inputs[i];
            if (input.connection)
                input.connection.path.setAttributeNS(null, "d", this.editor.createPathString(
                    this.editor.findSocketPos(input.connection.out.el.find(".outputsocket")),
                    this.editor.findSocketPos(input.el.find(".inputsocket"))));
        }

        for (var o in this.outputs) {
            var output = this.outputs[o];
            for (var i in output.connections) {
                var c = output.connections[i];
                c.path.setAttributeNS(null, "d", this.editor.createPathString(
                    this.editor.findSocketPos(output.el.find(".outputsocket")),
                    this.editor.findSocketPos(c.in.el.find(".inputsocket"))));
            }
        }

    }


    buildConnections(data, editor) {
        for (var o in this.outputs) {
            var output = this.outputs[o];
            if (!data.outputs[output.name])
                continue;
            for (var c in data.outputs[output.name].connections) {
                var con = data.outputs[output.name].connections[c];
                var input = null;
                for (var i in editor.nodes[con.node].inputs)
                    if (editor.nodes[con.node].inputs[i].name == con.input)
                        input = editor.nodes[con.node].inputs[i];
                if (input == null)
                    console.log("Could not find input " + con.input + " in node " + con.node);
                editor.buildConnection(editor.nodes[con.node], input, this, output);
            }
        }
    }


}

class NodeEditor {
    nodes = {}
    components = {}
    selectedNode = null;
    componentList = null;
    el = null;
    componentCategories = {};
    scale = 1;
    viewPos = [0, 0];

    constructor(element, componentList) {
        this.el = element;
        this.el.css("overflow: hidden");
        this.componentList = componentList;
        var editor = this;
        element.keydown(e => {
            if (e.keyCode == 46) //delete
            {
                if (editor.selectedNode != null) {
                    for (var i in editor.selectedNode.inputs) {
                        if (editor.selectedNode.inputs[i].connection) {
                            var con = editor.selectedNode.inputs[i].connection;
                            con.el.remove();
                            con.out.connections = con.out.connections.filter(e =>
                            {
                                return !(e.node == editor.selectedNode.id &&
                                    e.input == editor.selectedNode.inputs[i].name);
                            });
                            
                        }
                    }
                    for (var o in editor.selectedNode.outputs) {
                        for (var c in editor.selectedNode.outputs[o].connections) {
                            var con = editor.selectedNode.outputs[o].connections[c];
                            con.el.remove();
                            con.in.connection = null;
                        }
                    }
                            

                    editor.selectedNode.el.card.remove();
                    delete editor.nodes[editor.selectedNode.id];
                }
            }
        });
        this.el.append(this.nodeList);
        this.el.parent().bind("mousewheel", function (e) {
            var scroll = e.originalEvent.wheelDelta / 120;

            editor.scale *= (1 + 0.05 * scroll);
            element.css("transform", "translate(" + editor.viewPos[0] + "px, " + editor.viewPos[1] + "px) scale(" + editor.scale + ")");
            return false;
        });

        this.el.parent().mousedown(function (e) {
            if (e.preventDefault)
                e.preventDefault();
            var lastX = e.pageX;
            var lastY = e.pageY;

            function handle_dragging(e) {
                editor.viewPos[0] += (e.pageX - lastX);
                editor.viewPos[1] += (e.pageY - lastY);

                element.css("transform", "translate(" + editor.viewPos[0] + "px, " + editor.viewPos[1] + "px) scale(" + editor.scale + ")");
                lastX = e.pageX;
                lastY = e.pageY;
            }
            function handle_mouseup(e) {
                var mousePos = { left: editor.el.offset().left, top: editor.el.offset().top };
                mousePos.left = e.pageX - mousePos.left;
                mousePos.top = e.pageY - mousePos.top;
                $('body')
                    .off('mousemove', handle_dragging)
                    .off('mouseup', handle_mouseup);
            }
            $('body')
                .on('mouseup', handle_mouseup)
                .on('mousemove', handle_dragging);
            return false;
        });
        element.css("transform-origin", "0px 0px");
        element.css("transform", "translate(" + editor.viewPos[0] + "px, " + editor.viewPos[1] + "px) scale(" + editor.scale + ")");

    }


    mousePos(event) {
        return [
            (event.pageX - this.el.parent().offset().left - this.viewPos[0]) / this.scale ,
            (event.pageY - this.el.parent().offset().top  - this.viewPos[1]) / this.scale];
    }

    registerComponent(component) {
        this.components[component.name] = component;
        if (!this.componentCategories[component.cat]) {
            var header = $(`<li class="list-group-item bg-primary">` + component.cat + `</li>`);
            this.componentList.append(header);
            this.componentCategories[component.cat] = header;
        }

        var el = $(`<li class="list-group-item bg-dark" style="cursor: pointer">` + component.name + `</li>`);
        el.insertAfter(this.componentCategories[component.cat]);
        el.mousedown(e => {
            var newId = 1;
            while (editor.nodes[newId])
                newId++;

            editor.nodes[newId] = new Node(
                {
                    id: newId,
                    name: component.name,
                    position: [ 100,100 ]
                }, editor);


        });


    }


    findSocketPos(el) {
        var e = el;
        var position = {
            left: -editor.viewPos[0],
            top: -editor.viewPos[1]
        };
        while (!el.is(editor.el)) {
            var pos = el.position();
            position.left += pos.left;
            position.top += pos.top;
            el = el.parent();
        }
        position.left /= editor.scale;
        position.top /= editor.scale;
        if (e.hasClass("inputsocket")) {
            position.left -= 14; //center
            position.top -= 10;
        } else {
            position.left += 10; //center
            position.top -= 36;
        }


        return position;
    }

    createPathString(p1, p2) {
        return 'M ' +
            p1.left + ' ' + p1.top + ' C ' +
            (p1.left + Math.max(100, (p2.left-p1.left)/2)) + ' ' + p1.top + ' ' +
            (p2.left - Math.max(100, (p2.left-p1.left)/2)) + ' ' + p2.top + ' ' +
            p2.left + ' ' + p2.top;
    }

    empty() {
        this.el.empty();
        this.nodes = {};
        this.selectedNode = null;
    }

    fromJSON(obj) {
        this.el.empty();
        this.nodes = {};
        this.selectedNode = null;

        var deferreds = [];
        for (var n in obj.nodes) {
            var node = obj.nodes[n];
            this.nodes[node.id] = new Node(node, this);
            if (this.nodes[node.id].waiter)
                deferreds.push(this.nodes[node.id].waiter);
        }


        $.when.apply($, deferreds).done(function () {
            for (var n in obj.nodes) {
                var node = obj.nodes[n];
                editor.nodes[node.id].buildConnections(node, editor);
            }
        });
    }

    toJSON() {
        var nodes = {};
        for (var n in this.nodes)
            nodes[n] = this.nodes[n].toJSON();
        return {
            id: "demo@0.1.0",
            nodes: nodes,
            comments: []
            };
    }

    buildConnection(nodeIn, socketIn, nodeOut, socketOut) {
        var connection = {};
        connection.node = nodeIn.id;
        connection.input = socketIn.name;
        connection.in = socketIn;
        connection.el = $('<svg class="connection"></svg>');
        connection.path = document.createElementNS("http://www.w3.org/2000/svg", "path");
        connection.path.setAttributeNS(null, "class", "main-path");
        connection.path.setAttributeNS(null, "d", editor.createPathString(
            editor.findSocketPos(socketOut.el.find(".outputsocket")),
            editor.findSocketPos(socketIn.el.find(".inputsocket"))));
        connection.el.append(connection.path);
        editor.el.append(connection.el);

        socketOut.connections.push(connection);

        socketIn.connection = {
            node: nodeOut.id,
            output: socketOut.name,
            el: connection.el,
            path: connection.path,
            out: socketOut
        };
    }
}
