import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons } from 'Helpers/Props';
import Button from 'Components/Link/Button';
import Link from 'Components/Link/Link';
import Icon from 'Components/Icon';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import TextInput from 'Components/Form/TextInput';
import PageContent from 'Components/Page/PageContent';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import AddNewArtistSearchResultConnector from './AddNewArtistSearchResultConnector';
import styles from './AddNewArtist.css';

class AddNewArtist extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      term: props.term || '',
      isFetching: false
    };
  }

  componentDidMount() {
    const term = this.state.term;

    if (term) {
      this.props.onArtistLookupChange(term);
    }
  }

  componentDidUpdate(prevProps) {
    const {
      term,
      isFetching
    } = this.props;

    if (term && term !== prevProps.term) {
      this.setState({
        term,
        isFetching: true
      });
      this.props.onArtistLookupChange(term);
    } else if (isFetching !== prevProps.isFetching) {
      this.setState({
        isFetching
      });
    }
  }

  //
  // Listeners

  onSearchInputChange = ({ value }) => {
    const hasValue = !!value.trim();

    this.setState({ term: value, isFetching: hasValue }, () => {
      if (hasValue) {
        this.props.onArtistLookupChange(value);
      } else {
        this.props.onClearArtistLookup();
      }
    });
  }

  onClearArtistLookupPress = () => {
    this.setState({ term: '' });
    this.props.onClearArtistLookup();
  }

  //
  // Render

  render() {
    const {
      error,
      items
    } = this.props;

    const term = this.state.term;
    const isFetching = this.state.isFetching;

    return (
      <PageContent title="Add New Artist">
        <PageContentBodyConnector>
          <div className={styles.searchContainer}>
            <div className={styles.searchIconContainer}>
              <Icon
                name={icons.SEARCH}
                size={20}
              />
            </div>

            <TextInput
              className={styles.searchInput}
              name="artistLookup"
              value={term}
              placeholder="eg. Breaking Benjamin, lidarr:854a1807-025b-42a8-ba8c-2a39717f1d25"
              autoFocus={true}
              onChange={this.onSearchInputChange}
            />

            <Button
              className={styles.clearLookupButton}
              onPress={this.onClearArtistLookupPress}
            >
              <Icon
                name={icons.REMOVE}
                size={20}
              />
            </Button>
          </div>

          {
            isFetching &&
              <LoadingIndicator />
          }

          {
            !isFetching && !!error &&
              <div>Failed to load search results, please try again.</div>
          }

          {
            !isFetching && !error && !!items.length &&
              <div className={styles.searchResults}>
                {
                  items.map((item) => {
                    return (
                      <AddNewArtistSearchResultConnector
                        key={item.foreignArtistId}
                        {...item}
                      />
                    );
                  })
                }
              </div>
          }

          {
            !isFetching && !error && !items.length && !!term &&
              <div className={styles.message}>
                <div className={styles.noResults}>Couldn't find any results for '{term}'</div>
                <div>You can also search using MusicBrainz ID of an artist. eg. lidarr:cc197bad-dc9c-440d-a5b5-d52ba2e14234</div>
                <div>
                  <Link to="https://github.com/Lidarr/Lidarr/wiki/FAQ#why-cant-i-add-a-new-artist-when-i-know-the-musicbrainz-id">
                    Why can't I find my artist?
                  </Link>
                </div>
              </div>
          }

          {
            !term &&
              <div className={styles.message}>
                <div className={styles.helpText}>It's easy to add a new artist, just start typing the name the artist you want to add.</div>
                <div>You can also search using MusicBrainz ID of an artist. eg. lidarr:cc197bad-dc9c-440d-a5b5-d52ba2e14234</div>
              </div>
          }

          <div />
        </PageContentBodyConnector>
      </PageContent>
    );
  }
}

AddNewArtist.propTypes = {
  term: PropTypes.string,
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  isAdding: PropTypes.bool.isRequired,
  addError: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  onArtistLookupChange: PropTypes.func.isRequired,
  onClearArtistLookup: PropTypes.func.isRequired
};

export default AddNewArtist;
