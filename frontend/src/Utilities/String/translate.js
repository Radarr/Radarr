import createAjaxRequest from 'Utilities/createAjaxRequest';

function getTranslations() {
  return createAjaxRequest({
    global: false,
    dataType: 'json',
    url: '/localization'
  }).request;
}

let translations = {};

export function fetchTranslations() {
  return new Promise(async(resolve) => {
    try {
      const data = await getTranslations();
      translations = data.Strings;

      resolve(true);
    } catch (error) {
      resolve(false);
    }
  });
}

export default function translate(key, args) {
  const translation = translations[key] || key;

  if (args) {
    return translation.replace(/\{(\d+)\}/g, (match, index) => {
      return args[index];
    });
  }

  return translation;
}
