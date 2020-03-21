import PropTypes from 'prop-types';
import React, { Component } from 'react';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import { align, icons } from 'Helpers/Props';
import PageContent from 'Components/Page/PageContent';
import Measure from 'Components/Measure';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import FilterMenu from 'Components/Menu/FilterMenu';
import NoMovie from 'Movie/NoMovie';
import CalendarLinkModal from './iCal/CalendarLinkModal';
import CalendarOptionsModal from './Options/CalendarOptionsModal';
import LegendConnector from './Legend/LegendConnector';
import CalendarConnector from './CalendarConnector';
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
  }

  onGetCalendarLinkPress = () => {
    this.setState({ isCalendarLinkModalOpen: true });
  }

  onGetCalendarLinkModalClose = () => {
    this.setState({ isCalendarLinkModalOpen: false });
  }

  onOptionsPress = () => {
    this.setState({ isOptionsModalOpen: true });
  }

  onOptionsModalClose = () => {
    this.setState({ isOptionsModalOpen: false });
  }

  onSearchMissingPress = () => {
    const {
      missingMovieIds,
      onSearchMissingPress
    } = this.props;

    onSearchMissingPress(missingMovieIds);
  }

  //
  // Render

  render() {
    const {
      selectedFilterKey,
      filters,
      hasMovie,
      movieError,
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
    const PageComponent = hasMovie ? CalendarConnector : NoMovie;

    return (
      <PageContent title="Calendar">
        <PageToolbar>
          <PageToolbarSection>
            <PageToolbarButton
              label="iCal Link"
              iconName={icons.CALENDAR}
              onPress={this.onGetCalendarLinkPress}
            />

            <PageToolbarSeparator />

            <PageToolbarButton
              label="RSS Sync"
              iconName={icons.RSS}
              isSpinning={isRssSyncExecuting}
              onPress={onRssSyncPress}
            />

            <PageToolbarButton
              label="Search for Missing"
              iconName={icons.SEARCH}
              isDisabled={!missingMovieIds.length}
              isSpinning={isSearchingForMissing}
              onPress={this.onSearchMissingPress}
            />
          </PageToolbarSection>

          <PageToolbarSection alignContent={align.RIGHT}>
            <PageToolbarButton
              label="Options"
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

        <PageContentBodyConnector
          className={styles.calendarPageBody}
          innerClassName={styles.calendarInnerPageBody}
        >
          {
            movieError &&
              <div className={styles.errorMessage}>
                {getErrorMessage(movieError, 'Failed to load movie from API')}
              </div>
          }

          {
            !movieError &&
              <Measure
                whitelist={['width']}
                onMeasure={this.onMeasure}
              >
                {
                  isMeasured ?
                    <PageComponent
                      useCurrentPage={useCurrentPage}
                    /> :
                    <div />
                }
              </Measure>
          }

          {
            hasMovie && !movieError &&
              <LegendConnector />
          }
        </PageContentBodyConnector>

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
