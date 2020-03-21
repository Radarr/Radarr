import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons, inputTypes, kinds, sizes } from 'Helpers/Props';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import ClipboardButton from 'Components/Link/ClipboardButton';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormLabel from 'Components/Form/FormLabel';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormInputButton from 'Components/Form/FormInputButton';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';

function getUrls(state) {
  const {
    unmonitored,
    asAllDay,
    tags
  } = state;

  let icalUrl = `${window.location.host}${window.Radarr.urlBase}/feed/calendar/Radarr.ics?`;

  if (unmonitored) {
    icalUrl += 'unmonitored=true&';
  }

  if (asAllDay) {
    icalUrl += 'asAllDay=true&';
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

class CalendarLinkModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    const defaultState = {
      unmonitored: false,
      asAllDay: false,
      tags: []
    };

    const urls = getUrls(defaultState);

    this.state = {
      ...defaultState,
      ...urls
    };
  }

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    const state = {
      ...this.state,
      [name]: value
    };

    const urls = getUrls(state);

    this.setState({
      [name]: value,
      ...urls
    });
  }

  onLinkFocus = (event) => {
    event.target.select();
  }

  //
  // Render

  render() {
    const {
      onModalClose
    } = this.props;

    const {
      unmonitored,
      asAllDay,
      tags,
      iCalHttpUrl,
      iCalWebCalUrl
    } = this.state;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          Radarr Calendar Feed
        </ModalHeader>

        <ModalBody>
          <Form>
            <FormGroup>
              <FormLabel>Include Unmonitored</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="unmonitored"
                value={unmonitored}
                helpText="Include unmonitored movies in the iCal feed"
                onChange={this.onInputChange}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>Show as All-Day Events</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="asAllDay"
                value={asAllDay}
                helpText="Events will appear as all-day events in your calendar"
                onChange={this.onInputChange}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>Tags</FormLabel>

              <FormInputGroup
                type={inputTypes.TAG}
                name="tags"
                value={tags}
                helpText="Feed will only contain movies with at least one matching tag"
                onChange={this.onInputChange}
              />
            </FormGroup>

            <FormGroup
              size={sizes.LARGE}
            >
              <FormLabel>iCal Feed</FormLabel>

              <FormInputGroup
                type={inputTypes.TEXT}
                name="iCalHttpUrl"
                value={iCalHttpUrl}
                readOnly={true}
                helpText="Copy this URL to your client(s) or click to subscribe if your browser supports webcal"
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
            Close
          </Button>
        </ModalFooter>
      </ModalContent>
    );
  }
}

CalendarLinkModalContent.propTypes = {
  tagList: PropTypes.arrayOf(PropTypes.object).isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default CalendarLinkModalContent;
