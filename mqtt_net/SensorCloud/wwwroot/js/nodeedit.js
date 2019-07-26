function handle_dragnode(e) {
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

class Socket {
    name = "";
    constructor(name) {
        this.name = name;
    }
}

class Input {
    controls = [];
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

class NumberControl {
    el = null;
    build() {
        this.el = $(`<input type="number">`);
        return this.el;
    }
    get value() { return parseInt(this.el.val()); }
    set value(newValue) { this.el.val(+newValue) }
}

class Node {
    el = {};
    name = "";
    inputs = [];
    outputs = [];
    data = {}

    constructor(data, editor) {
        console.log(data);

        this.name = data.name;
        this.id = data.id;

        this.component = editor.components[this.name];

        this.el.card = $(`<div class="node card bg-dark text-white">`);
        this.el.card.css({ left: data.position[0], top: data.position[1] });

        this.el.card.append($(`<div class="card-header">` + data.name + `</div>`));
        this.el.connections = $(`<ul class="list-group list-group-flush"/>`);
        this.el.card.append(this.el.connections);
        this.component.build(this, data);
        this.el.card.mousedown(handle_dragnode);


        this.el.card.find(".outputsocket").mousedown(function (e) {
            if (e.preventDefault)
                e.preventDefault();

            function findSocketPos(el) {
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
            var position = findSocketPos($(this));

            var mousePos = { left: editor.el.offset().left, top: editor.el.offset().top };
            mousePos.left = e.pageX - mousePos.left;
            mousePos.top = e.pageY - mousePos.top;

            window.my_dragging = {};
            my_dragging.svg = $('<svg class="connection"></svg>');
            my_dragging.position = position;

            my_dragging.path = document.createElementNS("http://www.w3.org/2000/svg", "path");
            my_dragging.svg.append(my_dragging.path);
            my_dragging.path.setAttributeNS(null, "class", "main-path");
            my_dragging.path.setAttributeNS(null, "d", 'M ' +
                position.left + ' ' + position.top + ' C ' +
                mousePos.left + ' ' + position.top + ' ' +
                position.left + ' ' + mousePos.top + ' ' +
                mousePos.left + ' ' + mousePos.top);
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
                        var position = findSocketPos(input.el.find(".inputsocket"));
                        if (Math.abs(mousePos.left - position.left) < 32 &&
                            Math.abs(mousePos.top - position.top) < 32) {
                            foundInput = input;
                            foundNode = node;
                        }
                    });
                });

                console.log(foundNode);


                if (!foundNode)
                    my_dragging.svg.remove();

                $('body')
                    .off('mousemove', handle_dragging)
                    .off('mouseup', handle_mouseup);
            }
            $('body')
                .on('mouseup', handle_mouseup)
                .on('mousemove', handle_dragging);
            return false;
        });


        editor.el.append(this.el.card);
    }


    addOutput(output, data) {
        this.outputs.push(output);
        output.el = $(`<li class="list-group-item bg-dark">` + output.title + `<br /></li>`);
        output.control = output.build();
        output.el.append(output.control);
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
        input.value = data.data[input.name];
        this.el.connections.append(input.el);
        return this;
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



    fromJSON(obj) {
        this.el.empty();
        for (var n in obj.nodes) {
            var node = obj.nodes[n];
            this.nodes[node.id] = new Node(node, this);
        }
    }
}



var numSocket = new Socket("Number");


class NumberComponent extends Component {
    constructor() {
        super("Number");
    }

    build(node, data) {
        var out1 = new Output('num', "Value", numSocket);
        out1.addControl(new NumberControl());
        //return node.addControl(new NumControl(this.editor, 'num')).addOutput(out1);
        return node.addOutput(out1, data);
    }

}


class AddComponent extends Component {
    constructor() {
        super("Add");
    }

    build(node, data) {
        var out1 = new Output('num', "Value", numSocket);
        var in1 = new Input('num', 'Number 1', numSocket);
        var in2 = new Input('num2', 'Number 2', numSocket);
        return node.addOutput(out1, data).addInput(in1, data).addInput(in2, data);
    }

}