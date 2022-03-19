import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { without } from 'underscore';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputButton from 'Components/Form/FormInputButton';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import ClipboardButton from 'Components/Link/ClipboardButton';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { icons, inputTypes, kinds, sizes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';

class CalendarLinkModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    const defaultState = {
      unmonitored: false,
      asAllDay: false,
      tags: [],
      selectedReleaseTypes: ['all']
    };

    this.state = {
      ...defaultState
    };
  }

  getUrls = () => {
    const {
      unmonitored,
      asAllDay,
      selectedReleaseTypes,
      tags
    } = this.state;

    let icalUrl = `${window.location.host}${window.Radarr.urlBase}/feed/v3/calendar/Radarr.ics?`;

    if (unmonitored) {
      icalUrl += 'unmonitored=true&';
    }

    if (asAllDay) {
      icalUrl += 'asAllDay=true&';
    }

    if (selectedReleaseTypes?.length !== 0 && selectedReleaseTypes[0] !== 'all') {
      icalUrl += `releaseType[]=${selectedReleaseTypes.join()}&`;
    }

    if (tags.length) {
      icalUrl += `tags=${tags.toString()}&`;
    }

    icalUrl += `apikey=${window.Radarr.apiKey}`;

    const iCalHttpUrl = `${window.location.protocol}//${icalUrl}`;
    const iCalWebCalUrl = `webcal://${icalUrl}`;

    return {
      iCalHttpUrl,
      iCalWebCalUrl
    };
  }

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.setState({
      [name]: value
    });
  };

  onLinkFocus = (event) => {
    event.target.select();
  };

  onReleaseTypeInputChange = ({ name, value }) => {
    const { releaseTypes } = this.props;
    const { selectedReleaseTypes } = this.state;

    let newSelectedReleaseTypes = value;
    if (value.length === 0) {
      newSelectedReleaseTypes = ['all'];
    } else if (value.length > selectedReleaseTypes.length) {
      const selectedReleaseType = releaseTypes.find((releaseType) => releaseType.key === without(value, ...selectedReleaseTypes)[0]);
      const unselectedReleaseTypes = selectedReleaseType?.unselectValues || [];
      newSelectedReleaseTypes = without([selectedReleaseType.key, ...selectedReleaseTypes], ...unselectedReleaseTypes);
    }

    this.setState({
      selectedReleaseTypes: newSelectedReleaseTypes
    });
  }

  //
  // Render

  render() {
    const {
      onModalClose,
      releaseTypes
    } = this.props;

    const {
      unmonitored,
      asAllDay,
      tags,
      selectedReleaseTypes
    } = this.state;

    const { iCalHttpUrl, iCalWebCalUrl } = this.getUrls();

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          {translate('RadarrCalendarFeed')}
        </ModalHeader>

        <ModalBody>
          <Form>
            <FormGroup>
              <FormLabel>{translate('IncludeUnmonitored')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="unmonitored"
                value={unmonitored}
                helpText={translate('UnmonitoredHelpText')}
                onChange={this.onInputChange}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('ShowAsAllDayEvents')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="asAllDay"
                value={asAllDay}
                helpText={translate('AsAllDayHelpText')}
                onChange={this.onInputChange}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('Tags')}</FormLabel>

              <FormInputGroup
                type={inputTypes.TAG}
                name="tags"
                value={tags}
                helpText={translate('TagsHelpText')}
                onChange={this.onInputChange}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('ReleaseType')}</FormLabel>

              <FormInputGroup
                type={inputTypes.SELECT}
                name="releaseType"
                values={releaseTypes}
                value={selectedReleaseTypes}
                onChange={this.onReleaseTypeInputChange}
              />
            </FormGroup>

            <FormGroup
              size={sizes.LARGE}
            >
              <FormLabel>{translate('ICalFeed')}</FormLabel>

              <FormInputGroup
                type={inputTypes.TEXT}
                name="iCalHttpUrl"
                value={iCalHttpUrl}
                readOnly={true}
                helpText={translate('ICalHttpUrlHelpText')}
                buttons={[
                  <ClipboardButton
                    key="copy"
                    value={iCalHttpUrl}
                    kind={kinds.DEFAULT}
                  />,

                  <FormInputButton
                    key="webcal"
                    kind={kinds.DEFAULT}
                    to={iCalWebCalUrl}
                    target="_blank"
                    noRouter={true}
                  >
                    <Icon name={icons.CALENDAR_O} />
                  </FormInputButton>
                ]}
                onChange={this.onInputChange}
                onFocus={this.onLinkFocus}
              />
            </FormGroup>
          </Form>
        </ModalBody>

        <ModalFooter>
          <Button onPress={onModalClose}>
            {translate('Close')}
          </Button>
        </ModalFooter>
      </ModalContent>
    );
  }
}

CalendarLinkModalContent.propTypes = {
  tagList: PropTypes.arrayOf(PropTypes.object).isRequired,
  onModalClose: PropTypes.func.isRequired,
  releaseTypes: PropTypes.arrayOf(PropTypes.object).isRequired
};

export default CalendarLinkModalContent;
