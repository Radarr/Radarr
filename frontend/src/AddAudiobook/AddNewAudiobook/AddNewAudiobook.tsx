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
import {
  clearAddAudiobook,
  lookupAudiobook,
} from 'Store/Actions/addAudiobookActions';
import createAddAudiobookSelector from 'Store/Selectors/createAddAudiobookSelector';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import translate from 'Utilities/String/translate';
import AddNewAudiobookSearchResult from './AddNewAudiobookSearchResult';
import styles from './AddNewAudiobook.css';

function AddNewAudiobook() {
  const dispatch = useDispatch();
  const { isFetching, isPopulated, error, items } = useSelector(
    createAddAudiobookSelector()
  );

  const [term, setTerm] = useState('');
  const lookupTimeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  useEffect(() => {
    return () => {
      if (lookupTimeoutRef.current) {
        clearTimeout(lookupTimeoutRef.current);
      }
      dispatch(clearAddAudiobook());
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
          dispatch(lookupAudiobook({ term: value }));
        }, 300);
      } else {
        dispatch(clearAddAudiobook());
      }
    },
    [dispatch]
  );

  const onClearPress = useCallback(() => {
    setTerm('');
    dispatch(clearAddAudiobook());
  }, [dispatch]);

  return (
    <PageContent title={translate('AddNewAudiobook')}>
      <PageContentBody>
        <div className={styles.searchContainer}>
          <div className={styles.searchIconContainer}>
            <Icon name={icons.SEARCH} size={20} />
          </div>

          <TextInput
            className={styles.searchInput}
            name="audiobookLookup"
            value={term}
            placeholder="e.g. The Martian, narrator:R.C. Bray"
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
              <AddNewAudiobookSearchResult key={item.id} {...item} />
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
              {translate('AddNewAudiobookMessage')}
            </div>
            <div>{translate('AddNewAudiobookAsinMessage')}</div>
          </div>
        ) : null}
      </PageContentBody>
    </PageContent>
  );
}

export default AddNewAudiobook;
