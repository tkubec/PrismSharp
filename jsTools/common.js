function reloadPrism() {

    globalThis.Prism = null;
    global.Prism - null;
    globalThis.loadLanguages = null;
    global.loadLanguages = null;

    Object.keys(require.cache).forEach(function (key) {
        if (key.indexOf('node_modules\\prismjs\\') > -1)
            delete require.cache[key]
    })

    globalThis.Prism = require('prismjs');
    globalThis.loadLanguages = require('prismjs/components/');

    global.Prism = globalThis.Prism;
    global.loadLanguages = globalThis.loadLanguages;
}


function getLanguageNames()
{
    var componentsJson = require('prismjs/components.json');
    return list = Object.entries(componentsJson.languages).filter(x => x[1].title !== undefined)
    .map(x => x[0]);

}

module.exports = {reloadPrism, getLanguageNames}