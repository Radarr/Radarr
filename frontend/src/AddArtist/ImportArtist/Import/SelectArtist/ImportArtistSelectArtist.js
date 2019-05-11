import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { Manager, Popper, Reference } from 'react-popper';
import getUniqueElememtId from 'Utilities/getUniqueElementId';
import { icons, kinds } from 'Helpers/Props';
import Icon from 'Components/Icon';
import Portal from 'Components/Portal';
import FormInputButton from 'Components/Form/FormInputButton';
import Link from 'Components/Link/Link';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import TextInput from 'Components/Form/TextInput';
import ImportArtistSearchResultConnector from './ImportArtistSearchResultConnector';
import ImportArtistName from './ImportArtistName';
import styles from './ImportArtistSelectArtist.css';

class ImportArtistSelectArtist extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this._artistLookupTimeout = null;
    this._scheduleUpdate = null;
    this._buttonId = getUniqueElememtId();
    this._contentId = getUniqueElememtId();

    this.state = {
      term: props.id,
      isOpen: false
    };
  }

  componentDidUpdate() {
    if (this._scheduleUpdate) {
      this._scheduleUpdate();
    }
  }

  //
  // Control

  _addListener() {
    window.addEventListener('click', this.onWindowClick);
  }

  _removeListener() {
    window.removeEventListener('click', this.onWindowClick);
  }

  //
  // Listeners

  onWindowClick = (event) => {
    const button = document.getElementById(this._buttonId);
    const content = document.getElementById(this._contentId);

    if (!button || !content) {
      return;
    }

    if (
      !button.contains(event.target) &&
      !content.contains(event.target) &&
      this.state.isOpen
    ) {
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
      <Manager>
        <Reference>
          {({ ref }) => (
            <div
              ref={ref}
              id={this._buttonId}
            >
              <Link
                ref={ref}
                className={styles.button}
                component="div"
                onPress={this.onPress}
              >
                {
                  isLookingUpArtist && isQueued && !isPopulated ?
                    <LoadingIndicator
                      className={styles.loading}
                      size={20}
                    /> :
                    null
                }

                {
                  isPopulated && selectedArtist && isExistingArtist ?
                    <Icon
                      className={styles.warningIcon}
                      name={icons.WARNING}
                      kind={kinds.WARNING}
                    /> :
                    null
                }

                {
                  isPopulated && selectedArtist ?
                    <ImportArtistName
                      artistName={selectedArtist.artistName}
                      disambiguation={selectedArtist.disambiguation}
                      // year={selectedArtist.year}
                      isExistingArtist={isExistingArtist}
                    /> :
                    null
                }

                {
                  isPopulated && !selectedArtist ?
                    <div className={styles.noMatches}>
                      <Icon
                        className={styles.warningIcon}
                        name={icons.WARNING}
                        kind={kinds.WARNING}
                      />

                      No match found!
                    </div> :
                    null
                }

                {
                  !isFetching && !!error ?
                    <div>
                      <Icon
                        className={styles.warningIcon}
                        title={errorMessage}
                        name={icons.WARNING}
                        kind={kinds.WARNING}
                      />

                        Search failed, please try again later.
                    </div> :
                    null
                }

                <div className={styles.dropdownArrowContainer}>
                  <Icon
                    name={icons.CARET_DOWN}
                  />
                </div>
              </Link>
            </div>
          )}
        </Reference>

        <Portal>
          <Popper
            placement="bottom"
            modifiers={{
              preventOverflow: {
                boundariesElement: 'viewport'
              }
            }}
          >
            {({ ref, style, scheduleUpdate }) => {
              this._scheduleUpdate = scheduleUpdate;

              return (
                <div
                  ref={ref}
                  id={this._contentId}
                  className={styles.contentContainer}
                  style={style}
                >
                  {
                    this.state.isOpen ?
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
                      </div> :
                      null
                  }

                </div>
              );
            }}
          </Popper>
        </Portal>
      </Manager>
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
