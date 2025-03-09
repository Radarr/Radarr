function equalsIgnoringCase(a: string, b: string): boolean {
  return a.localeCompare(b, undefined, { sensitivity: 'accent' }) === 0;
}

export default equalsIgnoringCase;
