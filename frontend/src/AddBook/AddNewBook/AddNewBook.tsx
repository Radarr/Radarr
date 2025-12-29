import React, { useCallback, useEffect, useRef, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import Alert from 'Components/Alert';
import TextInput from 'Components/Form/TextInput';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import { icons, kinds } from 'Helpers/Props';
import { clearAddBook, lookupBook } from 'Store/Actions/addBookActions';
import createAddBookSelector from 'Store/Selectors/createAddBookSelector';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import translate from 'Utilities/String/translate';
import AddNewBookSearchResult from './AddNewBookSearchResult';
import styles from './AddNewBook.css';

function AddNewBook() {
  const dispatch = useDispatch();
  const { isFetching, isPopulated, error, items } = useSelector(
    createAddBookSelector()
  );

  const [term, setTerm] = useState('');
  const lookupTimeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  useEffect(() => {
    return () => {
      if (lookupTimeoutRef.current) {
        clearTimeout(lookupTimeoutRef.current);
      }
      dispatch(clearAddBook());
    };
  }, [dispatch]);

  const onSearchInputChange = useCallback(
    ({ value }: { value: string }) => {
      setTerm(value);

      if (lookupTimeoutRef.current) {
        clearTimeout(lookupTimeoutRef.current);
      }

      if (value.trim()) {
        lookupTimeoutRef.current = setTimeout(() => {
          dispatch(lookupBook({ term: value }));
        }, 300);
      } else {
        dispatch(clearAddBook());
      }
    },
    [dispatch]
  );

  const onClearPress = useCallback(() => {
    setTerm('');
    dispatch(clearAddBook());
  }, [dispatch]);

  return (
    <PageContent title={translate('AddNewBook')}>
      <PageContentBody>
        <div className={styles.searchContainer}>
          <div className={styles.searchIconContainer}>
            <Icon name={icons.SEARCH} size={20} />
          </div>

          <TextInput
            className={styles.searchInput}
            name="bookLookup"
            value={term}
            placeholder="e.g. The Great Gatsby, isbn:978-0743273565"
            autoFocus={true}
            onChange={onSearchInputChange}
          />

          <Button className={styles.clearLookupButton} onPress={onClearPress}>
            <Icon name={icons.REMOVE} size={20} />
          </Button>
        </div>

        {isFetching ? <LoadingIndicator /> : null}

        {!isFetching && !!error ? (
          <div className={styles.message}>
            <div className={styles.helpText}>
              {translate('FailedLoadingSearchResults')}
            </div>
            <Alert kind={kinds.DANGER}>{getErrorMessage(error)}</Alert>
          </div>
        ) : null}

        {!isFetching && !error && isPopulated && items.length > 0 ? (
          <div className={styles.searchResults}>
            {items.map((item) => (
              <AddNewBookSearchResult key={item.id} {...item} />
            ))}
          </div>
        ) : null}

        {!isFetching && !error && isPopulated && items.length === 0 && term ? (
          <div className={styles.message}>
            <div className={styles.noResults}>
              {translate('CouldNotFindResults', { term })}
            </div>
          </div>
        ) : null}

        {!term ? (
          <div className={styles.message}>
            <div className={styles.helpText}>
              {translate('AddNewBookMessage')}
            </div>
            <div>{translate('AddNewBookIsbnMessage')}</div>
          </div>
        ) : null}
      </PageContentBody>
    </PageContent>
  );
}

export default AddNewBook;
