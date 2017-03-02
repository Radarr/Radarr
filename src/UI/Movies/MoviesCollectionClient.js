var movieCollection = require('./MoviesCollection');

var ClientCollection = movieCollection.clone();
ClientCollection.bindSignalR();
ClientCollection.switchMode('client'); //state.pageSize = 100000;
module.exports = ClientCollection;
