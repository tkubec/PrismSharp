const common = require('./common');
const exporter = require('./export');
const testBuilder = require('./testBuilder');


//exporter.exportAllLanguages();

exporter.exportLanguageNames();

//createAllTestSuits();

//return;


function createAllTestSuits()
{
    testBuilder.createAllTests('../prism-master/tests/languages/', '../Test/data/tests/core/token/basic/',  {html: false });
    testBuilder.createAllTests('../Test/data/tests/examples/exampletestbase/', '../Test/data/tests/examples/token/basic/',  {html: false });

    testBuilder.createAllTests('../prism-master/tests/languages/', '../Test/data/tests/core/html/basic/',  {html: true });
    testBuilder.createAllTests('../Test/data/tests/examples/exampletestbase/', '../Test/data/tests/examples/html/basic/',   {html: true });

}


