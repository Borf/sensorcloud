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

class TextComponent extends Component {
    constructor() {
        super("Text");
    }

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

    build(node, data) {
        return node
            .addInput(new Input('text', 'Message', textSocket))
            .addInput(new Input('trigger', 'trigger', actionSocket));
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