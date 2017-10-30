import React, { Component } from 'react';
import { Route } from 'react-router-dom';
import Switch from 'Components/Router/Switch';
import ImportArtistSelectFolderConnector from 'AddArtist/ImportArtist/SelectFolder/ImportArtistSelectFolderConnector';
import ImportArtistConnector from 'AddArtist/ImportArtist/Import/ImportArtistConnector';

class ImportArtist extends Component {

  //
  // Render

  render() {
    return (
      <Switch>
        <Route
          exact={true}
          path="/add/import"
          component={ImportArtistSelectFolderConnector}
        />

        <Route
          path="/add/import/:rootFolderId"
          component={ImportArtistConnector}
        />
      </Switch>
    );
  }
}

export default ImportArtist;
