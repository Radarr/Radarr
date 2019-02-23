import PropTypes from 'prop-types';
import React, { Component } from 'react';
import ReactDOM from 'react-dom';
import TetherComponent from 'react-tether';
import { icons, kinds } from 'Helpers/Props';
import Icon from 'Components/Icon';
import FormInputButton from 'Components/Form/FormInputButton';
import Link from 'Components/Link/Link';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import TextInput from 'Components/Form/TextInput';
import ImportArtistSearchResultConnector from './ImportArtistSearchResultConnector';
import ImportArtistName from './ImportArtistName';
import styles from './ImportArtistSelectArtist.css';

const tetherOptions = {
  skipMoveElement: true,
  constraints: [
    {
      to: 'window',
      attachment: 'together',
      pin: true
    }
  ],
  attachment: 'top center',
  targetAttachment: 'bottom center'
};

class ImportArtistSelectArtist extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this._artistLookupTimeout = null;

    this.state = {
      term: props.id,
      isOpen: false
    };
  }

  //
  // Control

  _setButtonRef = (ref) => {
    this._buttonRef = ref;
  }

  _setContentRef = (ref) => {
    this._contentRef = ref;
  }

  _addListener() {
    window.addEventListener('click', this.onWindowClick);
  }

  _removeListener() {
    window.removeEventListener('click', this.onWindowClick);
  }

  //
  // Listeners

  onWindowClick = (event) => {
    const button = ReactDOM.findDOMNode(this._buttonRef);
    const content = ReactDOM.findDOMNode(this._contentRef);

    if (!button) {
      return;
    }

    if (!button.contains(event.target) && content && !content.contains(event.target) && this.state.isOpen) {
      this.setState({ isOpen: false });
      this._removeListener();
    }
  }

  onPress = () => {
    if (this.state.isOpen) {
      this._removeListener();
    } else {
      this._addListener();
    }

    this.setState({ isOpen: !this.state.isOpen });
  }

  onSearchInputChange = ({ value }) => {
    if (this._artistLookupTimeout) {
      clearTimeout(this._artistLookupTimeout);
    }

    this.setState({ term: value }, () => {
      this._artistLookupTimeout = setTimeout(() => {
        this.props.onSearchInputChange(value);
      }, 200);
    });
  }

  onRefreshPress = () => {
    this.props.onSearchInputChange(this.state.term);
  }

  onArtistSelect = (foreignArtistId) => {
    this.setState({ isOpen: false });

    this.props.onArtistSelect(foreignArtistId);
  }

  //
  // Render

  render() {
    const {
      selectedArtist,
      isExistingArtist,
      isFetching,
      isPopulated,
      error,
      items,
      isQueued,
      isLookingUpArtist
    } = this.props;

    const errorMessage = error &&
      error.responseJSON &&
      error.responseJSON.message;

    return (
      <TetherComponent
        classes={{
          element: styles.tether
        }}
        {...tetherOptions}
      >
        <Link
          ref={this._setButtonRef}
          className={styles.button}
          component="div"
          onPress={this.onPress}
        >
          {
            isLookingUpArtist && isQueued && !isPopulated &&
              <LoadingIndicator
                className={styles.loading}
                size={20}
              />
          }

          {
            isPopulated && selectedArtist && isExistingArtist &&
              <Icon
                className={styles.warningIcon}
                name={icons.WARNING}
                kind={kinds.WARNING}
              />
          }

          {
            isPopulated && selectedArtist &&
              <ImportArtistName
                artistName={selectedArtist.artistName}
                disambiguation={selectedArtist.disambiguation}
                // year={selectedArtist.year}
                isExistingArtist={isExistingArtist}
              />
          }

          {
            isPopulated && !selectedArtist &&
              <div className={styles.noMatches}>
                <Icon
                  className={styles.warningIcon}
                  name={icons.WARNING}
                  kind={kinds.WARNING}
                />

                No match found!
              </div>
          }

          {
            !isFetching && !!error &&
              <div>
                <Icon
                  className={styles.warningIcon}
                  title={errorMessage}
                  name={icons.WARNING}
                  kind={kinds.WARNING}
                />

                Search failed, please try again later.
              </div>
          }

          <div className={styles.dropdownArrowContainer}>
            <Icon
              name={icons.CARET_DOWN}
            />
          </div>
        </Link>

        {
          this.state.isOpen &&
            <div
              ref={this._setContentRef}
              className={styles.contentContainer}
            >
              <div className={styles.content}>
                <div className={styles.searchContainer}>
                  <div className={styles.searchIconContainer}>
                    <Icon name={icons.SEARCH} />
                  </div>

                  <TextInput
                    className={styles.searchInput}
                    name={`${name}_textInput`}
                    value={this.state.term}
                    onChange={this.onSearchInputChange}
                  />

                  <FormInputButton
                    kind={kinds.DEFAULT}
                    spinnerIcon={icons.REFRESH}
                    canSpin={true}
                    isSpinning={isFetching}
                    onPress={this.onRefreshPress}
                    title="Refresh"
                  >
                    <Icon name={icons.REFRESH} />
                  </FormInputButton>
                </div>

                <div className={styles.results}>
                  {
                    items.map((item) => {
                      return (
                        <ImportArtistSearchResultConnector
                          key={item.foreignArtistId}
                          foreignArtistId={item.foreignArtistId}
                          artistName={item.artistName}
                          disambiguation={item.disambiguation}
                          // year={item.year}
                          onPress={this.onArtistSelect}
                        />
                      );
                    })
                  }
                </div>
              </div>
            </div>
        }
      </TetherComponent>
    );
  }
}

ImportArtistSelectArtist.propTypes = {
  id: PropTypes.string.isRequired,
  selectedArtist: PropTypes.object,
  isExistingArtist: PropTypes.bool.isRequired,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  isQueued: PropTypes.bool.isRequired,
  isLookingUpArtist: PropTypes.bool.isRequired,
  onSearchInputChange: PropTypes.func.isRequired,
  onArtistSelect: PropTypes.func.isRequired
};

ImportArtistSelectArtist.defaultProps = {
  isFetching: true,
  isPopulated: false,
  items: [],
  isQueued: true
};

export default ImportArtistSelectArtist;
