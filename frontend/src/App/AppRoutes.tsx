import React from 'react';
import { Redirect, Route } from 'react-router-dom';
import Blocklist from 'Activity/Blocklist/Blocklist';
import History from 'Activity/History/History';
import Queue from 'Activity/Queue/Queue';
import AddNewAudiobook from 'AddAudiobook/AddNewAudiobook/AddNewAudiobook';
import AddNewBook from 'AddBook/AddNewBook/AddNewBook';
import AddNewMovieConnector from 'AddMovie/AddNewMovie/AddNewMovieConnector';
import ImportMovies from 'AddMovie/ImportMovie/ImportMovies';
import AudiobookDetailsPage from 'Audiobook/Details/AudiobookDetailsPage';
import AudiobookIndex from 'Audiobook/Index/AudiobookIndex';
import AuthorDetailsPage from 'Author/Details/AuthorDetailsPage';
import AuthorIndex from 'Author/Index/AuthorIndex';
import BookDetailsPage from 'Book/Details/BookDetailsPage';
import BookIndex from 'Book/Index/BookIndex';
import CalendarPage from 'Calendar/CalendarPage';
import CollectionConnector from 'Collection/CollectionConnector';
import NotFound from 'Components/NotFound';
import Switch from 'Components/Router/Switch';
import DiscoverMovieConnector from 'DiscoverMovie/DiscoverMovieConnector';
import MovieDetailsPage from 'Movie/Details/MovieDetailsPage';
import MovieIndex from 'Movie/Index/MovieIndex';
import SeriesDetailsPage from 'Series/Details/SeriesDetailsPage';
import SeriesIndex from 'Series/Index/SeriesIndex';
import CustomFormatSettingsPage from 'Settings/CustomFormats/CustomFormatSettingsPage';
import DownloadClientSettingsConnector from 'Settings/DownloadClients/DownloadClientSettingsConnector';
import GeneralSettingsConnector from 'Settings/General/GeneralSettingsConnector';
import ImportListSettings from 'Settings/ImportLists/ImportListSettings';
import IndexerSettings from 'Settings/Indexers/IndexerSettings';
import MediaManagement from 'Settings/MediaManagement/MediaManagement';
import MetadataSettings from 'Settings/Metadata/MetadataSettings';
import NotificationSettings from 'Settings/Notifications/NotificationSettings';
import Profiles from 'Settings/Profiles/Profiles';
import QualityConnector from 'Settings/Quality/QualityConnector';
import Settings from 'Settings/Settings';
import TagSettings from 'Settings/Tags/TagSettings';
import UISettingsConnector from 'Settings/UI/UISettingsConnector';
import BackupsConnector from 'System/Backup/BackupsConnector';
import LogsTableConnector from 'System/Events/LogsTableConnector';
import Logs from 'System/Logs/Logs';
import Status from 'System/Status/Status';
import Tasks from 'System/Tasks/Tasks';
import Updates from 'System/Updates/Updates';
import getPathWithUrlBase from 'Utilities/getPathWithUrlBase';
import CutoffUnmet from 'Wanted/CutoffUnmet/CutoffUnmet';
import Missing from 'Wanted/Missing/Missing';

function RedirectWithUrlBase() {
  return <Redirect to={getPathWithUrlBase('/')} />;
}

function AppRoutes() {
  return (
    <Switch>
      {/*
        Movies
      */}

      <Route exact={true} path="/" component={MovieIndex} />

      {window.Radarr.urlBase && (
        <Route
          exact={true}
          path="/"
          // eslint-disable-next-line @typescript-eslint/ban-ts-comment
          // @ts-ignore
          addUrlBase={false}
          render={RedirectWithUrlBase}
        />
      )}

      <Route path="/add/new" component={AddNewMovieConnector} />

      <Route path="/collections" component={CollectionConnector} />

      <Route path="/add/import" component={ImportMovies} />

      <Route path="/add/discover" component={DiscoverMovieConnector} />

      <Route path="/movie/:titleSlug" component={MovieDetailsPage} />

      {/*
        Books
      */}

      <Route exact={true} path="/books" component={BookIndex} />

      <Route path="/books/add/new" component={AddNewBook} />

      <Route path="/book/:id" component={BookDetailsPage} />

      {/*
        Audiobooks
      */}

      <Route exact={true} path="/audiobooks" component={AudiobookIndex} />

      <Route path="/audiobooks/add/new" component={AddNewAudiobook} />

      <Route path="/audiobook/:id" component={AudiobookDetailsPage} />

      {/*
        Authors
      */}

      <Route exact={true} path="/authors" component={AuthorIndex} />

      <Route path="/author/:id" component={AuthorDetailsPage} />

      {/*
        Series
      */}

      <Route exact={true} path="/series" component={SeriesIndex} />

      <Route path="/series/:id" component={SeriesDetailsPage} />

      {/*
        Calendar
      */}

      <Route path="/calendar" component={CalendarPage} />

      {/*
        Activity
      */}

      <Route path="/activity/history" component={History} />

      <Route path="/activity/queue" component={Queue} />

      <Route path="/activity/blocklist" component={Blocklist} />

      {/*
        Wanted
      */}

      <Route path="/wanted/missing" component={Missing} />

      <Route path="/wanted/cutoffunmet" component={CutoffUnmet} />

      {/*
        Settings
      */}

      <Route exact={true} path="/settings" component={Settings} />

      <Route path="/settings/mediamanagement" component={MediaManagement} />

      <Route path="/settings/profiles" component={Profiles} />

      <Route path="/settings/quality" component={QualityConnector} />

      <Route
        path="/settings/customformats"
        component={CustomFormatSettingsPage}
      />

      <Route path="/settings/indexers" component={IndexerSettings} />

      <Route
        path="/settings/downloadclients"
        component={DownloadClientSettingsConnector}
      />

      <Route path="/settings/importlists" component={ImportListSettings} />

      <Route path="/settings/connect" component={NotificationSettings} />

      <Route path="/settings/metadata" component={MetadataSettings} />

      <Route path="/settings/tags" component={TagSettings} />

      <Route path="/settings/general" component={GeneralSettingsConnector} />

      <Route path="/settings/ui" component={UISettingsConnector} />

      {/*
        System
      */}

      <Route path="/system/status" component={Status} />

      <Route path="/system/tasks" component={Tasks} />

      <Route path="/system/backup" component={BackupsConnector} />

      <Route path="/system/updates" component={Updates} />

      <Route path="/system/events" component={LogsTableConnector} />

      <Route path="/system/logs/files" component={Logs} />

      {/*
        Not Found
      */}

      <Route path="*" component={NotFound} />
    </Switch>
  );
}

export default AppRoutes;
