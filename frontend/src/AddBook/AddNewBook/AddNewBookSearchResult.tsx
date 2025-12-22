import React from 'react';
import Book from 'Book/Book';
import styles from './AddNewBook.css';

function AddNewBookSearchResult(props: Book) {
  const { title, isbn13, publisher } = props;

  return (
    <div className={styles.searchResult}>
      <div className={styles.title}>{title}</div>
      <div className={styles.subtitle}>
        {publisher && <span>{publisher}</span>}
        {isbn13 && <span> - ISBN: {isbn13}</span>}
      </div>
    </div>
  );
}

export default AddNewBookSearchResult;
