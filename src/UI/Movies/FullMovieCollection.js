var movieCollection = require('./MoviesCollection');

var fullCollection = movieCollection.clone();
fullCollection.reset();
fullCollection.bindSignalR();
fullCollection.state.pageSize = -1;
fullCollection.state.page = 0;
//fullCollection.mode = "client";
fullCollection.parseRecords = function(resp) {
    return resp;
};

fullCollection.fetch({reset : true});
module.exports = fullCollection;

/*var movieCollection = require('./MoviesCollectionClient');

movieCollection.bindSignalR();
module.exports = movieCollection.fullCollection;*/
