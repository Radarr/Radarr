function countryCodeToFlag(countryCode: string): string {
  const codePoints = countryCode
    .toUpperCase()
    .split('')
    .map((char) => 0x1f1e6 + char.charCodeAt(0) - 65);

  return String.fromCodePoint(...codePoints);
}

const languageFlagMap: Record<string, string> = {
  English: 'GB',
  French: 'FR',
  Spanish: 'ES',
  German: 'DE',
  Italian: 'IT',
  Danish: 'DK',
  Dutch: 'NL',
  Japanese: 'JP',
  Icelandic: 'IS',
  Chinese: 'CN',
  Russian: 'RU',
  Polish: 'PL',
  Vietnamese: 'VN',
  Swedish: 'SE',
  Norwegian: 'NO',
  Finnish: 'FI',
  Turkish: 'TR',
  Portuguese: 'PT',
  Flemish: 'BE',
  Greek: 'GR',
  Korean: 'KR',
  Hungarian: 'HU',
  Hebrew: 'IL',
  Lithuanian: 'LT',
  Czech: 'CZ',
  Hindi: 'IN',
  Romanian: 'RO',
  Thai: 'TH',
  Bulgarian: 'BG',
  'Portuguese (Brazil)': 'BR',
  Arabic: 'SA',
  Ukrainian: 'UA',
  Persian: 'IR',
  Bengali: 'BD',
  Slovak: 'SK',
  Latvian: 'LV',
  'Spanish (Latino)': 'MX',
  Catalan: 'ES',
  Croatian: 'HR',
  Serbian: 'RS',
  Bosnian: 'BA',
  Estonian: 'EE',
  Tamil: 'IN',
  Indonesian: 'ID',
  Telugu: 'IN',
  Macedonian: 'MK',
  Slovenian: 'SI',
  Malayalam: 'IN',
  Kannada: 'IN',
  Albanian: 'AL',
  Afrikaans: 'ZA',
  Marathi: 'IN',
  Tagalog: 'PH',
  Urdu: 'PK',
  Romansh: 'CH',
  Mongolian: 'MN',
  Georgian: 'GE',
};

export default function languageToFlag(languageName?: string): string | null {
  if (!languageName) {
    return null;
  }

  const countryCode = languageFlagMap[languageName];

  if (!countryCode) {
    return null;
  }

  return countryCodeToFlag(countryCode);
}
