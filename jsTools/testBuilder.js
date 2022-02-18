fs = require('fs');
require("json-circular-stringify");
const transformer = require("../prism-master/tests/helper/token-stream-transformer");

const common = require('./common');


function recompileTest(sourceFileName, targetFilename, language, options) {
    var content = fs.readFileSync(sourceFileName, 'utf8');
    var code = content.match(/(^.*?\n)-{20,}/s)[1];
    loadLanguages([language]);
   // code = code.replace(/\r/g, '');

    code = code.trim();
    
    var env = {
        code: code,
        grammar: Prism.languages[language],
        language: language
    };

    if (options.hooks) Prism.hooks.run('before-tokenize', env);
    try {
        env.tokens  = Prism.tokenize(code, Prism.languages[language]);
        if (options.hooks) Prism.hooks.run('after-tokenize', env);
        var pp = transformer.prettyprint(env.tokens, "\t");
    }
    catch
    {
        console.log('PrismJS tokenize failed');
        return;
    }


    var output = code + '\r\n----------------------------------------------------\r\n' + pp + '\r\n\r\n----------------------------------------------------';

    fs.writeFileSync(targetFilename, output, function (err) {
        if (err)
            return console.log(err);
    });
}


function recompileHtmlTest(sourceFileName, targetFilename, language) {
    var content = fs.readFileSync(sourceFileName, 'utf8');
    var code = content.match(/(^.*?\n)-{20,}/s)[1];
    loadLanguages([language]);
    Prism.hooks.all.wrap = [];
   // code = code.replace(/\r/g, '');

    // code = code.replace(/\n/g, '\r\n');
    code = code.trim();


    //Prism.hooks.run('before-tokenize', env);
    try {
        var env = {
            code: code,
            grammar: Prism.languages[language],
            language: language
        };
        //_.hooks.run('before-tokenize', env);

        if (!env.grammar) {
            throw new Error('The language "' + env.language + '" has no grammar.');
        }

        env.tokens = Prism.tokenize(env.code, env.grammar);
        //_.hooks.run('after-tokenize', env);
        var res = Prism.Token.stringify(Prism.util.encode(env.tokens), env.language);

    }
    catch
    {
        console.log('PrismJS highlight failed');
        return;
    }


    var output = code + '\r\n----------------------------------------------------\r\n' + res + '\r\n\r\n----------------------------------------------------';

    fs.writeFileSync(targetFilename, output, function (err) {
        if (err)
            return console.log(err);
    });
}

function createTests(list, sourceDir, targetDir, options) {
    
    console.log('Building tests for selected languages ' + (options.html ? "HTML " : "Tokens ") + (options.hooks ? "with hooks " : "basic ") + "...");

    for (var lang of list) {

        common.reloadPrism();
        if (lang === 'meta') continue;
        if (!fs.existsSync(sourceDir + "/" + lang)) continue;

        console.log(lang);
        loadLanguages([lang]);
        var dir = targetDir + lang;
        if (!fs.existsSync(dir)) {
            fs.mkdirSync(dir);
        }
        fs.readdirSync(sourceDir + lang).forEach(file => {

            if (!options.html) {
                recompileTest(sourceDir + lang + '/' + file, targetDir + lang + '/' + file, lang, options);
            }
            else {
                recompileHtmlTest(sourceDir + lang + '/' + file, targetDir + lang + '/' + file.replace(".test", ".htmlTest"), lang, options);
            }
        });
    }
    console.log("DONE");
}

function createAllTests(sourceDir, targetDir, options) {
    var list = common.getLanguageNames();
    createTests(list, sourceDir, targetDir, options);
}

module.exports = { recompileTest, recompileHtmlTest, createBareTests: createTests, createAllTests }