var movieCollection = require('./MoviesCollection');

var GCCollection = movieCollection.clone();
GCCollection.bindSignalR();
GCCollection.switchMode('client'); //state.pageSize = 100000;
//CCollection.fetch();
module.exports = GCCollection;
