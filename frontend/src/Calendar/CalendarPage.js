import PropTypes from 'prop-types';
import React, { Component } from 'react';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import Measure from 'Components/Measure';
import FilterMenu from 'Components/Menu/FilterMenu';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import { align, icons } from 'Helpers/Props';
import NoMovie from 'Movie/NoMovie';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import translate from 'Utilities/String/translate';
import CalendarConnector from './CalendarConnector';
import CalendarLinkModal from './iCal/CalendarLinkModal';
import LegendConnector from './Legend/LegendConnector';
import CalendarOptionsModal from './Options/CalendarOptionsModal';
import styles from './CalendarPage.css';

const MINIMUM_DAY_WIDTH = 120;

class CalendarPage extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isCalendarLinkModalOpen: false,
      isOptionsModalOpen: false,
      width: 0
    };
  }

  //
  // Listeners

  onMeasure = ({ width }) => {
    this.setState({ width });
    const days = Math.max(3, Math.min(7, Math.floor(width / MINIMUM_DAY_WIDTH)));

    this.props.onDaysCountChange(days);
  };

  onGetCalendarLinkPress = () => {
    this.setState({ isCalendarLinkModalOpen: true });
  };

  onGetCalendarLinkModalClose = () => {
    this.setState({ isCalendarLinkModalOpen: false });
  };

  onOptionsPress = () => {
    this.setState({ isOptionsModalOpen: true });
  };

  onOptionsModalClose = () => {
    this.setState({ isOptionsModalOpen: false });
  };

  onSearchMissingPress = () => {
    const {
      missingMovieIds,
      onSearchMissingPress
    } = this.props;

    onSearchMissingPress(missingMovieIds);
  };

  //
  // Render

  render() {
    const {
      selectedFilterKey,
      filters,
      hasMovie,
      movieError,
      movieIsFetching,
      movieIsPopulated,
      missingMovieIds,
      isRssSyncExecuting,
      isSearchingForMissing,
      useCurrentPage,
      onRssSyncPress,
      onFilterSelect
    } = this.props;

    const {
      isCalendarLinkModalOpen,
      isOptionsModalOpen
    } = this.state;

    const isMeasured = this.state.width > 0;

    return (
      <PageContent title={translate('Calendar')}>
        <PageToolbar>
          <PageToolbarSection>
            <PageToolbarButton
              label={translate('iCalLink')}
              iconName={icons.CALENDAR}
              onPress={this.onGetCalendarLinkPress}
            />

            <PageToolbarSeparator />

            <PageToolbarButton
              label={translate('RSSSync')}
              iconName={icons.RSS}
              isSpinning={isRssSyncExecuting}
              onPress={onRssSyncPress}
            />

            <PageToolbarButton
              label={translate('SearchForMissing')}
              iconName={icons.SEARCH}
              isDisabled={!missingMovieIds.length}
              isSpinning={isSearchingForMissing}
              onPress={this.onSearchMissingPress}
            />
          </PageToolbarSection>

          <PageToolbarSection alignContent={align.RIGHT}>
            <PageToolbarButton
              label={translate('Options')}
              iconName={icons.POSTER}
              onPress={this.onOptionsPress}
            />

            <FilterMenu
              alignMenu={align.RIGHT}
              isDisabled={!hasMovie}
              selectedFilterKey={selectedFilterKey}
              filters={filters}
              customFilters={[]}
              onFilterSelect={onFilterSelect}
            />
          </PageToolbarSection>
        </PageToolbar>

        <PageContentBody
          className={styles.calendarPageBody}
          innerClassName={styles.calendarInnerPageBody}
        >
          {
            movieIsFetching && !movieIsPopulated &&
              <LoadingIndicator />
          }

          {
            movieError &&
              <div className={styles.errorMessage}>
                {getErrorMessage(movieError, 'Failed to load movies from API')}
              </div>
          }

          {
            !movieError && movieIsPopulated && hasMovie &&
              <Measure
                whitelist={['width']}
                onMeasure={this.onMeasure}
              >
                {
                  isMeasured ?
                    <CalendarConnector
                      useCurrentPage={useCurrentPage}
                    /> :
                    <div />
                }
              </Measure>
          }

          {
            !movieError && movieIsPopulated && !hasMovie &&
              <NoMovie />
          }

          {
            hasMovie && !movieError &&
              <LegendConnector />
          }
        </PageContentBody>

        <CalendarLinkModal
          isOpen={isCalendarLinkModalOpen}
          onModalClose={this.onGetCalendarLinkModalClose}
        />

        <CalendarOptionsModal
          isOpen={isOptionsModalOpen}
          onModalClose={this.onOptionsModalClose}
        />
      </PageContent>
    );
  }
}

CalendarPage.propTypes = {
  selectedFilterKey: PropTypes.string.isRequired,
  filters: PropTypes.arrayOf(PropTypes.object).isRequired,
  hasMovie: PropTypes.bool.isRequired,
  movieError: PropTypes.object,
  movieIsFetching: PropTypes.bool.isRequired,
  movieIsPopulated: PropTypes.bool.isRequired,
  missingMovieIds: PropTypes.arrayOf(PropTypes.number).isRequired,
  isRssSyncExecuting: PropTypes.bool.isRequired,
  isSearchingForMissing: PropTypes.bool.isRequired,
  useCurrentPage: PropTypes.bool.isRequired,
  onSearchMissingPress: PropTypes.func.isRequired,
  onDaysCountChange: PropTypes.func.isRequired,
  onRssSyncPress: PropTypes.func.isRequired,
  onFilterSelect: PropTypes.func.isRequired
};

export default CalendarPage;
