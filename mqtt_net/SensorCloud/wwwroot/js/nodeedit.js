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
    }
    build() {
        for (var c in this.controls)
            this.controls[c].build();
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
        var node = this;
        this.el.card.find(".outputsocket").mousedown(function (e) {
            var output = null;
            for (var o in node.outputs)
                if (node.outputs[o].el.find(".outputsocket").is($(this)))
                    output = node.outputs[o];

            if (e.preventDefault)
                e.preventDefault();

            var position = editor.findSocketPos($(this));

            var mousePos = { left: editor.el.offset().left, top: editor.el.offset().top };
            mousePos.left = e.pageX - mousePos.left;
            mousePos.top = e.pageY - mousePos.top;

            window.my_dragging = {};
            my_dragging.svg = $('<svg class="connection"></svg>');
            my_dragging.position = position;

            my_dragging.path = document.createElementNS("http://www.w3.org/2000/svg", "path");
            my_dragging.svg.append(my_dragging.path);
            my_dragging.path.setAttributeNS(null, "class", "main-path");
            my_dragging.path.setAttributeNS(null, "d", editor.createPathString(position, mousePos));
            editor.el.append(my_dragging.svg);
            
            function handle_dragging(e) {
                var mousePos = { left: editor.el.offset().left, top: editor.el.offset().top };
                mousePos.left = e.pageX - mousePos.left;
                mousePos.top = e.pageY - mousePos.top;

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

                editor.buildConnection(node, foundInput, foundNode, output);
            }
            $('body')
                .on('mouseup', handle_mouseup)
                .on('mousemove', handle_dragging);
            return false;
        });


        editor.el.append(this.el.card);
    }


    handle_dragnode(e) {
        var node = e.data;
        window.my_dragging = {};
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
    }

    addOutput(output, data) {
        this.outputs.push(output);
        output.el = $(`<li class="list-group-item bg-dark">` + output.title + `<br /></li>`);
        output.control = output.build();
        output.el.append(output.control);
        if(data && data.data && data.data[output.name])
            output.value = data.data[output.name];
        output.el.append($(`<div class="outputsocket"></div>`));
        this.el.connections.append(output.el);
        return this;
    }
    addInput(input, data) {
        this.inputs.push(input);
        input.el = $(`<li class="list-group-item bg-dark"><div class="inputsocket"></div>` + input.title + `<br /></li>`);
        input.control = input.build();
        input.el.append(input.control);
        if(data && data.data && data.data[input.name])
            input.value = data.data[input.name];
        this.el.connections.append(input.el);
        return this;
    }


    updateConnectionPositions() {
        for (var i in this.inputs) {
            var input = this.inputs[i];
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
            for (var c in data.outputs[output.name].connections) {
                var con = data.outputs[output.name].connections[c];
                var input = null;
                for (var i in editor.nodes[con.node].inputs)
                    if (editor.nodes[con.node].inputs[i].name == con.input)
                        input = editor.nodes[con.node].inputs[i];
                editor.buildConnection(editor.nodes[con.node], input, this, output);
            }
        }
    }


}

class NodeEditor {
    nodes = {}
    components = {}

    constructor(element) {
        this.el = element;
    }

    registerComponent(component) {
        this.components[component.name] = component;
    }


    findSocketPos(el) {
        var e = el;
        var position = { left: 0, top: 0 };
        while (!el.is(editor.el)) {
            var pos = el.position();
            position.left += pos.left;
            position.top += pos.top;
            el = el.parent();
        }
        if (e.hasClass("inputsocket")) {
            position.left -= 14; //center
            position.top -= 10;
        } else {
            position.left += 14; //center
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


    fromJSON(obj) {
        this.el.empty();
        for (var n in obj.nodes) {
            var node = obj.nodes[n];
            this.nodes[node.id] = new Node(node, this);
        }
        for (var n in obj.nodes) {
            var node = obj.nodes[n];
            this.nodes[node.id].buildConnections(node, this);
        }
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
