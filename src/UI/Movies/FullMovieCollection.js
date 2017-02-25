var movieCollection = require('./MoviesCollection');

var fullCollection = movieCollection.clone();
fullCollection.bindSignalR();
fullCollection.state.pageSize = 10000;
fullCollection.fetch({reset : true});
module.exports = fullCollection;
