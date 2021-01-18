import PropTypes from 'prop-types';
import React, { Component } from 'react';
import TextInput from 'Components/Form/TextInput';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import Link from 'Components/Link/Link';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import { icons } from 'Helpers/Props';
import AddNewAuthorSearchResultConnector from './Author/AddNewAuthorSearchResultConnector';
import AddNewBookSearchResultConnector from './Book/AddNewBookSearchResultConnector';
import styles from './AddNewItem.css';

class AddNewItem extends Component {

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
      this.props.onSearchChange(term);
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
      this.props.onSearchChange(term);
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
        this.props.onSearchChange(value);
      } else {
        this.props.onClearSearch();
      }
    });
  }

  onClearSearchPress = () => {
    this.setState({ term: '' });
    this.props.onClearSearch();
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
      <PageContent title="Add New Item">
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
              name="searchBox"
              value={term}
              placeholder="eg. War and Peace, goodreads:656, isbn:067003469X, asin:B00JCDK5ME"
              autoFocus={true}
              onChange={this.onSearchInputChange}
            />

            <Button
              className={styles.clearLookupButton}
              onPress={this.onClearSearchPress}
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
                    if (item.author) {
                      const author = item.author;
                      return (
                        <AddNewAuthorSearchResultConnector
                          key={item.id}
                          {...author}
                        />
                      );
                    } else if (item.book) {
                      const book = item.book;
                      const edition = book.editions.find((x) => x.monitored);
                      return (
                        <AddNewBookSearchResultConnector
                          key={item.id}
                          isExistingBook={'id' in edition && edition.id !== 0}
                          isExistingAuthor={'id' in book.author && book.author.id !== 0}
                          {...book}
                        />
                      );
                    }
                    return null;
                  })
                }
              </div>
          }

          {
            !isFetching && !error && !items.length && !!term &&
              <div className={styles.message}>
                <div className={styles.noResults}>Couldn't find any results for '{term}'</div>
                <div>
                  You can also search using the
                  <Link to="https://goodreads.com"> Goodreads ID </Link>
                  of a book (e.g. goodreads:656), the isbn (e.g. isbn:067003469X) or the asin (e.g. asin:B00JCDK5ME)
                </div>
              </div>
          }

          {
            !term &&
              <div className={styles.message}>
                <div className={styles.helpText}>It's easy to add a new author or book, just start typing the name of the item you want to add.</div>
                <div>
                  You can also search using the
                  <Link to="https://goodreads.com"> Goodreads ID </Link>
                  of a book (e.g. goodreads:656), the isbn (e.g. isbn:067003469X) or the asin (e.g. asin:B00JCDK5ME)
                </div>
              </div>
          }

          <div />
        </PageContentBodyConnector>
      </PageContent>
    );
  }
}

AddNewItem.propTypes = {
  term: PropTypes.string,
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  isAdding: PropTypes.bool.isRequired,
  addError: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  onSearchChange: PropTypes.func.isRequired,
  onClearSearch: PropTypes.func.isRequired
};

export default AddNewItem;
