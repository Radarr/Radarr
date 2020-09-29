import qs from 'qs';

// See: https://developer.mozilla.org/en-US/docs/Web/API/HTMLHyperlinkElementUtils
const anchor = document.createElement('a');

export default function parseUrl(url) {
  anchor.href = url;

  // The `origin`, `password`, and `username` properties are unavailable in
  // Opera Presto. We synthesize `origin` if it's not present. While `password`
  // and `username` are ignored intentionally.
  const properties = {
    hash: anchor.hash,
    host: anchor.host,
    hostname: anchor.hostname,
    href: anchor.href,
    origin: anchor.origin,
    pathname: anchor.pathname,
    port: anchor.port,
    protocol: anchor.protocol,
    search: anchor.search
  };

  properties.isAbsolute = (/^[\w:]*\/\//).test(url);

  if (properties.search) {
    // Remove leading ? from querystring before parsing.
    properties.params = qs.parse(properties.search.substring(1));
  } else {
    properties.params = {};
  }

  return properties;
}
