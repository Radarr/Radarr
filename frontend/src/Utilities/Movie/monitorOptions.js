import translate from 'Utilities/String/translate';

const monitorOptions = [
  {
    key: 'movieOnly',
    get value() {
      return translate('MovieOnly');
    }
  },
  {
    key: 'movieAndCollection',
    get value() {
      return translate('MovieAndCollection');
    }
  },
  {
    key: 'none',
    get value() {
      return translate('None');
    }
  }
];

export default monitorOptions;
