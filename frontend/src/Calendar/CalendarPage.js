import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Measure from 'react-measure';
import { align, icons } from 'Helpers/Props';
import PageContent from 'Components/Page/PageContent';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import FilterMenu from 'Components/Menu/FilterMenu';
import NoArtist from 'Artist/NoArtist';
import CalendarLinkModal from './iCal/CalendarLinkModal';
import Legend from './Legend/Legend';
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

  onFilterSelect = (selectedFilterKey) => {
    this.props.onUnmonitoredChange(selectedFilterKey === 'unmonitored');
  }

  onGetCalendarLinkPress = () => {
    this.setState({ isCalendarLinkModalOpen: true });
  }

  onGetCalendarLinkModalClose = () => {
    this.setState({ isCalendarLinkModalOpen: false });
  }

  //
  // Render

  render() {
    const {
      selectedFilterKey,
      filters,
      hasArtist,
      colorImpairedMode
    } = this.props;

    const isMeasured = this.state.width > 0;

    let PageComponent = 'div';

    if (isMeasured) {
      PageComponent = hasArtist ? CalendarConnector : NoArtist;
    }

    return (
      <PageContent title="Calendar">
        <PageToolbar>
          <PageToolbarSection>
            <PageToolbarButton
              label="iCal Link"
              iconName={icons.CALENDAR}
              onPress={this.onGetCalendarLinkPress}
            />
          </PageToolbarSection>

          <PageToolbarSection alignContent={align.RIGHT}>
            <FilterMenu
              alignMenu={align.RIGHT}
              isDisabled={!hasArtist}
              selectedFilterKey={selectedFilterKey}
              filters={filters}
              customFilters={[]}
              onFilterSelect={this.onFilterSelect}
            />
          </PageToolbarSection>
        </PageToolbar>

        <PageContentBodyConnector
          className={styles.calendarPageBody}
          innerClassName={styles.calendarInnerPageBody}
        >
          <Measure
            whitelist={['width']}
            onMeasure={this.onMeasure}
          >
            <PageComponent />
          </Measure>

          {
            hasArtist &&
            <Legend colorImpairedMode={colorImpairedMode} />
          }
        </PageContentBodyConnector>

        <CalendarLinkModal
          isOpen={this.state.isCalendarLinkModalOpen}
          onModalClose={this.onGetCalendarLinkModalClose}
        />
      </PageContent>
    );
  }
}

CalendarPage.propTypes = {
  selectedFilterKey: PropTypes.string.isRequired,
  filters: PropTypes.arrayOf(PropTypes.object).isRequired,
  hasArtist: PropTypes.bool.isRequired,
  colorImpairedMode: PropTypes.bool.isRequired,
  onDaysCountChange: PropTypes.func.isRequired,
  onUnmonitoredChange: PropTypes.func.isRequired
};

export default CalendarPage;
