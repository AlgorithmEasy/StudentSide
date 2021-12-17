/**
 * 自定义菜单风格。
 */
class CustomCategory extends Blockly.ToolboxCategory {
    /**
     * @override
     */
    constructor(categoryDef, toolbox, optParent) {
        super(categoryDef, toolbox, optParent);
    }

    /**
     * @override
     */
    addColourBorder_(colour) {
        this.rowDiv_.style.backgroundColor = colour;
    }
    /**
     * @override
     */
    setSelected(isSelected) {
        // We do not store the label span on the category,
        // so use getElementsByClassName.
        const labelDom = this.rowDiv_.getElementsByClassName('blocklyTreeLabel')[0];
        if (isSelected) {
            // Change the background color of the div to white.
            this.rowDiv_.style.backgroundColor = 'white';
            this.iconDom_.style.color = this.colour_;
            // Set the colour of the text to the colour of the category.
            labelDom.style.color = this.colour_;
        } else {
            // Set the background back to the original colour.
            this.rowDiv_.style.backgroundColor = this.colour_;
            this.iconDom_.style.color = 'white';
            // Set the text back to white.
            labelDom.style.color = 'white';
        }
        // This is used for accessibility purposes.
        Blockly.utils.aria.setState((this.htmlDiv_),
            Blockly.utils.aria.State.SELECTED, isSelected);
    }
}

/**
 * 更新生成 python 代码函数。
 * 用于监听 workspace, 变动时即重新生成代码。
 */
Blockly.workspaceUpdate = function () {
    const div = document.getElementById("python-code");
    const code = Blockly.workspaceToPython();
    if (code === "") {
        div.innerHTML = "";
        return;
    }
    div.innerHTML = hljs.highlight(code, { language: 'python' }).value;
    hljs.lineNumbersBlock(div);
};

/**
 * 将 workspace 中的块生成为 Python 代码。
 * @return {string} 生成的 Python 代码。
 */
Blockly.workspaceToPython = function () {
    return Blockly.Python.workspaceToCode(Blockly.workspace);
};

/**
 * 加载 xml 格式的 workspace.
 * @param {string} xml workspace 的 xml 字符串。
 */
Blockly.importWorkspace = function (xml) {
    Blockly.workspace.clear();
    if (xml)
        Blockly.Xml.domToWorkspace(Blockly.Xml.textToDom(xml), Blockly.workspace);
};

/**
 * 导出 xml 格式的 workspace.
 * @return {string} workspace 的 xml 字符串。
 */
Blockly.exportWorkspace = function () {
    const xml = Blockly.Xml.workspaceToDom(Blockly.workspace);
    return Blockly.Xml.domToText(xml);
};

/**
 * 清空 workspace.
 */
Blockly.clearWorkspace = function () {
    Blockly.workspace.clear();
};

Blockly.resizeWorkspace = (mask) => {
    const blocklyArea = document.getElementById('blockly');
    let element = blocklyArea;
    if (!element || !Blockly.workspace) return;
    let x = 0;
    let y = 0;
    do {
        x += element.offsetLeft;
        y += element.offsetTop;
        element = element.offsetParent;
    } while (element);
    if (x === 0 && y === 0) return;

    const blocklyDiv = document.getElementById(Blockly.blocklyDiv);
    blocklyDiv.style.left = x + 'px';
    blocklyDiv.style.top = y + 'px';
    blocklyDiv.style.width = blocklyArea.offsetWidth + 'px';
    blocklyDiv.style.height = blocklyArea.offsetHeight + 'px';
    Blockly.svgResize(Blockly.workspace);
    if (mask) {
        const blocklyMask = document.getElementById('blocklyMask');
        blocklyMask.style.left = x + 'px';
        blocklyMask.style.top = y + 'px';
        blocklyMask.style.width = blocklyArea.offsetWidth + 'px';
        blocklyMask.style.height = blocklyArea.offsetHeight + 'px';
    }
}

/**
 * 设置全局饱和度和亮度。
 */
Blockly.configColour = function () {
    Blockly.HSV_SATURATION = 0.6; // 饱和度, 范围 [0, 1], 默认为 0.45
    Blockly.HSV_VALUE = 0.8; // 亮度, 范围 [0, 1], 默认为 0.8
};

/**
 * 创建全局主题。
 * 主题中定义了块，菜单的颜色等属性。
 */
Blockly.configTheme = function () {
    if (Blockly.Themes.myTheme) {
        return;
    }
    const blockStyle = { // 块主题
        "logic_blocks": {
            "colourPrimary": 204
        },
        "loop_blocks": {
            "colourPrimary": 240
        },
        "math_blocks": {
            "colourPrimary": 150
        },
        "text_blocks": {
            "colourPrimary": 168
        },
        "variable_blocks": {
            "colourPrimary": 20
        },
        "list_blocks": {
            "colourPrimary": 330
        },
        "colour_blocks": {
            "colourPrimary": 24
        },
        "procedure_blocks": {
            "colourPrimary": 60
        },
        "expand_blocks": {
            "colourPrimary": 270
        }
    };
    // 菜单主题
    const categoryStyle = {
        "logic_category": {
            "colour": 204
        },
        "loop_category": {
            "colour": 240
        },
        "math_category": {
            "colour": 150
        },
        "text_category": {
            "colour": 168
        },
        "variable_category": {
            "colour": 20
        },
        "list_category": {
            "colour": 330
        },
        "colour_category": {
            "colour": 24
        },
        "procedure_category": {
            "colour": 60
        },
        "expand_category": {
            "colour": 270
        }
    };
    // 组件主题
    const componentStyle = {};
    Blockly.Themes.myTheme = Blockly.Theme.defineTheme("myTheme", {
        "base": Blockly.Themes.Classic,
        "blockStyles": blockStyle,
        "categoryStyles": categoryStyle,
        "componentStyles": componentStyle,
        "startHats": true
    });
};

/**
 * 创建自定义菜单。
 */
Blockly.configCategory = function () {
    Blockly.registry.register(
        Blockly.registry.Type.TOOLBOX_ITEM,
        Blockly.ToolboxCategory.registrationName,
        CustomCategory, true);
};

/**
 * 程序入口。<br/>
 * 先设置全局颜色、主题和菜单, 随后注入 workspace.
 * @param { string } blockDiv 待注入的 div id.
 */
Blockly.start = function (blockDiv) {
    // 覆写 Blockly 的 alert 和 prompt 实现.
    Blockly.blocklyDiv = blockDiv;
    Blockly.alert = function(message, opt_callback) {
        Swal.fire(message).then(() => {
            if (opt_callback) {
                opt_callback();
            }
        });
    };
    Blockly.prompt = function(message, defaultValue, callback) {
        Swal.fire({
            title: message,
            input: "text",
            inputAttributes: {
                autocapitalize: "off"
            },
            showCancelButton: true
        }).then((result) => {
            if (result.isConfirmed) {
                callback(result.value);
            }
        });
    };

    const xhttp = new XMLHttpRequest();
    xhttp.open("get", "../lib/blockly/blockly.xml", true);
    xhttp.onreadystatechange = function () {
        if (this.readyState === 4 && this.status === 200) {
            Blockly.configColour();
            Blockly.configTheme();
            Blockly.configCategory();

            // Create main workspace.
            Blockly.workspace = Blockly.inject(blockDiv,
                {
                    toolbox: xhttp.responseText,
                    theme: Blockly.Themes.myTheme,
                    media: "./lib/blockly/media/",
                    renderer: "zelos"
                });
            Blockly.workspace.addChangeListener(Blockly.workspaceUpdate);
            window.addEventListener('resize', Blockly.resizeWorkspace, false);
        }
    };
    xhttp.send();
};
