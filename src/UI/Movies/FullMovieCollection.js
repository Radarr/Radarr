var movieCollection = require('./MoviesCollection');

var fullCollection = movieCollection.clone();
fullCollection.reset();
fullCollection.bindSignalR();
fullCollection.state.pageSize = 100000;
fullCollection.fetch({reset : true});
module.exports = fullCollection;

/*var movieCollection = require('./MoviesCollectionClient');

movieCollection.bindSignalR();
module.exports = movieCollection.fullCollection;*/
