fs = require('fs');
require("json-circular-stringify");

const transformer = require("../prism-master/tests/helper/token-stream-transformer");

const common = require('./common');


function exportLanguages(list) {

    console.log('Exporting all languages...');

    console.log(list);

    for (var lang of list) {
        console.log(lang);
        common.reloadPrism();
        if (lang === 'meta') continue;
        loadLanguages([lang]);
        // patchLanguage(Prism, lang);
        saveLanguageGrammar(lang);
    }
    console.log('DONE.');
}

function exportAllLanguages() {
    var list = common.getLanguageNames();
    exportLanguages(list);
}

function exportLanguageNames() {

    common.reloadPrism();

    var componentsJson = require('prismjs/components.json');

    var list = Object.entries(componentsJson.languages).filter(x => x[1].title !== undefined);

    var clike = ['clike', 'actionscript', 'arduino', 'birb', 'bison', 'c', 'cfscript', 'chaiscript', 'coffeescript', 'cpp', 'crystal', 'csharp', 'd', 'dart', 'firestore-security-rules', 'flow', 'fsharp', 'glsl', 'gml', 'go', 'groovy', 'haxe', 'hlsl', 'java', 'javascript', 'jolie', 'kotlin', 'mongodb', 'n4js', 'objectivec', 'opencl', 'processing', 'protobuf', 'purebasic', 'qore', 'qsharp', 'reason', 'ruby', 'scala', 'solidity', 'sqf', 'squirrel', 'tt2', 'typescript', 'v', 'vala'];

    var res = [];
    for (item of list) {
        loadLanguages([item[0]]);
        if (Prism.languages[item[0]] === undefined) {
            continue;
        }

        var descriptor = {
            name: item[0], title: item[1].title, aliases: typeof item[1].alias === 'string' || item[1].alias instanceof String ? [item[1].alias] : item[1].alias,
            rangeTokenizationSettings: {
                preRange: 2000,
                postRange: 2000,
                safePoints: [ '\r\n', '\n'],
                safePointAdjusterName: null
            }
        };

        if (clike.includes(item[0]))  {
            descriptor.rangeTokenizationSettings.safePointAdjusterName = 'clike';
            descriptor.rangeTokenizationSettings.preRange = 0;
            descriptor.rangeTokenizationSettings.postRange = 0;
        }

        res.push(descriptor);
    }
    fs.writeFileSync("../PrismSharp/data/languages/languages.json", JSON.stringify(res, null, 2), function (err) {
        if (err)
        if (err)
            return console.log(err);
    });
}

function saveLanguageGrammar(lang) {
    var res = JSON.stringify(Prism.languages[lang], replacer, 2);
    if (res === undefined) {
        return;
    }
    var path = '../PrismSharp/data/languages/' + lang + '.json';
    fs.writeFileSync(path, res, function (err) {
        if (err)
            return console.log(err);
    });
}

function replacer(key, value) {
    if (typeof value === 'string' || value instanceof String) {
        value = ("s:" + value.toString());
    }

    if (value instanceof RegExp) {

        value = translateToCSharp(value.toString());
        value = ("r:" + value.toString());
    }
    return value;
}

function translateToCSharp(pattern) {
    var unescapedDot = /(?<!\\)(?:\\\\)*(\.)/g;
    var unescapedDollar = /(?<!\\)(?:\\\\)*(\$)/g;
    var charRange = /(?<!\\)(?:\\\\)*\[(((?<!\\)\\])|((?<!\\)\\\\\\])|([^\]]))+\]/g;
    var emptyRange = /(?<!\\)(?:\\\\)*(\[\])/g;
    var emptyNegativeRange = /(?<!\\)(?:\\\\)*(\[\^\])/g;

    var toFix = [];

    var rangeMatches = Array.from(pattern.matchAll(charRange));
    var dotMatches = pattern.matchAll(unescapedDot);
    var dollarMatches = pattern.matchAll(unescapedDollar);
    var erMatches = pattern.matchAll(emptyRange);
    var enrMatches = pattern.matchAll(emptyNegativeRange);

    addNonRangedMatches(toFix, dotMatches, rangeMatches, '.');
    addNonRangedMatches(toFix, dollarMatches, rangeMatches, '$');
    addNonRangedMatches(toFix, erMatches, rangeMatches, '[');
    addNonRangedMatches(toFix, enrMatches, rangeMatches, '^');

    if (toFix.length > 1) {
        console.log();
    }

    toFix.sort((a, b) => b[0] - a[0]);

    for (var f of toFix) {
        patternStart = pattern.slice(0, f[0]);
        patternEnd = pattern.slice(f[0] + 1);

        switch (f[1]) {
            case '.':
                pattern = patternStart + "[^\\r\\n]" + patternEnd;
                break;

            case '$':
                pattern = patternStart + "(?:(?=\\r$)|$)" + patternEnd;
                break;

            case '[':
                patternEnd = patternEnd.slice(1);
                pattern = patternStart + "[^\\s\\S]" + patternEnd;
                break;

            case '^':
                patternEnd = patternEnd.slice(2);
                pattern = patternStart + "[\\s\\S]" + patternEnd;
                break;
        }
    }

    return pattern;

}

function addNonRangedMatches(toFix, matches, rangeMatches, id) {
    if (matches === null) return;
    for (var dm of matches) {
        var consumed = false;
        for (var rm of rangeMatches) {
            if (dm.index + dm[0].length - dm[1].length > rm.index && dm.index + dm[0].length - dm[1].length + dm[1].length <= rm.index + rm[0].length) {
                consumed = true;
            }
        }
        if (!consumed) toFix.push([dm.index + dm[0].length - dm[1].length, id]);
    }
}




module.exports = { exportLanguages, exportAllLanguages, exportLanguageNames, };