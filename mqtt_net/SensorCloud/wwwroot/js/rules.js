var numSocket = new Socket("Number");
var textSocket = new Socket("Text");
var actionSocket = new Socket("Action");



class NumberControl {
    el = null;
    build() {
        this.el = $(`<input type="number">`);
        return this.el;
    }
    get value() { return parseInt(this.el.val()); }
    set value(newValue) { this.el.val(+newValue) }
}

class TextControl {
    el = null;
    build() {
        this.el = $(`<input>`);
        return this.el;
    }
    get value() { return this.el.val(); }
    set value(newValue) { this.el.val(newValue) }
}

class SelectControl {
    options = [];
    constructor() {
        if (arguments.length > 0)
            this.options = arguments[0];
    }
    el = null;
    build() {
        this.el = $(`<select></select>`);
        for (var i in this.options)
            this.el.append($(`<option>`+this.options[i]+`</option>`));

        return this.el;
    }
    get value() { return this.el.val(); }
    set value(newValue) { this.el.val(newValue); }
}


class NumberComponent extends Component {
    constructor() {
        super("Number");
    }
    cat = "Inputs";

    build(node, data) {
        var out1 = new Output('num', "Value", numSocket);
        out1.addControl(new NumberControl());
        //return node.addControl(new NumControl(this.editor, 'num')).addOutput(out1);
        return node.addOutput(out1, data);
    }
}

class TextComponent extends Component {
    constructor() {
        super("Text");
    }
    cat = "Inputs";

    build(node, data) {
        var out1 = new Output('text', "Value", textSocket);
        out1.addControl(new TextControl());
        return node.addOutput(out1, data);
    }
}


class TelegramReceiveMessage extends Component {
    constructor() {
        super("Receive Telegram Message");
    }
    cat = "Telegram";

    build(node, data) {
        return node
            .addOutput(new Output('text', 'Message', textSocket))
            .addOutput(new Output('trigger', 'trigger', actionSocket));
    }
}

class TelegramSendMessage extends Component {
    constructor() {
        super("Send Telegram Message");
    }
    cat = "Telegram";

    build(node, data) {
        return node
            .addInput(new Input('text', 'Message', textSocket))
            .addInput(new Input('trigger', 'trigger', actionSocket));
    }
}



class MqttSubscribeComponent extends Component {
    constructor() {
        super("Mqtt Subscribe");
    }
    cat = "Mqtt";

    build(node, data) {
        return node
            .addOutput(new Output('trigger', 'trigger', actionSocket))
            .addOutput(new Output('payload', 'Payload', textSocket))
            .addInput(new Input('topic', 'Topic', textSocket));
    }
}

class MqttPublishComponent extends Component {
    constructor() {
        super("Mqtt Publish");
    }
    cat = "Mqtt";

    build(node, data) {
        return node
            .addInput(new Output('trigger', 'trigger', actionSocket))
            .addInput(new Input('topic', 'Topic', textSocket))
            .addInput(new Input('payload', 'Payload', textSocket));
    }
}


class AddComponent extends Component {
    constructor() {
        super("Add");
    }
    cat = "Operations";

    build(node, data) {
        var out1 = new Output('num', "Value", numSocket);
        var in1 = new Input('num', 'Number 1', numSocket);
        var in2 = new Input('num2', 'Number 2', numSocket);
        return node.addOutput(out1, data).addInput(in1, data).addInput(in2, data);
    }
}


class IfComponent extends Component {
    constructor() {
        super("If");
    }
    cat = "Operations";

    build(node, data) {
        node
            .addInput(new Input('trigger', 'Trigger', actionSocket), data)
            .addOutput(new Output('trigger', 'Trigger', actionSocket), data)
            .addInput(new Input('val1', 'Value 1', textSocket), data)
            .addInput(new Input('comparator', 'Comparator', textSocket)
                .addControl(new SelectControl(['equals', 'not equal', 'larger than', 'smaller than'])), data)
            .addInput(new Input('val2', 'Value 2', textSocket), data);


        return node;
    }
}



class ModuleActionComponent extends Component {
    constructor() {
        super("Module functions");
    }
    cat = "Module";

    build(node, data) {
        var c = this;
        node
            .addInput(new Input('trigger', 'Trigger', actionSocket))
            .addInput(new Input('module', 'Module', textSocket)
                .addControl(this.moduleSelect = new SelectControl()))
            .addInput(new Input('function', 'Function', textSocket)
                .addControl(this.functionSelect = new SelectControl()));

        node.waiter = $.ajax({
            url: apiurl + "rules/functions",
            dataType: "json",
            success: function (d) {
                c.moduleSelect.el.empty();
                var modules = [];
                for (var i in d) {
                    var module = d[i].Module;
                    if (modules.indexOf(module) == -1) {
                        modules.push(module);
                        c.moduleSelect.el.append($("<option>"+module+"</option>"));
                    }
                }

                c.moduleSelect.el.change(e => {
                    c.functionSelect.el.empty();
                    for (var i in d) {
                        if (d[i].Module == c.moduleSelect.el.val()) {
                            c.functionSelect.el.append($("<option>" + d[i].FunctionName + "</option>"));
                        }
                    }
                    c.functionSelect.el.change();
                });

                c.functionSelect.el.change(e => {
                    while (node.inputs.length > 3) {
                        node.removeInput(3);
                    }
                    for (var i in d) {
                        if (d[i].Module == c.moduleSelect.el.val() && d[i].FunctionName == c.functionSelect.el.val()) {
                            for (var p in d[i].Parameters) {
                                var pp = d[i].Parameters[p];
                                var sock = null;
                                if (pp.Item2.name == "Text")
                                    sock = textSocket;
                                else if (pp.Item2.name == "Number")
                                    sock = numSocket;
                                node.addInput(new Input(pp.Item1, pp.Item1, sock));
                            }
                        }
                    }
                });

                if (data && data.data && data.data.module) {
                    c.moduleSelect.value = data.data.module;
                    c.moduleSelect.el.change();
                }
                if (data && data.data && data.data.function)
                    c.functionSelect.value = data.data.function;

            }
        });
        
        return node;
    }
}

class ModuleTriggerComponent extends Component {
    constructor() {
        super("Module triggers");
    }
    cat = "Module";

    build(node, data) {
        return node
            .addOutput(new Output('trigger', 'Trigger', actionSocket))
            .addInput(new Input('topic', 'Topic', textSocket))
            .addInput(new Input('payload', 'Payload', textSocket));
    }
}

