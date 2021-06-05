import PropTypes from 'prop-types';
import React, { Component } from 'react';
import TextInput from 'Components/Form/TextInput';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import Link from 'Components/Link/Link';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import { icons, kinds } from 'Helpers/Props';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import translate from 'Utilities/String/translate';
import AddNewMovieSearchResultConnector from './AddNewMovieSearchResultConnector';
import styles from './AddNewMovie.css';

class AddNewMovie extends Component {

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
      this.props.onMovieLookupChange(term);
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
      this.props.onMovieLookupChange(term);
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
        this.props.onMovieLookupChange(value);
      } else {
        this.props.onClearMovieLookup();
      }
    });
  }

  onClearMovieLookupPress = () => {
    this.setState({ term: '' });
    this.props.onClearMovieLookup();
  }

  //
  // Render

  render() {
    const {
      error,
      items,
      hasExistingMovies,
      colorImpairedMode
    } = this.props;

    const term = this.state.term;
    const isFetching = this.state.isFetching;

    return (
      <PageContent title={translate('AddNewMovie')}>
        <PageContentBody>
          <div className={styles.searchContainer}>
            <div className={styles.searchIconContainer}>
              <Icon
                name={icons.SEARCH}
                size={20}
              />
            </div>

            <TextInput
              className={styles.searchInput}
              name="movieLookup"
              value={term}
              placeholder="eg. The Dark Knight, tmdb:155, imdb:tt0468569"
              autoFocus={true}
              onChange={this.onSearchInputChange}
            />

            <Button
              className={styles.clearLookupButton}
              onPress={this.onClearMovieLookupPress}
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
            !isFetching && !!error ?
              <div className={styles.message}>
                <div className={styles.helpText}>
                  {translate('FailedLoadingSearchResults')}
                </div>
                <div>{getErrorMessage(error)}</div>
              </div> : null
          }

          {
            !isFetching && !error && !!items.length &&
              <div className={styles.searchResults}>
                {
                  items.map((item) => {
                    return (
                      <AddNewMovieSearchResultConnector
                        key={item.tmdbId}
                        colorImpairedMode={colorImpairedMode}
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
                <div className={styles.noResults}>
                  {translate('CouldNotFindResults', [term])}
                </div>
                <div>
                  {translate('YouCanAlsoSearch')}
                </div>
                <div>
                  <Link to="https://wiki.servarr.com/radarr/faq#why-cant-i-add-a-new-movie-to-radarr">
                    {translate('CantFindMovie')}
                  </Link>
                </div>
              </div>
          }

          {
            term ?
              null :
              <div className={styles.message}>
                <div className={styles.helpText}>
                  {translate('AddNewMessage')}
                </div>
                <div>
                  {translate('AddNewTmdbIdMessage')}
                </div>
              </div>
          }

          {
            !term && !hasExistingMovies ?
              <div className={styles.message}>
                <div className={styles.noMoviesText}>
                  {translate('HaveNotAddedMovies')}
                </div>
                <div>
                  <Button
                    to="/add/import"
                    kind={kinds.PRIMARY}
                  >
                    {translate('ImportExistingMovies')}
                  </Button>
                </div>
              </div> :
              null
          }

          <div />
        </PageContentBody>
      </PageContent>
    );
  }
}

AddNewMovie.propTypes = {
  term: PropTypes.string,
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  isAdding: PropTypes.bool.isRequired,
  addError: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  hasExistingMovies: PropTypes.bool.isRequired,
  onMovieLookupChange: PropTypes.func.isRequired,
  onClearMovieLookup: PropTypes.func.isRequired,
  colorImpairedMode: PropTypes.bool.isRequired
};

export default AddNewMovie;
