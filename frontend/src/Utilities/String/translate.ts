import moment from 'moment';
import createAjaxRequest from 'Utilities/createAjaxRequest';

function getTranslations() {
  return createAjaxRequest({
    global: false,
    dataType: 'json',
    url: '/localization',
  }).request;
}

interface LanguageResponse {
  identifier: string;
}

function getLanguage() {
  return createAjaxRequest({
    global: false,
    dataType: 'json',
    url: '/localization/language',
  }).request;
}

async function setMomentLocale() {
  const { identifier } = (await getLanguage()) as LanguageResponse;
  const locale = identifier.toLowerCase();

  if (locale === 'en') {
    moment.locale(locale);
    return;
  }

  try {
    await import(`moment/locale/${locale}`);
    moment.locale(locale);
  } catch {
    const language = locale.split('-')[0];

    try {
      await import(`moment/locale/${language}`);
      moment.locale(language);
    } catch {
      moment.locale('en');
    }
  }
}

let translations: Record<string, string> = {};

export async function fetchTranslations(): Promise<boolean> {
  return new Promise(async (resolve) => {
    try {
      const [data] = await Promise.all([
        getTranslations(),
        setMomentLocale().catch(() => moment.locale('en')),
      ]);
      translations = data.Strings;

      resolve(true);
    } catch {
      resolve(false);
    }
  });
}

export default function translate(
  key: string,
  tokens: Record<string, string | number | boolean> = {}
) {
  const { isProduction = true } = window.Radarr;

  if (!isProduction && !(key in translations)) {
    console.warn(`Missing translation for key: ${key}`);
  }

  const translation = translations[key] || key;

  tokens.appName = 'Radarr';

  // Fallback to the old behaviour for translations not yet updated to use named tokens
  Object.values(tokens).forEach((value, index) => {
    tokens[index] = value;
  });

  return translation.replace(/\{([a-z0-9]+?)\}/gi, (match, tokenMatch) =>
    String(tokens[tokenMatch] ?? match)
  );
}
