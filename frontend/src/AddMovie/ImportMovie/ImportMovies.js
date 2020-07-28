import React, { Component } from 'react';
import { Route } from 'react-router-dom';
import ImportMovieConnector from 'AddMovie/ImportMovie/Import/ImportMovieConnector';
import ImportMovieSelectFolderConnector from 'AddMovie/ImportMovie/SelectFolder/ImportMovieSelectFolderConnector';
import Switch from 'Components/Router/Switch';

class ImportMovies extends Component {

  //
  // Render

  render() {
    return (
      <Switch>
        <Route
          exact={true}
          path="/add/import"
          component={ImportMovieSelectFolderConnector}
        />

        <Route
          path="/add/import/:rootFolderId"
          component={ImportMovieConnector}
        />
      </Switch>
    );
  }
}

export default ImportMovies;
