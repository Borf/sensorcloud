var VueNumControl = {
    props: ['readonly', 'emitter', 'ikey', 'getData', 'putData'],
    template: '<input type="number" :readonly="readonly" :value="value" @input="change($event)" @dblclick.stop="" @pointermove.stop=""/>',
    data() {
        return {
            value: 0,
        }
    },
    methods: {
        change(e) {
            this.value = +e.target.value;
            this.update();
        },
        update() {
            if (this.ikey)
                this.putData(this.ikey, this.value);
            this.emitter.trigger('process');
        }
    },
    mounted() {
        this.value = this.getData(this.ikey);
    }
}

class NumControl extends Rete.Control {

    constructor(emitter, key, readonly) {
        super(key);
        this.component = VueNumControl;
        this.props = { emitter, ikey: key, readonly };
    }

    setValue(val) {
        this.vueContext.value = val;
    }
}


var VueTextControl = {
    props: ['readonly', 'emitter', 'ikey', 'getData', 'putData'],
    template: '<input :readonly="readonly" :value="value" @input="change($event)" @dblclick.stop="" @pointermove.stop=""/>',
    data() {
        return {
            value: "",
        }
    },
    methods: {
        change(e) {
            this.value = e.target.value;
            this.update();
        },
        update() {
            if (this.ikey)
                this.putData(this.ikey, this.value);
            this.emitter.trigger('process');
        }
    },
    mounted() {
        this.value = this.getData(this.ikey);
    }
};

class TextControl extends Rete.Control {

    constructor(emitter, key, readonly) {
        super(key);
        this.component = VueTextControl;
        this.props = { emitter, ikey: key, readonly };
    }

    setValue(val) {
        this.vueContext.value = val;
    }
}

var VueSelectControl = {
    props: ['label', 'items', 'emitter', 'ikey', 'callback', 'getData', 'putData'],
    template: 
`<div> 
    {{ label }} <br/>
    <select :value="value" @input="change($event)" @dblclick.stop="" @pointermove.stop="">
        <option v-for="item in items">{{ item }}</option>
    </select>
</div>`,
    data() {
        return {
            value: ""
        };
    },
    methods: {
        change(e) {
            this.value = e.target.value;
            this.update();
            this.callback(this);//e.target.value);
        },
        update() {
            if (this.ikey)
                this.putData(this.ikey, this.value);
            this.emitter.trigger('process');
        }
    },
    mounted() {
        this.value = this.getData(this.ikey);
    }
};



class SelectControl extends Rete.Control {

    constructor(emitter, key, label, items, changecallback) {
        super(key);
        this.component = VueSelectControl;
        this.props = { label, items, emitter, ikey: key, callback: changecallback };
    }

    setValue(val) {
        this.vueContext.value = val;
    }
}








const numSocket = new Rete.Socket('Number value');
const textSocket = new Rete.Socket('Text value');
const actionSocket = new Rete.Socket('Action');


class NumComponent extends Rete.Component {

    constructor() {
        super("Number");
    }

    builder(node) {
        var out1 = new Rete.Output('num', "Number", numSocket);

        return node.addControl(new NumControl(this.editor, 'num')).addOutput(out1);
    }

    worker(node, inputs, outputs) {
        outputs['num'] = node.data.num;
    }
}

class TextComponent extends Rete.Component {

    constructor() {
        super("Text");
    }

    builder(node) {
        var out1 = new Rete.Output('text', "Text", textSocket);

        return node.addControl(new TextControl(this.editor, 'text')).addOutput(out1);
    }

    worker(node, inputs, outputs) {
        outputs['text'] = node.data.text;
    }
}


class LogComponent extends Rete.Component {
    constructor() {
        super('Log');
    }

    builder(node) {
        let inp = new Rete.Input('num', 'Number', numSocket);
        inp.addControl(new NumControl(this.editor, 'num'))
        node.addInput(inp);

        return node;
    }

    worker(node, inputs, outputs) {
        inputs['num'] = node.data.num;
    }
}

class TelegramMessageComponent extends Rete.Component {
    constructor() {
        super('Send Telegram Message');
    }

    builder(node) {
        let inp = new Rete.Input('text', 'Text', textSocket);
        inp.addControl(new TextControl(this.editor, 'text'));
        node.addInput(inp);

        inp = new Rete.Input('trigger', 'trigger', actionSocket);
        node.addInput(inp);
        return node;
    }

    worker(node, inputs, outputs) {
        inputs['text'] = node.data.text;
    }
}

class TelegramMessageReceivedComponent extends Rete.Component {
    constructor() {
        super('Receive Telegram Message');
    }

    builder(node) {
        let out = new Rete.Output('text', 'Message', textSocket);
        node.addOutput(out);

        out = new Rete.Output('trigger', 'trigger', actionSocket);
        node.addOutput(out);
        return node;
    }

    worker(node, inputs, outputs) {
        outputs['text'] = node.data.text;
    }
}

class MqttSubscribeComponent extends Rete.Component {
    constructor() {
        super('Mqtt Subscribe');
    }

    builder(node) {
        node.addInput(new Rete.Input('topic', 'Topic', textSocket));
        node.addOutput(new Rete.Output('payload', 'Payload', textSocket));
        node.addOutput(new Rete.Output('trigger', 'trigger', actionSocket));
        return node;
    }

    worker(node, inputs, outputs) {
        outputs['payload'] = node.data.payload;
    }
}

class MqttPublishComponent extends Rete.Component {
    constructor() {
        super('Mqtt Publish');
    }

    builder(node) {
        node.addInput(new Rete.Input('topic', 'Topic', textSocket));
        node.addInput(new Rete.Input('payload', 'Payload', textSocket));
        node.addInput(new Rete.Input('trigger', 'trigger', actionSocket));
        return node;
    }

    worker(node, inputs, outputs) {
        outputs['payload'] = node.data.payload;
    }
}
var bla;
class ModuleActionComponent extends Rete.Component {
    constructor() {
        super('Module Action');
    }

    builder(node) {
        var actionControl = new SelectControl(this.editor, 'action', "Action", ["--------"]);
        node.addInput(new Rete.Input('trigger', 'trigger', actionSocket));
        node.addControl(new SelectControl(this.editor, 'module', "Module", ["mqtt", "telegram", "onkyo", "sensorcloud"],
            function (control)
            {
                var sel = $(actionControl.vueContext.$el).find("select");
                bla = sel;
                sel.empty();
                function addOptions(options) {
                    for (var i in options) {
                        sel.append($('<option value="' + options[i] + "'>" + options[i] + "</option>"));
                    }
                }
                if (control.value == "mqtt")
                    addOptions(['publish', 'poop']);
                else
                    addOptions(['pee', 'poop']);


            }));
        node.addControl(actionControl);
        return node;
    }

    worker(node, inputs, outputs) {
    }
}


class ModuleTriggerComponent extends Rete.Component {
    constructor() {
        super('Module Trigger');
    }

    builder(node) {
        node.addOutput(new Rete.Output('trigger', 'trigger', actionSocket));
        return node;
    }

    worker(node, inputs, outputs) {
    }
}