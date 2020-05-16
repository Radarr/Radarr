import getNewAuthor from 'Utilities/Author/getNewAuthor';

function getNewBook(book, payload) {
  const {
    searchForNewBook = false
  } = payload;

  getNewAuthor(book.author, payload);

  book.addOptions = {
    searchForNewBook
  };
  book.monitored = true;

  return book;
}

export default getNewBook;
