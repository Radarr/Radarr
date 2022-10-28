import createAjaxRequest from 'Utilities/createAjaxRequest';

function getTranslations() {
  let localization = null;
  const ajaxOptions = {
    async: false,
    dataType: 'json',
    url: '/localization',
    success: function(data) {
      localization = data.Strings;
    }
  };

  createAjaxRequest(ajaxOptions);

  return localization;
}

const translations = getTranslations();

export default function translate(key, args = '') {
  if (args) {
    const translatedKey = translate(key);
    return translatedKey.replace(/\{(\d+)\}/g, (match, index) => {
      return args[index];
    });
  }

  return translations[key] || key;
}
