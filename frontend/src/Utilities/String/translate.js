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

export default function translate(key, args) {
  const translation = translations[key] || key;

  if (args) {
    return translation.replace(/\{(\d+)\}/g, (match, index) => {
      return args[index];
    });
  }

  return translation;
}
